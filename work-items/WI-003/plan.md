# Production Order List (Screen B) — Implementation Plan

Revisions are kept in full and in chronological order (oldest first), so the plan can be back-tracked. The last revision is the current one.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-22 | Design (brief → BD → DB → DD + mockup) | approved; complete; superseded by revision 2 | user message 2026-09-22: "approved, let move on to BD-002" |
| 2 | 2026-09-22 | Implementation, tests, PR | **current** — complete; PR #9 squash-merged (DEC-013). Never shown to the user for approval before execution (DEC-014) | none for the revision itself — work started on the DD approval, "the DD is approved, move on to the implementation"; step 14 on "yes push it and open the PR" (DEC-014) |

## Revision 1 — design phase

Revision 1, 2026-09-22. First plan for WI-003 (Screen B, production-order list), started after WI-002 was merged to `master`. It covers the design phase only; implementation, tests and the PR will be a later revision, drafted once the designs are reconciled and approved — the same two-revision shape WI-002 used.

### Objective

Produce the reconciled design set for Screen B — brief, BD-002, the DB-design change needed to keep the list query index-backed, and DD-002 with a rendered mockup — tracing every requirement in `brief.md` (REQ-020–REQ-027).

### Scope

#### In scope

- Requirements brief and decision log (`work-items/WI-003/`).
- Basic design BD-002 (`docs/en/010_basic-design/BD-002-production-order-list.md`).
- Database design DB-003 (`docs/en/database/0003-production-order-list-queries.md`): indexes for the filters and the default sort, the case-insensitive order-number match, and the demo-order seed (DEC-007). No new table or column is expected; if design finds one is needed, that is a stop condition (see Risks).
- Detailed design DD-002 and its companions DD-002-API, DD-002-FN, DD-002-SPD, each as its own file, plus a rendered mockup (`docs/en/020_detailed-design/`).
- Design-consistency reconciliation across all of the above, against the checklist in `ai/checklists/design-consistency.md`.

#### Out of scope

