# Production Dashboard (Screen C) — Decision Log

Decisions for WI-004. Decisions carried over from earlier work items keep their original IDs and are referenced as `WI-00N DEC-0MM`; IDs in this file are WI-004's own.

## Log

| ID | Date | Decision needed | Decision maker | Status | Rationale (summary) |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | 2026-09-22 | Which current-state widgets the dashboard shows | user | decided | All four: status count tiles, overdue & due-soon list, due-date workload chart, top products by open quantity |
| DEC-002 | 2026-09-22 | Whether the dashboard includes history-based metrics | user | decided | Yes — add completion tracking, and show all four history metrics: completed this period, on-time rate, completion trend, average lead time |
| DEC-003 | 2026-09-22 | How completion is tracked in the database | user | decided | One nullable `completed_at_utc` column on `production_orders`, not a status-history table |
| DEC-004 | 2026-09-22 | What happens to orders already `Completed` when tracking is added | user | decided | Seeded demo orders get plausible completion dates; any other completed order is backfilled from `updated_at_utc` |
| DEC-005 | 2026-09-22 | Where the dashboard lives | user | decided | It replaces the placeholder home page at `/` |
| DEC-006 | 2026-09-22 | Whether widgets drill down into Screen B / Screen A | user | decided | No — the dashboard is read-only |
| DEC-007 | 2026-09-22 | What "due soon" means | user | decided | Active orders due from today to today + 7 days (plant timezone) |
| DEC-008 | 2026-09-22 | Windows and sizes each metric uses | user | decided | 10 rows per group; 8-week look-ahead; top 10 products; Mon–Sun weeks; calendar month; 30-day on-time and lead-time window; 12-week trend |
| DEC-009 | 2026-09-22 | Where the workload chart counts active orders due after the 8-week look-ahead | user | decided | In a "later" bar, so every active order is counted exactly once |
| DEC-010 | 2026-09-22 | How charts are drawn and made accessible | Claude (UI/accessibility, during BD-003) | decided | Inline SVG drawn by the page, no charting library; value labels on bars and a "View as table" disclosure |
| DEC-011 | 2026-09-22 | One snapshot endpoint or one endpoint per widget | Claude (technical, during BD-003) | decided | One endpoint returning every widget from one consistent snapshot |
| DEC-012 | 2026-09-22 | Where the placeholder home page's two links go | Claude (UI, during BD-003) | superseded by DEC-016 | Page actions on the dashboard itself; the shared header is not changed |
| DEC-013 | 2026-09-22 | How the demo seed gives the delivery widgets data | user | decided | Both: re-date the 16 seeded completed orders and add 40 historical completed orders |
| DEC-014 | 2026-09-22 | Whether the seed adds active orders due beyond WI-003's 40-day horizon | Claude (technical, during DB-004) | decided — DB-004 approved without objection | Add 4 (due +42, +49, +56, +63), so week bars 6–7 and Later are never empty |
| DEC-015 | 2026-09-22 | How the dashboard reads one consistent snapshot | Claude (technical, during DB-004) | decided | One `REPEATABLE READ READ ONLY` transaction around the seven statements |
| DEC-016 | 2026-09-22 | Application navigation | user (mockup review) | decided | A navbar in the shared header on every screen — Dashboard, Production orders, New production order — replacing the dashboard's page actions; supersedes DEC-012 |
| DEC-017 | 2026-09-22 | Server and database health indicator on the dashboard | user (mockup review) | decided | A status pill checked on load and every 30 s through a new authenticated endpoint that pings the database; text states, no operational detail exposed |
| DEC-018 | 2026-09-22 | Chart full screen | user (mockup review) | decided | Each chart can be maximized to a full-window overlay and restored (Escape or Restore); no browser Fullscreen API |

## DEC-001: Which current-state widgets the dashboard shows

### Context

WI-001 DEC-010 locked Screen C as "the dashboard" but left its widgets and metrics to this work item's requirements step. The widget set decides the aggregate queries, the API shape and the whole screen layout, so it is settled before basic design.

### Options considered

