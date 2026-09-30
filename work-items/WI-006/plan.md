# Product master — Plan

This is the first proposed work item in the Master data group. The remaining functions are proposed as separate work items, in dependency order: Plant calendar (independent), Production lines (after Product master), and Bill of materials (after Product master). Their scopes and plans are not approved by this revision.

| Revision | Date | Phase / purpose | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-09-29 | Requirements and design for Product master | closed — superseded after draft artifacts | User message, 2026-09-29: “ok plan đc chấp thuận” |
| 2 | 2026-09-29 | Reconcile Product master with order quantities and mixed-unit dashboard | closed — superseded after brief and `004_BD` draft | User message, 2026-09-29: “approved” |
| 3 | 2026-09-29 | Document cross-screen impacts in new WI-006 artifacts without changing approved prior work-item designs | closed — design-consistency passed 2026-09-30 | User message, 2026-09-29: “approved” (reply to revision 3) |
| 4 | 2026-09-30 | Implement Product master and unit-aware order/dashboard behavior | closed — delivered and reviewed | User message, 2026-09-30: “approved” (reply to revision 4) |
| 5 | 2026-09-30 | Merge approved PR and close out WI-006 | closed by the authorized closeout change | User message, 2026-09-30: “approved” (reply to revision 5) |

## Revision 1 — Requirements and design

Revision 1, 2026-09-29.

### Objective

Define a reviewable Product master feature that lets authorized users maintain automobile-part reference data while preserving historical production orders. Produce requirements, screen/API/database design, and test scenarios before proposing an implementation revision.

### Scope

#### In scope

- Specify product list, create, edit, and retire behavior for SKU, Japanese name, unit, and drawing number.
- Specify how retired products appear on existing orders and in the SCR-001 product picker.
- Specify authorization, validation, migration of the existing 30 seeded products, accessibility, API errors, and observability.
- Record business decisions and produce design artifacts for review.

#### Out of scope

- Application code, migrations, and tests in this revision.
- Production lines, Plant calendar, and Bill of materials; each needs its own work item and plan.
- A project-wide role and permission matrix, production deployment, merge, or deployment.

### Inputs and assumptions

| Input | Revision / state | Assumption or limit |
| --- | --- | --- |
| User request, 2026-09-29 | “let start with master data group” | Begin with Product master because Production lines and Bill of materials depend on it. |
| [Development opportunities source PDF](../../docs/en/pdf/project-development-opportunities.pdf) and the untracked future-functions summary in the main worktree | Proposal, not approved requirements | Candidate fields and retirement behavior require confirmation in the brief and decisions. |
| [Project context](../../ai/project.md), WI-001 DEC-015, WI-005 DEC-001–DEC-005 | Current | Japanese-only UI; exact role matrix remains open. |
| Existing `Product`, `products` table, `GET /api/products`, SCR-001 picker | Implemented | Design must preserve order references and existing product IDs. |
| [Decision log](decisions.md) | Open | Do not settle business behavior by assumption. |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill used | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Draft Product master brief with stable requirement/use-case IDs, success and failure criteria, and explicit exclusions | Approval of this revision | requirements | `brief.md`; any system requirements document required by the workflow | Every requirement has a success path and relevant failure path; no conflicting ID | done 2026-09-29 — `brief.md` defines REQ-049–REQ-056 and UC-015–UC-016; business answers incorporated |
| 2 | Resolve the decisions that affect behavior and authorization | 1 | requirements, planning | `decisions.md` | Each blocking question has an explicit user decision before dependent design | done 2026-09-29 for DEC-001–DEC-004 via user replies; DEC-005 is a technical migration decision for step 4 |
| 3 | Design the Japanese product screens and navigation | 2 | basic-design, screen-design | BD, wireframes, and linked English/Japanese PDFs | Actions, error states, responsive layouts, and WCAG 2.2 AA viewpoints map to requirements | drafted 2026-09-29; consistency review remains open |
| 4 | Design schema migration and API contracts | 2 | database-design, architecture, detailed-design | DB and API designs and linked PDFs | Existing 30 products and production-order foreign keys remain valid; constraints, auth, errors, and migration recovery are specified | drafted 2026-09-29; new quantity/unit decisions require revision |
| 5 | Complete behavior design and test plan; reconcile designs | 3, 4 | detailed-design, testing | DD set, test plan, linked PDFs | Pass `ai/checklists/design-consistency.md`; test scenarios trace to requirements and design | test plan drafted; gate not passed because DEC-007–DEC-010 affect SCR-001–SCR-003 and schema |
| 6 | Summarize design evidence and draft a separate implementation plan revision for review | 5 | planning | `evidence.md`, `status.md`, revision 2 in this file | Actual checks recorded; revision 2 shown to user and explicitly approved before its steps | replaced by revision 2; implementation plan still pending |

