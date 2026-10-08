# WI-016 — Status

As of 2026-10-07. Work item state: in-review (plan revision 2 complete, local only).

## Overall status

**RAG:** Green — CSV export implemented and tested locally; nothing committed. Awaiting the user's review and a plan revision 3 for commit/push/PR.

## Approved plan reference

[plan.md](plan.md) — revision 1 complete (DEC-006); revision 2 complete (approval DEC-019).

## Accomplishments this period

| Date | Milestone / deliverable | Evidence link |
| --- | --- | --- |
| 2026-10-07 | Local branch `feature/WI-016-order-csv-export` from `master` `a470146`; brief and plan revision 1 drafted | [brief.md](brief.md), [plan.md](plan.md) |
| 2026-10-07 | Plan revision 1 approved; Q1–Q4 answered (DEC-002–DEC-006) | [decisions.md](decisions.md) |
| 2026-10-07 | Step 1 trace table; step 2 002_BD-CSV v1 with PC/SP wireframes | [evidence.md](evidence.md), [002_BD-CSV](../../docs/en/010_basic-design/002/002_BD-CSV_製造指示一覧.md) |
| 2026-10-07 | Review round 1: export hint item 30 → 002_BD-CSV v2 (DEC-009) | [decisions.md](decisions.md) |
| 2026-10-07 | 002_BD-CSV v2 approved; EN/JA PDFs (DEC-010) | [evidence.md](evidence.md) |
| 2026-10-07 | 002_DD-CSV v1 with wireframes and EN/JA mockups (DD split into four files, DEC-011) | [evidence.md](evidence.md) |
| 2026-10-07 | 002_DD-CSV approved, PDFs (DEC-012); 002_DD-API-CSV v1 + sample CSV (DEC-013) | [evidence.md](evidence.md) |
| 2026-10-07 | 002_DD-API-CSV approved, PDFs (DEC-014); 002_DD-FN-CSV v1 (DEC-015) | [evidence.md](evidence.md) |
| 2026-10-07 | 002_DD-FN-CSV approved, PDFs (DEC-016); 002_DD-SPD-CSV v1, DEC-017 proposed | [evidence.md](evidence.md) |
| 2026-10-07 | 002_DD-SPD-CSV approved, DEC-017 accepted (DEC-018); 002_DD-CSV v2; all PDFs rendered; plan revision 1 closed, revision 2 drafted | [plan.md](plan.md) |
| 2026-10-07 | Revision 2: backend, frontend, test plan TC-440–469; 286 unit / 234 integration / 307 frontend / 3 new E2E passed; 10,000 rows in 227 ms | [evidence.md](evidence.md), [test-plan.md](test-plan.md) |

## Planned for next period

Plan revision 3 (commit, push, PR to `master`, CI watch) approved 2026-10-08 (DEC-021); in progress.

## Risks and issues

| Issue / blocker | Owner | Since | Impact |
| --- | --- | --- | --- |
| TC-469 Not run (no spreadsheet app); 12 plant-calendar E2E specs fail locally (CI fixture only) | ThongTM | 2026-10-07 | Excel display unverified by a real application; CI needed for the calendar specs |

## Next action

User reviews the implementation and decides on plan revision 3 (commit/PR), owner ThongTM.
