# Product master and affected screens — API Specification Design (API仕様設計)

`004_DD-API` is the WI-006-owned API contract for [004_DD](004_DD_製品マスタ.md) and the approved [004_BD](../../010_basic-design/004/004_BD_製品マスタ.md), [004_DB](../../database/004/004_DB_製品マスタ.md), [existing-screen BD addendum](../../010_basic-design/004/004_BD-EXISTING-SCREENS_製品マスタ影響.md), and [DB addendum](../../database/004/004_DB-EXISTING-SCREENS_製品マスタ影響.md). It covers REQ-049–REQ-052, REQ-054, and REQ-056–REQ-060. The approved WI-002–WI-004 API design files are read-only; this document defines their WI-006 contract changes. No endpoint has been implemented under the current design plan.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | 004_DD-API |
| System / subsystem | ProductionManagementAI / Product master, production orders and dashboard |
| Work item | WI-006 |
| Created / updated | Codex, 2026-09-29 |
| Version | 2 — unit lock and decimal/mixed-unit API reconciliation |

| Version | Change |
| --- | --- |
| 1 | Initial Product master draft under plan revision 1 |
| 2 | Add reference lock indicator, affected order/dashboard contracts, exact decimal syntax and conflict mapping |

## Common HTTP and JSON contract

| Concern | WI-006 rule |
| --- | --- |
| Authentication | Cookie session. Product master and existing order/dashboard endpoints require `Admin` or `Operator`; authorization is checked at the API, before an ID existence lookup. Existing 401/403 responses have empty bodies and retain the existing login/forbidden UI behavior. |
| Mutations | `Content-Type: application/json`; other media types return 415. Same-origin cookie and no credentialed CORS. New Product master mutations reject unknown body properties rather than silently ignoring them. Existing order endpoints retain their approved unknown-field behavior. |
| Successful JSON | `application/json`, camelCase; UUID strings, plant-local dates `YYYY-MM-DD`, UTC timestamps ISO 8601 with `Z`, and existing status enum strings. Reads and errors use `Cache-Control: no-store`. |
| Errors | RFC 9457 `application/problem+json` for application errors, with `type`, `title`, `status`, stable `code`, optional `errors` keyed by JSON field, and `traceId`. No stack trace, SQL or raw exception. 401/403 retain their established empty-body exception. |
| Quantity | JSON **number**, parsed and stored as exact decimal, never through binary floating point. Response may have 0–3 fractional digits; a measured input token may have at most three digits after the decimal point. |

The API checks the submitted **raw JSON numeric token** before decimal conversion. It accepts ordinary JSON integer or fixed-point tokens such as `12`, `12.5`, `0.001`; it rejects strings, exponent notation, more than three written fractional digits (including `1.2340`), non-finite values, non-positive values and values above `999999999`. For `個`, `本`, `枚`, `台`, `セット`, the value must also be integral; `kg` and `m` may be fractional. Existing integer JSON clients remain valid. A response serializes the exact numeric value as a JSON number; no scale beyond three is produced. Client code must preserve the text entered until validation/submission instead of making binary floating point its authoritative representation. The server repeats unit-specific validation after loading and locking the product in the order-write transaction. The approved DB addendum owns the `numeric` storage and CHECKs.

## Operation catalog and stable registry IDs

| ID | Method and route | Purpose / owner |
| --- | --- | --- |
| API-PM-01 | `GET /api/product-master` | Search/page catalog; Product master controller → FN-027 |
| API-PM-02 | `GET /api/product-master/{id}` | Load product, version and unit-lock state; FN-027 |
| API-PM-03 | `POST /api/product-master` | Create active product; FN-028 |
| API-PM-04 | `PUT /api/product-master/{id}` | Edit mutable fields; FN-029 |
| API-PM-05 | `POST /api/product-master/{id}/retire` | Retire active product; FN-030 |
| API-PRD-01 | `GET /api/products` | Existing product picker API extended in place; existing registry ID from [001_DD-API](../001/001_DD-API_製造指示登録・編集.md); FN-031 |
| API-PO-01–04 | Existing `POST`, `GET /{id}`, `PUT /{id}`, and list `/api/production-orders` | Quantity/unit and selection-rule changes to existing IDs; see affected contract below |
| API-DSH-01 | `GET /api/dashboard` | Mixed-unit-safe dashboard response; existing ID from [003_DD-API](../003/003_DD-API_ダッシュボード.md) |

