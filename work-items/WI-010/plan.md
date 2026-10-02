# WI-010 — Execution plan

| Revision | Date | Phase | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-01 | Requirements and sequential design | Complete — superseded by revision 2 | Explicit user "approved" after revision 1 presentation, 2026-10-01 |
| 2 | 2026-10-02 | Local implementation and verification | Complete — local handoff approved | Explicit user "approved" after revision 2 presentation, 2026-10-02 |
| 3 | 2026-10-02 | CI fixture preparation and PR delivery | Implementation and publication complete; final record-head checks tracked in PR36 | Explicit user "approved" after revision3 presentation,2026-10-02 |

## Revision 1 — Requirements and sequential design

### Objective and scope

Finalize REQ-070–075 and approved calendar rules, then produce the new 006 design
family in a reviewable sequence. This revision covers requirements/design and
future implementation planning only. Feature selection authorizes drafting this
plan. Execution is now authorized by explicit user approval of revision 1; DEC-001–007 are now confirmed; 006_REQ version 1 including RP-01–05 is approved.

Proposed functional boundary: weekly plant pattern, manual holiday/shutdown and
working-day exceptions, monthly desktop/mobile lookup, optional line exceptions
and dated capacity subject to business confirmation. Explicitly resolve source
PDF working-day due-date/window suggestions. No runtime order/dashboard change
is approved by this proposal; dependent impacts must be defined before design.
If answers materially expand this boundary, present a revised plan first.

No application code, schema execution, dependency change, evidence video output,
commit, push, PR, merge, deployment or old approved design edits in this revision.

### Inputs and existing preparation

- User selected Plant calendar; decisions.md records outstanding questions.
- Baseline 42e8932 includes merged WI-009, Product master and current order rules.
- Existing plant clock/timezone, exact hours/timing/unit semantics and concurrency.
- Source PDF pages 2/3/5 is a proposal; it is not a business specification.
- Isolated planning checkout prepared under the direct feature request:
  C:/Data/project/ProductionManagementAI-WI010, feature/WI-010-plant-calendar.
- Document/screen number 006 proposed; audit before final reservation.

### Deliverables and milestones

| # | Step | Depends on | Skill | Deliverable | Verification / review stop | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Resolve business rules, audit numbering/source and reconcile WI-009 current-state records | Revision 1 approval and dependent user answers | planning / requirements | Confirmed brief/decisions; correct merge/CI/cleanup records in this checkout | Verify PR #34/42e8932 and actual CI links; preserve earlier history/designs/videos; no rule inferred | Done: source/numbering/dependency audit and DEC-001–007 confirmed |
| 2 | Finalize 006_REQ Plant calendar with EN/JA PDFs | 1 | requirements | Stable REQ-070–075, actors and success/failure criteria | Core decisions resolved; source and EN/JA PDFs verified and presented; wait for review including RP-01–05 | Done: explicit user approval of 006_REQ version 1 including RP-01–05 |
| 3 | Assess architecture/security and impacts in work-item records | REQ reviewed | architecture | Assessment and finalized document ownership | Reuse existing boundaries; if new ADR required, revise plan before authoring it | Done: existing architecture reused; security/impact assessment recorded |
| 4 | Write 006_BD_稼働カレンダー and desktop/mobile diagrams | 3 | basic-design | BD, numbered SVGs/navigation and EN/JA PDFs | Calendar states, dates, roles and accessibility consistent; present and wait | Done: BD version 1 explicitly approved with four SVGs and EN/JA PDFs |
| 5 | Write 006_DB_稼働カレンダー | BD reviewed | database-design | DB, effective dates/precedence, concurrency and migration/recovery; EN/JA PDFs | Check range/overlap/fallback/history constraints; present and wait | Done: DB version 1 explicitly approved with EN/JA PDFs |
| 6 | Write main 006_DD_稼働カレンダー | DB reviewed | detailed-design | Module design, traceability and EN/JA PDFs | Reconcile BD/DB and date model; present and wait | Done: main DD version 1 and visual/PDF companions explicitly approved |
| 7 | Write 006_DD-API_稼働カレンダー | Main DD reviewed | detailed-design | API roles, contracts, validation/errors/versions and EN/JA PDFs | Date-only semantics, role and conflict/unknown-outcome paths; present and wait | Done: API version 1 and EN/JA PDFs explicitly approved |
| 8 | Write 006_DD-FN_稼働カレンダー | API reviewed | detailed-design | Resolution/calculation/transaction algorithms and EN/JA PDFs | Testable precedence and date boundaries, no mixed-unit capacity; present and wait | Done: FN version 1 and EN/JA PDFs explicitly approved |
| 9 | Write 006_DD-SPD_稼働カレンダー | FN reviewed | screen-design | Calendar behavior, SVGs, EN/JA HTML mockups and PDFs | Keyboard/month navigation, mobile/zoom, save/conflict/dirty states; present and wait | Done: SPD version 1, two SVGs, EN/JA mockups and PDFs explicitly approved |
| 10 | Write new 006_DD-IMP_calendar-impacts if confirmed scope changes existing screens | SPD reviewed; impacts confirmed in step 1 | detailed-design | New impact addendum, applicable diagrams/mockups and EN/JA PDFs | Existing approved designs read only; reconcile date/capacity behavior; present and wait; record not applicable if no impact | Done: impact addendum not applicable; lookup/capacity remain SCR-006; old order/dashboard semantics unchanged |
| 11 | Reconcile designs/security and prepare test plan | All required designs reviewed | testing / security-review | Design-consistency gate and planned verification | Trace every requirement and exception; record document checks, not unrun application tests | Done: design-consistency and design-stage security review recorded; TP-WI-010 covers TC-366–407, all runtime cases not run |
| 12 | Present implementation plan revision 2 | 11 | planning | Appended implementation revision and status | Show concrete scope/checks/external boundaries; wait for explicit revision 2 approval before code | Done: revision 2 prepared for user review; execution remains pending explicit approval |

