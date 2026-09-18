# Production Order Create/Edit (Screen A) — Requirements Traceability & Evidence

As of branch `feature/WI-002-production-order-screen` (commits `5c40eaa`…HEAD, see PR), 2026-09-18.

## Traceability matrix

| Requirement ID | Requirement | Design artifact | Code / PR | Test case ID | Status |
| --- | --- | --- | --- | --- | --- |
| REQ-010 | Create order | BD-001 business flow, §3, §6 E-04/E-05; DB-002 counter table, transactions; DD-001 P-02; DD-001-API §2; DD-001-FN §3; DD-001-SPD §2 | `ProductionOrderService.CreateAsync`, `OrderNumberIssuer`, `ProductionOrdersController.Create`, `ProductionOrderForm` | TC-001, TC-002 | verified |
| REQ-011 | Edit order | BD-001 business flow, 0-3, §5 V-08, §6 E-01/E-05; DB-002 `xmin` concurrency; DD-001 P-01/P-03; DD-001-API §3–4; DD-001-FN §2, §4; DD-001-SPD §1–3 | `ProductionOrderService.GetAsync/UpdateAsync`, `ProductionOrderPage`, `ProductionOrderForm` | TC-004, TC-005, TC-006 | verified |
| REQ-012 | Admin/Operator only | BD-001 0-1, actions, exception flows; DD-001 module 6, P-01 step 1; DD-001-API common auth | `ProductionOrderEditor` policy, controllers, `ProductionOrderPage` role gate, `apiClient` 401 handler | TC-007, TC-008, TC-009, TC-027 | verified |
| REQ-013 | Quantity positive integer | BD-001 §5 V-02; DB-002 `ck_production_orders_quantity_positive`; DD-001 item (9), MSG-E003/E010 | `ProductionOrder.ValidateQuantity`, `validation.ts`, `ck_production_orders_quantity_positive` | TC-010 | verified |
| REQ-014 | Due date ≥ today | BD-001 §5 V-03, V-04 (DEC-009, DEC-011); DD-001 item (10), P-02/P-03, `IPlantClock` | `ProductionOrderService` due-date checks, `PlantClock`, `validation.ts validateDueDate` | TC-003, TC-011 | verified |
| REQ-015 | Product exists | BD-001 §5 V-01; DB-002 `fk_production_orders_products_product_id`; DD-001 item (8); DD-001-API §2 fields | `ProductExistsAsync`, FK `RESTRICT`, product `<select>` | TC-009, TC-012 | verified |
| REQ-016 | Notes ≤ 500 | BD-001 §5 V-05; DB-002 `notes varchar(500)`; DD-001 item (11) | `ProductionOrder.ValidateNotes/NormalizeNotes`, notes counter | TC-013 | verified |
| REQ-017 | Status state machine | BD-001 status diagram, §4 M-02, §5 V-06; DB-002 `ck_production_orders_status`; DD-001 module 2, state transitions | `ProductionOrderStatusExtensions`, `ProductionOrder.Update`, status select | TC-014, TC-015, TC-016 | verified |
| REQ-018 | Product/quantity lock | BD-001 §3, §5 V-07; DD-001 module 1 `Update`; DD-001-FN §4 step 7 | `ProductionOrder.Update` lock check, read-only fields | TC-017, TC-018 | verified |
| REQ-019 | Confirm discard on Cancel | BD-001 §3 items 17–19, §6 E-07/E-09; DD-001 module 9, P-04; DD-001-SPD §4 | `DiscardChangesDialog`, `ProductionOrderForm.handleCancel` | TC-019, TC-020 | verified |

## Test execution log

| Date | Check | Command | Environment | Result (pass / fail / not run) | Report / log link |
| --- | --- | --- | --- | --- | --- |
| 2026-09-18 | design-consistency checklist (BD-001 scope) | manual review | local | pass for BD-level items; DD/DB/test items not yet applicable (see below) | this file |
| 2026-09-18 | design-consistency checklist (DB-002 scope) | manual review | local | pass — every index justified, DB vs. application-only rules listed with reasons, migration impact stated; DEC-016 (role split) decided 2026-09-18 — runtime login and grant list added | this file |
| 2026-09-18 | design-consistency checklist (DD-001 scope) | manual review | local | pass — see the checklist walk below; mockup published privately | this file |
| 2026-09-18 | Design review | user review of the DD-001 set and mockup | — | pass — approved by the user ("the DD is reviewed and approved") | status.md |
| 2026-09-18 | Unit / integration / E2E | — | — | not run — no code in scope for plan revision 1 | — |
| 2026-09-18 | Backend unit (plan rev. 2) | `dotnet test src/backend/ProductionManagementAI.slnx` | local, .NET SDK 10.0.303 | pass — 49/49 (Application.Tests: 45 new + 4 existing) | final run 03:12Z |
| 2026-09-18 | Backend integration | same command | local, Testcontainers `postgres:17`, Docker 29.7.2; app as `pmai_app` | pass — 38/38 (34 new + 4 existing auth tests, now under the restricted login) | final run 03:12Z |
| 2026-09-18 | Frontend lint / type check / build | `npm run lint`; `npx tsc -b`; `npm run build` (src/frontend) | local, Node 24.21.0 | pass — 0 lint findings, 0 type errors, build OK | final run 03:12Z |
| 2026-09-18 | Frontend unit (incl. vitest-axe) | `npm test` (src/frontend) | local, Vitest 5.0.1 + jsdom | pass — 38/38 (34 new + 4 existing) | final run 03:12Z |
| 2026-09-18 | Migration SQL review | `dotnet ef migrations script InitialIdentitySchema AddProductionOrders` | local | pass — matches DB-002 (plus DEC-029 index); `xmin` not emitted as a column; grants as specified | reviewed in session |
| 2026-09-18 | Compose smoke | `docker compose -f deploy/compose.yaml up -d --build` after `down`/volume wipe, owner migration | local Docker Desktop | pass — `/health` 200, frontend 200, `/api/products` unauthenticated 401 via Nginx; init script created `pmai_app`; seeder ran as `pmai_app`; 30 products seeded | session log |
| 2026-09-18 | Runtime-login privilege check (Compose DB) | `psql -U pmai_app -c "DELETE FROM production_orders"` | local Compose | pass — `permission denied for table production_orders` | session log |
| 2026-09-18 | E2E (Playwright + axe) | `npx playwright test` (tests/e2e) with `E2E_ADMIN_PASSWORD` from deploy/.env | local Compose stack, Chromium (desktop + Pixel 7) | pass — 8/8, first run and final run | final run 03:12Z |
| 2026-09-18 | Dependency vulnerability checks | `dotnet list … package --vulnerable --include-transitive`; `npm audit` (src/frontend, tests/e2e) | local | pass — no vulnerable packages; 0 vulnerabilities | session log |
| 2026-09-18 | CI (GitHub Actions) on PR #2 (harness) | triggered by opening the PR | GitHub | not run — both jobs refused to start: "account is locked due to a billing issue" | https://github.com/thanhtn95/ProductionManagementAI/actions/runs/35300628038 |

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

