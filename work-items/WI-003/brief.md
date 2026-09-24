# Production Order List (Screen B) — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-003 | Claude (for ThanhTN) | draft — revision 1 | unscheduled (demo MVP) |

Revision 1, 2026-09-22. First brief for Screen B, written after WI-002 (Screen A) was merged to `master`. The scope and design answers in `decisions.md` DEC-001–DEC-007 were all given by the user on 2026-09-22 when Screen B was started; no business question is left open.

## Overview

Screen B lets a signed-in Admin or Operator find production orders: a paged, sortable, filterable list of every order in the system, from which an order is opened in Screen A (WI-002) for editing. It is the screen that makes Screen A reachable by normal use instead of by typing a URL, and the second of the three demo screens on the locked roadmap (WI-001 DEC-008/DEC-010).

## Objective

Continue demonstrating the full AI development lifecycle (requirements → BD → DB → DD → code → test → PR) on a second screen whose shape differs from Screen A's: a read-only query screen with server-side filtering, sorting and paging, rather than a form with a state machine. Per `docs/vi/000-mo-ta-harness-va-quy-trinh-phat-trien-ai.md` §14 and the roadmap in `work-items/WI-001/decisions.md` DEC-008/DEC-010 (Screen B = production-order list).

## Success metrics

| Goal | Metric | Target |
| --- | --- | --- |
| Full traceability | Every `REQ-###` below maps to a BD section, DD section, DB/API element and at least one test case | 100% of in-scope REQs |
| Correct query behavior | Acceptance criteria below pass as automated tests (unit/integration/E2E) | all pass |
| Demo usability | A reviewer can filter, sort and page the list and open an order in Screen A in the local Docker Compose environment | demonstrated once, with evidence |
| Query cost | The list query stays index-backed for the default sort and each supported filter | confirmed in the DB-design review |

## Assumptions

- Users reach this screen already authenticated via WI-001's cookie login (`docs/en/architecture/0002/0002_ADR_auth-rbac-foundation.md`).
- `Admin` and `Operator` are the only roles in the system for now (WI-001 DEC-015 placeholder roles); both may use this screen, matching Screen A (WI-002 DEC-001).
- Production orders and products already exist with the WI-002 schema (`docs/en/database/001/001_DB_製造指示登録・編集.md`); Screen B reads them and adds no new business entity.
- Products stay seeded reference data with no catalog UI (WI-002 DEC-005), so the product filter lists the 30 seeded products.
- Dates shown and filtered are plant-local dates in `Asia/Tokyo` (WI-002 DEC-011, DEC-017); `due_date` is already a date, not a timestamp.
- The placeholder home page stays; the list lives at its own route (DEC-004).
- The database seeds roughly 60–100 demo production orders so paging and filtering have data (DEC-007); the exact count and its environment scope are settled in database design.

## Actors and user stories

| Actor | As a… | I want to… | So that… | Use case ID |
| --- | --- | --- | --- | --- |
| Admin / Operator | production planner | see all production orders in one list with their key fields | I know what is in the plan without opening each order | UC-004 |
| Admin / Operator | production planner | narrow the list by status, product, due-date range or order number | I can find the order I need among many | UC-005 |
| Admin / Operator | production planner | sort the list and page through it at a page size I choose | I can work through a long list in the order that matters to me | UC-006 |
| Admin / Operator | production planner | open an order from the list, or start a new one | I can act on what I found without retyping a URL | UC-007 |

