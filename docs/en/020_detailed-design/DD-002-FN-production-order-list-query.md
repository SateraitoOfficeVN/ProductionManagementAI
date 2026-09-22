<!-- Based on ai/templates/DD/function-design.md (revision at commit e7e0d36). -->

# Production Order List Query — Function Design (機能設計)

DD-002-FN — used by DD-002 and DD-002-API, requirements REQ-020–REQ-027.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | DD-002-FN |
| System name | ProductionManagementAI |
| Subsystem name | Production orders |
| Work item | WI-003 |
| Created by | Claude (for ThanhTN) |
| Created date | 2026-09-22 |
| Last updated by | Claude (for ThanhTN) |
| Last updated date | 2026-09-22 |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | 2026-09-22 | Claude (for ThanhTN) | Initial creation |

## Overview and method index

| Field | Value |
| --- | --- |
| Module name | `ProductionOrderService.ListAsync` and the repository methods behind it (`ProductionManagementAI.Application.ProductionOrders`, implemented in `ProductionManagementAI.Infrastructure.ProductionOrders`) |
| Overview | The read side of production orders: turn a validated list query into a counted page of rows. Extends the existing `ProductionOrderService` and `IProductionOrderRepository` from DD-001-FN rather than introducing a second service, so the plant clock, telemetry and result conventions stay shared |

| No | Method name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `ProductionOrderService.ListAsync` | Validate the product filter, count matches, fetch the page, mark overdue rows | API-PO-04 |
| 2 | `IProductionOrderRepository.CountOrdersAsync` | `count(*)` over the filtered set | New |
| 3 | `IProductionOrderRepository.ListOrdersAsync` | One ordered, projected page of rows | New |
| 4 | `ProductionOrderQueryExtensions.ApplyFilters` | Compose only the filters that are set onto an `IQueryable` | New, internal |
| 5 | `ProductionOrderQueryExtensions.ApplySort` | Map the allow-listed sort key to an ordering, with the order-number tie-breaker | New, internal |
| 6 | `ProductionOrderListMapper.ToListItem` | Row → `ProductionOrderListItem`, computing `isOverdue` | New, internal |
| 7 | `IPlantClock.Today` | Plant-local date (`Asia/Tokyo`) used for `isOverdue` | Existing (DD-001-FN §6), reused unchanged |
| 8 | `IProductionOrderRepository.ProductExistsAsync` | Product-filter existence check | Existing (DD-001-FN), reused unchanged |

`ListAsync` returns the same `Result<T>` type as DD-001-FN's methods, using only the `Ok` and `Invalid` variants — a list query cannot be `NotFound`, `Conflict` or a `RuleViolation`. Unexpected exceptions propagate to the global Problem Details handler (500, MSG-E013) rather than becoming a `Result`.

### Shared utility references

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `IProductionOrderRepository` | EF Core data access; gains the two read methods above | Existing interface (DD-001-FN) |
| 2 | `IPlantClock` | Plant-local date and year | Existing; `FakeTimeProvider` in tests makes `isOverdue` deterministic |
| 3 | `ProductionOrderTelemetry` | `ActivitySource` + `Meter` named `ProductionManagementAI.ProductionOrders` | Existing; gains one counter and one histogram (see Observability) |
| 4 | `ILogger<ProductionOrderService>` | Structured logging, `LoggerMessage` source-generated | Never logs the order-number fragment as free text — it is a user-supplied value and goes in as a structured field only |
| 5 | `ProductionOrderListQuery` | The validated query object the API layer produces | Defined in DD-002 module 5; this module never sees raw query strings |

## Request data

Parameters this module sends to the database. All are bound parameters; nothing is concatenated into SQL (`ai/rules/database.md`).

