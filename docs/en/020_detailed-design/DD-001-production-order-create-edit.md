<!-- Based on ai/templates/detailed-design.md (revision at commit e7e0d36). -->

# Production Order Create/Edit — Detailed Design Document (詳細設計書)

DD-001 — implements BD-001 (SCR-001), requirements REQ-010–REQ-019.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | DD-001 |
| Category | UI + API |
| System name | ProductionManagementAI |
| Subsystem name | Production orders |
| Work item | WI-002 |
| Implements | BD-001 revision 4 |
| Created by | Claude (for ThanhTN) |
| Created date | 2026-09-18 |
| Last updated by | Claude (for ThanhTN) |
| Last updated date | 2026-09-18 |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | 2026-09-18 | Claude (for ThanhTN) | Initial creation |
| 2 | 2026-09-18 | Claude (for ThanhTN) | Added the function-design (DD-001-FN) and screen-processing-design (DD-001-SPD) companions; modules 3–5 and flows P-01–P-05 moved there, with pointers left here |
| 3 | 2026-09-18 | Claude (for ThanhTN) | Aligned with implementation (plan revision 2): `AllowedNext()` extension method; OpenTelemetry instrumentation set; test-plan IDs filled in |

## Overview and reference documents (概要・目次)

| Field | Value |
| --- | --- |
| File / component name | Frontend `ProductionOrderPage.tsx` (+ feature components); backend `ProductionOrdersController`, `ProductsController`, `ProductionOrderService`, `ProductionOrder` entity |
| Overview | Create/edit form for one production order, with status transitions, field locking, stale-save protection and discard confirmation, backed by a JSON API over the DB-002 schema |

### Module / method / processing index

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `ProductionOrder` (Domain) | Entity + business invariants: transitions, field lock, quantity/notes rules | Unit-tested without a host (ADR-0001) |
| 2 | `ProductionOrderStatus` (Domain) | Enum + allowed-next-states table | Single source of M-02 / V-06 |
| 3 | `ProductionOrderService` (Application) | Create / get / update use cases, validation orchestration, error mapping | Designed in DD-001-FN |
| 4 | `IPlantClock` / `PlantClock` | "Today" and current year in the plant timezone | Designed in DD-001-FN |
| 5 | `IOrderNumberIssuer` / `OrderNumberIssuer` (Infrastructure) | Per-year sequence upsert | Designed in DD-001-FN |
| 6 | `ProductionOrdersController`, `ProductsController` (Api) | HTTP endpoints, auth policy, Problem Details | Contract in DD-001-API |
| 7 | `ProductionOrderPage` (frontend) | Route component: load, mode, states, title | |
| 8 | `ProductionOrderForm` (frontend) | Fields, client validation, dirty tracking, save | |
| 9 | `DiscardChangesDialog` (frontend) | Confirmation on Cancel with unsaved changes | REQ-019 |
| 10 | `apiClient` (frontend) | JSON fetch wrapper, Problem Details parsing, 401 handling | Shared |
| 11 | Processing flows P-01–P-05 | Load, create, update, cancel, error handling | Designed in DD-001-SPD |

### Reference documents

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| 1 | `docs/en/010_basic-design/BD-001-production-order-create-edit.md` | Screen items, validation V-01–V-08, events E-01–E-09, flows | Item numbers (n) below are BD-001 §3 numbers |
| 2 | `docs/en/database/0002-production-order-schema.md` (DB-002) | Tables, constraints, counter upsert, `xmin`, privileges | |
| 3 | `docs/en/020_detailed-design/DD-001-API-production-orders.md` (DD-001-API) | Endpoint request/response/error catalog | Companion |
| 3a | `docs/en/020_detailed-design/DD-001-FN-production-order-service.md` (DD-001-FN) | Shared service/clock/issuer method design | Companion |
| 3b | `docs/en/020_detailed-design/DD-001-SPD-production-order-create-edit.md` (DD-001-SPD) | Per-component processing flows | Companion |
| 4 | `docs/en/architecture/0001-backend-layered-structure.md` | Layer placement | |
| 5 | `docs/en/architecture/0002-auth-rbac-foundation.md` | Cookie auth, fallback policy, CSRF (DEC-020) | |
| 6 | `work-items/WI-002/brief.md`, `decisions.md` | REQs, DEC-001–DEC-024 | |

### Referenced by

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| 1 | DD-001-API, DD-001-FN, DD-001-SPD | Companions; each lists DD-001 as its parent | |
| 2 | `work-items/WI-002/evidence.md` | Traceability | |

### Component / file organization

**X-1. Path structure**

