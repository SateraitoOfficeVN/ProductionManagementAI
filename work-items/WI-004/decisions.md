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
| DEC-010 | 2026-09-22 | How charts are drawn and made accessible | Claude (UI/accessibility, during 003_BD) | decided | Inline SVG drawn by the page, no charting library; value labels on bars and a "View as table" disclosure |
| DEC-011 | 2026-09-22 | One snapshot endpoint or one endpoint per widget | Claude (technical, during 003_BD) | decided | One endpoint returning every widget from one consistent snapshot |
| DEC-012 | 2026-09-22 | Where the placeholder home page's two links go | Claude (UI, during 003_BD) | superseded by DEC-016 | Page actions on the dashboard itself; the shared header is not changed |
| DEC-013 | 2026-09-22 | How the demo seed gives the delivery widgets data | user | decided | Both: re-date the 16 seeded completed orders and add 40 historical completed orders |
| DEC-014 | 2026-09-22 | Whether the seed adds active orders due beyond WI-003's 40-day horizon | Claude (technical, during 003_DB) | decided — 003_DB approved without objection | Add 4 (due +42, +49, +56, +63), so week bars 6–7 and Later are never empty |
| DEC-015 | 2026-09-22 | How the dashboard reads one consistent snapshot | Claude (technical, during 003_DB) | decided | One `REPEATABLE READ READ ONLY` transaction around the seven statements |
| DEC-016 | 2026-09-22 | Application navigation | user (mockup review) | decided | A navbar in the shared header on every screen — Dashboard, Production orders, New production order — replacing the dashboard's page actions; supersedes DEC-012 |
| DEC-017 | 2026-09-22 | Server and database health indicator on the dashboard | user (mockup review) | decided | A status pill checked on load and every 30 s through a new authenticated endpoint that pings the database; text states, no operational detail exposed |
| DEC-018 | 2026-09-22 | Chart full screen | user (mockup review) | decided | Each chart can be maximized to a full-window overlay and restored (Escape or Restore); no browser Fullscreen API |
| DEC-019 | 2026-09-22 | Whether the health poll renews the sign-in session | user | decided | Never — the health endpoint is excluded from sliding-expiration renewal |
| DEC-020 | 2026-09-22 | Health states, timeouts and endpoint | Claude (technical, during 003_BD v2) | decided | Server OK/Unreachable (5 s client timeout), Database OK/Unavailable/Unknown (`SELECT 1`, 2 s); `GET /api/system/health`, authenticated |
| DEC-021 | 2026-09-22 | Whether SCR-001/SCR-002 keep their breadcrumbs next to the navbar | Claude (UI, during 003_BD v2) | decided | Kept — they show position, the navbar shows destinations; removing them would change both screens beyond their header |
| DEC-022 | 2026-09-22 | Leaving an edited Screen A form through a navbar (or other in-app) link | user | decided | Ask first with the existing discard dialog, as Cancel does; implemented with a shared navigation guard, not a router migration |
| DEC-023 | 2026-09-22 | Icons in the UI | user (mockup review) | decided | Icons in the navbar and actions, tiles, widget headings and states, and the health indicator, from `lucide-react` (ISC) — a new runtime dependency |
| DEC-024 | 2026-09-22 | How the dashboard reader runs its SQL; the health code's namespace | Claude (technical, during implementation) | decided | ADO.NET commands on the EF connection and transaction with bound parameters, instead of `SqlQuery<T>`; namespace `Health`, not `System` |
| DEC-025 | 2026-09-22 | How the health path is kept from renewing the session | Claude (technical, during implementation) | decided | Suppress renewal both in sliding expiration and after Identity's security-stamp revalidation in `OnValidatePrincipal`; the stamp is still validated |
| DEC-026 | 2026-09-22 | Merge PR #15 | user | decided | Squash-merged into `master` as `cd3a3b9` with all three CI jobs green |

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
| 003_BD, 003_DD | One section per widget; layout on PC and SP |
| 003_DB | Aggregate queries and their index support |

## DEC-002: Whether the dashboard includes history-based metrics

### Context

The schema (001_DB) holds only each order's current status and its `created_at_utc`/`updated_at_utc`. Any metric that looks back over time — what was completed when, whether it was on time — needs the completion moment, which is not stored. The user was asked whether to stay with current-state metrics (no schema change) or add completion tracking.

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
| 001_DB → 003_DB | New column, constraint, backfill and seed (DEC-003, DEC-004) |
| 001_BD, 001_DD set | Screen A's save records the completion time on `InProgress → Completed` |

