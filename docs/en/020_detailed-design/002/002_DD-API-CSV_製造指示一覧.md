<!-- Based on ai/templates/DD/api-design.md. Companion of 002_DD-CSV (WI-016 DEC-011). 002_DD-API stays unedited. -->

# Production Order List — CSV Export — API Specification Design (API仕様設計)

002_DD-API-CSV — supports [002_DD-CSV](002_DD-CSV_製造指示一覧.md), implements
[002_BD-CSV](../../010_basic-design/002/002_BD-CSV_製造指示一覧.md) FN-041/FN-042, requirements REQ-085–REQ-088.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | 002_DD-API-CSV |
| System name | ProductionManagementAI |
| Subsystem name | Production orders |
| Work item | WI-016 |
| Created by | Claude (for ThongTM) |
| Created date | 2026-10-07 |
| Last updated by | Claude (for ThongTM) |
| Last updated date | 2026-10-07 |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | 2026-10-07 | Claude (for ThongTM) | Initial creation: API-PO-05; answers the 002_BD-CSV open questions on the row-limit status and the row-count header |

## Overview and operation catalog

| Field | Value |
| --- | --- |
| API / module name | Production orders API — CSV export |
| Overview | One read-only endpoint that returns, as a CSV file, every production order matching the list's filters in the list's sort order |

| No | Endpoint / method | Handler | Purpose | Notes |
| --- | --- | --- | --- | --- |
| 1 | `GET /api/production-orders/export` | `ProductionOrdersController.Export` (002_DD-CSV module 6) → `ProductionOrderService.ExportAsync` (002_DD-FN-CSV) | CSV export of the filtered list | New |

### System-wide API registry entries

| ID | Endpoint | Overview | Notes |
| --- | --- | --- | --- |
| API-PO-05 | `GET /api/production-orders/export` | Export production orders matching the filters as CSV | New; continues the registry of 001_DD-API and 002_DD-API (API-PO-01–API-PO-04 unchanged) |

## Endpoint / method design

### 1. GET /api/production-orders/export

| Field | Value |
| --- | --- |
| Description | Return all production orders matching the filters, in the requested order, as one UTF-8 CSV file with a header row; refuse when more than 10,000 orders match |
| Return type | File (`text/csv`), streamed |
| Created by / date | Claude / 2026-10-07 |
| Last modified by / date | — |

**Arguments**: query-string parameters only — see Request fields. No route or body parameters.

Processing overview: authorize with the list's policy; validate the query string with the list's validator
(`ProductionOrderListQuery.TryCreate`, 002_DD-API §1 rules, paging left at its defaults and not used); verify the
product filter; in one read-only transaction count the matches, refuse with 422 when the count exceeds 10,000,
otherwise send the response headers (including the count) and stream the header row and one CSV row per order in
the requested order. Formatting and escaping are FN-042 (002_DD-FN-CSV).

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Authorize (`Admin`/`Operator`) | policy `ProductionOrderEditor` (class attribute, as the list) |
| 2 | Validate the query; on failure 400 with per-parameter message IDs | `ProductionOrderListQuery.TryCreate` (unchanged) |
| 3 | Verify the product filter exists, when sent; else 400 `productId: MSG-E002` | `IProductionOrderRepository.ProductExistsAsync` (existing) |
| 4 | Open a read-only snapshot; count matches | `ProductionOrderService.ExportAsync` → 002_DD-FN-CSV §1 |
| 5 | Count > 10,000 → 422 `MSG-E024` | 002_DD-FN-CSV §1 |
| 6 | 200: headers, then BOM, header row and rows, streamed | 002_DD-FN-CSV §2, §3 |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| CSV file | body | See "Response body (CSV)" below |

**Request fields**

The filter and sort parameters of API-PO-04 (002_DD-API §1 request fields 1–7), with the same names, types, defaults,
allow-lists and error message IDs. They are not restated here.

| No | Name | Variable name | Type | Length | Required | Source | Example | Description | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1–7 | Status, Product, Due from, Due to, Order number, Sort, Direction | `status` (repeatable), `productId`, `dueFrom`, `dueTo`, `orderNumber`, `sort`, `dir` | as 002_DD-API | as 002_DD-API | no | query | `?status=InProgress&status=Draft&dueFrom=2026-10-01&dueTo=2026-10-31&sort=dueDate&dir=asc` | Same meaning as on the list | Same validation and message IDs (002_BD V-09–V-13); default sort `dueDate` `asc`, order number ascending as tie-breaker |
| — | Page, Page size | `page`, `pageSize` | — | — | — | — | — | Not parameters of this endpoint | Ignored if sent, like any unknown query parameter; the export always covers all pages (DEC-002). The client does not send them (002_DD-CSV module 3) |

**Response headers (200)**

| No | Header | Value | Example | Notes |
| --- | --- | --- | --- | --- |
| 1 | `Content-Type` | `text/csv; charset=utf-8` | | REQ-087 |
| 2 | `Content-Disposition` | `attachment; filename="production-orders_{yyyyMMdd-HHmm}.csv"; filename*=UTF-8''{percent-encoded 製造指示一覧_{yyyyMMdd-HHmm}.csv}` | `attachment; filename="production-orders_20261007-1345.csv"; filename*=UTF-8''%E8%A3%BD%E9%80%A0%E6%8C%87%E7%A4%BA%E4%B8%80%E8%A6%A7_20261007-1345.csv` | 002_BD-CSV M-16; time is plant-local (`IPlantClock`) at the start of the export (RFC 6266 / RFC 8187) |
| 3 | `X-Total-Count` | Number of data rows in the body | `87` | Decided here (BD open question). Equals the count from step 4, which reads the same snapshot as the rows (002_DD-FN-CSV §1). Read by the client for MSG-I009 |
| 4 | `Cache-Control` | `no-store` | | Existing class-level `[ResponseCache(NoStore = true)]` |

