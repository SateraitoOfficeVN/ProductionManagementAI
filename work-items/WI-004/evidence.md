# Production Dashboard (Screen C) — Requirements Traceability & Evidence

As of `master` at `cd3a3b9`, the squash merge of PR #15, 2026-09-22.

## Traceability matrix

Design columns were filled as each document was written; code and test columns were filled in plan revision 3. 003_DD maps every REQ to test viewpoints: REQ-028 TC-201/214/215/216; REQ-029 TC-203; REQ-030 TC-204; REQ-031 TC-205; REQ-032 TC-206; REQ-033 TC-207/208; REQ-034 TC-209; REQ-035 TC-210; REQ-036 TC-211; REQ-037 TC-212; REQ-038 TC-213; REQ-039 TC-202; plus TC-217 (accessibility), TC-218 (seed), TC-219 (indexes), TC-220 (SP), TC-221 (query string ignored).

| Requirement ID | Requirement | Design artifact | Code | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| REQ-028 | Dashboard is the landing page at `/`; error and empty states | 003_BD; 003_DD module 1; 003_DD-SPD §1–§2 | `DashboardPage`, `App.tsx` route, `DashboardService` | TC-201, TC-214, TC-215, TC-216 | verified |
| REQ-029 | Status count tiles | 003_BD items 7–11 | `StatusTiles`, 003_DB Q1 in `DashboardReader`, `DashboardMapper` | TC-203 | verified |
| REQ-030 | Overdue and due-soon groups | 003_BD D-01, D-02 | `AttentionList`, Q3a/Q3b | TC-204 | verified |
| REQ-031 | Workload chart by due week | 003_BD D-03; DEC-009, DEC-010 | `BarChart`, Q2, `DashboardMapper.ToWorkload` | TC-205, TC-217 | verified |
| REQ-032 | Top 10 products by open quantity | 003_BD D-04 | `TopProducts`, Q4 | TC-206 | verified |
| REQ-033 | Completion time recorded | 001_BD v7; 001_DD v4; 003_DB | `ProductionOrder.Update`, `AddProductionOrderCompletionTracking`, `SeedDashboardDemoHistory` | TC-207, TC-208, TC-218 | verified |
| REQ-034 | Completed this week / month | 003_BD D-05, D-06 | Q5, `DeliveryTiles` | TC-209 | verified |
| REQ-035 | On-time rate, 30 days | 003_BD D-07, M-13 | Q5, `formatRate` | TC-210 | verified |
| REQ-036 | Completion trend, 12 weeks | 003_BD D-08 | Q6, `DashboardMapper`, `BarChart` | TC-211, TC-217 | verified |
| REQ-037 | Average lead time, 30 days | 003_BD D-09, M-14 | Q5, `DashboardMapper` rounding, `formatLeadTime` | TC-212 | verified |
| REQ-038 | Admin/Operator only | 003_BD 0-1; 003_DD-API | `DashboardController`, `SystemController` (`ProductionOrderEditor`), client role gate | TC-213 | verified |
| REQ-039 | Read-only, no drill-down | 003_BD §6; DEC-006 | No links in widgets; `SET TRANSACTION READ ONLY` in the reader | TC-202, TC-214 | verified |
| REQ-040 | Navbar on every authenticated screen | 003_BD shared header; DEC-016 | `AppHeader`, `AppNavbar`, `lib/navigation.ts` | TC-222, TC-220 | verified |
| REQ-041 | Server and database health indicator | 003_BD HS-01–HS-05; 003_DD-API §2; 003_DD-FN §6–§7 | `SystemHealthService`, `DatabasePing`, `SystemController`, cookie events, `useSystemHealth`, `HealthIndicator` | TC-224, TC-225, TC-226 | verified |
| REQ-042 | Maximize and restore a chart | 003_BD items 27–29; DEC-018 | `ChartDialog`, `BarChart` | TC-227 | verified |
| REQ-019 (WI-002, extended) | Confirm before discarding changes on any in-app link | 001_BD v7 E-07a; DEC-022 | `NavigationGuardProvider`, `GuardedLink`, `ProductionOrderForm` registration | TC-223 | verified |

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| 2026-09-22 | design-consistency checklist (003_BD scope) | manual review | local | pass for BD-level items; one defect found and fixed (below) | walk below |
| 2026-09-22 | BD review | user review of 003_BD | — | pass — approved ("003_BD is approved, move on to 003_DB") | status.md |
| 2026-09-22 | Seed simulation | Python script over 003_DB's seed tables and WI-003's parsed seed, for each run weekday and three days of the month | local | pass after one fix — first draft left week 6 empty on Monday/Tuesday runs; order #44 moved to +42 | 003_DB "What the seed produces" |
| 2026-09-22 | design-consistency checklist (003_DB and Screen A amendments scope) | manual review | local | pass — walk below | this file |
| 2026-09-22 | DB review | user review of 003_DB and the Screen A amendments | — | pass — approved ("003_DB is approved, move on to the DD") | status.md |
| 2026-09-22 | Mockup render check | one headless Edge screenshot of the mockup | local | pass after one fix — workload labels "Overdue"/"This week" collided and tile window captions wrapped mid-date; bar slots widened and captions put on their own line | — |
| 2026-09-22 | design-consistency checklist (003_DD set scope) | manual review | local | pass — walk below | this file |
| 2026-09-22 | Mockup published | Artifact publish, private | claude.ai | done — https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot (8 artboards) | 003_DD |
| 2026-09-22 | DD review (first pass) | user review of the 003_DD set and mockup v1 | — | changes requested — navbar, health indicator, chart maximize (DEC-016–DEC-018); handled by plan revision 2 | decisions.md |
| 2026-09-22 | Mockup v2 render check | one headless Edge full-page screenshot | local | pass — first capture blank (anchor jump before render, a capture issue), retaken without the anchor; one caption clarified | — |
| 2026-09-22 | Mockup v2 published | Artifact republish, private, same URL | claude.ai | done — version 2, 11 artboards | 003_DD |
| 2026-09-22 | design-consistency checklist (plan revision 2 amendments) | manual review | local | pass — walk below | this file |
| 2026-09-22 | Icons added (DEC-023) | 003_BD v3 M-21, 003_DD v4, mockup v3 with Lucide 1.47.0 glyphs inlined; one render check | local | pass | — |
| 2026-09-22 | Mockup v3 published | Artifact republish, private, same URL | claude.ai | done — version 3 | 003_DD |
| 2026-09-22 | Amended design review | user review of 003_BD v3, 001_BD v7, 002_BD v4, the 003_DD set and mockup v3 | — | pass — approved ("the DD is approved, move on to implementation") | status.md |
| 2026-09-22 | Migrations against the local Compose database (fresh volume) | `dotnet ef database update …` as the owner | local Compose | pass — all migrations applied; 124 orders (35/25/56/8); both checks present; six `production_orders` indexes, none INVALID; counter 124; SQL spot-check 23 of 30 on time, 13.7 days | — |
| 2026-09-22 | Endpoint smoke run | API against the migrated database; signed-in `curl` of `/api/system/health` and `/api/dashboard` | local | pass — health `ok` with `no-store`; dashboard figures equal 003_DB's prediction and the mockup | — |
| 2026-09-22 | Backend build | `dotnet build src/backend/ProductionManagementAI.slnx` | local, .NET SDK 10 | pass — 0 warnings, 0 errors | — |
| 2026-09-22 | Backend unit | `dotnet test src/backend/ProductionManagementAI.slnx` | local | pass — 148/148 (116 existing + 32 new) | final run |
| 2026-09-22 | Backend integration | same command | local, Testcontainers `postgres:17`, app as `pmai_app` | pass — 85/85 (63 existing, 5 of them updated to the new seed, + 22 new). First run 83/85: TC-225 found the security-stamp renewal path (fixed, DEC-025) and a WI-003 sort test assumed one page held the whole seed (fixed to read every page) | final run |
| 2026-09-22 | Frontend lint | `npm run lint` | local, oxlint | pass — no findings (three warnings from the first draft fixed, not suppressed) | — |
| 2026-09-22 | Frontend build | `npm run build` | local, tsc + Vite | pass | — |
| 2026-09-22 | Frontend unit | `npm test` | local, Vitest + RTL + vitest-axe | pass — 87/87 (56 existing + 31 new) | — |
| 2026-09-22 | E2E | `npx playwright test` | local Compose stack; Playwright Chromium desktop + Pixel 7 | pass — 22/22 (16 existing + 6 new). First run 19/22: axe found a real contrast failure (Top products caption 4.39:1, fixed to `gray-600`), and a WI-003 journey expected "In progress" on page 1, which the new seed no longer puts there (assertion moved to "Completed") | `tests/e2e/playwright-report/` |
| 2026-09-22 | Compose config | `docker compose -f deploy/compose.yaml config` | local | pass | — |
| 2026-09-22 | Dependency audit | `npm audit --omit=dev` after adding `lucide-react@1.47.0` (exact pin, ISC, only peer dependency React) | local | pass — 0 vulnerabilities | — |
| 2026-09-22 | Secret scan of the branch diff | the three local `.env` secrets searched in `git diff master` without printing them | local | pass — no credential in the diff. The local owner password is a dictionary word that also occurs as ordinary text (for example inside "postgresql"); the two other secrets do not occur | — |
| 2026-09-22 | design-consistency, security-review, delivery checklists (implementation) | manual review | local | pass — walks below | this file |
| 2026-09-22 | CI, run 1 (PR #15) | GitHub Actions | ubuntu-latest | pass — Backend 1m4s, Frontend 28s, E2E 2m33s | https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/35711772988 |

## Design-consistency walk — 003_BD scope

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — REQ-028–REQ-039 each with success and failure criteria; UC-008–UC-011 |
| BD covers navigation, primary actions and exceptions | pass — screen transition from login and to SCR-001/SCR-002; page actions; 401, 403, load failure, empty system and no-recent-completion flows |
| DD agrees with BD | not yet applicable — 003_DD not written |
| API and DB mappings agree | not yet applicable — 003_DB and 003_DD-API not written; 003_BD states what each must provide |
| Missing decisions resolved before dependent work | pass — DEC-008 and DEC-009 (user) settled before 003_BD; DEC-010–DEC-012 recorded; one technical question (how the snapshot read is made consistent) deferred to 003_DB and listed in 003_BD's open questions |
| Test scenarios map to the design | partly — each metric has one definition (D-01–D-09) to derive test viewpoints from in 003_DD |
| Security-relevant fields identified | pass — no input, no PII; completion time is server-written only; role gate server-side |
| Accessibility captured | pass — region headings, SVG `role="img"` with summaries, printed bar values, table equivalents, non-text contrast |
| Migration impact described | not yet applicable — 003_DB |
| Tracing/logging specified | deferred to 003_DD-FN, as 002_BD did |

**Defect found and fixed:** the brief's REQ-028 said the application header leads to the list and New-order screens, but the header has no navigation. Satisfying it as written would visibly change SCR-001 and SCR-002, outside plan revision 1's scope. Resolved by DEC-012 (page actions on SCR-003) and the brief's wording corrected. A second gap was found while applying DEC-008: active orders due after the 8-week look-ahead fell into no workload bar; the user decided a "later" bar (DEC-009).

## Design-consistency walk — 003_DB and Screen A amendments scope

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — unchanged |
| BD covers navigation, primary actions and exceptions | pass — 001_BD v6 adds the completion rule with no new screen behavior |
| DD agrees with BD | pass for 001_DD v4 / 001_DD-FN v3 against 001_BD v6: the completion time is set in one place (entity step 5), in the same save, never from the request |
| API and DB mappings agree | pass — 003_DB maps every 003_BD item to a query and a proposed API field; 001_DD-API reviewed: no field added, none accepted. `completed_at_utc` is exposed by no endpoint |
| Missing decisions resolved | pass — seed growth asked of the user (DEC-013); DEC-014 and DEC-015 recorded; 003_BD's open question on the snapshot mechanism closed by DEC-015 |
| Test scenarios map to the design | pass at this level — 001_DD's new viewpoint covers REQ-033; seed properties are stated so 003_DD can derive tests; integration-test isolation from the seed is called out |
| Security-relevant fields identified | pass — no client input reaches any dashboard statement; the time zone is a server configuration value passed as a parameter; no new grant |
| Accessibility captured | not applicable at DB scope |
| Migration impact described | pass — three migrations with type, locking, deploy order, recovery limits and rollback; Screen B's changed demo totals listed |
| Tracing/logging specified | deferred to 003_DD-FN, as planned |

## Design-consistency walk — 003_DD set scope

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — unchanged |
| BD covers navigation, primary actions and exceptions | pass — unchanged |
| DD agrees with BD | pass — every 003_BD item (3–25), mapping (M-11–M-19) and event (E-20–E-23) has one home in 003_DD or a companion; D-01–D-09 are referenced, not restated; all four DD files exist and the main DD lists the three companions |
| API and DB mappings agree | pass — every 003_DD-API response field maps to a 003_DB query (Q1–Q6) or to the window; `DashboardWindow`'s parameters are exactly 003_DB's parameter table; one defect found and fixed: the current week's workload bucket starts at today, not Monday, so `weekStart` for it is `today` (003_DD-API 7.2, 003_DD-FN §4 step 2) |
| Missing decisions resolved | pass — none open. One implementation note (flat vs. nested row type for `SqlQuery`) recorded in 003_DD-FN without contract impact |
| Test scenarios map to the design | pass — 21 viewpoints, every REQ covered, calendar boundaries in plant time for week, month, 30-day window and trend; test-data isolation from the demo seed specified |
| Security-relevant fields identified | pass — no input; `no-store`; server-only completion time; no figure logged |
| Accessibility captured | pass — 003_DD "Accessibility", 003_DD-SPD §3–§6, TC-217 |
| Migration impact described | pass — in 003_DB; 003_DD names the three migrations and the deploy-order failure mode |
| Tracing/logging specified | pass — span `ProductionOrder.Dashboard`, counter `pmai.production_orders.dashboard_loaded`, `DashboardSnapshotFailed` (003_DD-FN "Observability") |

## Design-consistency walk — plan revision 2 amendments

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — REQ-040–REQ-042 added with success and failure criteria; UC-012; REQ-019's extension recorded against WI-002's ID rather than a new one |
| BD covers navigation, primary actions and exceptions | pass — the shared header is specified once in 003_BD and referenced from 001_BD/002_BD; health exception flows (database down, server unreachable, session expired while polling) added |
| DD agrees with BD | pass — 003_BD items 26–29, H-1–H-5, E-24–E-31, HS-01–HS-05 each have one home in 003_DD or a companion; 001_BD E-07a matches 003_DD-SPD §9. One defect found and fixed during the work: the claim that SCR-001's discard confirmation already covered router navigation was false (it guards Cancel only); corrected, then settled by the user as DEC-022 |
| API and DB mappings agree | pass — API-SYS-01 has two response fields, both produced by 003_DD-FN §6; no table read, so 003_DB unchanged (checked) |
| Missing decisions resolved | pass — DEC-019 and DEC-022 asked of the user; DEC-020, DEC-021 recorded; none open |
| Test scenarios map to the design | pass — TC-222–TC-227 added; TC-201/TC-202 updated for the navbar |
| Security-relevant fields identified | pass — health endpoint authenticated, two fixed fields, exception type only in logs, no-store, and no session renewal (TC-225); anonymous `/health` unchanged |
| Accessibility captured | pass — labelled `<nav>` with `aria-current`, SP menu with `aria-expanded` and Escape, polite live region announcing changes only, shape-coded status dots, native modal `<dialog>` with focus return |
| Migration impact described | not applicable — no schema change in this revision |
| Tracing/logging specified | pass — span `System.Health`, counter `pmai.system.health_checks{database}`, `DatabasePingFailed` Warning |

## Design-consistency walk — implementation (plan revision 3)

| Checklist item | Result |
| --- | --- |
| Requirements have stable IDs and acceptance criteria | pass — unchanged; every REQ traced above to code and tests |
| BD covers navigation, primary actions and exceptions | pass — implemented as designed; no behavior outside 003_BD/001_BD v7 |
| DD agrees with BD | pass — two implementation divergences found and recorded, documents updated in the same commits: the reader's call mechanism and the `Health` namespace (DEC-024, 003_DD-FN v3/003_DD X-1), and the session rule's second hook (DEC-025, 003_DD-FN v4). TC-214's verification method was corrected to match (003_DD) |
| API and DB mappings agree | pass — the smoke run and TC-218 show the endpoint returning exactly 003_DB's predicted figures; the response shape matches 003_DD-API, asserted in integration |
| Missing decisions resolved | pass — none open |
| Test scenarios map to the design | pass — TP-004: every TC-201–TC-228 maps to named, passing tests |
| Security-relevant fields identified | pass — see the security walk |
| Accessibility captured | pass — axe clean in jsdom and in a real browser (desktop and SP), with the dialog open; one real contrast failure found by E2E and fixed |
| Migration impact described | pass — 003_DB's three migrations applied on a fresh volume; the named-index pitfall caught at scaffold time (an unnamed second index on the same columns would have dropped 002_DB's index) and fixed before any migration ran |
| Tracing/logging specified | pass — span `ProductionOrder.Dashboard`, `System.Health`; counters `pmai.production_orders.dashboard_loaded`, `pmai.system.health_checks`; `DashboardSnapshotFailed`, `DatabasePingFailed` (type only) |

