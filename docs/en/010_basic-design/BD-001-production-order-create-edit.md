<!-- Basic Design Document (基本設計書) template, based on conventional Japanese SI basic-design composition (system overview, architecture, function/screen list, business flow, data design overview, external interfaces, non-functional requirements). Copy into the relevant work item or docs/en/010_basic-design area; replace {bracketed} prompts with task-specific facts, or "Not applicable" with a reason. Do not fabricate results or approval. -->

# Production Order Create/Edit — Basic Design Document (基本設計書)

BD-001 — WI-002, based on brief.md revision 1.

## System overview

A single screen that lets an authenticated Admin or Operator create a new production order or edit an existing one, including moving it through its status workflow. This is the harness's first end-to-end business screen ("Screen A").

| Requirement ID | Description | Covered by section |
| --- | --- | --- |
| REQ-010 | Create a new production order | Actors and business flow; Success and exception flows |
| REQ-011 | Edit an existing production order | Actors and business flow; Success and exception flows |
| REQ-012 | Unauthenticated users cannot create/edit | Actions and business rules |
| REQ-013 | Quantity must be a positive integer | Actions and business rules |
| REQ-014 | Due date must be today or later | Actions and business rules |
| REQ-015 | Product must reference an existing product | Actions and business rules; Data design overview |
| REQ-016 | Notes optional, max 500 characters | Actions and business rules |
| REQ-017 | Status follows a fixed state machine | Screen list and screen transition; Actions and business rules |
| REQ-018 | Product/quantity locked once order leaves Draft | Actions and business rules |

## Overall configuration and architecture

Fits the confirmed stack from `ai/project.md` / `work-items/WI-001/decisions.md`: React (Vite+TS+Tailwind) frontend calling a .NET 10 EF Core backend (layered: Domain/Application/Infrastructure/Api) over a JSON API, backed by PostgreSQL 17. The screen sits behind the cookie-based auth session and role check established by WI-001 (`AuthContext`/`ProtectedRoute` on the frontend, the authenticated-by-default policy plus a role check on the backend). No new architecture boundary is introduced by this screen — it uses the same API/DB pattern WI-001 establishes. Formal layering/auth-boundary rationale is recorded in ADR-0001 and ADR-0002 (`work-items/WI-001/plan.md` step 3, not yet written); this BD does not depend on their exact text, only on the confirmed stack decisions already in `work-items/WI-001/decisions.md`.

## Function list

| Function ID | Function name | Description | Related requirement ID |
| --- | --- | --- | --- |
| FN-001 | Create production order | Persist a new order with a system-generated order number and `Draft` status | REQ-010 |
| FN-002 | Edit production order | Persist changes to an existing order's editable fields | REQ-011, REQ-018 |
| FN-003 | Validate order input | Enforce quantity, due date, product-reference and notes-length rules | REQ-013, REQ-014, REQ-015, REQ-016 |
| FN-004 | Enforce status transition rules | Allow only the defined state-machine edges | REQ-017 |
| FN-005 | Enforce field lock by status | Reject/ignore product or quantity changes once order is not `Draft` | REQ-018 |
| FN-006 | Enforce authentication and role | Reject unauthenticated requests; require `Admin` or `Operator` role | REQ-012 |

## Actors and business flow

