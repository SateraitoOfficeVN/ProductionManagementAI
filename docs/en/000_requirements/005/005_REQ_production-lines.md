# Production lines — Requirements

## Document control

| Field | Value |
| --- | --- |
| Document ID / version | 005_REQ / 1 |
| Work item | WI-009 |
| Date / author | 2026-10-01 / Agent |
| State | Requirements confirmed; not implementation authorization |
| Plan | [Revision 1](../../../../work-items/WI-009/plan.md), explicitly approved 2026-10-01 |

## 1. Objective and scope

Add Japanese Production lines / work-center master data to the existing single
plant application. Maintain line identity, working hours/day and supported
products with minutes per one product unit. Assign a compatible line to an order
before it starts production. Product master is already implemented; preserve
its existing unit and retirement rules.

No plant calendar, scheduling, load chart, automatic assignment, material/BOM,
results entry, new authentication provider, new roles or live deployment.
Capacity describes current master configuration. It does not reserve hours,
reject overloaded days or recalculate historical production durations.

## 2. Actors and use cases

| Actor | Use case | Result |
| --- | --- | --- |
| Admin / Operator | UC-017: maintain production lines and supported-product timings | Find, create, edit and retire line or line/product association |
| Existing order editor | UC-018: select a production line and start production | Save eligible assignment and apply the confirmed Draft/status rules |

Admin and Operator both have master write permission, enforced at the API and
UI. Other authenticated roles must not acquire these write permissions by hiding
or showing controls. No expansion of the project's general role matrix.

## 3. Requirements and acceptance criteria

| ID | Requirement | Success criteria | Failure / exception criteria |
| --- | --- | --- | --- |
| REQ-064 | List and find lines | Desktop table/mobile cards show code, name, working hours/day and active/retired state; code/name search, state filter and paging support empty/loading/error states | No data fabrication on fetch failure; forbidden/unauthenticated access has explicit state; applied filters survive reload via URL |
| REQ-065 | Maintain identity | Admin/Operator create a line and edit its name and hours; code trimmed at creation, immutable and case-insensitive unique, including retired lines | Blank or duplicate code/name invalid; code changes rejected server-side; stale update cannot overwrite newer data |
| REQ-066 | Configure supported products and time | Each line/product pair has a positive minutes-per-one-product-unit value with <=3 decimals; line hours/day >0 and <=24 with <=3 decimals; display the actual product unit | Zero/negative/overprecision/out-of-range hours, duplicate pair and invalid product rejected; old-unit pairs cannot be selected or start production until explicitly reconfirmed |
| REQ-067 | Retire while preserving history | Retire active line/pair with confirmation; retain old order assignment, including unchanged historical saves; current timing changes do not alter historical quantities or recalculate history | New/changed assignment to retired line/pair blocked; no hard deletion of referenced rows; Cancel/Escape makes no write |
| REQ-068 | Assign and lock lines on orders | Draft may have no line; Draft -> InProgress requires active compatible line/pair; existing InProgress/Completed orders with no line may retain null; assignment locked outside Draft | Incompatible, inactive, nonexistent or stale-unit new assignment rejected, including retirement races; outside-Draft line changes rejected; no fabricated legacy backfill |
| REQ-069 | Preserve secure accessible behavior | Japanese UI; centered accessible confirmations; keyboard focus/return, mobile/zoom reachability; distinct centralized navbar glyph; exact numeric display and role checks | API must independently reject unauthorized writes; no mixed-unit capacity sum, lost dirty draft, silent conflict overwrite or credential logging |

## 4. Confirmed business rules

[Decision record](../../../../work-items/WI-009/decisions.md) is the approval source.

1. Line code: trim at creation, compare case-insensitively, immutable after creation.
2. Hours/day: greater than 0, not more than 24; at most three fractional digits.
3. Production time: minutes for exactly one unit of the selected product, not a
   batch; positive, at most three fractional digits, separately per line/product.
   Examples of the quantity basis: 1 個, 1 本, 1 枚, 1 台, 1 セット, 1 kg or 1 m.
4. Product unit is read-only in line configuration; maintain it in Product master
   under existing WI-006 constraints. After a unit change, old-unit timings require explicit reconfirmation before
   new selection or starting production (DEC-009). No implicit conversion.
5. Retirement removes eligibility for new selection, preserves existing references
   and allows unchanged historical saves. Reaching InProgress from Draft is a
   new production-start validation and still requires active eligibility.
6. A product change in Draft must revalidate/clear an incompatible line selection;
   saving fails if a submitted line is not eligible for the resulting product.
7. Line is locked outside Draft. Legacy InProgress/Completed orders with null line
   remain editable only for otherwise permitted fields; no mandatory assignment.
8. Line/pair timing changes describe current configuration only. No historical
   estimated-duration snapshot, accounting recomputation or scheduling algorithm
   is introduced by these requirements.

## 5. Information and boundaries

SCR-005: 「生産ライン・工程」 (Production lines and processes). Proposed routes:
`/production-lines`, `/production-lines/new`, `/production-lines/:id/edit`.
Existing SCR-001 gains a line selection/display; SCR-002 shows assigned line or
an explicit unassigned value. Existing dashboard quantities/counts stay unchanged.
Use new WI-009-owned order-impact designs; completed old designs are read only.

Line code/name are business reference data, not authentication credentials.
Use existing cookie/role enforcement, same-origin JSON mutations, server validation
and optimistic concurrency. Keep telemetry dimensions bounded; no names, raw
search text, quantities, secrets or timing values as metric labels.

## 6. Verification and prerequisites

Trace REQ-064–069 through 005_BD, 005_DB, DD/API/FN/SPD and new order-impact DD,
then implementation plan revision 2 and tests. Planned tests: role/validation,
case-insensitive code conflict, time precision, compatible assignment/status lock,
retirement/selection race, historical exceptions, stale saves, keyboard/axe,
mobile/zoom and existing order/dashboard regression. None has run for WI-009.

Schema change must be additive with nullable historical assignment and explicit
rollback limits; no automatic mapping of old orders to invented lines. Exact API
and schema fields, technical length/maximum limits and transaction locking are
owned by the sequential design phase, not assumed implemented here.

All business questions raised for this phase are answered in the decision record.
Plan approval is not approval of design or application.