Design production order: REQ → BD → DB → main DD → DD-API → DD-FN → DD-SPD →
conditional new impact DD. Each source is completed with its required companion
artifacts and reviewed before the next source is written. Routine records and
test planning do not create additional design review stops.

### Roles and external actions

Agent: plan/requirements/design author. User: business owner, plan approver and
sequential design reviewer. Apply design-consistency and security-review gates;
implementation delivery/release gates belong to later authorized phases.

| Action | Authorization / boundary |
| --- | --- |
| Isolated planning checkout and initial work-item records | Direct feature request; already prepared |
| Revision 1 requirements/design steps and routine merge-record reconciliation | Authorized by explicit revision 1 approval, 2026-10-01 |
| Application implementation/tests/migration rehearsal | Excluded; later approved implementation revision required |
| Commit/push/PR/merge/deploy/publication/live data changes | Excluded; later task-specific authorization required |
| Old approved design changes or video output changes | Excluded |

### Risks and stop conditions

- Missing roles/precedence/history/date-rule answers: ask and stop only dependent work.
- Business scope conflicts or additional existing-screen changes: show revised plan
  and wait for explicit approval before its steps.
- Missing visual/PDF tool: identify actual limitation and follow the approved artifact
  approach; no mockup or document reported as running application evidence.
- Calendar changes do not imply historic order/quantity recalculation or scheduling.
- Protect existing source/design/video artifacts and unrelated user changes.

### Review status and sign-off

Review status: approved. Approval source: explicit user "approved" after this
revision was presented, 2026-10-01. Approved revision: 1. Closure: complete on 2026-10-02, superseded by revision 2.
All sequential designs explicitly approved; conditional impact document not applicable.
Design-consistency and design-stage security gate recorded, test plan prepared and
revision 2 presented for review. Runtime work was excluded and remains unstarted.


---

## Revision 2 — Local implementation and verification

Revision 2, 2026-10-02. Supersedes completed revision 1; prior design approval
permits proposing this revision, not executing it. **Approved by explicit user reply after presentation, 2026-10-02.**

### Objective and scope

Implement SCR-006 Plant calendar against the seven approved version-1 designs,
then prove REQ-070–075 using TP-WI-010 and prepare a reviewable local delivery.

In scope: Japanese /plant-calendar UI and shared distinct navigation entry;
weekly effective definitions, plant/line date exceptions, retained markers/history,
coherent reads and exact current capacity; eleven role-gated APIs; three additive
PostgreSQL tables, restricted grants and explicit owner activation tooling; scoped
telemetry; isolated migration/activation rehearsals and unit/integration/E2E/manual
verification. Add operational instructions needed to run/recover the feature, and
maintain English WI/project records with actual results.

Out of scope: changing existing order due-date/start/dashboard-day rules, working-day
arithmetic, scheduling, holidays feed, BOM/shifts/load allocation, historical capacity,
new auth/provider/package/CI framework, old approved design rewrites, evidence video
output, existing mutable demo/live database writes, commit/push/PR/merge/publication
or deployment. A later task-specific instruction is required for external delivery.

