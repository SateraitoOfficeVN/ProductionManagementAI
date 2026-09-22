<!-- Based on ai/templates/DD/api-design.md (revision at commit e7e0d36). -->

# Production Order List API — API Specification Design (API仕様設計)

DD-002-API — supports DD-002; handlers designed in DD-002-FN; requirements REQ-020–REQ-027.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | DD-002-API |
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

## Overview and operation catalog

| Field | Value |
| --- | --- |
| API / module name | Production order list API |
| Overview | One read-only JSON endpoint behind SCR-002, plus the existing product endpoint reused for the product filter. Adds no write path (DEC-003) |

| No | Endpoint / method | Handler | Purpose | Notes |
| --- | --- | --- | --- | --- |
| 1 | `GET /api/production-orders` | `ProductionOrdersController.List` → `ProductionOrderService.ListAsync` | One page of orders matching the filters, with the total | New in WI-003 |
| 2 | `GET /api/products` | `ProductsController.List` → `ProductionOrderService.ListProductsAsync` | Product filter options | Existing (DD-001-API §1), reused unchanged — DD-001-API anticipated this |

Common to both endpoints, unchanged from DD-001-API:

- **Auth:** cookie session (ADR-0002) plus policy `ProductionOrderEditor` = role `Admin` or `Operator`. No session → 401 with an empty body; session without the role → 403 with an empty body. The policy name is kept although this endpoint only reads: introducing a second, read-only policy would imply a permission split that WI-001 DEC-015 has not decided. Recorded as a limitation in DD-002, not invented here.
- **Content type:** responses are `application/json`; errors are `application/problem+json` (RFC 9457). The list endpoint has no request body, so the 415 rule in DD-001-API does not apply to it.
- **JSON:** camelCase; dates `YYYY-MM-DD`; timestamps ISO 8601 UTC with `Z`; enums as strings.
- **CSRF:** not applicable — `GET`, no state change, no request body (WI-002 DEC-020 posture unchanged).
- **Error body:** the RFC 9457 shape defined in DD-001-API, with `code` and `errors` carrying message IDs from the single catalog. Screen B's new IDs are MSG-E015–MSG-E020 and MSG-I003–MSG-I004; MSG-E002 and MSG-E013 are reused. Stack traces and exception text are never returned.

```json
{
  "type": "urn:pmai:problem:validation",
  "title": "One or more query parameters are invalid.",
  "status": 400,
  "code": "VALIDATION",
  "errors": { "dueFrom": ["MSG-E017"], "pageSize": ["MSG-E019"] },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

### System-wide API registry entries

| ID | Endpoint | Overview | Notes |
| --- | --- | --- | --- |
| API-PO-04 | `GET /api/production-orders` | List production orders (filter, sort, page) | New; continues DD-001-API's registry |

`API-PRD-01` (`GET /api/products`) and `API-PO-01`–`API-PO-03` stay as registered in DD-001-API.

### Shared response object: `ProductionOrderListItem`

One element of the `items` array. It is **not** `ProductionOrderResponse` (DD-001-API): the list deliberately returns a narrower row — no `notes`, no `version`, no `allowedNextStatuses`, no `isProductQuantityEditable` — because the list neither shows nor writes those, and DB-003's projection does not read them. The two shapes are intentionally separate so a later change to the edit contract cannot silently widen the list.

| No | Name | Variable name | Type | Length | Required | Repeats (array) | Value mapping | Example | Description | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | ID | `id` | string (uuid) | 36 | yes | no | — | `3f2c…` | `production_orders.id` | Row link target (REQ-025) |
| 2 | Order number | `orderNumber` | string | 13 | yes | no | — | `PO-2026-00042` | Generated column | |
| 3 | Product | `product` | object | — | yes | no | Join to `products` | — | `{ "id", "sku", "name" }` | Rendered as "{sku} — {name}" (BD-002 M-06) |
| 3.1 | Product ID | `product.id` | string (uuid) | 36 | yes | no | — | | | |
| 3.2 | Product SKU | `product.sku` | string | 50 | yes | no | — | `P-1004` | | |
| 3.3 | Product name | `product.name` | string | 200 | yes | no | — | `Drive shaft` | | |
| 4 | Quantity | `quantity` | integer | — | yes | no | — | `250` | | |
| 5 | Due date | `dueDate` | string (date) | 10 | yes | no | — | `2026-10-01` | Plant-local date, no timezone conversion | |
| 6 | Status | `status` | string | — | yes | no | Enum name | `InProgress` | | Label mapped client-side (BD-002 M-05) |
| 7 | Overdue | `isOverdue` | boolean | — | yes | no | `dueDate < plantToday && status ∈ {Draft, InProgress}` | `true` | Computed server-side, never stored (DB-003) | Drives M-08; the server owns "today" (WI-002 DEC-011) |
| 8 | Updated at | `updatedAt` | string (date-time) | — | yes | no | UTC | `2026-09-20T01:12:03Z` | | |

### Shared response object: `PagedResult<T>`

| No | Name | Variable name | Type | Required | Repeats (array) | Value mapping | Example | Description |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Items | `items` | `ProductionOrderListItem[]` | yes | yes | — | — | The requested page; empty when the page is past the last one |
| 2 | Total | `total` | integer | yes | no | Exact `count(*)` over the same filters | `87` | Drives the summary and page count (BD-002 M-09) |
| 3 | Page | `page` | integer | yes | no | Echo of the effective value | `1` | |
| 4 | Page size | `pageSize` | integer | yes | no | Echo of the effective value | `20` | |
| 5 | Sort | `sort` | string | yes | no | Echo of the effective value | `dueDate` | |
| 6 | Direction | `dir` | string | yes | no | Echo of the effective value | `asc` | |

The four echoed controls are returned so the client renders from the response it actually got, not from what it believes it asked for — which keeps the URL, the table and the summary consistent even if a request is retried or arrives out of order.

## Endpoint / method design

### 1. GET /api/production-orders

| Field | Value |
| --- | --- |
| Description | Return one page of production orders matching the filters, in the requested order, with the total number of matches |
| Return type | `PagedResult<ProductionOrderListItem>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**: query-string parameters only — see Request fields. No route or body parameters.