### Roles and responsibilities

| Role | Owner |
| --- | --- |
| Plan author and design implementer | Codex |
| Business decision maker and plan approver | User |

### Resources and external actions

| Action | Authorized? | Source of authorization | Scope limit |
| --- | --- | --- | --- |
| Local requirements/design documentation after plan approval | Awaiting approval | This revision | WI-006 requirements/design artifacts only |
| GitHub push, PR, merge, deployment, publication | No | No task-specific authorization | None in revision 1 |

### Risks and stop conditions

| Risk / stop condition | Trigger | Response |
| --- | --- | --- |
| Product-edit permission is unresolved | A screen or endpoint must be gated by a specific role | Ask for the role decision; record it in `decisions.md` before dependent design. |
| Retirement, SKU edits, unit values, or drawing-number rules change business behavior | Brief cannot state acceptance criteria without choosing a rule | Present options and recommendation; record the answer before dependent design. |
| Seeded products conflict with mutable catalog data | A migration or future seed would overwrite user edits or break order references | Design a safe seed/migration strategy and review its recovery limits. |
| Scope expands to other master-data functions | Product master design begins depending on new behavior outside this plan | Draft and show an affected plan revision before that work. |
| PDF rendering dependency or access is unavailable | Required English/Japanese PDF cannot be generated | Record blocked check; do not mark design complete until the artifact is produced. |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** User message, 2026-09-29: “ok plan đc chấp thuận”
- **Approved revision:** revision 1, 2026-09-29 (requirements and design only)
- **Closure:** Superseded on 2026-09-29 after draft artifacts. The design-consistency gate did not pass because mixed units and decimal order quantities materially affect existing screens, API contracts, and schema. Revision 2 covers those affected design steps; no application implementation was started.

## Revision 2 — Reconcile units, quantities, and dashboard design

Revision 2, 2026-09-29, updated for the user's design-Markdown-only review instruction. **Approved and active; step 1 is complete and step 2 is in progress.**

### Objective

Finish a coherent design for Product master and its effects on existing production orders and the dashboard, using the user's decisions DEC-007–DEC-010. Pass the design-consistency gate before proposing implementation.

### Scope

#### In scope

- Amend WI-006 requirements and acceptance checks for product units, historical orders, and measured quantities.
- Specify positive `kg`/`m` order quantities with at most three fractional digits; quantities in `個`, `本`, `枚`, `台`, and `セット` remain whole numbers. Check the existing upper bound of 999,999,999 against API, database, and UI needs.
- Specify that a product's unit cannot change after any order references it. Keep retired-product behavior from DEC-003.
- Amend affected SCR-001, SCR-002, and SCR-003 designs: input, validation, display, sorting/filtering where applicable, summary cards, workload chart, top products, and per-order units. Cross-unit summaries use order counts; quantity totals identify their unit.
- Reconcile Product master BD, DB, DD, API, wireframes, mockups, test plan, and the affected existing-screen design documents. Regenerate English and Japanese PDFs for every changed `docs/en/` document.
- Record actual design checks, remaining limits, and a separate implementation plan for review.
- Produce or revise design `.md` files one at a time. Present each design file with its required diagrams/mockups and EN/JA PDFs; wait for the user's review before writing the next design `.md` file. Resolve feedback on the same design file and present it again if needed. Brief, test plan and work-item records do not create this review stop.

