# WI-016 — Evidence

## Plan step 1 — trace table (2026-10-07)

Sources read: [002_BD](../../docs/en/010_basic-design/002/002_BD_製造指示一覧.md), 002_DD-API (API-PO-04),
`src/frontend/src/features/production-orders/` (`ProductionOrderListPage.tsx`, `ProductionOrderTable.tsx`,
`messages.ts`, `types.ts`), `ProductionOrderListContracts.cs`.

| Requirement | Existing basis reused | New in 002_BD-CSV |
| --- | --- | --- |
| REQ-085 all pages, current filters/sort | 002_BD 0-3 parameters, V-09–V-13, sort tie-breaker; list query (FN-010) | FN-041, FN-043, item 28, E-30 |
| REQ-086 columns/formats | 002_BD M-05 status labels, M-08 overdue, M-10 date formats; list row (order, product SKU/name/unit, quantity, due, status, overdue, updated, line) | Columns 1–13, M-11–M-15; notes/created/completed added (DEC-003); plant-time timestamps (DEC-008) |
| REQ-087 Excel-friendly file | — | FN-042, file format section, M-16 filename (DEC-004) |
| REQ-088 security/limit | FN-011, FN-016, MSG-E020, list security posture | V-14 (10,000, DEC-005), V-15, formula neutralising, `no-store` |
| REQ-089 feedback/a11y | List live-region and error conventions | Item 29, E-30–E-33, MSG-I009, MSG-E024, MSG-E025 |

Next free IDs at baseline `a470146`: MSG-E024, MSG-I009, FN-041, UC-022, API-PO-05 (checked with `grep` over
`src/` and `docs/`).

## Plan step 2 — 002_BD-CSV (2026-10-07)

- Written: `docs/en/010_basic-design/002/002_BD-CSV_製造指示一覧.md` version 1, wireframes
  `wireframes/002_BD-CSV_SCR-002-pc.svg` and `-sp.svg`.
- Wireframes rendered with headless Chrome at 960 and 390 px and inspected: every callout (1, 7–13, 14, 15, 16, 22,
  23, 28, 29) is in the legend; Japanese text renders; no overflow on SP. Render PNGs kept in the scratchpad only.
- Design-consistency checklist (BD items): requirements mapped; navigation/primary action/exceptions covered;
  Mermaid transition matches the screen-list row for the new action; security fields identified (notes in the
  export, formula injection); WCAG 2.2 AA needs captured; no migration; logging named (exact names in DD).
  PDF item: deferred until approval (user rule, see plan step 6).
- Approved 002 documents unchanged (`git status` shows only new files under `docs/en/010_basic-design/002/`).

## Plan step 3 — BD review round 1 (2026-10-07)

- Feedback "export CSV by filter" clarified with a question; answer "Keep applied filters" (DEC-009).
- 002_BD-CSV version 2: item 30 export hint (§1 legend, §3, M-18, E-33, accessibility). Wireframes regenerated
  and re-rendered at 960/390 px; callouts 28, 29, 30 legible, SP footer overlap found and fixed before handover.

## Plan step 6 — 002_BD-CSV PDFs (2026-10-07)

- BD version 2 approved (DEC-010). `pip install markdown` (documented prerequisite) was needed first.
- `docs/en/pdf/010_basic-design/002/002_BD-CSV_製造指示一覧.pdf` and `docs/ja/pdf/…` rendered with
  `scripts/docs-pdf.py`, source note "002_BD-CSV version 2 (2026-10-07)"; 14 pages each. Pages spot-checked with
  PyMuPDF renders (scratchpad only): Mermaid transition drawn, SVG wireframes embedded, Japanese text and footer correct.
  The Japanese translation stays in the scratchpad (not committed). The first EN run printed a `UnicodeEncodeError`
  only when echoing the output path to the console; the PDF was complete. Later runs use `PYTHONIOENCODING=utf-8`.

## Plan step 4 (2a) — 002_DD-CSV (2026-10-07)