## Security-review walk — implementation

| Checklist item | Result |
| --- | --- |
| Every new or changed endpoint enforces auth | pass — `GET /api/dashboard` and `GET /api/system/health` carry `[Authorize(Policy = ProductionOrderEditor)]`; 401/403 asserted for both (TC-213). The anonymous liveness `/health` is unchanged |
| External input validated; no concatenated queries | pass — neither endpoint binds input (TC-221 shows a crafted query string has no effect). The reader's SQL is constant text; every value is an `NpgsqlParameter`, including the plant time zone. The two `IN ('Draft', 'InProgress')` fragments are compile-time constants |
| No credential in code, config, logs, evidence or diff | pass — secret scan above; `deploy/.env` is git-ignored and was copied locally only |
| New dependencies trusted and checked | pass — `lucide-react@1.47.0`: ISC, exact pin, React peer only, `npm audit` clean (DEC-023) |
| Least privilege | pass — no new role or grant; `pmai_app`'s table-level `UPDATE` covers the new column; the health ping reads no table |
| External content treated as data | not applicable — no external content consumed at run time |
| Errors leak no detail | pass — 500s use the global Problem Details handler (generic, MSG-E013); the health response has two fixed fields, and TC-224 asserts that a connection error's host text does not appear; `DatabasePingFailed` logs the exception type only |
| No sensitive data in logs or evidence | pass — no figure, order data or connection detail is logged |
| Trust boundary threat-modeled | pass — no new boundary. The session rule was reviewed against 0002_ADR's STRIDE table: suppressing renewal on one path can only shorten a session, never extend it, and the security stamp is still validated on that path (DEC-025) |

