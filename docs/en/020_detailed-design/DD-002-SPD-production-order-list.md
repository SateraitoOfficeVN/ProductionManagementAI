<!-- Based on ai/templates/DD/screen-processing-design.md (revision at commit e7e0d36). -->

# Production Order List — Screen Processing Design (画面処理設計)

DD-002-SPD — elaborates DD-002, implements BD-002, requirements REQ-020–REQ-027.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | DD-002-SPD |
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
| 2 | 2026-09-22 | Claude (for ThanhTN) | WI-004 (DEC-016): the page renders inside the shared `AppHeader`, which now contains the navbar (`AppNavbar`, designed in DD-003 module 10 and DD-003-SPD §8; breadcrumb links become `GuardedLink`, DD-003 module 11). No processing step of this screen changes |

## Overview and process list

| Field | Value |
| --- | --- |
| Screen / file name | `src/frontend/src/features/production-orders/ProductionOrderListPage.tsx` and the components it composes; the server side of the request is in `ProductionOrdersController.List` |
| Overview | How SCR-002 reads its view state from the URL, queries, renders each state, and reacts to filtering, sorting, paging and navigation — one block per component |

| No | Process name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `ProductionOrderListPage` — P-10 Load and query | Role gate, view state from the URL, product options, query, state selection | E-10, E-19 |
| 2 | `useListViewState` — P-11 View-state synchronization | URL ⇄ state, defaults, history entries | E-10–E-15, E-19 |
| 3 | `ProductionOrderFilters` — P-12 Filter, search and clear | Local filter values, client validation, submit | E-11, E-12 |
| 4 | `ProductionOrderTable` — P-13 Sort and row activation | Header buttons, `aria-sort`, row link and mouse row click | E-13, E-16 |
| 5 | `ListPagination` — P-14 Page and page size | Prev/Next bounds, page-size change | E-14, E-15 |
| 6 | `ProductionOrderListPage` — P-15 Query-response handling | Mapping of each HTTP outcome to a screen state | E-10, E-18 |
| 7 | `ProductionOrdersController.List` — request boundary | Auth, query binding and validation, result → HTTP | Server steps are in DD-002-FN |

### Reference documents

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| 1 | DD-002 | Items, states, message catalog, module list | Parent document |
| 2 | DD-002-API | The endpoint contract called below | |
| 3 | DD-002-FN | Server-side steps (`ListAsync`, repository methods) | |
| 4 | BD-002 | Events E-10–E-19, validation V-09–V-13, states | |
| 5 | DD-001-SPD | SCR-001's processing, reached from this screen | Navigation target |

## Processing design

### 1. ProductionOrderListPage — P-10 Load and query

| Field | Value |
| --- | --- |
| Detail | Route component for `/production-orders` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: the URL is the single source of truth for what is displayed. The page reads the view state from it, fires exactly one list query per distinct view state, and renders one of five mutually exclusive states. Every user action changes the URL; the query is a reaction to that change, never a second, parallel path — so Back, Forward, reload and a pasted link all go through the same code as a button press.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `useAuth` | Current user and roles | WI-001 |
| 2 | `useListViewState` | View state from the URL | §2 |
| 3 | `api.ts` (`listOrders`, `listProducts`) | Typed wrappers over `apiClient` | DD-002 module 10 |
| 4 | `ProductionOrderFilters`, `ProductionOrderTable`, `ListPagination`, `MessageBanner` | Rendered children | `MessageBanner` reused from DD-001 |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Read the view state from the URL | Unreadable or out-of-range value → that parameter falls back to its default and the URL is rewritten once with `replace` | §2 | No error page (REQ-027) |
| 2 | Role gate on `user.roles` | Neither `Admin` nor `Operator` → stop | — | `forbidden` panel (MSG-E020), no API call |
| 3 | Fetch the product options once per mount | Failure → product filter disabled with a load error; the rest of the screen still works | `GET /api/products` | Other filters remain usable |
| 4 | Set state `loading`; run the list query for the current view state | A view state identical to the one in flight is not re-queried; a new one supersedes the old, whose response is ignored | `GET /api/production-orders` | Loading indicator replaces the table body, `aria-busy="true"`; previous rows are not presented as the result |
| 5 | Handle the response | See §6 | §6 | `ready` / `empty` / `noMatch` / `error` |
| 6 | Set `document.title` to `Production orders — ProductionManagementAI` | — | — | — |