Four current-state widgets were offered, each computable from the orders' present status and dates with no schema change: status count tiles; an overdue & due-soon list; a due-date workload chart (active orders by due week); top products by open quantity. The question was multi-select.

### Decision and rationale

- **Decision:** all four (REQ-029–REQ-032).
- **Decided by:** user, 2026-09-22 (Screen C scope questions: "let do all 4").
- **Rationale:** together they answer the three questions a planner opens a dashboard with — where things stand, what is late, where the load is — and they exercise different aggregate shapes (group-by status, filtered top-N lists, date bucketing, group-by product with ranking).

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-029–REQ-032 |
| BD-003, DD-003 | One section per widget; layout on PC and SP |
| DB-004 | Aggregate queries and their index support |

## DEC-002: Whether the dashboard includes history-based metrics

### Context

The schema (DB-002) holds only each order's current status and its `created_at_utc`/`updated_at_utc`. Any metric that looks back over time — what was completed when, whether it was on time — needs the completion moment, which is not stored. The user was asked whether to stay with current-state metrics (no schema change) or add completion tracking.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Current state only | No schema change; WI-002 untouched | No delivery-performance view; a dashboard of snapshots only |
| Add completion tracking | Enables delivery metrics (output, on-time, trend, lead time); a realistic dashboard | Changes WI-002's table and Screen A's save path; needs a backfill |

### Decision and rationale

- **Decision:** add completion tracking, and show all four history metrics offered: completed this period, on-time completion rate, completion trend chart, average lead time (REQ-033–REQ-037).
- **Decided by:** user, 2026-09-22 ("Yes, add completion tracking"; follow-up: all four metrics selected).
- **Rationale:** the delivery view is what makes a production dashboard useful, and carrying a schema change back into an existing screen is itself worth demonstrating in the lifecycle.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-033–REQ-037 |
| DB-002 → DB-004 | New column, constraint, backfill and seed (DEC-003, DEC-004) |
| BD-001, DD-001 set | Screen A's save records the completion time on `InProgress → Completed` |

## DEC-003: How completion is tracked in the database

### Context

Follows from DEC-002. The four history metrics need only "when did this order become `Completed`".

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| `completed_at_utc` column on `production_orders` | Smallest change that supports all four metrics; `Completed` is terminal and reached once, so one column is enough | No started-at, cancelled-at or audit trail |
| Status-history table | Every transition with time and user; enables more metrics and an audit trail | Larger change to Screen A's save path; more to design and test than the metrics need |

### Decision and rationale

- **Decision:** one nullable `completed_at_utc timestamptz` column on `production_orders`, set by the application on the `InProgress → Completed` transition (REQ-033). The exact constraint (e.g. a check tying it to `status = 'Completed'`) is settled in DB-004.
- **Decided by:** user, 2026-09-22 ("completed_at column").
- **Rationale:** it meets every chosen metric with the least change to WI-002, and it fits the existing state machine, where `Completed` is terminal.

### Impact

| Artifact | Change required |
| --- | --- |
| DB-004 | Column, constraint, migration and index design |
| BD-001, DD-001-FN | The transition that sets it |
| Brief "Not doing" | Status history, started-at and cancelled-at are out of scope |

## DEC-004: What happens to orders already `Completed`

### Context

When the column is added, some orders are already `Completed`: 16 of the 80 seeded demo orders (DB-003), plus any a user completed by hand. Without a value they would either break the constraint or be invisible to the history widgets.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Seed + backfill | History widgets have data from the first demo; the constraint can hold for every row | Backfilled times are approximations |
| Leave NULL and exclude | No invented values | History widgets start empty; the constraint cannot be "completed ⇔ has a time" |

### Decision and rationale

- **Decision:** seeded demo orders receive plausible completion dates (and, where needed, adjusted creation dates so lead times are positive); any other `Completed` order is backfilled from its `updated_at_utc` — for a completed order, the last update is the completion save or later. Exact seeded values are settled in DB-004.
- **Decided by:** user, 2026-09-22 ("Seed + backfill").
- **Rationale:** the demo needs populated history widgets, and `updated_at_utc` is the best available evidence for a real order.

### Impact

