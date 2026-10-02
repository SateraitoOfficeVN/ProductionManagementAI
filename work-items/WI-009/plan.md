# WI-009 — Execution plan

| Revision | Date | Phase | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-30 | Requirements and design for Production lines | complete; followed by revision 2 | Explicit user reply "approved" after revision 1 presentation, 2026-10-01 |
| 2 | 2026-10-01 | Implement and verify Production lines | complete — local only | Explicit user "approved" after revision 2 presentation, 2026-10-01 |
| 3 | 2026-10-01 | Commit, publish feature branch and open PR | complete | Explicit user "approved" after revision 3 presentation, 2026-10-01 |

## Revision 1 — Requirements and sequential design

### Objective and scope

Produce reviewable requirements and
sequential designs for [REQ-064–REQ-069](brief.md). The user selected Production lines, Admin/Operator write permissions and
working hours/day plus per-product production time. These answers settle scope
decisions, not plan approval. Resolve [open decisions](decisions.md) before dependent
design. This revision covers requirements and design only; implementation needs
its own newly presented and explicitly approved revision after design review.

No application code, migration execution, runtime recording, dependency change,
push, PR, merge, deployment or old approved design edits in this phase.

### Inputs

- [Brief](brief.md), [decisions](decisions.md) and source PDF: business scope and confirmed DEC-001–DEC-003.
- Master baseline `20c8d61`: Product master and UI fixes already merged.
- Existing React/.NET/EF/Identity, Japanese UI catalog and centralized icon patterns.
- Existing order quantity/unit, status and concurrency contracts remain constraints.

### Deliverables and milestones

Revision 1 approved on 2026-10-01. Steps 1–9 completed; step 10 DD-ORD version 1 submitted for review; DEC-010 resolved.
Later steps await their dependencies and sequential design reviews.

| # | Step | Depends on | Skill | Deliverable | Verification / review stop |
| --- | --- | --- | --- | --- | --- |
| 1 | Create isolated feature/WI-009-production-lines worktree; audit source, numbering and read WI-006/WI-008 records; reconcile WI-008 merge/CI current-state notes | Plan approval | planning | Isolated checkout and accurate baseline/status; no old-design edits | Git state; merge commit and actual CI evidence; preserve unrelated changes; done 2026-10-01 — isolated baseline, numbering audit and current-state reconciliation |
| 2 | Resolve business questions; finalize brief and requirement traceability | 1 and user answers | requirements | Confirmed brief/decisions and new English requirements document with EN/JA PDFs | Success/failure criteria for every requirement; no inferred role/capacity/assignment rule |
| 3 | Assess architecture/security and reserve screen/document numbers | 2 | architecture | SA assessment in work-item records | Reuse existing boundaries; if a new ADR is needed, revise plan for approval before writing it |
| 4 | Write 005_BD_生産ライン・工程 | 3 | basic-design | New BD, numbered desktop/mobile SVG layouts, Mermaid navigation and EN/JA PDFs | Check diagrams/legend, entry/exit routes, accessible states; present this design file and wait for user instruction |
| 5 | Write 005_DB_生産ライン・工程 | BD reviewed | database-design | New DB design with line/product/order references, capacity model and migration/recovery rules; EN/JA PDFs | Resolve constraints, unit/history semantics and old-order strategy; present and wait |
| 6 | Write 005_DD_生産ライン・工程 | DB reviewed | detailed-design | Main DD, state/traceability and EN/JA PDFs | Match BD/DB and confirmed requirements; present and wait |
| 7 | Write 005_DD-API_生産ライン・工程 | Main DD reviewed | detailed-design | API contracts, roles, validation/errors/concurrency/telemetry and EN/JA PDFs | Map endpoint fields and DB constraints; present and wait |
| 8 | Write 005_DD-FN_生産ライン・工程 | API DD reviewed | detailed-design | Function/transaction design and EN/JA PDFs | Verify race handling, referenced products/units and role checks; present and wait |
| 9 | Write 005_DD-SPD_生産ライン・工程 | Function DD reviewed | screen-design | Screen behavior, numbered SVGs, English/Japanese HTML mockups, EN/JA PDFs | Japanese labels, keyboard/dialog behavior, mobile layout and diagrams agree; present and wait |
| 10 | Write new WI-009-owned 005_DD-ORD_production-line-assignment | Screen DD reviewed | detailed-design | New impact design for existing order selection/edit/list; EN/JA PDFs | Read-only comparison with old designs; legacy/retired/status behavior; present and wait |
| 11 | Reconcile requirements/design/security and prepare test plan | All designs reviewed | testing / security-review | Design-consistency gate, security assessment and test scenarios | Map requirements to tests; record actual document checks, not unrun app tests |
| 12 | Present implementation plan revision 2 | 11 | planning | New revision appended after revision 1; updated status | Show scope, steps, checks/actions and wait for explicit revision 2 approval before any code |