## DEC-003: How completion is tracked in the database

### Context

Follows from DEC-002. The four history metrics need only "when did this order become `Completed`".

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| `completed_at_utc` column on `production_orders` | Smallest change that supports all four metrics; `Completed` is terminal and reached once, so one column is enough | No started-at, cancelled-at or audit trail |
| Status-history table | Every transition with time and user; enables more metrics and an audit trail | Larger change to Screen A's save path; more to design and test than the metrics need |

### Decision and rationale

- **Decision:** one nullable `completed_at_utc timestamptz` column on `production_orders`, set by the application on the `InProgress → Completed` transition (REQ-033). The exact constraint (e.g. a check tying it to `status = 'Completed'`) is settled in 003_DB.
- **Decided by:** user, 2026-09-22 ("completed_at column").
- **Rationale:** it meets every chosen metric with the least change to WI-002, and it fits the existing state machine, where `Completed` is terminal.

### Impact

| Artifact | Change required |
| --- | --- |
| 003_DB | Column, constraint, migration and index design |
| 001_BD, 001_DD-FN | The transition that sets it |
| Brief "Not doing" | Status history, started-at and cancelled-at are out of scope |

## DEC-004: What happens to orders already `Completed`

### Context

When the column is added, some orders are already `Completed`: 16 of the 80 seeded demo orders (002_DB), plus any a user completed by hand. Without a value they would either break the constraint or be invisible to the history widgets.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Seed + backfill | History widgets have data from the first demo; the constraint can hold for every row | Backfilled times are approximations |
| Leave NULL and exclude | No invented values | History widgets start empty; the constraint cannot be "completed ⇔ has a time" |

### Decision and rationale

- **Decision:** seeded demo orders receive plausible completion dates (and, where needed, adjusted creation dates so lead times are positive); any other `Completed` order is backfilled from its `updated_at_utc` — for a completed order, the last update is the completion save or later. Exact seeded values are settled in 003_DB.
- **Decided by:** user, 2026-09-22 ("Seed + backfill").
- **Rationale:** the demo needs populated history widgets, and `updated_at_utc` is the best available evidence for a real order.

### Impact

| Artifact | Change required |
| --- | --- |
| 003_DB | Backfill statement, seed values, and whether more completed demo orders are needed for a readable 12-week trend |
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
| 003_BD | Screen transition from login; where the list and "New production order" links live |
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

The user chose each metric (DEC-001, DEC-002) but not its exact window or size. These decide what every test asserts and what the demo seed must contain, so they were fixed before 003_BD. A proposal was shown with plan revision 1; the user answered each parameter separately.

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
| 003_BD, 003_DD | Each metric's definition states its window once |
| 003_DB | Bucketing boundaries and the seed must populate an 8-week look-ahead, 10 ranked products and a 12-week trend |

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
| 003_BD, 003_DB, 003_DD | Ten buckets, not nine |

## DEC-010: How charts are drawn and made accessible

### Context

REQ-031 and REQ-036 need two bar charts. The frontend stack has no charting library and no component kit (`ai/rules/frontend.md`); adding a runtime dependency was a stop condition in plan revision 1's risks.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Inline SVG drawn by the page | No new dependency; two simple bar charts are a few dozen lines; full control of labels, contrast and ARIA | Axis and layout code written by hand |
| A charting library (e.g. Recharts) | Less drawing code, tooltips for free | New runtime dependency and bundle weight for two bar charts; tooltip-first designs hide values from keyboard and screen-reader users unless reworked |

### Decision and rationale

