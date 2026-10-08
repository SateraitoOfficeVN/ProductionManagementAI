# WI-016 — Production order CSV export — Implementation Plan

Revisions are kept in full and in chronological order (oldest first), so the plan can be back-tracked. The last
revision is the current one.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-07 | Design (BD addendum, then DD addendum with wireframes/mockups); documents only | Complete — 002_BD-CSV v2, 002_DD-CSV v2, -API/-FN/-SPD-CSV v1 approved with EN/JA PDFs | User: "approved, go with all recommendations" (2026-10-07, DEC-006) |
| 2 | 2026-10-07 | Implementation and tests (local only, no commit) | Complete — results in evidence.md and test-plan.md | User: "approved" (2026-10-07, DEC-019) |
| 3 | 2026-10-08 | Commit, push and open a PR to `master`; record CI | **current** — approved; in progress | User: "approved, go ahead" (2026-10-08, DEC-021) |

## Revision 1 — Design (documents only)

Revision 1, 2026-10-07.

### Objective

Design the 「CSV出力」 action of SCR-002 so that REQ-085–REQ-089 in [brief.md](brief.md) are fully specified and
reviewable before any code is written. Implementation, tests and PR are a later revision, drafted only after the
design is approved.

### Proposed design direction (for review; detailed in steps 2 and 4)

- **Where:** a secondary (outlined) button 「CSV出力」 with a download icon next to the result summary / 「表示件数」
  on SCR-002 (PC and SP). It is hidden on the empty state, disabled while a list query or an export is running,
  and shows 「出力中…」 while downloading.
- **What:** it exports the current *applied* filters and sort (the URL state, 002_BD 0-3) — not unsaved edits in the
  filter panel. `page`/`pageSize` are ignored: all matching rows are exported (Q1).
- **How (server-side generation):** new endpoint `GET /api/production-orders/export` with the same query parameters
  and validation as the list API (002_DD-API, FN-011), streaming `text/csv` with
  `Content-Disposition: attachment; filename="production-orders_YYYYMMDD-HHmm.csv"; filename*=UTF-8''製造指示一覧_YYYYMMDD-HHmm.csv`.
  Server-side keeps one source of truth for filtering, sorting and formatting, avoids N page requests from the
  browser, and works the same on SP. The frontend fetches it as a blob (so 401/400/409 errors can be shown in the
  page) and saves it via an object URL.
- **Formatting:** RFC 4180 (double quotes around fields containing `,` `"` CR/LF, doubled quotes), CRLF line ends,
  UTF-8 with BOM (Q3). Status as Japanese label, dates `YYYY/MM/DD`, timestamps `YYYY/MM/DD HH:mm` in plant time,
  quantity as the exact decimal string (no thousands separator), overdue as 「納期遅れ」 or empty.
- **Security:** same `Admin`/`Operator` policy as the list; text cells starting with `=`, `+`, `-`, `@`, tab or CR
  get a leading `'` (OWASP CSV-injection guidance); `Cache-Control: no-store`; row limit (Q4) returns 409/422 with
  a Problem Details `code` that the UI maps to 「出力件数が上限（10,000件）を超えています。条件を絞り込んでください。」.
- **Messages (new IDs after the existing catalog):** nothing to export (button disabled when total = 0, so no
  message needed), export failed (generic with retry), too many rows, session expired (existing 401 handling).
- **No DB change:** reuses the list query, sort-key mapping and indexes of 002_DB; the export just skips paging.
  Verified with the 124-order demo data; the limit bounds the worst case.

### Open decisions (answers go into decisions.md before step 2)

