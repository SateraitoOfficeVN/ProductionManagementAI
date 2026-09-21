# 03-database-design

Create screen A DB design. Show ERD, constraints and reconciliation with DD/API.

Prerequisites: agreed screen A, approved work item plan and relevant artifacts. Screen A is confirmed as production-order create/edit (WI-002; roadmap locked in `ai/project.md`).

This step was recorded directly after 01-basic-design and before 02-detailed-design, which is the order the work actually ran in (BD → DB → DD).

## Recorded run — WI-002, 2026-09-18

| File | What it is |
| --- | --- |
| `WI-002/ScreenA_DB.mp4` | The unedited session, 10 min 44 s (gitignored, kept locally) |
| `WI-002/transcript.md` | Step-by-step record of the session, with the output of every step |
| `WI-002/ScreenA_DB_edited.mp4` | 3 min 59 s jump-cut version for the slides (gitignored, kept locally) |
| `WI-002/ScreenA_DB_presentation.pdf` | 16-slide deck, step numbers matching the edited video |

Three one-line prompts and one four-question form produced DB-002 (216 lines), eight decisions (DEC-013 to DEC-020), a new requirement (REQ-019) and BD-001 revision 3 — about 4.5 minutes of agent work. The session's turning point is the user's second prompt, "Are there any open questions left?", which made Claude re-check its own artifacts and surface three gaps it had quietly decided for itself.

The artifacts themselves are in `work-items/WI-002/` and `docs/en/database/0002-production-order-schema.md`.
