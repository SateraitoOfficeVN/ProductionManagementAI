# WI-017 — Notes limit 500 → 1000 — Implementation Plan

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-24 | Design (batch) + implementation + tests, local only | **current** | User's direct request (DEC-001, DEC-006) |

## Revision 1

### Objective

Deliver REQ-090–REQ-092 ([brief.md](brief.md)) end to end from one instruction: dated design revisions, PDFs,
code, migration and tests, all local.

### Permitted actions

Edit files on local branch `feature/WI-017-notes-1000`; add an EF migration; run builds and tests locally
(Testcontainers). Not permitted: commit, push, PR, merge, deployment, changes to the running demo database.

### Steps

| # | Step | Deliverable | Verification | Outcome |
| --- | --- | --- | --- | --- |
| 1 | Find every place the limit lives (`grep` for 500/450/MSG-E006 in `docs/`, `src/`, `tests/`) | Impact table in evidence.md | All hits classified | done — evidence.md step 1 |
| 2 | Dated design revisions (batch, DEC-001/002): 001_BD, 001_DD, 001_DD-API, 001_DD-FN, 001_DD-SPD, 001_DB, 002_DD-API-CSV, 002_DD-FN-CSV; dated wireframes/mockups | `*_20261024.*` | design-consistency checklist; no stale "500" for notes | done — 8 docs, 3 SVG, 2 HTML |
| 3 | EN/JA PDFs of the eight dated documents | `docs/en/pdf/…_20261024.pdf`, `docs/ja/pdf/…_20261024.pdf` | Files produced, pages spot-checked | done — 16 PDFs, text-checked |
| 4 | Code: domain constant, EF config + migration, FE constant/counter threshold, message text | Source diff | Build + lint | done — frontend and backend builds pass (backend after SDK install, 2026-10-25) |
| 5 | Tests: update unit/integration/frontend boundaries to 1000/1001; test-plan.md | Test diff | `dotnet test`, `npm test` | done — 307 frontend, 286 unit, 234 integration pass |
| 6 | Evidence, status | evidence.md, status.md | — | done |
