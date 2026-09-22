# Production Dashboard (Screen C) — Implementation Plan

Revisions are kept in full and in chronological order (oldest first), so the plan can be back-tracked. The last revision is the current one.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-22 | Design (brief → BD → DB → DD + mockup) | approved; steps 1–6 done, steps 7–8 superseded by revision 2 | user message 2026-09-22: "plan revision 1 is approved, let answer the DEC-008" |
| 2 | 2026-09-22 | Design amendment after mockup review (navbar, health indicator, chart maximize) and design-phase close | **current** — approved; in progress | user message 2026-09-22: "ok revision 2 is approved" |

## Revision 1 — design phase

Revision 1, 2026-09-22. First plan for WI-004 (Screen C, production dashboard), started after WI-003 was merged to `master`. It covers the design phase only. Implementation, tests and the PR come in revision 2, which will be drafted once these designs are reconciled and approved, and shown for its own approval before any of its steps start (`ai/policies.md`).

### Objective

Produce the reconciled design set for Screen C: the brief, BD-003, DB-004 for completion tracking and the dashboard's aggregate queries, and DD-003 with its companions and a rendered mockup. It also covers the amendments that completion tracking needs in Screen A's BD-001 and DD-001 set. Every requirement in `brief.md` (REQ-028–REQ-039) must be traced.

### Scope

#### In scope

- Requirements brief and decision log (`work-items/WI-004/`).
- Basic design BD-003 (`docs/en/010_basic-design/BD-003-production-dashboard.md`): SCR-003 at `/`, one section per widget, PC and SP layouts, the chart approach and each chart's accessible text or table equivalent, and the screen transition from login.
- Database design DB-004 (`docs/en/database/0004-completion-tracking-and-dashboard-queries.md`): the `completed_at_utc` column and its constraint, backfill, seed extension (DEC-004), the aggregate queries with plant-timezone bucketing, and their index support.
- Amendments to Screen A's design for REQ-033: BD-001 (the `InProgress → Completed` transition records the completion time) and DD-001 / DD-001-FN / DD-001-API as far as they are affected. Screen A's visible behavior does not change.
- Detailed design DD-003 and its companions DD-003-API, DD-003-FN and DD-003-SPD, each as its own file, plus a rendered mockup (`docs/en/020_detailed-design/`).
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
| BD-001, DD-001 set, DB-002 (Screen A) | current `master` | Status labels, state machine, message conventions and the plant clock are reused, not restated |
| BD-002, DD-002 set, DB-003 (Screen B) | current `master` | The overdue rule (M-08) and the 80-row demo seed are reused and extended |
| ADR-0001 (layered backend), ADR-0002 (auth/RBAC) | current `master` | The dashboard endpoint reuses the cookie auth and the `ProductionOrderEditor` policy (Admin or Operator) |
| `ai/templates/basic-design.md`, `detailed-design.md`, `DD/*`, `database-design.md` | current `master` | Unchanged since WI-003 |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Requirements brief and decision log | none | requirements | `work-items/WI-004/brief.md`, `decisions.md`, `status.md` | Every REQ has success and failure criteria; every scope answer recorded as a DEC | done 2026-09-22 — REQ-028–REQ-039, UC-008–UC-011; DEC-001–DEC-007 decided; DEC-008 proposed |
| 2 | Settle the metric windows and sizes (DEC-008) | 1 | — | `decisions.md`, brief updated | User answer recorded with rationale | done 2026-09-22 — DEC-008 decided; brief updated |
| 3 | Basic design BD-003 | 1, 2 | basic-design, screen-design | `docs/en/010_basic-design/BD-003-production-dashboard.md` | Every REQ-028–REQ-039 maps to a BD section; each metric's definition, window and empty state is stated once; chart approach and its accessible equivalent chosen and recorded as a DEC | done 2026-09-22 — BD-003 version 1 (SCR-003, FN-017–FN-023, D-01–D-09, M-11–M-19, E-20–E-23); DEC-009 (user) and DEC-010–DEC-012 recorded; approved by the user ("BD-003 is approved, move on to DB-004") |
| 4 | Database design DB-004 | 3 | database-design | `docs/en/database/0004-completion-tracking-and-dashboard-queries.md` | Column, constraint, backfill and seed specified with their migration order and recovery limits; each aggregate query written down with its plant-timezone bucketing and index or cost argument; seeded data gives every widget a non-empty, non-trivial figure and no negative lead time | drafted 2026-09-22 — DB-004 version 1: one column, two CHECKs, two partial indexes, seven queries in one snapshot, three migrations; seed figures checked by simulation; DEC-013 (user), DEC-014, DEC-015 recorded; approved by the user ("DB-004 is approved, move on to the DD") |
| 5 | Screen A design amendments for REQ-033 | 4 | basic-design, detailed-design | BD-001, DD-001, DD-001-FN, DD-001-API (new versions) | The completion-time rule appears once, in the transition logic; no visible change to SCR-001; API stays backward-compatible | drafted 2026-09-22 — BD-001 v6, DD-001 v4, DD-001-FN v3, DD-001-API v2 (reviewed, no contract change), DB-002 pointer to DB-004; approved with DB-004 |
| 6 | Detailed design DD-003, its companions and the mockup | 3, 4, 5 | detailed-design, screen-design | `DD-003-production-dashboard.md`, `DD-003-API-…`, `DD-003-FN-…`, `DD-003-SPD-…`, rendered mockup | DD agrees with BD-003 and DB-004; the API contract covers every figure and its empty value; test viewpoints cover every REQ, including window boundaries in `Asia/Tokyo` | drafted 2026-09-22 — DD-003, DD-003-API, DD-003-FN, DD-003-SPD (21 test viewpoints TC-201–TC-221) and the 8-artboard mockup source; mockup published privately; awaiting user review |
| 7 | Reconcile and close the design phase | 6 | — | `status.md`, `evidence.md` | `ai/checklists/design-consistency.md` passes; no open business decision remains; user review of the design set | superseded — the user's mockup review raised DEC-016–DEC-018; the close moves to revision 2 step 8 |
| 8 | Draft plan revision 2 (implementation, tests, PR) | 7 | planning | `plan.md` revision 2 | Shown to the user and stopped; no revision 2 step starts before it is explicitly approved | superseded — the implementation plan becomes revision 3 (revision 2 step 9) |

