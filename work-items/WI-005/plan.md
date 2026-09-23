<!-- Based on ai/templates/plan.md. -->

# Japanese UI and automobile-parts domain — Implementation Plan

Revisions are kept in full and in chronological order (oldest first), so the plan can be back-tracked. The last revision is the current one.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-23 | Design updates, implementation and tests | **current** — approved | ThanhTN, 2026-09-23: "approved" (reply to revision 1 as shown) |

## Revision 1 — design updates, implementation and tests

Revision 1, 2026-09-23.

### Objective

Deliver REQ-043–REQ-048 (brief.md): a Japanese UI on every screen, demo data in the automobile-parts domain, and design
documents that describe the Japanese screens, with no functional change.

### Scope

#### In scope

- Frontend text: every component, the message catalog, page titles, `aria` text, the login page; date/number display;
  `lang="ja"` and font stack.
- Demo data: 30 product names and seeded notes (new migration), project description.
- Design documents: 001_BD–003_BD, 001_DD–003_DD with their companions, 001_DB (seed table), the wireframe SVGs, the
  mockup HTML sources; regenerated English and Japanese PDFs.
- Harness rule for quoting UI text (DEC-003), as an amendment to RFC 0008.
- Tests: expected text updated in unit, integration and E2E suites.

#### Out of scope

- Any behavior change; an English UI or switcher; renaming identifiers (brief "Not doing").
- `demos/`. (Republishing the mockup Artifacts was out of scope at approval; the user asked for it on 2026-09-23 and it is done, see Resources and external actions.)

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| brief.md | 1 | — |
| decisions.md DEC-001–DEC-003 | decided | — |
| decisions.md DEC-004–DEC-008 | proposed | Approving this revision approves them as written; any change requested at review is applied before step 2 |
| 001_BD–003_BD, 001_DD–003_DD (+ companions), 001_DB–003_DB | current in `docs/en/` | — |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Confirm the glossary and part names (DEC-002, DEC-004–DEC-008) at this review | none | requirements | decisions.md | User approval of this revision | Done: approved with revision 1, 2026-09-23 |
| 2 | Harness: quoting rule (DEC-003) in `ai/rules/documentation.md`, RFC 0008 amended; `ai/project.md`, `README.md`, `CLAUDE.md` describe the automobile-parts domain and the Japanese UI | 1 | harness-improvement | rule, RFC, project docs | Review of the diff | Done 2026-09-23: quoting rule in `ai/rules/documentation.md` and `ai/templates/basic-design.md`; RFC 0008 Amendment 1; `ai/project.md`, `README.md`, `CLAUDE.md` describe the domain and the Japanese UI |
| 3 | Design documents: BD and DD quote the Japanese text with English gloss; value mappings (status labels, M-04, M-10, M-13–M-19) and message catalogs updated; 001_DB product seed table; revision rows added | 1 | basic-design, detailed-design, database-design | `docs/en/010_basic-design/`, `020_detailed-design/`, `database/001/` | design-consistency checklist; every quoted text matches the catalog | Done 2026-09-23: 3 BD, 9 DD (all but 001_DD-FN and 003_DD-FN, which have no UI text), 001_DB and 002_DB revised; every 「…」 quote checked against the catalog (evidence.md); design-consistency checklist passed |
| 4 | Wireframes redrawn with Japanese UI text; mockup HTML sources updated | 3 | screen-design | `…/###/wireframes/*.svg`, `…/###/mockups/*.html` | SVGs rasterized and inspected | Done 2026-09-23: 13 SVG wireframes regenerated with Japanese UI text and rasterized for inspection; 3 mockup HTML sources converted (`lang="ja"`, Japanese font, UI strings; captions stay English) |
| 5 | Frontend: `labels` catalog, every component reads from it, `lang="ja"`, font stack, ja-JP formatting (DEC-006) | 1 | implementation | `src/frontend/` | `npm run lint`, `npm run build` | Done 2026-09-23: `labels` catalog, `src/lib/format.ts`, 20 components, `lang="ja"`, font stack; lint and build clean |
| 6 | Backend: `ProductSeed` names, migration `LocalizeDemoDataToJapanese` (DEC-008); no API change | 1 | implementation, database-design | `src/backend/` | `dotnet build` | Done 2026-09-23: migration `20260923053923_LocalizeDemoDataToJapanese` (30 `UpdateData` + guarded notes SQL, reversible); build clean |
| 7 | Tests: update expected text in Vitest, xUnit and Playwright; add a check that no English UI text remains in the catalog's consumers (REQ-043) | 5, 6 | testing | `src/frontend/tests/`, `tests/` | `npm test`; `dotnet test`; Compose + `npx playwright test` (incl. axe, SP profile) | Done 2026-09-23: Vitest 129, xUnit 148 + 86 (new TC-301, TC-302), Playwright 22 of 22 incl. axe and SP (evidence.md) |
| 8 | PDFs: re-render all English and Japanese PDFs | 3, 4 | — | `docs/en/pdf/`, `docs/ja/pdf/` | No unrendered diagram; spot check | Done 2026-09-23: 30 PDFs (15 documents × EN/JA) re-rendered; 0 unrendered diagrams; the 8 PDFs of the 4 unchanged documents were left as they were |
| 9 | Evidence, status, traceability | 7, 8 | testing | evidence.md, status.md | delivery checklist | Done 2026-09-23: evidence.md, status.md; delivery checklist items that need commit/PR remain open (not authorized) |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author, implementer | this agent |
| Reviewer, approver | ThanhTN |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local edits, builds, tests, local Docker Compose (applying the new migration as the owner) | yes, on approval of this revision | this revision | local machine |
| Branch and commit | yes | ThanhTN, 2026-09-23: "create the branch and commit" | a `feature/WI-005-japanese-ui` branch, local only |
| Push, PR, merge | no | not yet authorized | — |
| Republish mockup Artifacts | yes | ThanhTN, 2026-09-23: "also update the mock up artifact that have been published those still in english" | the three existing mockup URLs only |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| Glossary or part names not accepted | Review comments | Apply before step 2; record in decisions.md |
| Japanese text breaks a layout (longer or wider labels) | SP E2E or visual check fails | Adjust spacing only; no behavior change; record |
| Tests or E2E selectors depend on English text | Failures in step 7 | Update the expectation, never the behavior; a failure that isn't text-only is investigated per ai/policies.md |
| The demo migration touches user-created data | Review of the migration | Match seed rows by fixed ID prefixes only (DEC-008) |
| Scope grows into behavior changes | Any requirement beyond REQ-043–REQ-048 | Pause and ask |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** ThanhTN, 2026-09-23, "approved", replying to plan revision 1 after it was shown, together with DEC-002 and DEC-004–DEC-008 as proposed
- **Approved revision:** 1, 2026-09-23
- **Closure:** —
