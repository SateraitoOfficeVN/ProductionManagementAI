<!-- Based on ai/templates/DD/screen-processing-design.md (revision at commit e7e0d36). -->

# Production Order Create/Edit — Screen Processing Design (画面処理設計)

DD-001-SPD — elaborates DD-001, implements BD-001 (SCR-001), requirements REQ-010–REQ-019.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | DD-001-SPD |
| System name | ProductionManagementAI |
| Subsystem name | Production orders |
| Work item | WI-002 |
| Created by | Claude (for ThanhTN) |
| Created date | 2026-09-18 |
| Last updated by | Claude (for ThanhTN) |
| Last updated date | 2026-09-18 |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | 2026-09-18 | Claude (for ThanhTN) | Initial creation. Moves DD-001 revision 1's processing flows P-01–P-05 into this document and splits them per component |

## Overview and process list

| Field | Value |
| --- | --- |
| Screen / file name | `src/frontend/src/features/production-orders/ProductionOrderPage.tsx` and the components it composes; the server side of each request is in `ProductionOrdersController` |
| Overview | How SCR-001 loads, validates, saves, cancels and reacts to every API outcome, one block per component |

| No | Process name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `ProductionOrderPage` — P-01 Load | Role gate, parallel fetch, page state, title, flash message | E-01 |
| 2 | `ProductionOrderForm` — P-02 Create / P-03 Update (client side) | Blur/submit validation, request build, response handling | E-02–E-06, E-08 |
| 3 | `ProductionOrderForm` — P-05 Save-response handling | Mapping of each HTTP outcome to UI | E-06 |
| 4 | `DiscardChangesDialog` — P-04 Cancel | Dirty check and confirmation | E-07, E-09 |
| 5 | `ProductionOrdersController` / `ProductsController` — request boundary | Auth, content type, dispatch to the service, result → HTTP | Server steps are in DD-001-FN |

### Reference documents

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| 1 | DD-001 | Items, states, message catalog, module list | Parent document |
| 2 | DD-001-API | Endpoint contracts called below | |
| 3 | DD-001-FN | Server-side steps (`CreateAsync`, `UpdateAsync`, …) | |
| 4 | BD-001 | Events E-01–E-09, validation V-01–V-08 | |

## Processing design

### 1. ProductionOrderPage — P-01 Load

| Field | Value |
| --- | --- |
| Detail | Route component for `/production-orders/new` and `/production-orders/:id` |
| Created by / date | Claude / 2026-09-18 |
| Last modified by / date | — |

Processing overview: decide the mode from the route, apply the client-side role gate, fetch the data, then either render the form or a full-page panel. Runs on mount and on **Reload** / **Try again**.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `useAuth` | Current user and roles | WI-001 |
| 2 | `api.ts` (`listProducts`, `getOrder`) | Typed wrappers over `apiClient` | DD-001 module 10 |
| 3 | `ProductionOrderForm`, `MessageBanner` | Rendered when ready | |
| 4 | `useLocation` / `useNavigate` | Flash message from the create redirect | react-router |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Mode = `create` if the route is `/new`, else `edit` with `id` | — | — | — |
| 2 | Role gate on `user.roles` | Neither `Admin` nor `Operator` → stop | — | `forbidden` panel (MSG-E012), no API call |
| 3 | Set state `loading`, `document.title` to the mode's title (the order number is added after load) | — | — | Skeleton card, `aria-busy="true"` |
| 4 | Fetch in parallel: products, and in edit mode the order | — | `GET /api/products`; `GET /api/production-orders/{id}` | — |
| 5 | Order response | 404 → `notFound`; 401 → apiClient redirects to `/login`; 403 → `forbidden`; network/5xx → `loadError` | apiClient | Not-found (MSG-E011) / forbidden / load-error (MSG-E013 + **Try again**) panel, focus moved to the panel |
| 6 | Products response | Failure → `loadError`; empty list → continue with no options | apiClient | Product select shows MSG-E014 and is disabled |
| 7 | Build `initialValues`: from the order (edit) or empty with status `Draft` (create); set title `Production order {orderNumber} — ProductionManagementAI` in edit mode | — | — | `ready` |
| 8 | If `location.state.flash` is set (after create), show that message and clear the state with `navigate(location.pathname, { replace: true, state: null })` | No flash → skip | — | Success banner MSG-I001 |

### 2. ProductionOrderForm — P-02 Create / P-03 Update (client side)

| Field | Value |
| --- | --- |
| Detail | Controlled form for items 6–16, owning values, errors, dirty state and the save request |
| Created by / date | Claude / 2026-09-18 |
| Last modified by / date | — |

Processing overview: validate on blur for early feedback, validate everything on Save, then send one request. The server repeats every check (DD-001-FN), so the client checks are advisory. After a successful update the form re-bases on the server response, so `isDirty` becomes false and the lock/status options reflect the new status.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `validation.ts` | `validateProduct`, `validateQuantity`, `validateDueDate(value, initial, mode)`, `validateNotes` → message ID or null | Mirrors V-01–V-05 |
| 2 | `messages.ts` | Message ID → text | DD-001 catalog |
| 3 | `api.ts` (`createOrder`, `updateOrder`) | POST/PUT wrappers | DD-001-API §2, §4 |
| 4 | `FieldError`, `MessageBanner` | Error display | |

**Processing flow — blur (E-02) and status change (E-08)**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Field loses focus | Read-only field → skip | — | — |
| 2 | Run that field's validator | Due date: check "≥ browser-local today" only in create mode or when the value differs from `initialValues.dueDate` (DEC-009, DEC-021) | `validation.ts` | Inline error with `aria-invalid`, or the error cleared |
| 3 | Notes input (E-03) | — | — | Counter `{n}/500` (code points); red above 500 |
| 4 | Status change | Options are only `[current, ...allowedNextStatuses]` | — | Value changes locally; nothing saved until Save |