#### Out of scope

- Application code, executable database migrations, automated tests, and deployment in this revision.
- Other Master data functions, inventory, or a general reporting redesign.
- GitHub push, PR creation, merge, publication, or deployment.

### Inputs, dependencies, and assumptions

| Input | State | Design dependency |
| --- | --- | --- |
| Revision 1 draft artifacts | Locally drafted, not yet gate-passed | Reconcile instead of discarding; preserve stable IDs and existing order references. |
| DEC-001–DEC-010 | User decisions; DEC-005–DEC-006 are technical proposals | Apply all settled business rules. Verify the proposed quantity maximum during design. |
| Existing SCR-001–SCR-003 code and designs | Implemented baseline | Document every affected contract and historical-data behavior before implementation. |
| Existing 30 seeded products and 124 seeded orders | Demo baseline | Retain IDs/FKs; assign reviewed units per product without rewriting old quantities. |

### Deliverables and milestones

The design `.md` order below is sequential. Each ADR, BD, DB or DD Markdown file is a separate review stop, even when several files share one numbered step. A changed design file is presented with its updated English/Japanese PDFs and relevant wireframe or mockup. Internal read-only impact mapping and non-design documentation can continue, but no next design `.md` file is written until the user reviews the current design file and explicitly says to proceed.

| # | Document order and step | Depends on | Skill | Deliverable | Verification and review stop |
| --- | --- | --- | --- | --- | --- |
| 1 | Update WI-006 brief with DEC-007–DEC-010 and traceability | Approval of revision 2 | requirements | `brief.md` | Stable requirements with success/failure criteria; no design-document review stop. |
| 2 | Reconcile Product master basic design | 1 | basic-design, screen-design | `004_BD` plus affected wireframes and EN/JA PDFs | Check navigation, fields, states, PC/SP and accessibility; present the design file and wait. |
| 3 | Reconcile affected existing-screen basic designs, in SCR-001, SCR-002, SCR-003 order | User review of each preceding design `.md` file | basic-design, screen-design | One affected BD and its EN/JA PDFs at a time | Check unit display/input and dashboard metric labels; present each changed BD separately and wait. Skip a BD only with a recorded no-change reason. |
| 4 | Reconcile Product master database design, then each affected existing-screen DB design in screen-number order | User review of each preceding design `.md` file | database-design, architecture | One DB document and EN/JA PDFs at a time | Map numeric precision, constraints, migration, history, concurrency and recovery limits; present each changed DB separately and wait. |
| 5 | Reconcile DD documents in screen-number order: affected SCR-001–SCR-003, then SCR-004; within a screen: main DD, API, FN, SPD | User review of each preceding design `.md` file | detailed-design, screen-design | One DD document and EN/JA PDFs at a time; companion mockups/wireframes with their owning DD | Check field/state/error behavior, API/DB mapping, permissions, accessibility and mixed-unit calculations; present each changed DD separately and wait. Skip unchanged DD documents with a recorded reason. |
| 6 | Update WI-006 test plan and evidence after the last design file is reviewed | User review of the last affected design file | testing, planning | `test-plan.md`, then `evidence.md` | Trace scenarios to requirements and design; run the design-consistency checklist and record actual results. No design-document review stop for these records. |
| 7 | Draft a separate implementation plan revision | 6 and a passed design-consistency gate | planning | Revision 3 draft in `plan.md` | Show its code, migration, test, verification and external-action scope for separate explicit approval before any implementation. |

### Roles and permitted actions

