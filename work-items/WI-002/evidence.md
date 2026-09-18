# Production Order Create/Edit (Screen A) — Requirements Traceability & Evidence

As of source revision `e7e0d36` plus uncommitted WI-002 design files, 2026-09-18.

## Traceability matrix

| Requirement ID | Requirement | Design artifact | Code / PR | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| REQ-010 | Create order | BD-001 business flow, §3, §6 E-04/E-05; DB-002 counter table, transactions; DD-001 P-02; DD-001-API §2; DD-001-FN §3; DD-001-SPD §2 | — | — | not started |
| REQ-011 | Edit order | BD-001 business flow, 0-3, §5 V-08, §6 E-01/E-05; DB-002 `xmin` concurrency; DD-001 P-01/P-03; DD-001-API §3–4; DD-001-FN §2, §4; DD-001-SPD §1–3 | — | — | not started |
| REQ-012 | Admin/Operator only | BD-001 0-1, actions, exception flows; DD-001 module 6, P-01 step 1; DD-001-API common auth | — | — | not started |
| REQ-013 | Quantity positive integer | BD-001 §5 V-02; DB-002 `ck_production_orders_quantity_positive`; DD-001 item (9), MSG-E003/E010 | — | — | not started |
| REQ-014 | Due date ≥ today | BD-001 §5 V-03, V-04 (DEC-009, DEC-011); DD-001 item (10), P-02/P-03, `IPlantClock` | — | — | not started |
| REQ-015 | Product exists | BD-001 §5 V-01; DB-002 `fk_production_orders_products_product_id`; DD-001 item (8); DD-001-API §2 fields | — | — | not started |
| REQ-016 | Notes ≤ 500 | BD-001 §5 V-05; DB-002 `notes varchar(500)`; DD-001 item (11) | — | — | not started |
| REQ-017 | Status state machine | BD-001 status diagram, §4 M-02, §5 V-06; DB-002 `ck_production_orders_status`; DD-001 module 2, state transitions | — | — | not started |
| REQ-018 | Product/quantity lock | BD-001 §3, §5 V-07; DD-001 module 1 `Update`; DD-001-FN §4 step 7 | — | — | not started |
| REQ-019 | Confirm discard on Cancel | BD-001 §3 items 17–19, §6 E-07/E-09; DD-001 module 9, P-04; DD-001-SPD §4 | — | — | not started |

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| 2026-09-18 | design-consistency checklist (BD-001 scope) | manual review | local | pass for BD-level items; DD/DB/test items not yet applicable (see below) | this file |
| 2026-09-18 | design-consistency checklist (DB-002 scope) | manual review | local | pass — every index justified, DB vs. application-only rules listed with reasons, migration impact stated; DEC-016 (role split) decided 2026-09-18 — runtime login and grant list added | this file |
| 2026-09-18 | design-consistency checklist (DD-001 scope) | manual review | local | pass — see the checklist walk below; mockup published privately | this file |
| 2026-09-18 | Design review | user review of the DD-001 set and mockup | — | pass — approved by the user ("the DD is reviewed and approved") | status.md |
| 2026-09-18 | Unit / integration / E2E | — | — | not run — no code in scope for plan revision 1 | — |

Design-consistency walk for BD-001:

- Requirements have stable IDs and acceptance criteria — pass (brief.md has success and failure criteria for each REQ).
- BD covers navigation, primary actions and exceptions — pass (screen transition, §6 events, exception flows).
- DD agrees with BD; API/DB mappings agree — pass: DD-001 items (6)–(19) follow BD-001 §3–§6 (BD-001 revision 4 picked up the one DD refinement, the V-02 upper bound); DD-001-API fields match DB-002's API mapping, and `version`/`allowedNextStatuses`/`isProductQuantityEditable` were added as response-only fields.
- Missing decisions resolved before dependent implementation — pass (DEC-001–DEC-012 decided; remaining items are DB/DD technical choices).
- Test scenarios map to design — pass: DD-001 test viewpoints cover REQ-010–REQ-019 and DEC-001/009/010/012/013/016/017/020 (test-plan IDs are assigned when test-plan.md is written).
- Security-relevant fields identified — pass (server-side role gate; no PII or secrets; CSRF decided in DEC-020; least-privilege runtime login in DEC-016).
- Accessibility captured — pass (BD-001 non-functional requirements).
- Migration impact — pass (DB-002: additive, recovery limit and rollback command stated).
- Tracing/logging specified — pass: DD-001 Observability section (spans, counters, attributes, what is never logged). OpenTelemetry isn't wired in the backend yet, so plan revision 2 must add it.

## Defects, failures and blockers

| Item | Reason | Blocker | Follow-up |
| --- | --- | --- | --- |
| None open | — | — | — |

## External references

- PR: not opened — not authorized
- CI run: not applicable
- Deployment: not applicable

## Remaining limitations and next action

Design set complete (brief, BD-001, DB-002, DD-001, DD-001-API, DD-001-FN, DD-001-SPD, mockup); nothing is implemented or tested. Next action: approval of plan revision 2 (implementation), which covers the DB login split (DEC-016), OpenTelemetry setup, Playwright E2E (DEC-025) and axe checks (DEC-026).