| # | Question | Options | Recommendation |
| --- | --- | --- | --- |
| Q1 | Export scope | (a) all orders matching the applied filters, in the current sort, across all pages; (b) only the rows on the visible page | (a) — users filter first, and a page-sized file is rarely what they want |
| Q2 | Columns | (a) the list's data: 指示番号, 製品コード, 製品名, 生産ライン, 数量, 単位, 納期, ステータス, 納期遅れ, 更新日時; (b) (a) plus 備考, 作成日時, 完了日時 | (b) — the export is for offline use, where notes and dates matter; it needs only an extra projection, no schema change |
| Q3 | Encoding | (a) UTF-8 with BOM (Excel 2016+ opens it directly; lossless for every character); (b) Shift_JIS/CP932 (older Excel/legacy tools; some characters cannot be represented) | (a) |
| Q4 | Row limit | (a) 10,000 rows, above that an error asking to narrow filters; (b) no limit; (c) another number | (a) — bounded memory/time, far above the demo's 124 orders |

### Design files, in review order (RFC 0012: one file per review stop)

Approved WI-003 002 documents stay unedited; the new documents are additive (WI-012/WI-015 pattern). No DB design
document is produced because nothing in the schema changes; the BD states this and links 002_DB.

1. `docs/en/010_basic-design/002/002_BD-CSV_製造指示一覧.md` (name per DEC-007) + wireframe
   `wireframes/002_BD-CSV_SCR-002-pc.svg` and `-sp.svg` (where the button sits; flow; function list FN-030–;
   message list; business rules; security).
2. `docs/en/020_detailed-design/002/002_DD-CSV_製造指示一覧.md` (name per DEC-007) — one DD for this small feature covering API
   (endpoint, parameters, headers, status codes, Problem Details codes), processing (query reuse, streaming, escaping,
   formatting, limit), screen behaviour (states, focus, live-region announcements, error mapping) and the column
   spec; + SVG wireframes `wireframes/002_DD-CSV_SCR-002-{pc,sp}.svg` and HTML state mockups
   `mockups/002_DD-CSV_SCR-002.html` / `.ja.html`, plus a sample CSV `mockups/002_DD-CSV_sample.csv`.

### Steps

| # | Step | Depends on | Skill | Deliverable | Verification / stop | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Record Q1–Q4 answers and this revision's approval in decisions.md; reconcile 002_BD/DD/DD-API/DD-FN/DD-SPD/002_DB and the running list code into a trace table (filters, sort mapping, formats, messages) | approval | requirements, basic-design | WI-016 decisions/status | Every REQ-085–089 mapped | done 2026-10-07 — trace table in evidence.md; DEC-002–DEC-008 recorded |
| 2 | Write 002_BD-CSV + its PC/SP SVG wireframes | 1 | basic-design, screen-design | file 1 above | design-consistency checklist; render SVGs in Chromium | done 2026-10-07 — 002_BD-CSV v1 + PC/SP SVGs, rendered and inspected |
| 3 | **Stop:** present 002_BD-CSV for review; resolve feedback | 2 | — | — | Explicit "continue" from the user | done 2026-10-07 — v2 approved (DEC-009, DEC-010) |
| 4 | Write 002_DD-CSV + wireframes, EN/JA HTML state mockups and the sample CSV (split into 2a–2d per the amendment below) | 3 | detailed-design, screen-design, security-review | file 2 above | design-consistency + security-review checklists; mockups inspected at 1280/390 px | |
| 5 | **Stop:** present 002_DD-CSV for review; resolve feedback | 4 | — | — | Explicit approval | done 2026-10-07 — four DD files approved one at a time (DEC-012, DEC-014, DEC-016, DEC-018); DEC-017 accepted, 002_DD-CSV v2 |
| 6 | After each design `.md` is approved: render EN/JA PDFs via `scripts/docs-pdf.py` (user rule: PDFs only after approval) | 3, 5 | — | `docs/en/pdf/...`, `docs/ja/pdf/...` | Pages spot-checked | done 2026-10-07 — EN/JA PDFs of BD v2, DD v2, DD-API v1, DD-FN v1, DD-SPD v1 (evidence.md) |
| 7 | Update status/evidence; draft plan revision 2 (implementation + tests + `test-plan.md`) and present it | 6 | planning | plan.md revision 2 | Stop for approval | done 2026-10-07 — revision 2 below |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author, designer | this agent |
| Reviewer / approver | ThongTM |