| Role or action | Owner / permission |
| --- | --- |
| Design author, local document and PDF edits after approval | Codex, limited to WI-006 and the affected SCR-001–SCR-003 design artifacts |
| Business decisions and plan approval | User |
| App code, migrations, tests, external GitHub operations, publication, deployment | Not authorized by this revision |

### Risks and stop conditions

| Risk | Response |
| --- | --- |
| Existing dashboard card meaning changes when mixed units are introduced | State the replacement metric and label explicitly in BD/DD; trace every dashboard total and chart. |
| Decimal storage or formatting changes the meaning of historical order quantities | Specify additive migration, existing integer preservation, precision/range and recovery limits before implementation. |
| Product unit edit races with an order creation, or an old order retains a retired product | Design server-side checks and transaction behavior; include failure and concurrency scenarios. |
| A newly discovered business rule materially expands behavior beyond this revision | Record the question and show an affected plan revision before dependent steps. |
| Required PDF or gate check cannot be completed | Record the actual blocked result and do not mark design complete. |
| The current design `.md` file has not been reviewed, or feedback is unresolved | Stop before writing the next design `.md` file; revise and re-present the current one. A request to change scope triggers a new plan review. |

### Approval / sign-off

- **Review status:** approved
- **Approval source:** User message, 2026-09-29: “approved”
- **Approved revision:** revision 2, 2026-09-29
- **Closure:** —

## Revision 3 — Preserve completed work-item design baselines

Revision 3, 2026-09-29. Approved after the user's correction that design documents of completed, approved work items must remain unchanged; new documents should be created when needed. This revision supersedes the remaining design steps in revision 2. Its first step is in progress.

### Objective

Complete the WI-006 design for Product master and its effects on SCR-001–SCR-003 while keeping every previously approved WI-002, WI-003 and WI-004 design Markdown and PDF unchanged.

### Scope and document order

Revision 2's completed brief update and current `004_BD` draft are retained. After approval, finish and present `004_BD` with PC/SP wireframes and current English/Japanese PDFs. Then create or revise the following WI-006-owned design Markdown files **in this exact order**, presenting each file with its diagrams/mockups and current English/Japanese PDFs and waiting for an explicit instruction to continue before the next design Markdown file:

| Order | WI-006-owned design file | Purpose |
| --- | --- | --- |
| 1 | `docs/en/010_basic-design/004/004_BD_製品マスタ.md` | Product master screen and high-level impact; current revision 2 draft |
| 2 | `docs/en/010_basic-design/004/004_BD-EXISTING-SCREENS_製品マスタ影響.md` | New addendum for SCR-001–SCR-003 navigation, quantities, units and dashboard metric meaning; new wireframes where behavior changes |
| 3 | `docs/en/database/004/004_DB_製品マスタ.md` | Product schema, seed backfill and references |
| 4 | `docs/en/database/004/004_DB-EXISTING-SCREENS_製品マスタ影響.md` | New addendum for order quantity precision, migration and dashboard query implications |
| 5 | `docs/en/020_detailed-design/004/004_DD_製品マスタ.md` | Product master detailed screen behavior and mockup |
| 6 | `docs/en/020_detailed-design/004/004_DD-API_製品マスタ.md` | Product and affected order/dashboard API contracts |
| 7 | `docs/en/020_detailed-design/004/004_DD-FN_製品マスタ.md` | Product, order and aggregation service rules and concurrency |
| 8 | `docs/en/020_detailed-design/004/004_DD-SPD_製品マスタ.md` | Product screen fields, states and accessibility |
| 9 | `docs/en/020_detailed-design/004/004_DD-EXISTING-SCREENS_製品マスタ影響.md` | New addendum for SCR-001–SCR-003 field/validation/state and dashboard presentation, with needed mockups |

Each new document uses a unique `004_` identifier, lives with WI-006, and links to the unchanged approved baseline it extends. Existing SCR-001–SCR-003 design files and their PDFs are read-only. No prior work-item design file is re-rendered. Work-item records, test plan and evidence can be updated without an individual design-document review stop.

