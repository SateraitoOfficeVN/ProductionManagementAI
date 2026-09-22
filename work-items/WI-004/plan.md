# Production Dashboard (Screen C) — Implementation Plan

Revisions are kept in full and in chronological order (oldest first), so the plan can be back-tracked. The last revision is the current one.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-22 | Design (brief → BD → DB → DD + mockup) | **current** — approved; in progress | user message 2026-09-22: "plan revision 1 is approved, let answer the DEC-008" |

## Revision 1 — design phase (current)

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
| 6 | Detailed design DD-003, its companions and the mockup | 3, 4, 5 | detailed-design, screen-design | `DD-003-production-dashboard.md`, `DD-003-API-…`, `DD-003-FN-…`, `DD-003-SPD-…`, rendered mockup | DD agrees with BD-003 and DB-004; the API contract covers every figure and its empty value; test viewpoints cover every REQ, including window boundaries in `Asia/Tokyo` | drafted 2026-09-22 — DD-003, DD-003-API, DD-003-FN, DD-003-SPD (21 test viewpoints TC-201–TC-221) and the 8-artboard mockup source; mockup publication awaits authorization; awaiting user review |
| 7 | Reconcile and close the design phase | 6 | — | `status.md`, `evidence.md` | `ai/checklists/design-consistency.md` passes; no open business decision remains; user review of the design set | pending |
| 8 | Draft plan revision 2 (implementation, tests, PR) | 7 | planning | `plan.md` revision 2 | Shown to the user and stopped; no revision 2 step starts before it is explicitly approved | pending |

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
| Publish the rendered mockup as a private Artifact | ask at execution | — | step 6 only |
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