### Resources and external actions

| Action | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local branch `feature/WI-016-order-csv-export`, design files and WI-016 records | yes, on approval of this revision | — | docs and work-items only |
| Application/test code, DB writes | no | needs revision 2 | — |
| Commit, push, PR, merge, deploy | no | not yet authorized | — |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| Large exports are slow or memory-heavy | Result set near the limit | Stream rows with `AsAsyncEnumerable`; row limit (Q4); measured in integration test |
| CSV formula injection via product names or notes | Cell text starts with `=`/`+`/`-`/`@`/tab/CR | Prefix `'`; unit tests; security-review checklist |
| Negative numbers wrongly escaped by the injection rule | Numeric cells | Apply the prefix only to text columns; quantities are always positive |
| Excel shows mojibake | Missing BOM / wrong encoding | BOM test; manual open check recorded in evidence |
| Export and screen disagree (filters/sort) | Separate code paths | Share the list's query builder and validator; integration test compares export rows with paged list rows |
| An answer to Q1–Q4 changes scope (e.g. Shift_JIS or .xlsx) | User choice | Revise this plan before step 2 |

### Amendment (DEC-011, 2026-10-07)

The detailed-design skill requires four DD files. Design file 2 above becomes four files, written and reviewed in this
order, one review stop each, PDFs after each approval:

2a. `002_DD-CSV_製造指示一覧.md` — main DD: screen items, states and transitions, screen modules; SVG wireframes
    `wireframes/002_DD-CSV_SCR-002-{pc,sp,states}.svg`; HTML state mockups `mockups/002_DD-CSV_SCR-002.html` and
    `.ja.html`.
2b. `002_DD-API-CSV_製造指示一覧.md` — API-PO-05: parameters, headers, status codes, Problem Details, sample CSV
    `mockups/002_DD-API-CSV_sample.csv`.
2c. `002_DD-FN-CSV_製造指示一覧.md` — backend functions: query reuse, streaming, CSV writer, limit, logging.
2d. `002_DD-SPD-CSV_製造指示一覧.md` — screen processing: click → fetch → save, error mapping, focus and announcements.

Steps 4–5 repeat per file (write, then stop for review); step 6 renders each file's PDFs after its approval.
Source: user answer "4 files, one at a time" to the plan/skill conflict question.

### Approval / sign-off

- **Review status:** approved
- **Approval source:** User: "approved, go with all recommendations" (2026-10-07), recorded as DEC-006; Q1–Q4 answered as recommended (DEC-002–DEC-005)
- **Approved revision:** 1 (2026-10-07)
- **Closure:** 2026-10-07 — complete; all design files approved with PDFs; no application code changed. Superseded by revision 2 (implementation).

---

## Revision 2 — Implementation and tests (local only)

Revision 2, 2026-10-07. Follows the completed revision 1 (design).

### Objective

Implement the approved design — [002_BD-CSV](../../docs/en/010_basic-design/002/002_BD-CSV_製造指示一覧.md) v2,
[002_DD-CSV](../../docs/en/020_detailed-design/002/002_DD-CSV_製造指示一覧.md) v2 and its API/FN/SPD companions v1 —
with unit, integration and E2E tests and a `test-plan.md`, verified locally. REQ-085–REQ-089.

### Scope

#### In scope

- Backend: `Msg.ExportLimitExceeded`; `IPlantClock.ToPlantTime` (+ the two test doubles); shared `IsOverdue`;
  `ProductionOrderExportRow`; repository `BeginReadSnapshotAsync` / `StreamExportRowsAsync`;
  `ProductionOrderService.ExportAsync`; telemetry (counter, histogram, activity, log 2005); `ProductionOrderCsvWriter`;
  `CsvExportResult`; `ProductionOrdersController.Export` (API-PO-05).
- Frontend: catalog entries (labels + MSG-I009/E024/E025); `ExportIcon`; `apiClient.getFile`; `exportOrdersCsv`;
  `lib/download.ts`; `ExportCsvButton`; the `ProductionOrderListPage` change.
