<!-- Based on ai/templates/test-plan.md (IEEE 829-1998). -->

# WI-015 — Production lines screen redesign — Test Plan

## Test plan identifier

TP-015, work item WI-015, revision 1, 2026-10-06 (plan revision 2).

## References

[brief.md](brief.md) (BUG-009, REQ-082–084), [plan.md](plan.md) revision 2,
[005_DD-SPD-REDESIGN](../../docs/en/020_detailed-design/005/005_DD-SPD-REDESIGN_生産ライン・工程.md) version 3
(§11 verification viewpoints, §12 artboards), decisions DEC-002, DEC-003, DEC-006, DEC-008, DEC-009, DEC-011 and DEC-012.
The approved WI-009 test cases TC-325–TC-365 remain the regression baseline.

## Introduction

This plan covers the redesigned SCR-005 list and register/edit screens, the optional API page sizes (§10.1 and
§10.2), and, at the user's request (DEC-011), long-text cases that check the UI does not break. The new case IDs
start at TC-420 so they do not collide with WI-009 (TC-325–365) or WI-010/012 (TC-400–414).

## Test items

| Requirement ID | Description |
| --- | --- |
| REQ-084 | The running SCR-005 matches design version 3 on PC and SP: control order, grouping, labels, conditional visibility and states |
| DEC-002 | Filled buttons 40 px, outlined buttons and fields 42 px, as on the order screens |
| DEC-003 | Adding a product in the dialog confirms its unit; 「現在の単位で確認」 only on saved stale pairs |
| DEC-006 | 「表示件数」 10/20/50/100 on the list (default 20, in the URL); optional `pageSize` on API-PL-01 |
| DEC-008 | 20 per page in the product table and the add dialog; optional `pairsPageSize` on API-PL-02 and `pageSize` on API-PL-06 |
| DEC-009 | Every list scrolls inside a bounded box with a sticky header; keyboard-reachable; no horizontal page scroll |
| DEC-011 | Long text (maximum lengths, no spaces) never breaks the layout |
| DEC-012 | The screen explains that a saved 使用停止 pair cannot be registered again |

## Features to be tested

TC-420 to TC-435 below, plus the WI-009 regression cases updated only for changed wording or layout.

## Features not to be tested

- Order assignment (SCR-001/002, API-PL-07): unchanged; existing specs run as regression only.
- Database schema: unchanged.
- Screen-reader speech and physical mobile keyboard/IME: not available in this environment (as in WI-010/012).

## Approach

| Level | Included? | Rationale |
| --- | --- | --- |
| Unit (xUnit) | yes | Page-size allow-list and field names in `LineValidation` |
| Integration (xUnit + Testcontainers) | yes | Endpoint contracts for `pageSize` / `pairsPageSize`, defaults, rejection, eligible unchanged |
| Unit (Vitest + RTL + axe) | yes | Rendering, request bodies (DEC-003), URL rules, dialogs, validation, catalog wording |
| E2E (Playwright + axe) | yes | Real layout: scroll boxes, heights, long text, SP, keyboard |
| Manual screenshots | yes | PC/SP compared with the approved mockup artboards |

## Item pass/fail criteria

Every case below passes. Existing backend, frontend and E2E suites pass, apart from the known local
plant-calendar environment failures, which need CI's calendar fixture. axe reports no violations.

## Suspension criteria and resumption requirements

Suspend if the local Compose stack or Docker (Testcontainers) is unavailable. Resume when they are running again.

## Test deliverables

This plan; the test code listed in the cases; results in [evidence.md](evidence.md) and in the table below.

## Cases

