# 04-implementation

Implement screen A from reconciled design. Show checks, review and authorized CI/deployment evidence.

Prerequisites: agreed screen A, approved work item plan and relevant artifacts. Screen A is confirmed as production-order create/edit (WI-002; roadmap locked in `ai/project.md`).

## Recorded run — WI-002, 2026-09-18

| File | What it is |
| --- | --- |
| `WI-002/ScreenA_Implementaion.mp4` | The unedited session, 50 min 44 s (gitignored, kept locally) |
| `WI-002/transcript.md` | Step-by-step record of the session, with the output of every step |
| `WI-002/ScreenA_Implementaion_edited.mp4` | 5 min 59 s jump-cut version for the slides (gitignored, kept locally) |
| `WI-002/ScreenA_Implementaion_presentation.pdf` | 15-slide deck, step numbers matching the edited video |

Six prompts and one four-question form produced the whole of Screen A. The shape is unlike the other three recordings: three quarters of the agent's working time is a **single 34-minute turn** running domain → application → infrastructure → API → frontend → E2E unattended, so the interesting material sits at both ends — the harness work and the plan negotiation before it, and the merge reconciliation after it.

WI-002's implementation ran as plan revision 2 (`work-items/WI-002/plan.md`) and was merged via PR #3:

- backend, frontend and DB migration;
- restricted runtime database login (DEC-016);
- unit, integration, frontend and Playwright E2E tests with axe checks;
- security-review and delivery checklists.

CI now runs the backend, frontend and e2e jobs on every PR. The written record (inputs, decisions, commands, results) is in `work-items/WI-002/` (`plan.md`, `decisions.md`, `evidence.md`, `test-plan.md`).