Design-consistency walk after implementation (plan revision 2):

- DD agrees with code — pass. Implementation-driven doc updates: DD-001 revision 3 and DD-001-FN revision 2 (`AllowedNext()` extension, values returned via `RETURNING`, instrumentation set), DB-002 (DEC-029 index, grants in the migration, login creation baked into the db image).
- API and DB mappings agree, including constraints and errors — pass (integration tests assert status codes, codes and constraint behavior).
- Test scenarios map to design — pass: TC-001–TC-027 in `test-plan.md`, with IDs filled into DD-001's test viewpoints.

Security-review checklist (`ai/checklists/security-review.md`):

- Every new endpoint enforces authN/authZ — pass (policy on both controllers; TC-007/TC-008 cover all 4 endpoints).
- External input validated; no concatenated SQL — pass (service validation; EF Core queries; the counter upsert uses interpolated → parameterized SQL). Test-only setup SQL uses constants.
- No credentials in code/config/logs/diff — pass. The runtime password is a required env var with no default; the owner password was removed from `appsettings.Development.json` (use `PGPASSWORD`). Test passwords are constants for throwaway containers, as WI-001 already does.
- New dependencies trusted and vulnerability-checked — pass (OpenTelemetry 1.18.0, Npgsql.OpenTelemetry 10.0.3, Microsoft.Extensions.* 10.x, vitest-axe 0.1.0 → axe-core 4.13.0, Playwright 1.63.0, @axe-core/playwright 4.13.0; no known vulnerabilities).
- New DB role least privilege — pass (DEC-016; TC-023).
- Error responses don't leak — pass (TC-022; overflow 500 has no detail/exception/driver text).
- Sensitive data not logged — pass (field names only; notes never logged; no user id in spans).
- New trust boundary threat-modeled — not applicable: no new boundary (same cookie auth, same origin); CSRF reasoning in DEC-020.

Delivery checklist (`ai/checklists/delivery.md`):

- Approved scope and plan revision identifiable — pass (plan revision 2, approved 2026-09-18).
- Design, code and tests agree with requirements — pass (see above).
- Required checks recorded; not-run checks have reasons — pass (CI: billing lock).
- Review findings and remaining limitations explicit — pass (below and in the PR).
- External operations within authorization — pass (push + PR only, DEC-027; no merge/deploy).
- Status, decisions and evidence support another agent continuing — pass.
- No secrets in diff/evidence/status/decisions/PR — pass (`deploy/.env` is gitignored; values never printed).
- External content treated as data — pass (none consumed beyond package metadata).
- Flaky or skipped checks quarantined with a reason — pass (none flaky; none skipped).
- New dependencies pinned and least-privilege — pass (exact versions; no new CI actions).

## Defects, failures and blockers

| Item | Reason | Blocker | Follow-up |
| --- | --- | --- | --- |
| GitHub Actions CI | Jobs are not started: "account is locked due to a billing issue" (observed on PR #2) | GitHub account billing (user action) | Clear the billing lock, then re-run CI on both PRs |
| `libgssapi_krb5.so.2: cannot open shared object file` in backend container log | Npgsql probes for Kerberos/GSSAPI at startup; the aspnet image doesn't ship it. Harmless (password auth is used; the app starts and works) | none | Optional: silence it later, e.g. by adding `libgssapi-krb5-2` to the image |
| Docker Desktop bind mount of `deploy/db/init` refused for the worktree path | The path wasn't in Docker Desktop's File Sharing list | none — resolved by baking the script into `deploy/docker/db.Dockerfile` | none |

## External references

- PR: https://github.com/thanhtn95/ProductionManagementAI/pull/3 (Screen A); harness PR: https://github.com/thanhtn95/ProductionManagementAI/pull/2
- CI run: https://github.com/thanhtn95/ProductionManagementAI/actions/runs/35302566782 — jobs not started (billing lock). GitGuardian security check: pass on PR #3 and PR #2
- Deployment: not applicable (not authorized)

## Remaining limitations and next action

Implemented and verified locally at all four test levels. Limitations: CI hasn't run (billing lock); E2E runs locally only and on Chromium only; no manual screen-reader pass. Next action: user review of the PR; clear the GitHub billing lock so CI can run; merge only on the user's go-ahead.
