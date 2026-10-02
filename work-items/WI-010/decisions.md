# WI-010 — Decisions and open questions

## Confirmed instructions

- User selected Plant calendar (稼働カレンダー) on 2026-10-01.
- Revision 1 requirements/design phase explicitly approved, 2026-10-01.
- English artifacts, Japanese UI/PDFs, Vietnamese conversation.
- Review each design Markdown with companions before the next file.
- Completed approved designs remain immutable; add new impact documents.
- Evidence videos are separate artifacts; no additional video work in this feature.

| ID | Question | Proposal | State |
| --- | --- | --- | --- |
| DEC-001 | Next feature | Plant calendar | Decided: explicit user request |
| DEC-002 | Read/write roles | Admin and Operator; do not inherit permission without confirmation | Decided: explicit bundled user agreement, 2026-10-01 |
| DEC-003 | Calendar scope | Shared plant calendar plus line-specific exceptions | Decided: explicit bundled user agreement, 2026-10-01 |
| DEC-004 | Order/date-rule impact | Lookup and dated capacity only; no order-start block or automatic scheduling in initial phase | Decided: explicit bundled user agreement, 2026-10-01 |
| DEC-005 | Weekly/default hours and exception precedence | Mon–Fri working; weekend closed; default working hours from WI-009; explicit working exception hours >0 and <=24, scale <=3; line-date > plant-date > weekly, including reopening; removal restores fallback | Decided: user "approved" after the proposed-rule confirmation request, 2026-10-01 |
| DEC-006 | Past editing, effective dates and history | Past dates read only; weekly changes effective today/future with prior versions retained; dated capacity only today/future from current configuration | Decided: user "approved" after the proposed-rule confirmation request, 2026-10-01 |
| DEC-007 | Holiday input and timezone | Manual exceptions; configured plant timezone; no external holiday feed | Decided with DEC-005: explicit user approval, 2026-10-01 |

The source PDF proposes working-day due-date and dashboard windows. DEC-004 must
explicitly settle this suggestion; selecting the feature does not approve a
change to existing order or dashboard semantics. User business answers do not
approve plan revision 1. No answer or approval inferred from silence.


## Revision 1 approval — 2026-10-01

Explicit user "approved" received after plan presentation. This authorizes
revision 1 only. DEC-002–004 answers remain pending; a bundled confirmation was
sent to distinguish business answers from plan approval. No permissions, calendar
precedence or date-rule changes inferred. Implementation remains excluded.


## DEC-002–004 confirmed — 2026-10-01

User explicitly agreed to all three bundled proposals: Admin/Operator read/write;
shared plant calendar plus line exceptions; lookup/dated capacity only in this
phase. Existing due-date validation and dashboard windows remain unchanged;
calendar does not gate order starts. No automatic scheduling. Original PDF date
arithmetic suggestion is deferred by this explicit decision, not silently omitted.

Sent dependent questions for DEC-005–007: default weekdays/hours/manual holidays,
exception precedence/reopening/removal, and past/effective-date/capacity history.
No dependent answer inferred yet. No additional plan revision required: confirmed
scope fits the explicitly proposed revision 1 boundary.


## DEC-005–007 approval — 2026-10-01

After the three proposed-rule questions and a handoff stating that those three
confirmations were the remaining dependency, the user replied "approved". No
new plan approval was pending. Recorded this as approval of all three presented
rule proposals and stated that interpretation before authoring 006_REQ:
Mon–Fri working/weekend closed; WI-009 fallback hours, explicit positive <=24
hours scale <=3, closed zero; manual holidays/configured timezone; line-date >
plant-date > weekly including reopening and removal fallback; protected past
dates, effective weekly versions today/future, capacity only today/future from
current configuration. DEC-001–007 are resolved.

006_REQ version 1 explicitly separates additional review proposals RP-01–05
(initial activation/unavailable earlier dates, retained revisions/future correction,
plant-view hours, conservative capacity precision and retired-line handling).
These await document review, not an invented user answer. No implementation
or next-design approval inferred from the business-rule response.


## Sequential requirements approval — 006_REQ version 1

User explicitly replied "approved" after receiving 006_REQ version 1, both PDFs
and the handoff specifically identifying RP-01–05 for approval, 2026-10-01.
All five review proposals are now accepted, including activation/unavailable past,
retained revisions/future correction, line-dependent plant-view hours, conservative
capacity precision and retired-line handling. Approved source SHA-256: 5df4cd78086586ccb445c2a51e0a601b8547f628bc332613840d1dfbe3b730f9.
This authorizes revision 1 steps 3–4: architecture assessment and BD only. DB,
DD or implementation authoring is not authorized by this sequential reply.


## Sequential basic-design approval — 006_BD version 1

User replied "approved" after the 006_BD source and both PDFs were presented.
Approved source SHA-256: e37d1b8d9a6c621277dd4a34461567295a5b3ad2e09b4201c220ca7bf61feac3.
This authorizes revision 1 step 5, 006_DB and companion PDFs. Main DD must wait
for DB review. No implementation or new plan revision approval inferred.


## Sequential database-design approval — 006_DB version 1

User explicitly replied "approved" after the DB source and EN/JA PDFs were
presented, 2026-10-02. Approved source SHA-256:
465e06e53985fcddd485f039fbb594c4dc5653edba2492e77f25cc9d54a43793.
Accepted DB technical choices: three additive tables, retained markers, global
bigint revision, weekday mask and reason bound 500 code points. This authorizes
revision 1 step 6 main DD and companions required for that review package.
DD-API/FN/SPD Markdown remain sequential later steps. Implementation excluded.