The user reviews each design document as it is finished (BD-003, then DB-004 together with the Screen A amendments, then the DD-003 set), as in WI-003. The next step starts only after that document is approved.

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
| Seeded history is inconsistent or too thin | 16 completed demo orders spread over 12 weeks give a flat trend; seeded `created_at_utc` later than the seeded completion date gives a negative lead time | DB-004 designs the seed extension explicitly (adjusted creation dates, more completed orders if needed) and states the resulting figures, relative to the run date as WI-003 DEC-011 did |
| The completion-time change breaks Screen A | The new column's constraint rejects an existing save path, or a client could set the value | The value is set only in the domain transition, never bound from the request; the constraint is checked against every existing transition in DB-004 and re-tested in revision 2 |
| Week and day boundaries drift between the database and the application | Bucketing done in UTC, or `date_trunc` in the server's timezone | All bucketing uses `Asia/Tokyo` from `IPlantClock`, passed to the query explicitly; boundary cases become test viewpoints in DD-003 |
| A chart needs a new frontend dependency | BD-003 chooses a charting library over plain SVG | Recorded as a technical DEC with bundle-size and accessibility trade-offs; stopped and asked if it adds a runtime dependency |
| Aggregate queries become slow or numerous | One query per widget, each scanning the table | DB-004 bounds the number of queries per load and gives an index or cost argument for each |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** user message 2026-09-22: "plan revision 1 is approved, let answer the DEC-008"
- **Approved revision:** revision 1, 2026-09-22
- **Closure:** steps 1–6 done 2026-09-22 (BD-003 and DB-004 with the Screen A amendments approved; the DD-003 set drafted and its mockup published). At the mockup review the user asked for a navbar, a health indicator and chart maximize (DEC-016–DEC-018), which widen the scope to the shared header and a new endpoint, so steps 7–8 are superseded by revision 2.

---

## Revision 2 — design amendment after mockup review (current)

Revision 2, 2026-09-22. Supersedes revision 1's steps 7–8; revision 1 stays above unchanged except for its Outcome column and Closure line. The user reviewed the DD-003 mockup and asked for three additions, settled in DEC-016–DEC-018. This revision amends the approved design to include them and then closes the design phase. Implementation, tests and the PR move to revision 3, which is drafted at the end of this revision and shown for its own approval.

### Objective

Bring the reconciled design set up to date with DEC-016 (navbar on every screen), DEC-017 (server/database health indicator, polled) and DEC-018 (chart maximize/restore), republish the mockup, and close the design phase with every requirement — REQ-028–REQ-042 — traced.

### Scope

#### In scope