The design production order is BD → DB → main DD → DD-API → DD-FN → DD-SPD →
new order-impact DD. Companion PDFs/mockups/diagrams are part of each file's
review package. Feedback is resolved before writing the next design Markdown.
No design file is produced by this initial planning request.

### Roles and external actions

Agent: requirements/plan author; later designer under approval. User: business
rule owner, plan approver and sequential design reviewer.

| Action | Authorization |
| --- | --- |
| Draft these work-item planning records | Current feature request |
| Execute revision 1 requirements/design steps | Authorized by explicit user approval on 2026-10-01 |
| Application code/tests/migrations | Excluded; requires approved revision 2 |
| Push/PR/merge/publish/deploy | Not authorized for WI-009 |

### Risks and stop conditions

- Changed feature/scope selection: revise the bounded plan for explicit approval.
- Missing business rule: ask and record the reply; dependent design waits.
- Time-based capacity must define working-hours limits and production-time quantity basis, especially kg/m; no invented cross-unit totals or scheduling promises.
- Unexpected architecture/impact scope: present a revised plan before its work.
- Preserve completed approved designs; use new documents for order impacts.
- Apply design-consistency before dependent implementation and security/delivery gates at their applicable later stages; release-readiness only for authorized deployment.
- New docs/en documents require current English and Japanese PDFs naming their revision.

### Approval / sign-off

- Review status: approved.
- Approval source: explicit user reply "approved" on 2026-10-01, responding to the presented revision 1.
- Approved revision: 1, dated 2026-09-30.
- Closure: 2026-10-01 — requirements and all seven sequential design files approved; final design/security assessment and test planning complete; revision 2 prepared for presentation in this turn. No application implementation executed.

### Execution outcomes — 2026-10-01

| Step | Actual outcome |
| --- | --- |
| 1 | Done: isolated checkout and verified WI-008 merge/CI current-state reconciliation |
| 2 | Done: DEC-001–DEC-009 confirmed; 005_REQ version 1 and current EN/JA PDFs prepared and validated |
| 3 | Done: SA assessment, no new ADR; SCR-005/UC-017/018/FN-032–036 and document group 005 reserved |
| 4 | Done: 005_BD version 1, two numbered SVGs and current EN/JA PDFs explicitly approved by the user on 2026-10-01 |
| 5 | Done: 005_DB version 1 and current EN/JA PDFs explicitly approved by the user on 2026-10-01 |
| 6 | Done: main 005_DD version 1 and EN/JA PDFs explicitly approved by the user on 2026-10-01 |
| 7 | Done: API version 2 and EN/JA PDFs explicitly approved by the user on 2026-10-01 |
| 8 | Done: DD-FN version 1 and EN/JA PDFs explicitly approved by the user on 2026-10-01 |
| 9 | Done: DD-SPD version 1 and SVG/HTML/PDF companions explicitly approved on 2026-10-01 |
| 10 | Done: DD-ORD version 1 and all companions explicitly approved on 2026-10-01 |
| 11 | Done: full-family design consistency and design security assessment recorded in review.md; TC-326–365 planned in test-plan.md, all runtime cases not run |
| 12 | Revision 2 prepared for presentation with this turn; approval pending; no execution |

Revision 1 outcome: design phase complete. Revision 2 below is proposed for
explicit approval; no application execution or implicit authorization inferred.


---

## Revision 2 — Implement and verify Production lines

Revision 2, 2026-10-01. **Approved; local execution complete.**
Follows completed revision 1 above. The explicit user reply "approved" after
revision 2 presentation on 2026-10-01 authorizes its local implementation steps.

