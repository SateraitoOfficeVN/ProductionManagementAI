# WI-014 — Evidence

All results 2026-10-06, local branch `fix/WI-014-order-list-overflow`, plan revision 1.

## Changes

| Area | Files |
| --- | --- |
| Filter panel sizing | `src/frontend/src/features/production-orders/ProductionOrderFilters.tsx`: `min-w-0` on the form and product field, `grid-cols-1 sm:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]`, `w-full min-w-0` on the select |
| List page | `src/frontend/src/features/production-orders/ProductionOrderListPage.tsx`: `min-w-0` on `main` |
| Regression E2E | `tests/e2e/specs/screen-b.spec.ts`: mocked 200+-character product added to `GET /api/products` (no database write); at 390 and 1280 px asserts the option text is present, no horizontal scroll, select inside the filter form |

## Checks

| Check | Command | Result |
| --- | --- | --- |
| Frontend lint | `npm run lint` | Passed |
| Frontend build | `npm run build` | Passed |
| Frontend unit | `npx vitest run` | Passed: 22 files, 267 tests |
| Regression E2E on old image (`46bf430` frontend) | `npx playwright test specs/screen-b.spec.ts -g BUG-008` | Failed as expected (page scroll width > viewport) |
| Regression E2E after rebuilding Compose `frontend` | same | Passed |
| Full E2E | `npx playwright test` against rebuilt local Compose stack | 56 passed, 11 failed — all 11 are Plant calendar specs needing CI's activated-calendar fixture (same local limitation as WI-013); `mobile.spec.ts`, which failed locally before because of the demo DB's long-name products, now passes |

## Measurements (local demo DB with long-name products, Chromium)

| Viewport | Before (page / select) | After (page / select / filter form) |
| --- | --- | --- |
| 390 px | 1763 / 1713 px | 390 / 324 / 358 px |
| 1280 px | 1961 px page | 1280 / 382.5 / 976 px |
| 1920 px | 2281 px page | 1920 / 382.5 / 976 px |

PC and SP screenshots after the fix match the 002 mockup layout: status and product share the first row at
about 1.4 : 1, date and order-number fields in three columns, buttons unchanged; on SP the fields stack at full width.
Screenshots were not added to the work item.

## Not done

No backend, database, design document or other screen changes. The demo DB keeps its long-name products.