- Brief revision 2 (REQ-040–REQ-042, UC-012) and DEC-016–DEC-018 — already drafted with this revision, for review together with it.
- **BD-003 version 2:** the navbar replaces items 5–6; the health indicator (item, states, polling, failure display); chart Expand/Restore with its overlay; new events and messages; screen transition.
- **Shared header in BD-001 and BD-002:** a new version of each stating that the header now carries the navbar (layout and navigation only — no field, rule, validation or API of either screen changes); BD-002's "Home" breadcrumb is kept or dropped in favour of the navbar, decided in BD-003 v2 and applied to both.
- **DD-003 set version 2:** navbar component (in the shared `components/` folder), health indicator component and polling, chart overlay (`dialog` semantics, focus trap, Escape), the new endpoint in DD-003-API and DD-003-FN (authorization, `SELECT 1` with a short timeout, response shape, `no-store`, observability), new test viewpoints.
- **DD-001-SPD and DD-002-SPD:** new versions noting the shared header's navbar, and any breadcrumb change.
- **DB-004:** reviewed; expected unchanged (the ping reads no table). Any change found is a stop condition.
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
| `decisions.md` DEC-016–DEC-018 | decided by the user 2026-09-22 | Details they leave to design — health states and timeout, endpoint path, breadcrumb, overlay layout — are settled in BD-003 v2 / DD-003 v2 and recorded as Claude decisions |
| BD-003 v1, DB-004 v1, BD-001 v6, DD-001 set | approved | Amended only where DEC-016–DEC-018 require |
| DD-003 set v1–v2, mockup v1 | drafted, not yet approved | Revised rather than rewritten |
| ADR-0002 (auth) | current | The health endpoint uses the same cookie session and `ProductionOrderEditor` policy as the dashboard |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Brief revision 2 and DEC-016–DEC-018 | none | requirements | `brief.md`, `decisions.md` | Each new REQ has success and failure criteria; each decision records the user's words | done 2026-09-22 — confirmed by this revision's approval |
| 2 | BD-003 version 2 | 1 | basic-design, screen-design | BD-003 | REQ-040–REQ-042 each map to BD sections; the health states, poll interval, timeout and failure display are stated once; navbar on SP defined | done 2026-09-22 — BD-003 v2: Shared application header (H-1–H-5, E-29–E-31), Health definitions HS-01–HS-05, items 26–29, M-20, E-24–E-28, FN-024–FN-026; DEC-019 (user: poll never renews the session), DEC-020 and DEC-021 recorded; sketch figures corrected to DB-004's seed |
| 3 | Shared-header amendments to BD-001 and BD-002 | 2 | basic-design | BD-001 v7, BD-002 v4 | Only the header/navigation changes; screen transitions updated | done 2026-09-22 — BD-001 v7 and BD-002 v4 point to the shared header. Found while writing BD-001 v7: SCR-001's discard confirmation guarded only Cancel, and a draft note wrongly said it would cover navbar links; the claim was removed and the user decided DEC-022 (every in-app link asks first), so BD-001 v7 also gains E-07a — a Screen A behavior change by the user's decision |
| 4 | DD-003 set version 2 | 2 | detailed-design, screen-design | DD-003, DD-003-API, DD-003-FN, DD-003-SPD | The health endpoint's contract, authorization, timeout, observability and security are specified; overlay focus management specified; new test viewpoints cover REQ-040–REQ-042 | done 2026-09-22 — DD-003 v3, DD-003-API v2 (API-SYS-01), DD-003-FN v2 (§6 health service, §7 no-renew rule), DD-003-SPD v2 (§8–§12); modules 10–13 (navbar, navigation guard, health indicator, chart dialog); test viewpoints TC-222–TC-227 |
| 5 | DD-001-SPD and DD-002-SPD notes | 3, 4 | detailed-design | new versions | The navbar component is referenced, not restated | done 2026-09-22 — DD-001-SPD v2 (navbar and `NavigationGuard` registration), DD-002-SPD v2 (navbar) |
| 6 | DB-004 check | 4 | database-design | DB-004 (unchanged, or a stop) | The ping reads no table and needs no grant | done 2026-09-22 — no change: the ping is `SELECT 1`, reads no table and needs no grant |
| 7 | Mockup regenerated and republished | 4 | screen-design | `mockups/DD-003-screen-c-mockup.html`, same private URL | Navbar (PC and SP menu), indicator OK and database-unavailable, maximized chart artboards added | done 2026-09-22 — 11 artboards (adds database-down, SP menu, maximized chart, navbar on Screen B); one render check; republished as version 2 to https://claude.ai/artifact/5f5hbKibAX3xURVAS5Aeot |
| 8 | Reconcile and close the design phase | 2–7 | — | `status.md`, `evidence.md` | `ai/checklists/design-consistency.md` passes; no open business decision; user review of the amended design set | in progress — design-consistency walk passed (evidence.md); awaiting the user's review of the amended design set |
| 9 | Draft plan revision 3 (implementation, tests, PR) | 8 | planning | `plan.md` revision 3 | Shown to the user and stopped; no revision 3 step starts before it is explicitly approved | pending |

As in revision 1, the user reviews the amended design set before the design phase is closed.

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
| Polling adds load or keeps a session alive indefinitely | A 30 s poll from every open dashboard | One `SELECT 1` per poll with a short timeout; polling pauses while the tab is hidden; the poll does not extend anything the session would not otherwise extend — to be confirmed against ADR-0002's cookie settings in DD-003 v2, and raised as a question if sliding expiration makes it an issue |
| The navbar changes Screens A and B beyond their header | Tests or designs of A/B depend on the old header or breadcrumb | Header-only change; every A/B test that locates by header or breadcrumb is listed in revision 3; E2E helper `signIn` already waits on the "New production order" link, which the navbar keeps |
| The maximized chart is an accessibility trap | Focus escapes, or Escape does nothing | Native `<dialog>` with `showModal()` (focus containment and Escape by the platform), explicit return of focus; covered by an axe and keyboard test viewpoint |
| A new business ambiguity appears | e.g. what an Operator should see when only the database is down | Pause and ask; record a DEC |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** user message 2026-09-22: "ok revision 2 is approved"
- **Approved revision:** revision 2, 2026-09-22