### Objective and scope

Implement approved REQ-064–REQ-069 across SCR-005 and line-related additions to
SCR-001/SCR-002, with isolated migration rehearsals, actual runtime verification
and a reviewable local delivery. Preserve existing Product master, order quantity/
status/concurrency and dashboard contracts.

In scope: Domain/Application/Infrastructure/Api models, mappings, two staged
additive migrations and restricted grants; all seven approved feature endpoints;
atomic timing/unit/retirement flows; order presence-aware nullable line assignment
and projection; Japanese React UI, responsive table/cards, centered confirmations,
dirty/conflict/unknown handling; existing OTel pipeline registration and bounded
metrics; tests, local isolated environment, execution evidence and code/security
review. Update only routine records and necessary implementation/runbook docs.

Out of scope: old approved design edits, new business rules/roles/frameworks/
dependencies, scheduling/overload/automatic allocation, BOM/calendar/results,
historical timing recomputation, live/demo DB/data/volumes, deployment, video
production/output changes and unapproved external GitHub operations. Existing
four final videos remain preserved. Any necessary design change uses a new
addendum and a newly presented plan revision before affected work.

### Inputs and assumptions

| Input | Revision / status |
| --- | --- |
| Brief / DEC-001–010 | Confirmed; unchanged |
| 005_REQ, 005_BD, 005_DB, main DD, FN, SPD, ORD | Version 1; design sources approved and immutable |
| 005_DD-API | Version 2; approved |
| review.md / test-plan.md | 2026-10-01 design gates; TP-WI-009 revision 1; all runtime cases not run |
| Checkout | C:/Data/project/ProductionManagementAI-WI009, feature/WI-009-production-lines, baseline 20c8d61 |
| Environment | Existing .NET/Node/Docker/Chrome/PostgreSQL17 tooling and locked packages; inspect availability during step 1, do not assume a working runtime |

### Deliverables and milestones

| # | Step | Depends on | Skills | Deliverable | Verification | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Recheck isolated checkout, approved hashes, current source/migration/grant patterns and local tooling; prepare only throwaway test topology | Explicit revision 2 approval | implementation / testing | Bounded file map, isolated DB/Compose identifiers and credential-safe commands | Git state; no unrelated overwrite; Docker/tool availability; source hashes; fixture targets confirmed separate from live/demo | Done: isolated checkout/tooling and disposable topology confirmed |
| 2 | Implement line/pair domain and EF mappings, product-unit revision trigger and nullable order assignment; add ExpandProductionLines and IndexProductionLineAssignments migrations, restricted grants and guarded Down | 1 | implementation / csharp-docs / testing | Additive schema/migrations and migration fixtures | TC-349/350/357–359: direct numeric checks, trigger ABA, exact grants, fresh/upgrade catalog, concurrent index validity/recovery, legacy rows unchanged and unsafe Down rejected on isolated data | Done: fresh/upgrade/numeric/grant/recovery migration verification and final review pass |
| 3 | Implement Application ports/use cases and seven API operations with strict feature-owned schemas, exact decimals/versions, snapshot reads, atomic writes/locks, deadlines, errors and registered telemetry | 2 | implementation / csharp-docs / testing | ProductionLine service/repository/controller/contracts; backend tests | TC-326–330/333/334/346–352/355/356: duplicates, parent xmin, cross-action rollback, request caps/auth, snapshots, known rollback versus unknown, redacted once-only spans/metrics | Done: strict API, snapshot, atomic write, telemetry and synchronized race verification pass |
| 4 | Extend existing order contracts/Domain/service/projections for lineId presence, eligible assignment/start, outside-Draft lock and historical/null exceptions using the existing transaction | 3 | implementation / csharp-docs / testing | Order extension plus current line detail/list projections | TC-331/332/353/354 and prior order tests: omitted vs null, old clients, product changes, status/xmin and retirement/unit races with synchronized independent connections | Done: presence/history/start, product/unit/retirement locks and competing order xmin checks pass |
| 5 | Implement SCR-005 Japanese routes, shared Factory nav, forms, cross-page timing editor and native centered confirmations; enforce matching role/validation/draft/error guards | 3 | implementation / screen-design / testing | Master React UI/API adapters/catalog and frontend tests | TC-335–341/360/361: URL history, string precision, opaque revisions, keyed paged intents/errors, explicit reconfirmation, staged retirement, Cancel/Escape and pending/unknown handling | Done: Japanese list/editor, cross-page intents/errors, confirmation, modal and pending/unknown checks pass |
| 6 | Implement ORD additions to order form/picker and table/cards; preserve original quantity serializer, success routing, field/status locks and list state | 4 and 5 | implementation / testing | Order selection/display and catalog/unit tests | TC-342–345/362: current/history summary independent from choices, product clearing/late response, legacy null, non-sortable column, old quantity/version/list behavior | Done: historical/current picker, product clearing, nullable projections and original order regression pass |
| 7 | Run targeted and full backend/frontend regression, isolated app E2E, axe/manual keyboard/zoom/mobile/dialog checks; verify actual telemetry and migration results | 2–6 | testing | Real results for TC-326–365; redacted evidence and required fixes | Existing build/test/lint commands in test-plan.md; disposable PostgreSQL and isolated Compose project; actual counts/results, no silent skips/flaky retry or reused live data | Done: final build/lint; 192 unit / 157 integration / 196 frontend / 42 E2E pass; native zoom verified |
| 8 | Review implementation diff/security, reconcile observed behavior with immutable designs, update required runbook/records and report reviewable local delivery with remaining limits | 7 | security-review / pr-review | Code/security review, current evidence/status and local delivery summary | Full design/security/delivery checklists against code and actual results; no secrets; no old design changes; all required cases dispositioned; clean diff checks | Done: design/security/delivery review, runbook and current records complete; local only |

