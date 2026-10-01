# WI-009 — Planning evidence

2026-09-30 baseline: master `20c8d61`; main checkout clean before these records.

- Read project/policies/common/documentation rules, feature-delivery workflow,
  requirements/planning skills, templates and design-consistency checklist.
- Read the source PDF locally: all five pages. Production lines follows Product
  master in its master-data section; the suggested waves are proposals only.
- Initial PDF extraction failed because PyMuPDF was unavailable. A pinned pypdf
  6.1.1 reader installed under the OS temporary directory successfully extracted
  all five pages. Project package files and application dependencies unchanged.
- WI-008 merge is known from the completed prior operation: PR #33 squash commit
  `20c8d61`, with Backend/Frontend/E2E successful on tested head `e08208d`.
  Tracked project/work-item text still reflects PR-time state; plan step 1 includes
  reconciliation after approval. Earlier local cleanup artifacts were removed
  per the user's request to retain only four final output videos.
- No design-consistency, security, delivery or release completion claimed for the
  new feature. No application code, migration or test executed for WI-009.
- Three scope questions sent: feature choice, line write roles and capacity model.

Planning-record validation passed: all five files exist, their relative links
resolve and no line has trailing whitespace.

User replies received: Production lines selected; Admin and Operator selected;
capacity uses working hours/day and per-product production time. Records updated
with these decisions; no plan approval or implementation inferred.

## Revision 1 execution — 2026-10-01

- Explicit plan approval received after revision 1 presentation.
- Created isolated feature/WI-009-production-lines checkout at `20c8d61` and
  copied only the five WI-009 planning records. Main drafts preserved.
- GitHub PR #33 API confirms MERGED at 2026-09-30 10:09:34 UTC as
  `20c8d61f1eb6a36e8b59877457e3fc4beb80f6aa`. Run 36699874595 confirms
  successful Backend, Frontend and E2E jobs on tested head `e08208d`.
- Source audit: no ProductionLine/LineId model in Domain/Application; order
  currently has ProductId, decimal Quantity and status transitions with
  product/quantity locking outside Draft. Product contains Unit, IsActive
  and xmin concurrency. Product master uses repository ports and share-lock
  protection for new selections. These existing contracts constrain new design.
- Numbering audit: screen/document folders 001–004 in use; 005 not allocated.
  Existing Product master uses UC-015/016. SCR-005, UC-017/018 and REQ-064–069
  reserved for WI-009 pending finalized requirements. No new ADR concluded yet:
  step 3 depends on completed step 2.
- WI-008 work-item and root current-state notes reconciled in this isolated
  checkout, preserving approval-history correction and completed designs.
- Sent three business questions: line code/retirement history, assignment
  obligation/legacy orders and time measurement/precision. Dependent designs
  wait for answers. No application or runtime verification started.

User confirmation received on 2026-10-01: immutable trimmed case-insensitive
line codes; retired lines retained on unchanged historical orders but excluded
from new selection. Remaining assignment/time/association rules await replies.

User chose optional assignment in Draft, required compatible active line when
transitioning to InProgress, and no mandatory backfill for old InProgress/Completed
orders. DEC-005 recorded; other pending replies remain prerequisites.

User confirmed DEC-007: positive working hours/day <=24, at most 3 decimals;
positive minutes per one product unit, at most 3 decimals, per line/product.
No kg/m conversion or production-batch assumption is inferred.

Explicit user answer recorded as DEC-008: lock line changes outside Draft;
legacy orders without a line may retain that state during other edits.
No association-retirement answer inferred; its dependent design remains pending.

User confirmed association retirement: preserve history and unchanged existing
assignments, block new selections; current timing edits do not recalculate
history. DEC-006 settled. Source audit identified DEC-009: an unreferenced
product can change unit, so time per unit must not silently change meaning.
User question sent; no answer assumed.

## Requirements, architecture and first design package — 2026-10-01

DEC-001–DEC-009 now answered. Explicit DEC-009 answer retains Product master unit
edit rights, blocks old-unit timing for new selection/start and requires explicit
current-unit reconfirmation. Requirements finalized as 005_REQ version 1.
Architecture assessment reuses current layers/auth boundary; no new ADR needed.
Only 005_BD was authored; no later design Markdown exists.

Actual document verification:

- Repository scripts/docs-pdf.py produced 005_REQ EN/JA (3 pages each) and
  005_BD EN/JA (10 pages each); source footers version 1, 2026-10-01 on every page.
- Temporary tooling: Markdown 3.9 and PyMuPDF 1.26.4 installed outside project;
  no project dependency/lockfile changes. Japanese translations and review
  screenshots are temporary, not committed Japanese Markdown or video outputs.