- **Decision:** inline SVG bars rendered by React and styled with Tailwind; each bar's value printed as text; each chart `role="img"` with a summarizing accessible name; a **View as table** disclosure exposing a real `<table>` of the same figures (003_BD §3 items 20–24, E-23).
- **Decided by:** Claude, 2026-09-22, during 003_BD (UI/accessibility detail within the approved scope; no dependency added, so the plan's stop condition does not trigger).
- **Rationale:** the charts are fixed-size bar charts (10 and 12 bars) that need no interaction; avoiding a dependency keeps the stack as confirmed, and printed values plus a table meet WCAG 1.1.1 and 1.4.1 without relying on hover.

### Impact

| Artifact | Change required |
| --- | --- |
| 003_BD | Items 20–24, accessibility NFR |
| 003_DD-SPD | Chart component structure, sizing, SP horizontal scroll |

## DEC-011: One snapshot endpoint or one endpoint per widget

### Context

Eight widgets could be served by one request or eight. REQ-028 requires "no stale or partial figures", and the figures are meant to cross-check (the workload bars sum to Draft + In progress).

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| One endpoint, one snapshot | Figures consistent with each other; one round trip; one error state; one "today" for every widget | One slow aggregate delays the whole screen |
| One endpoint per widget | Widgets load independently | Figures can disagree if data changes between requests; eight loading and error states; eight "today"s across midnight |

### Decision and rationale

- **Decision:** one read-only endpoint returning every widget's figures computed from one consistent read with a single plant-local today, plus the snapshot time (003_BD FN-017). How the consistent read is achieved (transaction isolation or statement shape) is settled in 003_DB.
- **Decided by:** Claude, 2026-09-22, during 003_BD (technical).
- **Rationale:** it is the only option that satisfies REQ-028's "no partial figures" and makes the cross-checks in 003_BD's consistency NFR hold; at demo volume the aggregate cost is small (to be confirmed in 003_DB).

### Impact

| Artifact | Change required |
| --- | --- |
| 003_DD-API | One contract covering every figure and its empty value |
| 003_DB | The snapshot read and the cost of all queries together |

## DEC-012: Where the placeholder home page's two links go

### Context

The placeholder home page at `/` holds the only links to "Production orders" and "New production order"; the shared application header has no navigation. The brief's first draft of REQ-028 said "the application header still leads to…", which would change a component every screen shares — beyond plan revision 1's "no change to Screen A's or Screen B's visible behavior".

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Page actions on the dashboard | Same two entry points, same place users already find them; no other screen changes | No global navigation bar |
| Navigation links in the shared header | Reachable from every screen | Visibly changes SCR-001 and SCR-002; outside this plan revision's scope |

### Decision and rationale

- **Decision:** SCR-003 carries "Production orders" and "New production order" as page actions beside its heading (003_BD items 5, 6). The shared header is unchanged; SCR-001's and SCR-002's "Home" breadcrumb and the header's app-name link keep targeting `/`, which is now the dashboard. REQ-028's wording is corrected to match (brief revision 1, same day).
- **Decided by:** Claude, 2026-09-22, during 003_BD (UI; chosen as the option that stays within the approved scope).
- **Rationale:** keeps the change confined to the screen this work item owns. Header navigation can be proposed later as its own change if wanted.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | REQ-028 success criterion: "the dashboard still offers…" instead of "the application header still leads to…" |
| 003_BD | Items 5, 6; screen transition |

## DEC-013: How the demo seed gives the delivery widgets data

### Context

Found while writing 003_DB. WI-003's seed has only 16 `Completed` orders, all created at most 49 days before the seed ran, and a completion cannot precede creation. Those orders could fill at most the last 7 of the trend's 12 weeks, and the 30-day on-time rate would rest on about 5 orders. DEC-004 ("seed + backfill") did not say whether the seed may grow, which changes the totals Screen B shows. So the user was asked before 003_DB changed anything.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Add ~40 historical completed orders | Full 12-week trend; a meaningful 30-day rate | Demo total grows from 80; WI-003 figures and tests that assert 80 change |
| Re-date the existing 16 only | Screen B totals unchanged | Trend of 1–2 a week with gaps; rate on ~5 orders |
| Convert some existing orders to `Completed` | Total stays 80 | Screen B's status spread and tests change; thinner active-order widgets |

### Decision and rationale

- **Decision:** both of the first two options. Re-date the 16 existing completed orders, giving them older creation dates and completion times over the last 33 days without changing their due dates. Add 40 historical completed orders completed 0–88 days ago, about 30% of them late (003_DB "Demo seed data").
- **Decided by:** user, 2026-09-22 ("do both 1 and 2 options").
- **Rationale:** with both, every trend week holds at least 2 completions for any run weekday, and the 30-day rate rests on 30 orders (003_DB "What the seed produces").

### Impact

| Artifact | Change required |
| --- | --- |
| 003_DB | Seed tables, guards, order-number allocation, recovery limits |
| WI-003 artifacts | 002_DB's seed figures and the tests asserting 80 rows, the status spread or `00081` are updated in plan revision 2 |

## DEC-014: Far-due active orders in the seed

### Context

WI-003's seeded active orders are due at most 40 days out. On the run date the workload chart's week 7 and "Later" bars are always empty, and week 6 is empty on a Monday or Tuesday run. That makes DEC-009's "Later" bar undemonstrable. 003_DB's own seed check caught the week-6 case.

### Decision and rationale

- **Decision:** the new seed also adds 4 active orders, due T + 42, + 49, + 56 and + 63. They always land in week 6, week 7, Later and Later, whatever the run weekday (003_DB).
- **Decided by:** Claude, 2026-09-22, during 003_DB (technical detail of DEC-013's seed). It is flagged at 003_DB review so the user can object; it adds 4 rows to Screen B's demo total (124 instead of 120).
- **Rationale:** every one of the ten workload bars is non-zero on the run date, so REQ-031 is visibly demonstrable.

## DEC-015: How the dashboard reads one consistent snapshot

### Context

DEC-011 requires every widget to come from one consistent read. 003_BD left the mechanism to 003_DB.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| One `REPEATABLE READ READ ONLY` transaction around seven ordinary statements | Each query stays readable and separately testable; PostgreSQL gives every statement the same snapshot; a read-only transaction cannot fail with a serialization error, so no retry | One explicit transaction to manage in the repository |
| One statement with CTEs | Single round trip | Merges seven differently shaped results into one; EF Core cannot map it cleanly |
| No guarantee (`READ COMMITTED`) | Simplest | Figures can disagree, breaking REQ-028 and 003_BD's cross-checks |

### Decision and rationale

- **Decision:** one `REPEATABLE READ READ ONLY` transaction (003_DB "Transactions and concurrency").
- **Decided by:** Claude, 2026-09-22, during 003_DB (technical).
- **Rationale:** consistent figures with ordinary queries and no retry path.

## DEC-016: Application navigation

### Context

Raised by the user on reviewing the 003_DD mockup: the list and New-order links sat as buttons on the dashboard (DEC-012), whereas the user wants a navbar that carries the application's functions and links. DEC-012 had kept navigation off the shared header only to stay inside plan revision 1's "no visible change to Screens A and B".

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
| 003_BD | Items 5–6 removed, navbar described; screen transition |
| 001_BD, 002_BD, their DD-SPDs | The shared header now carries navigation (layout only; no behavior of either screen changes) |
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

- **Decision:** a small indicator on the dashboard showing the server's and the database's status as text ("OK", and a failure state), checked on load and every 30 seconds through a **new authenticated** endpoint that runs a trivial database round trip with a short timeout. No version, hostname, connection string, latency figure or error text is exposed. The existing anonymous `/health` is left unchanged for container liveness. Exact states, timeout and endpoint path are fixed in 003_BD v2 and 003_DD-API v2.
- **Decided by:** user, 2026-09-22 (mockup review: "add a heath check indicator on the dashboard to show the server and db status"; follow-up: "Status pill, polled").
- **Rationale:** keeps the indicator honest while the page is open, at negligible cost, without widening what an Operator can learn about the infrastructure.

### Impact

| Artifact | Change required |
| --- | --- |
| brief.md | New REQ-041 |
| 003_BD, 003_DD set | Indicator item, states, polling; new endpoint contract, security and observability |
| 003_DB | No schema change; the ping is `SELECT 1` |

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
| 003_BD, 003_DD, 003_DD-SPD | Chart controls, overlay, focus management, a new event |

## DEC-019: Whether the health poll renews the sign-in session

### Context

Found while starting plan revision 2. The app uses ASP.NET Core Identity's cookie defaults (`DependencyInjection.cs` sets no `ExpireTimeSpan` or `SlidingExpiration`): a 14-day session renewed on requests once more than half of it has elapsed. A dashboard polling health every 30 seconds would therefore keep its session alive indefinitely, even untouched. Plan revision 2's risks said this would be raised as a question.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| The poll never renews | The session lasts exactly as long as without the dashboard; small, targeted change | A change to the shared cookie configuration (one event handler) |
| Accept renewal | No auth change; suits a wall display | An unattended browser keeps an Admin/Operator session forever |
| Stop polling when idle | No auth change | More client logic; the indicator goes stale when idle |

### Decision and rationale

- **Decision:** requests to the health endpoint never renew the session: the cookie options' `Events.OnCheckSlidingExpiration` sets `ShouldRenew = false` for that path. Every other request, including the dashboard snapshot, renews as before.
- **Decided by:** user, 2026-09-22 ("No — poll never renews").
- **Rationale:** the indicator must not change how long a login lasts.

### Impact

| Artifact | Change required |
| --- | --- |
| 003_BD v2 | Health definitions, E-27, security NFR |
| 003_DD-FN v2 | The cookie event and its test |
| 0002_ADR | None to the decision; the session behaviour it describes is unchanged for every user action |

## DEC-020: Health states, timeouts and endpoint

### Context

DEC-017 left the exact states, timeouts and endpoint to design.

### Decision and rationale

- **Decision:** the browser calls `GET /api/system/health` (authenticated, `ProductionOrderEditor`, `no-store`), which runs `SELECT 1` with a 2-second timeout and returns `{ database: "ok" | "unavailable", checkedAt }` with 200 either way. The browser derives the server's status: OK when that call returns 200 within 5 seconds, Unreachable on no answer, a network failure or a 5xx, and in that case the database is Unknown rather than its last value (003_BD HS-01–HS-05).
- **Decided by:** Claude, 2026-09-22, during 003_BD v2 (technical detail of DEC-017).
- **Rationale:** a 200 with a database verdict separates "the server is up but the database is not" from "nothing answers", which is what the indicator exists to show. A 2-second database timeout keeps a hung database from holding a request for the default 30 seconds. The path sits under `/api` so it goes through the same proxy and authorization as every other call, apart from the anonymous liveness `/health`.

## DEC-021: Breadcrumbs next to the navbar

### Context

With a navbar on every screen, SCR-001's and SCR-002's breadcrumbs ("Home > Production orders > …") partly duplicate navigation.

### Decision and rationale

- **Decision:** keep them. The breadcrumb shows where the user is (and on SCR-001 which order); the navbar shows where they can go. Removing them would change both screens beyond their header, outside plan revision 2's scope.
- **Decided by:** Claude, 2026-09-22, during 003_BD v2 (UI).

## DEC-022: Leaving an edited Screen A form through an in-app link

### Context

Found while writing 001_BD v7: SCR-001's discard-changes confirmation (REQ-019, FN-009) is attached to its Cancel button only (`ProductionOrderForm.tsx`). The breadcrumb and app-name links already leave an edited form silently, and the new navbar would make that the common way out. An earlier draft of the 001_BD v7 and DD-SPD notes wrongly said the confirmation would apply to navbar links; that claim was removed before this question was asked.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| Ask first, like Cancel | Consistent with REQ-019's intent; closes the existing breadcrumb gap | A small change to Screen A's behavior |
| Leave without asking | No Screen A change | The navbar silently discards edits |

### Decision and rationale

- **Decision:** any in-app link that leaves an edited SCR-001 form — navbar, breadcrumb, app name — opens the existing discard dialog. Discard goes to that link's destination; Keep editing stays. Browser Back, reload and closing the tab stay unguarded, as today.
- **Decided by:** user, 2026-09-22 ("Ask first, like Cancel").
- **Mechanism (Claude, technical):** the app uses `BrowserRouter`, so React Router's `useBlocker` (data routers only) is unavailable without migrating the router. Instead a small `NavigationGuard` context is used: the form registers "dirty" and an "ask" callback, and a shared `GuardedLink` used by the header, navbar and breadcrumbs consults it (003_DD module 11).
- **Rationale:** the navbar must not make losing work easier than Cancel does.

### Impact

| Artifact | Change required |
| --- | --- |
| 001_BD v7 | FN-009, E-07a, E-09, business rule |
| 001_DD-SPD v2, 003_DD v3 | `NavigationGuard`, `GuardedLink` |
| Plan revision 2 | Screen A gains this one behavior change beyond its header, by the user's decision |
| Tests (revision 3) | Navbar and breadcrumb with a dirty form → dialog; Discard → destination |

## DEC-023: Icons in the UI

### Context

Raised by the user after reviewing mockup version 2: "please add icons so it more user friendly". The frontend has no icon set; `ai/rules/frontend.md` forbids a component kit or another UI framework without a project decision, and plan revision 1 listed a new runtime dependency as a stop condition.

### Options considered

| Option | Pros | Cons |
| --- | --- | --- |
| `lucide-react` | Open source (ISC); one React component per icon, tree-shaken so only used icons ship; consistent 24-unit stroke style that suits the Tailwind gray look; has server and database glyphs | One new runtime dependency |
| Hand-drawn inline SVG | No dependency | About 30 glyphs to draw and maintain; inconsistent |
| `@heroicons/react` | MIT; from Tailwind Labs | No dedicated server/database glyphs |

### Decision and rationale

- **Decision:** icons in four places — the navbar and actions, the dashboard tiles, the widget headings and state messages, and the health indicator — using `lucide-react`, pinned at an exact version (1.47.0 at the time of design). The mapping is fixed in 003_BD M-21. Icons are decorative (`aria-hidden="true"`) next to visible text everywhere; icon-only controls (Expand, Restore, Menu) keep their accessible names. No state is carried by an icon alone.
- **Decided by:** user, 2026-09-22 (placement: all four options; source: "lucide-react").
- **Rationale:** faster recognition at a glance, without a component kit and without hand-maintained artwork. It is an icon library, not a component kit, so the stack rule's intent is kept; it is still a new dependency and is recorded as such.
- **Scope:** handled inside plan revision 2 as a direct user request editing its in-scope artifacts (003_BD, 003_DD, the mockup), not a new plan revision.

### Impact

| Artifact | Change required |
| --- | --- |
| 003_BD v3 | M-21 icon mapping; accessibility note |
| 003_DD v4 | Dependency, `components/icons.ts` single import point |
| Mockup v3 | Real Lucide glyphs inlined |
| `ai/project.md` | Frontend stack gains `lucide-react` — updated when the dependency is actually added (plan revision 3) |
| Screens A/B | Only through the shared header (navbar, Sign out, Menu); their own bodies are unchanged |

## DEC-024: Dashboard reader mechanics and the health namespace

### Context

Found while implementing plan revision 3, steps 3–4.

### Decision and rationale

- **Reader:** 003_DD-FN §3 specified EF Core's `Database.SqlQuery<T>`. How it maps result columns onto ad-hoc row types under this project's snake-case naming convention is not documented clearly enough to rely on. The reader therefore runs 003_DB's seven statements as ADO.NET commands on the EF connection and its `REPEATABLE READ` transaction, with every value an `NpgsqlParameter`, and maps rows by ordinal. Behavior, SQL text, the snapshot and the parameter binding are exactly as designed; only the call mechanism differs. 003_DD-FN §3 was updated in the same change.
- **Namespace:** 003_DD X-1 row 6a named `Application/System` and `Infrastructure/System`. A namespace `ProductionManagementAI.Application.System` would shadow .NET's `System` namespace inside it, so the folders and namespace are `Health`. 003_DD X-1 was updated.
- **Decided by:** Claude, 2026-09-22 (technical; no behavior change).
- **Verified:** a local smoke run against the migrated Compose database returned exactly 003_DB's predicted figures; the integration tests (TC-203–TC-214) cover the reader.

## DEC-025: Keeping the health poll from renewing the session

### Context

Found by TC-225 during plan revision 3, step 8. 003_DD-FN §7 suppressed renewal only in `OnCheckSlidingExpiration`. The test showed the cookie was still reissued on a health request made 8 days after sign-in. The cause is ASP.NET Core Identity's security-stamp validator: it runs in `OnValidatePrincipal` once its 30-minute interval has passed, and on success it replaces the principal and sets `ShouldRenew = true`. That happens before the sliding-expiration check, so the hook alone could not honour DEC-019.

### Decision and rationale

- **Decision:** keep the sliding-expiration hook, and also wrap Identity's `OnValidatePrincipal`: run the security-stamp validation unchanged, then set `ShouldRenew = false` when the path is the health path. A revoked or changed stamp still rejects the request; only the reissue of the cookie is suppressed. Every other path is unchanged.
- **Decided by:** Claude, 2026-09-22 (technical: the only way to implement the user's DEC-019 as stated; no behavior change beyond it).
- **Verified:** TC-225 now passes. A health request 8 days after sign-in returns no `Set-Cookie`; a dashboard request at the same moment does.
- **Documents:** 003_DD-FN §7 updated in the same change.

## DEC-026: Merge PR #15

- **Decision:** PR #15 squash-merged into `master` as `cd3a3b9`, 2026-09-22.
- **Decided by:** user (merged it themselves; "pr merged").
- **Rationale:** all three CI jobs green on the PR (evidence.md); local verification complete.
