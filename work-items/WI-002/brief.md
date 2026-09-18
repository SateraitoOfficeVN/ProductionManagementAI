# Production Order Create/Edit (Screen A) — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-002 | Claude (for ThanhTN) | draft — revision 1 | unscheduled (demo MVP) |

Revision 1, 2026-09-18. Restart of WI-002 after its first design pass was scrapped when `ai/templates/basic-design.md` and the detailed-design template family were rewritten (commit `e7e0d36`). The business answers from that first pass are carried over unchanged — see `decisions.md` DEC-001–DEC-008 and `demos/01-basic-design/WI-002/recording.md` / `demos/03-database-design/WI-002/recording.md` for where each was originally given.

## Overview

Screen A lets a signed-in Admin or Operator create a new production order or edit an existing one, including moving it through its status workflow. It is the first end-to-end business screen of ProductionManagementAI and the vehicle for the four demo walkthroughs (BD, DD, DB, implementation).

## Objective

Demonstrate the full AI development lifecycle (requirements → BD → DB → DD → code → test → PR) on one small but realistic screen, per `docs/vi/000-mo-ta-harness-va-quy-trinh-phat-trien-ai.md` §14 and the roadmap locked in `work-items/WI-001/decisions.md` DEC-008/DEC-010 (Screen A = production-order create/edit).

## Success metrics

| Goal | Metric | Target |
| --- | --- | --- |
| Full traceability | Every `REQ-###` below maps to a BD section, DD section, DB/API element and at least one test case | 100% of in-scope REQs |
| Correct business rules | Acceptance criteria below pass as automated tests (unit/integration/E2E) | all pass |
| Demo usability | A reviewer can create and edit an order end-to-end in the local Docker Compose environment | demonstrated once, with evidence |

## Assumptions

- Users reach this screen already authenticated via WI-001's cookie login (`docs/en/architecture/0002-auth-rbac-foundation.md`).
- `Admin` and `Operator` are the only roles in the system for now (WI-001 DEC-015 placeholder roles); DEC-001 confirms both may use this screen.
- Products are a minimal reference table seeded with sample data; there is no product-catalog UI (DEC-005).
- There is no list screen yet (Screen B, a later work item); until it exists, the edit screen is reached by its direct route.
- Single-site use with one configured plant timezone, `Asia/Tokyo` (DEC-011, DEC-017).

## Actors and user stories

| Actor | As a… | I want to… | So that… | Use case ID |
| --- | --- | --- | --- | --- |
| Admin / Operator | production planner | create a production order for a product, quantity and due date | the shop floor knows what to produce and by when | UC-001 |
| Admin / Operator | production planner | edit an existing order's details | the order reflects current plans | UC-002 |
| Admin / Operator | production planner | move an order through Draft → In Progress → Completed, or cancel it | the order's status reflects real progress | UC-003 |

## Requirements (in scope)

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| REQ-010 | An authenticated Admin or Operator can create a production order. | **Success:** submitting a valid product, quantity, due date and optional notes creates an order with a system-generated unique order number in the format `PO-YYYY-NNNNN` (DEC-012) and status `Draft`, and the screen then shows that order in edit mode with a success message. **Failure:** any validation failure (REQ-013–REQ-016) creates nothing and shows the field error(s). | must |
| REQ-011 | An authenticated Admin or Operator can edit an existing production order. | **Success:** changing editable fields and saving persists the changes and shows a success message with the saved values. **Failure:** an unknown order ID shows a not-found message and no form; a validation failure changes nothing and shows the field error(s); if the order was changed by someone else since it was loaded, the save is rejected and the user is asked to reload (DEC-010). | must |
| REQ-012 | Only authenticated users with the `Admin` or `Operator` role can view or use the screen or its API. | **Success:** an Admin or Operator can open the screen and save. **Failure:** an unauthenticated request is rejected with 401 and the UI redirects to `/login`; an authenticated user without either role is rejected with 403 and nothing is changed. | must |
| REQ-013 | Quantity is required and must be a positive whole number. | **Success:** `1` and `250` are accepted. **Failure:** empty, `0`, `-5`, `2.5` and non-numeric input are rejected with a field error; nothing is saved. | must |
| REQ-014 | Due date is required and must be today or later. | **Success:** today's date and a future date are accepted. **Failure:** empty or yesterday's date is rejected with a field error; nothing is saved. On edit, the rule applies only when the due date is changed, so an overdue order can still be saved (DEC-009). "Today" is the date in the configured plant timezone, `Asia/Tokyo` (DEC-011, DEC-017). | must |
| REQ-015 | Product is required and must reference an existing product. | **Success:** selecting a product from the list is accepted. **Failure:** no selection, or a product ID that does not exist (e.g. a crafted API request), is rejected with a field error; nothing is saved. | must |
| REQ-016 | Notes are optional, at most 500 characters. | **Success:** empty notes and exactly 500 characters are accepted. **Failure:** 501 characters are rejected with a field error; nothing is saved. | must |
| REQ-017 | Status follows a fixed state machine: `Draft → InProgress → Completed`, and `Draft → Cancelled` / `InProgress → Cancelled`. `Completed` and `Cancelled` are terminal. New orders always start as `Draft`. | **Success:** each allowed transition is saved. **Failure:** any other transition (e.g. `Draft → Completed`, `Completed → InProgress`, any change from `Cancelled`) is rejected with an error and the status is unchanged; the UI only offers allowed next states (DEC-008). | must |
| REQ-018 | Product and quantity can only be changed while the order is `Draft`. | **Success:** a `Draft` order's product/quantity can be changed. **Failure:** for an `InProgress`, `Completed` or `Cancelled` order these fields are read-only in the UI; a request that changes them anyway is rejected as a whole — no other field in that request is saved (DEC-007). | must |
| REQ-019 | Pressing Cancel after changing the form asks for confirmation before discarding the changes. | **Success:** with no changes, Cancel leaves immediately; with changes, Cancel shows "Discard your changes?" and **Discard** leaves without saving. **Failure:** **Keep editing** (or Escape) closes the dialog and every entered value is kept (DEC-018). | should |

## Not doing (out of scope)

- Production-order list/search (Screen B, separate work item) and dashboard (Screen C).
- Product catalog create/edit UI — products are seeded reference data only (DEC-005).
- Deleting production orders — `Cancelled` is the only way to retire an order.
- Audit history / change log beyond created/updated timestamps.
- Finer-grained permissions than "Admin or Operator" (WI-001 DEC-015 stays open for other screens).

## Open questions

| Question | Impact if unresolved | Owner | Status |
| --- | --- | --- | --- |
| Does the "due date ≥ today" rule apply to an existing order whose saved due date is already in the past when the user saves without changing the due date? | An overdue order could become impossible to save | user | answered in decisions.md (DEC-009) |
| Concurrent edits, meaning of "today", order number format | Edit behavior and data format | user | answered in decisions.md (DEC-010–DEC-012) |
| Plant timezone value, Cancel with unsaved changes, demo products | "Today"/order year, data loss on Cancel, demo realism | user | answered in decisions.md (DEC-017–DEC-019) |
