# WI-008 — Decision log

| ID | Date | Decision | Maker | Status | Rationale |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | 2026-09-30 | Track the two application defects in a new bug-fix work item | User, latest message | decided | Recording alone stays outside project scope; application defects require the project bug-fix workflow |
| DEC-002 | 2026-09-30 | Preserve completed and approved design documents | User, standing instruction | decided | Create a new amendment if necessary; do not revise old baselines |
| DEC-003 | 2026-09-30 | Center both Product master dialogs using existing native-dialog conventions | User continuation instruction after approval correction | accepted for continuation | Both dialogs lack explicit centering; confirm geometry before applying the minimal fix |
| DEC-004 | 2026-09-30 | Use `Package` as `NavProductsIcon`; keep WI-003's `ClipboardList` | User continuation instruction after approval correction | accepted for continuation | Separate product identity from production-order list navigation without adding a dependency |

DEC-001 authorizes creating the new work-item records. It does not constitute
approval of a plan revision that had not yet been presented. The agent incorrectly attributed initial plan approval to an acknowledgment.
The user explicitly stated that the plan had not been approved. The subsequent
continuation instruction permits continuing the existing WI-008 scope from then
onward; it does not retroactively authorize the earlier work.

All new project records are English; Japanese UI labels remain Japanese.

## DEC-005 — Correct the approval history

On 2026-09-30, the user challenged the premature implementation. The agent
acknowledged the mistaken approval interpretation and stopped. The user then
explicitly instructed continuation of WI-008. Retain actual code, test results
and local commit `a628f2e`; correct the records without inventing earlier approval.
No external operation or new scope is authorized by this continuation.

## DEC-006 — Push and PR authorization

On 2026-09-30, the user explicitly requested pushing WI-008 and creating its PR.
The existing branch was pushed and PR #33 opened into master. This request does
not authorize merge, publishing or deployment.