| No | Name | Variable name | Type | Length | Used by (which method(s)) | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Status set | `statuses` | `ProductionOrderStatus[]` | ≤ 4 | ListOrdersAsync, CountOrdersAsync | `status = ANY(@statuses)`; omitted entirely when empty |
| 2 | Product ID | `productId` | `Guid?` | — | ListOrdersAsync, CountOrdersAsync, ProductExistsAsync | Omitted when null |
| 3 | Due from | `dueFrom` | `DateOnly?` | — | ListOrdersAsync, CountOrdersAsync | `due_date >= @dueFrom` |
| 4 | Due to | `dueTo` | `DateOnly?` | — | ListOrdersAsync, CountOrdersAsync | `due_date <= @dueTo` |
| 5 | Order-number pattern | `pattern` | `string?` | ≤ 24 | ListOrdersAsync, CountOrdersAsync | Already trimmed, upper-cased, escaped and wrapped as `%…%` by the API layer (DD-002-API); used with `EF.Functions.Like(o.OrderNumber, pattern, "\\")` |
| 6 | Skip / take | `skip`, `take` | `int` | — | ListOrdersAsync | `(page − 1) × pageSize`, `pageSize` |

## Response / value mapping

`ProductionOrderListItem` (field catalog in DD-002-API) is built by `ProductionOrderListMapper.ToListItem` from the projected row:

| No | Name | Variable name | Type | Length | Required | Value mapping | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | ID | `Id` | `Guid` | — | yes | as stored | |
| 2 | Order number | `OrderNumber` | `string` | 13 | yes | as stored (generated column) | |
| 3 | Product | `Product` | `ProductSummary` | — | yes | `{ p.Id, p.Sku, p.Name }` from the join | Narrower than `ProductResponse`, but the same three fields |
| 4 | Quantity | `Quantity` | `int` | — | yes | as stored | |
| 5 | Due date | `DueDate` | `DateOnly` | — | yes | as stored | Serialized `YYYY-MM-DD`, no timezone conversion |
| 6 | Status | `Status` | `string` | — | yes | `row.Status.ToString()` | |
| 7 | Overdue | `IsOverdue` | `bool` | — | yes | `row.DueDate < plantToday && (row.Status == Draft \|\| row.Status == InProgress)` | Computed in memory over the page's rows, after the query — never stored, never a SQL predicate (DB-003) |
| 8 | Updated at | `UpdatedAt` | `DateTimeOffset` | — | yes | `updated_at_utc` | UTC |

`PagedResult<T>` wraps the items with `Total`, `Page`, `PageSize`, `Sort` and `Dir` echoed from the validated query.

## Method design

### 1. `ProductionOrderService.ListAsync`

| Field | Value |
| --- | --- |
| Description | Return one counted page of production orders for a validated list query |
| Return type | `Task<Result<PagedResult<ProductionOrderListItem>>>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `ProductionOrderListQuery` | `query` | Already validated and normalized by the API layer (DD-002 module 5) |
| 2 | `CancellationToken` | `cancellationToken` | |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `Result<PagedResult<ProductionOrderListItem>>` | result | `Ok` with the page, or `Invalid` with `productId → MSG-E002` |

Processing overview: check the product filter exists (the one validation that needs the database), count the matches, fetch the page only when the count is non-zero, then map the rows and mark the overdue ones against the plant clock. The count runs first so a page past the last one costs one cheap query instead of two, and so `total` is always present for the summary.

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Start the span `production_orders.list` with the filter shape as attributes (which filters are set, sort key, direction, page, page size — never the fragment text) | `ProductionOrderTelemetry.Source` |
| 2 | If `query.ProductId` is set and `ProductExistsAsync` is false → return `Invalid(productId: MSG-E002)`; record outcome `validation_failed` | `IProductionOrderRepository.ProductExistsAsync` |
| 3 | `total = await CountOrdersAsync(query)` | §2 |
| 4 | If `total == 0` → skip the page query and use an empty item list | — |
| 5 | Else `rows = await ListOrdersAsync(query)` | §3 |
| 6 | `today = plantClock.Today`; map each row with `ToListItem(row, today)` | §6, `IPlantClock` |
| 7 | Record the result-count histogram and outcome `success`; return `Ok(new PagedResult(...))` with the echoed controls | `ProductionOrderTelemetry` |

A page beyond the last one is not an error (REQ-024): step 5 simply returns no rows, and `total` still reports the real number of matches, so the client can show "Page 9 of 5" honestly and page back.

### 2. `IProductionOrderRepository.CountOrdersAsync`

| Field | Value |
| --- | --- |
| Description | Number of production orders matching the query's filters |
| Return type | `Task<int>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `ProductionOrderListQuery` | `query` | Only its filter fields are used; sort and paging are ignored |
| 2 | `CancellationToken` | `cancellationToken` | |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `int` | total | Exact count, not an estimate (REQ-024) |

