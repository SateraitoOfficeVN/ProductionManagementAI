# 02-detailed-design

Create screen A DD from BD. Show processing, validation and pending DB/API reconciliation.

Prerequisites: agreed screen A, approved work item plan and relevant artifacts. Screen A is confirmed as production-order create/edit (WI-002; roadmap locked in `ai/project.md`).

This step was recorded after 03-database-design, which is the order the work actually ran in (BD → DB → DD).

## Recorded run — WI-002, 2026-09-18

| File | What it is |
| --- | --- |
| `WI-002/ScreenA_DD.mp4` | The unedited session, 15 min 37 s (gitignored, kept locally) |
| `WI-002/transcript.md` | Step-by-step record of the session, with the output of every step |
| `WI-002/ScreenA_DD_edited.mp4` | 4 min 00 s jump-cut version for the slides (gitignored, kept locally) |
| `WI-002/ScreenA_DD_presentation.pdf` | 16-slide deck, step numbers matching the edited video |

Two prompts and one authorization question produced DD-001 and its DD-001-API companion, a rendered mockup published as a private artifact, and four decisions (DEC-021 to DEC-024) — about 10.5 minutes of agent work. The session's turning point is the user's second prompt, which pointed out that two of the four DD templates had produced no document; Claude saved that as a standing preference before writing the missing DD-001-FN and DD-001-SPD, and the proposal to change the shared templates became harness RFC `ai/improvements/0001-dd-companions-always-produced.md`.

The artifacts themselves are in `docs/en/020_detailed-design/`, with the decisions in `work-items/WI-002/decisions.md`.