| No | Path / namespace | Purpose | Notes |
| --- | --- | --- | --- |
| 1 | `src/backend/ProductionManagementAI.Domain/ProductionOrders/` | `ProductionOrder`, `ProductionOrderStatus`, `Product`, `DomainRuleViolation` | No external dependencies |
| 2 | `src/backend/ProductionManagementAI.Application/ProductionOrders/` | `ProductionOrderService`, request/response DTOs, `IPlantClock`, `IOrderNumberIssuer`, `IProductionOrderRepository`, `ProductionOrderTelemetry` | |
| 3 | `src/backend/ProductionManagementAI.Infrastructure/ProductionOrders/` | EF configuration, repository, `OrderNumberIssuer`, `PlantClock`; new migration `AddProductionOrders` | |
| 4 | `src/backend/ProductionManagementAI.Api/Controllers/` | `ProductionOrdersController`, `ProductsController` | |
| 5 | `src/frontend/src/features/production-orders/` | `ProductionOrderPage.tsx`, `ProductionOrderForm.tsx`, `DiscardChangesDialog.tsx`, `validation.ts`, `messages.ts`, `api.ts`, `types.ts` | |
| 6 | `src/frontend/src/lib/apiClient.ts` | Shared fetch wrapper | New |
| 7 | `tests/backend/ProductionManagementAI.Application.Tests/ProductionOrders/` | Domain + service unit tests | |
| 8 | `tests/integration/ProductionManagementAI.Integration.Tests/ProductionOrders/` | Endpoint tests (Testcontainers Postgres) | |
| 9 | `src/frontend/tests/unit/production-orders/` | Component tests (Vitest + RTL) | |

**X-2. Shared/common components used**

| No | Name | Purpose | Notes |
| --- | --- | --- | --- |
| 1 | `useAuth` / `ProtectedRoute` (WI-001) | Signed-in user and roles; redirect to `/login` | Existing |
| 2 | `apiClient` | `getJson`/`sendJson`: same-origin credentials, `Content-Type: application/json`, parses RFC 9457 bodies into `ApiProblem`, and on 401 clears the auth state and navigates to `/login` | New; the WI-001 auth calls keep their current code |
| 3 | ASP.NET Core `AddProblemDetails()` + `[ApiController]` | Problem Details for all API errors | New registration in `Program.cs` |

**X-3. Feature-level components used**

| No | Name | Purpose | Notes |
| --- | --- | --- | --- |
| 1 | `ProductionOrderForm` | Items 6–16 | Stateless about routing; receives initial values + callbacks |
| 2 | `DiscardChangesDialog` | Items 17–19 | Native `<dialog>` with `showModal()` (built-in focus trap, Escape) |
| 3 | `FieldError` | Inline error text linked by `aria-describedby` | |
| 4 | `MessageBanner` | Item 5, `role="status"` (success) / `role="alert"` (error) | |

**X-4. External APIs used** — see "APIs used" below.

**X-5. Responsive composition** — one component tree with Tailwind responsive classes: the `sm:` breakpoint (640px) switches quantity and due date to two columns, and buttons to inline right-aligned. Below 640px everything is one column, and the buttons are full width with Save above Cancel (DOM order Cancel, Save; `flex-col-reverse` on mobile keeps the tab order logical).

### Companion design documents

| No | Document | Type | Covers |
| --- | --- | --- | --- |
| 1 | `DD-001-API-production-orders.md` | api-design | `GET /api/products`, `POST /api/production-orders`, `GET`/`PUT /api/production-orders/{id}` |
| 2 | `DD-001-FN-production-order-service.md` | function-design | `ProductionOrderService` (list/get/create/update), `ProductionOrderMapper`, `IPlantClock`, `IOrderNumberIssuer` |
| 3 | `DD-001-SPD-production-order-create-edit.md` | screen-processing-design | P-01–P-05 per component: page, form, save-response handling, discard dialog, controller boundary |

### Task / design index

| No | Category | File-level task | Function/process-level task | Item-level task | Confirmed | Issue | Reviewer | Reworked | Date | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | basic-design | BD-001 | SCR-001 all sections | — | yes | — | ThanhTN | yes | 2026-09-18 | revision 4 (V-02 upper bound) |
| 2 | detailed-design | DD-001 | Modules 1–10, P-01–P-05 | Items 4–19 | yes | — | ThanhTN | yes | 2026-09-18 | this document; approved 2026-09-18 |
| 3 | api-design | DD-001-API | 4 endpoints | request/response fields | yes | — | ThanhTN | no | 2026-09-18 | approved 2026-09-18 |
| 5 | function-design | DD-001-FN | 7 methods | — | yes | — | ThanhTN | no | 2026-09-18 | approved 2026-09-18 |
| 6 | screen-processing-design | DD-001-SPD | 5 processing blocks | — | yes | — | ThanhTN | no | 2026-09-18 | approved 2026-09-18 |
| 4 | detailed-design | DB-002 | Tables, counter, `xmin` | — | yes | — | ThanhTN | no | 2026-09-18 | reviewed by user |

