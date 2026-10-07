# WI-015 — Status

2026-10-07: done. BUG-009 (SCR-005 production lines did not match its mockup and design) fixed with a new additive
design, [005_DD-SPD-REDESIGN](../../docs/en/020_detailed-design/005/005_DD-SPD-REDESIGN_生産ライン・工程.md)
version 3 (approved, DEC-010), and its implementation under plan revisions 1–3. The WI-009 005 documents are unedited.

- **List:** 「表示件数」 10/20/50/100 in the URL (default 20), badges, red 使用停止 on active lines, separate
  empty/no-match notices, pager only when needed.
- **Register/edit:** 基本情報 card; product table 20 per page; new 製品を追加 dialog whose add confirms the shown
  unit (DEC-003); 「現在の単位で確認」 only on stale saved pairs; staged retire with undo.
- **Every list** scrolls inside a bounded box with a sticky header (DEC-009); controls use the order-screen
  heights (DEC-002).
- **Retired pairs:** still one-way; the retire dialog, the retired row and the add dialog say a saved 使用停止
  product cannot be registered again (DEC-012).
- **API:** optional allow-listed `pageSize` (line list, product choices) and `pairsPageSize` (line detail), default
  50 (DEC-006, DEC-008).

PR [#45](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/45) was squash-merged as `2470c8e` at the user's request, after final head `7c22966` passed CI
[run 37562084126](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/37562084126): 251 backend unit, 215 integration, 282 frontend unit, 73 E2E. `master` run 37563252878
passed. The feature branch is deleted locally and remotely, and `master` is synchronized. No deployment.
Documentation closeout PR on `chore/WI-015-closeout` awaits the user's review; its merge is not yet authorized.

Known limits: screen-reader speech and physical mobile keyboard/IME were not run (as in WI-010/012). See
[test-plan](test-plan.md) known gaps and [evidence](evidence.md).

Open follow-ups (not started; candidates for harness-improvement RFCs, not yet requested):
- require `test-plan.md` whenever a work item adds tests (user rule from WI-014);
- render design PDFs only after the design Markdown is approved (user rule, DEC-007).