- Tests and `work-items/WI-016/test-plan.md` (TC-440 onward, mapped to REQ-085–089; includes long-text, decimal,
  formula-injection, multi-line-note and 10,001-row cases).
- WI-016 status/evidence; design-consistency, security-review and delivery checklists.

#### Out of scope

- Commit, push, PR, merge, deployment (a later revision, as in WI-015).
- Database schema or migration changes (none designed); edits to approved design files unless a defect is found
  (then stop and ask).
- Other screens' exports; `.xlsx`; README / `ai/project.md` / `CLAUDE.md` closeout (after merge).

### Inputs and assumptions

| Input | Revision | Assumption |
| --- | --- | --- |
| 002_BD-CSV | v2 | — |
| 002_DD-CSV / -API-CSV / -FN-CSV / -SPD-CSV | v2 / v1 / v1 / v1 | — |
| Local Compose stack (`deploy-db-1`, `deploy-backend-1`, `deploy-frontend-1`) | running | E2E needs the `backend` and `frontend` images rebuilt with the new code; the `db` volume and demo data are kept; no migration runs |
| `deploy/.env` | local | Credentials are read by the commands only, never printed or recorded |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Write `test-plan.md` (TC IDs → REQ/BD/DD, levels, data, expected results) | none | testing | `work-items/WI-016/test-plan.md` | Every REQ-085–089 and E-30–E-33 has at least one TC | done 2026-10-07 — TC-440–TC-469 |
| 2 | Backend implementation (scope list above) | 1 | implementation | `src/backend/**` | `dotnet build src/backend/ProductionManagementAI.slnx` | done — build passed, 0 warnings |
| 3 | Backend unit tests: CSV writer (BOM, CRLF, quoting, injection prefix only on text, quantity/date/time formats, retired/unassigned line, overdue), `ToPlantTime`, service limit/product/outcomes | 2 | testing | `tests/backend/**` | `dotnet test` | done — 286 unit passed |
| 4 | Integration tests (Testcontainers): 200 headers (`Content-Type`, `Content-Disposition`, `X-Total-Count`, `no-store`); body rows equal the list rows across all pages in the same order for several filter/sort combinations; notes edge cases; 400 / 401 / 403; 422 `MSG-E024` with 10,001 seeded rows; `page`/`pageSize` ignored | 2 | testing | `tests/integration/**` | `dotnet test src/backend/ProductionManagementAI.slnx` | done — 234 integration passed |
| 5 | Frontend implementation (scope list above) | 2 | implementation | `src/frontend/src/**` | `npm run lint`, `npm run build` | done — both passed |
| 6 | Frontend unit tests (Vitest + RTL + vitest-axe): hint variants, hidden states, request URL without page/pageSize, success announcement with the header count, `aria-disabled`/`aria-busy` with focus kept, double activation, client limit, 403/422/400/500/network mapping, reset on view change, `getFile` 401 handling, file-name parsing and sanitising | 5 | testing | `src/frontend/tests/unit/production-orders/**` | `npm test` | done — 307 passed |
| 7 | Rebuild the local stack's `backend`/`frontend`; E2E (Playwright): PC and SP download, file name, BOM, header row, row count equals the summary total, filtered export, axe on the list with the new controls; existing Screen B specs still pass | 4, 6 | testing | `tests/e2e/specs/csv-export.spec.ts` | Verified E2E command in `ai/project.md` | done — 3 new passed; full run 64 passed, 12 plant-calendar failed (known local limitation) |
| 8 | Performance: export of 10,000 rows on a disposable database (Testcontainers), time recorded against the 5 s target | 4 | testing | evidence.md | Measured value | done — 227 ms for 10,000 rows |
| 9 | Open an exported file in a spreadsheet app if one is installed (Excel or LibreOffice); otherwise record Not run | 7 | testing | evidence.md | Result recorded; screenshots outside git | Not run — no spreadsheet application installed |
| 10 | Review: diff self-review; design-consistency, security-review and delivery checklists; update status, evidence and test-plan results; present results | 3–9 | pr-review, security-review | WI-016 records | Checklists passed or gaps recorded | done — evidence.md review section; DEC-020 |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Implementer, tester | this agent |
| Reviewer / approver | ThongTM |