Five render states, mutually exclusive: `loading`, `ready` (table with rows), `empty` (no orders exist at all, MSG-I003), `noMatch` (filters set, nothing matched, MSG-I004), `error` (banner MSG-E013 with **Retry**). `forbidden` is a sixth, reached only at step 2 or from a 403.

### 2. useListViewState — P-11 View-state synchronization

| Field | Value |
| --- | --- |
| Detail | Hook owning the parse/serialize of the nine URL parameters (BD-002 0-3) |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: one place converts between the URL's query string and the typed view state, so the page never reads `location.search` itself and the API query is built from the same values that are displayed. Parsing is total — it never throws and never produces an invalid state; anything unrecognized falls back to that parameter's default.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `useSearchParams` | react-router URL access | |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Parse `status` (repeatable) | Unknown value → dropped; duplicates collapsed | — | `Set<Status>` |
| 2 | Parse `productId` | Not a UUID → dropped | — | `string \| null` |
| 3 | Parse `dueFrom`, `dueTo` | Malformed → dropped; `from > to` → both kept and shown with the V-10 error, since silently dropping a value the user can see would be worse | — | `string \| null` each |
| 4 | Parse `orderNumber` | Trimmed; longer than 20 → truncated to 20 | — | `string \| null` |
| 5 | Parse `sort`, `dir` | Outside the allow-list → defaults `dueDate`, `asc` | — | Enum values |
| 6 | Parse `page`, `pageSize` | `page < 1` or non-numeric → 1; `pageSize` outside {10,20,50,100} → 20 | — | Integers |
| 7 | `setView(next, { push })` serializes back, omitting every parameter that equals its default | Filter/sort/page change → `push` (a history entry, so Back works). A fallback rewrite at step 1–6 → `replace` | — | URL updated; P-10 step 4 reacts |

Omitting defaults keeps a shared link short and readable (`/production-orders?status=Draft` rather than nine parameters), and it means the plain route and the fully defaulted URL are the same view.

### 3. ProductionOrderFilters — P-12 Filter, search and clear

| Field | Value |
| --- | --- |
| Detail | Filter panel (items 7–13), a real `<form>` submitted by **Search** |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: the panel holds its own draft values while the user edits, and only publishes them to the view state on submit (DEC-008). That is what makes "the URL reflects what is displayed" true — an unsubmitted draft is not displayed, so it is not in the URL.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `validateFilters` | Client-side V-09, V-10 | DD-002 module 5 |
| 2 | `messages.ts` | Message IDs → text | Extended catalog |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Initialize the draft from the current view state | On every view-state change (e.g. Back) the draft re-bases, so the panel always shows what produced the rows | — | Fields filled |
| 2 | Status checkbox / product select / date / text edits update the draft only | — | — | No query |
| 3 | **Search** (click or Enter in the panel) → run `validateFilters` | Invalid → show inline errors, move focus to the first invalid field, send no query and leave the rows as they are | §DD-002 module 5 | Field errors (MSG-E015/E016/E017) |
| 4 | Valid → publish the draft to the view state with `page = 1`, keeping sort and page size | — | §2 `setView(push)` | P-10 queries |
| 5 | **Clear** (item 12) or **Clear filters** (item 25) → reset the draft to empty, publish with `page = 1` | Disabled when no filter is set | §2 | P-10 queries the unfiltered list |

### 4. ProductionOrderTable — P-13 Sort and row activation

| Field | Value |
| --- | --- |
| Detail | Results table (items 16–22, 27), or the card list at the SP breakpoint |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: a real `<table>` whose header cells contain buttons; `aria-sort` on the active column is the accessible equivalent of the ▲/▼ marker. Row activation is a genuine link, with the row-level mouse click as a convenience on top (DEC-009).

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `Link` (react-router) | Order-number cell link to SCR-001 | |
| 2 | `statusLabels`, `formatDate` | Status label (M-05) and date formatting (M-10) | `messages.ts` |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Render one row per item; the order-number cell is a `<Link to={`/production-orders/${id}`}>` | `isOverdue` → render the overdue marker next to the due date (item 27) | — | Table body |
| 2 | Header button click/Enter/Space | Already the active key → flip `dir`; otherwise set the key with `asc` | §2 `setView(push)` with `page = 1` | P-10 queries; `aria-sort` moves |
| 3 | Row mouse click | Ignored when the click ended a text selection, or landed on the link itself or another interactive element | `navigate(id)` | Same destination as the link |
| 4 | SP breakpoint | Table replaced by one card per order, the whole card being the link; the sort select replaces the header buttons | §2 | Same query behavior |