### Approved inputs and assumptions

| Input | Revision / use | Remaining dependency |
| --- | --- | --- |
| Brief/DEC-001–007 and 006_REQ | Approved, RP-01–05 accepted | None |
| 006_BD / 006_DB | Approved version 1; layout, schema, grants/activation/recovery | Runtime implementation and isolated proof |
| Main 006_DD / API / FN / SPD | All approved version 1; complete four-document family | Runtime code and actual checks |
| TP-WI-010 | Revision 1; TC-366–407, all not run | Execute under approved revision 2 |
| Architecture/design/security review | Existing ADRs reused; design gate passed | Repeat security/delivery review against final code |
| Worktree | C:/Data/project/ProductionManagementAI-WI010; feature/WI-010-plant-calendar; baseline 42e8932 | Preserve unrelated changes; dependencies not yet installed in frontend worktree |
| Icon/package | lucide-react locked1.47.0; CalendarRange specified | Verify exact installed export after locked restore; no dependency change inferred |

No global architecture or business decision is unresolved. Existing approved
sources and companions stay immutable. If implementation reveals a design change,
stop its dependent step, present a plan revision and author a new additive design
only after authorization; apply sequential review before dependent code.

### Deliverables and milestones

Execution of the following steps is authorized by revision 2 approval. Follow existing layered .NET/EF/PostgreSQL,
React/TypeScript/Tailwind/router and single Japanese catalog. C# types use XML
comments under the applicable csharp-docs skill; no sub-agent delegation authorized.

| # | Step / deliverable | Depends on | Skills | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- |
| 1 | Verify worktree/baseline/tooling, restore existing locked packages, inspect matching icon export and prepare isolated fixtures | Explicit revision 2 approval | implementation/testing | Read git state, dotnet/npm/Docker versions, existing package locks; no package-version edits; fail early on access/tool gaps | Done: locked restores/tool/icon/isolated fixture checks passed |
| 2 | Implement calendar types, exact validators, resolver/floor and typed results/ports in Domain/Application/PlantCalendar | 1 | implementation/csharp-docs | Backend unit slice TC-366–378; seven floor vectors and full date/unit/marker boundary behavior | Done:47 calendar unit tests pass |
| 3 | Add EF mappings/model snapshot and ExpandPlantCalendar migration; owner activation script with explicit date/timezone and restricted grants | 2 | implementation/database-design | Review generated SQL and column updates; isolated fresh/upgrade/activation/grant/constraint/unsafe-Down cases TC-396–398; legacy data checksums unchanged | Done: fresh/upgrade/activation/grants/Down rehearsal passed |
| 4 | Implement EF calendar repository and eleven application methods; read snapshots, retained transitions, lock/date/version checks and commit certainty | 3 | implementation/testing | Real PostgreSQL atomicity, snapshot, retirement/midnight and rollback/commit fault cases TC-391–400; no replay/settings leakage | Done: snapshot/lock/fresh-date/commit fault tests pass |
| 5 | Implement PlantCalendarController, strict feature parser/Problem Details, DI and scoped request telemetry/no-store | 4 | implementation/security-review | All eleven Admin/Operator/anonymous/other-role contracts; payload/query limits and safe errors; real Activity/Meter accounting TC-389/390/401 | Done: all11 role contracts, strict parser and bounded telemetry checked |
| 6 | Implement feature API client/types/validation, router registration, shared CalendarRange alias and Japanese catalog additions | 5 | implementation | Frontend unit slice TC-377/379–386; opaque exact versions/decimals, request cancellation/auth/unknown semantics; no global API tightening | Done: exact adapter/URL/value/settlement checks pass |
| 7 | Implement calendar month/agenda, weekly/date editors, history, capacity and centered confirmation/dirty navigation | 6 | implementation/screen-design | Frontend TC-379–388, strict build/lint, field/focus and independent read-state behavior against approved SPD/visuals | Done: responsive editors/history/capacity; draft refresh findings fixed |
| 8 | Run isolated composed application journeys and interactive accessibility checks | 7 | testing/security-review | Playwright/axe TC-402–406; desktop/mobile, scrolled native dialog, dirty Back, committed response loss, native 200% zoom and mobile keyboard; record actual manual limitations | Done with accepted limits: six final E2E, axe/native 200% pass; two manual checks remain Not run, user permits local handoff2026-10-02 |
| 9 | Run final existing backend/frontend/E2E regressions; reconcile operational/migration/recovery instructions and test evidence | 8 | testing | Full builds/lint/unit/integration/E2E and TC-407; closed-calendar orders still follow existing rules; actual counts/no hidden skip/retry | Done:239 unit/191 integration/253 frontend/48 full E2E; final affected checks pass |
| 10 | Review complete local diff with design/security/delivery gates; update status/project notes and preserve final artifacts | 9 | pr-review/security-review | Requirement-to-code/test trace, immutable209-artifact check, no secrets, evidence for every result; clean only verified task-owned fixtures; user handoff | Done: local review/security/delivery recorded,209 artifacts preserved, fixtures cleaned; manual limits explicitly accepted for local handoff |