### Resources and external actions

| Action | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Edit application and test code; WI-016 records | on approval of this revision | — | files in Scope |
| Local builds and tests (`dotnet`, `npm`, Testcontainers, Playwright) | on approval | — | local machine |
| Rebuild/restart the local Compose `backend` and `frontend` containers | on approval | — | `deploy/compose.yaml` services `backend`, `frontend`; `db` and its volume untouched; no `down -v` |
| Install npm/NuGet packages | no | — | none planned; stop and ask if one becomes necessary |
| Commit, push, PR, merge, deploy | no | not yet authorized | later revision |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| A design defect found during implementation | Code cannot follow an approved DD | Stop and ask; amend the design file (new version + PDFs) only after approval |
| Streaming buffered or truncated by Kestrel/Nginx | Integration or E2E shows it | Record; adjust within the design (e.g. response buffering off); ask if the contract would change |
| Local Plant calendar E2E specs fail without an activated calendar (known since WI-013) | Full E2E run | Record as pre-existing; the CI fixture covers them |
| Rebuilding the demo containers disturbs a running demo | Step 7 | Only `backend`/`frontend` are rebuilt; data kept; rebuilding from `master` restores the previous images |
| Secrets exposure | Reading `deploy/.env` | Values passed via environment only; never echoed; nothing recorded in the repository |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** User: "approved" after revision 2 was shown (2026-10-07), DEC-019
- **Approved revision:** 2 (2026-10-07)
- **Closure:** 2026-10-07 — steps 1–10 done (TC-469 Not run: no spreadsheet application). Results in evidence.md and test-plan.md. Commit/push/PR need a plan revision 3.

---

## Revision 3 — Commit and PR

Revision 3, 2026-10-08. Follows the completed revision 2 (implementation and tests).

### Objective and scope

Publish the finished revision 1–2 work as one PR to `master` and get CI green. No code or design change beyond fixing
what the pre-commit checks or CI find (each fix is reported; a fix that changes behaviour or an approved design stops
for a new revision).

### What goes in the PR

| Group | Files |
| --- | --- |
| Design (approved: BD v2, DD v2, DD-API/FN/SPD v1) | `docs/en/010_basic-design/002/002_BD-CSV_製造指示一覧.md` + `wireframes/002_BD-CSV_SCR-002-{pc,sp}.svg`; `docs/en/020_detailed-design/002/002_DD-CSV_製造指示一覧.md`, `002_DD-API-CSV_…`, `002_DD-FN-CSV_…`, `002_DD-SPD-CSV_…`; `wireframes/002_DD-CSV_SCR-002-{pc,states}.svg`; `mockups/002_DD-CSV_SCR-002.html` and `.ja.html`; `mockups/002_DD-API-CSV_sample.csv`; the 10 English and Japanese PDFs under `docs/en/pdf/` and `docs/ja/pdf/` |
| Backend (API-PO-05) | `ProductionOrdersController.cs`; new `Api/ProductionOrders/CsvExportResult.cs`; `Ports.cs`, `ProductionOrderListContracts.cs`, `ProductionOrderService.cs`, `ProductionOrderTelemetry.cs`, new `ProductionOrderExport.cs`; `ProductionOrderMessages.cs`; `PlantClock.cs`, `ProductionOrderRepository.cs` |
| Frontend (SCR-002 「CSV出力」) | `components/icons.ts`; `lib/apiClient.ts`, new `lib/download.ts`; `production-orders/`: `ProductionOrderListPage.tsx`, `api.ts`, `messages.ts`, new `ExportCsvButton.tsx` |
| Tests | `TokyoClock.cs`, `ProductionOrderServiceTests.cs`, new `ProductionOrderCsvWriterTests.cs`; new `ProductionOrderExportEndpointTests.cs`; new `ExportCsvButton.test.tsx`; new `tests/e2e/specs/csv-export.spec.ts`, `csv-export.mobile.spec.ts`, `csvExport.ts` |
| Records | `work-items/WI-016/`: brief, plan, decisions, status, evidence, test-plan |