Actors: Admin, Operator (both authenticated via WI-001's login flow before reaching this screen).

1. Admin/Operator navigates to the Production Order Create/Edit screen (create mode via a "New order" entry point, or edit mode via an existing order's route) → system renders the form, pre-filled in edit mode.
2. User enters/changes product, quantity, due date, status (edit mode only, per allowed transitions) and notes → system performs client-side validation matching FN-003/FN-004/FN-005 as the user types/submits.
3. User submits the form → system re-validates authoritatively server-side (FN-003, FN-004, FN-005, FN-006) → on success, system persists the order (FN-001 or FN-002) and shows a success state; on failure, system shows the specific field-level or action-level error and makes no change.

## Screen list and screen transition

| Screen ID | Screen name | Entry point | Exit / next screen |
| --- | --- | --- | --- |
| SCR-001 | Production Order Create/Edit | Create mode: a "New order" action (exact trigger location — e.g. Screen B's list — to be finalized once Screen B exists; for now, a direct route). Edit mode: navigating to an existing order's route (e.g. from a future Screen B row action) | On successful save: show a success state and remain on SCR-001 with the persisted values (create mode transitions to edit mode for the same order); no navigation to Screen B yet since it doesn't exist. On cancel: return to the entry point (Screen B once it exists; otherwise no-op) |

Status transition diagram (state machine per REQ-017):

```
Draft --> InProgress --> Completed
  |            |
  v            v
Cancelled   Cancelled
```

`Completed` and `Cancelled` are terminal — no outgoing transitions from either.

## Actions and business rules

| Action | Trigger | Business rule | Related requirement ID |
| --- | --- | --- | --- |
| Create order | User submits the form in create mode | Requires authenticated `Admin` or `Operator`; product must exist; quantity > 0 integer; due date ≥ today; notes ≤ 500 chars; new order gets a system-generated sequential order number and status `Draft` | REQ-010, REQ-012, REQ-013, REQ-014, REQ-015, REQ-016 |
| Edit order | User submits the form in edit mode | Same field validation as create, applied only to editable fields for the order's current status | REQ-011, REQ-012, REQ-013, REQ-014, REQ-015, REQ-016 |
| Change status | User selects a new status in edit mode | Only transitions along the defined edges (`Draft→InProgress`, `Draft→Cancelled`, `InProgress→Completed`, `InProgress→Cancelled`) are accepted; any other requested transition is rejected | REQ-017 |
| Edit product/quantity | User changes product or quantity in edit mode | Allowed only while order status is `Draft`; once status is `InProgress`, `Completed` or `Cancelled`, these fields are read-only in the UI and rejected/ignored if submitted anyway | REQ-018 |
| Access the screen or submit the form | Any request to SCR-001 or its API | Requires a valid authenticated session; unauthenticated requests are rejected (401) and the frontend redirects to `/login` | REQ-012 |

## Success and exception flows

| Flow | Trigger condition | System behavior | Resulting state |
| --- | --- | --- | --- |
| Success — create | All fields valid, user authenticated with required role | Order persisted with new order number, status `Draft` | SCR-001 shows the created order in edit mode with a success indicator |
| Success — edit | All fields valid for the order's current status, valid status transition (if any) | Order's editable fields updated | SCR-001 shows the updated order with a success indicator |
| Exception — unauthenticated | No valid session | Request rejected, HTTP 401 | Frontend redirects to `/login`; no order created/modified |
| Exception — validation failure | Quantity ≤ 0/non-integer, due date in the past, unknown product, or notes > 500 chars | Request rejected with field-level error(s) | SCR-001 shows the specific field error(s); no order created/modified |
| Exception — invalid status transition | Requested status is not a valid edge from the current status | Request rejected with an action-level error | SCR-001 shows the transition error; order status unchanged |
| Exception — locked-field edit attempt | Product/quantity change submitted while order is not `Draft` | Request rejected (or those fields silently ignored server-side, decision deferred to DD) with an error/no-op | SCR-001 shows the lock error or the fields remain unchanged; other valid changes in the same request may still be evaluated (exact behavior — reject whole request vs. ignore locked fields — deferred to DD) |

## Data design overview

Two entities, at business level (column-level detail belongs in `database-design`):

- **ProductionOrder**: order number (system-generated, unique), reference to a Product, quantity, due date, status (`Draft`/`InProgress`/`Completed`/`Cancelled`), notes, created/updated timestamps.
- **Product**: name, SKU — a minimal reference entity seeded with sample data (`work-items/WI-002/decisions.md` DEC-005), not a full catalog; ProductionOrder has a required reference to exactly one Product.

## External interfaces

None — this screen only talks to this application's own backend API, itself authenticated via the session established by WI-001. No third-party or external system integration.

## Non-functional requirements

- **Security-relevant fields:** the role check (`Admin`/`Operator` required) gates a state-changing action and must be enforced server-side, not just hidden in the UI — flagged per `ai/checklists/design-consistency.md`. No field in ProductionOrder/Product is PII or a secret.
- **Accessibility:** per `ai/rules/frontend.md`, the form must be keyboard-operable, show visible focus indicators, meet color-contrast requirements, and surface validation errors in a way assistive technology can announce (WCAG 2.2 AA) — detailed interaction spec deferred to `screen-design`/`detailed-design`.
- Otherwise inherits project defaults; no additional performance/availability requirement identified for this screen.

## Open questions and linked DD

| Question | Linked DD section | Status |
| --- | --- | --- |
| Exact behavior when a locked-field edit is attempted alongside other valid changes in the same request (reject whole request vs. ignore locked fields and apply the rest) | DD-001 (not yet written) — Actions/validation section | open |
| Exact status-transition UI control (dropdown restricted to valid next states vs. free select + server rejection) | DD-001 (not yet written) — Screen fields/interactions section | open |
| Exact order-number generation mechanism (DB sequence vs. application-level counter) and collision handling | Future `docs/en/database/0002-production-order-schema.md` | open |