- Windows default stdout encoding rejected Japanese path printing after the
  first BD PDF was produced. PYTHONIOENCODING=utf-8 corrected the invocation;
  both companions regenerated. A HTML rebuild initially missed its temporary
  PYTHONPATH; setting it fixed the tool command without renderer/source changes.
- Initial diagram screenshots captured temporary measurement SVGs before
  Mermaid completion. Final checks waited for actual node counts. EN/JA both
  render two diagrams with 9 total nodes (5 navigation, 4 session-exit nodes).
- Actual visual inspection found the original edit-retirement/401 edge labels
  overlapped. Session exits were separated while preserving all 15 declared
  transitions. Revised diagrams and final EN/JA PDF page 3 were visually inspected
  and readable; no raw Mermaid source appears in any PDF page.
- Both SVG files parse as XML, render in Chrome, and contain callouts exactly
  1–9 matching the BD legend. Both wireframes inspected visually.
- EN/JA heading counts and REQ/FN/SCR/UC ID sets match. All work-item/requirements/
  BD relative links resolve. Every PDF footer verified and sampled pages inspected:
  requirements page 1; BD architecture/navigation pages and final pages.
- Scoped git diff contains no old design, application, test, dependency or CI
  configuration edits. Current-state/WI-008 record reconciliation is the approved
  step 1 scope; application verification for this new feature has not run.

### Applicable gate at BD review

Requirements have stable IDs and success/failure criteria. BD covers primary,
exception and dirty-navigation states. Screen-table entries/exits match both
Mermaid diagrams. Numbered regions match both wireframes. Security-sensitive
operations, accessibility, nullable legacy migration/history and bounded telemetry
obligations identified. Both new docs/en Markdown files have current EN/JA PDFs.
These requirements/BD-level checks passed. DB/DD/API consistency, final security
and delivery gates are pending their future stages; no implementation-ready or
release-ready claim. Sequential stop: review 005_BD before writing 005_DB.

## DB design package — 2026-10-01

The user's explicit approval of 005_BD authorized revision 1 step 5 only.
005_DB version 1 authored with two new tables, product-unit revision, nullable
order assignment, ER diagram, constraints/indexes, least-privilege grants,
transaction locking, migration staging and recovery limits. Technical choices
are review proposals; no SQL, migration, application test or code change run.

Actual document verification:

- Repository docs-pdf.py rendered EN (10 pages) and JA (9 pages). Every page
  contains the version 1 (2026-10-01) source footer; no raw Mermaid source remains.
- Chrome completed both four-entity ER diagrams. EN PDF page 3 and JA page 2
  visually inspected: entities, relationship labels and keys readable without
  clipping. EN page 10 and JA page 9 inspected for table wrapping and footer.
- EN/JA headings and stable requirement/function/screen/use-case ID sets match.
  DB and work-item relative links resolve. The approved BD SHA-256 matches its
  recorded approval hash; no approved BD source or companion was revised.
