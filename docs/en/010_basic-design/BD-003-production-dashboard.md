<!-- Based on ai/templates/basic-design.md (revision at commit c3747ed). -->

# Production Dashboard — Basic Design Document (基本設計書)

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | BD-003 |
| Category | UI |
| System name | ProductionManagementAI |
| Subsystem name | Production orders |
| Work item | WI-004 |
| Based on brief.md revision | 1 |
| Created by | Claude (for ThanhTN) |
| Created date | 2026-09-22 |
| Last updated by | Claude (for ThanhTN) |
| Last updated date | 2026-09-22 |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | 2026-09-22 | Claude (for ThanhTN) | Initial creation (Screen C, production dashboard; WI-004 DEC-001–DEC-012) |

## System overview

One screen, SCR-003, is the production dashboard at `/`: the page a signed-in Admin or Operator lands on after login, replacing WI-001's placeholder home page (DEC-005). It shows eight read-only widgets computed from one consistent snapshot of the production orders — four about the current state (status counts, overdue and due-soon orders, the open workload by due week, the products with the most open quantity) and four about delivery (orders completed this week and month, the on-time completion rate, the weekly completion trend, the average lead time). It is "Screen C" on the locked roadmap. SCR-003 changes no data and links into no other screen from its widgets (DEC-006).

The delivery widgets depend on a new fact about each order — when it was completed — recorded by SCR-001 (BD-001) at the moment it saves the `InProgress → Completed` transition (DEC-002, DEC-003). That rule belongs to Screen A and is specified there, in BD-001's next version (plan revision 1, step 5); this document only consumes it.

| Requirement ID | Description | Covered by section |
| --- | --- | --- |
| REQ-028 | The dashboard is the landing page at `/`, with a failure and an empty state | Screen list and screen transition; 0-1; §1 items 3–6; §6 E-20, E-21; Success and exception flows |
| REQ-029 | Status count tiles | §3 items 7–11; §4 M-11 |
| REQ-030 | Overdue and due-soon lists | Metric definitions D-01, D-02; §3 items 16–19; §4 M-11, M-12, M-15 |
| REQ-031 | Due-date workload chart with overdue, 8 week and later bars | D-03; §3 items 20, 21; §4 M-16; DEC-009, DEC-010 |
| REQ-032 | Top 10 products by open quantity | D-04; §3 item 22; §4 M-12 |
| REQ-033 | The completion time is recorded | Data design overview; Actions and business rules (consumed here, specified in BD-001) |
| REQ-034 | Orders completed this week and this month | D-05, D-06; §3 items 12, 13 |
| REQ-035 | On-time completion rate over 30 days | D-07; §3 item 14; §4 M-13 |
| REQ-036 | Weekly completion trend over 12 weeks | D-08; §3 items 23, 24; §4 M-16; DEC-010 |
| REQ-037 | Average lead time over 30 days | D-09; §3 item 15; §4 M-14 |
| REQ-038 | Only authenticated Admin/Operator | 0-1; Actions and business rules; Exception flows |
| REQ-039 | Read-only, no drill-down | §6 (no writing or linking event); Actions and business rules |

## Overall configuration and architecture

Same stack and boundaries as BD-001 and BD-002 (`ai/project.md`, ADR-0001, ADR-0002): a React page behind `ProtectedRoute`, calling the .NET backend's JSON API over the same origin, gated by the same cookie session and the same `Admin`/`Operator` policy server-side. No new trust boundary, external system or runtime dependency is introduced.

SCR-003 calls **one** read-only endpoint that returns every widget's figures together (DEC-011). The server computes "today", the week and month boundaries and every window from the plant clock (`Asia/Tokyo`, WI-002 DEC-011/DEC-017); the browser does no date arithmetic on the figures and never receives order rows beyond the at most 10 + 10 it lists. The response carries the moment the snapshot was taken, which the screen shows. The exact path and contract are fixed in DD-003-API; the queries, their bucketing and their index support in DB-004.

Charts are drawn as inline SVG by the page itself, with no charting library (DEC-010).

## Function list

