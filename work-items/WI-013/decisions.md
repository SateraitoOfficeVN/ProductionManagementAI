# WI-013 — Decisions

| ID | Date | Decision | Source |
| --- | --- | --- | --- |
| DEC-001 | 2026-10-06 | ~~Keep 48 px controls~~; superseded by DEC-006. | User |
| DEC-002 | 2026-10-06 | Fix behaviour gaps (edit route, return to list after save) in this WI. | User |
| DEC-003 | 2026-10-06 | Keep the shared WI-004 top bar; the mockup's navy bar predates it. Keep the form's back link to 「製品マスタ」. | Agent proposal presented to user, not objected |
| DEC-004 | 2026-10-06 | Old `/products/:id` URLs redirect to `/products/:id/edit` so existing bookmarks keep working. | Agent, within scope |
| DEC-005 | 2026-10-06 | "Hidden by current conditions" means the saved product is not among the rows on the returned list page. | Agent, within scope |
| DEC-006 | 2026-10-06 | Button and field heights match the production-order screens (40/42 px; pager as `ListPagination`). | User: "button height make same product-oder screen" |
| DEC-007 | 2026-10-06 | Rows per page 10/20/50/100 (default 20) through an optional allow-listed `pageSize` on `GET /api/product-master`, reusing the order list's `PageSizeSelect`. Approved 004_DD-API stays unedited. | User: "add dropdown allow select record per page 10, 20, 50,100" |
| DEC-008 | 2026-10-06 | Revision 1 partial code changes restored to `master`; plan recreated as revision 2. | User: "roll-back all code chage. recreate the plan" |
| DEC-009 | 2026-10-06 | `pageSize` change recorded in DEC-007 only; no design addendum. | User: "decisions record only" |
| DEC-010 | 2026-10-06 | Total count top-left and 「表示件数」 top-right above the table; pager stays at the bottom right. | User: "display total count at top-left and select per-page at top-right" |
| DEC-011 | 2026-10-06 | Required fields marked with a red 「*」 instead of 「必須」 on the create and edit forms (shared marker); the asterisk is `aria-hidden` because inputs carry `required`. | User: "register product screen change 必須 to * with red color" |
| DEC-012 | 2026-10-06 | PC table headers, status badge and row actions are `white-space: nowrap`; a long product name wraps within the remaining width. | User: "make table header, status and action button no-wrap" |
| DEC-013 | 2026-10-06 | CI run 37409988722 fixes limited to Product master: SP card title wraps long SKU/name anywhere; the DEC-012 long-name E2E case uses a mocked list response instead of creating a product, so it no longer leaves data that widens the order-list product filter. The order-list SP overflow is a separate bug for a new WI requested later by the user. | User: "error in order list will be fix-bug in a new WI ... in WI-013 only update for product master" |
| DEC-014 | 2026-10-06 | Squash-merge PR #41 (repository convention) and delete the feature branch; closeout records go in a separate documentation-only PR, as for WI-012. | User: "merge PR #41 and clean up branch", "yes, create closeout PR for WI-013" |