- Application code, migrations, tests, PR — plan revision 2.
- Screen C (dashboard) and everything under "Not doing" in `brief.md`.
- Any change to Screen A's behavior, its API or its schema semantics.
- Any push, PR, merge, image publication or deployment.

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| `work-items/WI-003/brief.md` | revision 1, 2026-09-22 | REQ-020–REQ-027 as written, with DEC-005–DEC-007 already folded in |
| `work-items/WI-003/decisions.md` DEC-001–DEC-007 | decided 2026-09-22 | — |
| BD-001, DD-001 set, DB-002 (Screen A) | current `master` | Screen A's field semantics, status model and message conventions are reused rather than restated |
| ADR-0001 (layered backend), ADR-0002 (auth/RBAC) | current `master` | Stack and auth conventions unchanged; the list endpoint reuses the existing cookie auth and role policy |
| WI-002 DEC-011/DEC-017 (`Asia/Tokyo` plant clock) | decided | "Today" for the overdue marker and date filters comes from the existing `IPlantClock` |
| `ai/templates/basic-design.md`, `detailed-design.md`, `DD/*`, `database-design.md` | current `master` | Templates unchanged since WI-002 |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Requirements brief + decision log | none | requirements | `work-items/WI-003/brief.md`, `decisions.md`, `status.md` | Every REQ has success and failure criteria; every scope answer recorded as a DEC | done 2026-09-22 — REQ-020–REQ-027, UC-004–UC-007; DEC-001–DEC-004 decided (DEC-005–DEC-007 raised here and closed in step 2) |
| 2 | Answer the design-phase business questions (DEC-005–DEC-007) | 1 | — | `decisions.md`, brief updated | User answers recorded with rationale | done 2026-09-22 — DEC-005 multi-select status filter, DEC-006 show all by default, DEC-007 seed ~60–100 demo orders |
| 3 | Basic design BD-002 | 1, 2 | basic-design, screen-design | `docs/en/010_basic-design/BD-002-production-order-list.md` | Every REQ-020–REQ-027 maps to a BD section; screen transition to Screen A and from the home page is stated | done 2026-09-22 — BD-002 version 1 (SCR-002, FN-010–FN-016, V-09–V-13, E-10–E-19); DEC-008, DEC-009 recorded; design-consistency walk in evidence.md |
| 4 | Database design DB-003 | 3 | database-design | `docs/en/database/0003-production-order-list-queries.md` | Each filter and the default sort has a justified index; the query-plan argument is written down; migration impact and the demo-order seed (DEC-007) specified | done 2026-09-22 — DB-003: no schema change, two new indexes (+`pg_trgm`), sort-key mapping, 80-row seed, two migrations with recovery limits; DEC-010, DEC-011 recorded; BD-002 v2 reconciled |
| 5 | Detailed design DD-002 + companions + mockup | 3, 4 | detailed-design, screen-design | `docs/en/020_detailed-design/DD-002-production-order-list.md`, `DD-002-API-…`, `DD-002-FN-…`, `DD-002-SPD-…`, rendered mockup | DD agrees with BD-002 and DB-003; the API contract covers every filter, sort field, paging parameter and its rejection case; test viewpoints cover every REQ | done 2026-09-22 — all four DD-002 documents written (19 test viewpoints TC-101–TC-119) plus the 7-state mockup, published privately with the user's authorization; message IDs renumbered after a collision with DD-001's catalog (BD-002 v3) |
| 6 | Reconcile and close the design phase | 5 | — | `status.md`, `evidence.md` | `ai/checklists/design-consistency.md` passes; no open business decision remains; user review of the design set | done 2026-09-22 — checklist passed at BD, DB and DD scope (one defect found and fixed: the message-ID collision); BD-002, DB-003 and the DD-002 set each approved by the user |
| 7 | Draft plan revision 2 (implementation, tests, PR) | 6 | planning | `plan.md` revision 2 | Submitted for review; revision 1 closed in place per the plan-revision rule | done 2026-09-22 — revision 2 appended below, but not submitted for review: implementation started straight away (DEC-014) |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author, designer | this agent (Claude) |
| Reviewer, approver, business decisions | ThanhTN |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local file edits under `work-items/WI-003/` and `docs/en/` | yes | user request 2026-09-22 ("now that we done with screen A, let move on to screen B as we planned") | design documents only |
| Create the `feature/WI-003-production-order-list` branch and commit locally | ask at execution | not yet authorized | would be requested with plan revision 2 |
| Publish the rendered mockup as a private Artifact | yes | asked and approved at execution, 2026-09-22 ("Yes, publish privately") | step 5 only — https://claude.ai/artifact/2XrZ9xnEfzQ6pCbUnZovL3 |
| git push / PR / merge / deploy | no | not yet authorized | — |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| A new business ambiguity appears during BD/DB/DD | A rule the brief does not settle (e.g. what "last updated" means to a planner, or how overdue is marked) | Pause and ask per `ai/policies.md`; record it as a new DEC rather than assuming |
| The list needs a schema change, not just indexes | DB-003 finds a required column or table (e.g. a denormalized product name) | Stop and ask before writing the migration: it would change WI-002's schema and widen the plan's scope |
| Filter or sort parameters become an injection or unbounded-query surface | DD-002-API design of `sort`, `pageSize`, order-number `LIKE` | Allow-list sort fields and page sizes, bind every filter as a parameter, cap page size; record it in DD-002-API and re-check at `ai/checklists/security-review.md` in revision 2 |
| Screen B and Screen A drift on labels, statuses or date handling | BD-002/DD-002 restate Screen A's rules instead of referencing them | Reference BD-001/DD-001 and the existing message IDs; the design-consistency checklist covers this |
| Seeded demo orders (DEC-007) reach a non-demo environment | Seed written into a migration that runs everywhere | Settle the seed's environment scope in DB-003; keep it reversible and clearly identifiable |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** user message 2026-09-22: "approved, let move on to BD-002"
- **Approved revision:** revision 1, 2026-09-22
- **Closure:** all seven steps done 2026-09-22. The design set (BD-002, DB-003, DD-002 + API/FN/SPD companions, mockup) was reviewed and approved by the user in three passes ("the BD look good", "the DB design is approved", "the DD is approved"). Superseded by revision 2.

---

## Revision 2 — implementation, tests and PR (current)

Revision 2, 2026-09-22. Supersedes revision 1, whose design phase is complete and approved; revision 1 stays above unchanged except for its Outcome column and Closure line. This revision builds Screen B against that approved design and takes it to a reviewable PR.

### Objective

Implement SCR-002 end to end — database migrations, the list query endpoint, the React screen, and unit, integration and E2E tests — so a signed-in Admin or Operator can find a production order in the running Compose stack and open it in Screen A, with every acceptance criterion in `brief.md` covered by an automated test.

### Scope

#### In scope

