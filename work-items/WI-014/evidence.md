# WI-014 — Evidence

All results 2026-10-06, local branch `fix/WI-014-order-list-overflow`, plan revisions 1 and 2.

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

## Revision 1 CI

PR #43, run [37427110603](https://github.com/SateraitoOfficeVN/ProductionManagementAI/actions/runs/37427110603):
Backend, Frontend and E2E passed — 239 backend unit, 203 integration, 267 frontend, 67 E2E.

## Revision 2 — open product list contained, truncated, full name on hover (DEC-003, DEC-004)

### Changes

| Area | Files |
| --- | --- |
| Customizable select styling (`@supports (appearance: base-select)`): picker width = control width, one-line ellipsis for options and the chosen value, gray hover, bold checked option | `src/frontend/src/index.css` |
| `title` with the full `SKU — name` on the select and each option; hidden `<button><selectedcontent>` for the chosen value | `src/frontend/src/features/production-orders/ProductionOrderFilters.tsx`, `src/frontend/src/selectedcontent.d.ts` |
| Unit test (titles) | `src/frontend/tests/unit/production-orders/ProductionOrderListPage.test.tsx` |
| E2E TC-121 | `tests/e2e/specs/screen-b.spec.ts` (P-1001 renamed only in the mocked `/api/products` response, so filtering runs a real query) |

### Checks

| Check | Result |
| --- | --- |
| TC-121 on the revision 1 image | Failed as expected (the native popup is not part of the page, so the options were not visible). A first run after the change also failed: the chosen long name wrapped over many lines in the closed control, which led to DEC-004 |
| TC-121 after the change, 390 and 1280 px | Passed: every option inside the control's edges, long option truncated (`scrollWidth > clientWidth`), `title` = full label, no page scroll, choosing it sets the value and title and 検索 filters (`productId` in URL, result summary shown) |
| Frontend lint / build / unit | Passed; 22 files, 269 tests |
| Screen B + mobile E2E | 13 passed |
| Full E2E, rebuilt local Compose stack | 57 passed, 11 failed — the same Plant calendar environment failures as revision 1 |
| Accessibility tree (Chromium 153) | `combobox "製品"` with full-name options; the helper button is not exposed. Axe in unit and E2E passes |
| Keyboard (Chromium 153) | Focus → Enter opens the list, ArrowDown + Enter changes the value, Escape closes |
| Control height | Product select 42 px, same as the other filter fields (42 px) at 390 and 1280 px |
| Hover tooltip rendering | Not run: headless Chromium does not render OS tooltips. `title` attributes are asserted; visual tooltip check left to user review |
| Firefox / Safari | Not run: no `base-select` support; they keep the native popup by design (DEC-003) |

### Known differences

In base-select mode the closed control shows Chromium's ▼ picker icon instead of the native chevron of other selects.
Height and border match.