| Artifact | Change required |
| --- | --- |
| DB-004 | Backfill statement, seed values, and whether more completed demo orders are needed for a readable 12-week trend |
| REQ-033, REQ-037 | Backfilled and seeded rows must satisfy "no negative lead time" |

## DEC-005: Where the dashboard lives

### Context

WI-003 DEC-004 kept the placeholder home page at `/` so that Screen C could take it over.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Replace the home page at `/` | The dashboard is what a user sees on sign-in; the landing route changes once | The placeholder home's links must move to the header |
| Own route `/dashboard` | No change to `/` | One more click; the placeholder home stays a dead end |

### Decision and rationale

- **Decision:** the dashboard replaces the placeholder home page at `/` (REQ-028).
- **Decided by:** user, 2026-09-22 ("Replace the home page `/`").
- **Rationale:** it completes the plan WI-003 DEC-004 set up.

### Impact

| Artifact | Change required |
| --- | --- |
| BD-003 | Screen transition from login; where the list and "New production order" links live |
| Frontend | The `/` route and the placeholder home component |
| Screens A/B | Any link or redirect that targets `/` is checked for the new meaning |

## DEC-006: Whether widgets drill down

### Context

Screen B keeps its filters in the URL (WI-003 REQ-027), so a widget could open a pre-filtered list. The user was asked whether to use that.

### Decision and rationale

- **Decision:** no — the dashboard is read-only; widgets do not link into Screen B or Screen A (REQ-039).
- **Decided by:** user, 2026-09-22 ("No, read-only").
- **Rationale:** keeps Screen C's scope to display; navigation stays with the normal application links.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-039; drill-down listed under "Not doing" |

## DEC-007: What "due soon" means

### Decision and rationale

- **Decision:** an active order is *due soon* when its due date is from today to today + 7 days inclusive, plant timezone (REQ-030). An overdue order is not also due soon.
- **Decided by:** user, 2026-09-22 ("Next 7 days").
- **Rationale:** a one-week look-ahead matches a weekly planning rhythm.

## DEC-008: Windows and sizes each metric uses

### Context

The user chose each metric (DEC-001, DEC-002) but not its exact window or size. These decide what every test asserts and what the demo seed must contain, so they were fixed before BD-003. A proposal was shown with plan revision 1; the user answered each parameter separately.

### Options considered and decision

| Parameter | Options offered | Decision |
| --- | --- | --- |
| Rows shown per overdue / due-soon group (REQ-030) | 10 (proposed), 5, 20 | **10**, with the group's total count |
| Workload look-ahead (REQ-031) | 8 weeks (proposed), 4, 12 | **8 weeks**: one "overdue" bar + the current week + the next 7 weeks |
| Top products (REQ-032) | 5 (proposed), 10 | **Top 10** |
| Performance window for on-time rate (REQ-035) and lead time (REQ-037) | last 30 days (proposed), last 90 days, current month | **Last 30 days**, today included: completion date from today − 29 to today |
| Week (REQ-031, REQ-034, REQ-036) | Monday–Sunday (proposed), Sunday–Saturday | **Monday–Sunday** (ISO week), plant timezone |
| Month (REQ-034) | calendar month (proposed), rolling 30 days | **Calendar month**, from the 1st to today, plant timezone |
| Trend length (REQ-036) | 12 weeks (proposed), 8, 26 | **12 weeks**, the current week included |

- **Decided by:** user, 2026-09-22, answering DEC-008 after approving plan revision 1 ("plan revision 1 is approved, let answer the DEC-008"). Every value is the proposal except top-products, where the user chose 10 over the proposed 5.
- **Rationale:** a weekly planning rhythm (Monday-start weeks, 8-week look-ahead, 12-week trend), a performance window recent enough to react and wide enough to be stable, and a top-products list long enough to cover a third of the 30 seeded products.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-030–REQ-032, REQ-034–REQ-037 reference these values; open question closed |
| BD-003, DD-003 | Each metric's definition states its window once |
| DB-004 | Bucketing boundaries and the seed must populate an 8-week look-ahead, 10 ranked products and a 12-week trend |

## DEC-009: Active orders due after the workload look-ahead

### Context