### Remaining milestones

| Step | Deliverable | Verification / stop |
| --- | --- | --- |
| 1 | Finish each design file and companion in the order above | Review one design Markdown file at a time; resolve feedback before the next |
| 2 | Update `test-plan.md` and `evidence.md` after the last design review | Trace REQ-049–REQ-060; run design-consistency gate and record actual result |
| 3 | Draft a separate implementation plan revision 4 | Show implementation, migration, verification and external-action scope for new approval before application code |

### Permitted actions and risks

- Permitted after approval: WI-006 requirements/design records, new WI-006 design files, WI-006-owned wireframes/mockups, and their English/Japanese PDFs. Existing approved design artifacts remain read-only.
- Application code, executable migrations, tests, GitHub push/PR/merge, publication and deployment remain outside this revision.
- If an impact cannot be expressed without changing an approved baseline, record the conflict and ask for a new scope decision; do not silently edit the old file.
- If an existing document or PDF is locked by another process, preserve the completed output separately, record the blocked canonical update and do not present that design file as fully complete until its companion is synchronized.

### Approval / sign-off

- **Review status:** approved.
- **Approval source:** User message, 2026-09-29: “approved” (reply to revision 3).
- **Approved revision:** revision 3, 2026-09-29.
- **Closure:** 2026-09-30 — all nine WI-006 design Markdown files and their EN/JA PDFs were reviewed; `004_DD-FN` version 3 correction was approved; test plan revision 2 traces REQ-049–REQ-060; design-consistency gate passed. Superseded by revision 4 for implementation.

## Revision 4 — Product master implementation and verification

Revision 4, 2026-09-30. Approved by the user in reply to this revision. Revision 3 finished the design stage; this revision authorizes the bounded implementation, verification and feature PR work below.

### Objective

Implement REQ-049–REQ-060 from the approved WI-006 BD/DB/DD set: a Japanese Product master for `Admin` and `Operator`, preserved historical orders, exact unit-aware quantities, and a dashboard that never combines unlike units. Verify fresh and upgraded database paths and keep completed WI-002–WI-004 design baselines unchanged.

### Scope

#### In scope

- Implement SCR-004 list/create/edit/retire and the specified product API with server-side role checks, version conflicts, SKU uniqueness and referenced-unit lock.
- Extend SCR-001/SCR-002 product selection, decimal quantity validation, historical retired-product display, unit presentation and API responses.
- Change SCR-003 workload/top-product/completed metrics and queries to count-based cross-unit views with per-unit subtotals, preserving its existing windows and snapshot semantics.
- Add the two WI-006 schema migrations, reviewed 30-product unit backfill, restricted database grants, exact `numeric` quantity constraints, telemetry, Japanese catalog entries, and meaningful automated tests.
- Rehearse migration against **isolated, resettable** PostgreSQL databases; run local/CI-equivalent build, lint, unit, integration, E2E and accessibility checks; record actual outcomes.
- Create a dedicated `feature/WI-006-product-master` worktree/branch, commit focused WI-006 changes, push that branch and open a PR to `master` after delivery and security gates pass. Approval of this revision would authorize those listed GitHub actions only.

#### Out of scope

- Editing WI-002–WI-004 approved design Markdown/PDFs, Production lines, Plant calendar, BOM, product deletion, import/export or a general role-management UI.
- Applying migrations to the user's mutable demo or production database, destructive reset, live deployment, PR merge, image publication, or changing CI permissions. Those require separate authorization if later requested.
- A zero-write-downtime migration: the approved DB design requires a coordinated cutover and write pause for an actual environment; this revision only rehearses it on isolated test data.

### Inputs and repository state