Processing overview: bind the query string **as strings only** — every value is parsed by the validator, so each failure carries its designed message ID instead of a generic model-binding error — then validate it (allow-lists for `status`, `sort`, `dir`, `pageSize`; parse dates; bound `page`; trim, bound and escape the order-number fragment) and hand a validated query object to `ProductionOrderService.ListAsync` (DD-002-FN §1), which runs the count and the page query described in DB-003 and marks each row's `isOverdue` against the plant clock. Absent parameters take their defaults; parameters present but invalid are rejected, never defaulted (BD-002 V-13), so a crafted request cannot widen the query.

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Authorize | policy `ProductionOrderEditor` |
| 2 | Bind and validate the query parameters; on failure return 400 with per-parameter message IDs | `ProductionOrderListQuery.TryCreate` (DD-002 module 5) |
| 3 | Verify the product filter exists, when one was sent; on failure 400 `productId: MSG-E002` | `IProductionOrderRepository.ProductExistsAsync` (existing) |
| 4 | Count matches, then fetch the page | `ProductionOrderService.ListAsync` → DD-002-FN §1 |
| 5 | Map rows to `ProductionOrderListItem`, computing `isOverdue` from `IPlantClock.Today` | DD-002-FN §1 |
| 6 | 200 with `PagedResult` | — |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `PagedResult<ProductionOrderListItem>` | body | See the shared objects above |

**Request fields**

Every parameter is optional. `status` is the only repeatable one (`?status=Draft&status=InProgress`) — the multi-select filter, DEC-005.

