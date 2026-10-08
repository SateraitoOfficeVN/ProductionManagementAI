# WI-016 — Status

As of 2026-10-08. Work item state: in-review (PR #48 open, CI passed; not merged).

## Overall status

**RAG:** Green — PR #48 open against `master`; CI run 37713513237 passed all three jobs on `127c81f`. Merge, cleanup and closeout need the user's approval.

## Approved plan reference

[plan.md](plan.md) — revision 1 complete (DEC-006); revision 2 complete (DEC-019); revision 3 complete (DEC-021).

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
| 2026-10-08 | Revision 3: commits `5f0380d` (design) and `127c81f` (code); PR #48; CI run 37713513237 passed: 286 unit / 234 integration / 307 frontend / 76 E2E | [evidence.md](evidence.md) |

## Planned for next period

On the user's approval only: plan revision 4 — merge PR #48, delete the branch, closeout documentation (README, `ai/project.md`, CLAUDE.md).

## Risks and issues

| Issue / blocker | Owner | Since | Impact |
| --- | --- | --- | --- |
| TC-469 Not run (no spreadsheet app) | ThongTM | 2026-10-07 | Excel display unverified by a real application; the plant-calendar specs that fail locally passed in CI |

## Next action

User reviews PR #48 and decides on merge and closeout (plan revision 4), owner ThongTM.