No `Content-Length`: the body is streamed (chunked). Same origin, so no CORS exposure of `X-Total-Count` is needed.

**Response body (CSV)**

Encoding UTF-8 with BOM (`EF BB BF`) as the first three bytes; lines end with CRLF, including the last; separator
comma; RFC 4180 quoting; formula-like text neutralised (002_BD-CSV CSV file layout, DEC-004). The header row is
always present, so a body has 1 + `X-Total-Count` lines.

| No | Column header | Source | Type in file | Value mapping | Example | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | `指示番号` | `production_orders.order_number` | text | — | `PO-2026-00041` | |
| 2 | `製品コード` | `products.sku` | text | — | `P-1001` | |
| 3 | `製品名` | `products.name` | text | — | `ブレーキキャリパー` | Neutralised if formula-like |
| 4 | `生産ライン` | `production_lines.code`, `.name`, `.is_active` | text | `{code} — {name}`, plus ` (使用停止)` if retired; empty if none (M-14) | `L-01 — 第1組立ライン` | |
| 5 | `数量` | `production_orders.quantity` | number | Invariant culture, `.` decimal point, no grouping, trailing zeros removed (M-12) | `12.5` | Never neutralised |
| 6 | `単位` | `products.unit` | text | — | `kg` | |
| 7 | `納期` | `production_orders.due_date` | date | `yyyy/MM/dd` (M-13) | `2026/10/02` | Plant-local date, no conversion |
| 8 | `ステータス` | `production_orders.status` | text | `Draft`→`下書き`, `InProgress`→`進行中`, `Completed`→`完了`, `Cancelled`→`取消` (M-11) | `進行中` | |
| 9 | `納期遅れ` | computed | text | `納期遅れ` when due date < plant today and status ∈ {Draft, InProgress}, else empty (M-15) | `納期遅れ` | Same rule as `isOverdue` in API-PO-04 |
| 10 | `備考` | `production_orders.notes` | text | As stored; null → empty; quoted when it contains `,` `"` CR or LF | `"梱包は""A""仕様、2段積み"` | ≤ 500 characters (001_BD); neutralised if formula-like |
| 11 | `作成日時` | `production_orders.created_at_utc` | timestamp | UTC → `Asia/Tokyo`, `yyyy/MM/dd HH:mm` (M-13, DEC-008) | `2026/09/28 09:15` | |
| 12 | `更新日時` | `production_orders.updated_at_utc` | timestamp | as column 11 | `2026/09/30 14:02` | |
| 13 | `完了日時` | `production_orders.completed_at_utc` | timestamp | as column 11; empty when null | `2026/10/05 17:30` | Set by WI-004 on `InProgress → Completed` |

Header row (exact): `指示番号,製品コード,製品名,生産ライン,数量,単位,納期,ステータス,納期遅れ,備考,作成日時,更新日時,完了日時`

Sample file (BOM, CRLF, all edge cases: decimal quantity, retired line, unassigned line, notes with comma, quotes,
line break and a leading `=`): [`mockups/002_DD-API-CSV_sample.csv`](mockups/002_DD-API-CSV_sample.csv). Excerpt
(BOM not shown):

```text
指示番号,製品コード,製品名,生産ライン,数量,単位,納期,ステータス,納期遅れ,備考,作成日時,更新日時,完了日時
PO-2026-00041,P-1001,ブレーキキャリパー,L-01 — 第1組立ライン,120,個,2026/10/02,進行中,納期遅れ,,2026/09/20 10:12,2026/09/28 09:15,
PO-2026-00052,P-2002,防錆塗料,L-03 — 塗装ライン (使用停止),12.5,kg,2026/10/03,下書き,,"梱包は""A""仕様、2段積み",2026/09/29 08:40,2026/09/30 14:02,
PO-2026-00060,P-1010,エアフィルター,,300,個,2026/10/12,下書き,,'=1+1 は顧客指定の品番表記,2026/10/01 13:00,2026/10/01 13:00,
```

**Error codes**

Errors are RFC 9457 Problem Details (`application/problem+json`) with `code` and `traceId`, exactly as the other
production-order endpoints (`ProductionOrderProblems`); no CSV body is sent with an error.

| Code | Meaning | HTTP status |
| --- | --- | --- |
| `VALIDATION` with `errors` `MSG-E015`–`MSG-E019`, `MSG-E002` | Invalid filter or sort parameter (same keys and IDs as API-PO-04) | 400 |
| — | Not signed in | 401 |
| — | Missing role (client shows MSG-E020) | 403 |
| `MSG-E024` (type `urn:pmai:problem:rule-violation`) | More than 10,000 orders match; decided here (BD open question): the existing `Result.RuleViolation` → 422 mapping, so no new error shape | 422 |
| `MSG-E013` | Unexpected failure before the first byte of the body | 500 |
| — | Failure after streaming started: the connection is aborted, so the client sees a network error, never a truncated "successful" file (002_DD-FN-CSV §3) | (connection reset) |

Example 422 body:

```json
{
  "type": "urn:pmai:problem:rule-violation",
  "title": "The change breaks a production order rule.",
  "status": 422,
  "code": "MSG-E024",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

The `title` is the shared English title of the existing rule-violation mapping; it is API text and never shown in
the UI (the client shows MSG-E024 from its catalog).

## Unresolved decisions

None. Decided in this document: 422 `MSG-E024` for the row limit and `X-Total-Count` for the row count (both
002_BD-CSV open questions). The policy limitation of API-PO-04 applies unchanged: the endpoint is gated by
`ProductionOrderEditor` until the role/permission matrix (WI-001 DEC-015) is settled.
