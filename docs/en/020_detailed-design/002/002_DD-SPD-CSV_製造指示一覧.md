<!-- Based on ai/templates/DD/screen-processing-design.md. Companion of 002_DD-CSV (WI-016 DEC-011). 002_DD-SPD stays unedited. -->

# Production Order List — CSV Export — Screen Processing Design (画面処理設計)

002_DD-SPD-CSV — elaborates [002_DD-CSV](002_DD-CSV_製造指示一覧.md), implements
[002_BD-CSV](../../010_basic-design/002/002_BD-CSV_製造指示一覧.md) E-30–E-33 and FN-043, requirements REQ-085 and
REQ-089.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | 002_DD-SPD-CSV |
| System name | ProductionManagementAI |
| Subsystem name | Production orders |
| Work item | WI-016 |
| Created by | Claude (for ThongTM) |
| Created date | 2026-10-07 |
| Last updated by | Claude (for ThongTM) |
| Last updated date | 2026-10-07 |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | 2026-10-07 | Claude (for ThongTM) | Initial creation |

## Overview and process list

| Field | Value |
| --- | --- |
| Screen / file name | `ExportCsvButton.tsx`, `ProductionOrderListPage.tsx`, `api.ts`, `lib/apiClient.ts`, `lib/download.ts`; backend `ProductionOrdersController.Export` |
| Overview | What happens, step by step, from a press of 「CSV出力」 (Export CSV) to the saved file or the shown error, and how the controls reset when the list view changes |

Process IDs continue 002_DD-SPD (P-10–P-15).

| No | Process name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `ExportCsvButton` — P-16 Export from the list | Activation, limit pre-check, busy state, request, save, announce or map the error | E-30, E-31, E-32 |
| 2 | `ProductionOrderListPage` — P-17 Reset on view change | Re-mount the export controls whenever the applied view changes | E-33 |
| 3 | `exportOrdersCsv` / `apiClient.getFile` — P-18 File request | Build the URL, fetch, keep the session/error conventions, read blob, name and count | API-PO-05 |
| 4 | `saveFile` — P-19 Save | Hand the blob to the browser as a download | |
| 5 | `ProductionOrdersController.Export` — request boundary | Validate, call the service, return the CSV result or Problem Details | Server side in 002_DD-FN-CSV |

### Reference documents

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| 1 | 002_BD-CSV | Events E-30–E-33, items 28–30, messages | Version 2 |
| 2 | 002_DD-CSV | Modules 1–6, states, screen items | Version 1 |
| 3 | 002_DD-API-CSV | Request, headers (`Content-Disposition`, `X-Total-Count`), status codes | Version 1 |
| 4 | 002_DD-FN-CSV | Server processing behind P-18 | Version 1 |
| 5 | 002_DD-SPD | P-10–P-15, the list's load/query and view-state processing that P-17 hooks into | Unedited |

## Processing design

### 1. `ExportCsvButton` — P-16 Export from the list

| Field | Value |
| --- | --- |
| Detail | Handles activation of item 28 and drives items 29 and 30 through the states of 002_DD-CSV |
| Created by / date | Claude / 2026-10-07 |
| Last modified by / date | — |

Processing overview: local state `phase: 'ready' | 'exporting'` and `notice: { kind: 'success', count } | { kind:
'error', ids: string[] } | null`, plus a `busyRef` set synchronously so a fast double activation cannot start two
requests. An `AbortController` lives for the component's lifetime; unmounting aborts any request in flight (P-17).

