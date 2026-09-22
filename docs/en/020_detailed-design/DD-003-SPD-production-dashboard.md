<!-- Based on ai/templates/DD/screen-processing-design.md (revision at commit c3747ed). -->

# Production Dashboard — Screen Processing Design (画面処理設計)

DD-003-SPD — elaborates DD-003, implements BD-003, requirements REQ-028–REQ-042.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | DD-003-SPD |
| System name | ProductionManagementAI |
| Subsystem name | Production orders |
| Work item | WI-004 |
| Created by | Claude (for ThanhTN) |
| Created date | 2026-09-22 |
| Last updated by | Claude (for ThanhTN) |
| Last updated date | 2026-09-22 |

| Version | Date | Author | Revision content |
| --- | --- | --- | --- |
| 1 | 2026-09-22 | Claude (for ThanhTN) | Initial creation |
| 2 | 2026-09-22 | Claude (for ThanhTN) | Page actions removed; §8 navbar, §9 navigation guard, §10 health polling, §11 maximized chart, §12 health request boundary (DEC-016–DEC-022) |

## Overview and process list

| Field | Value |
| --- | --- |
| Screen / file name | `src/frontend/src/features/dashboard/DashboardPage.tsx` and the components it composes; the server side of the request is in `DashboardController.Get` |
| Overview | How SCR-003 loads its one snapshot, renders each state and each widget, and draws its two charts with their table equivalents — one block per component |

| No | Process name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `DashboardPage` — P-20 Load | Role gate, one request, state selection, Retry | E-20, E-21 |
| 2 | `DashboardPage` — P-21 Response handling | Each HTTP outcome → a screen state | E-20 |
| 3 | `StatusTiles`, `DeliveryTiles` — P-22 Tiles | Figures and their captions, via `dashboardFormat` | Items 7–15 |
| 4 | `AttentionList` — P-23 Overdue and due-soon groups | Two titled tables (cards on SP), "Showing 10 of N" | Items 16–19 |
| 5 | `TopProducts` — P-24 Ranked list | Ordered list of up to 10 | Item 22 |
| 6 | `BarChart` — P-25 Chart and table equivalent | SVG bars, value labels, accessible summary, "View as table" | Items 20–21, 23–24; E-23 |
| 7 | `DashboardController.Get` — request boundary | Auth, delegate, `no-store` | Server steps in DD-003-FN |
| 8 | `AppNavbar` — P-26 Navbar | Links, current entry, SP menu | E-29–E-31 |
| 9 | `NavigationGuard` / `GuardedLink` — P-27 Guarded navigation | Dirty form intercepts in-app links | BD-001 E-07a, E-09 |
| 10 | `HealthIndicator` / `useSystemHealth` — P-28 Health polling | First check, 30 s chain, visibility pause | E-26–E-28 |
| 11 | `ChartDialog` — P-29 Maximized chart | Open, focus, restore | E-24, E-25 |
| 12 | `SystemController.Health` — request boundary | Auth, delegate, `no-store`, no renewal | DD-003-FN §6–§7 |

### Reference documents

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| 1 | DD-003 | Items, states, message catalog, module list | Parent document |
| 2 | DD-003-API | The contract rendered below | |
| 3 | DD-003-FN | Server-side steps | |
| 4 | BD-003 | D-01–D-09, M-11–M-19, E-20–E-23 | |

## Processing design

### 1. DashboardPage — P-20 Load

| Field | Value |
| --- | --- |
| Detail | Route component for `/` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: one request per mount (and per Retry) produces one immutable snapshot, and every widget renders from that same object. No widget fetches on its own, so the figures cannot disagree (DEC-011).

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `useAuth` | Current user and roles | WI-001 |
| 2 | `dashboardApi.getDashboard` | Typed wrapper over `apiClient` | DD-003 module 8 |
| 3 | `StatusTiles`, `DeliveryTiles`, `AttentionList`, `TopProducts`, `BarChart`, `MessageBanner` | Children | `MessageBanner` reused from DD-001 |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Set `document.title` to `Dashboard — ProductionManagementAI` | — | — | — |
| 2 | Role gate on `user.roles` | Neither `Admin` nor `Operator` → stop | — | `forbidden` panel (MSG-E021), no API call |
| 3 | Render the heading and mount `HealthIndicator` (items 3, 26) immediately; the navbar is in the shared header | Neither depends on the snapshot, so navigation and health work in every state, including `error` | §10 | — |
| 4 | Set state `loading`; `GET /api/dashboard` with an `AbortSignal` tied to the component's lifetime | Unmount mid-request → the request is aborted and its result ignored | `getDashboard` | Skeleton tiles and widget frames, `aria-busy="true"` on the widget region; no previous figures |
| 5 | Handle the response | See §2 | §2 | `ready` / `error` / `forbidden` |
| 6 | **Retry** (E-21) | Only in `error` | back to step 4 | — |