## Module design

### 1. `ProductionOrder` (Domain entity)

| Field | Value |
| --- | --- |
| Description | Aggregate for one production order. Owns every invariant that doesn't need I/O: status transitions, the product/quantity lock, and quantity/notes value rules |
| Return type | — (entity) |
| Created by / date | Claude / 2026-09-18 |
| Last modified by / date | — |

Preconditions: none.

Rule type: business invariant.

**Rule references**

| No | Rule / validator | Location | Notes |
| --- | --- | --- | --- |
| 1 | `ProductionOrderStatusExtensions.AllowedNext` | Domain | V-06 |

**Condition / data references**

| No | Key | Source | Notes |
| --- | --- | --- | --- |
| 1 | Saved `Status`, `ProductId`, `Quantity`, `DueDate` | Entity state loaded by the service | V-04 "changed", V-07 |

**Check parameters**

| No | Name | Type | Target field |
| --- | --- | --- | --- |
| 1 | `quantity` | `int` | (9) |
| 2 | `notes` | `string?` | (11) |
| 3 | `newStatus` | `ProductionOrderStatus` | (7) |
| 4 | `productId`, `quantity` vs. saved | `Guid`, `int` | (8)(9) |

**Arguments** (factory and mutator)

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `Guid` | `productId` | `Create` / `Update` |
| 2 | `int` | `quantity` | 1–999,999,999 (V-02, DEC-024) |
| 3 | `DateOnly` | `dueDate` | Date checks against "today" are done in the service (they need `IPlantClock`) |
| 4 | `string?` | `notes` | Trimmed; empty → `null`; ≤ 500 Unicode code points (V-05) |
| 5 | `ProductionOrderStatus` | `status` | `Update` only |
| 6 | `short`, `int` | `orderYear`, `orderSeq` | `Create` only, from `IOrderNumberIssuer` |

**Dependencies**: none.

Processing overview: `ProductionOrder.Create(...)` returns a new `Draft` order. `Update(...)` first checks the lock and the transition, and throws `DomainRuleViolation` with a code (`MSG-E007`, `MSG-E008`) before changing any state. Value-rule failures (quantity, notes) are reported by the static `Validate...` helpers, which return error lists rather than throwing. The service calls them first so all field errors can be returned together.

**Processing flow (`Update`)**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | If `Status != Draft` and (`productId != ProductId` or `quantity != Quantity`) → throw `DomainRuleViolation("MSG-E008")`; nothing changed (DEC-007) | — |
| 2 | If `status != Status` and `status ∉ AllowedNext(Status)` → throw `DomainRuleViolation("MSG-E007")` | `ProductionOrderStatusExtensions.AllowedNext` |
| 3 | Assign `ProductId`, `Quantity` (only possible while `Draft`), `DueDate`, `Notes`, `Status` | — |
| 4 | Set `UpdatedAtUtc` = the given UTC time | — |

**Return value**: none — state change or exception.

### 2. `ProductionOrderStatus` (Domain enum)

