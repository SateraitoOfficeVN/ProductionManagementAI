# demos

Four walkthroughs of Screen A's lifecycle (production-order create/edit, WI-002). Each directory holds a scenario description (`README.md`). A recorded run is stored as a subfolder per work item, with a written record of what happened (input files, prompt, tool used, start revision, outputs, observed checks) and its video.

Recording status:

- **[01-basic-design](01-basic-design/README.md), [03-database-design](03-database-design/README.md), [02-detailed-design](02-detailed-design/README.md):** recorded during WI-002's *first* design pass (2026-09-16, run in BD → DB → DD order). That pass was scrapped when the BD/DD templates were rewritten, and WI-002 restarted on 2026-09-18. The recordings and their notes are kept locally by the project owner and are not committed; videos (`*.mp4`) are gitignored. The restarted run's outcome is in `work-items/WI-002/`.
- **[04-implementation](04-implementation/README.md):** WI-002's implementation has run (plan revision 2, merged via PR #3), but no walkthrough recording is in the repository.

`work-items/WI-002/` (plan revisions, decisions, status, evidence) is the authoritative written record of every step, whether or not a video exists.