- Plan/skill conflict (one DD file vs. four) raised; user chose four files, one review each (DEC-011).
- Written: `docs/en/020_detailed-design/002/002_DD-CSV_製造指示一覧.md` version 1; wireframes
  `wireframes/002_DD-CSV_SCR-002-pc.svg` and `-states.svg`; mockups `mockups/002_DD-CSV_SCR-002.html` and `.ja.html`.
- Wireframes rendered at 960 px and inspected; badge/label overlaps and an SP card overflow found and fixed.
- Code names checked against the source: `toSearchParams` and `hasFilters` (listViewState.ts), `apiClient`
  (`ApiError`, `getJson`), `formatNumber`, `ErrorIcon`/`iconProps`, policy `ProductionOrderEditor`,
  `ProductionOrderListQuery.TryCreate`. A first draft named a non-existent `toQueryString`; corrected.

## 002_DD-CSV PDFs and 002_DD-API-CSV (2026-10-07)

- 002_DD-CSV v1 approved (DEC-012). EN/JA PDFs rendered, 16 pages each, source note "002_DD-CSV version 1
  (2026-10-07)"; state diagram page checked in the Japanese PDF (Mermaid drawn with Japanese state names).
- Written: `002_DD-API-CSV_製造指示一覧.md` v1 and `mockups/002_DD-API-CSV_sample.csv` (DEC-013).
- Cited column names checked against `ProductionManagementAI.Infrastructure/Migrations/*ModelSnapshot.cs`:
  `order_number`, `quantity`, `due_date`, `status`, `notes`, `line_id`, `created_at_utc`, `updated_at_utc`,
  `completed_at_utc`; `production_lines.code`/`is_active`. Problem Details shape checked against
  `ProductionOrderProblems.cs` (`RuleViolation` → 422 `urn:pmai:problem:rule-violation`, `code`, `traceId`).
- Sample CSV checked with Python's `csv` module: BOM `EF BB BF`; 6 records (header + 5) × 13 fields; CRLF record
  ends; one LF inside the quoted multi-line note; `"` doubled; leading `=` neutralised as `'=`.

## 002_DD-API-CSV PDFs and 002_DD-FN-CSV (2026-10-07)

- 002_DD-API-CSV v1 approved (DEC-014); EN/JA PDFs rendered, 6 pages each, footer "version 1" checked.
- Written: `002_DD-FN-CSV_製造指示一覧.md` v1 (DEC-015). Checked against the source: `ProductionOrderService.ListAsync`
  and `SetQueryTags`; `ProductionOrderRepository.CountOrdersAsync`/`ListOrdersAsync` (line subquery, `ApplyFilters`,
  `ApplySort`); `IProductionOrderTransaction` (`CommitAsync` + `IAsyncDisposable`); `IPlantClock`/`PlantClock`
  (no local-time method yet, hence `ToPlantTime`); telemetry names and outcomes; log EventIds in use (2001–2004,
  3001, 3101 → 2005 is free); test doubles `TokyoClock.cs` and `ProductionOrderServiceTests.cs`; `pmai_app` grants
  (`SELECT` on `production_orders`, `production_lines`; products already read by the list).

## 002_DD-FN-CSV PDFs and 002_DD-SPD-CSV (2026-10-07)

- 002_DD-FN-CSV v1 approved (DEC-016); EN/JA PDFs rendered, 8 pages each, footer "version 1" checked.
- Written: `002_DD-SPD-CSV_製造指示一覧.md` v1 (P-16–P-19 + request boundary). Checked against the source:
  `lib/apiClient.ts` (`request`, `readProblem`, unauthorized handler — no raw-response helper yet, hence `getFile`),
  `listViewState.toSearchParams` (emits `page`/`pageSize` only when non-default, deleted for the export),
  `ProductionOrderListPage` (`viewKey`, `result === null` while loading, 400 `errors` flattening), 002_DD-SPD process
  IDs P-10–P-15 (→ P-16 onward).
- Found while writing: approved 002_DD-CSV says `disabled` for the busy button, which can drop keyboard focus;
  proposed `aria-disabled` instead (DEC-017, awaiting the user).

## Design close (2026-10-07)