**Processing flow — Save (E-04)**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Run all validators (read-only fields skipped) | Any error → show all, focus the first invalid field in DOM order, stop | `validation.ts` | Validation-error state |
| 2 | Build the body: `productId`, `quantity` (as number), `dueDate`, `notes` (trimmed, `""` → `null`); in edit mode also `status` and `version` from `initialValues` | — | — | — |
| 3 | Set `saving`: Save disabled, label "Saving…" | — | — | Saving state |
| 4 | Send | Create → POST; Edit → PUT | `createOrder` / `updateOrder` | — |
| 5 | Create success (201) | — | `navigate('/production-orders/{id}', { replace: true, state: { flash: 'MSG-I001' } })` | Page re-runs P-01 in edit mode; `replace` keeps Back from returning to an empty create form |
| 6 | Update success (200) | — | — | `initialValues = values = response`; banner MSG-I002 (`role="status"`); lock and status options recomputed from `isProductQuantityEditable` / `allowedNextStatuses` |
| 7 | Failure | — | Process 3 (P-05) | — |
| 8 | Clear `saving` | always | — | Save enabled again |

### 3. ProductionOrderForm — P-05 Save-response handling (E-06)

| Field | Value |
| --- | --- |
| Detail | Maps every non-success save outcome to UI |
| Created by / date | Claude / 2026-09-18 |
| Last modified by / date | — |

Processing overview: an `ApiError` from apiClient carries the status and the parsed Problem Details. Entered values are always kept, except after a 401 (session gone) or when the user presses **Reload** after a 409.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `apiClient` `ApiError` | `{ status, problem? }` | DD-001 module 10 |
| 2 | `messages.ts` | IDs → text | |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | 400 `VALIDATION` | For each key in `problem.errors` → that field's message | — | Inline errors, focus first invalid |
| 2 | 401 | — | apiClient `onUnauthorized` | Auth cleared → `ProtectedRoute` → `/login` (unsaved values lost; accepted) |
| 3 | 403 | — | — | Forbidden panel (MSG-E012) |
| 4 | 404 `MSG-E011` | — | — | Not-found panel |
| 5 | 409 `MSG-E009` | — | — | Error banner with **Reload** → P-01 (discards local edits) |
| 6 | 422 `MSG-E007` / `MSG-E008` | — | — | Error banner with that message |
| 7 | 415, 5xx, network error | — | — | Error banner MSG-E013; Save enabled again |

### 4. DiscardChangesDialog — P-04 Cancel (E-07, E-09)

| Field | Value |
| --- | --- |
| Detail | Confirmation before discarding unsaved changes (REQ-019, DEC-018) |
| Created by / date | Claude / 2026-09-18 |
| Last modified by / date | — |

Processing overview: Cancel checks `isDirty` (values compared with `initialValues`, notes trimmed). Only a dirty form opens the dialog. Covers the Cancel button only, not browser Back or tab close (DEC-018).

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | Native `<dialog>` + `showModal()` | Focus trap, Escape, `::backdrop` | No library |
| 2 | `useNavigate` | Leave to `/production-orders` | SCR-002, since WI-003 |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Cancel clicked | `isDirty` false → `navigate('/')` | — | Home |
| 2 | Open the dialog | `isDirty` true | `dialog.showModal()` | Dialog; focus on **Keep editing** |
| 3 | **Discard** | — | `dialog.close()`, `navigate('/')` | Home; nothing saved |
| 4 | **Keep editing** or Escape (`cancel` event) | — | `dialog.close()` | Form with all values; focus returns to Cancel |

### 5. ProductionOrdersController / ProductsController — request boundary

| Field | Value |
| --- | --- |
| Detail | HTTP entry points for DD-001-API; no business logic |
| Created by / date | Claude / 2026-09-18 |
| Last modified by / date | — |

Processing overview: the ASP.NET Core pipeline authenticates and authorizes, then checks the content type. The action calls one service method and maps its `Result` to HTTP. All business checks happen in DD-001-FN.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | Policy `ProductionOrderEditor` | `RequireRole("Admin", "Operator")` | DEC-001 |
| 2 | `[Consumes("application/json")]` | 415 on other content types | DEC-020 |
| 3 | `ProductionOrderService` | Use cases | DD-001-FN |
| 4 | `ProblemResults` helper | `Result` → Problem Details (`type`, `title`, `status`, `code`, `errors`, `traceId`) | DEC-023 |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Authentication (cookie) | None → 401, empty body | ASP.NET Core | — |
| 2 | Authorization (policy) | Missing role → 403, empty body | ASP.NET Core | — |
| 3 | Route match `{id:guid}` | Not a GUID → 404 | routing | — |
| 4 | Content type (POST/PUT) | Not JSON → 415 | `[Consumes]` | — |
| 5 | Model binding | Binding error → 400 with message IDs (custom `InvalidModelStateResponseFactory`) | MVC | — |
| 6 | Call the service method | — | `ListProductsAsync` / `GetAsync` / `CreateAsync` / `UpdateAsync` | — |
| 7 | Map the result | `Ok` → 200 (201 + `Location` for create); `Invalid` → 400; `NotFound` → 404; `Conflict` → 409; `RuleViolation` → 422 | `ProblemResults` | Response |
| 8 | Unhandled exception | — | `AddProblemDetails` + exception handler | 500 MSG-E013, no exception detail |

## Unresolved decisions

None. Related decisions: DEC-007, DEC-008, DEC-009, DEC-018, DEC-020, DEC-021, DEC-023 in `work-items/WI-002/decisions.md`.
