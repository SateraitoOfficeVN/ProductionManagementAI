# WI-009 — Status

As of 2026-10-01. State: PR-open; revision 3 delivery authorized.

## Current delivery

Local revision 2 implementation and verification are complete: 192 unit, 157
integration, 196 frontend and 42 Playwright pass; frontend lint/build and backend
build clean. Approved designs preserved; isolated resources cleaned up.

Implementation commit: 810ea066763b8855cfbb8c289d9873813c5c5902, published on
feature/WI-009-production-lines. [PR #34](https://github.com/SateraitoOfficeVN/ProductionManagementAI/pull/34) targets master at 20c8d61.
[Initial CI](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/36832509363) has passed Backend and Frontend; E2E is in progress at this
record update. This record-only follow-up will trigger checks again. Final
exact-head results are recorded in the PR description/checks after verification;
no final CI success is claimed by this earlier snapshot.

Security/design/delivery assessment: [review](review.md), [evidence](evidence.md),
[test dispositions](test-plan.md). Checkout retained at
C:/Data/project/ProductionManagementAI-WI009. Main/demo resources and the four
final videos preserved. No merge, deployment or live migration performed.
Next: complete CI review and present PR; merge requires separate authorization.
No new feature or design revision approved.

## Execution history

The following entries preserve each milestone's state when it was recorded;
current delivery and later evidence supersede their then-pending statements.

[Plan revision 1](plan.md) explicitly approved by the user's "approved" reply
on 2026-10-01 after its presentation. Requirements/design only; application
implementation requires separately approved revision 2.

Checkout: `C:/Data/project/ProductionManagementAI-WI009`.
Branch: `feature/WI-009-production-lines`; baseline `20c8d61`.
Main checkout's original planning drafts remain preserved.

Steps 1–3 complete: isolated baseline/current-state reconciliation, confirmed
business rules and requirements with EN/JA PDFs, architecture assessment without
new ADR. DEC-001–DEC-009 answered; REQ-064–REQ-069 and UC-017/018 traceable.

Step 4 package — explicitly approved on 2026-10-01:
- [005_BD version 1](../../docs/en/010_basic-design/005/005_BD_生産ライン・工程.md).
- [English PDF](../../docs/en/pdf/010_basic-design/005/005_BD_生産ライン・工程.pdf).
- [Japanese PDF](../../docs/ja/pdf/010_basic-design/005/005_BD_生産ライン・工程.pdf).
- Numbered PC/SP SVGs linked in the BD; both rendered and inspected in Chrome.

Both BD PDFs have 10 pages; both requirements PDFs have 3 pages. Every page's
revision footer, translated headings/IDs, relative links and numbered legends
validated. Diagrams and sampled PDF pages visually inspected. Requirements/BD
level design-consistency checks passed; full DB/DD/API reconciliation awaits
later documents. No old approved design changed; no application test, migration,
code, push, PR, merge or deployment performed for WI-009.

005_BD version 1 approved by explicit user reply on 2026-10-01.
Step 5 review package:
- [005_DB version 1](../../docs/en/database/005/005_DB_生産ライン・工程.md).
- [English PDF](../../docs/en/pdf/database/005/005_DB_生産ライン・工程.pdf), 10 pages.
- [Japanese PDF](../../docs/ja/pdf/database/005/005_DB_生産ライン・工程.pdf), 9 pages.

ER diagrams rendered and visually inspected in both PDFs; sampled final pages
readable. All revision footers, translation heading/ID parity and local links
validated. DB-level consistency with approved BD and confirmed requirements
checked; API/DD reconciliation and runtime verification remain future work.
The approved BD source hash is unchanged. No SQL or application test executed.

005_DB explicitly approved on 2026-10-01; its recorded source hash remains unchanged.

Step 6 review package:
- [Main 005_DD version 1](../../docs/en/020_detailed-design/005/005_DD_生産ライン・工程.md).
- [English PDF](../../docs/en/pdf/020_detailed-design/005/005_DD_生産ライン・工程.pdf), 15 pages.
- [Japanese PDF](../../docs/ja/pdf/020_detailed-design/005/005_DD_生産ライン・工程.pdf), 14 pages.

Main DD defines screen-owned modules, fields, validation, permissions, state
transitions and persistence boundaries. Two state diagrams render in EN/JA;
8 list and 9 form transitions match their tables. Current revision footers,
translated headings/IDs and local links validated. State pages and final pages
visually inspected. Approved BD regions 1–9 reused unchanged; detailed SVG/HTML
mockups remain the approved step 9 SPD package. Main-DD stage checks passed;
full DD/API/FN/SPD consistency and implementation-ready gate remain incomplete.
No code, SQL, migration or runtime application test performed.

Main DD explicitly approved on 2026-10-01.

Step 7 finalized review package:
- [005_DD-API version 2](../../docs/en/020_detailed-design/005/005_DD-API_生産ライン・工程.md).
- [English PDF](../../docs/en/pdf/020_detailed-design/005/005_DD-API_生産ライン・工程.pdf), 10 pages.
- [Japanese PDF](../../docs/ja/pdf/020_detailed-design/005/005_DD-API_生産ライン・工程.pdf), 9 pages.

Seven feature endpoints, exact decimal strings, version/unit guards, atomic
pair actions, legacy order additions, error mappings and telemetry specified.
Technical bounds and contracts are submitted for API review. DEC-010 now
confirms Admin/Operator read access via the existing policy. Heading/
ID parity, links, all PDF revision footers and approved source hashes checked.
EN error page 8; JA update/error pages 5/7 visually inspected. No app/SQL/test run.

API-stage field/schema/error checks passed after DEC-010 resolution. Every PDF
page identifies version 2; EN/JA first pages visually inspected. BD/DB/main-DD
hashes remain unchanged. Full family/security reconciliation awaits FN/SPD.

API version 2 explicitly approved on 2026-10-01.

Step 8 review package:
- [005_DD-FN version 1](../../docs/en/020_detailed-design/005/005_DD-FN_生産ライン・工程.md).
- [English PDF](../../docs/en/pdf/020_detailed-design/005/005_DD-FN_生産ライン・工程.pdf), 10 pages.
- [Japanese PDF](../../docs/ja/pdf/020_detailed-design/005/005_DD-FN_生産ライン・工程.pdf), 9 pages.

Eight method entries cover snapshot reads, atomic master writes and order-line
validation in the existing order transaction. Product/line/pair lock order,
forced parent xmin advancement, observed-unit validation, Draft/history cases,
commit ambiguity and bounded cleanup/instrumentation are specified. EN/JA
headings/IDs/SQL and API error codes match. Every PDF page has the current
footer; lock/update/commit and final pages visually inspected. All four approved
source hashes unchanged. FN-stage consistency checks passed; full SPD/order/
security and implementation-ready reconciliation remains future work.
No application code/test, SQL or migration execution performed.

FN version 1 explicitly approved on 2026-10-01.

Step 9 review package:
- [005_DD-SPD version 1](../../docs/en/020_detailed-design/005/005_DD-SPD_生産ライン・工程.md).
- [English PDF](../../docs/en/pdf/020_detailed-design/005/005_DD-SPD_生産ライン・工程.pdf), 11 pages.
- [Japanese PDF](../../docs/ja/pdf/020_detailed-design/005/005_DD-SPD_生産ライン・工程.pdf), 11 pages.
- Three detailed SVGs and English/Japanese 20-state static HTML galleries, linked in SPD.

Six processing blocks specify URL reads, form saves, cross-page timing intents,
centered confirmations, exact validation and auth/error/unknown/dirty handling.
Both galleries checked in Chrome at 320/390/640/1280px with no page overflow;
illustrated dialogs centered with >=16px margins. Native modal behavior and
application accessibility remain unrun runtime checks. Headings/IDs, links,
tables, numbered regions and every PDF footer checked; representative mobile,
flow-table, dialog and final pages visually inspected. All five approved source
hashes unchanged. SPD-stage design-consistency checks passed; full-family/
security/order-impact reconciliation remains pending. No app/code/SQL/test run.

SPD version 1 explicitly approved on 2026-10-01.

Step 10 review package:
- [005_DD-ORD version 1](../../docs/en/020_detailed-design/005/005_DD-ORD_production-line-assignment.md).
- [English PDF](../../docs/en/pdf/020_detailed-design/005/005_DD-ORD_production-line-assignment.pdf), 12 pages.
- [Japanese PDF](../../docs/ja/pdf/020_detailed-design/005/005_DD-ORD_production-line-assignment.pdf), 11 pages.
- Four numbered PC/SP SVGs and EN/JA 15-state static HTML galleries linked in ORD.

Order impact covers Draft optional selection, start-time eligibility, immutable
non-Draft assignment/legacy null, separate historical/current and eligible views,
product-change clearing, lineId presence semantics, order field errors, list
projection and concurrency/unknown outcome boundaries. All existing designs
preserved (143 captured source/PDF/visual hashes unchanged). EN/JA headings/IDs,
local links, tables, six diagram edges, seven wireframe regions and all PDF
footers validated. Eight responsive previews had no page horizontal overflow;
wireframe text fit all four canvases. Representative diagram/form/list/final
PDF pages and mobile states visually inspected. ORD-stage design consistency
and review-package delivery passed; no application/API/SQL/test run.

ORD version 1 explicitly approved on 2026-10-01.

Revision 1 steps 11–12 complete: final design-consistency and design security
assessment in [review.md](review.md); [test plan](test-plan.md) covers 40 scenario
groups TC-326–TC-365, every runtime case not run. Eight current EN/JA PDF pairs,
links, diagram edges, SVG XML and all 143 protected hashes rechecked. No open
blocking design finding; runtime implementation/security/verification remain
unproven. Revision 1 is closed as design-phase complete.

[Implementation plan revision 2](plan.md#revision-2--implement-and-verify-production-lines)
is submitted with eight steps, isolated migration/test permissions, risks and
external-operation boundaries. Approval source: none. All steps not started.
Next: user explicitly approves revision 2 before any application code/tests/
migrations. No push/PR/commit/merge/deploy authorized. Existing unrelated changes
and four final videos preserved. WI-009 is not complete.

Revision 2 explicitly approved by the next user "approved" reply, 2026-10-01.
Step 1 active: .NET 10.0.303, Node v24.18.0, Docker 29.8.1 available; feature
checkout retained. Default sandbox Docker access denied; bounded elevated
read succeeded. No running containers listed; no existing service modified.
Application implementation begins under the approved eight-step scope.

Revision 2 checkpoint: schema/domain slice implemented. Backend build passed
without warnings; 173 unit and 120 integration tests passed (includes nine new
migration cases). One migration index defect and two test assertions corrected
before successful reruns; details in evidence.md. Locked frontend/E2E restores
complete. Current step: Application contracts/service and feature API persistence.
Feature and runtime delivery/security gates remain incomplete.

Revision 2 next checkpoint: feature API and order extension implemented; full
backend verification passed 189 unit and 133 integration tests. Frontend list,
editor and order picker build passed; frontend regression corrections ongoing.
Isolated Compose image build in progress (project pmai-wi009-isolated, ports
5499/8099/3099), no existing/live resources targeted. Steps 5–7 active;
implementation security/design/delivery review remains pending.

Resumed verification checkpoint: full frontend 180/180 and latest completed
backend 189/148 passed; added picker 3/3 and expanded database races/numeric
7/7 passed. Current full backend rerun has 190/190 unit pass; integration pending.
Full E2E recorded 38/39 with a real modal Tab containment defect now corrected;
current-image rerun and final review still required. Snapshot/telemetry tests are
implemented; global meter listener isolated from unrelated parallel fixtures.
See evidence.md for failures, fixes and actual results. Revision 2 remains active;
no commit/push/PR/deployment authorization inferred.

Housekeeping complete: only pmai-wi009-isolated containers/network/two volumes
removed after label inspection; follow-up queries empty. Temporary generated
credentials/native-review files removed. Final whitespace check passed. Branch
and working tree retained for the separately authorized next action.


## Delivery plan revision 3 — awaiting review

User requested continuation after the completed local delivery. Prepared revision
3 for scoped staging/commit, feature-branch push, PR creation and actual CI review.
No execution or external authorization inferred before explicit approval of this
revision. Merge, worktree removal, deployment and video changes remain excluded.


## Revision 3 authorization — 2026-10-01

Explicit user approval received after plan presentation. Remote master remains
20c8d61; no existing feature PR found. Publishing is authorized; merge and
deployment remain excluded. Local verification results unchanged.