| Input | State and implementation boundary |
| --- | --- |
| [Brief](brief.md), [decisions](decisions.md), [test plan](test-plan.md) | REQ-049–REQ-060, DEC-001–DEC-011 and TC-303–TC-323 are the acceptance basis; no open business decision. |
| [004_BD](../../docs/en/010_basic-design/004/004_BD_製品マスタ.md), [BD addendum](../../docs/en/010_basic-design/004/004_BD-EXISTING-SCREENS_製品マスタ影響.md), [004_DB](../../docs/en/database/004/004_DB_製品マスタ.md), [DB addendum](../../docs/en/database/004/004_DB-EXISTING-SCREENS_製品マスタ影響.md) | Approved schema, screen effects, exact units, seed mapping and migration/recovery limits. |
| [004_DD](../../docs/en/020_detailed-design/004/004_DD_製品マスタ.md), [API](../../docs/en/020_detailed-design/004/004_DD-API_製品マスタ.md), [FN](../../docs/en/020_detailed-design/004/004_DD-FN_製品マスタ.md), [SPD](../../docs/en/020_detailed-design/004/004_DD-SPD_製品マスタ.md), [existing-screen DD](../../docs/en/020_detailed-design/004/004_DD-EXISTING-SCREENS_製品マスタ影響.md) | Approved request/response, transaction, client state and dashboard rules. FN version 3 is the current approved version. |
| [Evidence](evidence.md) | Design-consistency passed on 2026-09-30; no WI-006 application build, test or migration has run. |
| Current working tree | WI-006 design/work-item files are uncommitted in the shared main worktree alongside unrelated changes, including `scripts/docs-pdf.py` and WI-007 status. Snapshot only WI-006-owned files into the isolated worktree; preserve every unrelated edit and do not implement in the main worktree. |

### Deliverables and milestones

| # | Milestone / step | Depends on | Skill | Deliverable | Verification method | Outcome |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Create isolated branch/worktree and bring in only current WI-006 design/work-item artifacts | Revision 4 approval | implementation, git-review | `feature/WI-006-product-master` worktree with the approved design snapshot | Compare file list and hashes with the approved WI-006 main-worktree snapshot; inspect branch diff and confirm unrelated paths excluded | Done — isolated worktree; 52 initial WI-006 files copied and hash checked |
| 2 | Implement product schema and order quantity migrations; remove current-model `HasData` without `DeleteData`; add seed-unit backfill, index and least-privilege grants | 1 | database-design, implementation | EF model/migrations and owner-run preflight/rehearsal instructions | Fresh and upgrade integration tests on isolated PostgreSQL; confirm 30 IDs/FKs, edited-seed persistence, SKU collation behavior, numeric checks, no accidental data delete or unsafe Down | Done — fresh/upgrade rehearsal, preserved IDs/FKs/values, no DeleteData |
| 3 | Implement Product master domain/Application/Infrastructure/API operations with auth, locks, conflicts and telemetry | 2 | implementation, security-review | Product endpoints, service/repository, Problem Details and bounded spans/metrics | Unit and API integration tests for TC-303–TC-307, TC-311, TC-314–TC-315, TC-322; inspect DB grants and concurrent unit-lock/retire races | Done — role, SKU/version conflicts, row-lock races and bounded telemetry verified |
| 4 | Extend order writes/reads and list for exact decimal quantities, active selection and retained retired references | 2–3 | implementation | Order DTO/service/repository/controller changes, unit-aware list/detail | Unit/integration tests for TC-308–TC-309, TC-316, TC-318–TC-319; verify raw JSON number syntax, transaction locking and historical integer preservation | Done — exact tokens, measured/discrete rules and retired historical selection verified |
| 5 | Update dashboard SQL, mapping and response fields for counts and per-unit totals | 2–4 | implementation | Q2/Q3/Q4/Q5 reader/DTO changes | PostgreSQL and mapper tests for TC-320; assert ten workload buckets, twelve trend buckets, count tie-breaks, snapshot consistency and no mixed quantity | Done — count ranking and exact per-unit subtotals verified |
| 6 | Implement Japanese SCR-004 and affected SCR-001–SCR-003 UI; add precise decimal request/response codec | 3–5 | modern-web-guidance, screen-design, implementation | React pages/components, routes/nav, message catalog, responsive tables/cards and accessible chart table | Frontend component tests for TC-303, TC-312, TC-317, TC-319, TC-321–TC-322; verify 7 units, raw-token rejection, focus, keyboard and 200% zoom | Done — Japanese PC/SP UI, lossless codec, focus and explicit recovery verified |
| 7 | Run full regression, migration rehearsal, E2E/axe and security review; fix findings and record evidence | 2–6 | testing, security-review | Executable TC-303–TC-323 coverage, `evidence.md`, `review.md` as needed | `dotnet build/test`, frontend `npm test`, `npm run lint/build`, Compose-backed Playwright/axe, fresh+upgrade DB checks; apply security-review and delivery checklists, record pass/fail/not run | Done — local build, regression, E2E and security review; see evidence/review |
| 8 | Review focused diff, commit, push feature branch and open PR | 7 with required gates passed | pr-review, git-review | Committed WI-006 branch and PR to `master`; status/evidence updated | Check PR diff excludes unrelated files and protected old designs, CI results and PDF revisions; record PR URL and actual checks. Do not merge | Done — commit 3251ebf pushed; PR #31 open to master; CI passed (run 36682579899); user review pending, merge excluded |