- 002_DD-SPD-CSV v1 approved and DEC-017 accepted (DEC-018). SPD PDFs rendered (EN 7 pages, JA 8 pages).
- 002_DD-CSV v2: busy button `aria-disabled` (screen item definition, state table, test viewpoint); states wireframe
  caption and mockup markup (`aria-disabled="true" aria-busy="true"`) updated; EN/JA PDFs re-rendered (16 pages each,
  footer "version 2", text contains `aria-disabled`).
- No application code changed in revision 1 (`git status`: only new docs and WI-016 files).

## Plan revision 2 — implementation and tests (2026-10-07)

Approval: DEC-019. Local only; nothing committed or pushed.

**Code** (15 modified files, 408 insertions / 12 deletions, plus new files):
- Backend: `ProductionOrderExport.cs` (export row, `ProductionOrderExport`, `ProductionOrderCsvWriter`,
  `ProductionOrderExportLog`), `CsvExportResult.cs`, `ProductionOrdersController.Export`, `ProductionOrderService.ExportAsync`,
  repository `BeginReadSnapshotAsync`/`StreamExportRowsAsync`, `IPlantClock.ToPlantTime`, shared
  `ProductionOrderListMapper.IsOverdue`, `Msg.ExportLimitExceeded`, two telemetry instruments + `Outcomes.Cancelled`.
- Frontend: `ExportCsvButton.tsx`, `lib/download.ts`, `apiClient.getFile`, `api.exportOrdersCsv`/`exportFileName`,
  catalog entries (MSG-I009, MSG-E024, MSG-E025, `labels.list.export*`, `exportHint`), `ExportIcon`, list-page row.

**Checks** (commands from `ai/project.md`):

| Check | Result |
| --- | --- |
| `dotnet build src/backend/ProductionManagementAI.slnx` | Passed, 0 warnings |
| `dotnet test src/backend/ProductionManagementAI.slnx` | Passed: 286 unit, 234 integration |
| `npm run lint` / `npm run build` / `npm test` (src/frontend) | Passed / passed / 307 passed (23 files) |
| `docker compose -f deploy/compose.yaml up -d --build --no-deps backend frontend` | Rebuilt and restarted; `db` untouched (Up 29 h, healthy) |
| `npx playwright test csv-export` (tests/e2e) | 3 passed (desktop 2, mobile 1) |
| `npx playwright test` (full) | 64 passed, 12 failed — all plant-calendar specs (known local limitation, CI fixture only) |
| Performance, 10,000 rows (Testcontainers) | 227 ms (target 5 s) |
| Manual open in a spreadsheet app (TC-469) | Not run — none installed |
| Secret scan of the diff and new files | No secret found; `deploy/.env` values were passed via environment only and never printed |

**Defects found and fixed during the work** (tests caught them before reporting):
- A unit test's long-text data was shorter than intended (test bug).
- An integration test assumed exactly 124 orders although another test in the class adds one (test isolation bug).
- Shell escaping put a NUL byte into `api.ts`'s file-name sanitising regex; caught by inspection, rewritten and checked
  for control bytes.
- Playwright forbids importing a spec from a spec; shared helpers moved to `specs/csvExport.ts`.
- Two `bash.exe.stackdump` files from Git Bash crashes (repo root earlier, `src/backend/` now) were deleted.
- Working-tree line endings normalised back to CRLF for the edited files (index stays LF via `core.autocrlf`).

**Design-consistency, security-review and delivery checklists:** see review section below.

## Plan revision 2 — review (2026-10-07)

