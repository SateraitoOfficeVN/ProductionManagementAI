# Production Dashboard (Screen C) — Implementation Plan

Revisions are kept in full and in chronological order (oldest first), so the plan can be back-tracked. The last revision is the current one.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-22 | Design (brief → BD → DB → DD + mockup) | approved; steps 1–6 done, steps 7–8 superseded by revision 2 | user message 2026-09-22: "plan revision 1 is approved, let answer the DEC-008" |
| 2 | 2026-09-22 | Design amendment after mockup review (navbar, health indicator, chart maximize) and design-phase close | approved; complete; superseded by revision 3 | user message 2026-09-22: "ok revision 2 is approved" |
| 3 | 2026-09-22 | Implementation, tests, PR | **current** — approved; complete; PR #15 squash-merged as `cd3a3b9` (DEC-026) | user message 2026-09-22: "plan approved, let move on to implementation" |

## Revision 1 — design phase

Revision 1, 2026-09-22. First plan for WI-004 (Screen C, production dashboard), started after WI-003 was merged to `master`. It covers the design phase only. Implementation, tests and the PR come in revision 2, which will be drafted once these designs are reconciled and approved, and shown for its own approval before any of its steps start (`ai/policies.md`).

### Objective

Produce the reconciled design set for Screen C: the brief, 003_BD, 003_DB for completion tracking and the dashboard's aggregate queries, and 003_DD with its companions and a rendered mockup. It also covers the amendments that completion tracking needs in Screen A's 001_BD and 001_DD set. Every requirement in `brief.md` (REQ-028–REQ-039) must be traced.

### Scope

#### In scope

- Requirements brief and decision log (`work-items/WI-004/`).
- Basic design 003_BD (`docs/en/010_basic-design/003/003_BD_production-dashboard.md`): SCR-003 at `/`, one section per widget, PC and SP layouts, the chart approach and each chart's accessible text or table equivalent, and the screen transition from login.
- Database design 003_DB (`docs/en/database/003/003_DB_completion-tracking-and-dashboard-queries.md`): the `completed_at_utc` column and its constraint, backfill, seed extension (DEC-004), the aggregate queries with plant-timezone bucketing, and their index support.
- Amendments to Screen A's design for REQ-033: 001_BD (the `InProgress → Completed` transition records the completion time) and 001_DD / 001_DD-FN / 001_DD-API as far as they are affected. Screen A's visible behavior does not change.
- Detailed design 003_DD and its companions 003_DD-API, 003_DD-FN and 003_DD-SPD, each as its own file, plus a rendered mockup (`docs/en/020_detailed-design/`).
- Design-consistency reconciliation across all of the above, using `ai/checklists/design-consistency.md`.

#### Out of scope