| Field | Value |
| --- | --- |
| Description | `Draft`, `InProgress`, `Completed`, `Cancelled`, plus the `AllowedNext()` extension method in `ProductionOrderStatusExtensions` (C# enums can't hold methods), whose table is: Draft → {InProgress, Cancelled}; InProgress → {Completed, Cancelled}; Completed, Cancelled → {} |
| Return type | `IReadOnlySet<ProductionOrderStatus>` |

Not applicable — a plain lookup table, not a rule/validator with its own fields. Persisted as a string via EF value conversion (DEC-015). The API returns `allowedNextStatuses` so the frontend never duplicates the table (M-02).

### 3–5. `ProductionOrderService`, `IPlantClock`, `IOrderNumberIssuer` (shared)

These modules will be reused by Screen B, so per `ai/templates/detailed-design.md` they are designed in the function-design companion, **DD-001-FN** (`DD-001-FN-production-order-service.md`). That document covers the methods, arguments, the fixed check order, telemetry points and value mapping. It is not repeated here.

### 6. Controllers (Api)

| Field | Value |
| --- | --- |
| Description | `ProductionOrdersController` (`api/production-orders`) and `ProductsController` (`api/products`), both `[Authorize(Policy = "ProductionOrderEditor")]`, where the policy is `RequireRole("Admin", "Operator")` (DEC-001). Map `Result` to 200/201/400/404/409/422 Problem Details. `[Consumes("application/json")]` on POST/PUT (DEC-020) |
| Return type | `ActionResult<T>` |

Not applicable — plain handler. Route constraint `{id:guid}`, so a non-GUID id returns 404 without reaching the service.

### 7. `ProductionOrderPage` (frontend)

| Field | Value |
| --- | --- |
| Description | Route component for `/production-orders/new` and `/production-orders/:id`. Decides the mode, runs P-01, holds page state (`loading` \| `ready` \| `notFound` \| `forbidden` \| `loadError`), sets `document.title` (0-2-1), shows the banner, and renders `ProductionOrderForm` |
| Return type | JSX |

Not applicable — plain component. Role check (UX only; the server is authoritative): if `user.roles` includes neither `Admin` nor `Operator`, render the forbidden panel (MSG-E012) without calling the API.

### 8. `ProductionOrderForm` (frontend)

| Field | Value |
| --- | --- |
| Description | Controlled form for items 6–16. Keeps `initialValues` (a snapshot from load or save) and `values`; `isDirty = !equal(values, initialValues)` (with notes compared after trimming). Runs field validators on blur (E-02) and all of them on Save (E-04) |
| Return type | JSX |

Rule type: format check (client-side, advisory; mirrors V-01–V-05 in `validation.ts`, same message IDs).

**Check parameters**

| No | Name | Type | Target field |
| --- | --- | --- | --- |
| 1 | `productId` | string | (8) non-empty |
| 2 | `quantity` | string | (9) `^[0-9]{1,9}$` and ≥ 1 |
| 3 | `dueDate` | `YYYY-MM-DD` | (10) non-empty; ≥ browser-local today when creating or changed (DEC-021) |
| 4 | `notes` | string | (11) `[...notes].length ≤ 500` |

Not applicable for V-06/V-07/V-08: the UI prevents V-06 and V-07 (restricted options, read-only fields), and V-08 is server-only.

### 9. `DiscardChangesDialog` (frontend)

| Field | Value |
| --- | --- |
| Description | Items 17–19. Native `<dialog>` opened with `showModal()`; initial focus on **Keep editing** (`autofocus`); Escape/`cancel` event = Keep editing; on close, focus returns to the Cancel button |
| Return type | JSX |

Not applicable — plain component.

### 10. `apiClient` (frontend)

| Field | Value |
| --- | --- |
| Description | `getJson<T>(url)`, `sendJson<T>(method, url, body)`. Always sends `credentials: 'same-origin'`, plus `Content-Type: application/json` on bodies. Non-2xx → throws `ApiError { status, problem?: ProblemDetails }`. 401 → calls the auth context's `onUnauthorized` (clears the user; `ProtectedRoute` then redirects to `/login`) |
| Return type | `Promise<T>` |

Not applicable — plain module.

## Screen layout and mockup

Refines the BD-001 §1 wireframe. Changes from BD: the timestamps (13)(14) moved to a footer line inside the card; the locked-field hint text was added; the discard dialog (17–19) is drawn.

```
+----------------------------------------------------------------------+
| ProductionManagementAI                          Taro Yamada · Sign out |  (1)(2)
+----------------------------------------------------------------------+
| Home / Production orders / PO-2026-00042                             |  (3)
| Production order PO-2026-00042                                       |  (4)
| [✓ Production order PO-2026-00042 saved.                         ]   |  (5)
| +------------------------------------------------------------------+ |
| | Order number      PO-2026-00042                                  | |  (6)
| | Status            [In progress        v]                         | |  (7)
| | Product *         [P-1004 — Drive shaft v]  (read-only)          | |  (8)
| |                   Locked after the order leaves Draft.           | |
| | Quantity *        [ 250      ]      Due date *   [2026-10-01   ] | |  (9)(10)
| | Notes             [                                            ] | |  (11)
| |                   [                                            ] | |
| |                                                         42/500   | |  (12)
| | ---------------------------------------------------------------- | |
| | Created 2026-09-18 10:02 · Last updated 2026-09-18 14:31         | |  (13)(14)
| +------------------------------------------------------------------+ |
|                                             [ Cancel ]  [  Save  ]   |  (15)(16)
+----------------------------------------------------------------------+

Discard dialog (17–19), centered over a dimmed page:
      +------------------------------------------------+
      | Discard your changes?                          |
      | Your changes to this production order haven't  |
      | been saved.                                    |
      |                   [ Keep editing ] [ Discard ] |
      +------------------------------------------------+
```

Mockup artifact: https://claude.ai/artifact/FEo1RG27UjZ6vxFCUjxoHq (private; the owner shares it from the page's Share menu). Source: [`mockups/DD-001-screen-a-mockup.html`](mockups/DD-001-screen-a-mockup.html). It has 11 static states: create empty, product list open (30 items), validation errors, created → edit (Draft), in progress (locked), completed (terminal, overdue), stale save (409), discard dialog, not found, SP create, SP locked.

| Region | Contains (field/control) | Notes |
| --- | --- | --- |
| Header | (1)(2) | Shared app header (new small `AppHeader` component, also used on the home page) |
| Page head | (3)(4)(5) | Breadcrumb collapses to "‹ Home" below 640px |
| Form card | (6)–(14) | `max-w-3xl` centered, `rounded-lg border border-gray-200 p-6` (same vocabulary as `LoginPage`) |
| Actions | (15)(16) | Right-aligned ≥ 640px; full width, stacked, Save first < 640px |
| Dialog | (17)–(19) | `<dialog>` with `::backdrop` dimming |

Visual vocabulary follows the existing `LoginPage.tsx`: gray-900 primary button, `border-gray-300` inputs, `text-red-600` errors. Additions: `bg-gray-50` read-only fields with a lock hint, a green success banner (`bg-green-50 text-green-800`) and a red error banner (`bg-red-50 text-red-800`). Focus rings are `focus-visible:ring-2 ring-gray-900 ring-offset-2`, and all text/background pairs are ≥ 4.5:1 contrast.

## Screen item definition

| Field | Type | Required | Validation rule | Source (BD ref) |
| --- | --- | --- | --- | --- |
| (6) `orderNumber` | string, response only | — | — | §3 item 6 |
| (7) `status` | enum select; label in create mode | edit: yes | Options = current + `allowedNextStatuses` (from API); select disabled when that list is empty | §3 item 7, M-01, M-02, V-06 |
| (8) `productId` | GUID string, `<select>` of 30 products ordered by SKU, label `{sku} — {name}` | yes | Non-empty (MSG-E001); server: exists (MSG-E002). `disabled`-looking but focusable read-only when not Draft (`aria-readonly`, `aria-describedby` → lock hint) | §3 item 8, V-01, V-07, DEC-022 |
| (9) `quantity` | `<input type="text" inputmode="numeric" maxlength="9">` | yes | `^[0-9]+$`, 1–999,999,999 (MSG-E003 / MSG-E010); read-only when not Draft | §3 item 9, V-02, DEC-024 |
| (10) `dueDate` | `<input type="date">` | yes | Non-empty (MSG-E004); ≥ today on create or when changed (MSG-E005). Client uses the browser-local date; the server uses the plant date (DEC-021) | §3 item 10, V-03, V-04 |
| (11) `notes` | `<textarea rows="4">` | no | ≤ 500 code points (MSG-E006); no `maxlength` attribute (it counts UTF-16 units), so the counter turns red and blocks Save instead | §3 item 11, V-05 |
| (12) counter | text | — | `{n}/500`, red at > 500, `aria-live="polite"` announced only at ≥ 450 | §3 item 12 |
| (13)(14) `createdAt`, `updatedAt` | ISO UTC → `Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' })` | — | — | §3 items 13–14, M-04 |
| (hidden) `version` | uint | update: yes | Echoed back on PUT | V-08 |

Message catalog (`messages.ts`; the server returns the same IDs as `code`):

| ID | Text |
| --- | --- |
| MSG-E001 | Select a product. |
| MSG-E002 | The selected product no longer exists. |
| MSG-E003 | Enter a whole number of 1 or more. |
| MSG-E004 | Enter a due date. |
| MSG-E005 | Due date can't be in the past. |
| MSG-E006 | Notes can't exceed 500 characters. |
| MSG-E007 | This status change isn't allowed. |
| MSG-E008 | Product and quantity can't be changed after the order leaves Draft. |
| MSG-E009 | This order was changed by someone else. Reload to see the latest version. |
| MSG-E010 | Quantity can't exceed 999,999,999. |
| MSG-E011 | This production order doesn't exist. |
| MSG-E012 | You don't have permission to manage production orders. |
| MSG-E013 | Something went wrong. Try again. |
| MSG-E014 | No products available. |
| MSG-I001 | Production order {orderNumber} created. |
| MSG-I002 | Production order {orderNumber} saved. |

## Loading / empty / error / success states

| State | Trigger | UI behavior | Data shown |
| --- | --- | --- | --- |
| Loading | P-01 in flight | Header, heading placeholder and a skeleton card (`aria-busy="true"`); no form controls | — |
| Ready — create | Products loaded | Empty form, status label "Draft", order number "Assigned on save" | Product options |
| Ready — edit (Draft) | Order + products loaded, status Draft | All inputs editable; status options Draft/In progress/Cancelled | Saved values |
| Ready — edit (locked) | Status InProgress/Completed/Cancelled | Product and quantity read-only with the lock hint; for Completed/Cancelled the status select is disabled too; due date and notes stay editable (DEC-004) | Saved values |
| Empty products | `GET /api/products` returns `[]` | Product select shows MSG-E014 and is disabled; Save shows MSG-E001 | — |
| Saving | Save pressed, request in flight | Save disabled with label "Saving…"; inputs stay as they are; Cancel still enabled | Entered values |
| Validation error | Client or 400 response | Inline errors (`FieldError`, `aria-invalid`), focus to the first invalid field | Entered values kept |
| Rule / conflict error | 422 (MSG-E007/E008) or 409 (MSG-E009) | Error banner (`role="alert"`); for 409 the banner has a **Reload** button (re-runs P-01 and discards local edits) | Entered values kept |
| Not found | 404 on load | Panel MSG-E011 with a "Back to production orders" link; no form | — |
| Forbidden | Missing role (client check) or 403 | Panel MSG-E012 with a "Back to production orders" link | — |
| Load error | Network/5xx on load | Panel MSG-E013 with **Try again** (re-runs P-01) | — |
| Success | 201 / 200 | Banner MSG-I001 / MSG-I002 (`role="status"`); form reset to the saved values, so `isDirty = false` | Saved values |

## Processing and state transitions

### State transitions

| From state | Event | To state | Side effect |
| --- | --- | --- | --- |
| (none) | Create (P-02) | Draft | Row inserted; counter incremented; `orders.created` metric |
| Draft | Update with status InProgress | InProgress | Product/quantity become locked |
| Draft | Update with status Cancelled | Cancelled | Terminal |
| InProgress | Update with status Completed | Completed | Terminal |
| InProgress | Update with status Cancelled | Cancelled | Terminal |
| any | Update with the same status | same | Other editable fields saved |
| any other pair | Update | unchanged | 422 MSG-E007 |

### Processing flows

The step-by-step processing is in the screen-processing companion, **DD-001-SPD** (`DD-001-SPD-production-order-create-edit.md`), with one block per component:

| Flow | Where | Events |
| --- | --- | --- |
| P-01 Load | DD-001-SPD §1 `ProductionOrderPage` | E-01 |
| P-02 Create / P-03 Update (client) | DD-001-SPD §2 `ProductionOrderForm` | E-02–E-06, E-08 |
| P-02 / P-03 (server) | DD-001-FN §3 `CreateAsync`, §4 `UpdateAsync`; request boundary in DD-001-SPD §5 | — |
| P-04 Cancel | DD-001-SPD §4 `DiscardChangesDialog` | E-07, E-09 |
| P-05 Save-response handling | DD-001-SPD §3 | E-06 |

## APIs used

| Endpoint | Method | Purpose | Design doc |
| --- | --- | --- | --- |
| `/api/products` | GET | Product options (FN-004) | DD-001-API §1 |
| `/api/production-orders` | POST | Create (FN-001) | DD-001-API §2 |
| `/api/production-orders/{id}` | GET | Load (FN-003) | DD-001-API §3 |
| `/api/production-orders/{id}` | PUT | Update (FN-002) | DD-001-API §4 |

## Database and transaction mapping

| Operation | Table(s) | Transaction boundary | Concurrency handling |
| --- | --- | --- | --- |
| List products | `products` (`id, sku, name`) | none; `AsNoTracking` | not applicable |
| Load order | `production_orders` only (no join; the client resolves the product label from the product list) | none; `AsNoTracking` for GET | not applicable |
| Create | `production_order_number_counters`, `production_orders` | Explicit transaction: counter upsert + insert, then commit | Counter row lock; unique `(order_year, order_seq)` backstop |
| Update | `production_orders` (+ `products` existence query when `productId` changes) | Single `SaveChanges` (implicit transaction) | `xmin` token (DEC-014): stale → 409 |

Queries select only the columns used (projection to DTOs). The runtime login is `pmai_app`, with the grants in DB-002 (DEC-016).

## Exception handling

| Failure | Retry policy | Timeout | User-facing error | Logging |
| --- | --- | --- | --- | --- |
| Validation (400) | none | — | Inline field errors | Information: `ProductionOrderValidationFailed` with order id (if any), user id, failed field names (not values) |
| Business rule (422) | none | — | Banner MSG-E007/E008 | Information: `ProductionOrderRuleViolated` with code, from/to status |
| Stale save (409) | none (user reloads) | — | Banner MSG-E009 + Reload | Information: `ProductionOrderConcurrencyConflict` |
| Not found (404) | none | — | Panel MSG-E011 | Debug |
| Counter overflow / DB error / unhandled | none automatic | Npgsql command timeout 30s (default) | Banner or panel MSG-E013 | Error with exception (server-side only); response is generic Problem Details with no stack trace or exception text (`ai/rules/backend.md`) |
| Transient connection failure | EF Core `EnableRetryOnFailure` is **not** enabled: the create uses an explicit transaction, which would need an execution-strategy wrapper; not worth it for the demo | — | MSG-E013; user retries | Error |
| Frontend network failure | none automatic; user presses Save / Try again | browser default | MSG-E013 | `console.error` only in development |

Never logged: notes text, full request/response bodies, cookies.

## Observability (OpenTelemetry)

Per `ai/rules/backend.md` and the design-consistency checklist. The backend has no OpenTelemetry setup yet, so plan revision 2 adds `OpenTelemetry.Extensions.Hosting` with ASP.NET Core and Npgsql instrumentation and the OTLP exporter. Npgsql spans cover the database statements; EF Core's own instrumentation package is still prerelease and the backend makes no outgoing HttpClient calls, so neither is added. The exporter is enabled only when `OTEL_EXPORTER_OTLP_ENDPOINT` is set; otherwise telemetry is collected but not exported.

| Signal | Name | Attributes / tags | Emitted by |
| --- | --- | --- | --- |
| Span (auto) | `POST api/production-orders`, `PUT api/production-orders/{id}`, `GET ...` | `http.route`, `http.response.status_code` | ASP.NET Core instrumentation |
| Span (auto) | DB statements | `db.system=postgresql` (no parameter values) | Npgsql instrumentation |
| Span | `ProductionOrder.Create` | `production_order.id`, `production_order.number`, `outcome` | `ProductionOrderService` (ActivitySource `ProductionManagementAI.ProductionOrders`) |
| Span | `ProductionOrder.Update` | `production_order.id`, `status.from`, `status.to`, `outcome` | same |
| Counter | `pmai.production_orders.created` | `outcome` = `success` \| `validation_failed` \| `error` | Meter `ProductionManagementAI.ProductionOrders` |
| Counter | `pmai.production_orders.updated` | `outcome` = `success` \| `validation_failed` \| `rule_violation` \| `conflict` \| `not_found` \| `error` | same |
| Counter | `pmai.production_orders.status_transitions` | `from`, `to` | same, on successful status change |
| Histogram | `http.server.request.duration` (auto) | route, status | ASP.NET Core |

The user id is not a span attribute (it's personal data). Logs correlate with spans through the trace id.

## Configuration

| Key | Default | Notes |
| --- | --- | --- |
| `Plant:TimeZone` | `Asia/Tokyo` | IANA ID; validated at startup (DEC-017) |
| `ConnectionStrings:DefaultConnection` | — | Runtime login `pmai_app` (DEC-016); secret from environment |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | unset | Optional |

## Accessibility (WCAG 2.2 AA)

- Every input has a `<label for>`. Required inputs have `aria-required="true"` and a visible `*` with an "(required)" legend.
- Errors: `aria-invalid="true"` + `aria-describedby` → the `FieldError` id. On a failed save, focus moves to the first invalid field.
- Read-only locked fields: native `readonly` on the quantity input. The product `<select>` can't be `readonly`, so it's rendered as a read-only text input showing the label, with `aria-describedby` → the lock hint. It stays focusable, unlike `disabled`.
- Banners: success `role="status"`, errors `role="alert"`. Panels get focus on render (`tabIndex=-1`).
- Dialog: native `<dialog>` + `showModal()` (focus trap, Escape). It is labelled by its title via `aria-labelledby`.
- Focus-visible rings on every control; target size ≥ 24×24 CSS px (2.5.8); contrast ≥ 4.5:1 for text and ≥ 3:1 for control borders.
- Keyboard: the natural tab order matches the visual order; native date input and select.

## Test viewpoints and unresolved decisions

Level: U = unit (xUnit Domain/Application, or Vitest+RTL for the frontend), I = integration (`WebApplicationFactory` + Testcontainers Postgres), E = E2E (Playwright, DEC-025). Test-plan IDs refer to `work-items/WI-002/test-plan.md` (TP-002).

| Scenario | Precondition | Expected result | Test-plan ID |
| --- | --- | --- | --- |
| Create valid order (REQ-010) — I, E | Admin signed in, products seeded | 201, `status=Draft`, `orderNumber` matches `^PO-\d{4}-\d{5}$`, redirect to edit + MSG-I001 | TC-001 |
| First order of a year gets `00001`; next gets `00002` (DEC-012/013) — I | Empty counter; plant clock faked to 2026 then 2027 | `PO-2026-00001`, `PO-2026-00002`, `PO-2027-00001` | TC-002 |
| Concurrent creates get distinct numbers — I | 20 parallel POSTs | 20 distinct sequential numbers, no gaps | TC-002 |
| Year boundary uses the plant timezone (DEC-017) — U | `TimeProvider` at 2026-12-31T15:30Z (= 2027-01-01 00:30 JST) | Year 2027; "today" 2027-01-01 | TC-003 |
| Edit Draft order fields (REQ-011) — I | Draft order | 200, values saved, `updatedAt` changed | TC-004 |
| Unknown id (REQ-011) — I, U(fe) | — | 404 MSG-E011; not-found panel | TC-005 |
| Stale save (DEC-010) — I | Two clients load version v; A saves | B's PUT → 409 MSG-E009; row keeps A's values | TC-006 |
| Unauthenticated (REQ-012) — I, U(fe) | No cookie | 401 on all 4 endpoints; UI redirects to `/login` | TC-007 |
| No role (REQ-012) — I, U(fe) | User without Admin/Operator | 403 on all 4 endpoints; forbidden panel without API call | TC-008 |
| Operator allowed (DEC-001) — I | Operator signed in | 200/201 | TC-009 |
| Quantity rules (REQ-013) — U, I | — | `1`, `250`, `999999999` pass; empty, `0`, `-5`, `2.5`, `abc`, `1000000000` → MSG-E003/E010 | TC-010 |
| Due date rules (REQ-014, DEC-009) — U, I | Plant today = D | D and D+1 pass; D-1 → MSG-E005 on create; overdue order saved without changing the due date → 200; changing it to D-1 → MSG-E005 | TC-011 |
| Product rules (REQ-015) — I | — | Missing → MSG-E001; random GUID → MSG-E002 | TC-012 |
| Notes rules (REQ-016) — U, I | — | Empty → `null`; 500 chars (incl. emoji counted as 1) pass; 501 → MSG-E006 | TC-013 |
| Each allowed transition (REQ-017) — U, I | Order in the source state | 200, new status; `allowedNextStatuses` updated | TC-014 |
| Disallowed transitions (REQ-017) — U, I | e.g. Draft→Completed, Completed→InProgress, Cancelled→Draft | 422 MSG-E007, status unchanged | TC-015 |
| Status options per state (M-02) — U(fe) | Each status | Select options match; disabled for terminal states | TC-016 |
| Locked-field change rejected whole (REQ-018, DEC-007) — U, I | InProgress order; PUT changes quantity + notes | 422 MSG-E008; notes not saved | TC-017 |
| Locked fields read-only in UI (REQ-018) — U(fe) | InProgress order | Product/quantity read-only, focusable, lock hint | TC-018 |
| Cancel without changes (REQ-019) — U(fe), E | Pristine form | Navigates home, no dialog | TC-019 |
| Cancel with changes → Discard / Keep editing / Escape (REQ-019) — U(fe), E | Edited form | Dialog; Discard → home; Keep editing/Escape → values kept, focus on Cancel | TC-020 |
| Non-JSON body (DEC-020) — I | POST `text/plain` / form-encoded | 415, nothing created | TC-021 |
| Problem Details shape — I | Any 4xx/5xx | `application/problem+json`, `type`, `title`, `status`, `code`; no stack trace | TC-022 |
| Runtime login privileges (DEC-016) — I | App connected as `pmai_app` | All flows work; `DELETE FROM production_orders` → permission denied | TC-023 |
| Telemetry emitted — I | In-memory exporter | Create/Update spans + counters with the expected tags | TC-024 |
| Accessibility — U(fe) + manual | — | Labels, `aria-invalid`/`describedby`, focus on first error, dialog focus; automated axe check (vitest-axe + @axe-core/playwright, DEC-026) has no violations | TC-025 |

Unresolved decisions: none. DD-level technical decisions DEC-021–DEC-024 are recorded in `work-items/WI-002/decisions.md`.