## Delivery walk — before push (plan revision 3)

| Checklist item | Result |
| --- | --- |
| Approved scope and plan revision identifiable | pass — plan revision 3, approved 2026-09-22 ("plan approved, let move on to implementation") |
| Design, code and tests agree with requirements | pass — traceability above; divergences recorded as DEC-024/DEC-025 with documents updated |
| Required checks recorded; not-run checks have reasons | pass — CI green on PR #15 (all three jobs) |
| Review findings and limitations explicit | pass — limitations: WI-001 DEC-015 (a read-only role cannot be expressed); TP-004's "not to be tested" list; Screen B's default first page now shows old completed orders, a consequence of the seed noted for the user |
| External operations within authorization | pass — local only: edits, commits, the local Compose stack, `npm install` of one pinned package, and republishing the private mockup (plan revision 2). No push, PR, merge or deploy |
| Status, decisions and evidence support continuation | pass |
| Close-out documents | not yet applicable — README, `ai/project.md` (including `lucide-react` in the confirmed stack) and `CLAUDE.md` follow the merge |
| No secret in diff, evidence, status, decisions or PR text | pass — so far; re-checked when the PR text is written |
| External content treated as data | pass |
| Flaky or skipped checks quarantined with reasons | pass — none skipped or quarantined; the two first-run failures in each of integration and E2E were fixed at their cause, not retried |
| New CI action or dependency pinned | pass — no CI change; `lucide-react` pinned exactly |