## Requirements (in scope)

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| REQ-020 | An authenticated Admin or Operator can open the production-order list and see the orders that exist. | **Success:** opening the screen shows the first page of orders in the default sort (due date ascending, DEC-002) with no filter pre-applied — orders of every status, including `Completed` and `Cancelled` (DEC-006) — together with the total number of matching orders. **Failure:** if the list cannot be loaded, the screen shows an error message and a retry action, and no partial or stale rows; when the system has no orders at all, it shows an empty state with a "New production order" action instead of an empty table. | must |
| REQ-021 | Each row shows the order's identifying and planning fields. | **Success:** every row shows order number, product (SKU and name), quantity, due date, status and last-updated date; an order whose due date is before today (plant timezone) and whose status is `Draft` or `InProgress` is visibly marked as overdue. **Failure:** no row shows a raw identifier (GUID) or a bare enum name in place of its label. | must |
| REQ-022 | The list can be filtered by status, product, due-date range and order number, combined with AND. | **Success:** each filter alone and any combination returns exactly the matching orders and the matching total; the status filter is multi-select, so any set of statuses can be selected and selecting none means no status restriction (DEC-005); the order-number filter matches a partial, case-insensitive fragment of the order number; the due-date range is inclusive at both ends and either end may be left empty. **Failure:** a filter combination that matches nothing shows a "no orders match" state with a clear-filters action, not the unfiltered list; an invalid filter value (unknown status, unknown product, `from` after `to`, malformed date) is rejected with a field error and the displayed rows are not replaced by wrong ones. | must |
| REQ-023 | The list can be sorted by order number, product, quantity, due date, status or last-updated, ascending or descending. | **Success:** choosing a column sorts the whole result set (not only the current page) by it; choosing the same column again reverses the direction; the active sort column and direction are visible. The default is due date ascending, with order number ascending as the tie-breaker so paging is stable. **Failure:** an unsupported sort field or direction (e.g. a crafted API request) is rejected rather than silently falling back to an arbitrary order. | must |
| REQ-024 | The list is paged server-side, and the user chooses the page size. | **Success:** the screen shows one page at a time with the page size chosen from a dropdown (10, 20, 50, 100; default 20, DEC-002), the current range and total, and controls to move between pages; changing filters, sort or page size returns to page 1. **Failure:** a page beyond the last page shows an empty page with the paging controls intact (no error page); an invalid page number or page size (0, negative, non-numeric, above the maximum) is rejected by the API rather than loading an unbounded result set. | must |
| REQ-025 | The user can open an order in Screen A from the list, and start a new order from the list. | **Success:** activating a row — by mouse or keyboard — opens that order in Screen A's edit mode (`/production-orders/{id}`); a "New production order" action opens Screen A's create mode (`/production-orders/new`). **Failure:** the list itself changes no order data — it exposes no save, status-change or delete action (DEC-003). | must |
| REQ-026 | Only authenticated users with the `Admin` or `Operator` role can view the screen or call its API. | **Success:** an Admin or Operator can open the screen and query the list. **Failure:** an unauthenticated request is rejected with 401 and the UI redirects to `/login`; an authenticated user without either role is rejected with 403 and sees no order data. | must |
| REQ-027 | The current filters, sort, page and page size are kept in the screen's URL. | **Success:** reloading the page, or sharing the URL with another authorized user, reproduces the same view; the browser Back button returns to the previous view. **Failure:** an unreadable or out-of-range value in the URL falls back to the default view rather than showing an error page. | should |

## Not doing (out of scope)

- Dashboard and metrics (Screen C, separate work item).
- Creating or editing an order from the list — every write path stays in Screen A (DEC-003).
- Deleting production orders — unchanged from WI-002; `Cancelled` is the only way to retire an order.
- Bulk actions, multi-select and inline status change.
- Export (CSV/Excel) and printing.
- Saved or named filter presets and per-user default views.
- Full-text search across notes or product names — the free-text filter matches the order number only (DEC-001).
- Product catalog UI, and finer-grained permissions than "Admin or Operator" (WI-001 DEC-015 stays open).

## Open questions

| Question | Impact if unresolved | Owner | Status |
| --- | --- | --- | --- |
| Is the status filter single-select or multi-select? | Whether a planner can see all active orders in one view; changes the API contract and the query/index design | user | answered in decisions.md (DEC-005) — multi-select |
| Does the default view show `Completed` and `Cancelled` orders, or only active ones? | What a planner sees on opening the screen, and what the displayed total means | user | answered in decisions.md (DEC-006) — show all, no filter pre-applied |
| Should the demo database seed enough production orders to make paging and filtering demonstrable? | Whether paging and sorting can be shown at all without manual data entry, and whether a seed migration is in scope | user | answered in decisions.md (DEC-007) — seed roughly 60–100 orders |
