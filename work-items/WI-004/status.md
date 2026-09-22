# Production Dashboard (Screen C) — Status Report

As of 2026-09-22. Work item state: in-progress.

## Overall status

**RAG:** Green — on track
WI-004 was opened on 2026-09-22 after WI-003 (Screen B) was merged to `master`. The user settled the dashboard's scope
(DEC-001–DEC-007): all four current-state widgets, completion tracking with all four history metrics, the dashboard
replacing `/`, read-only, and a 7-day due-soon window. Plan revision 1 (design phase) was approved; the user then set
the metric windows and sizes (DEC-008) and chose a "later" workload bar (DEC-009). BD-003 is approved. DB-004 and the Screen
A design amendments for completion tracking are approved (the user chose to both re-date and extend the demo seed,
DEC-013). The DD-003 set is drafted and awaiting review; its mockup is published privately.

## Approved plan reference

[plan.md](plan.md) — approved revision 1, approval source user message 2026-09-22: "plan revision 1 is approved, let answer the DEC-008".

## Accomplishments this period

| Date | Milestone / deliverable | Evidence link |
| --- | --- | --- |
| 2026-09-22 | DD-003, DD-003-API, DD-003-FN, DD-003-SPD and the mockup source drafted; design-consistency walk at DD scope | [DD-003](../../docs/en/020_detailed-design/DD-003-production-dashboard.md), [evidence.md](evidence.md) |
| 2026-09-22 | DB-004 and the Screen A amendments approved by the user | [evidence.md](evidence.md) |
| 2026-09-22 | DB-004 version 1 and the Screen A amendments (BD-001 v6, DD-001 v4, DD-001-FN v3, DD-001-API v2) drafted; DEC-013–DEC-015 recorded | [DB-004](../../docs/en/database/0004-completion-tracking-and-dashboard-queries.md), [evidence.md](evidence.md) |
| 2026-09-22 | BD-003 approved by the user | [evidence.md](evidence.md) |
| 2026-09-22 | BD-003 version 1 drafted; DEC-010–DEC-012 recorded; design-consistency walk at BD scope | [BD-003](../../docs/en/010_basic-design/BD-003-production-dashboard.md), [evidence.md](evidence.md) |
| 2026-09-22 | Plan revision 1 approved; DEC-008 and DEC-009 decided; branch `feature/WI-004-production-dashboard` created in worktree `../WMS-worktrees/WI-004` | [plan.md](plan.md), [decisions.md](decisions.md) |
| 2026-09-22 | WI-004 opened; brief revision 1 (REQ-028–REQ-039, UC-008–UC-011), decision log, plan revision 1 | [brief.md](brief.md), [decisions.md](decisions.md), [plan.md](plan.md) |

## Planned for next period

After the DD-003 set is approved: close the design phase (step 7), then draft plan revision 2 and show it for approval
(step 8).

## Risks and issues

| Issue / blocker | Owner | Since | Impact |
| --- | --- | --- | --- |
| WI-001 DEC-015 (role/permission matrix) still open | ThanhTN | 2026-09-16 | The dashboard reuses the `ProductionOrderEditor` policy, so a read-only viewer role cannot be expressed |

## Next action

User review of the DD-003 set.
