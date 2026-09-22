# Production Dashboard (Screen C) — Status Report

As of 2026-09-22. Work item state: in-progress.

## Overall status

**RAG:** Green — on track
WI-004 was opened on 2026-09-22 after WI-003 (Screen B) was merged to `master`. The user settled the dashboard's scope
(DEC-001–DEC-007): all four current-state widgets, completion tracking with all four history metrics, the dashboard
replacing `/`, read-only, and a 7-day due-soon window. Plan revision 1 (design phase) was approved; the user then set
the metric windows and sizes (DEC-008) and chose a "later" workload bar (DEC-009). BD-003 is approved. DB-004 and the Screen
A design amendments for completion tracking are approved (the user chose to both re-date and extend the demo seed,
DEC-013). The DD-003 set was drafted and its mockup published privately; on reviewing the mockup the user asked for a navbar
on every screen, a server/database health indicator and chart maximize (DEC-016–DEC-018). Plan revision 2 (design
amendment) is complete and the design phase is closed: the user approved the amended design set. Plan revision 3
(implementation, tests, PR) is drafted and awaiting review; no code has been written.

## Approved plan reference

[plan.md](plan.md) — approved revision 3, approval source user message 2026-09-22: "plan approved, let move on to implementation"; approved revision 2, approval source user message 2026-09-22: "ok revision 2 is approved"; before it, approved revision 1, approval source user message 2026-09-22: "plan revision 1 is approved, let answer the DEC-008".

## Accomplishments this period

| Date | Milestone / deliverable | Evidence link |
| --- | --- | --- |
| 2026-09-22 | Design phase closed (amended design set approved); plan revision 3 drafted | [plan.md](plan.md) |
| 2026-09-22 | Icons (DEC-023, user): BD-003 v3, DD-003 v4, mockup v3 republished | https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot |
| 2026-09-22 | Plan revision 2 steps 1–7: BD-003 v2, BD-001 v7, BD-002 v4, DD-003 set v2/v3, DD-001-SPD v2, DD-002-SPD v2, mockup v2 republished; DEC-019 and DEC-022 decided by the user, DEC-020 and DEC-021 recorded | [evidence.md](evidence.md), https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot |
| 2026-09-22 | Mockup review by the user: DEC-016 (navbar), DEC-017 (health indicator), DEC-018 (chart maximize); brief revision 2 and plan revision 2 drafted | [decisions.md](decisions.md), [plan.md](plan.md) |
| 2026-09-22 | Mockup published privately | https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot |
| 2026-09-22 | DD-003, DD-003-API, DD-003-FN, DD-003-SPD and the mockup source drafted; design-consistency walk at DD scope | [DD-003](../../docs/en/020_detailed-design/DD-003-production-dashboard.md), [evidence.md](evidence.md) |
| 2026-09-22 | DB-004 and the Screen A amendments approved by the user | [evidence.md](evidence.md) |
| 2026-09-22 | DB-004 version 1 and the Screen A amendments (BD-001 v6, DD-001 v4, DD-001-FN v3, DD-001-API v2) drafted; DEC-013–DEC-015 recorded | [DB-004](../../docs/en/database/0004-completion-tracking-and-dashboard-queries.md), [evidence.md](evidence.md) |
| 2026-09-22 | BD-003 approved by the user | [evidence.md](evidence.md) |
| 2026-09-22 | BD-003 version 1 drafted; DEC-010–DEC-012 recorded; design-consistency walk at BD scope | [BD-003](../../docs/en/010_basic-design/BD-003-production-dashboard.md), [evidence.md](evidence.md) |
| 2026-09-22 | Plan revision 1 approved; DEC-008 and DEC-009 decided; branch `feature/WI-004-production-dashboard` created in worktree `../WMS-worktrees/WI-004` | [plan.md](plan.md), [decisions.md](decisions.md) |
| 2026-09-22 | WI-004 opened; brief revision 1 (REQ-028–REQ-039, UC-008–UC-011), decision log, plan revision 1 | [brief.md](brief.md), [decisions.md](decisions.md), [plan.md](plan.md) |

## Planned for next period

After plan revision 3 is approved: implementation, tests, full local verification, gates, then the push and PR if
authorized.

## Risks and issues

| Issue / blocker | Owner | Since | Impact |
| --- | --- | --- | --- |
| WI-001 DEC-015 (role/permission matrix) still open | ThanhTN | 2026-09-16 | The dashboard reuses the `ProductionOrderEditor` policy, so a read-only viewer role cannot be expressed |

## Next action

User review of plan revision 3, including authorization to push and open the PR (step 14).