Found while applying DEC-008 to REQ-031. With an "overdue" bar and 8 week bars, an active order due more than 8 weeks out fell into no bar, which contradicted the brief's "an order is counted in exactly one bar".

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Add a "later" bar | Every active order is counted once; the chart total matches the active-order count and the status tiles | One more bar, of a different kind than the week bars |
| Leave them out | Only true weeks on the chart | The chart silently under-counts the open workload |

### Decision and rationale

- **Decision:** the chart has an "overdue" bar, 8 week bars (the current week and the next 7) and a "later" bar for active orders due after the last week (REQ-031).
- **Decided by:** user, 2026-09-22 ("Add a 'Later' bar").
- **Rationale:** the chart's bars sum to the number of active orders (`Draft` + `In progress` tiles), which is a check a reviewer can make by eye.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-031 success and failure criteria |
| BD-003, DB-004, DD-003 | Ten buckets, not nine |

## DEC-010: How charts are drawn and made accessible

### Context

REQ-031 and REQ-036 need two bar charts. The frontend stack has no charting library and no component kit (`ai/rules/frontend.md`); adding a runtime dependency was a stop condition in plan revision 1's risks.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Inline SVG drawn by the page | No new dependency; two simple bar charts are a few dozen lines; full control of labels, contrast and ARIA | Axis and layout code written by hand |
| A charting library (e.g. Recharts) | Less drawing code, tooltips for free | New runtime dependency and bundle weight for two bar charts; tooltip-first designs hide values from keyboard and screen-reader users unless reworked |

### Decision and rationale