| No | Name | Variable name | Type | Length | Required | Source | Example | Description | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Status | `status` | string (repeatable) | — | no | query | `Draft` | Restrict to the given statuses; absent = no restriction | Allow-list `Draft`\|`InProgress`\|`Completed`\|`Cancelled`, matched **by name only** — a numeric enum value such as `3` is rejected; duplicates collapse; unknown value → 400 `MSG-E018` (BD-002 V-11) |
| 2 | Product | `productId` | string (uuid) | 36 | no | query | `0197e4a0-…` | Restrict to one product | Unparseable → 400 `MSG-E002`; parseable but unknown → 400 `MSG-E002` (BD-002 V-12) |
| 3 | Due from | `dueFrom` | string (date) | 10 | no | query | `2026-09-01` | Inclusive lower bound on `dueDate` | Malformed → 400 `MSG-E016` |
| 4 | Due to | `dueTo` | string (date) | 10 | no | query | `2026-10-31` | Inclusive upper bound on `dueDate` | Malformed → 400 `MSG-E016`; `dueFrom > dueTo` → 400 `MSG-E017` on `dueFrom` (BD-002 V-10) |
| 5 | Order number | `orderNumber` | string | ≤ 20 | no | query | `00042` | Case-insensitive fragment of the order number | Trimmed first; longer than 20 after trimming → 400 `MSG-E015`; empty after trimming = absent (BD-002 V-09) |
| 6 | Sort | `sort` | string | — | no | query | `dueDate` | Sort key | Allow-list `orderNumber`\|`product`\|`quantity`\|`dueDate`\|`status`\|`updatedAt`; default `dueDate`; unknown → 400 `MSG-E019` |
| 7 | Direction | `dir` | string | — | no | query | `asc` | Sort direction | Allow-list `asc`\|`desc`; default `asc`; unknown → 400 `MSG-E019` |
| 8 | Page | `page` | integer | — | no | query | `2` | 1-based page number | Default 1; `< 1`, non-integer or `> 100000` → 400 `MSG-E019`. A page past the last one is **valid** and returns an empty `items` (REQ-024) |
| 9 | Page size | `pageSize` | integer | — | no | query | `20` | Rows per page | Allow-list 10\|20\|50\|100; default 20; anything else → 400 `MSG-E019` (DEC-002) |

Normalization of the order-number fragment (DEC-010): trim → reject if longer than 20 → upper-case → escape `\`, `%` and `_` with a `\` prefix → wrap as `%fragment%` → match with `LIKE … ESCAPE '\'` against `order_number`. Upper-casing is enough for case-insensitivity because `order_number` is generated and always upper-case, and it keeps the trigram index usable. The pattern is a bound parameter; nothing is concatenated into SQL (`ai/rules/database.md`).

**Response fields**: `PagedResult<ProductionOrderListItem>` — see the shared objects above.

Example response:

```json
{
  "items": [
    {
      "id": "0197e4a0-0000-7000-8000-0000000002a1",
      "orderNumber": "PO-2026-00042",
      "product": { "id": "0197e4a0-0000-7000-8000-000000001004", "sku": "P-1004", "name": "Drive shaft" },
      "quantity": 250,
      "dueDate": "2026-09-18",
      "status": "InProgress",
      "isOverdue": true,
      "updatedAt": "2026-09-20T01:12:03Z"
    }
  ],
  "total": 87,
  "page": 1,
  "pageSize": 20,
  "sort": "dueDate",
  "dir": "asc"
}
```

**Error codes**

| Code | Meaning | HTTP status |
| --- | --- | --- |
| `MSG-E015` | Order-number fragment longer than 20 characters | 400 |
| `MSG-E016` | Malformed `dueFrom` or `dueTo` | 400 |
| `MSG-E017` | `dueFrom` after `dueTo` | 400 |
| `MSG-E018` | Unknown status value | 400 |
| `MSG-E002` | `productId` unparseable or not an existing product | 400 |
| `MSG-E019` | Unsupported `sort`, `dir`, `page` or `pageSize` | 400 |
| — | Not signed in | 401 |
| — | Missing role (client shows MSG-E020) | 403 |
| `MSG-E013` | Unexpected failure | 500 |

Every 400 returns `errors` keyed by the offending parameter name, so the filter panel can attach each message to its own field. Multiple invalid parameters are reported together in one response rather than one at a time.

### 2. GET /api/products

Unchanged — designed in DD-001-API §1 and already implemented. SCR-002 calls it for the product filter options (BD-002 FN-012) and uses the same "{sku} — {name}" label and SKU ordering as SCR-001 (WI-002 DEC-022). No paging or search is added: the list is 30 rows.

## Unresolved decisions

None. The decisions this contract rests on are recorded in `work-items/WI-003/decisions.md` (DEC-002 paging and sort, DEC-005 multi-select status, DEC-010 fragment matching) and `work-items/WI-002/decisions.md` (DEC-011/DEC-017 plant clock, DEC-020 CSRF, DEC-022 product labels, DEC-023 the RFC 9457 error contract this reuses).

One limitation is recorded rather than decided here: the endpoint is gated by the existing `ProductionOrderEditor` policy, so a hypothetical read-only role cannot be expressed yet. That waits on WI-001 DEC-015 (the full role/permission matrix), which stays open.