- **Design consistency:** code follows 002_BD-CSV v2 and 002_DD-CSV v2 / -API / -FN / -SPD v1. Two behaviour-neutral
  deviations from 002_DD-FN-CSV (DEC-020): the over-limit log entry uses outcome `rule_violation` (the metric's name)
  instead of `limit`; the logger, user ID and timer are held by `ProductionOrderExport` (which `CsvExportResult`
  completes) rather than passed to `CsvExportResult`. No API, screen or data behaviour differs.
- **Security review:** endpoint inherits the class-level `ProductionOrderEditor` policy (401/403 tested); all query input
  goes through the list validator and bound parameters (400 tested); `Content-Disposition` built with
  `ContentDispositionHeaderValue`; formula neutralising on every text column (unit + integration tested); `no-store`
  tested; Problem Details carry only `code`/`traceId`; logs carry user ID, outcome, row count and duration only — no
  order contents, notes or filter values; READ ONLY transaction; no new dependency, role or permission; no secret in the
  diff. Threat surface unchanged from the list (same users can already read every order).
- **Delivery:** all approved design documents have EN/JA PDFs; required checks recorded above, with Not-run reasons;
  no commit, push, PR, merge or deploy performed (outside revision 2).

## Plan revision 3 — pre-commit checks (2026-10-08)

Approval: DEC-021.

| Check | Result |
| --- | --- |
| Stray files | `bash.exe.stackdump` (Git Bash crash dump, repository root, 2026-10-07 18:05) deleted |
| File list vs. plan revision 3 | 53 files (22 design, 10 backend, 7 frontend, 8 tests, 6 records), matching the plan exactly; plus `.gitattributes` (below) |
| Secret scan (secret-like `deploy/.env` keys and token/key patterns, values never printed) | No secret. One false positive: the 6-character local database password matches a substring of the over-long order-number digit run in `ProductionOrderExportEndpointTests` (a 400 validation case); the value is not stored on `master` and is local-only |
| Approved WI-003 002 design files | Unchanged (no diff against `master`); all WI-016 design files are new |
| `git diff --check` | No whitespace errors |
| Japanese UI-text scan (`NoInlineText.test.ts`, TC-301) | 65 passed |
| `master` moved? | No — still `bcff62e` (fetched 2026-10-08) |
| Line endings of the sample CSV | `mockups/002_DD-API-CSV_sample.csv` has CRLF record ends and one LF inside a quoted note (as 002_DD-API-CSV documents); with `core.autocrlf=true` git would have rewritten them. Added `*.csv -text` to `.gitattributes` so CSV files are stored byte for byte; no tracked CSV existed before. Not a behaviour change |

## Plan revision 3 — commits, PR and CI (2026-10-08)

| Item | Result |
| --- | --- |
| Commits | `5f0380d` docs(WI-016): add approved CSV export design for SCR-002 (23 files); `127c81f` feat(WI-016): export production orders as CSV from the order list (31 files) |
| Push | `feature/WI-016-order-csv-export` → origin; `master` unchanged at `bcff62e`, no rebase needed |
| PR | #48 https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/48 → `master`; MERGEABLE, CLEAN |
| CI | CI run 37713513237 on `127c81f` (01:33–01:39 UTC): Backend (build, test) success — 286 unit, 234 integration; Frontend (build, lint, test) success — 307 tests, 23 files; E2E (Compose stack + Playwright) success — 76 passed, including the plant-calendar specs and the new CSV specs |
| CI fixes | None needed |
| Not done (outside revision 3) | Merge, branch deletion, closeout documentation, deployment |

## Plan revision 4 — merge, cleanup and closeout (2026-10-08)

Approval: DEC-022.

| Item | Result |
| --- | --- |
| Pre-merge check | PR #48 head `403befd`, run 37714084761 passed, MERGEABLE/CLEAN, no reviews or comments; `origin/master` `bcff62e` |
| Merge | Squash-merged with `--match-head-commit 403befd`, as `8649f2d` `feat(WI-016): export production orders as CSV from the order list (#48)` |
| Cleanup | Remote and local `feature/WI-016-order-csv-export` deleted; refs pruned; local `master` at `8649f2d`. Another `bash.exe.stackdump` (Git Bash crash dump) deleted from the repository root |
| `master` CI | `master` run 37715472260 on `8649f2d` passed |
| Closeout | Branch `chore/WI-016-closeout`: README, `ai/project.md`, CLAUDE.md, WI-016 records; stale WI-015 status line corrected (PR #46 merged as `a470146`) |
| Local Compose stack | Not rebuilt: its `backend`/`frontend` images were built from the merged code (revision 2); `db` and demo data untouched |
| Not done | Merging the closeout PR (needs approval); deployment |
