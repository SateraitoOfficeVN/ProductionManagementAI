<!-- Based on ai/templates/test-plan.md (IEEE 829-1998). -->

# WI-016 — Production order CSV export — Test Plan

## Test plan identifier

TP-016, work item WI-016, revision 1, 2026-10-07 (plan revision 2).

## References

[brief.md](brief.md) (REQ-085–REQ-089), [plan.md](plan.md) revision 2,
[002_BD-CSV](../../docs/en/010_basic-design/002/002_BD-CSV_製造指示一覧.md) v2,
[002_DD-CSV](../../docs/en/020_detailed-design/002/002_DD-CSV_製造指示一覧.md) v2 (test viewpoints),
[002_DD-API-CSV](../../docs/en/020_detailed-design/002/002_DD-API-CSV_製造指示一覧.md) v1,
[002_DD-FN-CSV](../../docs/en/020_detailed-design/002/002_DD-FN-CSV_製造指示一覧.md) v1,
[002_DD-SPD-CSV](../../docs/en/020_detailed-design/002/002_DD-SPD-CSV_製造指示一覧.md) v1; decisions DEC-002–DEC-005,
DEC-008, DEC-009, DEC-013, DEC-015, DEC-017. The WI-003 Screen B cases remain the regression baseline.

## Introduction

This plan covers the 「CSV出力」 action on SCR-002 and its endpoint API-PO-05: the file's content and format, the
row limit, security, and the screen's states and accessibility. New case IDs start at TC-440 (WI-015 ended at
TC-435).

## Test items

| Requirement ID | Description |
| --- | --- |
| REQ-085 | Export all orders matching the applied filters, in the current sort, across all pages |
| REQ-086 | Fixed Japanese columns, screen formats (status label, dates, plant-time timestamps, exact decimals) |
| REQ-087 | UTF-8 with BOM, CRLF, RFC 4180 quoting, timestamped file name |
| REQ-088 | Admin/Operator only, list validation, formula neutralising, 10,000-row limit |
| REQ-089 | Hint, busy, success and error feedback; accessible on PC and SP |

## Features to be tested

TC-440 to TC-469 below.

## Features not to be tested

- Screen-reader speech and physical mobile keyboards (manual; Not run, as in WI-010–WI-015). Automated axe and
  ARIA-attribute assertions stand in.