- **Database:** the two migrations from DB-003 — `AddProductionOrderListIndexes` (`pg_trgm` plus the two indexes, created `CONCURRENTLY` outside the migration transaction) and `SeedDemoProductionOrders` (80 guarded demo rows and the counter row).
- **Backend:** `ProductionOrderListQuery` and its validation, the list contracts, `ProductionOrderService.ListAsync`, the repository's `CountOrdersAsync`/`ListOrdersAsync`, filter and sort composition, the list mapper, the controller action, and the telemetry specified in DD-002-FN.
- **Frontend:** `useListViewState`, `listValidation`, `ProductionOrderFilters`, `ProductionOrderTable`, `ListPagination`, `ProductionOrderListPage`, the `/production-orders` route, the home-page link, and the extensions to `api.ts`, `types.ts` and `messages.ts`.
- **The one Screen A change BD-002 names:** SCR-001's Cancel and its not-found/forbidden panels return to the list instead of `/`; BD-001 and DD-001 are updated in the same change.
- **Tests:** backend unit and integration, frontend unit (with `vitest-axe`), and E2E (Playwright with `@axe-core/playwright`), all traced to TC-101–TC-119.
- **Test plan** `work-items/WI-003/test-plan.md` (TP-003), and the evidence and status records.
- **Gates:** the design-consistency, security-review and delivery checklists.
- **Git:** branch, local commits, push and one PR — subject to the authorization in "Resources and external actions" below.

#### Out of scope

- Merging the PR, deployment, image publication.
- Screen C, and everything under "Not doing" in `brief.md`.
- Any change to Screen A's business rules, schema semantics or API contract beyond the navigation target above.
- Resolving WI-001 DEC-015 (the role/permission matrix): the list endpoint keeps the existing `ProductionOrderEditor` policy, as DD-002-API records.

### Inputs and assumptions

| Input (brief / BD / DD / DB / ADR / decisions) | Revision | Assumption made if input is missing or incomplete |
| --- | --- | --- |
| `brief.md` | revision 1, REQ-020–REQ-027 | — |
| BD-002 | version 3 (approved) | — |
| DB-003 | version 1 (approved) | — |
| DD-002, DD-002-API, DD-002-FN, DD-002-SPD | versions 1–2 (approved) | The implementation follows them; any divergence found while coding is recorded as a decision and the document updated in the same change, never left to drift |
| `decisions.md` DEC-001–DEC-011 | decided | — |
| WI-002's implementation (`src/`) | current `master` | Its conventions — `Result<T>`, RFC 9457 problems, `apiClient`, `messages.ts`, the plant clock, telemetry, test layout — are extended, not duplicated |
| Verified commands in `ai/project.md` | current | Build, test and E2E commands are used as listed; no new command is invented |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Branch `feature/WI-003-production-order-list` (worktree, as WI-002 used) | none | — | branch | `master` left clean; work happens on the branch |done 2026-09-22 — worktree at `../WMS-worktrees/WI-003`; `master` untouched |
| 2 | Migrations: indexes and `pg_trgm`, then the guarded demo seed | 1 | implementation, database-design | `Migrations/*AddProductionOrderListIndexes*`, `*SeedDemoProductionOrders*` | `dotnet ef database update` against the local Compose database as the owner; both indexes present; 80 seeded rows |done 2026-09-22 — both applied; 80 rows across 55 distinct due dates; no INVALID index left behind |
| 3 | Backend: query object, validation, contracts | 1 | implementation | `ProductionOrderListQuery`, `ProductionOrderListContracts` | `dotnet build`; covered by the unit tests in step 5 |done 2026-09-22 — enum values matched by name only, after a unit test caught `Enum.TryParse` accepting `status=3` |
| 4 | Backend: service, repository reads, controller, telemetry | 3 | implementation | `ListAsync`, `CountOrdersAsync`, `ListOrdersAsync`, query extensions, `ProductionOrdersController.List`, the new span/counter/histogram | `dotnet build` |done 2026-09-22 — sort applied before the projection (EF cannot translate an ORDER BY through a constructor projection); DD-002-FN updated |
| 5 | Backend unit tests | 3, 4 | testing | `Application.Tests` additions | `dotnet test` — allow-lists, date range, fragment normalization and escaping, sort mapping including the status workflow rank, the overdue rule, page arithmetic |done 2026-09-22 — 116/116 pass (67 new) |
| 6 | Backend integration tests | 2, 4 | testing | `Api.Tests` additions (Testcontainers) | `dotnet test` — TC-101, TC-104–TC-111, TC-114, TC-119, including an `EXPLAIN` assertion that the default-sort and fragment queries are index-backed |done 2026-09-22 — 63/63 pass (25 new) |
| 7 | Frontend: view state, validation, components, page, routing | 1 | implementation, screen-design | The files listed in DD-002 X-1 | `npm run lint`, `npm run build` |done 2026-09-22 — lint clean, build clean |
| 8 | Screen A navigation target change, with BD-001/DD-001 updated | 7 | implementation | `ProductionOrderPage`/`ProductionOrderForm`, BD-001, DD-001 | Existing Screen A tests still pass; both documents updated in the same commit |done 2026-09-22 — Cancel, the panels and the breadcrumb return to the list; Screen A unit and E2E tests updated and passing |
| 9 | Frontend unit tests | 7, 8 | testing | `tests/unit/production-orders/` additions | `npm test` — TC-102, TC-103, TC-112, TC-115, TC-116, TC-117, TC-118 (axe) |done 2026-09-22 — 56/56 pass (18 new) |
| 10 | E2E tests | 2, 4, 7 | testing | `tests/e2e/` additions | Compose stack plus `npx playwright test` — TC-101, TC-109, TC-113, TC-114, TC-115, TC-116, plus axe |done 2026-09-22 — 16/16 pass (8 new); axe found two real violations, both fixed rather than waived |
| 11 | Test plan TP-003 | 5, 6, 9, 10 | testing | `work-items/WI-003/test-plan.md` | Every TC-101–TC-119 maps to a real test with its recorded result |done 2026-09-22 — TP-003 written; every case maps to a named, passing test |
| 12 | Full local verification | 2–11 | — | `evidence.md` | `dotnet build`, `dotnet test`, `npm run lint`, `npm run build`, `npm test`, Compose plus Playwright — each result recorded as actually run, with counts |done 2026-09-22 — every command run and recorded with counts; CI recorded as not run, with its reason |
| 13 | Gates: design-consistency, security-review, delivery | 12 | security-review, pr-review | `evidence.md` walks | Every checklist item answered; security-review covers the allow-lists, the `LIKE` escaping, the page-size cap, the role gate and the seed's environment scope |done 2026-09-22 — all three walked item by item in evidence.md |
| 14 | Commit, push, open PR | 13 | pr-review, ci-cd | PR to `master` | CI (backend, frontend, e2e jobs) green on the PR; the diff reviewed against the design set |blocked — committed locally (4 commits); push and PR await the user’s authorization |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Implementer, test author | this agent (Claude) |
| Reviewer, approver, merge | ThanhTN |

