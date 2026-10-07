# WI-015 — Evidence

All results 2026-10-06, local branch `docs/WI-015-production-line-redesign`, plan revision 1.

## Step 1 — Sources read and approved files frozen

Read: [005_REQ](../../docs/en/000_requirements/005/005_REQ_production-lines.md) (REQ-064–069, rules 1–8),
[005_BD](../../docs/en/010_basic-design/005/005_BD_生産ライン・工程.md) (FN-032–036, regions 1–9, states, items,
events), [005_DD](../../docs/en/020_detailed-design/005/005_DD_生産ライン・工程.md) §4–6,
[005_DD-API](../../docs/en/020_detailed-design/005/005_DD-API_生産ライン・工程.md) §2–5 (API-PL-01–06),
[005_DD-SPD](../../docs/en/020_detailed-design/005/005_DD-SPD_生産ライン・工程.md) (processing 4.1–4.6, catalog §5),
the 20-artboard approved mockup, `src/frontend/src/features/production-lines/*`, the running screen, and the
製品マスタ (WI-013) visual tokens.

SHA-256 of all 20 approved 005 requirement/BD/DD files, wireframes and mockups was recorded before work
(scratchpad `wi015-baseline.sha256`); step 4 re-checks it.

## Step 1 — Trace of requirements to the redesign

| Source | Rule / behaviour | Redesign element (005_DD-SPD-REDESIGN section) |
| --- | --- | --- |
| REQ-064, BD 3/5, SPD 4.1 | Search code/name, state filter (default 使用中), URL filters, page 50, loading/no lines/no match/error | §4 list: filter row, 「N 件」, pager only when >1 page, four distinct notices |
| REQ-065, rule 1 | Code trimmed, immutable, case-insensitive unique; name required | §5 fields: code read-only on edit (grey, 「（変更不可）」), hint 「登録後は変更できません。」 on create; duplicate message |
| REQ-066, rules 2–4 | Hours 0<h≤24, ≤3 decimals; minutes per one displayed unit, >0, ≤3 decimals; unit read-only; explicit reconfirmation | §5 hints; pair table 「製造時間（分）」 with 「1 {unit} あたり」; stale notice + 「現在の単位で確認」 (DEC-003) |
| REQ-067, rule 5, SPD 4.3/4.4 | Line retire immediate with confirmation; pair retire staged until Save; no restore; unsaved row removable | §6 dialogs; 「使用停止予定（保存後に反映）」 row state; 「取り消す」 on staged row; 「削除」 on unsaved row |
| REQ-068 | Order assignment | Out of scope (SCR-001/002 unchanged) |
| REQ-069 | Admin/Operator, 401/403, centred native dialogs, keyboard/focus, mobile/zoom, Factory glyph | §7 accessibility; forbidden state; dialogs reuse existing geometry |
| SPD 4.2/4.6 | Conflict and unknown save: keep draft, guarded reload, verification without replay | §6 states 「変更が競合しています。」 / 「保存結果を確認できません。…」 with 「現在の状態を確認」 and 「再読み込み」 |
| DD-API API-PL-06 | Product choices: q, page (50), lineId; excludes saved pairs | §5 add-product dialog: search, list, pager inside the dialog |
| DD-API API-PL-02 | Pair pages of 50 | Pair table pager only when >50 pairs |
| BUG-009 audit | Wording, badges, colours, picker inline list, Save/Cancel order, focus box, 「状態」 text | All replaced as listed in §8 change list |

No requirement needs an API, database or business-rule change. Q2 (DEC-003) changes only when the client
sends `confirmUnit: true` for a new pair: after the user picks the product in the dialog, which displays the
unit. The server still verifies the unit and revision.

## Steps 2–3 — Design package

| Artifact | Path |
| --- | --- |
| Design (English, source) | `docs/en/020_detailed-design/005/005_DD-SPD-REDESIGN_生産ライン・工程.md` |
| Wireframes (SVG) | `docs/en/020_detailed-design/005/wireframes/005_DD-SPD-REDESIGN_SCR-005-{pc,sp,dialog}.svg` |
| State mockups (24 states; English / Japanese captions, identical Japanese UI) | `docs/en/020_detailed-design/005/mockups/005_DD-SPD-REDESIGN_SCR-005.html`, `.ja.html` |
| PDFs | Not in the review package (DEC-007): rendered after the design `.md` is approved (plan step 5) |