- Application code, migrations, tests and the PR belong to plan revision 2.
- Everything under "Not doing" in `brief.md`.
- Any change to Screen A's or Screen B's visible behavior, API contract or business rules beyond REQ-033.
- Any push, PR, merge, image publication or deployment.

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| `work-items/WI-004/brief.md` | revision 1, 2026-09-22 | REQ-028–REQ-039 as written |
| `work-items/WI-004/decisions.md` DEC-001–DEC-007 | decided 2026-09-22 | — |
| DEC-008 (metric windows and sizes) | decided 2026-09-22 | — |
| 001_BD, 001_DD set, 001_DB (Screen A) | current `master` | Status labels, state machine, message conventions and the plant clock are reused, not restated |
| 002_BD, 002_DD set, 002_DB (Screen B) | current `master` | The overdue rule (M-08) and the 80-row demo seed are reused and extended |
| 0001_ADR (layered backend), 0002_ADR (auth/RBAC) | current `master` | The dashboard endpoint reuses the cookie auth and the `ProductionOrderEditor` policy (Admin or Operator) |
| `ai/templates/basic-design.md`, `detailed-design.md`, `DD/*`, `database-design.md` | current `master` | Unchanged since WI-003 |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Requirements brief and decision log | none | requirements | `work-items/WI-004/brief.md`, `decisions.md`, `status.md` | Every REQ has success and failure criteria; every scope answer recorded as a DEC | done 2026-09-22 — REQ-028–REQ-039, UC-008–UC-011; DEC-001–DEC-007 decided; DEC-008 proposed |
| 2 | Settle the metric windows and sizes (DEC-008) | 1 | — | `decisions.md`, brief updated | User answer recorded with rationale | done 2026-09-22 — DEC-008 decided; brief updated |
| 3 | Basic design 003_BD | 1, 2 | basic-design, screen-design | `docs/en/010_basic-design/003/003_BD_production-dashboard.md` | Every REQ-028–REQ-039 maps to a BD section; each metric's definition, window and empty state is stated once; chart approach and its accessible equivalent chosen and recorded as a DEC | done 2026-09-22 — 003_BD version 1 (SCR-003, FN-017–FN-023, D-01–D-09, M-11–M-19, E-20–E-23); DEC-009 (user) and DEC-010–DEC-012 recorded; approved by the user ("003_BD is approved, move on to 003_DB") |
| 4 | Database design 003_DB | 3 | database-design | `docs/en/database/003/003_DB_completion-tracking-and-dashboard-queries.md` | Column, constraint, backfill and seed specified with their migration order and recovery limits; each aggregate query written down with its plant-timezone bucketing and index or cost argument; seeded data gives every widget a non-empty, non-trivial figure and no negative lead time | drafted 2026-09-22 — 003_DB version 1: one column, two CHECKs, two partial indexes, seven queries in one snapshot, three migrations; seed figures checked by simulation; DEC-013 (user), DEC-014, DEC-015 recorded; approved by the user ("003_DB is approved, move on to the DD") |
| 5 | Screen A design amendments for REQ-033 | 4 | basic-design, detailed-design | 001_BD, 001_DD, 001_DD-FN, 001_DD-API (new versions) | The completion-time rule appears once, in the transition logic; no visible change to SCR-001; API stays backward-compatible | drafted 2026-09-22 — 001_BD v6, 001_DD v4, 001_DD-FN v3, 001_DD-API v2 (reviewed, no contract change), 001_DB pointer to 003_DB; approved with 003_DB |
| 6 | Detailed design 003_DD, its companions and the mockup | 3, 4, 5 | detailed-design, screen-design | `003_DD_production-dashboard.md`, `003_DD-API-…`, `003_DD-FN-…`, `003_DD-SPD-…`, rendered mockup | DD agrees with 003_BD and 003_DB; the API contract covers every figure and its empty value; test viewpoints cover every REQ, including window boundaries in `Asia/Tokyo` | drafted 2026-09-22 — 003_DD, 003_DD-API, 003_DD-FN, 003_DD-SPD (21 test viewpoints TC-201–TC-221) and the 8-artboard mockup source; mockup published privately; awaiting user review |
| 7 | Reconcile and close the design phase | 6 | — | `status.md`, `evidence.md` | `ai/checklists/design-consistency.md` passes; no open business decision remains; user review of the design set | superseded — the user's mockup review raised DEC-016–DEC-018; the close moves to revision 2 step 8 |
| 8 | Draft plan revision 2 (implementation, tests, PR) | 7 | planning | `plan.md` revision 2 | Shown to the user and stopped; no revision 2 step starts before it is explicitly approved | superseded — the implementation plan becomes revision 3 (revision 2 step 9) |