### 2. DashboardPage — P-21 Response handling

| Field | Value |
| --- | --- |
| Detail | One place mapping each outcome of the dashboard request to a screen state |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: there are only three outcomes. The response is either complete (`ready`), or there is nothing to show (`error`, `forbidden`). An empty system is a `ready` snapshot of zeros, and each widget renders its own empty value (BD-003 D-01–D-09).

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | 200 | — | — | `ready`: render every widget from the snapshot; the snapshot-time line (item 4, M-17) is set, and its `role="status"` region announces "Dashboard updated" once per load |
| 2 | 401 | — | `apiClient` redirects | `/login` |
| 3 | 403 | — | — | `forbidden` panel MSG-E021 in place of the widgets; the health indicator unmounts (no polling); the navbar stays |
| 4 | Network failure or 5xx | — | — | `error`: `MessageBanner` with MSG-E013 and **Retry**, `role="alert"`; no figures and no skeleton |

### 3. StatusTiles, DeliveryTiles — P-22 Tiles

| Field | Value |
| --- | --- |
| Detail | Items 7–11 (status) and 12–15 (delivery); each tile is a `<div>` in a `<ul role="list">` with a visible label and a value |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: presentational. Each value and caption comes from a pure formatter in `dashboardFormat.ts`, so the rules M-13, M-14 and M-18 are unit-tested once.

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Status tiles: Total, then Draft, In progress, Completed, Cancelled in workflow order, labels from `statusLabels` | A 0 count renders "0" | `formatCount` | Items 7–11 |
| 2 | Completed this week / this month: "{n} orders" (singular at 1), caption "{q} units"; the tile's accessible description states the window ("since Monday 2026-09-21", "since 2026-09-01") from `from` | — | `formatCount`, `formatQuantity` | Items 12, 13 |
| 3 | On time: `formatRate(onTimeCount, completedCount)` → "77%", caption "23 of 30 on time" | `completedCount = 0` → "—" with caption MSG-I008 | `formatRate` | Item 14 (M-13) |
| 4 | Average lead time: "{averageDays with one decimal} days" (singular at exactly 1.0), caption "based on {n} orders" | `averageDays = null` → "—" with caption MSG-I008 | `formatLeadTime` | Item 15 (M-14) |

`formatRate` rounds half up: `Math.floor(100 * x / y + 0.5)` on integers. Values with an exact .5 then behave the same in every browser, and match the server's half-away-from-zero rule for non-negative numbers.

### 4. AttentionList — P-23 Overdue and due-soon groups

| Field | Value |
| --- | --- |
| Detail | Items 16–19: two groups, each a `<section>` whose `<h3>` carries the count ("Overdue (15)", "Due in the next 7 days (8)") |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | `orders.length > 0` → at `sm` and above a `<table>` with a `<caption>` (visually hidden, same text as the heading) and columns Order no., Product, Qty, Due date, Status; below `sm` one `<li>` card per order | Cells are plain text — no link, no row handler (DEC-006) | `statusLabels`, `formatQuantity` | Rows in response order |
| 2 | `total > orders.length` → note "Showing 10 of {total}" under the table | — | — | M-19 |
| 3 | `total = 0` → the group's empty message in place of the table | Overdue → MSG-I005; due soon → MSG-I006 | — | Heading still shows "(0)" |

### 5. TopProducts — P-24 Ranked list

