# WI-015 — Execution plan

| Revision | Date | Phase | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-06 | Requirement reading and additive SCR-005 redesign (documents only) | Complete — design version 3 approved (DEC-010), PDFs rendered | User answered Q1–Q3 and said "continue" after revision 1 was shown (2026-10-06) |
| 2 | 2026-10-06 | Implement the approved redesign and tests (no commit) | Complete — steps 1–9 done; retired-pair note added on review (DEC-012) | User: "revision 2 only implementation and test, not commit yet. Note Create test cases with long text to check if the UI breaks." (2026-10-06) |
| 3 | 2026-10-07 | Commit, push and open a PR to `master`; record CI | Complete — PR #45, final head `7c22966` CI run 37562084126 passed |
| 4 | 2026-10-07 | Merge, branch cleanup and documentation closeout | Complete merge/cleanup; closeout PR open | User: "merge PR #45 and clean up branch" and "yes, create closeout PR for WI-015" (2026-10-07) | User: "approved, go ahead" (2026-10-07) |

## Revision 1 — Redesign (design documents only)

### Objective and scope

Produce one new additive design document for SCR-005 and its companions, then stop for review (RFC 0012). The
design traces to 005_REQ REQ-064–069 and the confirmed WI-009 rules, fixes the gaps in [brief](brief.md)
BUG-009, and applies the user's preference for simple screens (RFC 0013). Implementation is a later revision,
planned only after the design is approved.

### Proposed design direction (for review; detailed in step 2)

- **List:**
  - Header: heading and 「新規ライン」 (primary).
  - Filter row: 「コード・名称で検索」, 「状態」 (使用中/使用停止/すべて, default 使用中), 検索, クリア.
  - Count 「N 件」 top-left; table on PC, cards on SP.
  - Columns: ラインコード, ライン名, 稼働時間／日, 状態 badge, 操作 (編集, red 使用停止 on active lines).
  - Pager 「前へ 1 / N 次へ」 bottom-right, shown only when there is more than one page (50 per page, API unchanged).
  - Separate notices for "no lines yet" and "no matches"; no-match notice includes a Clear hint.
- **Form (register/edit):**
  - **基本情報** card: ラインコード (grey and 「（変更不可）」 on edit), ライン名, 稼働時間／日 with 「時間」 suffix and a range hint; required marked by a red `*` as on 製品マスタ (WI-013).
  - **生産可能な製品**: a table on PC (製品, 単位, 製造時間（分／単位）, 状態, 操作) and cards on SP.
  - Retired pairs are read-only.
  - A pair marked for retirement shows 「使用停止予定（保存後に反映）」.
  - An unsaved new pair has 「削除」.
  - A stale-unit pair shows a yellow notice 「単位の再確認が必要です。確認済み：個　現在：kg」 and the button 「現在の単位で確認」.
  - 保存 then キャンセル, left-aligned, as on 製品マスタ.
  - Error summary at the top.
  - Programmatic focus on the heading without a visible box.
- **Product add (new, missing from the old mockup):**
  - 「製品を追加」 opens a centred dialog with a search field and a paged list of eligible products (code — name, unit), single choice.
  - 「追加」 adds a row with blank minutes; the page no longer lists every candidate inline.
- **Dialogs:**
  - Line retire: 「このラインを使用停止にしますか？」 plus code/name and 「過去の製造指示は保持されます。」
  - Pair retire: 「この製品を使用停止にしますか？」
  - Discard: 「変更を破棄しますか？」
  - Keyboard focus starts on the safe button and returns to the opener.
- **SP:** single column, menu navbar, stacked full-width filter buttons, cards for lines and pairs, the add dialog full width.

### Open decisions (answers recorded in decisions.md before step 2)

| # | Question | Options | Recommendation |
| --- | --- | --- | --- |
| Q1 | Control height | (a) 48 px as approved SPD §2; (b) the order/product screens' heights (40/42 px), as chosen for 製品マスタ in WI-013 | (b), for consistency across screens |
| Q2 | Unit confirmation for a newly added product | (a) Adding a product in the dialog, which shows its unit and asks 「1 kg あたりの分」, counts as explicit confirmation of that unit; (b) a separate 「現在の単位で確認」 click for every new row as well | (a): one less step; stale saved pairs still need the button. If the user prefers (b), the design shows the button on new rows too |
| Q3 | Wording source | (a) Follow the approved SPD §5 catalog (新規ライン, 生産ライン登録, 生産可能な製品, 製造時間／単位, …); (b) align with 製品マスタ phrasing (e.g. 「生産ラインを登録」) | (a), because it is the approved text |

