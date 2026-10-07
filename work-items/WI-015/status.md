# WI-015 — Status

2026-10-06:
- **Audit:** SCR-005 was audited against the approved 005 mockup and design (BUG-009, see [brief](brief.md)).
- **Plan:** plan revision 1 was approved ("continue" after Q1–Q3; DEC-002–005).
- **Design package (steps 1–3):** complete on the local branch `docs/WI-015-production-line-redesign`, uncommitted:
  - `005_DD-SPD-REDESIGN_生産ライン・工程.md`;
  - PC, SP and dialog SVG wireframes;
  - an English/Japanese 24-state HTML gallery.
- **Checks:** the approved 005 files are unchanged (hash check). The design-consistency checklist passes. See [evidence](evidence.md).

Review round 1 applied (DEC-006: rows per page with an optional `pageSize` on API-PL-01; DEC-007: PDFs only after
design approval, so the early PDFs were deleted). Review round 2 applied (DEC-008: 20 per page in the product table and
the add dialog, with optional page sizes on API-PL-02/06; DEC-009: every list scrolls inside a fixed-height box). The
design is now version 3.

Design version 3 approved ("approved, go ahead", DEC-010); English/Japanese PDFs rendered (step 5). Plan revision 1
complete.

Plan revision 2 approved with changes ("implementation and test, not commit yet"; long-text cases; DEC-011).

Implementation and tests are complete on the local branch `fix/WI-015-production-line-redesign`, uncommitted. See
[evidence](evidence.md) and [test-plan](test-plan.md):
- backend: 251 unit and 215 integration tests pass;
- frontend: 281 unit tests pass;
- E2E: 13/13 line specs; the full suite has 62 passed and 11 known local plant-calendar failures.

2026-10-07: on user review, the edit screen now explains that a saved 使用停止 pair cannot be registered again
(DEC-012, TC-435; rule unchanged). 282 frontend unit tests and 13/13 line E2E pass.

Next: plan revision 3 (commit, push, PR to `master`, CI) is proposed and awaits approval; see [plan](plan.md). A user-run Debug API
process (PID 16852) is still running and was not touched. Nothing is committed or pushed.

Temporary files, to remove when the work item no longer needs them:
- a scratch virtual environment (`pdfvenv`, with `markdown` and `pymupdf`);
- the Japanese translation source;
- generator and render scripts in the session scratchpad.