The user reviews each design document as it is finished (003_BD, then 003_DB together with the Screen A amendments, then the 003_DD set), as in WI-003. The next step starts only after that document is approved.

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author, designer | this agent (Claude) |
| Reviewer, approver, business decisions | ThanhTN |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local file edits under `work-items/WI-004/` and `docs/en/` | yes | user request 2026-09-22 ("screen B is done so let start working on screen C as we planned") | design documents only |
| Create branch `feature/WI-004-production-dashboard` in a worktree at `../WMS-worktrees/WI-004` and commit the design documents locally | yes | approval of this revision, 2026-09-22 | local only; nothing pushed |
| Publish the rendered mockup as a private Artifact | yes | asked and approved at execution, 2026-09-22 ("public it privately") | step 6 only — https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot |
| git push / PR / merge / deploy | no | not authorized | — |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| A new business ambiguity appears during BD/DB/DD | A metric rule the brief does not settle (e.g. whether an order completed today but due tomorrow counts as on time) | Pause and ask per `ai/policies.md`; record it as a new DEC rather than assuming |
| Seeded history is inconsistent or too thin | 16 completed demo orders spread over 12 weeks give a flat trend; seeded `created_at_utc` later than the seeded completion date gives a negative lead time | 003_DB designs the seed extension explicitly (adjusted creation dates, more completed orders if needed) and states the resulting figures, relative to the run date as WI-003 DEC-011 did |
| The completion-time change breaks Screen A | The new column's constraint rejects an existing save path, or a client could set the value | The value is set only in the domain transition, never bound from the request; the constraint is checked against every existing transition in 003_DB and re-tested in revision 2 |
| Week and day boundaries drift between the database and the application | Bucketing done in UTC, or `date_trunc` in the server's timezone | All bucketing uses `Asia/Tokyo` from `IPlantClock`, passed to the query explicitly; boundary cases become test viewpoints in 003_DD |
| A chart needs a new frontend dependency | 003_BD chooses a charting library over plain SVG | Recorded as a technical DEC with bundle-size and accessibility trade-offs; stopped and asked if it adds a runtime dependency |
| Aggregate queries become slow or numerous | One query per widget, each scanning the table | 003_DB bounds the number of queries per load and gives an index or cost argument for each |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** user message 2026-09-22: "plan revision 1 is approved, let answer the DEC-008"
- **Approved revision:** revision 1, 2026-09-22
- **Closure:** steps 1–6 done 2026-09-22 (003_BD and 003_DB with the Screen A amendments approved; the 003_DD set drafted and its mockup published). At the mockup review the user asked for a navbar, a health indicator and chart maximize (DEC-016–DEC-018), which widen the scope to the shared header and a new endpoint, so steps 7–8 are superseded by revision 2.

---

## Revision 2 — design amendment after mockup review

Revision 2, 2026-09-22. Supersedes revision 1's steps 7–8; revision 1 stays above unchanged except for its Outcome column and Closure line. The user reviewed the 003_DD mockup and asked for three additions, settled in DEC-016–DEC-018. This revision amends the approved design to include them and then closes the design phase. Implementation, tests and the PR move to revision 3, which is drafted at the end of this revision and shown for its own approval.

### Objective

Bring the reconciled design set up to date with DEC-016 (navbar on every screen), DEC-017 (server/database health indicator, polled) and DEC-018 (chart maximize/restore), republish the mockup, and close the design phase with every requirement — REQ-028–REQ-042 — traced.

### Scope

#### In scope

- Brief revision 2 (REQ-040–REQ-042, UC-012) and DEC-016–DEC-018 — already drafted with this revision, for review together with it.
- **003_BD version 2:** the navbar replaces items 5–6; the health indicator (item, states, polling, failure display); chart Expand/Restore with its overlay; new events and messages; screen transition.
- **Shared header in 001_BD and 002_BD:** a new version of each stating that the header now carries the navbar (layout and navigation only — no field, rule, validation or API of either screen changes); 002_BD's "Home" breadcrumb is kept or dropped in favour of the navbar, decided in 003_BD v2 and applied to both.
- **003_DD set version 2:** navbar component (in the shared `components/` folder), health indicator component and polling, chart overlay (`dialog` semantics, focus trap, Escape), the new endpoint in 003_DD-API and 003_DD-FN (authorization, `SELECT 1` with a short timeout, response shape, `no-store`, observability), new test viewpoints.
- **001_DD-SPD and 002_DD-SPD:** new versions noting the shared header's navbar, and any breadcrumb change.
- **003_DB:** reviewed; expected unchanged (the ping reads no table). Any change found is a stop condition.
- **Mockup:** regenerated with the navbar, the indicator (OK and database-unavailable states) and a maximized chart, and republished to the same private URL.
- Design-consistency reconciliation, then the design-phase close.

#### Out of scope

