# Product master — Test Plan

## Test plan identifier

TP-006, WI-006, revision 2, 2026-09-30. Design only; no application test has been run for WI-006.

## References

[Brief](brief.md) REQ-049–REQ-060; [004_BD](../../docs/en/010_basic-design/004/004_BD_製品マスタ.md) and [existing-screen BD addendum](../../docs/en/010_basic-design/004/004_BD-EXISTING-SCREENS_製品マスタ影響.md); [004_DB](../../docs/en/database/004/004_DB_製品マスタ.md) and [existing-screen DB addendum](../../docs/en/database/004/004_DB-EXISTING-SCREENS_製品マスタ影響.md); [004_DD](../../docs/en/020_detailed-design/004/004_DD_製品マスタ.md), [API](../../docs/en/020_detailed-design/004/004_DD-API_製品マスタ.md), [FN](../../docs/en/020_detailed-design/004/004_DD-FN_製品マスタ.md), [SPD](../../docs/en/020_detailed-design/004/004_DD-SPD_製品マスタ.md), and [existing-screen DD addendum](../../docs/en/020_detailed-design/004/004_DD-EXISTING-SCREENS_製品マスタ影響.md). All nine design Markdown files have been approved in sequence under plan revision 3.

## Introduction and test items

This plan checks catalog maintenance, retirement and order compatibility, database migration, authorization, concurrency, accessibility, Japanese display, unit locking, exact decimal quantities, and unit-safe dashboard metrics. The implementation revision will add executable tests and record actual results in `evidence.md`.

| Requirement | Test item |
| --- | --- |
| REQ-049 | Catalog read/search and states |
| REQ-050 | Product creation and validation |
| REQ-051 | Product edit and stale-write protection |
| REQ-052 | Retirement and historical orders |
| REQ-053 | Seed/migration preservation |
| REQ-054 | Server-side roles |
| REQ-055 | Japanese accessible PC/SP UI |
| REQ-056 | Existing order-flow compatibility |
| REQ-057 | Referenced-product unit immutability and race behavior |
| REQ-058 | Exact decimal syntax, range, scale and unit-specific rules |
| REQ-059 | Quantity/unit presentation and missing-unit recovery on SCR-001/002 |
| REQ-060 | Count-based dashboard and per-unit subtotals |

## Features to be tested

REQ-049–REQ-060, including both successful and failure paths below. Production lines, Plant calendar, BOM, import/export, restore and physical deletion are outside WI-006.

## Approach

| Level | Included? | Rationale |
| --- | --- | --- |
| Domain/Application unit | Planned | Validation, normalization, exact decimal rules, retirement, unit lock and eligibility logic are deterministic. |
| API/database integration | Planned | PostgreSQL indexes/constraints, EF concurrency, role checks, upgraded/fresh migrations, row-lock races and dashboard queries require the real stack. |
| Frontend unit/component | Planned | URL filters, unit-specific raw quantity input, contract decoding, dashboard formatting, Japanese errors and focus. |
| E2E/axe | Planned | Catalog journey, retirement/order-history journey, mixed-unit dashboard, PC/SP layouts and accessibility. |
| Deployment smoke | Not in design revision 3 | No deployment is authorized or performed. |

## Cases

