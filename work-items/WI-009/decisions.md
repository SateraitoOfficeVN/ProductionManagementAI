# WI-009 — Decisions and open questions

## Confirmed instructions

- User requested the next feature; the prior direction is the Master data group.
- Plan approval must be explicit after presentation. No initial approval inferred.
- Review design Markdown files sequentially with their required companion artifacts.
- Preserve completed approved designs; new impact documents belong to WI-009.
- Artifacts English; UI and Japanese PDFs Japanese; conversation Vietnamese.

## Questions

| ID | Question | Proposal / options | State |
| --- | --- | --- | --- |
| DEC-001 | Which next function? | User selected Production lines | Decided — explicit user reply, 2026-09-30 |
| DEC-002 | Who writes line masters? | User selected Admin and Operator for line master writes | Decided — explicit user reply, 2026-09-30 |
| DEC-003 | How is capacity measured? | User selected working hours/day plus production time for each product | Decided — explicit user reply, 2026-09-30 |
| DEC-004 | Can line codes change and how are duplicates normalized? | User agreed: immutable code, trim both ends, case-insensitive uniqueness | Decided — explicit user reply, 2026-10-01 |
| DEC-005 | Is line assignment optional/mandatory; what about existing orders and statuses? | User chose optional in Draft, required on transition to InProgress; old InProgress/Completed orders retain their missing assignment | Decided — explicit user reply, 2026-10-01 |
| DEC-006 | How do retirement and association changes affect old orders? | User agreed: retire line/product associations, preserve existing orders and allow unchanged historical saves, block new selection; current timing edits do not recalculate history | Decided — explicit user reply, 2026-10-01 |
| DEC-007 | What capacities, precision and unit-change rules apply? | User agreed: 0 < hours/day <= 24, at most 3 fractional digits; minutes per one product unit, positive with at most 3 fractional digits, configured per line/product | Decided — explicit user reply, 2026-10-01 |

DEC-001–DEC-003 have explicit user answers. They do not approve plan revision 1.
DEC-004/DEC-005 are answered; DEC-001–DEC-008 are answered; DEC-009 is now explicitly answered; no implementation starts
until its own later plan revision is explicitly approved.

## Plan approval — 2026-10-01

The user explicitly replied "approved" after plan revision 1 was presented.
This authorizes its requirements/design phase only. It is separate from
DEC-001–DEC-003 business answers and from later implementation authorization.
Three further questions were sent for DEC-004–DEC-007; no answer is inferred.

## Retirement/history confirmation — 2026-10-01

The user agreed that retired lines remain on existing orders, may be retained
during an unchanged historical edit, and cannot be newly selected. This settles
line retirement in DEC-006, but supported-product association retirement/removal
still needs its separate answer. Questions also sent for line locking outside
Draft and historical line/product association behavior.

## DEC-008 — Order line locking

The user explicitly chose on 2026-10-01 to lock line assignment outside Draft.
Legacy orders with no line may retain that missing assignment when editing other
permitted fields; they are not forced to backfill a line. Draft may choose/change
a compatible active line under DEC-005. The supported-product association
retirement question remains unanswered.

## DEC-009 — Product-unit change before any order references the product

WI-006 allows product-unit changes before any order references the product.
A line/product time coefficient refers to a specific unit and cannot safely be
reinterpreted. Proposed: keep existing Product master permissions/rules; retain
the configuration unit and require explicit timing confirmation after a unit
change before new selection or starting production. User explicitly agreed on 2026-10-01: retain existing product-unit edit rights,
block new selection/starting production for the old-unit pair until timing is
explicitly confirmed for the new unit. No numeric conversion is automatic.

## Sequential design approval — 005_BD

The user explicitly replied "approved" on 2026-10-01 after receiving the
005_BD version 1 package with EN/JA PDFs. This permits plan revision 1 step 5,
005_DB only. It is not approval of DB, later DD files or implementation.
Approved BD source SHA-256: `9ff444e228293ae5e9a0fd5bc73204c8e9440c0639c54a18c04fe3ab5b4a4c28`.

## DB technical proposals — pending 005_DB review

005_DB proposes immutable composite line/product identity and a nullable
composite order reference, a parent xmin token for all pair edits, product-unit
revision with a database trigger to invalidate changed-back units, and exact
numeric scale checks without typmod rounding. The proposed minutes/unit ceiling
is 999999999.999 as a finite storage/API bound, not a throughput business limit.
Product-unit permissions and legacy-order exceptions remain as confirmed.
New FK/index/grant and lock-order details, staged concurrent index creation and
recovery limits are design proposals; no SQL has been executed. These choices
await DB review and later API/DD reconciliation, not a new implementation
approval. No subsequent design Markdown has been written.

## Sequential design approval — 005_DB

The user explicitly replied "approved" on 2026-10-01 after receiving
005_DB version 1 and its EN/JA PDFs. Its technical choices are accepted as
design inputs. This authorizes revision 1 step 6, main 005_DD only, not API/FN/SPD
authoring or implementation. Approved DB source SHA-256: `856e05827f2dae352a7403f65beecbb575aec86bb932f6006b7975c0b48b1a93`.