### Execution sequence and limits

Implement and test in small vertical slices. The product migration must precede the order quantity conversion, and both migrations must be proven on fresh and upgrade paths before the new API contract is treated as runnable. The local test database may be reset only when it is an isolated WI-006 test fixture; no mutable user/demo database is reset. For the actual environment, the DB design's backup, preflight, write pause and no-overlap constraints remain a later deployment task.

The browser cannot use binary floating-point arithmetic as the source of truth for quantities. Step 6 must provide a lossless decimal JSON-number path for input, display and per-unit subtotals, then prove it with boundary cases (`0.001`, `1.234`, `1.2340`, `999999999`, aggregation). If the approved JSON-number contract cannot be implemented without a material new dependency or contract change, stop and show the smallest affected design/plan revision before proceeding.

### Roles and external actions

| Action | Authorization proposed by this revision | Limit |
| --- | --- | --- |
| Local branch/worktree, code, migrations, test fixtures, docs and commits | Authorized only after explicit revision 4 approval | WI-006 scope, isolated worktree; preserve main worktree and approved prior-work-item designs |
| Isolated PostgreSQL migration rehearsal / fixture reset | Authorized only after explicit revision 4 approval | Throwaway test database; no user/demo/live database reset or migration |
| Push `feature/WI-006-product-master` and create PR to `master` | Authorized only after explicit revision 4 approval and passing gates | Focused WI-006 PR, no direct push to `master` |
| Merge, live migration, deployment, image publication | Not requested in this revision | Separate task-specific authorization required |

### Risks and stop conditions

| Risk / trigger | Response |
| --- | --- |
| Uncommitted shared-tree WI-006 docs overlap unrelated user changes | Copy only named WI-006 artifacts to the worktree; compare file hashes and diff before the first implementation edit. Stop if provenance is ambiguous. |
| Seed backfill encounters unknown products, duplicate case-folded SKU, or unexpected 30 seed IDs | Abort isolated migration rehearsal and resolve data/design assumption; never assign a guessed unit or delete a row. |
| `numeric` conversion, index build or DB grant differs from approved migration behavior | Fix the migration and rerun fresh/upgrade tests; no live cutover under this plan. |
| Lock race, decimal lexing or lossless browser codec cannot meet the approved API/DB contract | Stop dependent work, record evidence, and seek a design or plan revision if the contract must change. |
| Security, accessibility, integration, CI or delivery gate fails | Diagnose and fix; do not weaken tests or report the feature complete. |
| Push/PR access unavailable | Preserve the committed branch/worktree and record the blocked external action; do not claim a PR exists. |