Steps 3–5 are checked incrementally; add tests at the layer where risk arises,
then use full regressions once the final slice is stable. TC IDs are scenario groups,
not a required number of test methods. Do not add cosmetic mirror tests or repeat
full suites without a concrete remaining risk/changed source.

### Verification commands and environments

Executed results are recorded in evidence.md: dotnet build and dotnet test against
src/backend/ProductionManagementAI.slnx; npm ci/build/lint/test in src/frontend;
npm ci, npx playwright install chromium and npx playwright test in tests/e2e.
Use targeted filters while building a slice; record exact executed commands and
final counts/commit/environment in evidence.md. Existing test frameworks/locks
are retained; no new dependency or CI job authorized by this proposal.

Docker/Testcontainers and a distinct Compose project/database/ports host only
throwaway fixtures, with generated credentials supplied in environment and never
printed/committed. Rehearse fresh and baseline upgrade as owner; exercise requests
as pmai_app. Explicit activation chooses a fixture plant-local date/timezone and
inserts first mask31/rev1 atomically; inspect existing activation rather than reset.
No writes/migrations to an existing demo/live database and no container replacement
of existing demo/evidence stacks. Resolve resource names/absolute cleanup targets
before any removal; delete only task-owned fixtures after evidence is retained.

Activation/recovery scripts and deploy/README operational updates document backup,
write pause, bounded DDL, timezone compatibility, unsafe Down and client-token reset
limits. Authoring/rehearsing these instructions is not a live cutover authorization.
Approved designs remain the specification; no deployment success claim.

### Roles, resources and external actions

| Action | Proposed authorization if revision 2 approved | Boundary |
| --- | --- | --- |
| Local code/tests/docs and locked package restore | Yes | This worktree; existing versions; no secrets/dependency upgrades |
| Isolated Docker/Testcontainers/Compose and owner migration/activation | Yes | Only newly task-owned disposable fixtures, no existing mutable demo/live data |
| Fixture cleanup | Yes | Verified task-owned resources and resolved paths; preserve final prior videos/designs |
| Commit, push, PR, merge, published image/Artifact, deployment/live migration | No | Needs later task-specific instruction |
| New package/architecture/business behavior or old approved design edits | No | Stop affected step and resolve through reviewed new revision/addendum |
| Video recording/output changes | No | Independent future evidence request only |

Agent: implementer/test author/reviewer. User: revision approver and delivery
reviewer; approval source must be a response to this presented revision. No standing
external permissions or deployment-host decision is created.

### Risks and stop conditions

| Risk / trigger | Mitigation / stop condition |
| --- | --- |
| EF emits table-wide update/default grants widen permissions | Explicit column changes and real restricted-login tests; fix before delivery |
| Locks/timezone/midnight or uncertain COMMIT mishandled | Controlled clock/races and typed commit-phase fault tests; block retry/false success |
| Runtime exact floor/parser differs from approved contracts | Boundary vectors and strict HTTP/frontend tests; do not clamp/round or weaken checks |
| UI route/state/modal or late read loses draft | Deferred-response tests plus real PC/SP keyboard/history/centering; preserve Unknown input |
| Installed icon/locked tooling unavailable | Verify after restore; if a new dependency/contract needed, stop dependent step and revise plan |
| Isolated fixture cannot be established or old data changes | Stop environmental/migration stage; never use live/demo as fallback |
| Design discrepancy or new existing-screen behavior required | Preserve approved documents; present revision/new design and wait before dependent implementation |
| Required runtime/manual gate cannot run | Record exact blocked/not-run check and risk; no completion claim or silent substitute |
| Request extends to external delivery | Finish concrete local package, then obtain required task-specific authorization |

### Approval / sign-off