Rendering checks (Chromium 153):
- The three SVGs parse as XML and render. After fixes, no text overflows and no badge overlaps.
- All 25 gallery boards render (state 24 holds two SP boards side by side). There are no console errors and no board overflows horizontally.
- I inspected the PC list, edit, validation, stale-unit, staged-retire, add-dialog, SP list, SP edit and SP add-dialog boards.
- PDFs of version 1 were rendered once (English and Japanese, 15 pages each) and then deleted at the user's request (DEC-007). They return after approval.
- No PNG screenshots were added to `docs/`.

Approved 005 files: `sha256sum -c` against the step 1 baseline passed, so all 20 files are unchanged.

A Git Bash crash dump (`docs/en/bash.exe.stackdump`, msys stack trace) appeared during rendering. I inspected
and deleted it.

## Design-consistency checklist (`ai/checklists/design-consistency.md`)

| Item | Result |
| --- | --- |
| Requirements have IDs/acceptance | Pass: REQ-064–069 (005_REQ), REQ-082–084 (brief) |
| BD navigation/primary actions/exceptions | Pass: unchanged 005_BD routes and transitions; the design adds no route or transition, so no new Mermaid diagram is needed |
| Every numbered SVG item in a region table | Pass: badges 1–12 = R1–R12 in §3 |
| DD fields/validation/states agree with BD | Pass: §4–§6 restate the BD/DD rules; only presentation changes |
| API/DB mappings agree | Pass: §10 uses API-PL-01–06 unchanged; no DB change |
| Missing decisions resolved | Pass: Q1–Q3 → DEC-002–004 |
| Test scenarios mapped | Pass: §11 verification viewpoints; a `test-plan.md` comes with the implementation revision |
| Security-relevant fields | Pass: none new; role checks unchanged |
| Accessibility | Pass: §9 |
| Migration impact | Not applicable: no schema change |
| Telemetry | Not applicable: no new endpoint or job |
| PDFs re-rendered, footers name the revision | Deferred by DEC-007 until the design `.md` is approved (plan step 5) |

## Review round 1 (2026-10-06)

User feedback:
1. "on list, allow select record per page same product screen", recorded as DEC-006.
2. "only generate pdf after disign md approved", recorded as DEC-007.

Changes made for version 2:
- **Design document §1, §3, §4, §7, §8, §10.1, §11 and §12:** 「表示件数」 10/20/50/100 (default 20) at the top-right of the list, kept in the URL `pageSize`. API-PL-01 gets an optional allow-listed `pageSize`, defaulting to 50.
- **Wireframes:** PC and SP region 4 now show the select.
- **Mockups:** every list board with rows shows 「表示件数 20」.
- **PDFs:** the generated PDFs were deleted.

Re-render: 25 boards, no console errors, no overflow. Checked: SVGs, list boards 7 and 23.

## Review round 2 (2026-10-06)

User feedback:
1. "register and edit screen limit 20 record per page"; answer "Products table + add dialog", recorded as DEC-008.
2. "all list has overflow scroll"; answer "Fixed-height, scroll inside", recorded as DEC-009.

Changes made for version 3:
- **20 per page:** the product table and the add dialog show 20 per page. The design §10.2 adds optional allow-listed `pairsPageSize` on API-PL-02 and `pageSize` on API-PL-06, with default 50 so other callers are unchanged.
- **Scroll boxes:** every list sits in a box with a maximum height, scrolls inside it and has a sticky header. Count, page size and pager stay outside. Each box is a keyboard-focusable named region (§3, §4, §5.2, §5.3, §9).
- **Mockups:** the list scrolls with 10 rows; the edit screen has 23 pairs with pager 1 / 2; the dialog shows 45 candidates over 3 pages; boards for 20 rows per page and SP were added.
- **Wireframes:** scroll indicators added, and the pager note changed to 21+ pairs.

Re-render: 25 boards, no console errors, no board overflow. Checked: SVGs, list board 1, edit board 12, dialog board 10.