- Application code, migrations, tests and the PR — plan revision 3.
- A detailed health panel, response-time figures, alerting, or a health indicator on screens other than the dashboard (DEC-017).
- The browser Fullscreen API and collapsing charts to a title bar (DEC-018).
- Any change to Screen A's or Screen B's fields, rules, validation or API — only their shared header changes.
- Any push, PR, merge, image publication or deployment.

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| `brief.md` | revision 2 (drafted with this plan revision) | REQ-040–REQ-042 as written; changes at review are applied before step 1 starts |
| `decisions.md` DEC-016–DEC-018 | decided by the user 2026-09-22 | Details they leave to design — health states and timeout, endpoint path, breadcrumb, overlay layout — are settled in 003_BD v2 / 003_DD v2 and recorded as Claude decisions |
| 003_BD v1, 003_DB v1, 001_BD v6, 001_DD set | approved | Amended only where DEC-016–DEC-018 require |
| 003_DD set v1–v2, mockup v1 | drafted, not yet approved | Revised rather than rewritten |
| 0002_ADR (auth) | current | The health endpoint uses the same cookie session and `ProductionOrderEditor` policy as the dashboard |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Brief revision 2 and DEC-016–DEC-018 | none | requirements | `brief.md`, `decisions.md` | Each new REQ has success and failure criteria; each decision records the user's words | done 2026-09-22 — confirmed by this revision's approval |
| 2 | 003_BD version 2 | 1 | basic-design, screen-design | 003_BD | REQ-040–REQ-042 each map to BD sections; the health states, poll interval, timeout and failure display are stated once; navbar on SP defined | done 2026-09-22 — 003_BD v2: Shared application header (H-1–H-5, E-29–E-31), Health definitions HS-01–HS-05, items 26–29, M-20, E-24–E-28, FN-024–FN-026; DEC-019 (user: poll never renews the session), DEC-020 and DEC-021 recorded; sketch figures corrected to 003_DB's seed |
| 3 | Shared-header amendments to 001_BD and 002_BD | 2 | basic-design | 001_BD v7, 002_BD v4 | Only the header/navigation changes; screen transitions updated | done 2026-09-22 — 001_BD v7 and 002_BD v4 point to the shared header. Found while writing 001_BD v7: SCR-001's discard confirmation guarded only Cancel, and a draft note wrongly said it would cover navbar links; the claim was removed and the user decided DEC-022 (every in-app link asks first), so 001_BD v7 also gains E-07a — a Screen A behavior change by the user's decision |
| 4 | 003_DD set version 2 | 2 | detailed-design, screen-design | 003_DD, 003_DD-API, 003_DD-FN, 003_DD-SPD | The health endpoint's contract, authorization, timeout, observability and security are specified; overlay focus management specified; new test viewpoints cover REQ-040–REQ-042 | done 2026-09-22 — 003_DD v3, 003_DD-API v2 (API-SYS-01), 003_DD-FN v2 (§6 health service, §7 no-renew rule), 003_DD-SPD v2 (§8–§12); modules 10–13 (navbar, navigation guard, health indicator, chart dialog); test viewpoints TC-222–TC-227 |
| 5 | 001_DD-SPD and 002_DD-SPD notes | 3, 4 | detailed-design | new versions | The navbar component is referenced, not restated | done 2026-09-22 — 001_DD-SPD v2 (navbar and `NavigationGuard` registration), 002_DD-SPD v2 (navbar) |
| 6 | 003_DB check | 4 | database-design | 003_DB (unchanged, or a stop) | The ping reads no table and needs no grant | done 2026-09-22 — no change: the ping is `SELECT 1`, reads no table and needs no grant |
| 7 | Mockup regenerated and republished | 4 | screen-design | `mockups/003_DD_screen-c-mockup.html`, same private URL | Navbar (PC and SP menu), indicator OK and database-unavailable, maximized chart artboards added | done 2026-09-22 — 11 artboards (adds database-down, SP menu, maximized chart, navbar on Screen B); one render check; republished as version 2 to https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot |
| 8 | Reconcile and close the design phase | 2–7 | — | `status.md`, `evidence.md` | `ai/checklists/design-consistency.md` passes; no open business decision; user review of the amended design set | done 2026-09-22 — design-consistency walk passed (evidence.md); the amended design set approved by the user ("the DD is approved, move on to implementation") |
| 9 | Draft plan revision 3 (implementation, tests, PR) | 8 | planning | `plan.md` revision 3 | Shown to the user and stopped; no revision 3 step starts before it is explicitly approved | done 2026-09-22 — revision 3 appended below and shown for review |

