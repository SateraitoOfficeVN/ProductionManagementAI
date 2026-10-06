# WI-013 — Product master screen alignment with approved mockup

| Work item | Workflow | Status | Baseline |
| --- | --- | --- | --- |
| WI-013 | bug-fix | Merged via PR #41 as b32656f | 33555f3 |

## Report and objective

On 2026-10-06 the user reported that the Product master screen (SCR-004, `/products`) "is
not implemented correctly" against its mockup. A side-by-side comparison of the running
demo stack with the approved
[004_DD mockup](../../docs/en/020_detailed-design/004/mockups/004_DD_screen-product-master-mockup.ja.html),
[004_BD](../../docs/en/010_basic-design/004/004_BD_製品マスタ.md) and
[004_DD-SPD](../../docs/en/020_detailed-design/004/004_DD-SPD_製品マスタ.md) confirmed it.
Objective: make the implemented SCR-004 list, form, retire dialog and phone layout match
the approved design without changing the approved design files.

## Defects

BUG-006 (visual/wording): undifferentiated grey buttons; Add labelled 「新規製品」 instead of
「製品を登録」; missing subtitle; search label, headings, dialog title/body, empty-state text
and forbidden text differ from the design; actions column titled 「編集」 instead of 「操作」;
status shown as plain text instead of badges; count/pager placement and `1 / N` form differ;
form lacks 「必須」/「任意」 markers, greyed 「（変更不可）」 read-only fields, retired-product
banner and error summary box; Save/Cancel order differs; phone cards differ.

BUG-007 (behaviour): edit route is `/products/:id` instead of `/products/:id/edit` (BD route
table); successful create/edit stays on the form instead of returning to the saved list URL
with a success notice and a Clear hint when filters hide the row (BD flow, DD state model,
SPD-03 step 4); empty catalog and no-match are not distinguished and a page past the end is
not corrected (SPD-01 step 4); forbidden state uses the production-order permission text.

Moderate UX/design-consistency defect; no data loss or security impact.

ENH-001 (user request 2026-10-06): a rows-per-page dropdown with 10/20/50/100 on the list. The
approved [004_DD-API](../../docs/en/020_detailed-design/004/004_DD-API_製品マスタ.md) fixes
`pageSize` at 20, so `GET /api/product-master` gains an optional allow-listed `pageSize` query
parameter (default 20; any other value is a 400 `VALIDATION` on `pageSize`). Response shape unchanged.

## Acceptance criteria

- Every state in the 004_DD mockup (list PC, form, error + retire dialog, phone cards,
  retired edit, forbidden, no match + saved-but-hidden) is reproduced in the running app.
- Shared top bar is kept (WI-004).
- Button and field heights match the production-order screens (user request 2026-10-06, replacing
  the earlier "keep 48 px" choice).
- The list offers 「表示件数」 10/20/50/100 (default 20), kept in the URL as `pageSize` and preserved
  by search, Clear, paging and return-after-save, as on the order list.
- Authorization and stale/unit-lock/uncertain-write handling unchanged.
- Backend build/unit/integration, frontend lint/build/unit tests and the full Playwright suite pass;
  axe has no violations.

## Out of scope

Database changes; edits to approved design documents; shared header redesign; other screens; API
changes beyond the optional `pageSize` parameter.
