# WI-009 — Architecture assessment

2026-10-01. Approved revision 1 step 3; assessment only, no new ADR.

## Inputs and conclusion

Confirmed REQ-064–REQ-069 and DEC-001–DEC-009. Existing 0001_ADR layered
.NET architecture and 0002_ADR cookie/RBAC foundation are read only. Reuse React,
same-origin controllers, Application use cases/ports, EF repositories and the
existing PostgreSQL database. No new external service, deployment topology,
authentication provider, trust boundary or dependency is needed. No ADR needed.

## Changes to design

New line and line/product reference data, per-unit timing and nullable historical
order line assignment. Application validates current product, line and pair
eligibility inside the write transaction. Existing optimistic concurrency and
product share-lock patterns constrain the detailed transaction design.

Product master keeps its existing unit-edit rule. Timing carries its configured
unit; after a permitted product-unit edit, mismatched configurations are not
eligible until explicitly reconfirmed. No inferred unit conversion or historical
recalculation. Draft -> InProgress validates current eligibility even when a
Draft retained an old retired assignment; unchanged non-Draft history is retained.

## Security and data integrity

Existing authentication boundary; enforce Admin/Operator line writes in API/UI.
Validate all identifiers and bounded text/decimal inputs. Same-origin JSON-only
mutations and existing session/error posture apply. No PII/secrets in master data;
no credentials, raw search/name or high-cardinality dimensions in metric labels.
Server revalidation prevents forged incompatible selection and race with retirement.
Detailed API/FN will specify telemetry and locking; this is not a completed runtime
security gate. Additive migration, no invented backfill, FK history preservation
and rollback limits must be explicit in DB design.

## Reservations and sequence

SCR-005; UC-017 and UC-018; document group 005; REQ-064–069; functions FN-032–036
follow existing FN-027–031. BD first, then DB/main DD/API/FN/SPD/order-impact DD,
each reviewed before the next file. Old approved design files remain unchanged.
Only BD is produced now; implementation waits for approved revision 2.