- Excel itself is not automated; TC-469 is a manual check, Not run if no spreadsheet application is installed.
- Browsers other than Chromium in E2E (the project's Playwright config uses Chromium).

## Approach

| Level | Included? | Rationale |
| --- | --- | --- |
| Unit (backend, xUnit) | yes | CSV writer rules are pure and edge-case heavy; service outcomes with fakes |
| Unit (frontend, Vitest + RTL + vitest-axe) | yes | States, ARIA, error mapping, request shape, file name parsing |
| Integration (xUnit + WebApplicationFactory + Testcontainers PostgreSQL) | yes | Real SQL filters/sort, snapshot, headers, auth, 10,001-row limit, streaming |
| E2E (Playwright, local Compose stack) | yes | Real browser download on PC and SP, file bytes, axe |
| Performance | yes (measurement) | 10,000-row export time against the 5 s target (002_BD-CSV NFR) |
| Manual | yes | Open in a spreadsheet application (TC-469) |

## Item pass/fail criteria

A case passes when its expected result is observed with no unhandled error. The work item's tests pass when every
high-priority case passes and the existing backend, frontend and Screen B E2E suites still pass.

## Suspension criteria and resumption requirements

Suspend E2E if the local Compose stack cannot be rebuilt or the database is unavailable; resume when
`docker compose -f deploy/compose.yaml ps` shows all services up. Suspend integration tests if Docker is not
running (Testcontainers).

## Test deliverables

- This test plan with results; test code in `tests/backend/`, `tests/integration/`,
  `src/frontend/tests/unit/production-orders/`, `tests/e2e/specs/csv-export.spec.ts`; results in
  [evidence.md](evidence.md).

## Cases

| Test ID | Requirement ID | Precondition / setup | Steps | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-440 | REQ-087 | Writer, one row | Write to a `MemoryStream` | Starts with `EF BB BF`; exactly one BOM; every record ends `\r\n`; header row exact (13 Japanese headers) | high |
| TC-441 | REQ-087 | Writer | `Field` with `,` / `"` / `\n` / `\r\n` / plain text | Quoted when needed; `"` doubled; plain text unquoted; embedded line break kept | high |
| TC-442 | REQ-088 | Writer | Text fields starting with `=`, `+`, `-`, `@`, tab, CR; and `a=b` | Prefixed `'` (then quoted if needed); `a=b` unchanged | high |
| TC-443 | REQ-086, REQ-088 | Writer | Quantities `0.125`, `12.500`, `120`; dates and timestamps | `0.125`, `12.5`, `120`; no `'` on numeric or date columns | high |
| TC-444 | REQ-086 | Writer, plant clock Asia/Tokyo | Row with UTC timestamps around midnight UTC, completed null | `yyyy/MM/dd HH:mm` in JST (e.g. `2026-10-06T15:30Z` → `2026/10/07 00:30`); completed empty; due date unconverted | high |
| TC-445 | REQ-086 | Writer | Status each of 4; overdue combos (past due Draft/InProgress vs Completed/Cancelled; due today) | Labels 下書き/進行中/完了/取消; 納期遅れ only for past-due Draft/InProgress | high |
| TC-446 | REQ-086 | Writer | Line active, retired, null | `L-01 — 名`, `… (使用停止)`, empty | medium |
| TC-447 | REQ-086 | Writer, long text | Product name 200 chars, notes 500 chars without spaces, Japanese and ASCII mixed | Written whole, unquoted unless special chars; round-trips through an RFC 4180 parser | medium |
| TC-448 | REQ-086 | `PlantClock` | `ToPlantTime` for known instants | Correct Asia/Tokyo wall clock; `DateOf` unchanged | medium |
| TC-449 | REQ-088 | Service with fake repository returning count 10,001 | `ExportAsync` | `RuleViolation(MSG-E024)`; rows never enumerated; snapshot disposed | high |
| TC-450 | REQ-085, REQ-088 | Service, unknown product filter; and count 10,000 | `ExportAsync` | Unknown product → `Invalid{productId: MSG-E002}`; 10,000 → Ok with Count 10,000 | high |
| TC-451 | REQ-085 | Integration, seeded orders over several pages | Export with filters + each sort key/dir; compare with all pages of API-PO-04 | Same orders in the same order; `X-Total-Count` = list `total` = data rows | high |
| TC-452 | REQ-087 | Integration | Export, inspect response | 200; `text/csv; charset=utf-8`; `Content-Disposition` attachment with `filename="production-orders_yyyyMMdd-HHmm.csv"` and `filename*=UTF-8''…製造指示一覧…`; `Cache-Control` contains `no-store`; body starts with BOM | high |
| TC-453 | REQ-086 | Integration, order with notes `=cmd`, `"A",B`, multi-line; retired line; kg decimal; completed order | Export and parse | Cells as designed (`'=cmd`, quoted, line break kept, `(使用停止)`, `12.5`, completed timestamp) | high |
| TC-454 | REQ-088 | Integration | Invalid `status`, `sort`, `dueFrom > dueTo`, 21-char order number, unknown product | 400 Problem Details, same `errors` keys and IDs as API-PO-04; no CSV | high |
| TC-455 | REQ-088 | Integration | Anonymous request; signed-in user without Admin/Operator | 401; 403 | high |
| TC-456 | REQ-088 | Integration, 10,001 matching rows seeded | Export | 422 `application/problem+json`, `code` `MSG-E024`; no CSV body | high |
| TC-457 | REQ-085 | Integration | Export with `page=3&pageSize=10` | Ignored: all matching rows returned | medium |
| TC-458 | REQ-089 | Frontend unit, list with 87 filtered rows | Render list page | 「CSV出力」 button; hint 「検索した絞り込み条件の87件を出力します。」; button `aria-describedby` → hint; no filters → 「すべての製造指示124件を出力します。」 | high |
| TC-459 | REQ-089 | Frontend unit | Loading, empty, no-match states | No 「CSV出力」 button | high |
| TC-460 | REQ-085 | Frontend unit, view with filters, sort, page 3, pageSize 50 | Click 「CSV出力」 | Fetch URL `/api/production-orders/export?…` with filters and sort, without `page`/`pageSize` | high |
| TC-461 | REQ-089 | Frontend unit, pending request | Click; inspect; click again | 「出力中…」, `aria-disabled="true"`, `aria-busy="true"`, no `disabled` attribute, focus still on the button; second click sends no request | high |
| TC-462 | REQ-085, REQ-089 | Frontend unit, 200 with headers | Click | `saveFile` called with blob and the decoded `filename*` name; status text 「CSVファイルを出力しました（87件）。」 from `X-Total-Count` | high |
| TC-463 | REQ-088 | Frontend unit, `total` 10,001 | Click | No request; alert MSG-E024 | high |
| TC-464 | REQ-088, REQ-089 | Frontend unit | Responses 403, 422 `MSG-E024`, 400 with `errors`, 500, network error, missing `X-Total-Count` | MSG-E020, MSG-E024, list messages, MSG-E025, MSG-E025, MSG-E025 as alerts; button usable again; 401 calls the unauthorized handler | high |
| TC-465 | REQ-089 | Frontend unit, success message shown | Search with new filters | Message cleared; hint shows new total; in-flight export aborted | medium |
| TC-466 | REQ-087 | Frontend unit | File-name parsing: `filename*` only, `filename` only, none, name with `/` and control chars | Decoded / plain / fallback `製造指示一覧_YYYYMMDD-HHmm.csv` / sanitised | medium |
| TC-467 | REQ-085, REQ-087, REQ-089 | E2E PC 1280 px, demo data | Filter (status), click 「CSV出力」 | Download event; name matches `製造指示一覧_\d{8}-\d{4}\.csv`; BOM; header row; data rows = summary total; success message; axe clean | high |
| TC-468 | REQ-089 | E2E SP 390 px | Click full-width 「CSV出力」 | Download as TC-467; button ≥ 44 px high; no horizontal page scroll | high |
| TC-469 | REQ-087 | Manual, exported demo file | Open in Excel or LibreOffice | Japanese text, dates, decimals and the multi-line note display correctly; `'=` shown as text | medium |

Performance (plan step 8, not a pass/fail TC): integration measurement of a 10,000-row export, target ≤ 5 s.

## Environmental needs

Local Windows machine: .NET 10 SDK, Node, Docker Desktop (Testcontainers and the Compose stack), Chromium for
Playwright. Integration tests create their own PostgreSQL container per fixture and seed their own rows. E2E runs
against the local Compose stack with the 124-order demo data; it only reads data.

## Commands and prerequisites

As verified in `ai/project.md`: `dotnet build src/backend/ProductionManagementAI.slnx`;
`dotnet test src/backend/ProductionManagementAI.slnx`; in `src/frontend`: `npm run lint`, `npm run build`,
`npm test`; Compose rebuild `docker compose -f deploy/compose.yaml up -d --build backend frontend`; in `tests/e2e`:
`E2E_ADMIN_PASSWORD=… E2E_BASE_URL=http://localhost:3000 npx playwright test`.

## Results

Executed 2026-10-07 on branch `feature/WI-016-order-csv-export` (local, not committed), Windows 11, Docker Desktop.

| Test ID | Result | Evidence |
| --- | --- | --- |
| TC-440–TC-447 | Passed | `ProductionOrderCsvWriterTests` (backend unit) |
| TC-448 | Passed | `PlantClockPlantTimeTests` (integration project, no database) |
| TC-449, TC-450 | Passed | `ProductionOrderServiceTests` `Export_*` (backend unit) |
| TC-451–TC-455, TC-457 | Passed | `ProductionOrderExportEndpointTests` (integration, Testcontainers PostgreSQL) |
| TC-456 | Passed | `ProductionOrderExportLimitTests`: 10,000 rows → 200, 10,001 → 422 `MSG-E024`, filtered back under → 200 |
| TC-458–TC-466 | Passed | `ExportCsvButton.test.tsx` (23 tests, includes axe) |
| TC-467 | Passed | `csv-export.spec.ts` (2 tests, desktop project, local Compose stack, includes axe) |
| TC-468 | Passed | `csv-export.mobile.spec.ts` (390 px; includes axe) |
| TC-469 | Not run | No Excel or LibreOffice installed on this machine; installing software is outside plan revision 2 |
| Performance | 227 ms for 10,000 rows (target ≤ 5 s) | `ProductionOrderExportLimitTests` output `PERF:` line |

Suite totals after the change: backend unit 286 (was 251), integration 234 (was 215), frontend unit 307 (was 282),
E2E 76 specs run locally: 64 passed, 12 failed — all 12 in the plant-calendar specs, which need the activated
calendar that only the CI fixture provides (a known local limitation since WI-013); no other spec failed.

## Known gaps

- Screen-reader speech and physical mobile keyboard: Not run (see Features not to be tested).
- TC-469 (open in a spreadsheet application): Not run — no Excel or LibreOffice installed locally.
- The 12 plant-calendar E2E specs cannot pass locally without the CI calendar fixture; CI is the reference run.
- CI run 37713513237 (PR #48, head `127c81f`, 2026-10-08): 286 backend unit, 234 integration, 307 frontend unit and all 76 E2E
  specs passed, including the plant-calendar specs and TC-467/TC-468.
