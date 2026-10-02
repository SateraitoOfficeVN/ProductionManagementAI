# WI-009 — Production lines

| Work item | Workflow | State |
| --- | --- | --- |
| WI-009 | [feature-delivery](../../ai/workflows/feature-delivery.md) | done — merged |

## Objective and sources

The user requested the next feature after WI-008 merged. The standing direction
is the Master data group. Confirmed next function: Production lines / work centers,
Japanese screen name 「生産ライン・工程」 (Production lines and processes).
The user selected this feature and confirmed Admin/Operator write access.
The user selected working hours per day and per-product production time as the
capacity model; a direct product-unit daily quantity is not the selected model.

[Development opportunities](../../docs/en/pdf/project-development-opportunities.pdf),
pages 1 and 5, proposes line capacity, supported products and line assignment on
orders. It identifies Product master as the dependency. This PDF is a proposal,
not an approved business specification. Product master is implemented by WI-006;
WI-008 fixes are merged in master at `20c8d61`.

## Confirmed business scope and acceptance criteria

REQ-064–REQ-069 identify confirmed business scope. Detailed technical rules are defined by the approved 005 design family.
Local implementation and verification under revision 2 are complete; commit/PR
and deployment remain outside its authorization.

| ID | Proposed requirement | Successful path | Failure / exception path |
| --- | --- | --- | --- |
| REQ-064 | View and search production lines on desktop/mobile | Authenticated permitted users can view line code, name and active/retired state; empty/loading/error states accessible | Unauthorized access is rejected; failed loading does not display fabricated data |
| REQ-065 | Create and edit line identity | Authorized roles save a unique code and a name; immutable code with trimming/case-insensitive uniqueness | Duplicate/blank values rejected server-side; stale saves report a conflict without overwriting changes |
| REQ-066 | Maintain supported products and time-based capacity | Authorized users maintain working hours per day and per-product production time; supported active products can be associated with a line | Duplicate association, invalid working hours or production time and incompatible unit semantics rejected; hours/day must be >0 and <=24 with up to 3 decimals; minutes per one product unit must be >0 with up to 3 decimals; invalid values rejected |
| REQ-067 | Retire a line without destroying history | Retired line remains visible on existing orders and is excluded from new assignment | No hard deletion of referenced data; unchanged retired assignment on historical edits may be retained; changed/new assignment to retired lines rejected |
| REQ-068 | Assign a compatible line to production orders | Draft may omit a line; transition to InProgress requires a valid compatible line; old InProgress/Completed orders retain their missing assignment | Inactive/incompatible new assignment rejected at save, including product/line changes and concurrent retirement; line changes outside Draft rejected, with legacy missing assignments retained |
| REQ-069 | Preserve accessible, secure and unit-consistent behavior | Japanese UI, distinct centralized icon, keyboard/mobile usability, API role checks and exact quantity/capacity display | Hidden UI cannot bypass authorization; mixed units are not summed as a single capacity; API errors preserve form input |

## Exclusions

Scheduling, load/overload charts, automatic allocation, plant calendar, BOM,
results entry, new roles and physical shop-floor integration are not included.
Time-based capacity is descriptive master data in this scope; automatic
scheduling and overload enforcement are excluded. Actual local verification is recorded in evidence.md; no deployed behavior is claimed.

Completed approved design documents are read only. Any impact on orders gets a
new WI-009-owned design document. Screen number 005 and document paths are
proposed, subject to requirements and the existing numbering audit.

## Confirmed business decisions

See [decisions](decisions.md). Feature selection, Admin/Operator write permissions and time-based capacity
measurement are confirmed by the user. Working-hours limits and production-time unit/precision/basis are confirmed:
0 < hours/day <= 24, up to 3 decimals; positive minutes per one product unit,
up to 3 decimals, configured per line/product. Line code, retired-line history and assignment requirement/legacy exceptions
are confirmed. Line assignment is locked outside Draft; legacy missing assignments may be
retained during unrelated edits (DEC-008). Association retirement preserves existing assignments; new selection is blocked,
and current time changes do not recalculate history. DEC-009 is confirmed: retain existing product-unit edit rights; require explicit
timing reconfirmation in the new unit before new selection or starting production.
DEC-001–DEC-010 are resolved. The user approved the proposed Admin/Operator
read access; write roles remain unchanged.

[Plan](plan.md) · [Status](status.md) · [Evidence](evidence.md)

Finalized [005_REQ version 1](../../docs/en/000_requirements/005/005_REQ_production-lines.md)
and [005_BD version 1](../../docs/en/010_basic-design/005/005_BD_生産ライン・工程.md)
are available with their English/Japanese PDFs. BD was explicitly approved on 2026-10-01.
[005_DB version 1](../../docs/en/database/005/005_DB_生産ライン・工程.md)
and its EN/JA PDFs were explicitly approved on 2026-10-01.
[Main 005_DD version 1](../../docs/en/020_detailed-design/005/005_DD_生産ライン・工程.md)
with EN/JA PDFs was explicitly approved on 2026-10-01.
[005_DD-API version 2](../../docs/en/020_detailed-design/005/005_DD-API_生産ライン・工程.md)
with current EN/JA PDFs was explicitly approved on 2026-10-01.
[005_DD-FN version 1](../../docs/en/020_detailed-design/005/005_DD-FN_生産ライン・工程.md)
with EN/JA PDFs was explicitly approved on 2026-10-01.
[005_DD-SPD version 1](../../docs/en/020_detailed-design/005/005_DD-SPD_生産ライン・工程.md)
with three SVGs, EN/JA static HTML and current EN/JA PDFs was explicitly approved on 2026-10-01.
[005_DD-ORD version 1](../../docs/en/020_detailed-design/005/005_DD-ORD_production-line-assignment.md)
with four SVGs, EN/JA static HTML and current PDFs was explicitly approved on 2026-10-01.
All sequential designs are approved. Final design/security assessment is in
[review.md](review.md), with planned TC-326–365 in [test-plan.md](test-plan.md).
Implementation plan revision 2 is explicitly approved. Local implementation and
isolated verification are complete; revision 3 commit/PR delivery is approved.


Current-state reconciliation: PR #34 merged as 42e8932; final exact-head CI
passed; branch/worktree cleanup complete. No deployment. WI-010 revision 1
step 1 updates these routine records; approved design documents remain unchanged.