- Review status: **approved**.
- Approval source: explicit user "approved" after revision 2 was presented, 2026-10-02.
- Approved revision: 2, local implementation and isolated verification.
- Closure: complete for local scope on2026-10-02 with explicitly accepted manual verification limits; unrun checks remain Not run.

Next action: await task-specific external delivery instruction; local revision2 complete. No implicit commit/push/PR authorization.

---

## Revision 3 — CI fixture preparation and PR delivery

Date:2026-10-02. State: **Approved after presentation**. Revision2 local delivery was
explicitly approved after its handoff. That approval authorizes preparing this
next revision, not executing its steps. Prior complete revisions remain above.

### Objective and concrete scope

Prepare WI-010 for the existing GitHub CI pipeline, commit the reviewed feature,
push its dedicated branch and open a PR to master. Retain approved designs and
accepted manual verification limits. No merge, deployment, live activation or videos.

Concrete readiness finding: `.github/workflows/ci.yml` migrates a fresh disposable
E2E database but does not activate calendar state. New calendar E2E journeys require
activation. Earlier local48-case proof used explicitly activated disposable fixtures;
it did not prove the current CI setup. Add one owner activation step to the existing
E2E job after migration and before API startup. Use the existing activate.sql, fixed
plant timezone from throwaway environment and an explicit plant-local date; generated
credentials remain masked. Do not add runtime autoactivation, jobs, actions, packages,
workflow permissions, repository secrets or live database access.

### Steps and verification

| # | Step | Verification / stop | Outcome |
| --- | --- | --- | --- |
| 1 | Update the existing E2E CI job with explicit disposable calendar activation; reconcile operational/verification records | Review owner SQL invocation, plant date/timezone and failure propagation; keep pinned actions/permissions unchanged | Done: one fixture step authored and actual extracted owner shell rehearsed |
| 2 | Rehearse migration/activation and E2E using fresh task-owned Compose fixtures; run final affected build/test gates | Same setup order as CI; actual full backend/frontend/E2E counts recorded; no hidden retry/skip. Preserve manual Not run with accepted local limits; clean only verified task-owned fixtures | Done: full Release239/195, frontend253, E2E48; owned fixtures cleaned |
| 3 | Review/security/delivery audit of complete WI-010 package and immutable209 artifacts; commit only intended files | Include authorized WI-009 merge-record reconciliation, approved006 designs/companions, code/tests/ops/CI and WI-010 records; exclude credentials/videos/unrelated files | Done: reviewed commit f2a1d82 after conflict-free rebase; protected artifacts verified |
| 4 | Synchronize feature branch with current master including merged RFC0013; push and create a focused PR to master | Commit before rebase, verify rebase preserves feature and immutable artifacts; resolve only in-scope conflicts. Use English PR summary and exact evidence/limitations | Done: pushed feature branch, PR36 opened against master f44283c |
| 5 | Inspect actual GitHub CI results and resolve concrete failures within scope; hand off PR for review | Never claim unrun CI. Fixes that change approved behavior/design require a new revision/additive design. Final PR head/check results recorded | Done: run36975008859 passed all three jobs on implementation f2a1d82; final publication-record head checks tracked in PR36 |

### Authorization if this revision is approved

Permitted: one existing CI fixture step and related routine records, disposable
fixture rehearsal/cleanup, local commits/rebase, feature branch push, PR creation
and bounded in-scope CI fixes. No direct push to master. No squash merge yet.

Excluded: changing old approved design documents, business/order/dashboard behavior,
new UX redesign, new packages/actions/jobs/permissions/secrets, live migration/
activation/deployment, image publication, videos, next feature or other branches.
The user requested only a standalone RFC0013 improvement; its merged proposal does
not authorize rewriting WI-010's approved designs. Retain the user's UX preference
within existing approved behavior and future authorized design work.

### Risks and controls

- Activation mistakenly targets existing data: use a new named project/database,
  verify resources/ports, fail on existing activation; never reset/replay it.
- Credential exposure: use generated environment values and existing CI masks;
  never print connection/password dumps or add secrets to records/PR.
- Rebase or broad staging includes unrelated work: audit explicit paths and current
  master, protect approved artifacts, preserve the main checkout and prior videos.
- CI failure needs a business/design change: stop dependent fix and present a revised
  plan/new additive document; local approval does not permit an old design rewrite.
- CI unavailable: record actual state and cause; no false success or blind retries.

### Review and handoff

Revision3 explicitly approved by the user after presentation on2026-10-02.
Execution authorized for its five steps; merge remains a separate instruction.
