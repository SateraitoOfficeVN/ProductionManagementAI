<!-- Based on ai/templates/evidence.md. -->

# Japanese UI and automobile-parts domain — Requirements Traceability & Evidence

As of source revision/commit `ba49b96` plus uncommitted work (committed on `feature/WI-005-japanese-ui`), 2026-09-23.

## Traceability matrix

| Requirement ID | Requirement | Design artifact | Code / PR | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| REQ-043 | Every UI text Japanese | 001_BD–003_BD, 001_DD–003_DD | `messages.ts` (`labels`), every component | TC-301 (source scan); all unit and E2E text assertions | done |
| REQ-044 | Japanese date/number display | 001_DD M-04, 002_DD M-10, 003_BD/003_DD M-13–M-19 | `src/lib/format.ts`, `dashboardFormat.ts` | `dashboardFormat.test.ts`, list and dashboard unit tests | done |
| REQ-045 | `lang="ja"`, font, layouts intact | 001_DD–003_DD layout | `index.html`, `index.css` | E2E SP profile (3 cases), axe on every screen | implemented, tested |
| REQ-046 | Automobile-parts demo data in Japanese | 001_DB, decisions DEC-002, DEC-008 | `ProductSeed.cs`, migration `LocalizeDemoDataToJapanese` | TC-302; migration applied to the existing Compose volume | done |
| REQ-047 | Design documents describe the Japanese UI | 001–003_BD, 001–003_DD sets, 001_DB, 002_DB, wireframes, mockups, PDFs | — | quote-versus-catalog check; design-consistency checklist; user review pending | done, awaiting review |
| REQ-048 | No functional change | REQ-010–REQ-042 designs | — | existing suites, unchanged except for expected text | passing |

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| 2026-09-23 | Frontend lint, type check, build | `npm run lint`; `npm run build` | local | pass (0 findings) | — |
| 2026-09-23 | Frontend unit (incl. TC-301) | `npm test` | local, jsdom | pass: 129 of 129 in 10 files | — |
| 2026-09-23 | Backend unit + integration (incl. TC-302) | `dotnet test src/backend/ProductionManagementAI.slnx` | local, Testcontainers Postgres 17 | pass: 148 of 148 unit, 86 of 86 integration | — |
| 2026-09-23 | Migration on an existing demo database | `dotnet ef database update` as owner, then `psql` counts | Compose `db` volume from WI-004 (no wipe) | pass: P-1001 ブレーキキャリパー, P-1030 ボルト・ナットキット; 16 notes rewritten, 0 `Demo order` left; 124 orders | — |
| 2026-09-23 | Every 「…」 quote in `docs/en` matches the catalog | scratch script comparing each quote's fixed fragments with `messages.ts` | local | pass: 9 quotes are runtime compositions of catalog pieces (health line, chart summaries, caption, 「{n}件に基づく」), each checked against the code by hand | — |
| 2026-09-23 | Wireframes | regenerated 13 SVGs, rasterized with headless Chrome, inspected | local | pass after two fixes (collapsed spaces in one row, overflowing SP note) | — |
| 2026-09-23 | PDFs | `scripts/docs-pdf.py` for 15 changed documents × EN/JA; text extraction for raw Mermaid | local, Chrome, Mermaid 11.17.2 | pass: 30 PDFs, 0 unrendered diagrams | — |
| 2026-09-23 | PDF layout fix after user review ("some diagram and chart got page cut off") | `scripts/docs-pdf.py`: diagrams and images capped at 160 mm and kept on one page, the line introducing a figure kept with it, Mermaid measuring labels in the page font with wider flowchart spacing; 001_BD's screen transition drawn with two longer edges; all 38 PDFs re-rendered | local | pass: page-edge scan finds no cut figure (was: 001_DB ER diagram split over pages 4–5 in both languages); contact sheets of every figure page inspected | — |
| 2026-09-23 | E2E incl. axe and SP | `npx playwright test` | Compose stack, rebuilt | pass: 22 of 22 (desktop 19, mobile 3) | — |

## Defects, failures and blockers

| Item | Reason | Blocker | Follow-up |
| --- | --- | --- | --- |
| TC-301 false positives in its first run | TypeScript return types (`=> Promise<`) read as JSX text; `PO-2026-…` placeholder read as a word | none | Patterns tightened; the scan still flags the English text of the pre-WI-005 components (checked against `HEAD`) |

| 003_DD said the `ready` state announces "Dashboard updated" | Pre-existing gap from WI-004: the implementation announces the snapshot-time line itself | none | Corrected the DD to match the code (no behavior change, REQ-048) |
| 002_BD's four-column message table collapsed by the Japanese-source converter | Converter matched every `MSG-` row | none | Restored from the backup and converted cell by cell |

## External references

- PR: not opened — not authorized
- CI run: not applicable
- Mockup Artifacts republished 2026-09-23 from the updated sources, same URLs: Screen A https://claude.ai/artifact/FEo1RG27UjZ6vxFCUjxoHq (version 2), Screen B https://claude.ai/artifact/2XrZ9xnEfzQ6pCbUnZovL3 (version 2), Screen C https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot (version 4)
- Japanese editions of the mockups (page chrome and captions in Japanese), published 2026-09-23 on the user's request and linked from the Japanese PDFs of 001_DD, 002_DD and 003_DD: Screen A https://claude.ai/artifact/QY4W3yuMnnBXxBkN2XioiD, Screen B https://claude.ai/artifact/TTxzX1PJVyKr3aLYpHSRZK, Screen C https://claude.ai/artifact/BPRHgKEpaYBnZMCzCsUBJz. Their sources are committed beside the English mockups as `mockups/*.ja.html` (user's request; RFC 0008 Amendment 2), and the English DDs link to them too
- Deployment: not applicable

## Remaining limitations and next action

All plan steps are done locally and verified. Committed on `feature/WI-005-japanese-ui` and pushed; PR open. Not done, because not authorized: merge. The three mockup Artifacts were republished on the user's request. The Japanese translation sources for the PDFs are not committed (RFC 0008). Next action: the user reviews the work and decides on branch/commit.