| Test ID | Requirement | Setup | Steps | Expected result | Level / file |
| --- | --- | --- | --- | --- | --- |
| TC-420 | DEC-006, DEC-008 | — | Query with page sizes 10/20/50/100, omitted, and 0/25/1000/abc/repeated | Allowed sizes echoed with at most that many rows; omitted gives 50; others give 400 `VALIDATION` naming `pageSize` or `pairsPageSize`; `eligible` still rejects `pageSize` | Unit `LineValidationTests`, integration `ProductionLineEndpointTests` |
| TC-421 | DEC-006 | Lines exist | Open the list; change 「表示件数」; search; Clear; page; open a URL with `pageSize=25` | Default 20 in the URL; a change goes to page 1; kept by search, Clear and paging; invalid replaced by 20 | Unit, E2E |
| TC-422 | REQ-084 list | Active and retired lines | Open the list (state すべて) | Description; 「新規ライン」 primary; badges; red 使用停止 only on active lines; 「N 件」 top-left; pager only when more than 1 page; 「前へ p / n 次へ」 | Unit, E2E, screenshot |
| TC-423 | REQ-084 list states | Mocked responses | No lines; no match; read error; loading | Distinct 「生産ラインはまだ登録されていません。」 and 「該当する生産ラインはありません。」 with hint; error with 再試行; no fabricated rows | Unit |
| TC-424 | REQ-084 form | Create and edit | Open both modes | 「生産ライン登録」 / 「生産ライン編集」; 基本情報 card; red `*`; hints; read-only code with 「（変更不可）」; 保存 before キャンセル; heading focused without a visible box | Unit, E2E |
| TC-425 | DEC-003 | Create | 製品を追加 → choose a product → 追加 → enter minutes → 保存 | Dialog closes, the 追加予定 row is focused; POST body has `confirmUnit: true` with the observed unit and revision; no checkbox | Unit, E2E |
| TC-426 | DEC-008 add dialog | More than 20 choices | Open the dialog; page; search; Escape | 20 per page, pager inside the dialog; already-added products are not offered; Escape changes nothing and returns focus | Unit, E2E |
| TC-427 | DEC-003 stale | A saved pair needing reconfirmation | Change minutes and save; click 「現在の単位で確認」; 取り消す | Error 「現在の単位で確認してください。」 until confirmed; then the PUT sends `setTiming` with `confirmUnit: true` | Unit |
| TC-428 | REQ-084, REQ-067 | A saved active pair | 使用停止 → confirm → 取り消す → retire again → 保存 | Row shows 「使用停止予定（保存後に反映）」; undo restores it; the retirement is sent only on 保存 | Unit, E2E |
| TC-429 | DEC-008 pairs | A line with more than 20 pairs (mocked) | Open edit; next page; an error on page 2 | Pager 「1 / 2」; intents kept across pages; the error is revealed and focused | Unit |
| TC-430 | DEC-009 | 100 rows per page; more than 20 pairs; 20 choices | Measure the boxes; Tab to each region; scroll | Each list scrolls inside its box, the header stays visible, the pager and buttons stay visible, no horizontal page scroll; regions are focusable and named | E2E |
| TC-431 | DEC-002 | — | Measure buttons and fields on list, form and dialog | Filled 40 px; outlined and fields 42 px; same as the order screen's 「+ 新規製造指示」 and filter buttons | E2E |
| TC-432 | DEC-011 | Line code 50 chars and name 200 chars (real save); mocked product SKU 50 / name 200; strings without spaces | Open the list (PC 1280, SP 390, 320), edit, add dialog, retire dialogs | No horizontal page scroll; text wraps inside cells, cards and dialogs; header, status and actions do not wrap on PC; dialogs inside the viewport; buttons keep their height | E2E |
| TC-433 | REQ-069 | — | axe on list, form, add dialog and retire/discard dialogs, PC and SP | No violations | Unit, E2E |
| TC-435 | DEC-012 | A saved retired pair | Retire a pair (dialog); save; reopen edit; open 製品を追加; search its SKU | The retire dialog warns that it cannot be registered again; the retired row shows 「再登録できません」 with no 使用停止 button or time field; the add dialog shows the excluded-products note (and in its description) and does not offer the product; axe passes | Unit, E2E |
| TC-434 | Regression | — | WI-009 line specs (updated wording) and order specs | Pass; order assignment unchanged | Unit, E2E |