Step 3 exists only for the mouse. Keyboard users reach step 1's link through normal tab order, so no behavior depends on the row handler — if it were removed, the screen would still be fully operable.

### 5. ListPagination — P-14 Page and page size

| Field | Value |
| --- | --- |
| Detail | Result summary, page-size select and Previous/Next (items 14, 15, 23) |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: page bounds are computed from the response's `total` and `pageSize`, not from what the client asked for, so the controls stay correct even when a request was made against a stale view.

**Used components / services**: none beyond §2.

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | `lastPage = max(1, ceil(total / pageSize))`; render "Page {page} of {lastPage}" and the M-09 summary | `total = 0` → the summary is replaced by the empty or no-match state | — | — |
| 2 | **Previous** / **Next** | Disabled (not hidden) at page 1 and at `lastPage`; a page past `lastPage` still renders the controls so the user can go back | §2 `setView(push)` | P-10 queries |
| 3 | Page-size change | Always resets `page` to 1 | §2 `setView(push)` | P-10 queries |

### 6. ProductionOrderListPage — P-15 Query-response handling

| Field | Value |
| --- | --- |
| Detail | One place mapping every HTTP outcome of the list query to a screen state |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: the client renders from the response's echoed `sort`, `dir`, `page` and `pageSize`, not from its own request, so a late or retried response can never leave the table, the summary and the sort marker disagreeing.

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | 200 with `total > 0` | — | — | `ready`: rows, summary, paging; the summary's live region announces the new total |
| 2 | 200 with `total = 0` | No filter set → `empty` (MSG-I003 + **New production order**); any filter set → `noMatch` (MSG-I004 + **Clear filters**) | — | Filter panel keeps its values in both cases |
| 3 | 200 with `items = []` but `total > 0` | Page past the last one | — | `ready` with an empty body and working paging controls — not an error (REQ-024) |
| 4 | 400 | Attach each `errors` entry to its filter field by parameter name; a parameter with no visible field (e.g. `sort`) → error banner | — | Rows unchanged; MSG-E015–E019, MSG-E002 |
| 5 | 401 | — | apiClient redirects | `/login` |
| 6 | 403 | — | — | `forbidden` panel (MSG-E020) |
| 7 | Network failure or 5xx | — | — | `error` banner MSG-E013 with **Retry** (E-18), which re-runs the same query unchanged |

### 7. ProductionOrdersController.List — request boundary

| Field | Value |
| --- | --- |
| Detail | Server entry point for `GET /api/production-orders` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: bind the query string, validate it into a typed query object, delegate, and map the result to HTTP. No business rule and no query composition lives here — those are in DD-002-FN.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `ProductionOrderListQuery.TryCreate` | Validation and normalization | DD-002 module 5 |
| 2 | `ProductionOrderService.ListAsync` | The query use case | DD-002-FN §1 |
| 3 | `ProductionOrderProblems.ToActionResult` | `Result<T>` → HTTP, RFC 9457 | Existing, DD-001 |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Authorize | No session → 401; wrong role → 403 | policy `ProductionOrderEditor` | Empty body, as DD-001-API |
| 2 | Bind the query parameters into the raw request type, whose fields are all strings | No type-level binding failure is possible, so the existing `InvalidModelStateResponseFactory` never fires here: `page=abc` reaches `TryCreate` and gets MSG-E019, its designed message, rather than a generic binding error | — | — |
| 3 | `TryCreate` validates and normalizes | Any failure → 400 with every offending parameter reported together | module 5 | — |
| 4 | Delegate | — | `ListAsync` | — |
| 5 | Map the result | `Ok` → 200; `Invalid` → 400 | `ToActionResult` | — |

## Unresolved decisions

None.