As in revision 1, the user reviews the amended design set before the design phase is closed.

Addition 2026-09-22, within this revision: after mockup version 2 the user asked for icons ("please add icons so it more user friendly") and chose placement and source (DEC-023). It edits only this revision's in-scope artifacts — 003_BD (v3), 003_DD (v4) and the mockup (v3, republished to the same URL) — so it is handled as a direct request under this revision rather than a new one. The `lucide-react` dependency itself is added in plan revision 3.

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author, designer | this agent (Claude) |
| Reviewer, approver, business decisions | ThanhTN |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local file edits under `work-items/WI-004/` and `docs/en/` | yes, once this revision is approved | the user's mockup-review request, 2026-09-22 | design documents only |
| Local commits on `feature/WI-004-production-dashboard` | yes | revision 1 approval, still in force | local only |
| Republish the mockup to its existing private URL | yes | approval of this revision, 2026-09-22 | step 7 only — https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot |
| git push / PR / merge / deploy | no | not authorized | — |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| The health endpoint leaks operational detail or becomes an unauthenticated probe | A design that returns error text, versions, latency, or allows anonymous access | Authenticated with the dashboard's policy; response is two fixed status values and a timestamp; failures logged server-side only; re-checked at `ai/checklists/security-review.md` in revision 3 |
| Polling adds load or keeps a session alive indefinitely | A 30 s poll from every open dashboard | One `SELECT 1` per poll with a short timeout; polling pauses while the tab is hidden; the poll does not extend anything the session would not otherwise extend — to be confirmed against 0002_ADR's cookie settings in 003_DD v2, and raised as a question if sliding expiration makes it an issue |
| The navbar changes Screens A and B beyond their header | Tests or designs of A/B depend on the old header or breadcrumb | Header-only change; every A/B test that locates by header or breadcrumb is listed in revision 3; E2E helper `signIn` already waits on the "New production order" link, which the navbar keeps |
| The maximized chart is an accessibility trap | Focus escapes, or Escape does nothing | Native `<dialog>` with `showModal()` (focus containment and Escape by the platform), explicit return of focus; covered by an axe and keyboard test viewpoint |
| A new business ambiguity appears | e.g. what an Operator should see when only the database is down | Pause and ask; record a DEC |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** user message 2026-09-22: "ok revision 2 is approved"
- **Approved revision:** revision 2, 2026-09-22
- **Closure:** all nine steps done 2026-09-22, including the icons addition (DEC-023). The design phase is closed: 003_BD v3, 001_BD v7, 002_BD v4, 003_DB, the 003_DD set (003_DD v4, API/FN/SPD v2), 001_DD v4 set, 001_DD-SPD v2, 002_DD-SPD v2 and mockup v3 approved by the user ("the DD is approved, move on to implementation"). Superseded by revision 3, which was drafted, not started.

---

## Revision 3 — implementation, tests and PR (current)

Revision 3, 2026-09-22. Supersedes revision 2, whose design phase is complete and approved. It builds Screen C and the changes it carries into Screens A and B against the approved design, and takes the work to a reviewable PR. Drafted on the user's DD approval ("the DD is approved, move on to implementation") and shown for its own approval: no step below starts until it is approved (`ai/policies.md`).

### Objective

Implement REQ-028–REQ-042 and the REQ-019 extension end to end in the running Compose stack. That covers 003_DB's three migrations, completion stamping in Screen A, the dashboard and health endpoints with the no-renew cookie rule, the dashboard screen, the shared navbar and navigation guard, and the icons. Every acceptance criterion is covered by an automated test traced to TC-201–TC-228, and WI-003's suites stay green against the new seed.

### Scope

#### In scope