- **Decision:** inline SVG bars rendered by React and styled with Tailwind; each bar's value printed as text; each chart `role="img"` with a summarizing accessible name; a **View as table** disclosure exposing a real `<table>` of the same figures (BD-003 §3 items 20–24, E-23).
- **Decided by:** Claude, 2026-09-22, during BD-003 (UI/accessibility detail within the approved scope; no dependency added, so the plan's stop condition does not trigger).
- **Rationale:** the charts are fixed-size bar charts (10 and 12 bars) that need no interaction; avoiding a dependency keeps the stack as confirmed, and printed values plus a table meet WCAG 1.1.1 and 1.4.1 without relying on hover.

### Impact

| Artifact | Change required |
| --- | --- |
| BD-003 | Items 20–24, accessibility NFR |
| DD-003-SPD | Chart component structure, sizing, SP horizontal scroll |

## DEC-011: One snapshot endpoint or one endpoint per widget

### Context

Eight widgets could be served by one request or eight. REQ-028 requires "no stale or partial figures", and the figures are meant to cross-check (the workload bars sum to Draft + In progress).

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| One endpoint, one snapshot | Figures consistent with each other; one round trip; one error state; one "today" for every widget | One slow aggregate delays the whole screen |
| One endpoint per widget | Widgets load independently | Figures can disagree if data changes between requests; eight loading and error states; eight "today"s across midnight |

### Decision and rationale

- **Decision:** one read-only endpoint returning every widget's figures computed from one consistent read with a single plant-local today, plus the snapshot time (BD-003 FN-017). How the consistent read is achieved (transaction isolation or statement shape) is settled in DB-004.
- **Decided by:** Claude, 2026-09-22, during BD-003 (technical).
- **Rationale:** it is the only option that satisfies REQ-028's "no partial figures" and makes the cross-checks in BD-003's consistency NFR hold; at demo volume the aggregate cost is small (to be confirmed in DB-004).

### Impact

| Artifact | Change required |
| --- | --- |
| DD-003-API | One contract covering every figure and its empty value |
| DB-004 | The snapshot read and the cost of all queries together |

## DEC-012: Where the placeholder home page's two links go

### Context

The placeholder home page at `/` holds the only links to "Production orders" and "New production order"; the shared application header has no navigation. The brief's first draft of REQ-028 said "the application header still leads to…", which would change a component every screen shares — beyond plan revision 1's "no change to Screen A's or Screen B's visible behavior".

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Page actions on the dashboard | Same two entry points, same place users already find them; no other screen changes | No global navigation bar |
| Navigation links in the shared header | Reachable from every screen | Visibly changes SCR-001 and SCR-002; outside this plan revision's scope |

### Decision and rationale

- **Decision:** SCR-003 carries "Production orders" and "New production order" as page actions beside its heading (BD-003 items 5, 6). The shared header is unchanged; SCR-001's and SCR-002's "Home" breadcrumb and the header's app-name link keep targeting `/`, which is now the dashboard. REQ-028's wording is corrected to match (brief revision 1, same day).
- **Decided by:** Claude, 2026-09-22, during BD-003 (UI; chosen as the option that stays within the approved scope).
- **Rationale:** keeps the change confined to the screen this work item owns. Header navigation can be proposed later as its own change if wanted.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-028 success criterion: "the dashboard still offers…" instead of "the application header still leads to…" |
| BD-003 | Items 5, 6; screen transition |

## DEC-013: How the demo seed gives the delivery widgets data

### Context

Found while writing DB-004. WI-003's seed has only 16 `Completed` orders, all created at most 49 days before the seed ran, and a completion cannot precede creation. Those orders could fill at most the last 7 of the trend's 12 weeks, and the 30-day on-time rate would rest on about 5 orders. DEC-004 ("seed + backfill") did not say whether the seed may grow, which changes the totals Screen B shows. So the user was asked before DB-004 changed anything.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Add ~40 historical completed orders | Full 12-week trend; a meaningful 30-day rate | Demo total grows from 80; WI-003 figures and tests that assert 80 change |
| Re-date the existing 16 only | Screen B totals unchanged | Trend of 1–2 a week with gaps; rate on ~5 orders |
| Convert some existing orders to `Completed` | Total stays 80 | Screen B's status spread and tests change; thinner active-order widgets |

### Decision and rationale

- **Decision:** both of the first two options. Re-date the 16 existing completed orders, giving them older creation dates and completion times over the last 33 days without changing their due dates. Add 40 historical completed orders completed 0–88 days ago, about 30% of them late (DB-004 "Demo seed data").
- **Decided by:** user, 2026-09-22 ("do both 1 and 2 options").
- **Rationale:** with both, every trend week holds at least 2 completions for any run weekday, and the 30-day rate rests on 30 orders (DB-004 "What the seed produces").

### Impact

| Artifact | Change required |
| --- | --- |
| DB-004 | Seed tables, guards, order-number allocation, recovery limits |
| WI-003 artifacts | DB-003's seed figures and the tests asserting 80 rows, the status spread or `00081` are updated in plan revision 2 |

## DEC-014: Far-due active orders in the seed

### Context

WI-003's seeded active orders are due at most 40 days out. On the run date the workload chart's week 7 and "Later" bars are always empty, and week 6 is empty on a Monday or Tuesday run. That makes DEC-009's "Later" bar undemonstrable. DB-004's own seed check caught the week-6 case.

### Decision and rationale

- **Decision:** the new seed also adds 4 active orders, due T + 42, + 49, + 56 and + 63. They always land in week 6, week 7, Later and Later, whatever the run weekday (DB-004).
- **Decided by:** Claude, 2026-09-22, during DB-004 (technical detail of DEC-013's seed). It is flagged at DB-004 review so the user can object; it adds 4 rows to Screen B's demo total (124 instead of 120).
- **Rationale:** every one of the ten workload bars is non-zero on the run date, so REQ-031 is visibly demonstrable.

## DEC-015: How the dashboard reads one consistent snapshot

### Context

DEC-011 requires every widget to come from one consistent read. BD-003 left the mechanism to DB-004.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| One `REPEATABLE READ READ ONLY` transaction around seven ordinary statements | Each query stays readable and separately testable; PostgreSQL gives every statement the same snapshot; a read-only transaction cannot fail with a serialization error, so no retry | One explicit transaction to manage in the repository |
| One statement with CTEs | Single round trip | Merges seven differently shaped results into one; EF Core cannot map it cleanly |
| No guarantee (`READ COMMITTED`) | Simplest | Figures can disagree, breaking REQ-028 and BD-003's cross-checks |

### Decision and rationale

- **Decision:** one `REPEATABLE READ READ ONLY` transaction (DB-004 "Transactions and concurrency").
- **Decided by:** Claude, 2026-09-22, during DB-004 (technical).
- **Rationale:** consistent figures with ordinary queries and no retry path.

## DEC-016: Application navigation

### Context

Raised by the user on reviewing the DD-003 mockup: the list and New-order links sat as buttons on the dashboard (DEC-012), whereas the user wants a navbar that carries the application's functions and links. DEC-012 had kept navigation off the shared header only to stay inside plan revision 1's "no visible change to Screens A and B".

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Navbar in the shared header, every screen | Consistent navigation everywhere; the current page is marked | Visibly changes SCR-001 and SCR-002; their tests and designs are updated |
| Navbar on the dashboard only | Screens A and B untouched | Navigation differs by screen |

### Decision and rationale

- **Decision:** the shared header (`AppHeader`) gains a navbar on every authenticated screen with Dashboard (`/`), Production orders (`/production-orders`) and New production order (`/production-orders/new`), the current one marked (`aria-current="page"`), collapsing behind a menu button below the `sm` breakpoint. The dashboard's two page-action buttons are removed. Supersedes DEC-012.
- **Decided by:** user, 2026-09-22 (mockup review: "we should add a navbar so the function and link could be in it"; follow-up: "Every screen").
- **Rationale:** navigation belongs to the application, not to one screen.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-028 wording; new REQ-040 |
| BD-003 | Items 5–6 removed, navbar described; screen transition |
| BD-001, BD-002, their DD-SPDs | The shared header now carries navigation (layout only; no behavior of either screen changes) |
| plan.md | Revision 2 widens the design scope to the shared header |

## DEC-017: Server and database health indicator

### Context

Raised by the user on reviewing the mockup. The backend already exposes an anonymous `/health` that returns 200 without checking the database; it serves container liveness and says nothing about the database.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Status pill, polled every 30 s | Stays current while the dashboard is open; small | One light request per open dashboard every 30 s |
| Status pill, on load only | Simplest | Goes stale while the page stays open |
| Detailed health panel | Response times, per-component detail | More to design; shows operational detail to every Operator |

### Decision and rationale

- **Decision:** a small indicator on the dashboard showing the server's and the database's status as text ("OK", and a failure state), checked on load and every 30 seconds through a **new authenticated** endpoint that runs a trivial database round trip with a short timeout. No version, hostname, connection string, latency figure or error text is exposed. The existing anonymous `/health` is left unchanged for container liveness. Exact states, timeout and endpoint path are fixed in BD-003 v2 and DD-003-API v2.
- **Decided by:** user, 2026-09-22 (mockup review: "add a heath check indicator on the dashboard to show the server and db status"; follow-up: "Status pill, polled").
- **Rationale:** keeps the indicator honest while the page is open, at negligible cost, without widening what an Operator can learn about the infrastructure.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | New REQ-041 |
| BD-003, DD-003 set | Indicator item, states, polling; new endpoint contract, security and observability |
| DB-004 | No schema change; the ping is `SELECT 1` |

## DEC-018: Chart full screen

### Context

Raised by the user on reviewing the mockup: "the chart should be able to go full screen and minimized".

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Maximize / restore in a full-window overlay | Works the same on desktop and phone; no browser permission prompt; the table can sit beside the enlarged chart | Not true OS full screen |
| Browser Fullscreen API plus a collapse control | True full screen | Browser-dependent (iOS Safari support is limited); focus handling and exit differ by browser |
| Both maximize and collapse | Most flexible | Two controls per chart; collapsed state to remember |

### Decision and rationale

- **Decision:** each chart card has an **Expand** control that opens the chart in a full-window overlay (a modal dialog: focus moves in, Escape or **Restore** closes it, focus returns to Expand) with the chart enlarged and its table shown alongside. "Minimized" is the restored, normal card. No collapse-to-title-bar and no browser Fullscreen API.
- **Decided by:** user, 2026-09-22 (mockup review; follow-up: "Maximize / restore").
- **Rationale:** consistent across devices and accessible as a standard dialog.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | New REQ-042 |
| BD-003, DD-003, DD-003-SPD | Chart controls, overlay, focus management, a new event |