### Resources and external actions

| Action (push / PR / merge / deploy / publish image / …) | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local file edits under `src/`, `tests/`, `docs/`, `work-items/WI-003/` | yes | user message 2026-09-22 ("the DD is approved, move on to the implementation") | The WI-003 scope above |
| Create the branch/worktree and commit locally | yes | same | `feature/WI-003-production-order-list` only |
| Run the local Compose stack and migrations against the local database | yes | same | Local only; the seed is guarded to an empty table |
| Push the branch and open a PR | yes | asked and granted at execution, 2026-09-22 ("yes push it and open the PR") | Step 14 — PR #9 |
| Merge the PR | no | not authorized | — |
| Deploy or publish an image | no | not authorized | — |

### Risks and mitigations

| Risk / stop condition | Trigger | Mitigation / response |
| --- | --- | --- |
| `CREATE INDEX CONCURRENTLY` fails part-way and leaves an `INVALID` index | Migration error at step 2 | Follow DB-003's recovery: find it with `SELECT indexrelid::regclass FROM pg_index WHERE NOT indisvalid;`, drop it concurrently, retry. Never mask it by quietly dropping `CONCURRENTLY` |
| The Testcontainers database needs the same extension and grants as Compose | Integration tests fail on a missing extension or privilege | The migration creates the extension, so it applies in both environments; if the test image genuinely lacks `pg_trgm`, that is a stop condition, not a reason to drop the index from the design |
| EF Core cannot translate a designed expression (the status workflow rank, `Contains` over the status set, `EF.Functions.Like` with `ESCAPE`) | Runtime translation error or silent client-side evaluation | Prove each with an integration test over the real database; if one truly cannot translate, record a decision and update DD-002-FN rather than evaluating in memory |
| The seed's relative dates make a test flaky | A test asserts an absolute date | DEC-011 requires assertions on counts, statuses and offsets from today; a date-dependent assertion is a test defect to fix, not a seed problem |
| Scope creep into Screen A | Step 8 invites wider edits | Only the navigation target changes; any other Screen A change is out of scope and needs its own decision |
| CI fails on the PR | Step 14 | Fix and push again; never report a red run as passing, and never weaken a test or a gate to make it green |

### Approval / sign-off

- **Review status:** not reviewed before execution (DEC-014). Corrected 2026-09-22; this line originally read "approved".
- **Approval source:** none for this revision. Steps 1–13 were started on the design approval, "the DD is approved, move on to the implementation", which approved DD-002 and not this revision; the revision was not shown to the user first. Step 14 (push and PR) was authorized separately at execution: "yes push it and open the PR". The user accepted the result by merging PR #9 (DEC-013) and corrected the process afterwards (DEC-014, RFC 0006).
- **Approved revision:** revision 2, 2026-09-22
- **Closure:** steps 1–14 done 2026-09-22. PR #9 was squash-merged by the user as `8eab65f` (DEC-013) with all three CI jobs green, and the branch, worktree and local Compose stack were removed. Work item complete.