- **Database:** 003_DB's migrations in order — `AddProductionOrderCompletionTracking` (column, backfill, two checks), `AddProductionOrderDashboardIndexes` (two partial indexes, `CONCURRENTLY`, outside a transaction), `SeedDashboardDemoHistory` (re-date 16, insert 44, advance the counter, guarded).
- **Screen A backend:** `ProductionOrder.CompletedAtUtc` set on `InProgress → Completed` (001_DD v4 module 1 step 5), its EF mapping and check constraints.
- **Dashboard backend:** `IPlantClock` additions; `DashboardWindow`, `IDashboardReader`/`DashboardReader` (raw SQL in one `REPEATABLE READ READ ONLY` transaction), `DashboardMapper`, `DashboardService`, `DashboardController` (`no-store`), telemetry.
- **Health backend:** `IDatabasePing`/`DatabasePing`, `SystemHealthService`, `SystemController`, and the cookie `OnCheckSlidingExpiration` rule for the health path (DEC-019).
- **Frontend:** `lucide-react` installed at an exact version, and `components/icons.ts`; `NavigationGuardProvider`, `GuardedLink`, `AppNavbar` in `AppHeader`; `ProductionOrderForm` registers with the guard, and the Screen A/B breadcrumbs become `GuardedLink`; the dashboard feature (page, tiles, attention list, top products, `BarChart`, `ChartDialog`, `HealthIndicator` with polling, formatters, API wrappers); route `/` → dashboard, placeholder home removed; message catalog MSG-E021, MSG-I005–MSG-I008.
- **WI-003 regression:** tests and documents that state the old seed's figures (80 orders, 32/24/16/8, next number `00081`) move to 003_DB's figures. Known today: `OrderNumberingTests.cs`, `ProductionOrderListEndpointTests.cs`, `tests/e2e/specs/screen-b.spec.ts`, `ProductionOrderListPage.test.tsx`, and 002_DB's seed section (a pointer to 003_DB).
- **Tests:** backend unit, integration (dashboard and health tests on their own container, orders cleared as the owner), frontend unit (with `vitest-axe`), E2E (Playwright + axe, desktop and Pixel 7) — traced to TC-201–TC-228, plus the updated Screen A and B cases.
- **Test plan** TP-004 (`work-items/WI-004/test-plan.md`), and the evidence and status records.
- **Gates:** design-consistency, security-review and delivery checklists.
- **Git:** local commits on `feature/WI-004-production-dashboard`, then push and one PR — subject to the authorization below.

#### Out of scope