| Function ID | Function name | Description | Related requirement ID |
| --- | --- | --- | --- |
| FN-017 | Compute the dashboard snapshot | Return every widget's figures from one consistent read of the orders, with the snapshot time and the plant-local today they were computed for | REQ-028, REQ-029–REQ-032, REQ-034–REQ-037 |
| FN-018 | Classify active orders by due date | Place each active order in exactly one of: overdue, due soon (D-01, D-02), and in exactly one workload bar (D-03) | REQ-030, REQ-031 |
| FN-019 | Rank products by open quantity | Sum the quantity of active orders per product and keep the top 10 (D-04) | REQ-032 |
| FN-020 | Compute delivery metrics | From completion times: completed this week and month, on-time rate, weekly trend, average lead time (D-05–D-09) | REQ-034–REQ-037 |
| FN-021 | Record the completion time | On SCR-001's save of `InProgress → Completed`, store the save's UTC time as the order's completion time; set once, never changed. Specified in BD-001 (amends FN-006); listed here for traceability | REQ-033 |
| FN-022 | Backfill completion times | When tracking is introduced, give every already-`Completed` order a completion time: seeded demo orders plausible values, any other its last update time (DEC-004). Specified in DB-004 | REQ-033 |
| FN-023 | Enforce authentication and role | Reject unauthenticated (401) and non-Admin/Operator (403) requests to the screen and its endpoint | REQ-038 |

## Metric definitions

Every definition uses **T**, the plant-local date (`Asia/Tokyo`) at the moment the snapshot is taken, and **W(d)**, the Monday on or before date *d* (weeks run Monday–Sunday, DEC-008). An order is **active** when its status is `Draft` or `InProgress`. The **completion date** of an order is its completion time converted to a plant-local date. Each rule is stated here once; the rest of this document and DD-003 refer to these IDs.

| ID | Figure | Definition | Window / size | Empty value | Requirement |
| --- | --- | --- | --- | --- | --- |
| D-01 | Overdue orders | Active orders with due date < T. Listed by due date ascending, then order number ascending | first 10 rows + total count | "No overdue orders." (MSG-I005), count 0 | REQ-030 |
| D-02 | Due-soon orders | Active orders with T ≤ due date ≤ T + 7 days (DEC-007). Same order as D-01. D-01 and D-02 never overlap | first 10 rows + total count | "No orders due in the next 7 days." (MSG-I006), count 0 | REQ-030 |
| D-03 | Workload by due week | Active orders in 10 bars, each order in exactly one (DEC-009): **Overdue** = due < T; **week k** (k = 0…7) = W(T) + 7k ≤ due ≤ W(T) + 7k + 6 and due ≥ T; **Later** = due ≥ W(T) + 56. Each bar: order count and total quantity. The bars' counts sum to the number of active orders (items 8 + 9) | overdue + current week + 7 weeks + later | every bar 0; MSG-I007 "No open orders." shown over the chart | REQ-031 |
| D-04 | Top products | Per product, the sum of quantity and the count of its active orders; products with no active order excluded; ordered by open quantity descending, then SKU ascending | top 10 | "No open orders." (MSG-I007) | REQ-032 |
| D-05 | Completed this week | Orders with a completion date in W(T) … T: count and total quantity | current Monday–Sunday week to date | 0 and 0 | REQ-034 |
| D-06 | Completed this month | Orders with a completion date from the 1st of T's month to T: count and total quantity | current calendar month to date | 0 and 0 | REQ-034 |
| D-07 | On-time completion rate | Of orders with a completion date in T − 29 … T (*y*), those whose completion date ≤ due date (*x*). Rate = *x* / *y*. `Cancelled` orders have no completion time, so they are in neither *x* nor *y* | last 30 days, today included | "—", with "No orders completed in the last 30 days." (MSG-I008) | REQ-035 |
| D-08 | Completion trend | For each of the 12 Monday–Sunday weeks W(T) − 77 … W(T), the count of orders whose completion date falls in that week. The current week is to date | 12 weeks, current included | every week 0 | REQ-036 |
| D-09 | Average lead time | Over the same orders as D-07's *y*: the mean of (completion time − creation time), in days (elapsed hours ÷ 24), with the number of orders it is based on | last 30 days, today included | "—", with MSG-I008 | REQ-037 |

