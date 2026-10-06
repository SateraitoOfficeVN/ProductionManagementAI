# WI-014 — Order list overflows the page when a product name is long

| Work item | Workflow | Status | Baseline |
| --- | --- | --- | --- |
| WI-014 | bug-fix | Plan revision 1 awaiting review | 46bf430 |

## Report and objective

Found by WI-013 CI run 37409988722 (`mobile.spec.ts` "SP layout shows the order list as cards") and requested
as a separate work item by the user on 2026-10-06. WI-012 recorded the same root cause on PC as a bug
candidate (WI-012 evidence: "production-order intrinsic-size layout"). Objective: the order list
(SCR-002, `/production-orders`) never becomes wider than the viewport, whatever the product names are.

## Defect and evidence (BUG-008)

Reproduced on 2026-10-06 against the local stack at `46bf430`, whose database holds products with names up to
200 characters (longest product-filter option 211 characters):

| Viewport | Page width | Product filter `<select id="productId">` |
| --- | --- | --- |
| 390 px phone | 1763 px (layout viewport forced to 1560 px) | 1713 px |
| 1280 px PC | 1961 px | 1713 px |
| 1920 px PC | 2281 px | 1713 px |

Cause: in `ProductionOrderFilters.tsx` the select has no width constraint, so its minimum size is its longest
option; the grid tracks around it (`sm:grid-cols-[1.4fr_1fr]`, and the implicit single-column track on phones,
and the list page's `main` grid track) use the default `auto` minimum, so they grow with it. The status check
boxes are squeezed and the page scrolls sideways.

Not affected (same data, same viewports): `/production-orders/new` (its product select stays in its column),
`/`, `/products`, `/production-lines`, `/plant-calendar`.

Severity: moderate UX defect. Any valid product name (up to 200 characters) triggers it; no data loss, no
security impact.

## Acceptance criteria

- At 390, 1280 and 1920 px with a 200-character product name, `/production-orders` has no horizontal page
  scroll, and the product filter stays inside its column.
- The product filter still lists every product with its full `SKU — name` text, and filtering still works.
- PC filter layout (status beside product, the three-column row below) and SP layout (single column, cards)
  are otherwise unchanged; approved 002 designs unchanged.
- Regression tests cover phone and PC without leaving long-name products in the shared database.

## Out of scope

Other screens; backend/API/database; product-name length rules; approved design documents.