- Merging the PR, deployment, image publication.
- Close-out after the merge (status, `ai/project.md` — including recording `lucide-react` in the confirmed stack — `CLAUDE.md`, and the root `README.md`), which follows the merge as its own change, per the feature-delivery workflow.
- Everything under "Not doing" in `brief.md`; any change beyond the approved design. A divergence found while coding is recorded as a decision and the document updated in the same change, never left to drift; a divergence that changes behavior is a stop condition.

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| `brief.md` | revision 2 (REQ-028–REQ-042) | — |
| 003_BD, 001_BD, 002_BD | v3, v7, v4 (approved) | — |
| 003_DB | v1 (approved) | — |
| 003_DD set, 001_DD set, 001_DD-SPD, 002_DD-SPD | 003_DD v4 + companions v2; 001_DD v4; SPDs v2 (approved) | — |
| `decisions.md` DEC-001–DEC-023 | decided | — |
| Existing code on `master` | `c3747ed` | Conventions of WI-002/WI-003 — `Result<T>`, RFC 9457, `apiClient`, `messages.ts`, telemetry, test layout — extended, not duplicated |
| Verified commands in `ai/project.md` | current | Build, test, migration and E2E commands used as listed; no new command invented |
| Local environment | Docker Desktop, .NET 10 SDK, Node | The WI-003 Compose volumes were removed, so the stack starts from a fresh volume and every migration (WI-001 to WI-004) runs in order; a `deploy/.env` with local values exists or is created from `.env.example` without committing it |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Domain and EF: `CompletedAtUtc`, mapping, check constraints | none | implementation | `ProductionOrder`, `ProductionOrderConfigurations` | `dotnet build`; unit tests in step 7 | done 2026-09-22 — `CompletedAtUtc` set on InProgress → Completed; mapping, both checks and the two partial indexes in the model |
| 2 | Migrations 1–3 (003_DB), applied to the local Compose database as the owner | 1 | implementation, database-design | three migrations | `dotnet ef database update`; column and checks present; no `INVALID` index; 124 orders, 35/25/56/8; counter advanced | done 2026-09-22 — three migrations applied to a fresh Compose volume: 124 orders (35/25/56/8), checks present, no INVALID index, counter 124. Scaffolding caught EF treating a second index on the same columns as a reconfiguration of 002_DB's index (it would have dropped it); fixed with a named index before anything ran |
| 3 | `IPlantClock` additions; dashboard window, reader, mapper, service, controller, telemetry | 1 | implementation | `Application/Dashboard/*`, `Infrastructure/Dashboard/*`, `DashboardController` | `dotnet build` | done 2026-09-22 — the reader runs 003_DB's SQL through ADO.NET on the EF transaction (DEC-024); smoke run matched 003_DB's figures exactly |
| 4 | Health: ping port, service, controller; cookie no-renew rule | 1 | implementation, security-review | `Application/System/*`, `Infrastructure/System/*`, `SystemController`, `DependencyInjection.cs` | `dotnet build` | done 2026-09-22 — health endpoint, `Health` namespace (DEC-024); no-renew rule in both sliding expiration and `OnValidatePrincipal` (DEC-025, found by TC-225) |
| 5 | Frontend foundation: `lucide-react` (exact version), `icons.ts`, navigation guard, `GuardedLink`, `AppNavbar`/`AppHeader`, Screen A form registration, breadcrumbs | none | implementation | files in 003_DD X-1 rows 2a–2f | `npm run lint`, `npm run build`; existing Screen A/B unit tests still pass | done 2026-09-22 — `lucide-react@1.47.0` pinned; icons, guard, navbar; lint clean; existing 56 frontend tests still pass |
| 6 | Frontend dashboard: page, tiles, attention list, top products, `BarChart`, `ChartDialog`, `HealthIndicator`, formatters, API wrappers, route, messages | 5 | implementation, screen-design | `features/dashboard/*`, `App.tsx`, `messages.ts` | `npm run lint`, `npm run build` | done 2026-09-22 — dashboard feature, route `/`, messages; lint and build clean |
| 7 | Backend unit tests | 1, 3, 4 | testing | `Application.Tests` additions | `dotnet test` — completion stamping (TC-207), `DashboardWindow` for every weekday and month boundary (TC-205, TC-209), mapper zero-fill and rounding (TC-203, TC-212), health outcomes | done 2026-09-22 — 148/148 (32 new) |
| 8 | Backend integration tests, including WI-003 regression updates | 2, 3, 4 | testing | `Integration.Tests` additions | `dotnet test` — TC-203–TC-214, TC-218, TC-219, TC-221, TC-224, TC-225, TC-208 | done 2026-09-22 — 85/85 (22 new; 5 WI-003 tests moved to the new seed) |
| 9 | Frontend unit tests | 5, 6 | testing | `tests/unit/` additions | `npm test` — TC-202, TC-210, TC-212, TC-215, TC-216, TC-217 (axe), TC-222, TC-223, TC-226, TC-227, TC-228; the `<dialog>` methods jsdom lacks are stubbed in the test setup only | done 2026-09-22 — 87/87 (31 new) |
| 10 | E2E tests, including WI-003 regression updates | 2, 3, 4, 6 | testing | `tests/e2e/specs/` additions | Compose stack + `npx playwright test` — TC-201, TC-202, TC-213, TC-216, TC-217, TC-220, TC-222, TC-223, TC-227; Screen A and B specs still pass | done 2026-09-22 — 22/22 (6 new; WI-003's sign-in helper and one journey updated); axe found and fixed one contrast failure |
| 11 | Test plan TP-004 | 7–10 | testing | `work-items/WI-004/test-plan.md` | Every TC-201–TC-228 maps to a named test with its recorded result | done 2026-09-22 — TP-004 written; every TC-201–TC-228 maps to named, passing tests |
| 12 | Full local verification | 2–11 | — | `evidence.md` | `dotnet build`, `dotnet test`, `npm run lint`, `npm run build`, `npm test`, Compose + Playwright — each recorded as actually run, with counts | done 2026-09-22 — every command run and recorded with counts; CI not run (not pushed) |
| 13 | Gates: design-consistency, security-review, delivery | 12 | security-review, pr-review | `evidence.md` walks | Every checklist item answered; security review covers the two new endpoints' authorization, the raw SQL's parameter binding, the read-only transaction, the health response's fields and logging, the no-renew rule, the navigation guard, and the new dependency's licence and pinning | done 2026-09-22 — all three walked; dependency audit clean; secret scan clean |
| 14 | Push the branch and open a PR | 13 | pr-review, ci-cd | PR to `master` | CI (backend, frontend, e2e) green on the PR; diff reviewed against the design set | done 2026-09-22 — pushed on the user's authorization ("sure"); PR #15 opened; CI green on Backend, Frontend and E2E |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Implementer, test author | this agent (Claude) |
| Reviewer, approver, merge | ThanhTN |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local edits under `src/`, `tests/`, `docs/`, `work-items/WI-004/` | yes | revision 3 approval, 2026-09-22 | This revision's scope |
| Local commits on `feature/WI-004-production-dashboard` | yes | revision 1 approval, still in force | local only |
| `npm install lucide-react@<exact>` in `src/frontend` (changes `package.json`, `package-lock.json`) | yes | revision 3 approval, 2026-09-22; DEC-023 (user) | one package, exact version |
| Run the local Compose stack and apply migrations to the local database | yes | revision 3 approval, 2026-09-22 | local only; fresh volume; the seeds are guarded |
| Push the branch and open a PR | yes | asked at step 14 and granted, 2026-09-22 ("sure") | step 14 only |
| Merge, deploy, publish an image | no | not authorized | — |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| EF Core's `SqlQuery<T>` cannot map a row shape, or the explicit transaction interferes with the execution strategy | Translation or retry errors in the reader | Flat row types (003_DD-FN's recorded fallback); if Npgsql's retrying execution strategy is enabled, wrap the transaction in `CreateExecutionStrategy().ExecuteAsync`; no contract change |
| The migration's validated CHECK fails on existing data | A `Completed` row the backfill misses, or `updated_at_utc < created_at_utc` | Backfill runs before the checks in one transaction, so a failure rolls everything back; a real violation stops the work and is reported, not bypassed |
| WI-003 tests break beyond the known seed figures | Relative-date or paging assertions affected by 44 more orders | Fix the assertion to the new documented figures only where the figure is a seed fact; any other failure is investigated as a real regression |
| Time-dependent tests flake | Dashboard figures depend on "now"; the seed uses the database's `now()` | Integration tests pin the plant clock and create their own rows on a cleared database; the seed test (TC-218) asserts only relative properties; E2E asserts structure and relative facts, not absolute figures |
| The no-renew rule does not take effect | The cookie handler ignores `ShouldRenew` or uses a different clock | TC-225 with the cookie handler's `TimeProvider` advanced past half the lifetime; a failure is a stop condition, because DEC-019 is the user's decision |
| jsdom lacks `HTMLDialogElement.showModal` | Frontend unit tests of `ChartDialog` | Stub in the test setup only; the real behavior is covered by E2E (TC-227) |
| Polling makes E2E flaky | Health checks racing assertions | E2E waits on rendered state, not timers; polling interval unchanged in production code |
| A design gap appears while coding | A behavior the documents do not settle | Stop and ask if it changes behavior; otherwise record a technical DEC and update the document in the same commit |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** user message 2026-09-22: "plan approved, let move on to implementation"
- **Approved revision:** revision 3, 2026-09-22
- **Closure:** all 14 steps done 2026-09-22. PR #15 passed CI and was squash-merged by the user as `cd3a3b9` (DEC-026). The close-out (this record, `ai/project.md`, `CLAUDE.md`, the root `README.md`) follows in its own change, per the feature-delivery workflow.