### Roles and external actions

Agent implements/verifies/reviews after approval; user approves this revision and
business changes. No delegation or sub-agents requested.

| Action | Authorization on approval of this revision | Limit |
| --- | --- | --- |
| Local source/test/migration and routine documentation edits | Yes | Feature checkout and approved feature scope; old designs immutable |
| Restore packages from existing lockfiles if missing | Yes | No package upgrades/new dependency or global tool changes |
| Create/run/reset test containers/databases and isolated Compose stack | Yes | Unique disposable fixture names/ports/volumes only; verify targets before cleanup; no main/demo DB/volume |
| Execute owner migrations/recovery rehearsal | Yes | Only bounded isolated fixtures for fresh/upgrade/failed-index cases; no live repair or unsafe schema removal |
| Local builds/tests and temporary review artifacts | Yes | Record actual results; no four-video output changes; no secrets |
| Git commit, push, PR creation, merge, publish/deploy | No | Requires task-specific authorization after local work is reviewable |

### Risks and stop conditions

| Risk / trigger | Mitigation and stop |
| --- | --- |
| Implemented behavior conflicts with approved design or needs a business choice | Pause affected work; present new addendum/plan revision and wait; do not edit old designs |
| EF sends unit_revision or immutable columns despite restricted grants | Verify generated operations; server-owned revision and column grants enforced by real runtime-role tests |
| Lock ordering/xmin fails under retirement/unit/status concurrency | Share scoped transaction/context, canonical lock order and final order xmin; use barrier-driven integration tests; no stale tracked reads |
| Commit acknowledgement lost / connection cleanup uncertain | Track outcome, fresh bounded cleanup, no automatic replay; expose guarded verification and preserve draft |
| Existing index stage interrupted or Down would destroy used data | Inspect catalogs in isolated fixture; owner-only bounded forward repair; reject unsafe Down; no automatic live repair |
| Existing app/ports/volumes could be touched | Inspect resolved fixture targets and allocate unique isolated project; never stop/reset main/demo services |
| Tool/container access unavailable | Record specific not-run/blocker and continue independent work; obtain required access before dependent checks |
| Dependency/auth/topology change or external action needed | Revise bounded plan/request authorization before proceeding |
| Required verification fails or flakes | Fix concrete risk; record failure/quarantine; do not weaken gate or report unrun checks passing |

### Approval / sign-off

- Review status: **approved**.
- Approval source: explicit user "approved" after revision 2 presentation, 2026-10-01.
- Approved revision: **2**, dated 2026-10-01.
- Closure: 2026-10-01 — all eight local steps complete; external actions remain excluded.