No second registry ID is assigned to `GET /api/products`. The product master endpoints are new; the order/dashboard endpoints remain at their existing paths. Their application sequencing and OpenTelemetry design belong to [004_DD-FN](004_DD-FN_製品マスタ.md); screen request behavior belongs to [004_DD-SPD](004_DD-SPD_製品マスタ.md) and the later WI-006 DD addendum.

## Product master representations

| Response | Fields and mapping |
| --- | --- |
| `ProductMasterItem` | `id` UUID; `sku` string ≤50; `name` string ≤200; `unit` one of seven approved values; `drawingNumber` string ≤100 or null; `isActive` boolean; `updatedAt` UTC timestamp; `version` unsigned 32-bit number from `xmin`; `unitLocked` boolean from existence of any `production_orders` reference. All fields are present, including nullable `drawingNumber`. |
| `PagedResult<ProductMasterItem>` | `items` array, `total` full filtered count, `page` effective page, `pageSize` always 20, `sort` always `sku`, `dir` always `asc`. A page beyond the end has empty `items` and the true `total`. |

`unitLocked` is read from the database, not inferred from `isActive`: a retired product with no orders may still change unit, and an active referenced product may not. It is included in list, detail and write responses so the edit screen can render without a separate check. It is informational; a concurrent order can make it stale immediately, and API-PM-04 always rechecks under the row lock. Product ID, SKU, and order references do not change on edit or retirement. A corrected product name is shown on historical orders because no name snapshot exists.

### API-PM-01 — `GET /api/product-master`

| Query field | Required | Rule / default |
| --- | --- | --- |
| `q` | no | Trim ends, at most 100 characters; case-insensitive SKU/name substring. Empty means all. |
| `state` | no | `all` (default), `active`, `retired`; unknown value → 400 `state`. |
| `page` | no | Integer ≥1 (default 1); invalid → 400 `page`. |

200 returns `PagedResult<ProductMasterItem>`, ordered by SKU ascending then ID for stable ties. Apply search and state before count/page. No body. Product rows include `unitLocked`, computed with a bounded existence check supported by `ix_production_orders_product_id`; the list page is capped at 20. Invalid query gives 400 field errors; an out-of-range but valid page is 200 with an empty array.

### API-PM-02 — `GET /api/product-master/{id}`

`id` is a UUID route segment. 200 returns `ProductMasterItem` including `unitLocked` and current `version`; retired products remain readable. Malformed or missing ID gives 404 after authorization. No body.

### API-PM-03 — `POST /api/product-master`

| Body field | Required | Normalization / validation |
| --- | --- | --- |
| `sku` | yes | Trim ends; 1–50 characters, nonblank; uniqueness under PostgreSQL `lower(sku)` collation, regardless of active state. No extra character whitelist. |
| `name` | yes | Trim ends; 1–200 characters, nonblank; Japanese text allowed. |
| `unit` | yes | Exactly `個`, `本`, `枚`, `台`, `セット`, `kg`, or `m`. |
| `drawingNumber` | no | Trim ends; blank becomes null; at most 100 characters. |

201 returns `Location: /api/product-master/{id}` and `ProductMasterItem` with `isActive=true`, `unitLocked=false`, generated ID/version/timestamp. Missing, malformed, overlong or unsupported fields give 400. A duplicate SKU, including a unique-index race, gives 409 with `errors.sku`. The body may not set ID, state, version, `unitLocked` or timestamps. A DB collation check is required during implementation for non-ASCII case behavior; the API does not invent a divergent in-memory SKU comparison.

### API-PM-04 — `PUT /api/product-master/{id}`

| Body field | Required | Rule |
| --- | --- | --- |
| `sku` | yes | Must match stored SKU exactly; a change gives 400 `errors.sku`. |
| `name` | yes | Same normalization and length as create. |
| `unit` | yes | One of the seven values. It may differ from stored unit only when no order references this product. |
| `drawingNumber` | no | Same as create; omitted or blank means null in this full-replacement edit. |
| `version` | yes | Unsigned 32-bit value from last product read; missing/invalid is 400, stale is 409. |