An order completed on its due date is on time (D-07: ≤). An order completed early is on time however early. Neither the rate nor the lead time is weighted by quantity.

## Actors and business flow

Actors: Admin, Operator — already signed in through WI-001's login screen, whose successful login navigates to `/` (unchanged).

**See where production stands (UC-008)**

1. User signs in, or opens `/` or the app name in the header → system requests the dashboard snapshot (FN-017) and shows a loading state until it returns.
2. System shows the status tiles (count per status and total) and the snapshot time.

**Spot what needs attention (UC-009)**

1. System shows the overdue and due-soon groups (D-01, D-02, FN-018), each with its total and its first 10 orders.
2. System shows the workload chart by due week (D-03), with its table equivalent available.

**See where material and capacity go (UC-010)**

1. System shows the top 10 products by open quantity (D-04, FN-019).

**Judge delivery performance (UC-011)**

1. System shows completed this week and this month, the on-time rate, the average lead time (D-05–D-07, D-09, FN-020) and the 12-week completion trend (D-08).
2. After the user completes an order in SCR-001 and returns to `/`, the completion figures include it (FN-021).

The user leaves the dashboard only through the page's navigation actions (item 5, 6) or the common header — never through a widget (DEC-006).

## Screen list and screen transition

| Screen ID | Screen name | Entry point | Exit / next screen |
| --- | --- | --- | --- |
| SCR-003 | Production Dashboard | `/` — after login (WI-001's login navigates to `/`); the app name in the common header; the "Home" breadcrumb on SCR-001 and SCR-002 | **Production orders** → SCR-002 `/production-orders`. **New production order** → SCR-001 create mode `/production-orders/new`. Session expired / 401 → `/login`. No widget navigates anywhere (DEC-006) |

Screen transition:

```
[/login] --login OK--> [SCR-003 dashboard /] --Production orders--> [SCR-002 list]
                           ^     |                                     |     ^
                           |     +--New production order--> [SCR-001 create/edit]
                           |                                     |           |
                           +------ header app name / "Home" breadcrumb ------+

Any 401 on SCR-003 --> [/login]
```

SCR-003 takes over the route and the two entry points the placeholder home page carried — "Production orders" and "New production order" — as page actions (DEC-012). SCR-001 and SCR-002 are unchanged: their "Home" breadcrumb and the header's app-name link already target `/`, which is now the dashboard.

## Screen design detail

### SCR-003 Production Dashboard

#### 0-1. Basic information (基本情報)

| No | Item | Content | Reference |
| --- | --- | --- | --- |
| 1 | Route / path | `/` | DEC-005 |
| 2 | API base path | One read-only dashboard endpoint (`/api/dashboard` proposed; exact contract in DD-003-API) | DEC-011 |
| 3 | Character encoding | UTF-8 | |
| 4 | Error page / fallback | Load failure: in-page error panel with a **Retry** action, no figures shown; no separate error route | Exception flows |
| 5 | Responsive | Yes — PC (≥ 640px) and SP (< 640px, Tailwind `sm` breakpoint), as BD-002 | §1 |
| 6 | Authentication required | Yes | REQ-038 |
| 7 | Authorization / role restriction | `Admin`, `Operator` (WI-002 DEC-001) | REQ-038 |
| 8 | Applicable channel(s) | Single web app — not applicable | |

#### 0-2. Page metadata (head)

The app's common head (`src/frontend/index.html`) is not restated.

##### 0-2-1. Title

| No | Title |
| --- | --- |
| 1 | `Dashboard — ProductionManagementAI` |

##### 0-2-2. Base

Not applicable — no `<base>` override.

##### 0-2-3. Link

Not applicable — no screen-specific `<link>` tags; styles are bundled.

##### 0-2-4. Meta

Not applicable — no screen-specific meta tags (internal, authenticated screen).

##### 0-2-5. Style

Not applicable — no inline `<style>` block; all styling via Tailwind classes, including the SVG charts' fills.

##### 0-2-6. Script

Not applicable — no extra `<script>` tags; app JS is bundled, and no charting library is loaded (DEC-010).

#### 0-3. URL parameters

None — the screen takes no URL parameters. Every window is fixed (DEC-008) and computed server-side from the plant clock; user-selectable ranges are out of scope.

#### 1. Layout and mockup

Layout-level sketch only; the rendered per-state mockup belongs in DD-003.

##### PC / desktop

```
+--------------------------------------------------------------------------------+
| (1) ProductionManagementAI                                 (2) user / Sign out  |
+--------------------------------------------------------------------------------+
| (3) Dashboard                  (5) [Production orders] (6) [+ New production order]
| (4) As of 2026-09-22 14:05 (Asia/Tokyo)                                         |
| (6a) [message banner: load error / Retry]                                       |
|                                                                                 |
| Current state                                                                   |
| +--------+ +--------+ +------------+ +-----------+ +-----------+                 |
| |(7)Total| |(8)Draft| |(9)In prog. | |(10)Compl. | |(11)Canc.  |                 |
| |   80   | |   32   | |    24      | |    16     | |     8     |                 |
| +--------+ +--------+ +------------+ +-----------+ +-----------+                 |
|                                                                                 |
| Delivery                                                                        |
| +-------------------+ +-------------------+ +----------------+ +--------------+ |
| |(12) Completed     | |(13) Completed     | |(14) On time    | |(15) Avg lead | |
| |  this week        | |  this month       | |  last 30 days  | |  time 30 days| |
| |  3 orders / 420   | |  9 orders / 1,310 | |  82%  9 of 11  | |  12.4 days   | |
| +-------------------+ +-------------------+ +----------------+ | (11 orders)  | |
|                                                               +--------------+ |
| +--------------------------------------------+ +-----------------------------+ |
| | (16) Overdue (15)                           | | (22) Top products by open   | |
| | Order no. | Product | Qty | Due | Status    | |      quantity               | |
| | PO-2026-… | P-1004  |  40 | 09-18 | In pr.  | | 1 P-1006 Gearbox  960 (4)   | |
| | … up to 10 rows   (17) "10 of 15 shown"     | | 2 P-1001 Bracket  720 (3)   | |
| |                                             | | … up to 10                  | |
| | (18) Due in the next 7 days (12)            | |                             | |
| | … up to 10 rows   (19) "10 of 12 shown"     | |                             | |
| +--------------------------------------------+ +-----------------------------+ |
| +--------------------------------------+ +-------------------------------------+ |
| | (20) Open workload by due week       | | (23) Completed per week, 12 weeks   | |
| |  ██                                  | |              ▇                      | |
| |  ██ ▇▇ ██ ▅▅ ▃▃ ▃▃ ▂▂ ▂▂ ▁▁ ▅▅        | |  ▂ ▃ ▁ ▅ ▂ ▃ ▆ ▃ ▇ ▅ ▆ ▂              | |
| |  Ovd W0 W1 W2 W3 W4 W5 W6 W7 Later   | |  06-29 …                 09-21       | |
| | (21) [View as table]                 | | (24) [View as table]                 | |
| +--------------------------------------+ +-------------------------------------+ |
+--------------------------------------------------------------------------------+
```

| Item No. | Region / element | Notes (behavior, condition) |
| --- | --- | --- |
| 1 | App header / name | Common app header; the name links to `/` (this screen) |
| 2 | Signed-in user + Sign out | Common app header (WI-001 behavior) |
| 3 | Page heading | "Dashboard" |
| 4 | Snapshot time | "As of {date} {time} (Asia/Tokyo)" — the moment the figures were computed |
| 5 | Production orders | Page action; opens SCR-002 (DEC-012) |
| 6 | New production order | Primary page action; opens SCR-001 create mode (DEC-012) |
| 6a | Message banner | Hidden unless the load failed; carries **Retry**; announced to assistive tech |
| 7–11 | Status tiles | Total and one per status, M-11 labels |
| 12–15 | Delivery tiles | D-05, D-06, D-07, D-09 |
| 16–19 | Attention list | Overdue (D-01) and due-soon (D-02) groups, each with its count in the heading and a "{shown} of {total} shown" note when the total exceeds 10 |
| 20, 21 | Workload chart | D-03, ten bars; value labels on the bars; **View as table** disclosure |
| 22 | Top products | D-04, a ranked list |
| 23, 24 | Completion trend chart | D-08, twelve bars; **View as table** disclosure |
| 25 | Loading state | Shown in place of every widget while the snapshot loads; no previous figures are left on screen |

##### SP / mobile

```
+------------------------------+
| (1) PMAI        (2) Sign out |
+------------------------------+
| (3) Dashboard                |
| (4) As of 09-22 14:05 JST    |
| (5) [Production orders]      |
| (6) [+ New production order] |
| (6a) [message banner]        |
| +-----------+ +-----------+  |
| |(7) Total  | |(8) Draft  |  |
| +-----------+ +-----------+  |
| |(9) In pr. | |(10) Compl.|  |
| +-----------+ +-----------+  |
| |(11) Canc. |                |
| +-----------+                |
| (12) (13) (14) (15)  2 x 2   |
| (16) Overdue (15)            |
| +--------------------------+ |
| | PO-2026-00002            | |
| | P-1004 Drive shaft       | |
| | Qty 40 · Due 09-18       | |
| | In progress              | |
| +--------------------------+ |
| … (17) (18) (19)             |
| (22) Top products            |
| (20) Workload chart (scrolls |
|      within its card)  (21)  |
| (23) Trend chart       (24)  |
+------------------------------+
```

Same item numbers as PC. Differences: tiles flow two per row; the attention-list rows become one card per order; widgets stack in one column in the order status tiles → delivery tiles → attention list → top products → workload → trend; a chart wider than the screen scrolls horizontally inside its own card, never the page.

#### 2. Content block definition (CMS)

None — no externally managed content blocks.

#### 3. Screen item definition

| No | Item (label) | Variable name | Control type | I/O | Data type | Width / length | Initial value | Placeholder | Display condition | Data source | Reference |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 3 | Page heading | — | label | O | text | — | "Dashboard" | — | Always | — | |
| 4 | Snapshot time | `asOf` | label | O | timestamp | — | — | — | When figures are shown | Snapshot | §4 M-17 |
| 5 | Production orders | — | button (link) | I | — | — | — | — | Always | — | §6 E-22 |
| 6 | New production order | — | button (link) | I | — | — | — | — | Always | — | §6 E-22 |
| 6a | Message banner | — | label + button | O | text | — | Hidden | — | On load failure or 403 | — | §6 E-21 |
| 7 | Total orders | `statusCounts.total` | label (tile) | O | integer | — | — | — | When figures are shown | FN-017 | Sum of items 8–11 |
| 8 | Draft | `statusCounts.draft` | label (tile) | O | integer | — | — | — | as item 7 | FN-017 | §4 M-11 |
| 9 | In progress | `statusCounts.inProgress` | label (tile) | O | integer | — | — | — | as item 7 | FN-017 | §4 M-11 |
| 10 | Completed | `statusCounts.completed` | label (tile) | O | integer | — | — | — | as item 7 | FN-017 | §4 M-11 |
| 11 | Cancelled | `statusCounts.cancelled` | label (tile) | O | integer | — | — | — | as item 7 | FN-017 | §4 M-11 |
| 12 | Completed this week | `completedThisWeek` | label (tile) | O | count + quantity | — | — | — | as item 7 | FN-020, D-05 | §4 M-18 |
| 13 | Completed this month | `completedThisMonth` | label (tile) | O | count + quantity | — | — | — | as item 7 | FN-020, D-06 | §4 M-18 |
| 14 | On time, last 30 days | `onTime` | label (tile) | O | percentage + counts | — | — | — | as item 7 | FN-020, D-07 | §4 M-13 |
| 15 | Average lead time, last 30 days | `leadTime` | label (tile) | O | decimal days + count | — | — | — | as item 7 | FN-020, D-09 | §4 M-14 |
| 16 | Overdue (group) | `overdue` | table (PC) / cards (SP) | O | order rows + total | ≤ 10 rows | — | — | as item 7 | FN-018, D-01 | §4 M-11, M-12, M-15; MSG-I005 |
| 17 | Overdue — shown note | — | label | O | text | — | — | — | When the overdue total > 10 | `overdue.total` | §4 M-19 |
| 18 | Due soon (group) | `dueSoon` | table (PC) / cards (SP) | O | order rows + total | ≤ 10 rows | — | — | as item 7 | FN-018, D-02 | §4 M-11, M-12, M-15; MSG-I006 |
| 19 | Due soon — shown note | — | label | O | text | — | — | — | When the due-soon total > 10 | `dueSoon.total` | §4 M-19 |
| 20 | Workload chart | `workload` | chart (SVG bars) | O | 10 buckets × (count, quantity) | — | — | — | as item 7 | FN-018, D-03 | §4 M-16; DEC-009, DEC-010 |
| 21 | Workload — View as table | — | disclosure button + table | I/O | — | — | Collapsed | — | as item 7 | same as item 20 | §6 E-23 |
| 22 | Top products | `topProducts` | ordered list / table | O | ≤ 10 × (product, open qty, active orders) | ≤ 10 rows | — | — | as item 7 | FN-019, D-04 | §4 M-12; MSG-I007 |
| 23 | Completion trend chart | `completionTrend` | chart (SVG bars) | O | 12 weeks × count | — | — | — | as item 7 | FN-020, D-08 | §4 M-16; DEC-010 |
| 24 | Trend — View as table | — | disclosure button + table | I/O | — | — | Collapsed | — | as item 7 | same as item 23 | §6 E-23 |
| 25 | Loading state | — | label | O | text | — | Shown on load | — | While the snapshot is loading | — | §6 E-20 |

The attention-list rows (items 16, 18) show, in order: order number, product, quantity, due date and status — plain text, not links (DEC-006).

#### 4. Item value mapping

Message and mapping IDs continue the catalogs of BD-001/BD-002: M-11 onward, MSG-E021 and MSG-I005 onward.

| No | Item | Source value | Displayed value | Reference |
| --- | --- | --- | --- | --- |
| M-11 | (8–11) tile labels / (16)(18) row status | `Draft` / `InProgress` / `Completed` / `Cancelled` | Draft / In progress / Completed / Cancelled — BD-001 M-01's labels; the row status badge follows BD-002 M-07 (text carries the meaning, never color alone) | REQ-029, REQ-030 |
| M-12 | (16)(18)(22) product | Product SKU and name | "{SKU} — {name}" (BD-001 M-03) | REQ-030, REQ-032 |
| M-13 | (14) On-time rate | *x*, *y* (D-07) | "{round(100·x/y)}%" rounded half up to a whole percent, with "{x} of {y} on time" beneath; *y* = 0 → "—" and MSG-I008 | REQ-035 |
| M-14 | (15) Average lead time | Mean in days, order count (D-09) | "{mean, one decimal} days" with "based on {n} orders" beneath (singular "day"/"order" when 1.0 / 1); *n* = 0 → "—" and MSG-I008 | REQ-037 |
| M-15 | (16)(18) due date | Stored plant-local date | Shown as-is (`YYYY-MM-DD`), no timezone conversion, as BD-002 M-10. Overdue rows need no separate marker: the group heading says they are overdue | REQ-030 |
| M-16 | (20)(23) bar labels | Bucket start dates | Workload: "Overdue", "This week", then the Monday date of each following week ("MM-DD"), then "Later". Trend: the Monday date of each week, the last labelled "This week". Each bar shows its value as text; the table equivalent gives the full date range of each week (`YYYY-MM-DD – YYYY-MM-DD`) and, for workload, both count and quantity | REQ-031, REQ-036 |
| M-17 | (4) Snapshot time | Snapshot UTC timestamp | Plant-local date and time `YYYY-MM-DD HH:mm (Asia/Tokyo)` — the same zone the figures were computed in, so "today" on the screen and in the figures always agree | |
| M-18 | (12)(13) completed tiles, (7–11)(20)(22) quantities | Count, quantity sum | "{count} orders" with "{quantity} units" beneath; numbers use thousands separators | REQ-034 |
| M-19 | (17)(19) shown note | Group total | "Showing 10 of {total}" when total > 10; hidden otherwise | REQ-030 |

#### 5. Validation rules

Not applicable — SCR-003 has no input field and its endpoint takes no parameters (§0-3); there is nothing for the user or a client to submit. A query string sent to the endpoint is ignored and cannot change a window or a size.

#### 6. Item events

| No | Item | Event | Event content | Reference |
| --- | --- | --- | --- | --- |
| E-20 | Screen | load | Request the snapshot (FN-017); show item 25 while in flight; on success render every widget from that one response; on 401 go to `/login`; on 403 show MSG-E021; on any other failure show item 6a with MSG-E013 and **Retry**, and no figures | REQ-028, REQ-038 |
| E-21 | (6a) Retry | click | Request the snapshot again, showing item 25 | REQ-028 |
| E-22 | (5) Production orders / (6) New production order | click | Open `/production-orders` (SCR-002) / `/production-orders/new` (SCR-001 create mode) | DEC-012 |
| E-23 | (21)(24) View as table | click / Enter / Space | Toggle the chart's table equivalent below the chart; the button states whether it will show or hide the table (`aria-expanded`) | REQ-031, REQ-036; DEC-010 |

No event on this screen writes data, and no widget, row, tile or bar navigates (REQ-039, DEC-006). Reloading the page, or returning to `/`, takes a new snapshot.

#### 7. External identity linkage

None — no external identity linkage.

## Actions and business rules

| Action | Trigger | Business rule | Related requirement ID |
| --- | --- | --- | --- |
| Open screen / call the dashboard API | Any request to SCR-003 or its endpoint | Requires a valid session (else 401 → `/login`) and role `Admin` or `Operator` (else 403) — enforced server-side, not only in the UI | REQ-038 |
| Take a snapshot | Screen load, Retry | Every figure comes from one consistent read, so tiles, lists and charts agree with each other (e.g. the workload bars sum to Draft + In progress); T is fixed once per snapshot and every definition D-01–D-09 uses it | REQ-028–REQ-032, REQ-034–REQ-037 |
| Record a completion | SCR-001 saves `InProgress → Completed` | The save's UTC time becomes the order's completion time, in the same save; set once, never changed, never taken from the client. Owned by BD-001 (FN-021) | REQ-033 |
| Show a figure | Rendering | Rates and averages with nothing to average show "—", never 0 or an error; counts with nothing to count show 0 | REQ-028, REQ-035, REQ-037 |
| Navigate | Page actions, header | Only items 5 and 6 and the common header navigate; widgets never do | REQ-039, DEC-006 |

## Success and exception flows

| Flow | Trigger condition | System behavior | Resulting state |
| --- | --- | --- | --- |
| Success — load | Authorized user opens `/` or signs in | Snapshot returned | Every widget populated, snapshot time shown |
| Success — after a completion | An order was completed in SCR-001, user returns to `/` | New snapshot includes it | Completed tile +1, In progress tile −1, completed-this-week/month, trend's current week and (if in the window) on-time and lead-time figures updated |
| Empty — no orders at all | The system holds no production orders | Snapshot of zeros | Tiles 0; attention groups MSG-I005/MSG-I006; workload and top products MSG-I007; on-time and lead time "—" with MSG-I008; trend all zero. No error |
| Empty — no recent completions | No order completed in the last 30 days | D-07 and D-09 have nothing to measure | Items 14 and 15 show "—" with MSG-I008; other widgets unaffected |
| Exception — unauthenticated | No valid session (including expiry) | 401, no figures returned | Redirect to `/login` |
| Exception — forbidden | Signed in without `Admin`/`Operator` | 403, no figures returned | Error panel MSG-E021 "You don't have permission to view the dashboard." |
| Exception — load failure | Server, database or network error | Nothing shown as figures | Error banner MSG-E013 with **Retry**; page actions 5 and 6 still usable |

## Data design overview

SCR-003 reads **ProductionOrder** (status, quantity, due date, creation time, and the new completion time) joined to **Product** (SKU, name). It adds no entity.

What database design has to add (DB-004), rather than restate:

- **Completion time** on ProductionOrder: a nullable UTC timestamp, present exactly when the status is `Completed`, never earlier than the order's creation time (so no lead time is negative, REQ-037), set by SCR-001's save (DEC-003).
- **Backfill** for orders already `Completed`: seeded demo orders get plausible completion times — spread over the 12-week trend window, some on time and some late — and any other completed order takes its last update time (DEC-004). Where a seeded order's creation time would fall after its completion time, the seed adjusts it.
- **Seed coverage** so every widget shows a non-trivial figure on the demo data: orders completed this week and this month, in most of the 12 trend weeks, both on time and late in the last 30 days; active orders overdue, due soon, in several of the 8 weeks and later; more than 10 products with open quantity. Whether the existing 80 seeded orders suffice or more are needed is decided in DB-004.
- **The aggregate queries** for D-01–D-09 in one consistent snapshot, bucketed by plant-local date, with an index or cost argument for each.

## External interfaces

None — only this application's own backend API.

## Non-functional requirements

- **Security:** the role check gates the dashboard endpoint server-side (REQ-038). The endpoint takes no parameters, so there is no input to validate or bind; every window is a server constant. It returns aggregates plus at most 20 order rows, never an unbounded set. No figure is PII or a secret. The completion time is written only by the server during SCR-001's save and is never accepted from a request (REQ-033). CSRF is not a concern for this read-only `GET`; the WI-002 posture is unchanged (WI-002 DEC-020).
- **Consistency:** all figures come from one snapshot (DEC-011), so a reviewer can cross-check them — the tiles sum to the total; the workload bars sum to Draft + In progress; the Completed tile is at least the completed-this-month count.
- **Accessibility (WCAG 2.2 AA, `ai/rules/frontend.md`):** each widget is a titled region (`section` with a heading) so the page can be navigated by headings; tile values are text, not images; each chart is an SVG with `role="img"` and an accessible name summarizing it (e.g. "Open workload by due week: 15 overdue, 9 this week, … 5 later"), every bar carries its value as visible text, and a **View as table** disclosure exposes a real `<table>` with the same figures (DEC-010); bars never rely on color alone; the attention lists are real tables with captions on PC; the loading state and the error banner are announced; the page actions are real links with visible focus indicators; contrast meets AA for text and for bars against their background (non-text contrast 3:1).
- **Performance:** one request per load; a bounded number of aggregate queries on the server, each index-backed or cheap at the expected volume (DB-004). Demo target: the dashboard rendered well within a second on the seeded data.
- **Observability:** the endpoint is traced and logged per project defaults, as BD-001's and BD-002's are; exact spans and metrics in DD-003-FN.
- **Time:** every date boundary uses the plant clock (`Asia/Tokyo`), never the server's or the browser's zone; tests pin the clock to exercise week, month and 30-day boundaries.
- Availability: inherits project defaults.

## Open questions and linked DD

| Question | Linked DD section | Status |
| --- | --- | --- |
| Which widgets and metrics | DD-003 | answered in decisions.md (DEC-001, DEC-002) |
| Windows and sizes | DD-003, DB-004 | answered in decisions.md (DEC-008, DEC-009) |
| Chart rendering approach and accessible equivalent | DD-003-SPD | answered in decisions.md (DEC-010) — inline SVG, no library, table disclosure |
| One snapshot endpoint or one endpoint per widget | DD-003-API | answered in decisions.md (DEC-011) — one endpoint, one snapshot |
| Where the placeholder home's two links go | DD-003-SPD | answered in decisions.md (DEC-012) — page actions on SCR-003; the shared header is unchanged |
| How the one-snapshot read is achieved (transaction isolation or a single statement) | DD-003-FN; DB-004 | open — DB-004 decides |
| Exact wording of MSG-E021 and MSG-I005–MSG-I008 | DD-003 message list | drafted here; confirmed in DD-003 |