## Step 4–5 — Approval and PDFs (2026-10-06)

Design version 3 was approved: "approved, go ahead" (DEC-010). The document's review state was set to approved.
I updated the temporary Japanese translation to version 3 and rendered both PDFs with `scripts/docs-pdf.py`,
using the source note "005_DD-SPD-REDESIGN version 3 (2026-10-06)":

| PDF | Pages | Footer check |
| --- | --- | --- |
| `docs/en/pdf/020_detailed-design/005/005_DD-SPD-REDESIGN_生産ライン・工程.pdf` | 18 | Names version 3 |
| `docs/ja/pdf/020_detailed-design/005/005_DD-SPD-REDESIGN_生産ライン・工程.pdf` | 17 | Names version 3 |

Spot-checked: page 1 (English) and the §10.1/10.2 page (Japanese). The approved 005 files still pass the hash check.

## Plan revision 2 — Implementation and tests (2026-10-06, uncommitted)

Branch: `fix/WI-015-production-line-redesign` (renamed from `docs/WI-015-production-line-redesign`, never pushed).

### Changes

| Area | Files |
| --- | --- |
| API page sizes (DEC-006, DEC-008): optional `pageSize` on API-PL-01 and API-PL-06 and `pairsPageSize` on API-PL-02; allow-list 10/20/50/100; default 50; invalid values return 400 `VALIDATION` naming the field; API-PL-07 unchanged | `ProductionLinesController.cs`, `LineContracts.cs`, `LineValidation.cs`, `ProductionLineService.cs`, `ProductionLineRepository.cs` |
| List (§4): description, primary 新規ライン, filter row, badges, red retire, 「N 件」 and 「表示件数」 (URL `pageSize`, default 20, canonicalized, past-end goes to the last page), pager only when needed, separate no-lines and no-match notices, one scroll region with a sticky header, cards on SP | `ProductionLineListPage.tsx`, `lineViewRules.ts` |
| Form (§5): 基本情報 card, red `*`, hints, read-only code, retired-line notice, product table/cards at 20 per page in a scroll region, badges, 削除/取り消す, stale-unit notice with 「現在の単位で確認」, error summary, recovery boxes, 保存 before キャンセル, heading focus without a visible box | `ProductionLineFormPage.tsx` |
| Add dialog (§5.3, DEC-003): search, 20 per page, single-choice radios in a scroll region, note; adding confirms the shown unit | new `AddProductDialog.tsx` |
| Dialogs (§6): new look, 「編集を続ける」 for discard, line and pair wording and notes | `LineDialog.tsx` (the old exported classes are kept for `OrderLinePicker`, so SCR-001 is unchanged) |
| Tokens (§3) | new `lineStyles.ts` (製品マスタ vocabulary) |
| Catalog (§7): line-page keys updated or added; keys shared with SCR-001/002 keep their text | `messages.ts` |
| API client: page sizes; list/pair/choice decoders accept allowed sizes; eligible lines stay at 50 | `api.ts` |
| Tests | `LineValidationTests.cs`, `LineWriteOutcomeTests.cs` (fake signature), `ProductionLineEndpointTests.cs` (old `?pageSize=10` rejection becomes `?pageSize=25`, per DEC-006), `LinePages.test.tsx`, `lineRules.test.ts`, `production-lines.spec.ts`, `production-lines.mobile.spec.ts`, new `production-lines-redesign.spec.ts` |

### Checks

| Check | Command | Result |
| --- | --- | --- |
| Backend build | `dotnet build ... --artifacts-path <scratch>` | Passed. A user-run Debug `ProductionManagementAI.Api` (PID 16852, started 15:48) locks `bin/`; I left it running and built to the scratchpad |
| Backend unit + integration | `dotnet test ... --artifacts-path <scratch>` | Passed: 251 unit (239 + 12), 215 integration (203 + 12) |
| Frontend lint / types / build | `npm run lint`, `npx tsc -b`, `npm run build` | Passed |
| Frontend unit | `npx vitest run` | Passed: 22 files, 281 tests (267 + 14), including the TC-301 inline-text scan and axe |
| Line E2E (old + new) | `npx playwright test specs/production-lines*.ts` | 13 passed |
| Full E2E | `npx playwright test`, rebuilt Compose `backend` and `frontend` | 62 passed; 11 failed, all plant-calendar specs needing CI's activated-calendar fixture (same as WI-012–014). Every line, order, product, dashboard and mobile spec passed |
| Approved files | `sha256sum -c` baseline | Unchanged |