- PostgreSQL 17 primary documentation checked for numeric scale/typmod behavior,
  row locks, nullable composite FKs, trigger behavior and concurrent-index failure:
  [numeric](https://www.postgresql.org/docs/17/datatype-numeric.html),
  [locks](https://www.postgresql.org/docs/17/explicit-locking.html),
  [constraints](https://www.postgresql.org/docs/17/ddl-constraints.html),
  [indexes](https://www.postgresql.org/docs/17/sql-createindex.html),
  [triggers](https://www.postgresql.org/docs/17/sql-createtrigger.html),
  [trigger functions](https://www.postgresql.org/docs/17/plpgsql-trigger.html).
- Temporary translations, HTML and visual-review images remain outside project
  and the evidence video output. No project dependency changes.

Applicable DB-stage design-consistency checks: REQ-064–REQ-069 map to schema and
later verification scenarios; BD capacity/history/unit/legacy rules preserved;
additive migration and recovery limits specified; authorization/grants and
concurrency-sensitive fields identified. These DB-stage checks passed. Full
API/DD consistency, runtime SQL tests, security/delivery/release gates remain
pending at their applicable later stages. Sequential stop: review 005_DB before
main 005_DD. No new plan revision or implementation authorization inferred.

## Main DD review package — 2026-10-01

The user's explicit approval of 005_DB authorized plan revision 1 step 6 only.
Main 005_DD version 1 authored; API/FN/SPD/order-impact Markdown files remain
unwritten. The approved plan assigns detailed SVGs and EN/JA HTML mockups to
step 9, so this main DD embeds unchanged approved BD PC/SP SVGs and maps regions
1–9. Mandatory companions are reserved by name without broken placeholder links;
the DD family is not claimed complete or ready for implementation.

Actual verification:

- Repository docs-pdf.py rendered the final English PDF (15 pages) and Japanese
  PDF (14 pages). Every page contains 005_DD version 1 (2026-10-01). No raw
  stateDiagram-v2 source remains in extracted PDF text.
- EN/JA heading counts and REQ/FN/SCR/UC ID sets match. Two Mermaid diagrams in
  each translation have exactly 8 list and 9 form edges; each diagram edge has
  the corresponding from/to row in its transition table.
- Headless Chrome waited for both actual state SVGs, then captured temporary
  diagrams. Final English state pages 11/12, Japanese state pages 10/11 and
  final pages EN 15 / JA 14 visually inspected for labels, arrows and table text.
- Local work-item/DD links resolve. Approved BD and DB source hashes match their
  respective recorded approvals. Only main DD Markdown exists in group 005;
  no following design document created ahead of review.
- Authorization text checked against current ProductMasterController: its read
  policy is role-gated, so the main DD does not grant all authenticated users a
  new entitlement. Exact read-policy mapping belongs to DD-API review; confirmed
  Admin/Operator write permissions remain unchanged.
- Parent aggregate xmin, product-unit revision/ABA guard, exact numeric bounds,
  retirement history and legacy null/order lock boundaries checked against DB.
  API catalogs, exact timeout/query bounds and step-by-step flows remain later
  companion work rather than duplicated/unconfigured claims.

Applicable main-DD design-consistency checks passed for fields/states against
approved BD/DB, transition tables, numbered region references, stable requirement
mapping, accessibility and observability obligations. Full companion/API/schema
reconciliation and security/delivery/release gates are pending at their planned
stages. No application code/test, migration execution, dependency change, push,
PR, merge or deployment performed. Temporary translations/HTML/screenshots are
outside the repository and four-video evidence output.

Sequential stop: user reviews main 005_DD before DD-API authoring. Plan revision
1 remains unchanged and approved; implementation still requires approved revision 2.

## API independent draft — 2026-10-01

The user's explicit main-DD approval permits plan revision 1 step 7 only.
Current ProductMasterController/Program policy confirms Admin/Operator reads;
main DD reserves exact read-policy mapping. Asked DEC-010 explicitly rather
than inferring the new screen's read entitlement. Its reply remains pending;
read-dependent policy not finalized, no permission or API implementation added.
Independent schema/error/concurrency/transport sections drafted in 005_DD-API.

Actual document checks:

- Repository docs-pdf.py produced API draft EN (10 pages), JA (9 pages), each
  footer naming 005_DD-API version 1 (2026-10-01); both state DEC-010 pending.
- Translation heading counts and REQ/FN/SCR/DEC/API-PL/API-PO ID sets agree;
  all seven API-PL entries present. Relative links and whitespace validated.
- Visually inspected English error table on page 8 and Japanese aggregate-update
  and error tables on pages 5/7; text and columns readable, no clipping observed.
- Approved BD, DB and main-DD SHA-256 values match recorded approvals. Exactly
  two DD Markdown files exist in group 005; FN/SPD/order-impact not authored.
- Primary documentation consulted for Problem Details and timeout semantics:
  [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457.html) and
  [PostgreSQL 17 connection defaults](https://www.postgresql.org/docs/17/runtime-config-client.html).
  Source facts inform the design; timeout limits are proposed, not configured.
- Existing API/controller/client inspected read-only. No application code, SQL,
  migration, test, project dependency, push/PR/merge or deployment performed.
  Temporary Japanese source and PDF review images remain outside repository.

Partial API-stage consistency: exact numeric bounds, aggregate xmin, composite
pair references, product unit revision, retirement/history and legacy null agree
with approved inputs. DEC-010 is unresolved, so API-stage gate is not called
complete. Full FN/SPD/order/security reconciliation awaits later documents.
Next: resolve DEC-010, finalize read-dependent sections and current PDFs, then
present final API for review before DD-FN. Plan revision 1 scope unchanged.

## API version 2 finalization — 2026-10-01

The user replied "approved" to the proposed Admin/Operator read access and API
handoff requesting its confirmation. This acceptance interpretation was stated
in commentary and recorded under DEC-010. Finalized read policy reuses existing
ProductionOrderEditor for all seven API-PL operations; existing order access
unchanged. No broader role matrix or implemented authorization claim.

Actual final verification:
- Regenerated both PDFs from API version 2 and its temporary full Japanese
  translation: English 10 pages, Japanese 9 pages. All page footers identify
  version 2 (2026-10-01), with no stale version 1 footer.
- Translation headings and requirement/function/screen/decision/API IDs match.
  Both sources contain the exact existing policy name and no pending DEC-010.
- Visually inspected both revision-2 first pages: revision history, document
  state and confirmed read-policy row readable, without clipping.
- Approved BD, DB and main-DD hashes still match recorded approvals; only main
  DD and API Markdown exist. FN/SPD/order-impact authoring has not begun.

API-stage design consistency passed for confirmed roles, exact numeric/version
mapping, atomic pair changes, legacy order semantics and errors. Full FN/SPD/
order-impact/security reconciliation remains future work. Application tests,
SQL/migrations, configured deadlines, push/PR/merge/deployment remain unexecuted.
Sequential stop: present finalized version 2 for user review before DD-FN.

## Function-design review package — 2026-10-01

Explicit approval of finalized API version 2 authorizes revision 1 step 8 only.
Created 005_DD-FN version 1 and current EN/JA PDFs. No SPD/order-impact Markdown
written. Function design specifies eight methods, in-process EF ports, read
snapshots, ordered product/line/pair locks, aggregate persistence, order-service
integration, commit outcomes/cleanup and exact telemetry sites.

Actual verification:
- Repository docs-pdf.py rendered EN 10 pages / JA 9 pages; every page contains
  005_DD-FN version 1 (2026-10-01).
- EN/JA heading counts and REQ/FN/SCR/API-PL ID sets match; design SQL blocks
  match exactly, and all eight proposed method names appear in both sources.
- All LINE_* codes used by FN exist in approved API version 2. Local links and
  whitespace validated. BD/DB/main-DD/API source SHA-256 values match recorded
  approvals; exactly three DD Markdown documents exist, no subsequent design.
- Visually inspected EN lock SQL/persistence page 4, final page 10 and JA
  aggregate-update page 6 / commit-handling page 8. Text, code and table columns
  readable without clipping observed.
- Current ProductionOrderService/Repository and ProductMasterRepository inspected
  read-only. Existing order optimistic xmin is retained; new eligibility locks
  are acquired before the order UPDATE, not after an exclusive order row lock.
  Forced parent UPDATE and shared remaining post-commit deadline are specified;
  Draft clearing does not invent a requirement to select a replacement line.
- PostgreSQL 17 primary documentation checked for
  [row locks](https://www.postgresql.org/docs/17/explicit-locking.html),
  [isolation](https://www.postgresql.org/docs/17/transaction-iso.html),
  [timeouts](https://www.postgresql.org/docs/17/runtime-config-client.html) and
  [SQLSTATE](https://www.postgresql.org/docs/17/errcodes-appendix.html).
  SQLSTATE classification uses provider evidence, not localized message parsing.

Applicable FN-stage consistency passed: API field/error mapping and DB lock/
history/unit constraints match, atomic rollback and commit ambiguity are explicit,
security/telemetry obligations and later verification viewpoints are traceable.
No runtime SQL, concurrency, migration, instrumentation or application tests run.
Full SPD/order-impact and final design/security/delivery/release gates remain
pending at their later stages. Temporary Japanese translation and review images
remain outside project/evidence video output; no project dependency changes.
Sequential stop: submit FN and both PDFs for user review before DD-SPD. No
implementation, push/PR/merge or deployment approval inferred.

## 2026-10-01 — FN approval and SPD review package (plan revision 1 step 9)

The latest explicit user "approved" accepts FN version 1 and authorizes SPD.
Prepared only 005_DD-SPD version 1 with three numbered SVGs, two static HTML
editions and English/Japanese PDFs. The skill's unavailable native Artifact
service was replaced by the local HTML deliverable already stated in the plan.
Six component flow blocks map to approved API version 2 and FN version 1.

Actual verification:
- docs-pdf.py rendered EN 11 pages / JA 11 pages. Every page contains
  005_DD-SPD version 1 (2026-10-01).
- EN/JA sources have 13 headings each, matching REQ/FN/API-PL/SCR ID sets,
  consistent Markdown table column counts and existing relative link targets.
- SVG XML parsed; PC/SP region sets 1–8 plus confirmation region 9 exactly
  match the BD legend and SPD region table. Mobile list/edit are adjacent
  print artboards for readability. Approved main-DD state charts remain the
  single owner of state transitions; no competing chart introduced.
- Playwright drove headless local Chrome against both HTML editions at
  1280, 640, 390 and 320px widths (8 previews). All have 20 artboards and no
  document horizontal overflow. PC table/mobile card switching matches 640px.
  All three illustrated dialogs center within 0.008px in both axes and have
  at least 17px margins at mobile widths; these are drawing geometry checks,
  not native runtime dialog/keyboard/focus tests.
- Visually inspected mobile stale-unit form, desktop line dialog, detailed
  mobile SVG, EN PDF mobile page 3 and processing table page 7, JA processing
  page 7 and final page 11. No observed clipping in these samples.
- Five approved source SHA-256 values (BD/DB/main-DD/API/FN) match approvals.
  git diff --check passed; no 005_DD-ORD or subsequent design file authored.

Applicable SPD-stage design-consistency passed: fields/validation match API,
opaque versions and decimal strings retained, paged intents/errors remain keyed,
retirement/discard write boundaries and unknown-outcome verification specified,
Admin/Operator access and WCAG requirements documented. REQ-068 detailed order
behavior, full-family/security reconciliation and implementation-ready gate remain
later steps. No application code, API, SQL, migration, runtime accessibility,
transaction or E2E test executed. No push/PR/merge/deploy. Temporary translation,
scripts and review images stay outside the repository and four-video output.
Submit SPD and companions for review; wait before the order-impact DD.

Review-package delivery checklist: approved revision 1 / step 9 identified;
current source and both PDFs present; actual checks and unrun runtime checks
recorded; remaining order/security/implementation gates explicit; no external
operation, new dependency, credential or secret introduced; continuation state
points to SPD review. This satisfies this design package's delivery boundary,
not completion of WI-009 or implementation/release readiness.

## 2026-10-01 — SPD approval and order-impact package (revision 1 step 10)

The user's explicit "approved" accepts SPD version 1 and authorizes the new
005_DD-ORD addendum. Created only this design Markdown plus four SVGs,
English/Japanese static HTML and required PDFs. Existing order designs and
all approved WI-009 sources were read only. No independent DD family created;
contracts remain API-owned and server transactions remain FN-owned.

Actual checks:
- Read current ProductionOrderPage/Form/Table/api/types and catalog, plus
  existing order designs and WI-006 addendum. Existing create→edit/update-in-place
  success and exact quantity number-token transport retained; Japanese heading,
  MSG-E009 and formatted MSG-I002 text aligned with current catalog.
- docs-pdf.py rendered EN 12 pages / JA 11 pages; every page identifies
  005_DD-ORD version 1 (2026-10-01). Mermaid diagram visibly rendered in both.
- EN/JA have 10 headings, matching REQ/FN/API-PL/SCR/error-code sets, consistent
  table column counts and existing relative links. Six distinct-state diagram
  edges match the first six transition rows; same/any-state cases table-only.
- Four SVGs parse as XML: SCR-001 regions 1–5, SCR-002 regions 1/6/7, all mapped
  in the region table. Chrome getBBox checks found no text beyond any canvas.
- Headless Chrome/Playwright checked both 15-state HTML editions at 1280, 640,
  390 and 320px (8 previews): no document horizontal overflow; table/cards switch
  at 640px. Mobile Save/Cancel adjusted to existing full-width stacked behavior.
  These are static visual checks, not application E2E/accessibility tests.
- Visually inspected mobile selected-line, legacy null and list-card states,
  EN form wireframe page 3, both diagrams/tables page 8, EN final page 12 and
  JA final page 11; sampled content readable without clipping observed.
- Captured SHA-256 values for 143 existing approved source/PDF/visual artifacts
  all unchanged. SPD approval hash recorded in decisions. git diff --check passed.

ORD-level design-consistency passed for API lineId missing/null semantics,
Draft/start/non-Draft history rules, current-master/null projection, no invented
pair-eligibility metadata, product-change clearing and late-response guards,
existing order xmin/quantity/status preservation, Admin/Operator authorization,
new field errors, safe dirty/unknown handling, accessibility and telemetry
obligations. Full-family/security reconciliation and test-plan step 11 await ORD
review. No runtime application, API, SQL, migration, concurrency, accessibility
or E2E checks executed; no code/dependency/CI/push/PR/merge/deploy changes.
Temporary translations/scripts/images remain outside the repository and video
output. Review-package delivery checks passed: approved scope identifiable,
companion artifacts current, actual/unrun checks explicit, continuation records
updated and no new secret/credential/external action. WI-009 is not complete.
Submit ORD package and wait for review before steps 11–12.

## 2026-10-01 — ORD approval, design reconciliation and revision 2 proposal

The explicit user "approved" accepts ORD version 1. Recorded its SHA-256 in
decisions.md. Executed approved revision 1 step 11 document review/test planning
and prepared step 12 implementation revision 2 for presentation in this turn.
No revision 2 approval inferred; every proposed implementation step not started.

Actual verification:
- Rechecked all eight 005 source documents and current EN/JA PDF pairs; every
  page uses API version 2 or other document version 1 as appropriate.
- All local document/image/mockup references exist; main-DD distinct-state
  edges total 17 (8 list + 9 form), ORD edges total 6; all SVG XML parses and
  the four required DD family members exist.
- Protected 143 earlier source/PDF/visual SHA-256 values still exact. New ORD
  approval hash 0b06cb382639... recorded in full in decisions/review records.
- Reviewed source/domain/schema/API/client boundary rules for identity/decimals,
  parent xmin and product-unit ABA, nullable/history/start semantics, locks,
  snapshots, paging/intent mapping, auth/input/grants, telemetry/deadlines and
  unknown write outcomes. Final design checklist and nine security checklist
  dispositions recorded in review.md; no blocking design finding identified.
- Read existing API auth/OTel configuration, cookie posture, integration fixture,
  frontend scripts and existing TC registrations read only. Highest existing
  registered TC was 325; reserved 326–365 for forty new scenario groups.
- test-plan.md uses existing frameworks and isolated resettable data; 20 unit,
  14 integration, 5 E2E and 1 system groups. Every runtime case not run because
  revision 1 excludes implementation/runtime checks. No fabricated results.
- git diff --name-only -- src tests deploy .github returned no changed paths;
  git diff --check passed. No application/test/SQL/migration/dependency execution.

Revision 1 design-consistency and design security assessment passed for proposal
readiness. Runtime security/delivery gates must be repeated on the actual code
and results; release-readiness not applicable without deployment authorization.
Routine review/test-plan records create no sequential design-document review
stop. Prior approved designs and revision 1 scope remain preserved; outcome/
closure and revision index updated, revision 2 appended chronologically.
Revision 2 proposes eight local implementation/verification steps, isolated
owner migration/recovery rehearsals only, lockfile restores if necessary,
actual evidence and implementation security review. Git commit/push/PR/merge/
publish/deploy are excluded. The user must approve this revision after it is
shown before any step executes. Current state: awaiting-plan-revision-2-review.
Design-phase delivery checks: reviewed scope identifiable; sources/companion
PDFs current; real versus planned checks explicit; continuation and limitations
recorded; no new secret/dependency/external action; feature not complete.

## 2026-10-01 — Revision 2 schema implementation and regression

Explicit user approval of presented revision 2 authorizes local implementation.
Implemented line/pair entities and mappings, nullable order line, generated
product unit revision, two staged migrations and restricted column grants.
All migration execution used disposable PostgreSQL 17 Testcontainers; no
live/demo service, data or volume modified.

Actual results:
- Backend build: passed, zero warnings/errors. Existing lockfile restores only.
- Migration tests: first 8 passed/1 failed; failed-index rehearsal exposed an
  unintended non-concurrent first-stage order index. Removed that generated
  operation, leaving concurrent creation exclusively in stage two. Rerun 9/9
  passed (21 seconds). Fresh/upgrade, ABA trigger, runtime grants, strict numeric
  checks, invalid-index repair guard and rejected rollback exercised.
- Unit tests: initial focused 22 passed/2 failed because assertions compared
  DomainRuleViolation.Message instead of its existing Code. Corrected assertions
  and added Completed coverage; full unit regression 173/173 passed, no skips.
- Full integration regression: 120/120 passed, no skips, 41 seconds.
- npm ci frontend and E2E: succeeded, zero reported vulnerabilities; no package
  upgrades or new dependencies. git diff --check passed before the next slice.

TC-328/357–359/365 have partial supporting results only; complete scenario-group
acceptance remains pending further implementation and verification. Service/API,
order eligibility, UI, race/telemetry/accessibility/E2E checks are not yet complete.
No commit/push/PR/merge/deployment or video changes performed.

## 2026-10-01 — Feature API, order integration and frontend implementation

Implemented seven feature operations with strict JSON/query/media/size boundaries,
string decimals/tokens, no-store, snapshot reads, product/parent/pair lock ordering,
atomic validation, forced parent audit update and bounded use-case/DB/cleanup waits.
Registered feature OTel source/meter and bounded counter/duration classification.
Added order nullable line projection, presence-aware edit, origin-Draft locks,
current eligibility/start checks and retained historical/null exceptions.
Existing Domain guards now expose a non-mutating validation method before new
line checks; quantity serialization and numeric order/product versions preserved.

Actual verification:
- New restricted-runtime endpoint suite: 10/10 passed, no skips (3 seconds):
  atomic rollback/omission, retirement/re-add, code/version, ABA/reconfirmation,
  strict bodies/query, media/size and auth.
- Added validation and commit ambiguity unit cases: 188/188 full unit passed.
- Order extension initially caused three old unit start fixtures to fail because
  they lacked the newly mandatory eligible assignment. Updated successful-start
  fixtures and added missing-line rejection/non-mutation case: 189/189 passed.
- Initial order integration regression: 126 passed/4 failed. Three successful
  start fixtures omitted the line; one exact field-catalog assertion omitted new
  nullable line. Updated approved-contract fixtures (not production rules).
  Intermediate 132 passed/1 failed while field assertion still old. After its
  correction, full integration 133/133 passed, zero skips, 32 seconds.
- New frontend list/editor/picker, Factory nav, Japanese catalog, string adapter
  and exact quantity payload additions implemented. Frontend build passed.
  Initial lint succeeded with two Fast Refresh warnings; moved hooks/URL rules
  into a separate module. Subsequent lint verification pending.
- Initial frontend regression: 135 passed/9 failed (144 total). Four nav fixture
  expectations and one body expectation omitted approved additions; two lexical
  source scans matched JSX-adjacent return syntax, fixed source parentheses;
  stale-reload test needs explicit discard review and order mocks need eligible
  response rather than fabricated 500. Corrections in progress; no frontend
  suite pass claimed yet.

Disposable Compose project pmai-wi009-isolated uses separate database pmai_wi009_test
and ports 5499/8099/3099; random temporary credentials outside repo. DB/backend
image build started, application not yet running. No live/demo modification,
external Git operation, video output or approved design edit. Final synchronized
race, snapshot/telemetry, accessibility/E2E and review gates remain pending.

## 2026-10-01 — Resumed runtime verification checkpoint

Resumed the approved revision 2 after the automatic reviewer usage-limit failure.
No rejected command was treated as executed. Restarted Docker Desktop hidden;
all application builds/rehearsals continue against pmai-wi009-isolated only.

Actual checks and corrections:
- Frontend full regression passed 177/177, then 180/180 after three added editor
  cases. Latest completed lint/build passed; no new dependencies.
- Real synchronized race suite initially passed 3/4. PostgreSQL's lock timeout
  was wrapped by EF and returned 500. Provider classification now walks only
  actual inner PostgreSQL failures; rerun passed 4/4. Expanded unit/pair races
  plus direct timing numeric constraints subsequently passed 7/7.
- New snapshot/telemetry fixture first passed 3/3: a concurrent insert between
  count/rows is invisible until the next snapshot; a concurrent unit change does
  not mix the product observation with eligible rows; bounded completion metrics
  emit once and search values are absent from captured tags.
- Broader integration run exposed process-wide meter contamination from unrelated
  parallel fixtures (expected 3 samples, observed 4). Quarantined that listener
  test by an explicitly nonparallel xUnit collection; no assertion loosened.
  Full backend then passed 189 unit / 148 integration, no skips.
- Full Playwright run: 38 passed / 1 failed (39 total, 54.8 seconds). Lifecycle
  case exposed keyboard Tab escaping the native modal to browser chrome. Added
  explicit forward/reverse Tab containment; verification rerun pending. Prior
  duplicate hidden save notice and browser Back interception defects also fixed.
- New editor tests passed 9/9 after correcting two fixture pagination labels and
  a detached old-page element reference. They observe cross-page draft retention,
  indexed server error focus and unit/coefficient changes clearing confirmation.
- New picker tests passed 3/3: historical selection independent of filtered rows,
  obsolete product reads ignored, pending/forbidden controls suppressed.
- Review corrections: confirmed rollback discards its failed context; bounded
  lock/validation span events added; unknown retirement retains Cancel so read
  verification is reachable; 503 retirement respects one-second retry cooldown;
  order browser exits warn on dirty/pending work; picker filter Clear added.
- Added direct statement-timeout rehearsal and fresh cancelled-request cleanup
  test; full backend verification in progress, 190/190 units already passed.

Application delivery/security gates remain pending the final current-image E2E,
remaining case dispositions and review. CSS 200% zoom is automated evidence, not
an assertion that manual native browser zoom was completed. No commit, push,
PR, merge, deployment, approved-design edit or video change performed.


## 2026-10-01 — Final local revision 2 delivery

Frozen-source verification completed after the recorded fixes. No failing case
was silently skipped or retried. The process-wide telemetry test collection
remains explicitly isolated; its original contamination failure is preserved above.

| Command / check | Actual final outcome |
| --- | --- |
| dotnet build src/backend/ProductionManagementAI.slnx --no-restore | Pass, zero warnings/errors |
| dotnet test src/backend/ProductionManagementAI.slnx --no-restore | Pass: 192 unit (15s), 157 integration (63s), zero skips |
| npm run lint / npm run build in src/frontend | Pass, clean lint and strict TypeScript/Vite build |
| npm test in src/frontend | Pass: 196 tests, 16 files, zero skips |
| npx playwright test --trace off against localhost:3099 | Pass: 42 tests, approximately 1 minute, retries 0 |
| Final focused timeout/cancellation/race check after cleanup review | Pass 9/9 before final full backend rerun |
| dotnet ef migrations has-pending-model-changes --no-build | No model changes since the last migration |
| Protected artifact SHA-256 comparison | 143 prior artifacts + 8 approved current sources unchanged; zero inaccessible files/mismatches |
| git diff --check | Pass before final record updates; recheck at cleanup |

Full E2E includes original Product master, WI-008 dialogs/icons, orders/list,
dashboard and mobile regression. Added eight feature journeys include applied
filter history and the 320px timing editor; all ran against final isolated images.
Frontend initially rejected 15 old success/source-scan assertions after stricter
line response validation: old fixtures omitted the newly required nullable field,
and an internal diagnostic sentence matched the UI source scan. Corrected fixture
responses to line:null and used an internal uppercase error code. Final full
frontend pass is 196/196; exact quantity number-token behavior remains verified.

Native browser visual check: a newly launched Chromium review window used OS
keyboard zoom increments; the browser UI visibly reported 200%. Actual layout
viewport 664x432 CSS pixels, devicePixelRatio 2, dialog x=92.25/y=129.25,
width=480/height=174; no horizontal page overflow. Cancel focused; Tab wraps
inside the dialog. Inspected a capture of only that review window: title/text,
actions and focus ring readable, dialog centered in the content viewport.
An initial measurement included the native scrollbar (innerWidth=672), causing
an incorrect centering failure. Corrected measurement to the layout viewport,
then inspected the OS-window capture because native-zoom CDP screenshots clip
incorrectly. A prior temporary script waited on the login heading rather than
completed sign-in; corrected it before the successful check. These were review
script failures, not hidden passing application results.

Final implementation review also found a second explicit rollback attempt could
restart a cleanup budget after a failed attempt. WriteSession now remembers that
rollback was attempted; disposal does not start another explicit attempt, and
failed contexts are discarded. Actual timeout/cancellation tests passed 9/9,
then the complete final backend suite passed 192/157. No automatic write replay.

Updated deploy/README.md with owner-only expand/index order, verified backup and
write-pause prerequisites, exact catalog checks, invalid-index owner recovery,
restricted grants, forward-fix limits and unknown-outcome handling. Forty case
results are mapped in test-plan.md. Agent design/security/delivery review is in
review.md. This is local delivery only: no commit, push, PR, remote CI, merge,
publication, live/demo cutover, external OTLP export or video change.

Isolated resource cleanup is the final housekeeping step and will be recorded
below only after completion. Temporary credentials are outside git and are never
included in evidence. Existing four final video outputs remain outside this scope.

### Isolated housekeeping completion — 2026-10-01

Inspected exact Compose labels before cleanup: three pmai-wi009-isolated
containers and only its db-data/dp-keys volumes. Authorized project-specific
`down --volumes` completed; follow-up container/volume label queries returned
no resources. Temporary isolated credential file and native-review script/images
removed by checked literal paths under TEMP. Main/demo Compose resources and
video outputs were not targeted. Final git diff --check passed. Local delivery
gate is complete; source/branch/worktree retained uncommitted for review.


## Revision 3 pre-publication audit — 2026-10-01

Explicit approval recorded. Remote master is 20c8d61; no existing feature PR.
Audited 123 staged files, including 16 approved EN/JA PDF companions and the
revision 1 WI-008 record reconciliation. No temporary test outputs or videos
included. Protected hashes verified again: 143 previous artifacts plus eight
current approved design sources unchanged. Credential-signature scan found no
private keys, GitHub tokens or AWS access keys; prior full security review remains
applicable. Staged diff --check passes. No application source changed during this
delivery stage; final local 192/157/196/42 verification remains applicable.
Initial hash audit script failed on Windows default text encoding; corrected to
explicit UTF-8 and reran successfully before committing. Remote CI not yet run.
