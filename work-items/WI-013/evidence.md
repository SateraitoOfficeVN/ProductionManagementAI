# WI-013 — Evidence

All results 2026-10-06, local branch `feature/WI-013-product-master-alignment` (uncommitted), plan revision 2.

## Changes

| Area | Files |
| --- | --- |
| Backend `pageSize` (DEC-007) | `src/backend/ProductionManagementAI.Application/Products/ProductMasterContracts.cs`, `src/backend/ProductionManagementAI.Infrastructure/Products/ProductMasterRepository.cs` |
| Frontend list/form/dialog, heights, rows per page, routes | `src/frontend/src/features/products/ProductMasterPage.tsx`, `products/api.ts`, `src/frontend/src/App.tsx`, `production-orders/messages.ts` |
| Tests | `tests/integration/.../Products/ProductMasterEndpointTests.cs`, `src/frontend/tests/unit/products/ProductMasterPage.test.tsx`, `tests/e2e/specs/product-master.spec.ts`, `tests/e2e/specs/product-ui.spec.ts` |

## Checks

| Check | Command | Result |
| --- | --- | --- |
| Backend build | `dotnet build src/backend/ProductionManagementAI.slnx` | Passed, 0 errors |
| Backend unit + integration | `dotnet test src/backend/ProductionManagementAI.slnx` | Passed: 239 unit, 203 integration (195 before + 8 new `pageSize` cases) |
| Frontend lint | `npm run lint` | Passed |
| Frontend build | `npm run build` | Passed |
| Frontend unit | `npm test` | Passed: 22 files, 266 tests (258 before + 8 new WI-013 cases), including TC-301 inline-text scan and axe |
| Product E2E | `npx playwright test specs/product-master.spec.ts specs/product-ui.spec.ts` | Passed after the height fix below |
| Full E2E | `npx playwright test` against rebuilt local Compose stack | 53 passed, 11 failed — all 11 are Plant calendar specs (environment, see below); every product, order, dashboard and production-line spec passed |

## Control heights (DEC-006), measured in Chromium at 1280×900

| Control | Order screen | Product screen |
| --- | --- | --- |
| Header add link | 「+ 新規製造指示」 40 px | 「製品を登録」 40 px |
| Filter search / clear | 検索 42 px / クリア 42 px | 検索 42 px / クリア 42 px |
| Text input | 42 px | 42 px |
| Row actions | — | 編集 / 使用停止 42 px |

The first E2E run caught two mismatches (product 検索 40 px vs order 42 px; row buttons 38 px because the table
uses `text-sm`). Fixed by placing 検索/クリア in a stretching flex row like the order filters and pinning
`text-base` on buttons; the height regression test then passed.

## Visual comparison with the 004_DD mockup

Screenshots of the rebuilt stack (PC 1280, SP 390) were compared with mockup states 1 (list), 2/3 (form,
error summary, retire dialog), 4 (SP cards) and 5 (edit with locked fields). They match in wording, colours,
badges, field markers, error/hint styling and button order. Intentional differences: shared WI-004 top bar
(DEC-003), 40/42 px controls (DEC-006), rows-per-page select (DEC-007). Screenshots were kept in the session
scratchpad only, not in the work item.

## Plant calendar E2E failures (environment, not WI-013)

The 11 failures are `plant-calendar.spec.ts` (5), `plant-calendar-layout.spec.ts` (5) and
`plant-calendar.mobile.spec.ts` (1). Each times out on a disabled calendar edit button (for example
「日付例外」), because the local demo database has no activated plant calendar. CI activates one only in its
disposable fixture (`.github/workflows/ci.yml`, `scripts/calendar/activate.sql`); WI-010 records that normal
startup never activates calendars. WI-013 changes no calendar file (`git diff --name-only`). Activating the
demo database is a data change outside the approved plan, so it was not done. CI on a PR would run these specs
with its fixture.

## Not run

Manual screen-reader speech and physical mobile keyboard/IME, consistent with earlier work items.

## DEC-010 layout follow-up (2026-10-06)

Total count moved to top-left and 「表示件数」 to top-right above the table; pager stays bottom-right. Product
unit tests 12/12, lint and build pass; frontend rebuilt; product E2E 13/13 pass, including a new position check
(count and select above the table, aligned to its left and right edges). PC 1280 and SP 390 screenshots inspected.

## DEC-011 required marker (2026-10-06)

「必須」 replaced by a red 「*」 (`#a12424`, `aria-hidden`) on the create and edit forms. Product unit tests 13/13
(one new marker case, which also checks the accessible name stays 「製品コード」) and TC-301 pass; lint and build
pass; frontend rebuilt; product E2E 13/13 pass. Create-form screenshot inspected.

## DEC-012 no-wrap table cells (2026-10-06)

PC table headers, status badge and row actions no longer wrap; the long product name wraps within the remaining
width. New E2E case creates a product with an eight-times-repeated name and checks single-line headers, a
single-line badge and side-by-side 編集/使用停止. Product unit 13/13, lint pass; frontend rebuilt; product E2E
14/14 pass. Screenshot at 1100 px inspected. The SKU cell can still break at a hyphen (not in this request).