### Steps

| # | Step | Files | Verification / stop |
| --- | --- | --- | --- |
| 1 | Create branch `docs/WI-015-production-line-redesign`; reconcile REQ-064–069, 005_BD/DD/DD-API/DD-FN/DD-SPD, the old mockup and the running screen into a region/field/action/state trace table. Record hashes of the approved 005 files to show they are unchanged | WI-015 records | Every requirement and rule mapped; open questions answered |
| 2 | Write `docs/en/020_detailed-design/005/005_DD-SPD-REDESIGN_生産ライン・工程.md`. Contents: user journey; region map; field/validation table; state and dialog table; Japanese UI catalog with English glosses; accessibility; API mapping to existing API-PL-01–06 (no change); verification viewpoints | that file | design-consistency checklist |
| 3 | Companions: SVG wireframes `wireframes/005_DD-SPD-REDESIGN_SCR-005-{pc,sp,dialog}.svg`; static HTML state mockups `mockups/005_DD-SPD-REDESIGN_SCR-005.html` (English captions) and `.ja.html`, one artboard per state | listed files | Rendered and inspected in Chromium at PC/SP widths; no PNG screenshots added |
| 4 | Present the design file, wireframes and mockups for review; record the result | WI-015 status/evidence | Stop until the user approves the design or asks for changes |
| 5 | After the design `.md` is approved (DEC-007): render English and Japanese PDFs via `scripts/docs-pdf.py` | `docs/en/pdf/...`, `docs/ja/pdf/...` | Pages spot-checked; footer names the approved version |

### Permitted actions

- Read-only inspection of the code, approved designs and running app.
- A local docs branch, plus the new design file, wireframes, mockups, PDFs and WI-015 records listed above.
- Temporary rendering files in the scratchpad.

Not permitted without a later approved revision:
- application, test or API changes, and database or demo-data writes;
- edits to approved 005 files;
- commit, push, PR, merge or deployment.

### Risks / stop conditions

- If a simpler UI needs an API or business-rule change (for example, product choices beyond what API-PL-06 returns), stop and ask. Exceptions requested by the user during review: the optional page sizes on API-PL-01 (DEC-006), API-PL-02 and API-PL-06 (DEC-008), designed here and implemented only under a later approved revision.
- If Q2(a) is judged to weaken the DEC-009 "explicit reconfirmation" rule, keep (b).
- The design must not drop any approved state (conflict, unknown save, forbidden, loading, read error).

### Review and approval

Review status: approved 2026-10-06 ("continue" after answering Q1–Q3). Steps 1–5 complete: design version 3 approved
(DEC-010), English and Japanese PDFs rendered from version 3.

## Revision 2 — Implementation

### Objective and scope

Make SCR-005 match the approved
[005_DD-SPD-REDESIGN](../../docs/en/020_detailed-design/005/005_DD-SPD-REDESIGN_生産ライン・工程.md) version 3
(REQ-084), including the additive API page sizes (§10.1 and §10.2). Keep every approved WI-009 business rule,
the order-assignment screens (SCR-001/002) and API-PL-07 unchanged.

### Steps