Processing overview: `db.ProductionOrders.AsNoTracking()`, filters applied by §4, then `CountAsync`. No join to `products`: every filter predicate is on `production_orders`, so the count query touches one table (DB-003).

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Start from `db.ProductionOrders.AsNoTracking()` | — |
| 2 | Apply the filters that are set | §4 |
| 3 | `CountAsync` | EF Core |

### 3. `IProductionOrderRepository.ListOrdersAsync`

| Field | Value |
| --- | --- |
| Description | One page of production orders, ordered and projected |
| Return type | `Task<IReadOnlyList<ProductionOrderListRow>>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `ProductionOrderListQuery` | `query` | Filters, sort, direction, page, page size |
| 2 | `CancellationToken` | `cancellationToken` | |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `IReadOnlyList<ProductionOrderListRow>` | rows | Projected rows in the requested order; at most `pageSize` |

Processing overview: the same filtered `IQueryable`, joined to `products` for the displayed product and the product sort key, ordered by §5, then `Skip`/`Take` and an explicit projection into `ProductionOrderListRow` — `notes`, `xmin`, `order_year`, `order_seq` and `created_at_utc` are never selected (DB-003 read projection).

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Start from `db.ProductionOrders.AsNoTracking()` | — |
| 2 | Apply the filters that are set | §4 |
| 3 | Join `db.Products` on `product_id` into `ProductionOrderJoin`, a named type with settable members (no navigation property exists, and EF sees through member-init projections) | — |
| 4 | Apply the ordering and the `order_number` tie-breaker **on the joined entities** | §5 |
| 5 | `Skip((page − 1) × pageSize).Take(pageSize)` | — |
| 6 | Project into `ProductionOrderListRow` and `ToListAsync` | — |

The order of steps 4 and 6 matters: EF Core cannot translate an `ORDER BY` that reads a member back out of a
constructor projection, so the sort is applied before the final projection, not after it. Proven by the integration
tests, which fail with a translation error if the two are swapped.

### 4. `ProductionOrderQueryExtensions.ApplyFilters`

| Field | Value |
| --- | --- |
| Description | Compose onto an `IQueryable<ProductionOrder>` only the filters the query actually carries |
| Return type | `IQueryable<ProductionOrder>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `IQueryable<ProductionOrder>` | `source` | |
| 2 | `ProductionOrderListQuery` | `query` | |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `IQueryable<ProductionOrder>` | filtered | The same source with zero to five predicates composed onto it |

Processing overview: each filter is added inside an `if`, so an unset filter contributes no SQL at all. This matters for the query plan — a `(@p IS NULL OR col = @p)` predicate would defeat the indexes DB-003 relies on — and it is the reason this is one shared method used by both the count and the page query, so the two can never drift apart.

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | `if (query.Statuses.Count > 0)` → `Where(o => query.Statuses.Contains(o.Status))` (translated to `status = ANY(@statuses)`) | — |
| 2 | `if (query.ProductId is Guid id)` → `Where(o => o.ProductId == id)` | — |
| 3 | `if (query.DueFrom is DateOnly from)` → `Where(o => o.DueDate >= from)` | — |
| 4 | `if (query.DueTo is DateOnly to)` → `Where(o => o.DueDate <= to)` | — |
| 5 | `if (query.OrderNumberPattern is string pattern)` → `Where(o => EF.Functions.Like(o.OrderNumber, pattern, "\\"))` | `pg_trgm` index (DEC-010) |