Screenshots were compared with the approved mockup:
- **List:** PC and SP.
- **Edit:** PC and SP.
- **Add dialog:** PC.
- **Long text:** list at 1280 and 320 px, and the retire dialog at 320 px.

They match the artboards' order, grouping and wording. One change after the first screenshot: the SP pair card was reordered so the badge and 「単位：…」 sit on one line above the time field, as in board 24.

Implementation notes:
- **Row 使用停止 height:** it is 42 px, not 40, because it stretches to its outlined 編集 neighbour, the same as 製品マスタ. TC-431 records this.
- **Unit change after adding:** a 409 on a new pair maps to 「単位が変更されました。」 on that row. The form follows the approved Conflict rule: reload after confirming the discard.

The demo database gained uniquely named test lines and products from the E2E runs; no assertion depends on totals.
Nothing is committed, pushed or opened as a PR (DEC-011).

## Retired-pair note (DEC-012), 2026-10-07

The user asked why a 使用停止 product cannot be registered again on the edit screen. The rule is unchanged (approved
WI-009 design: retirement is one-way; API-PL-06 excludes every saved pair, retired included). The user chose to explain
it on screen.

| Change | Files |
| --- | --- |
| Retire dialog: bold 「保存後は、この製品をこのラインに再登録できません。」 | `ProductionLineFormPage.tsx`, `messages.ts` |
| Saved retired row: 「再登録できません」 in 操作 | `ProductionLineFormPage.tsx` |
| Edit-mode add dialog: excluded-products note, also in `aria-describedby` | `AddProductDialog.tsx` |
| Tests: TC-435 unit; TC-428 and TC-426 assertions; TC-360 E2E extended | `LinePages.test.tsx`, `production-lines.spec.ts` |

| Check | Result |
| --- | --- |
| Frontend lint / build (types) | Passed |
| Frontend unit | Passed: 22 files, 282 tests (281 + 1) |
| Line E2E, Compose `frontend` rebuilt (backend unchanged) | 13 passed |

Screenshots at 1280 and 390 px (edit screen with a saved retired pair, and the add dialog): the row text sits in 操作
(PC) and at the end of the card (SP); the dialog note wraps inside the dialog. Still uncommitted (DEC-011).

## Plan revision 3 — Commit, PR and CI (2026-10-07)

### Pre-commit checks

| Check | Result |
| --- | --- |
| Staged files match the plan's list | Yes: 8 design files, 27 code/test/record files; no stray files |
| Secret scan of the diff and records (every `deploy/.env` value, common token patterns) | No hits |
| Approved 005 files (hash baseline, 20 files) | Unchanged |
| Japanese UI-text scan (TC-301, `NoInlineText.test.ts`) | Passed in the 282-test run after the last code change |

### Commits and PR

| Item | Value |
| --- | --- |
| Branch | `fix/WI-015-production-line-redesign` (from `2f502b1`) |
| Commits | `8f80342` docs (design package); `7ab6fa4` fix (code, tests, records); `c427dfe` test-only CI fix |
| PR | [#45](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/45) to `master`, open, mergeable |

### CI

| Run | Head | Result |
| --- | --- | --- |
| 37560735551 | `7ab6fa4` | Backend and Frontend passed; E2E 72 passed, 1 failed: TC-421/422/430 compared the sticky header with a box position read before focusing the box, and focusing can scroll the page (header off by 39 px). Test fault, not app behaviour |
| 37561507760 | `c427dfe` | All three jobs passed: 251 backend unit, 215 integration, 282 frontend unit, 73 E2E (including every plant-calendar spec) |

The fix measures the header and box in one read after scrolling, polled until stable, and bounds the box height by the
real viewport. It still fails if the header is not sticky. The spec passed 25/25 locally with `--repeat-each 5`.
No merge, branch deletion or deployment.