| Field | Value |
| --- | --- |
| Detail | Item 22: an `<ol>` of up to 10 products |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | One `<li>` per entry, in response order: rank, "{sku} — {name}", open quantity, "({n} orders)" | — | `formatQuantity` | The rank comes from the `<ol>`; it is not a separate number to keep in step |
| 2 | Each entry shows a proportional bar behind the quantity (width = quantity ÷ the first entry's quantity), `aria-hidden` | Decorative: the number is the content | — | — |
| 3 | Empty list | — | — | MSG-I007 |

### 6. BarChart — P-25 Chart and table equivalent

| Field | Value |
| --- | --- |
| Detail | One reusable component rendering items 20–21 (workload, 10 bars) and 23–24 (trend, 12 bars) as inline SVG (DEC-010) |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: a plain bar chart with no library. Each bar prints its value, so nothing depends on hover. The whole chart has one accessible name that says what it shows, and a real table holds the same numbers for anyone who wants to read them rather than see them.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `dashboardFormat.bucketLabel`, `weekRange` | M-16 labels and ranges | Pure |

**Arguments** (props): `title` (heading text), `bars: { label, value, detail? }[]`, `summary` (accessible name), `tableColumns`, `tableRows`.

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Geometry: `viewBox = "0 0 {n × 48} 180"`; each bar slot 48 wide, bar 32 wide; plot height 140 above a 40-unit label band; `scale = 140 / max(values)` | `max = 0` → every bar drawn as a 1-unit baseline mark, and the chart's empty message is shown over it (workload MSG-I007; the trend needs none, since all zeros is a valid figure) | — | Proportional bars |
| 2 | Each bar: a `<rect>` in the chart's fill token, its value as `<text>` above it, its label (M-16) in the label band | The current-week bar and the "Overdue" bar use a second fill token **and** their label is bold, so the distinction never rests on color alone | `bucketLabel` | — |
| 3 | The `<svg>` has `role="img"` and `aria-label={summary}`; child text is `aria-hidden` (the summary already says it) | — | — | Screen readers hear one sentence, e.g. "Open workload by due week: overdue 15, this week 6, week of 09-28 7, … later 2" |
| 4 | Responsiveness: the SVG scales to its card's width at `sm` and above. Below `sm` it keeps a minimum width of 480 and its card scrolls horizontally (`overflow-x-auto`, `tabIndex=0` with a label so keyboard users can scroll it). The page itself never scrolls sideways | — | — | BD-003 SP layout |
| 4a | **Expand** (E-24, `variant="card"` only): an icon button in the heading row, accessible name "Expand {title}" | — | §11 | Opens `ChartDialog` |
| 5 | **View as table** (E-23): a `<button aria-expanded aria-controls>` toggling a `<table>` below the chart | Workload columns: Bucket, Dates (`weekRange`, e.g. `2026-09-22 – 2026-09-27`), Orders, Quantity. Trend columns: Week, Dates, Orders completed | `weekRange` | Collapsed by default; state is not persisted |

### 7. DashboardController.Get — request boundary

| Field | Value |
| --- | --- |
| Detail | Server entry point for `GET /api/dashboard` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: authorize, delegate, mark the response uncacheable. Nothing else happens here.

**Used components / services**

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `DashboardService.GetSnapshotAsync` | The use case | DD-003-FN §1 |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Authorize | No session → 401; wrong role → 403 | `[Authorize(Policy = ProductionOrderEditor)]` on the controller | Empty body, as DD-001-API |
| 2 | Delegate | Exception → global handler, 500 MSG-E013 | `GetSnapshotAsync` | — |
| 3 | Set `Cache-Control: no-store`; return 200 | — | `[ResponseCache(NoStore = true, Location = None)]` | — |

The action declares no parameter, so model binding has nothing to bind and a query string has no effect (DD-003-API).

### 8. AppNavbar — P-26 Navbar

| Field | Value |
| --- | --- |
| Detail | BD-003 H-2, H-4, H-5 inside the shared `AppHeader` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Determine the current entry from `useLocation().pathname` | `/` → Dashboard; `/production-orders` or `/production-orders/{id}` → Production orders; `/production-orders/new` → New production order | — | `aria-current="page"` on that link, plus the current-entry style (underline + weight, not color alone) |
| 2 | `sm` and above: render `<nav aria-label="Main">` with three `GuardedLink`s between the app name and the user | — | §9 | — |
| 3 | Below `sm`: render **Menu** (`aria-expanded`, `aria-controls`) instead; the panel is closed by default | — | — | — |
| 4 | Menu click | Toggle the panel | — | Panel with the three links, the user and Sign out |
| 5 | Escape while the panel is open | — | — | Panel closes, focus returns to Menu |
| 6 | A link activated, or the route changes | — | — | Panel closes |

### 9. NavigationGuard / GuardedLink — P-27 Guarded navigation

| Field | Value |
| --- | --- |
| Detail | DEC-022: in-app links respect SCR-001's discard confirmation |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | `ProductionOrderForm` registers a guard on mount: `to => { if (!isDirty) return false; pendingTo = to; openDiscardDialog(); return true }`, and unregisters on unmount | Only the form registers; other screens never do | `useNavigationGuard().register` | — |
| 2 | A `GuardedLink` (navbar, breadcrumb, app name) is clicked | Modified click (Ctrl/Cmd/Shift or middle button) → no interception | — | Browser default |
| 3 | Ask the registered guard | No guard, or it returns `false` → normal navigation | — | Route changes |
| 4 | Guard returns `true` → `preventDefault()` | — | — | Dialog 17 open (BD-001), focus on Keep editing |
| 5 | Discard | Navigate to `pendingTo` (or to `/production-orders` when opened by Cancel, as today) | `useNavigate` | Leaves without saving |
| 6 | Keep editing / Escape | Clear `pendingTo` | — | Stays, values kept; focus returns to the element that triggered the dialog |

### 10. HealthIndicator / useSystemHealth — P-28 Health polling

| Field | Value |
| --- | --- |
| Detail | Item 26; BD-003 HS-01–HS-05, E-26–E-28 |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | On mount show "Checking…" and run a check | — | `GET /api/system/health` with `AbortSignal.any([unmountSignal, AbortSignal.timeout(5000)])` | — |
| 2 | 200 | `database = "ok"` → Server OK, Database OK; `"unavailable"` → Server OK, Database Unavailable | — | Statuses and "Checked HH:mm:ss" (from `checkedAt`, plant time) |
| 3 | Timeout, network failure or 5xx | — | — | Server Unreachable, Database Unknown; "Checked" keeps the last successful check's time, labelled "Last answered" |
| 4 | 401 | — | `apiClient` redirects | `/login` |
| 5 | 403 | Stop polling | — | Indicator hidden |
| 6 | After each completed check, if the tab is visible, schedule the next one in 30 s (`setTimeout`) | Tab hidden → do not schedule | — | No overlap: at most one check in flight |
| 7 | `visibilitychange` | Hidden → clear the timer; visible → check now, then continue from step 6 | — | — |
| 8 | Announce | The `role="status"` text changes only when a status changes; the check time sits outside the live region, so a repeat is silent | — | Screen readers hear "Database: Unavailable" once, not every 30 s |
| 9 | Unmount | Abort any request in flight, clear the timer | — | No request after leaving the page |

### 11. ChartDialog — P-29 Maximized chart

| Field | Value |
| --- | --- |
| Detail | Items 27–29; E-24, E-25; DEC-018 |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Expand → render `ChartDialog` with the chart's props from the snapshot already in memory and call `dialog.showModal()` | No request | — | Full-window dialog (`width: 100vw; height: 100dvh` minus safe-area insets), `aria-labelledby` = its `<h2>` title |
| 2 | Layout inside | `lg` and above: chart left (≈ 2/3), table right; below `lg`: chart, then the table | `BarChart variant="maximized"` | Bars scale to the dialog width; no horizontal scroll at `sm` and above |
| 3 | Initial focus on **Restore** | — | — | — |
| 4 | Restore click, or Escape (the dialog's `cancel` event) | — | `dialog.close()` | Dialog gone; focus returns to the Expand that opened it |
| 5 | The page behind | Inert while open (native modal behavior); the health indicator keeps polling | — | — |

### 12. SystemController.Health — request boundary

| Field | Value |
| --- | --- |
| Detail | Server entry point for `GET /api/system/health` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Authorize | No session → 401; wrong role → 403 | `[Authorize(Policy = ProductionOrderEditor)]` | Empty body |
| 2 | The cookie handler evaluates sliding expiration | Path is the health path → `ShouldRenew = false` | DD-003-FN §7 | No `Set-Cookie` |
| 3 | Delegate | — | `SystemHealthService.CheckAsync` | — |
| 4 | 200, `Cache-Control: no-store` | — | `[ResponseCache(NoStore = true, Location = None)]` | — |

## Unresolved decisions

None.