### Approval / sign-off

- **Review status:** approved.
- **Approval source:** User message, 2026-09-30: “approved” (reply to revision 4).
- **Approved revision:** revision 4, 2026-09-30.
- **Closure:** 2026-09-30 — implementation delivered in PR #31; final head ee21a97 passed backend/frontend/E2E CI. User approved PR review with “approved”. Merge is outside revision 4.


## Revision 5 — Merge and closeout

Revision 5, 2026-09-30. Explicitly approved by the user after this revision was presented; the earlier PR review approval remains a separate approval event.

### Objective and scope

Squash-merge the reviewed WI-006 implementation and complete the required post-merge records. Preserve unrelated main-worktree changes and all approved design baselines. No application change, live/demo migration, deployment or next work-item implementation is included.

### Inputs

PR #31 targets master at reviewed head `ee21a976934bde6d2fb9294cb42723b384900b0c`. GitHub reports CLEAN/MERGEABLE and successful backend/frontend/E2E checks in run 36683178701. The user replied “approved” to the delivered PR. Feature worktree was clean before this review record and proposed plan were added.

### Steps and checks

| Step | Action | Verification / gate | Outcome |
| --- | --- | --- | --- |
| 1 | Recheck PR head, diff, CI and merge readiness; retain review approval evidence | pr-review/security-review/delivery evidence remains valid; stop if head changes or a required check fails | Done — reviewed head ee21a97 unchanged; all three CI jobs passed, CLEAN/MERGEABLE |
| 2 | Squash-merge PR #31 to master using the reviewed head as an exact guard | Confirm GitHub merged state and actual squash SHA; no direct push to master | Done — PR #31 squash-merged as b806b1ca8d216364876147215daff710e302d81e |
| 3 | Create a separate closeout worktree/branch from the merged master; update WI-006 status, evidence, plan closure and brief state, root README, ai/project.md and CLAUDE.md current state in one focused change | feature-delivery step 8; accurately state implemented/merged versus undeployed; no old design changes or new feature scope | Done — closeout branch from merged master; WI-006 records and all three root current-state documents updated |
| 4 | Commit/push the closeout branch and open a documentation PR to master; inspect diff and applicable checks, then squash-merge that focused closeout PR | Delivery gate; no missing required check or unexpected path; document-only CI may be excluded by existing filters | PR #32 — seven Markdown files, valid links, clean diff; CLEAN/MERGEABLE and no required CI run under documentation filters; guarded squash merge authorized |
| 5 | Remove only merged WI-006/closeout worktrees and branches when clean, retaining unrelated main-worktree edits | Verify resolved paths and clean worktree state; stop cleanup if user changes are present | Scheduled — remove only clean merged worktrees after closeout merge; main worktree remains untouched |

### Proposed permitted actions

Approval authorizes squash-merging PR #31, the isolated closeout branch/worktree, focused record edits, commits/push, documentation PR creation and its squash merge after applicable checks, plus clean merged-worktree cleanup. It does not authorize production/demo migration, deployment, image publication, destructive user-data actions or work on the next feature.

### Risks and stop conditions

- A changed PR head, failed CI or material conflict requires investigation before merge; do not substitute an unreviewed change.
- Main-worktree edits remain untouched. Closeout runs in another isolated worktree; any ambiguous overlap or dirty cleanup target stops that affected action.
- Root current-state records describe only tracked, merged capabilities and do not claim a live cutover. The next master-data work item remains a proposal until separately scoped and approved.
- If GitHub access or a required gate is unavailable, preserve completed artifacts and report the actual blocker.

### Approval / sign-off

- **Review status:** approved.
- **Approval source:** User message, 2026-09-30: “approved” (reply to revision 5).
- **Closure:** Implementation merge verified 2026-09-30; this approved closeout PR #32 completes durable record closeout when merged. Clean merged-worktree cleanup follows verified merge.
