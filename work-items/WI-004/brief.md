# Production Dashboard (Screen C) — Product Brief

## Status

| Work item | Author | Status | Target release |
| --- | --- | --- | --- |
| WI-004 | Claude (for ThanhTN) | draft — revision 2 | unscheduled (demo MVP) |

Revision 1, 2026-09-22. First brief for Screen C, written after WI-003 (Screen B) was merged to `master`. The widgets and metrics that WI-001 DEC-010 left open were settled by the user on 2026-09-22 when Screen C was started (`decisions.md` DEC-001–DEC-007). The windows and sizes each metric uses were settled by the user in DEC-008 after plan revision 1 was approved; no business question is left open.

Revision 2, 2026-09-22. After reviewing the DD-003 mockup the user asked for three additions (DEC-016–DEC-018): application navigation in a navbar on every screen (REQ-040, and REQ-028's wording), a server and database health indicator (REQ-041), and maximizing a chart to full window (REQ-042).

## Overview

Screen C is the production dashboard: the landing page a signed-in Admin or Operator sees after login. It shows the state of production at a glance — how many orders are in each status, which active orders are overdue or due soon, how the open workload falls across the coming weeks, which products carry the most open quantity — and how production has been delivering: orders completed this week and month, the on-time completion rate, a weekly completion trend and the average lead time. It is read-only and replaces the placeholder home page at `/` (DEC-005, DEC-006). It is the third and last demo screen on the locked roadmap (WI-001 DEC-008/DEC-010).

The history metrics need something the schema does not have today: the moment an order was completed. This work item adds a `completed_at_utc` column to `production_orders`, set when Screen A moves an order to `Completed` (DEC-002, DEC-003), and backfills the orders that are already completed (DEC-004). That is a deliberate, bounded change to WI-002's schema and to Screen A's save path.

## Objective

Continue demonstrating the full AI development lifecycle (requirements → BD → DB → DD → code → test → PR) on a third screen whose shape differs from both earlier ones: an aggregate, read-only screen whose design turns on metric definitions, time windows in the plant timezone and aggregate-query cost, and which also carries a schema change back into an existing screen. Per `docs/vi/000-mo-ta-harness-va-quy-trinh-phat-trien-ai.md` §14 and the roadmap in `work-items/WI-001/decisions.md` DEC-008/DEC-010 (Screen C = dashboard).

## Success metrics

| Goal | Metric | Target |
| --- | --- | --- |
| Full traceability | Every `REQ-###` below maps to a BD section, DD section, DB/API element and at least one test case | 100% of in-scope REQs |
| Correct figures | Every metric's acceptance criteria pass as automated tests against known data, including the window boundaries in the plant timezone | all pass |
| Demo usability | A reviewer signs in, lands on the dashboard, sees every widget populated from the seeded data, completes an order in Screen A and sees the completion figures change | demonstrated once, with evidence |
| Query cost | The dashboard loads in a bounded number of aggregate queries, each index-backed or proven cheap at the expected volume | confirmed in the DB-design review |

## Assumptions

- Users reach this screen already authenticated via WI-001's cookie login (`docs/en/architecture/0002-auth-rbac-foundation.md`).
- `Admin` and `Operator` are the only roles for now (WI-001 DEC-015 placeholder roles); both may view the dashboard, matching Screens A and B.
- "Active" means status `Draft` or `InProgress`; "overdue" means an active order whose due date is before today — the same rule Screen B marks (WI-003 REQ-021, BD-002 M-08).
- Every date, week and month boundary is plant-local in `Asia/Tokyo`, from the existing `IPlantClock` (WI-002 DEC-011, DEC-017).
- Status transitions stay as WI-002 defined them: `Completed` is reachable only from `InProgress` and is terminal (BD-001 M-02), so an order is completed at most once and its completion time never changes.
- The 80 seeded demo orders (WI-003 DEC-007) remain; the seed is extended so the history widgets have data (DEC-004). Exact seeded values are settled in database design.
- The dashboard shows figures as of the moment it loads; reloading the page refreshes them. No live push or auto-refresh.

## Actors and user stories

| Actor | As a… | I want to… | So that… | Use case ID |
| --- | --- | --- | --- | --- |
| Admin / Operator | production planner | see how many orders are in each status as soon as I sign in | I know the overall state of production without searching | UC-008 |
| Admin / Operator | production planner | see which active orders are overdue or due in the next few days, and how open work falls across the coming weeks | I can act on what is late and see where the load is | UC-009 |
| Admin / Operator | production planner | see which products carry the most open quantity | I know where material and capacity will be needed | UC-010 |
| Admin / Operator | production manager | see what was completed recently, how often orders finish on time, the weekly trend and the average lead time | I can judge whether production is keeping up | UC-011 |
| Admin / Operator | any user | move between the dashboard, the list and a new order from anywhere, and see whether the system is healthy | I never hunt for navigation, and I know when figures may be unavailable | UC-012 |

## Requirements (in scope)

| ID | Requirement | Acceptance criteria | Priority |
| --- | --- | --- | --- |
| REQ-028 | The dashboard is the landing page at `/` for an authenticated Admin or Operator, replacing the placeholder home page (DEC-005). | **Success:** after login, and whenever `/` is opened, the dashboard is shown with every widget populated; the navbar (REQ-040) leads to the production-order list and to "New production order" (DEC-016). **Failure:** if the dashboard data cannot be loaded, the screen shows an error message and a retry action and no stale or partial figures; when the system has no orders at all, each widget shows its empty state (counts of zero, "no orders" messages, "—" for rates and averages) rather than an error. | must |
| REQ-029 | Status count tiles show the number of orders in each status and in total. | **Success:** one tile per status (`Draft`, `In progress`, `Completed`, `Cancelled`, with Screen A's labels) plus a total, each equal to the count of orders in that status at load time; the total equals the sum of the four. **Failure:** no tile shows a bare enum name; a status with no orders shows 0, not a blank tile. | must |
| REQ-030 | An overdue & due-soon widget lists the active orders that need attention. | **Success:** two groups — *overdue* (active, due date before today) and *due soon* (active, due date from today to today + 7 days inclusive, DEC-007) — each ordered by due date ascending then order number, each showing its total count and at most the first 10 rows (DEC-008), with order number, product (SKU and name), quantity, due date and status. **Failure:** `Completed` and `Cancelled` orders never appear in either group; an order appears in at most one group; an empty group shows a "none" message, not an empty table. | must |
| REQ-031 | A due-date workload chart shows active orders by due week. | **Success:** an "overdue" bar, a bar per Monday–Sunday week for the current week and the next 7 weeks (DEC-008), and a "later" bar for orders due after that (DEC-009), each showing the number of active orders and their total quantity; weeks with no orders are shown as zero, not omitted. The chart has a text or table equivalent. **Failure:** `Completed` and `Cancelled` orders are not counted; every active order is counted in exactly one bar, so the bars sum to the number of active orders. | must |
| REQ-032 | A top-products widget ranks products by open quantity. | **Success:** the top 10 products (DEC-008) by the total quantity of their active orders, descending, ties broken by SKU, each with SKU, name, open quantity and number of active orders. **Failure:** products with no active orders are not listed; `Completed` and `Cancelled` orders do not contribute. | must |
| REQ-033 | The system records when each order is completed (DEC-002, DEC-003). | **Success:** when Screen A saves an order's transition from `InProgress` to `Completed`, the completion time is recorded server-side as that save's UTC timestamp; it is never shown as an editable field and never changes afterwards. Orders already `Completed` before this change receive a completion time — the seeded demo orders plausible dates, any other from its last update time (DEC-004). **Failure:** after the migration, no order is `Completed` without a completion time and no other order has one; a client cannot set or change it through the API; a save that is rejected (invalid transition, stale version) records nothing. | must |
| REQ-034 | A completed-this-period widget shows recent output. | **Success:** the number of orders completed, and their total quantity, in the current Monday–Sunday week and in the current calendar month, each up to today (plant timezone, DEC-008). **Failure:** an order completed before the period starts, or cancelled, is not counted. | must |
| REQ-035 | An on-time completion rate shows delivery performance. | **Success:** over the last 30 days, today included (DEC-008), the share of completed orders whose completion date (plant timezone) is on or before their due date, shown as a whole-number percentage together with the underlying "x of y" counts. **Failure:** when no order was completed in the window the rate shows "—", never 0% or a division error; `Cancelled` orders are excluded from both numerator and denominator. | must |
| REQ-036 | A completion trend chart shows orders completed per week. | **Success:** one bar per Monday–Sunday week over the last 12 weeks (DEC-008), oldest to newest, including the current week to date; weeks with no completions are shown as zero. The chart has a text or table equivalent. **Failure:** each completed order is counted in exactly one week; orders without a completion time are not counted. | must |
| REQ-037 | An average lead time shows how long orders take. | **Success:** over the same last 30 days (DEC-008), the mean number of days from an order's creation to its completion, for orders completed in the window, shown to one decimal place with the number of orders it is based on. **Failure:** when no order was completed in the window it shows "—"; no negative lead time is possible for any order, seeded or real. | must |
| REQ-038 | Only authenticated users with the `Admin` or `Operator` role can view the dashboard or call its API. | **Success:** an Admin or Operator sees the dashboard. **Failure:** an unauthenticated request is rejected with 401 and the UI redirects to `/login`; an authenticated user without either role is rejected with 403 and sees no figures. | must |
| REQ-040 | Every authenticated screen carries a navbar with the application's functions (DEC-016). | **Success:** the shared header shows Dashboard, Production orders and New production order on SCR-001, SCR-002 and SCR-003; the current screen's entry is marked visually and with `aria-current`; below the phone breakpoint the entries sit behind a menu button that opens and closes by mouse, touch and keyboard. **Failure:** no screen offers a second, divergent set of navigation links; the navbar is not shown on the login screen or to an unauthenticated visitor. | must |
| REQ-041 | The dashboard shows whether the server and the database are reachable (DEC-017). | **Success:** an indicator shows the server's and the database's status as text, checked when the dashboard opens and every 30 seconds while it stays open, with the time of the last check. **Failure:** when the database does not answer within the check's timeout, the database shows as unavailable while the server shows as OK; when the server cannot be reached at all, both show as unknown/unreachable rather than keeping the last good state; the check never exposes versions, host names, connection details or error text, and requires the same Admin/Operator authorization as the dashboard. | must |
| REQ-042 | Each dashboard chart can be maximized to the full window and restored (DEC-018). | **Success:** an Expand control on each chart opens it enlarged in a full-window view with its table alongside; Restore or Escape returns to the normal dashboard with focus back on Expand. **Failure:** the maximized view shows the same snapshot's figures as the card (no new request); keyboard focus cannot leave the maximized view while it is open. | must |
| REQ-039 | The dashboard is read-only (DEC-006). | **Success:** the dashboard displays figures only; navigation to other screens is through the normal application links. **Failure:** no widget exposes a save, status-change, delete or drill-down action, and loading the dashboard changes no data. | must |

## Not doing (out of scope)

- Drill-down from a widget into Screen B or Screen A (DEC-006).
- A full status-history or audit table, and started-at / cancelled-at timestamps (DEC-003).
- Editing or correcting a completion time, or reopening a completed order.
- User-selectable date ranges, per-user dashboard layouts, and widget configuration.
- Live updates, auto-refresh, and notifications or alerts on overdue orders.
- Export (CSV/Excel/PDF) and printing.
- Capacity, machine, shift or material metrics — the schema holds none of that data.
- Product catalog UI, and finer-grained permissions than "Admin or Operator" (WI-001 DEC-015 stays open).

## Open questions

| Question | Impact if unresolved | Owner | Status |
| --- | --- | --- | --- |
| Which widgets does the dashboard show? | The whole screen's content | user | answered in decisions.md (DEC-001, DEC-002) — all four current-state widgets and all four history metrics |
| How is completion tracked, and what happens to already-completed orders? | Schema change to WI-002's table; whether history widgets have data in the demo | user | answered in decisions.md (DEC-003, DEC-004) — `completed_at_utc` column; seed + backfill |
| Where does the dashboard live, and does it drill down? | Routing, Screen A/B navigation, and the size of the frontend change | user | answered in decisions.md (DEC-005, DEC-006) — replaces `/`; read-only |
| What does "due soon" mean? | Which orders REQ-030 lists | user | answered in decisions.md (DEC-007) — today to today + 7 days |
| The windows and sizes each metric uses: rows per overdue/due-soon group, workload look-ahead, top-N products, week and month boundaries, on-time and lead-time windows, trend length | Exact figures every metric's tests assert; the seeded data needed to populate them | user | answered in decisions.md (DEC-008) — 10 rows; 8 weeks; top 10; Mon–Sun weeks; calendar month; 30 days; 12 weeks |
| Navigation, health indicator and chart full screen (raised at mockup review) | Shared header on every screen; a new endpoint; chart interaction | user | answered in decisions.md (DEC-016–DEC-018) |