## Sequential main-DD approval — 006_DD version 1

User explicitly replied "approved" after main DD, SVG/mockups and EN/JA PDFs
were presented, 2026-10-02. Approved source SHA-256:
c1869f77f5b960c8effa2fc331f25500032bd9053317d07e787530cf69aa556d.
Authorizes revision 1 step 7 DD-API and both PDFs; DD-FN waits for API review.
No implementation or simultaneous later design authoring approval inferred.


## Sequential API approval — 006_DD-API version 1

User explicitly replied "approved" after the API source and EN/JA PDFs were
presented, 2026-10-02. Approved API SHA-256:
a810385a00fe1813b8767fd73d0028c8d80e699901cf48c1cb62709c1ab852f1.
Accepts its eleven contracts, exact schemas/bounds, budgets, outcome certainty
and telemetry. Authorizes revision 1 step 8 DD-FN and both PDFs; DD-SPD waits
for FN review. No implementation or later design approval inferred.


## Sequential FN approval — 006_DD-FN version 1

User explicitly replied "approved" after the FN source and EN/JA PDFs were
presented, 2026-10-02. Approved FN SHA-256:
c212eba6b9a8d45e4879bba0144d1ad86b17a8be3cdf31d9d08f0a4596611610.
Accepts fifteen methods, exact capacity flooring, read/write algorithms and
outcome certainty. Authorizes revision 1 step 9 DD-SPD and required companions.
Later design review and implementation-plan approval remain separate stops.


## Sequential SPD approval — 006_DD-SPD version 1

User explicitly replied "approved" after the SPD source, EN/JA PDFs and bilingual
mockups were presented, 2026-10-02. Approved SPD SHA-256:
462337bd6a3c9e0de774ec37ed04b6b0f44030ff214b7ebc36487e5444093a78.
Accepts seven client flows, exact Japanese catalog/focus and reconciliation behavior,
with static visual companions. Authorizes revision 1 steps 10–12: impact assessment,
full design/security reconciliation, test planning and presenting revision 2.
It does not approve execution of the not-yet-presented implementation revision.


## Design-phase closure and proposed revision 2 — 2026-10-02

Conditional impact DD not applicable: confirmed scope changes no existing
order-start/due-date/dashboard-day semantics; shared navigation and coherent
master reads remain within the approved SCR-006 design. No prior design edits.
Full design-consistency and design-stage security review passed, with runtime
verification explicitly deferred. TP-WI-010 revision 1 reserves TC-366–407; all
application results Not run. Revision1 complete; revision 2 appended in plan.md
for presentation, local implementation/testing only and not approved yet.
No continuation before its presentation counts as revision 2 approval. Await the
user's explicit reply after reviewing the concrete revision; no external delivery
or live migration/deployment permissions inferred.


## Revision 2 approval — 2026-10-02

Explicit user "approved" received after the concrete revision 2 presentation.
Authorizes its ten local implementation/test milestones and isolated fixtures.
Commit/push/PR/merge/live migration/deployment and video output remain excluded.

## Local implementation handoff — 2026-10-02

Revision2 implementation and automated verification executed within approved scope.
Final review corrections preserve approved designs: strict response identity checks,
independent weekly list refresh and bounded unactivated Down. No new business rule,
package or architecture decision introduced. Evidence records real regression failures
and their fixes. Native200% checked; screen-reader and physical mobile keyboard/IME
not run without configured tools/device. No user waiver or final acceptance inferred.
Local review can proceed; manual acceptance and external delivery remain separate.

## Local manual-check disposition — 2026-10-02

The user explicitly selected: retain the Not run record and permit local handoff
with the screen-reader speech and physical mobile keyboard/IME limitations.
This authorizes local delivery sign-off with those limitations; it does not convert
unrun checks to passes or authorize commit/push/PR/merge/deployment/videos.
No change to approved requirements/designs. Revision 2 remains the approved scope.

## Local delivery approval and next revision proposal — 2026-10-02

User explicitly replied "approved" after completed local handoff. Revision2 local
delivery accepted with the previously stated manual limits; no external operation
inferred. Existing CI review found missing owner calendar activation for the fresh
E2E fixture. Prepare revision3 covering this bounded setup plus commit/push/PR and
actual CI verification. Revision3 not approved or executed yet. No design changes.

## Revision3 approval — 2026-10-02

Explicit user "approved" after revision3 presentation authorizes the existing CI
fixture activation step, isolated rehearsal, local commit/rebase, branch push/PR
and actual CI verification/fixes within scope. Merge/deploy/videos remain excluded.

## Revision 3 PR publication result — 2026-10-02

PR #36 opened under revision 3 authorization. Implementation head f2a1d82 passed
GitHub run 36975008859 (Backend/Frontend/E2E). Final routine publication records
will receive their own head verification in PR #36 before handoff. This adds no
business/design decision and does not authorize merge, deployment or videos.

## Separate merge/cleanup authorization — 2026-10-02

User approved the completed PR handoff, then replied "ok" to the specific request
to merge PR #36 and clean its branch/worktree. This authorizes that merge/cleanup;
no deployment/live activation/video or next feature is inferred. PR #36 squash
merged as d4dd976 after all final-head CI checks passed. Workflow step 8 routine
closure is recorded through a separate documentation PR, preserving approved designs.
