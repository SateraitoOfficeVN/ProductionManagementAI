# WI-014 — Execution plan

| Revision | Date | Phase | State | Approval source |
| --- | --- | --- | --- | --- |
| 1 | 2026-10-06 | Fix order-list overflow (BUG-008), test, PR | Approved | User: "approved, go ahead" (2026-10-06) |

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