Review this revision's scope, eight steps, isolated migration/test permissions,
risks and external-action exclusions. Execution begins only after explicit
approval responding to this presented revision 2.


### Revision 2 final outcome — 2026-10-01

All eight local implementation/verification/review steps completed. Final
frozen-source results: 192 unit, 157 integration, 196 frontend, 42 E2E pass;
no skips or automatic retries. Full case disposition and resolved review
findings are in test-plan.md/review.md/evidence.md. Approved design hashes
unchanged. Disposable test resource cleanup is recorded in evidence.md.
Commit, push, PR, merge, publication and deployment remain excluded and unrun.
No new plan revision or authorization inferred from local completion.


## Revision 3 — Commit and pull request delivery

### Review status and scope

Current revision: approved by explicit user "approved" after presentation on
2026-10-01. Scoped commit/push/PR/CI steps are authorized; merge and deployment
remain excluded. Revision 2 is complete.

Deliver the existing verified WI-009 change on feature/WI-009-production-lines
from baseline 20c8d61 to origin (SateraitoOfficeVN/ProductionManagementAI), with
one focused commit and a pull request targeting master. Include the approved 005
design companions and revision 1 WI-008 merge-record reconciliation. Preserve
unrelated user files, completed approved designs and the four final videos.
No new feature, design revision, dependency, deployment or video output is in scope.

### Steps and verification

| # | Step | Deliverable | Verification / stop condition | Outcome |
| --- | --- | --- | --- | --- |
| 1 | Audit tracked/untracked files, branch/base and staged diff; reconcile only routine current-state records for commit | Explicit authorized staging set; current records | Confirm no secrets, generated test outputs, unrelated files or protected-design edits; diff --check; stop for unexpected base divergence or unrelated changes | Done: baseline unchanged, 123-file scope, hashes and staged diff verified |
| 2 | Commit the reviewed delivery as feat(WI-009): Production lines and order assignment | One local feature commit | Inspect commit file list and staged scope; retain recorded 192/157/196/42 test evidence; rerun affected checks only if application changes become necessary | Done: implementation commit 810ea06, scope verified |
| 3 | Push the feature branch and create the WI-009 PR targeting master | Remote feature branch and PR URL | No direct master push or force push; inspect existing PR first to avoid duplicates; PR describes behavior, migrations, actual verification and limits | Done: feature branch pushed, PR #34 targets master |
| 4 | Monitor required CI checks and reconcile delivery records | Actual CI result and review handoff | Record passed/failed/pending checks on the exact head SHA; if an in-scope CI fix is necessary, use a follow-up commit and rerun affected gates; stop if it requires design/business/scope changes | Done: all three jobs passed on final head 6f49c31, CI run 36832958896; PR #34 handoff recorded |

Steps are sequential and require explicit approval of revision 3 before step 1.
Apply pr-review plus security-review/delivery checklists before publishing. Record
actual remote results in evidence/status; never claim merge or release from CI.
Routine record follow-up commits are permitted when remote PR/CI evidence becomes
available, with CI checked again on the final head. Do not amend published history.

### Permitted actions after approval

| Action | Boundary |
| --- | --- |
| Stage and commit WI-009 delivery plus approved WI-008 reconciliation | Isolated feature checkout only |
| Push feature branch; create/update this PR description; read/check CI | origin only, master as PR base; no unrelated messages or issues |
| Correct an implementation/CI defect within the existing approved design | Record failure and fix; rerun affected checks and gates; no silent weakening |
| Merge, delete branch/worktree, deployment, image publication, live migration | Excluded; requires later task-specific authorization |
| Change approved designs, add feature scope or alter video outputs | Excluded |

### Risks and handoff

The schema includes owner-run expand/concurrent-index phases. This PR does not
execute live migrations. Previously recorded local tests are evidence for the
reviewed source; remote CI remains not run until publication. Remote master
advancement, unavailable authentication or a failing gate must be reported with
actual evidence; no forced history rewrite or inferred merge authorization.
After final CI results, present the PR and remaining limitations for user review.

Approval source: explicit user "approved" after revision 3 presentation,
2026-10-01. Closure: complete; scoped delivery and exact-head CI verified. Later
explicit user instruction authorized squash merge and subsequent cleanup,
recorded in decisions/evidence/status. No deployment.
