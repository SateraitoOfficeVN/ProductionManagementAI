# Production Dashboard (Screen C) — Status Report

As of 2026-09-22. Work item state: in-progress.

## Overall status

**RAG:** Green — on track
WI-004 was opened on 2026-09-22 after WI-003 (Screen B) was merged to `master`. The user settled the dashboard's scope
(DEC-001–DEC-007): all four current-state widgets, completion tracking with all four history metrics, the dashboard
replacing `/`, read-only, and a 7-day due-soon window. Plan revision 1 (design phase) was approved; the user then set
the metric windows and sizes (DEC-008) and chose a "later" workload bar (DEC-009). BD-003 is drafted and awaiting review.

## Approved plan reference

[plan.md](plan.md) — approved revision 1, approval source user message 2026-09-22: "plan revision 1 is approved, let answer the DEC-008".

## Accomplishments this period

| Date | Milestone / deliverable | Evidence link |
| --- | --- | --- |
| 2026-09-22 | BD-003 version 1 drafted; DEC-010–DEC-012 recorded; design-consistency walk at BD scope | [BD-003](../../docs/en/010_basic-design/BD-003-production-dashboard.md), [evidence.md](evidence.md) |
| 2026-09-22 | Plan revision 1 approved; DEC-008 and DEC-009 decided; branch `feature/WI-004-production-dashboard` created in worktree `../WMS-worktrees/WI-004` | [plan.md](plan.md), [decisions.md](decisions.md) |
| 2026-09-22 | WI-004 opened; brief revision 1 (REQ-028–REQ-039, UC-008–UC-011), decision log, plan revision 1 | [brief.md](brief.md), [decisions.md](decisions.md), [plan.md](plan.md) |

## Planned for next period

After BD-003 is approved: DB-004 with the Screen A amendments (steps 4–5), then the DD-003 set and its mockup, each
reviewed by the user in turn.

## Risks and issues

| Issue / blocker | Owner | Since | Impact |
| --- | --- | --- | --- |
| WI-001 DEC-015 (role/permission matrix) still open | ThanhTN | 2026-09-16 | The dashboard reuses the `ProductionOrderEditor` policy, so a read-only viewer role cannot be expressed |

## Next action

User review of BD-003.