| # | Step | Files | Verification |
| --- | --- | --- | --- |
| 1 | Rename the local branch `docs/WI-015-production-line-redesign` to `fix/WI-015-production-line-redesign`; it has not been pushed. The design files and WI records go in the same PR, as WI-012 did | — | `git status` |
| 2 | Write `test-plan.md` before the code (user rule): TC-420+ mapped to REQ-084, DEC-003, DEC-006, DEC-008, DEC-009 and the regression scope | `work-items/WI-015/test-plan.md` | Reviewed against design §11 |
| 3 | Backend: optional allow-listed page sizes on API-PL-01 (`pageSize`), API-PL-02 (`pairsPageSize`) and API-PL-06 (`pageSize`), values 10/20/50/100, default 50, invalid value 400 `VALIDATION`, response echoes the size; API-PL-07 untouched | `ProductionLinesController.cs`, `LineContracts.cs`, `ProductionLineService.cs`, `ProductionLineRepository.cs`; tests `tests/backend/.../ProductionLines/*`, `tests/integration/.../ProductionLines/ProductionLineEndpointTests.cs` | Unit and integration tests: allowed, default, rejected and repeated values; existing tests unchanged and passing |
| 4 | Frontend, list: badges, red retire, count and 「表示件数」 (URL `pageSize`, default 20, canonicalized, kept by search/Clear/paging/save/retire), pager only when needed, separate empty and no-match notices, scroll box with sticky header, 製品マスタ tokens and heights | `ProductionLineListPage.tsx`, `api.ts`, `messages.ts` (`labels.lines` per design §7) | Unit tests; screenshots against artboards 1–8, 22, 23 |
| 5 | Frontend, form: 基本情報 card (required `*`, hints, read-only code), retired-line notice, product table/cards (20 per page, badges, 削除/取り消す, stale-unit notice with 「現在の単位で確認」), new add-product dialog (search, 20 per page, single choice, DEC-003 confirm on add), 保存 then キャンセル, heading focus without a box, error summary and recovery states, scroll boxes, dialogs per §6 | `ProductionLineFormPage.tsx`, `LineDialog.tsx`, new `AddProductDialog.tsx` in the same folder, `api.ts`, `messages.ts` | Unit tests; screenshots against artboards 9–21, 24 |
| 6 | Keep order assignment unchanged: `OrderLinePicker` and the eligible-lines decoder keep 50 per page | `api.ts` | Existing order and line-picker unit and E2E tests pass |
| 7 | Tests: update existing line unit and E2E specs only where wording or layout changed. Long-text cases (user request): maximum-length line code (50) and name (200), long product SKU/name (50/200) in the product table, cards, add dialog and retire dialogs, and long text with no spaces, on PC, SP 390 and 320 px; assert no horizontal page scroll, controls inside their boxes, header/status/actions not wrapping, dialogs inside the viewport. New E2E: rows per page; 20 per page for pairs and the dialog; add-dialog flow and confirm-on-add; stale-unit confirmation; staged retire and undo; scroll boxes (keyboard reachable, header visible, no horizontal scroll); heights against the order screens; axe; SP | `LinePages.test.tsx`, `lineRules.test.ts`, `production-lines.spec.ts`, `production-lines.mobile.spec.ts` | Each new case fails before its change and passes after it, where practical |
| 8 | Verify: backend build, unit and integration; frontend lint, build and unit; rebuild local Compose `backend` and `frontend`; full E2E; PC and SP screenshots compared with the mockup | — | evidence.md; plant-calendar specs need CI's fixture locally (known) |
| 9 | Records (evidence, status, test-plan results); stop for user review. Commit, push and PR need a later revision | — | evidence.md |

### Permitted actions

- Edits to the files listed above and to WI-015 records.
- Rebuilding the local Compose `backend` and `frontend` images and running tests against the local stack. Existing and new E2E specs create test lines and products in the local demo database, as current specs already do.

Not permitted without further approval:
- commit, push, PR, merge, branch deletion or deployment;
- database schema changes or migrations;
- edits to approved designs (WI-009 files and the approved 005_DD-SPD-REDESIGN version 3);
- changes to SCR-001/002 or API-PL-07.

### Risks / stop conditions

- If the implementation needs a behaviour or API change beyond design version 3, stop and ask (new revision).
- Default API page size stays 50, so other callers are unaffected. A test checks the default.
- Scroll boxes and sticky headers can hide focused rows: verify with keyboard and axe `scrollable-region-focusable`.
- Plant-calendar E2E specs fail locally without CI's calendar fixture (known since WI-012); CI is the proof for those.

### Review and approval

Review status: revision 2 approved 2026-10-06 with the user's changes: implementation and tests only, no commit;
add long-text test cases.

Result: steps 1–9 complete (see [evidence](evidence.md)). On review 2026-10-07 the user asked why a 使用停止 product
cannot be registered again; the rule stays, and the screen now explains it (DEC-012, TC-435), still uncommitted.

## Revision 3 — Commit and PR

### Objective and scope

Publish the finished revision 1–2 work as one PR to `master` and get CI green. No code or design change beyond fixing
what the pre-commit checks or CI find (each fix is reported; a fix that changes behaviour or the approved design stops
for a new revision).

### What goes in the PR