| Test ID | Requirement | Level | Setup and action | Expected result | Priority |
| --- | --- | --- | --- | --- | --- |
| TC-303 | REQ-049 | API integration + frontend | Search by SKU/name, filter active/retired, page beyond end, and trigger network failure | Correct total/order/URL; empty and retry states distinct; no unbounded response | high |
| TC-304 | REQ-050 | Unit + integration | Create valid product with optional drawing number | Trimmed SKU, active row, selected unit, 201 Location; appears in picker/list | high |
| TC-305 | REQ-050 | Unit + integration | Submit empty/long fields, unknown unit and case-only duplicate SKU; race two inserts | 400 field errors or 409 SKU conflict; exactly one row for the SKU | high |
| TC-306 | REQ-051 | Unit + integration | Edit name/unit/drawing; attempt SKU change; submit stale version | Mutable fields saved with new version; SKU change 400; stale edit 409 and no overwrite | high |
| TC-307 | REQ-052 | Integration + E2E | Retire a product referenced by old orders; inspect catalog/order and try new selection | Row retained and marked retired; old orders readable; new selection rejected by UI and server | high |
| TC-308 | REQ-052, REQ-056 | Integration + E2E | Edit another field on an old order retaining its retired product; try changing to another retired product | Unchanged product accepted; changed retired selection rejected | high |
| TC-309 | REQ-052, REQ-056 | Integration | Race order create or changed-product update with product retirement in both lock orders | Either order commits before retirement, or retirement wins and new selection is rejected; no invalid saved order | high |
| TC-310 | REQ-053 | Migration integration | Upgrade an existing database and migrate a fresh database; compare seed IDs/names and 30 unit mappings; edit a seed then apply later migration | IDs/FKs unchanged; units match 004_DB; user edit not overwritten; no DeleteData | high |
| TC-311 | REQ-054 | API integration | Call every product endpoint anonymously, as a no-role user, Admin and Operator | 401/403/allowed as specified; no unauthorized write | high |
| TC-312 | REQ-055 | Frontend + axe + manual | Use list/form/dialog on PC and SP with keyboard, screen reader semantics, 200% zoom | Visible focus, linked errors, dialog focus return, no color-only state, Japanese text, no critical axe finding | high |
| TC-313 | REQ-056 | Regression integration + E2E | Open SCR-001/002/003 after catalog changes; filter historical orders by retired product | Existing journeys work; picker/filter and dashboard show readable SKU/name | high |
| TC-314 | REQ-050–REQ-054 | API integration | Send non-JSON writes, unsupported state field, malformed UUID, unknown ID and unexpected failure | 415/400/404/generic 500 Problem Details as specified; no leaked internal data | medium |
| TC-315 | REQ-057 | Unit + API/PostgreSQL integration | Edit an unreferenced unit; then reference a product and attempt its unit edit, including a race with order creation and stale form `unitLocked=false` | Unreferenced unit can change; referenced unit is immutable at commit, other mutable fields remain editable; conflict has the approved code/field and no partial write | high |
| TC-316 | REQ-058 | Unit + API integration | Submit `kg`/`m` values `1`, `1.234`, `1.2340`, `0`, negative, exponent, over 999999999, and more than three fractional digits; submit a fractional value for every discrete unit | Accepted/rejected exactly as the approved lexical/unit/range contract; rejected body has `errors.quantity`; stored valid decimal is exact | high |
| TC-317 | REQ-058–REQ-059 | Frontend component + E2E | Enter `1.25 kg`, switch to discrete product, correct amount, then edit a historical retired Draft and a non-Draft order | Unit/help update and announcement; raw invalid value retained with linked error; unchanged retired product can save; non-Draft product/quantity stay read-only | high |
| TC-318 | REQ-053, REQ-058–REQ-059 | Migration integration + API | Upgrade existing integer orders and create a fresh database; inspect quantity constraints and all order-linked units; inject or simulate missing unit | Historical values/IDs/FKs preserved, valid decimal accepted, invalid precision/range rejected by DB; missing unit is a recoverable integrity failure, not fabricated `個` | high |
| TC-319 | REQ-056, REQ-059 | Frontend + API/E2E | Filter SCR-002 by a retired product; sort rows with mixed units by quantity on PC/SP; trigger list failure | Historical rows remain findable/readable, each quantity has its own unit, sort is numeric, result summary is count only, failure offers Retry | high |
| TC-320 | REQ-060 | Unit + PostgreSQL integration | Populate two units in the same workload bucket, an empty bucket, equal-count products with different SKUs, and completed orders of different units | Q2 has exactly ten buckets, order-count sum matches active orders, `unitQuantities` has one subtotal per present unit, Q4 ranks by count then SKU, Q5 emits counts without mixed quantity | high |
| TC-321 | REQ-055, REQ-060 | Frontend component + E2E/axe | Render mixed-unit/empty dashboard on PC/SP and maximized chart; simulate missing unit and snapshot failure; inspect keyboard/table at 200% zoom | Count bars/tiles and equivalent semantic per-unit table match one snapshot; no stale/fabricated values; focus and labels meet WCAG 2.2 AA | high |
| TC-322 | REQ-050–REQ-052, REQ-055 | Frontend component + API integration | Trigger SKU conflict, referenced-unit conflict, stale version, timeout after possible write commit, 401/403 and retire dialog Escape/Cancel | No automatic replay or false success; draft retained; errors focus the right control; 401/403 follow baseline; dialog restores focus | high |
| TC-323 | REQ-049–REQ-060 | Regression + security review | Run existing SCR-001–SCR-003 tests and full backend/frontend suites; inspect telemetry and API errors for sensitive data | Existing workflow contracts still work; Problem Details and logs reveal no internal detail, notes, raw quantity or cookie; new spans/metrics appear for new endpoints/queries | high |

## Pass/fail, suspension and environment

A case passes only when its observed result meets the stated expectation. A missing environment, unrun test, or flaky check is not a pass. Suspend integration/E2E execution if PostgreSQL/Testcontainers or the Compose stack is unavailable; resume from a clean isolated database. Use the project's xUnit, Vitest/React Testing Library, Playwright and axe setup. Migration checks need both an upgraded database and a fresh database. Do not reuse a mutable local demo database for destructive reset.

Planned commands after implementation: `dotnet test src/backend/ProductionManagementAI.slnx`; from `src/frontend`, `npm test`, `npm run lint`, `npm run build`; from `tests/e2e`, `npx playwright test` against the Compose stack with credentials configured as in [ai/project.md](../../ai/project.md). These commands have now run under approved implementation revision 4; see evidence.md for actual results.

## Responsibilities, results and gaps

Codex will author and execute the tests in the later, separately approved implementation revision; the user reviews the design and decisions. Actual results will be recorded in `evidence.md`. The original design-stage result was not run. Revision 4 execution results and remaining manual verification limits are recorded in evidence.md. No case is knowingly omitted from this design plan. Published mockup Artifact URLs are unavailable; the local English/Japanese mockups are the visual review source.
