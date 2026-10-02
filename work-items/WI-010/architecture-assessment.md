# WI-010 — Architecture and security assessment

2026-10-01. Approved revision 1 step 3. Inputs: approved 006_REQ version 1,
DEC-001–007 and accepted RP-01–05; baseline 42e8932.

## Existing boundaries and assessment

Reuse 0001_ADR layered backend and 0002_ADR cookie/RBAC foundation: Japanese
React catalog and central icons → same-origin controller → Application calendar
use cases/ports → EF/PostgreSQL. Existing plant clock defines authoritative
DateOnly/today/timezone. No new service, provider, queue, scheduling job, identity
boundary, role or dependency. A new ADR is unnecessary; current scope fits the
approved stack. Existing ADRs remain immutable.

Calendar owns effective weekly definitions and plant/line date exceptions with
retained revisions. Product/line identity, current hours, timing and unit generation
remain owned by WI-006/WI-009. Capacity is derived lookup, not persisted production
history, allocation or a change to order/dashboard rules. DB/API/FN will define
coherent snapshots, version checks, midnight boundary and migration activation.

## Proportional alternatives

- External holiday provider: rejected by confirmed manual-entry scope; no external
  data trust boundary or provider dependency needed.
- Stored historical capacity/automatic scheduling: outside approved scope; current
  configuration lookup avoids claiming past capability or production outcomes.
- Rewrite existing line or order models: unnecessary; date rules are additive and
  completed existing designs stay read only.
- Separate global day-hours baseline: inconsistent with accepted RP-03; use the
  selected line's current hours unless the winning exception supplies hours.

## Security and integrity assessment

| Risk / boundary | Required design control |
| --- | --- |
| Client spoofs role or editable date | Admin/Operator on all APIs; plant-local authoritative today; validate after relevant serialization/locks; past mutation rejected independently of UI |
| Stale/concurrent save or delete loses history | Optimistic concurrency; atomic retained revision transition; reject stale requests; past effective result protected |
| Configuration changes during derived read | Consistent calendar/line/product/unit snapshot; current unit generation validated; unsupported state unavailable rather than fabricated zero |
| Duplicate/ambiguous overlapping definitions | Unambiguous effective-date resolution, one current scope/date exception; DB and application constraints specified in DB/API/FN |
| Browser retries an uncertain write | Block replay, preserve draft, read verification and explicit reconciliation; no automatic mutation retries |
| Date/precision coercion | Date-only transport and exact numeric boundaries; contradictory hours/state rejected; parameters validated and bound |
| Injection or diagnostic leakage | No unvalidated SQL/path construction; parameterized queries, generic errors, bounded operation/outcome telemetry; no raw reasons/dates/names/credentials |
| Privilege escalation in schema/runtime | Owner-only additive migrations; restricted runtime grants; no runtime DDL |

No new payment/PII/provider/authentication boundary introduced. Existing cookie
transport/session policies must still be reviewed in the later API/security gate.
This is design assessment only; no implementation, penetration test, migration or
runtime accessibility verification performed.

## Handoff

SCR-006 only; no change in existing screen semantics or approved designs.
Use distinct central CalendarRange navigation icon alias, subject to installed
icon export audit before implementation. Stable function IDs FN-037–040 cover
month resolution, weekly versions, date exceptions and dated capacity.
Technical endpoint/schema/length/range/transaction contracts belong to sequential
DB/API/FN documents. Business questions resolved; no scope revision needed.


## Final impact assessment — 2026-10-02, revision 1 step 10

All seven version-1 designs approved. The feature owns SCR-006 only. Its shared
navbar/router registration adds the already-designed entry without changing any
existing screen's fields, order assignments, quantity meaning or date rules.
Existing master tables are read by coherent capacity lookup; no master/order write
or existing schema/index/trigger rewrite is required. The three new calendar tables
and owner activation are additive; existing-line FK DDL still has bounded lock risk.
Calendar controls never appear as a new order due-date/start/dashboard constraint.

Therefore conditional 006_DD-IMP_calendar-impacts is not applicable. No new ADR
or design document is needed. Existing approved designs remain immutable. Runtime
integration points (DI, scoped telemetry, navbar/catalog/router) are included in
proposed implementation revision 2 and regression TC-407, not implementation proof.
