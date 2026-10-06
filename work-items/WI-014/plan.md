# WI-014 — Execution plan

| Revision | Date | Phase | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-06 | Fix order-list overflow (BUG-008), test, PR | Approved, steps done (PR #43, CI 37427110603 passed) | User: "approved, go ahead" (2026-10-06) |
| 2 | 2026-10-06 | Contain the open product dropdown, truncate long names, full name on hover | Approved | User: "approved, go ahead" (2026-10-06) |

## Revision 1

### Objective and scope

Fix BUG-008 (see [brief](brief.md)) in the order list's filter panel only, with CSS sizing changes; no
behaviour, wording, API or design change.

### Steps

| # | Step | Files | Verification |
| --- | --- | --- | --- |
| 1 | Create a local branch `fix/WI-014-order-list-overflow` | — | `git status` |
| 2 | Let the product filter shrink to its column: `w-full min-w-0` on the select; zero-minimum grid tracks (`grid-cols-1`, `sm:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]`); `min-w-0` on the filter form and list `main` grid so no track grows with a long option | `ProductionOrderFilters.tsx`, `ProductionOrderListPage.tsx` | Measured widths at 390/1280/1920 px on the local stack |
| 3 | Regression E2E: serve a 200-character product from a mocked product-choices response (no database write) and assert no horizontal scroll and the select inside its column at 390 px (cards) and 1280 px (table); assert the full `SKU — name` option text is still present | `tests/e2e/specs/screen-b.spec.ts` or `mobile.spec.ts` | Fails on `46bf430`, passes after the fix |
| 4 | Frontend lint/build/unit tests; rebuild local Compose frontend; Screen B, mobile and full E2E suites; PC/SP screenshots compared with the 002 mockup | — | evidence.md |
| 5 | Record evidence/status; commit, push, open a PR to `master` and record its CI run | — | PR URL, CI run |

### Permitted actions

Edits to the two frontend files and E2E specs listed above, WI-014 records, rebuilding the local Compose
`frontend` image and running tests against the local stack, one commit, `git push` of the WI-014 branch,
`gh pr create`, and read-only CI checks. Not permitted without further approval: merge, branch deletion,
deployment, changes to the demo database (long-name products from earlier test runs stay as a reproduction
fixture), other screens, approved design documents.

### Risks / stop conditions

- A native `<select>` narrower than its longest option truncates the closed control's text; the open list
  still shows full names. If the 002 design requires the full text to be visible when closed, stop and ask.
- Zero-minimum tracks could change PC column proportions; screenshots are compared before and after.
- The local demo database keeps long-name products, so earlier local failures of `mobile.spec.ts` should turn
  green with the fix; Plant calendar specs still need CI's calendar fixture locally (known WI-013 limitation).
- Anything requiring backend or another screen stops the work for a new revision.

### Review and approval

Review status: revision 1 approved by the user on 2026-10-06 ("approved, go ahead").

## Revision 2

### Trigger

User review of PR #43 on 2026-10-06 (screenshot): the closed control fits its column, but the **open** product
list still extends past the container and the screen with a long name. Request: keep it inside the container,
truncate long names, and show the full name on hover.

### Objective and scope

Order-list product filter only (`ProductionOrderFilters.tsx`). It stays a native `<select>`, as 002_DD §4
requires: same options, labels `{sku} — {name}`, values, keyboard handling and accessibility semantics. Only
presentation changes. No backend, API, wording or other screen changes. The approved 002 designs are not edited.

### Approach (recommended)

Use the browser's customizable select (`appearance: base-select`, Chromium 135+; local Playwright Chromium 153,
prototype verified):
- the open list (`::picker(select)`) is exactly as wide as the select (`anchor-size(width)`), so it stays inside
  the filter column;
- each option and the selected value are cut to one line with an ellipsis (`…`);
- the select and every option get a `title` with the full `SKU — name`, so hovering shows the full name; the
  accessible names keep the full text.

Fallback: browsers without `base-select` (Firefox, Safari today) ignore these rules and keep today's native
popup. Revision 1 still keeps the page itself from overflowing there.

Alternative, not recommended: a custom combobox/listbox component. It works in every browser, but it departs
from the DD's native `<select>`, needs its own keyboard and screen-reader handling, and is much larger. It
would need a decision record as a design deviation.

### Steps

| # | Step | Files | Verification |
| --- | --- | --- | --- |
| 1 | Continue on `fix/WI-014-order-list-overflow` (PR #43 open, not merged) | — | `git status` |
| 2 | Add `base-select` styling (picker width = control width, one-line ellipsis, picker inside the viewport) and `title` on select and options | `ProductionOrderFilters.tsx`; CSS in `src/frontend/src/index.css` only if Tailwind arbitrary variants cannot express `::picker(select)` | Screenshots of the open list at 390/1280 px |
| 3 | Tests: unit test that the select and options carry the full `SKU — name` as `title`; E2E TC-121 that opens the list with the mocked long product and asserts every option's right edge stays within the select/filter form, the long option is truncated (`scrollWidth > clientWidth`), its `title` is the full text, and choosing it still filters (REQ-022); update `test-plan.md` | `ProductionOrderListPage.test.tsx` or a filter unit test, `tests/e2e/specs/screen-b.spec.ts`, `work-items/WI-014/test-plan.md` | TC-121 fails before step 2, passes after |
| 4 | Frontend lint/build/unit; rebuild local Compose `frontend`; Screen B, mobile and full E2E; keyboard check (open, arrows, Enter, Esc) | — | evidence.md |
| 5 | Record decisions/evidence/status/test-plan; one commit and push to the same branch (updates PR #43); record CI | — | CI run |

### Permitted actions

Edits to the files listed above and WI-014 records (including committing `test-plan.md`); rebuilding the local
Compose `frontend` image and running tests locally; one commit and `git push` to the existing branch; read-only CI
checks. Not permitted without further approval: merge, branch deletion, deployment, demo-DB changes, other
screens, approved design documents, a custom dropdown component.

### Risks / stop conditions

- Firefox/Safari keep the wide native popup until they support `base-select`. If every browser must behave the
  same, stop and ask (the custom component would need a new revision).
- `title` tooltips show on mouse hover only; touch devices still show the truncated text. The full name stays in
  the accessible name and on the product master screen.
- If `base-select` changes the control's height or look against the order screens (WI-013 height parity), it is
  restyled to match; if it cannot be, stop and ask.

### Review and approval

Review status: revision 2 approved by the user on 2026-10-06 ("approved, go ahead"), base-select approach.