Focus while busy: the button is **not** given the HTML `disabled` attribute during an export, because a focused
element that becomes `disabled` can drop keyboard focus to the page body, which would break "focus stays on the
button" (002_BD-CSV E-30). It gets `aria-disabled="true"` and `aria-busy="true"`, the disabled styling, and the click
handler returns immediately while busy. This refines 002_DD-CSV's screen item definition, which says `disabled`; see
"Unresolved decisions". The `disabled` prop of module 1 (list query in flight) still uses the real attribute, because
then the button is not the focused control.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `exportOrdersCsv` (P-18) | Request and response reading | |
| 2 | `saveFile` (P-19) | Download | |
| 3 | `exportHint`, `message`, `labels.list` | Texts | 002_DD-CSV catalog |
| 4 | `ApiError` | Error classification | `lib/apiClient.ts` |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Render: button 「CSV出力」 with `aria-describedby="export-hint"`, hint (item 30), message container (item 29, `role="status"`, empty) | `disabled` prop true → real `disabled` | `exportHint(view, total)` | Ready |
| 2 | Activation (click, or Enter/Space on the button) | `busyRef.current` → return (no second request) | — | — |
| 3 | Row-limit pre-check (V-14) | `total > 10_000` → `notice = error [MSG-E024]`; stop. No request | — | Failed (limit); alert rendered |
| 4 | Enter busy: `busyRef = true`, `phase = 'exporting'`, `notice = null` | — | — | Button shows 「出力中…」, `aria-disabled`, `aria-busy`; focus unchanged |
| 5 | Request | — | `exportOrdersCsv(view, controller.signal)` (P-18) | — |
| 6 | Success: save the file, then announce | — | `saveFile(blob, fileName)` (P-19) | `notice = success(count)` → `role="status"` text MSG-I009 with `formatNumber(count)` |
| 7 | Failure: classify | `AbortError` (unmounted) → do nothing (component is gone). `ApiError` 401 → nothing here; `apiClient` already cleared the session and `ProtectedRoute` redirects. 403 → [MSG-E020]. 422 with `problem.code === 'MSG-E024'` → [MSG-E024]. 400 with `problem.errors` → the message IDs in `errors`, flattened and de-duplicated (as the list page does). Anything else (other status, network `TypeError`, body read failure) → [MSG-E025] | — | Failed; `role="alert"` paragraph with `ErrorIcon` and the joined messages |
| 8 | Leave busy (`finally`, unless unmounted) | — | — | `busyRef = false`, `phase = 'ready'`; button 「CSV出力」 again; focus unchanged |

Notes:

- The success text uses the server's `X-Total-Count`, not the list's `total`, so a change between the list query
  and the export is reported truthfully (002_BD-CSV "Data changed during export").
- Errors are rendered as a separate `role="alert"` element inside item 29 so they are announced assertively; the
  success text replaces the content of the always-mounted `role="status"` container so it is announced politely.

### 2. `ProductionOrderListPage` — P-17 Reset on view change

| Field | Value |
| --- | --- |
| Detail | Places module 1 and guarantees an old export state never describes a new view |
| Created by / date | Claude / 2026-10-07 |
| Last modified by / date | — |

Processing overview: the page already derives `viewKey = JSON.stringify(view)` for its query effect (002_DD-SPD
P-10). It renders `<ExportCsvButton key={viewKey} view={view} total={page.total} disabled={loading} />` inside the
`page && page.total > 0` branch. A new applied view (Search, Clear, sort, page, page size, Back/Forward — P-11 to
P-14) changes the key, and a running list query removes the branch (loading), so the component unmounts: its
`AbortController` aborts any export in flight, and item 29 disappears. When the query finishes, a fresh instance
renders with the new hint. Page and page-size changes also re-mount it; harmless, and it keeps one rule for all view
changes.

**Used components / services**: `ExportCsvButton` (P-16).

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | List query starts (P-10) | — | — | Branch not rendered → export controls unmounted; in-flight export aborted |
| 2 | List query succeeds | `total ≥ 1` → render module 1 with the new key; `total = 0` → empty or no-match panel, no module 1 | — | Ready with the new hint, or Hidden |
| 3 | List query fails | — | — | Error banner (P-15); no module 1 |

### 3. `exportOrdersCsv` / `apiClient.getFile` — P-18 File request

| Field | Value |
| --- | --- |
| Detail | Fetch API-PO-05 with the list's session and error conventions and return the file |
| Created by / date | Claude / 2026-10-07 |
| Last modified by / date | — |