### 5. `ProductionOrderQueryExtensions.ApplySort`

| Field | Value |
| --- | --- |
| Description | Translate the allow-listed sort key and direction into an ordering, always ending with the order-number tie-breaker |
| Return type | `IOrderedQueryable<…>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `IQueryable<ProductionOrderJoin>` | `source` | The filtered query joined to `products`, before the list projection |
| 2 | `ProductionOrderSort` | `sort` | Enum, not a string — the API layer already rejected anything outside the allow-list |
| 3 | `SortDirection` | `dir` | `Asc` or `Desc` |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `IOrderedQueryable<…>` | ordered | Deterministic total order |

Processing overview: a `switch` over the enum picks the key expression; the direction picks `OrderBy`/`OrderByDescending`; `ThenBy(o => o.OrderNumber)` is always appended unless the key already is the order number. Because the enum is the only input, no client string ever reaches the ordering — the allow-list is enforced by the type system, not by string comparison at this layer.

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | `DueDate` → `j.Order.DueDate` (the default) | — |
| 2 | `OrderNumber` → `j.Order.OrderNumber`; no tie-breaker needed (unique) | — |
| 3 | `Product` → `j.Product.Sku`, then `j.Product.Name` | — |
| 4 | `Quantity` → `j.Order.Quantity` | — |
| 5 | `Status` → the workflow rank expression `Draft=1, InProgress=2, Completed=3, Cancelled=4`, not the stored string (DB-003 sort-key mapping) | — |
| 6 | `UpdatedAt` → `j.Order.UpdatedAtUtc` | — |
| 7 | Append `ThenBy(j => j.Order.OrderNumber)` ascending (steps 1, 3–6) | — |

The direction applies to the chosen key only; the tie-breaker stays ascending, so a descending page is still a stable total order.

### 6. `ProductionOrderListMapper.ToListItem`

| Field | Value |
| --- | --- |
| Description | Turn a projected row into the API list item, deciding the overdue flag |
| Return type | `ProductionOrderListItem` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `ProductionOrderListRow` | `row` | |
| 2 | `DateOnly` | `plantToday` | Read once per request, not per row, so every row on a page judges "today" identically |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `ProductionOrderListItem` | item | See Response / value mapping above |

Processing overview: a pure function — same row plus same date always gives the same item, which is what makes the overdue rule unit-testable without a database (BD-002 M-08, FN-013).

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Copy `Id`, `OrderNumber`, `Quantity`, `DueDate`, `Status`, `UpdatedAt` | — |
| 2 | Build `Product` from the row's three product columns | — |
| 3 | `IsOverdue = row.DueDate < plantToday && row.Status is Draft or InProgress` | — |

## Observability

Extends the existing `ProductionManagementAI.ProductionOrders` `ActivitySource`/`Meter` (DD-001-FN) rather than adding a second one, per `ai/rules/backend.md`.

| Signal | Name | Type | Attributes / buckets |
| --- | --- | --- | --- |
| Span | `production_orders.list` | Activity | `filter.status_count`, `filter.has_product`, `filter.has_due_range`, `filter.has_order_number` (booleans/counts only — never the fragment text), `sort`, `dir`, `page`, `page_size`, `result.total` |
| Metric | `pmai.production_orders.listed` | Counter | `outcome` = `success` \| `validation_failed` \| `error`, reusing DD-001-FN's outcome names |
| Metric | `pmai.production_orders.list_result_size` | Histogram | Number of rows returned on the page — shows whether users actually page, and whether page sizes above 20 are used |

Logging: one `Information` record per rejected query with the offending parameter names and their message IDs (structured fields, never an interpolated string), and `Error` with the exception for an unexpected failure. The order-number fragment is a user-supplied value and is logged as a structured field only, never concatenated into a message.

## Unresolved decisions

None. The behavior here follows DEC-002 (paging and sort), DEC-005 (multi-select status), DEC-010 (fragment matching) and DB-003's query design; the plant clock and the `Result<T>`/telemetry conventions are inherited unchanged from DD-001-FN.