200 returns the updated `ProductMasterItem` and new version. The edit service locks the product row, checks version and reference existence, then saves atomically (004_DB); the client-side `unitLocked` state never authorizes a write. A changed unit on a referenced product gives 409 `PRODUCT_UNIT_LOCKED` with `errors.unit`, even when the submitted version is current. A stale version gives 409 `PRODUCT_STALE` with `errors.version` before unit comparison. Both retain the draft and offer reload; no silent retry. A retired product may still correct name/drawing number and, if unreferenced, unit; it remains retired. The request cannot set `isActive` or `unitLocked`. Missing ID is 404.

### API-PM-05 — `POST /api/product-master/{id}/retire`

Only `{ "version": 81234 }` is accepted. 200 returns `ProductMasterItem` with `isActive=false` and a new version. Missing ID is 404; missing/invalid version or an already retired product is 400; stale version is 409 `PRODUCT_STALE`. This is a versioned update, never DELETE. It preserves all historical orders and their product link. A product may be retired even when referenced, because existing orders remain readable; an order write and retirement are serialized by the product-row locks described in 004_DB.

## Existing product and production-order API changes

### API-PRD-01 — `GET /api/products` (existing route)

Keep the existing unpaged array and `id`, `sku`, `name` fields; add required `unit` and `isActive` to each row. Return **all** products, including retired ones, ordered by SKU. Example row: `{ "id": "…", "sku": "P-1002", "name": "ブレーキディスクローター", "unit": "枚", "isActive": false }`. SCR-001 filters new/changed selections to active rows but retains the saved retired product on an old order; SCR-002 keeps retired rows in its filter. The server independently enforces eligibility on writes. A missing unit after a completed migration is an integrity failure, not an implicit `個`.

### API-PO-01/02/03 — order create, detail and edit

| Existing shape | WI-006 delta | Preserved behavior |
| --- | --- | --- |
| `POST /api/production-orders` request | `quantity` becomes an exact decimal JSON number validated against the selected product unit; `productId` must identify an active product | Due date, notes, order-number creation and success status remain per 001_DD-API. |
| `GET /api/production-orders/{id}` response | `quantity` becomes an exact decimal JSON number; add required top-level `unit` from the referenced product | Product ID, status, version, timestamps and editable-state fields remain. Retired product still joins. |
| `PUT /api/production-orders/{id}` request | `quantity` becomes an exact decimal JSON number; when product ID changes, the new product must be active; unchanged retired product ID is allowed | Existing Draft-only product/quantity editing, status transition, due-date, notes, version and response status remain. |
| POST/PUT success response | `quantity` becomes decimal JSON number; add required top-level `unit` | Other fields follow the existing `ProductionOrderResponse`. |

For Draft edits, the selected product is locked/read before validating quantity. If a product changes, the new target must be active; retaining the old retired product ID is permitted. The quantity must match the **selected product's immutable unit** at commit. For non-Draft edits, product ID and numeric quantity remain unchanged under the existing state rule; a retired product stays readable. A retired new/changed selection gives 400 `PRODUCT_INACTIVE` with `errors.productId`. Missing or invalid product ID follows existing `productId` validation. Nonpositive, excessive-scale, out-of-range or discrete fractional quantity gives 400 `QUANTITY_UNIT_INVALID` with `errors.quantity`. A race where retirement commits before selection is validated returns the same field error; no invalid order is saved. Existing order concurrency and 401/403/404 behavior remain.

### API-PO-04 — `GET /api/production-orders` (existing list)

Keep query filters, sort keys, pagination and envelope from [002_DD-API](../002/002_DD-API_製造指示一覧.md). Each `items[]` row keeps its existing fields, changes `quantity` to an exact decimal JSON number and adds required `product.unit` to the existing `{ id, sku, name }` summary. The product filter still includes retired products. `sort=quantity` sorts the stored numeric value; each displayed value carries its own unit, and the sort makes no cross-unit physical comparison claim. A missing product/unit is an integrity failure, not a unitless valid row.

## API-DSH-01 — `GET /api/dashboard` (existing snapshot)