Not included: `bash.exe.stackdump` (a Git Bash crash dump at the repository root, deleted in step 1); scratchpad files
(Japanese translation source, wireframe/mockup generator scripts, renders); downloaded test CSVs; build output. The
approved WI-003 002 design files stay unedited.

### Steps

| # | Step | Verification | Outcome |
| --- | --- | --- | --- |
| 1 | Pre-commit checks on `feature/WI-016-order-csv-export`: delete `bash.exe.stackdump`; the file list above matches `git status` exactly; secret scan of the diff and records (no `deploy/.env` value, password or token); approved WI-003 002 files unchanged (`git diff master --stat -- docs/` shows additions only); `git diff --check` for whitespace errors; the Japanese UI-text scan (NoInlineText) passes; `master` has not moved since `bcff62e` (checked 2026-10-08: not moved) | Results in evidence.md | |
| 2 | Two commits, so design and code can be reviewed separately: (a) `docs(WI-016): add approved CSV export design for SCR-002` (design group); (b) `feat(WI-016): export production orders as CSV from the order list` (backend, frontend, tests, records) | `git log` | |
| 3 | `git push -u origin feature/WI-016-order-csv-export`; `gh pr create --base master`. Description: summary of REQ-085–089 / UC-022, key decisions (DEC-002–005, 008, 009, 013, 017, 020), the new endpoint `GET /api/production-orders/export` (additive; existing APIs unchanged), test results and the 227 ms measurement, known local limits (TC-469 Not run; plant-calendar E2E needs CI's fixture; screen-reader speech and physical mobile keyboard not run), and "no deployment" | PR URL | |
| 4 | Watch CI (Backend, Frontend, E2E) read-only. If a job fails: diagnose; fix only test-environment or non-behavioural problems with one more commit on the same branch; otherwise stop and report | CI run ID and result | |
| 5 | Record the PR, commits and CI run in evidence/status/plan; push that record update as one more commit to the PR (documentation only, CI skips it) | evidence.md, status.md | |

### Permitted actions

- Local commits on `feature/WI-016-order-csv-export`, `git push` of that branch, `gh pr create`, PR description
  edits, read-only CI and PR checks.
- Commits that fix CI problems which do not change behaviour or an approved design.

Not permitted without further approval:
- merging the PR, deleting branches, or closeout documentation (`README.md`, `ai/project.md`, `CLAUDE.md` current state);
- deployment or image publication;
- edits to approved designs (WI-003 002 files and the WI-016 CSV design files);
- force-push or history rewrite of a pushed branch;
- installing packages.

### Risks / stop conditions

| Risk / stop condition | Trigger | Response |
| --- | --- | --- |
| CI's E2E runs on a fresh database, not the local demo data | A CSV E2E spec relied on local data | The specs assert against the totals the list reports, not fixed counts; if one still fails, fix the test (not the app) and report it |
| Plant-calendar specs fail in CI | They need CI's fixture and are expected to pass there | Stop and report |
| CI runner slower than the local machine | The 10,000/10,001-row integration test or the performance check times out | Report; only a test-timeout change is allowed without a new revision, and the measured time is recorded as is |
| `master` moves before the push | Another PR merges | Rebase locally before the first push only; after pushing, merge `master` instead |
| Secrets exposure | Secret scan or PR description | Nothing from `deploy/.env` is printed or committed; the scan result is recorded |

### Review and approval

- **Review status:** approved
- **Approval source:** User: "approved, go ahead" after revision 3 was shown (2026-10-08), DEC-021
- **Approved revision:** 3 (2026-10-08)