## Environmental needs

- Local Compose stack (`deploy/compose.yaml`), with `backend` and `frontend` rebuilt from the branch.
- Docker for the Testcontainers integration tests.
- E2E cases create uniquely named lines and products in the local demo database, as the existing specs do. Long-product cases use mocked responses.

## Commands and prerequisites

```
dotnet test src/backend/ProductionManagementAI.slnx --artifacts-path <scratch>   # a local Debug API locks bin/
cd src/frontend && npm run lint && npm run build && npx vitest run
cd tests/e2e && E2E_ADMIN_PASSWORD=<SEED_ADMIN_PASSWORD> E2E_BASE_URL=http://localhost:3000 npx playwright test
```

## Responsibilities and schedule

Single-agent execution in plan revision 2 steps 3–8. The user reviews before any commit.

## Risks and contingencies

| Risk | Contingency |
| --- | --- |
| jsdom has no layout, so it cannot check scrolling or wrapping | Covered by E2E measurements (TC-430–432) |
| The demo database keeps growing with test lines | Unique names; no assertion depends on totals |

## Results and linked evidence

| Test ID | Result | Evidence | Date |
| --- | --- | --- | --- |
| TC-420 | pass (full backend: 251 unit, 215 integration) | [evidence.md](evidence.md) | 2026-10-06 |
| TC-421, TC-422, TC-430 | pass (CI 37561507760 after test fix `c427dfe`) | Unit `LinePages.test.tsx`; E2E `production-lines-redesign.spec.ts` "TC-421/422/430" | 2026-10-06 |
| TC-423 | pass | Unit (no lines / no match / read error) | 2026-10-06 |
| TC-424 | pass | Unit (register and edit forms, axe) | 2026-10-06 |
| TC-425, TC-426 | pass | Unit; E2E "TC-425/426" (real save; `requiresUnitConfirmation: false` read back) | 2026-10-06 |
| TC-427 | pass | Unit (stale pair, confirm, clear on change) | 2026-10-06 |
| TC-428 | pass | Unit (stage, undo, send on Save); E2E TC-360 (stage and persist only on Save) | 2026-10-06 |
| TC-429 | pass | Unit (20 per page, off-page intent, revealed error) | 2026-10-06 |
| TC-431 | pass | E2E "TC-431" (row 使用停止 stretches to 42 px beside 編集, as on 製品マスタ) | 2026-10-06 |
| TC-432 | pass | E2E "TC-432" (real 50/200-character lines, 1280/390/320 px) and "TC-430/432" (mocked 50/200-character products, 25 pairs, 25 choices) | 2026-10-06 |
| TC-433 | pass | axe in unit tests and in every new E2E case, PC and SP | 2026-10-06 |
| TC-435 | pass | Unit "TC-435" and the retire-dialog assertion in TC-428; E2E TC-360 extended (real retired pair: row text, dialog note, P-1001 not offered, axe) | 2026-10-07 |
| TC-434 | pass | Full local E2E 62 passed; 11 plant-calendar failures are the known local fixture limit | 2026-10-06 |

## Known gaps

| Gap | Reason | Risk | Follow-up |
| --- | --- | --- | --- |
| Screen-reader speech, physical mobile keyboard/IME | Not available locally | Low | Same accepted limit as WI-010/012 |
| Plant-calendar E2E locally | Needs CI's calendar fixture | None for this change | CI on a later PR |
| "Fails before the change" not run for each new E2E case | The old screen lacks the controls entirely (no 「表示件数」, no add dialog, no regions), so the new cases fail by construction | Low | None |

## Approvals

Plan revision 2 approval (DEC-011). Results are reviewed by the user before any commit.