| Group | Files |
| --- | --- |
| Design (approved version 3, DEC-010) | `docs/en/020_detailed-design/005/005_DD-SPD-REDESIGN_生産ライン・工程.md`; `wireframes/005_DD-SPD-REDESIGN_SCR-005-{pc,sp,dialog}.svg`; `mockups/005_DD-SPD-REDESIGN_SCR-005.html` and `.ja.html`; English and Japanese PDFs under `docs/en/pdf/` and `docs/ja/pdf/` |
| Backend (API page sizes, DEC-006/008) | `ProductionLinesController.cs`, `LineContracts.cs`, `LineValidation.cs`, `ProductionLineService.cs`, `ProductionLineRepository.cs` |
| Frontend (SCR-005 redesign, DEC-012 note) | `production-lines/`: `ProductionLineListPage.tsx`, `ProductionLineFormPage.tsx`, `LineDialog.tsx`, `api.ts`, `lineViewRules.ts`, new `AddProductDialog.tsx` and `lineStyles.ts`; `production-orders/messages.ts` |
| Tests | `LineValidationTests.cs`, `LineWriteOutcomeTests.cs`, `ProductionLineEndpointTests.cs`, `LinePages.test.tsx`, `lineRules.test.ts`, `production-lines.spec.ts`, `production-lines.mobile.spec.ts`, new `production-lines-redesign.spec.ts` |
| Records | `work-items/WI-015/`: brief, plan, decisions, status, evidence, test-plan |

Not included: scratchpad files (PDF virtual environment, Japanese translation source, generator/render scripts,
screenshots, build artifacts), test screenshots or generated JSON. The approved WI-009 005 files stay unedited.

### Steps

| # | Step | Verification |
| --- | --- | --- |
| 1 | Pre-commit checks on `fix/WI-015-production-line-redesign`: the file list above matches `git status` exactly (no stray files such as `bash.exe.stackdump`); secret scan of the diff and the records (no `deploy/.env` value, password or token); approved 005 files unchanged (hash baseline); the Japanese UI-text scan (TC-301) passes | Results in evidence.md |
| 2 | Two commits, so the design and the code can be reviewed separately: (a) `docs(WI-015): add approved SCR-005 production-line redesign` (design group); (b) `fix(WI-015): align production lines screen with approved redesign` (backend, frontend, tests, records) | `git log` |
| 3 | `git push -u origin fix/WI-015-production-line-redesign`; `gh pr create` to `master`. Description: summary of BUG-009/REQ-084, DEC-002–012, API change (optional page sizes; defaults unchanged), test results, known local limits (plant-calendar E2E needs CI's fixture; screen-reader speech and physical mobile keyboard/IME not run), and "no deployment" | PR URL |
| 4 | Watch CI (Backend, Frontend, E2E) read-only. If a job fails: diagnose; fix only test-environment or non-behavioural problems with one more commit to the same branch; otherwise stop and report | CI run ID and result |
| 5 | Record the PR, commits and CI run in evidence/status/plan; push that record update as one more commit to the PR (documentation only, CI skips it) | evidence.md, status.md |

### Permitted actions

- Local commits on `fix/WI-015-production-line-redesign`, `git push` of that branch, `gh pr create`, PR description
  edits, read-only CI and PR checks.
- Commits that fix CI problems which do not change behaviour or the approved design.

Not permitted without further approval:
- merging the PR, deleting branches, or closeout documentation (`README.md`, `ai/project.md`, `CLAUDE.md` current state);
- deployment or image publication;
- edits to approved designs (WI-009 005 files and 005_DD-SPD-REDESIGN version 3);
- force-push or history rewrite of a pushed branch.

### Risks / stop conditions

- CI's E2E job runs against a fresh database, while local runs used the demo database: a test that relied on local
  data would fail in CI. Fix the test (not the app) and report it.
- Plant-calendar specs are expected to pass in CI (they need its fixture); if they fail there, stop and report.
- If `master` moved since `2f502b1`, rebase locally before the first push only; after pushing, merge `master` instead.

### Review and approval

Review status: approved 2026-10-07 ("approved, go ahead", DEC-013). Steps 1–5 complete; one test-only CI fix
(`c427dfe`, see evidence). Merge, cleanup and closeout need the user's approval.

## Revision 4 — Merge, cleanup and closeout

| # | Step | Result |
| --- | --- | --- |
| 1 | Confirm final head `7c22966` CI and mergeability | Run 37562084126 passed all three jobs; PR MERGEABLE, CLEAN |
| 2 | Squash-merge PR #45 (guarded by the head commit) and delete the feature branch | Merged as `2470c8e`; remote and local branch deleted; refs pruned; `master` synchronized; `master` CI run 37563252878 passed |
| 3 | Documentation-only closeout PR on `chore/WI-015-closeout`: README, `ai/project.md`, CLAUDE.md, WI-015 records; WI-014 status line about its closeout PR corrected | This PR |

Permitted: the merge/cleanup the user requested and this closeout PR. Not permitted: merging the closeout PR,
deployment, demo-database changes. The closeout PR's merge needs the user's approval.