The route, authorization, read-only `REPEATABLE READ` snapshot, `asOf`, `today`, `timeZone`, plant-time windows, status counts, overdue/due-soon totals, on-time, lead-time and 12-week completion trend remain as [003_DD-API](../003/003_DD-API_ダッシュボード.md) defines. The WI-006 response changes below prevent sums across unlike units:

| Response location | WI-006 contract | Meaning |
| --- | --- | --- |
| `overdue.orders[]`, `dueSoon.orders[]` | Change `quantity` to decimal JSON number; add required `product.unit` | Each attention row shows its own unit; group `total` remains order count. |
| `workload[]` | Exactly 10 existing buckets, each with unchanged `kind`, `weekStart`, `weekEnd`, `orderCount`; **remove** mixed-unit `quantity`; add required `unitQuantities: [{ unit, quantity }]` | `orderCount` drives chart bar and across-unit total. Each `quantity` is a decimal subtotal for that unit only. Empty bucket has `unitQuantities: []`. Unit entries use the approved unit-list order. |
| `topProducts[]` | 0–10 rows; add required `product.unit`; change `openQuantity` to decimal JSON number; `activeOrderCount` unchanged | Rank by `activeOrderCount` descending then SKU ascending; `openQuantity` is within one product/unit. |
| `completedThisWeek`, `completedThisMonth` | Keep `orderCount` and `from`; **remove** mixed-unit `quantity` | Display counts and periods only. |

No separate mixed-unit quantity total is exposed. Workload `unitQuantities` omit absent units; each unit appears at most once per bucket. The bucket `orderCount` counts all orders across units, while the array gives quantity subtotals without implying a common quantity total. `openQuantity` for one product is valid because its unit becomes immutable after its first order. A missing product/unit after migration is an integrity failure and the dashboard returns its generic failure instead of a fabricated unit. This response is a deliberate contract change requiring coordinated frontend/backend deployment after both WI-006 migrations; old dashboard clients must not render the removed fields as `個`.

## Error mapping, compatibility and observability

| Condition | HTTP / code | Field / client action |
| --- | --- | --- |
| Invalid list query or product/write field | 400 `VALIDATION` | `errors` keyed by `q`, `state`, `page`, `sku`, `name`, `unit`, `drawingNumber`, `quantity`, etc.; retain input. |
| Changed SKU | 400 `VALIDATION` | `errors.sku`; do not save. |
| Duplicate SKU | 409 `PRODUCT_SKU_CONFLICT` | `errors.sku`; correct and resubmit. |
| Referenced product unit changed | 409 `PRODUCT_UNIT_LOCKED` | `errors.unit`; reload product; no write. |
| Stale product version | 409 `PRODUCT_STALE` | `errors.version`; reload; no silent overwrite. |
| New/changed retired order selection | 400 `PRODUCT_INACTIVE` | `errors.productId`; choose active product. |
| Invalid quantity for selected unit | 400 `QUANTITY_UNIT_INVALID` | `errors.quantity`; correct numeric text. |
| Unknown product/order ID | 404 `NOT_FOUND` | Generic not-found; no foreign-key or SQL detail. |
| Non-JSON mutation | 415 `UNSUPPORTED_MEDIA_TYPE` | Generic Problem Details. |
| Unexpected/integrity failure | 500 `UNEXPECTED` | Generic Problem Details; server records trace ID and classified outcome without body/field values. |

These `code` values are stable machine-readable outcomes; the single Japanese client message catalog supplies user text and field messages. JSON model-binding failures map to the same Problem Details shape, never exposing framework exception text. A product unit-lock conflict is distinct from a stale version so the UI can explain why a fresh edit still fails. No new CORS or role policy is introduced. Never log cookie, full body, SKU, name, drawing number or decimal input text; the FN companion defines bounded OpenTelemetry spans/metrics by route and outcome only.

Before release, update all affected .NET and TypeScript DTOs in one coordinated rollout, run contract/integration tests against numeric inputs including `1.2345`, `1.2340`, `0.001` and the seven units, and verify old integer histories and retired-product reads. The present design phase has executed no endpoint, migration or application test. The only remaining technical contract work is to reconcile the FN/SPD drafts and the later existing-screen DD addendum with this approved API design before implementation.