## Sequential design approval — main 005_DD

The user explicitly replied "approved" on 2026-10-01 after receiving main
005_DD version 1 and its EN/JA PDFs. This authorizes revision 1 step 7,
005_DD-API only. Approved main DD source SHA-256: `af4dd7ab27574068c40d8f300e6e0f6ed4a534eb5757f20c4881f2f79f639d4f`.
Implementation and later design authoring are not approved by this reply.

## DEC-010 — Production-line read authorization

API review must define who may read line list/detail and feature-owned choices.
The current Product master controller requires Admin/Operator for reads as well
as writes. Asked the user to choose Admin/Operator (proposed, consistent with
existing master policy) or all authenticated users. Reply pending; read-dependent
contract/policy finalization waits. Confirmed write roles remain Admin/Operator.

## DEC-010 confirmation — 2026-10-01

The user replied "approved" after the Admin/Operator read proposal and the
API draft handoff explicitly requesting read confirmation. This is treated as
acceptance of the proposed Admin/Operator read access, stated to the user in the
next commentary. It is not a recorded selection of the alternative all-user
option. All seven feature read/write operations use the existing
ProductionOrderEditor policy; existing order policy is unchanged.
API version 2 incorporates this confirmation and is submitted with regenerated
EN/JA PDFs before the next design step. No completed API approval or DD-FN
execution is inferred from approval of the unresolved version 1 draft.

## Sequential design approval — 005_DD-API version 2

The user explicitly replied "approved" on 2026-10-01 after receiving the
finalized API version 2 and its current EN/JA PDFs. This authorizes plan
revision 1 step 8, 005_DD-FN only. Approved API source SHA-256: `8b27005ed4f31500839512ce6c9c4b35dc69ac68bd14a877411d872ad0f51b30`.
No SPD/order-impact authoring or implementation authorization inferred.

## Sequential design approval — 005_DD-FN version 1

The user explicitly replied "approved" on 2026-10-01 after receiving FN version 1
and its EN/JA PDFs. This authorizes approved plan revision 1 step 9,
005_DD-SPD and its SVG/HTML/PDF companions only. Approved FN source SHA-256: `ab397310e9e19b067b69a45f4ce5f8952da9b3c91a1be2ab315e84a21c62e809`.
No order-impact authoring or implementation authorization inferred.

## Step 9 visual delivery boundary

The screen-design skill requests a native design/Artifact service, which is not
available in this session. The already approved plan specifies local English/
Japanese HTML mockups; those static galleries are linked from SPD. This is a
review artifact, not an application, interactive prototype or runtime evidence.
Business DEC-001–DEC-010 remain unchanged. No new scope or plan revision.

## Sequential design approval — 005_DD-SPD version 1

The user explicitly replied "approved" on 2026-10-01 after receiving the SPD
source, EN/JA HTML galleries and current PDFs. This authorizes approved revision 1
step 10, the new 005_DD-ORD order-impact addendum and its companion artifacts.
Approved SPD source SHA-256: `e5c1121bdcf28616d4b0d7a85ca10cf6a7efd3aedcab71d09f6eddcdc9dc5845`.
Existing approved designs stay unchanged; no application implementation approval.

## Step 10 ownership and visual boundary

005_DD-ORD extends SCR-001/SCR-002 without a new route or DD family. It owns
line-related client sequences and presentation; the approved 005 API/FN documents
retain the contract/backend algorithms. All four DD family documents exist.
Local EN/JA static HTML continues the already recorded unavailable native design/
Artifact fallback. No business DEC-001–DEC-010 changed and no plan revision added.

## Sequential design approval — 005_DD-ORD version 1

The user explicitly replied "approved" on 2026-10-01 after receiving ORD version 1
and its PDF/mockup companions. Approved source SHA-256: `0b06cb38263959401fc8c83490bb3b3970bae0795d7f3214fb9e0688891592fa`.
This authorizes revision 1 steps 11–12: final reconciliation/test planning and
presentation of implementation revision 2. It does not approve executing
revision 2. DEC-001–DEC-010 remain unchanged; no new business decision needed.

## Implementation plan revision 2 approval

The user explicitly replied "approved" on 2026-10-01 after the revision 2
presentation. Local feature implementation, locked package restoration, tests
and isolated migration/recovery rehearsals are authorized within its eight steps.
Git commit/push/PR/merge/publication/deployment remain excluded. No design
change, new business decision or scope expansion authorized.


## Delivery revision 3 approval — 2026-10-01

Explicit user "approved" received after revision 3 presentation. Scoped commit,
feature-branch push, PR creation/update and CI review are authorized. Merge,
branch/worktree deletion, deployment and video changes remain excluded.


## Merge and cleanup authorization — current-state reconciliation

After completed revision 3 delivery and final-head CI success, user explicitly
requested merging PR #34. Squash merge completed at 2026-10-01 08:13:58 UTC as
42e8932. Subsequent continuation requested cleanup: merged branches/worktree
removed; unrelated drafts/configuration preserved. This task-specific instruction
supersedes the earlier exclusions only for this completed merge/cleanup. No
deployment permission inferred. Reconciled under WI-010 revision 1 step 1.