Processing overview: `apiClient` gains `getFile(url, signal) → Promise<Response>`: the same `fetch` as `request`
(`credentials: 'same-origin'`, `signal`) with `Accept: 'text/csv, application/problem+json'`; on a non-2xx response
it does exactly what `request` does (401 → unauthorized handler; throw `ApiError(status, readProblem(response))`),
and on 2xx it returns the `Response` unread. This keeps one place that knows about 401 handling and Problem Details
(002_DD-CSV X-2). `exportOrdersCsv` in `api.ts` builds the query with `toSearchParams(view)`, deletes `page` and
`pageSize`, and reads the response.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `toSearchParams` | Same serializer as the URL and the list query | Unchanged |
| 2 | `apiClient.getFile` | New export from `lib/apiClient.ts` | Shares `readProblem` and the unauthorized handler |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | `params = toSearchParams(view)`; `params.delete('page')`; `params.delete('pageSize')` | — | `toSearchParams` | Only filters and non-default sort remain |
| 2 | URL `/api/production-orders/export` plus `?{params}` when not empty | — | — | — |
| 3 | `response = await getFile(url, signal)` | non-2xx → `ApiError` thrown (P-16 step 7) | API-PO-05 | — |
| 4 | `blob = await response.blob()` | Connection aborted by the server mid-stream (002_DD-FN-CSV §4) → `TypeError` → P-16 maps to MSG-E025 | — | Whole file in memory (≤ 10,000 rows, a few MB) |
| 5 | `count = Number(response.headers.get('X-Total-Count'))` | Missing or not a non-negative integer → treat as protocol error → MSG-E025 (never announce a wrong count) | — | — |
| 6 | `fileName` from `Content-Disposition`: `filename*=UTF-8''…` decoded with `decodeURIComponent`; else quoted `filename="…"`; else fallback `製造指示一覧_{YYYYMMDD-HHmm}.csv` built from the browser clock | Any `/`, `\` or control character removed from the name | — | — |
| 7 | Return `{ blob, fileName, count }` | — | — | — |

### 4. `saveFile` — P-19 Save

| Field | Value |
| --- | --- |
| Detail | Trigger the browser download of a blob under a name |
| Created by / date | Claude / 2026-10-07 |
| Last modified by / date | — |

Processing overview: no library; works in Chromium, Firefox and Safari, PC and SP.

**Used components / services**: none.

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | `url = URL.createObjectURL(blob)` | — | — | — |
| 2 | Create `<a href={url} download={fileName}>` (not attached to the visible layout, `tabIndex = -1`), `click()` it | — | — | Browser download starts (Playwright `download` event) |
| 3 | `setTimeout(() => URL.revokeObjectURL(url), 0)` | — | — | Memory released after the browser took the file |

### 5. `ProductionOrdersController.Export` — request boundary

| Field | Value |
| --- | --- |
| Detail | `[HttpGet("export")]` on the existing controller; class-level policy and `NoStore` apply |
| Created by / date | Claude / 2026-10-07 |
| Last modified by / date | — |

Processing overview: the HTTP edge only; all processing is in 002_DD-FN-CSV.

**Used components / services**: `ProductionOrderListQuery.TryCreate`, `ProductionOrderService.ExportAsync`,
`CsvExportResult`, `ProductionOrderProblems`.

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Bind `[FromQuery] ProductionOrderExportRequest` (strings only, like the list) | — | — | — |
| 2 | Map to `ProductionOrderListRequest` with `Page = null`, `PageSize = null`; validate | Invalid → 400 Problem Details | `TryCreate` | 400 |
| 3 | `ExportAsync(query, userId, ct)` | `Invalid` → 400; `RuleViolation` → 422 `MSG-E024` | 002_DD-FN-CSV §1 | 400 / 422 |
| 4 | `Ok(export)` → `return new CsvExportResult(export, …)` | — | 002_DD-FN-CSV §4 | 200, streamed CSV |

The route `export` cannot collide with `{id:guid}`, because `export` is not a GUID.

## Unresolved decisions

1. **Busy button uses `aria-disabled`, not `disabled` (proposed, WI-016 DEC-017).** 002_DD-CSV version 1 (approved)
   specifies `disabled` + `aria-busy` while exporting. To keep keyboard focus on the button as 002_BD-CSV E-30
   requires, this document uses `aria-disabled="true"` + `aria-busy="true"` with a click guard and the same disabled
   styling. If accepted, 002_DD-CSV gets a version 2 with this one-line change in its screen item definition and
   state table, and its PDFs are re-rendered; no other document changes.
2. Everything else: none. `apiClient.getFile` (answering 002_DD-CSV's open question about raw responses), the
   `X-Total-Count` validation and the file-name sanitising are decided here.
