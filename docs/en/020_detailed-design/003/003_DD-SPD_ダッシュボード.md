<!-- Based on ai/templates/DD/screen-processing-design.md (revision at commit c3747ed). -->

# Production Dashboard — Screen Processing Design (画面処理設計)

003_DD-SPD — elaborates 003_DD, implements 003_BD, requirements REQ-028–REQ-042.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | 003_DD-SPD |
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
| 3 | 2026-09-23 | Claude (for ThanhTN) | WI-005: the UI is Japanese. Tile, list, chart and health texts quote the Japanese catalog with an English gloss (WI-005 DEC-003); number and date formats (件, 個, 日, `YYYY/MM/DD`, `M/D`) and the formatter names match the code. Corrected: the status region announces the snapshot-time line, not a separate "Dashboard updated" text. No field, rule, API or processing changes |

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
| 4 | `AttentionList` — P-23 Overdue and due-soon groups | Two titled tables (cards on SP), 「N件中 10件を表示」 (Showing 10 of N) | Items 16–19 |
| 5 | `TopProducts` — P-24 Ranked list | Ordered list of up to 10 | Item 22 |
| 6 | `BarChart` — P-25 Chart and table equivalent | SVG bars, value labels, accessible summary, 「表で表示」 (View as table) | Items 20–21, 23–24; E-23 |
| 7 | `DashboardController.Get` — request boundary | Auth, delegate, `no-store` | Server steps in 003_DD-FN |
| 8 | `AppNavbar` — P-26 Navbar | Links, current entry, SP menu | E-29–E-31 |
| 9 | `NavigationGuard` / `GuardedLink` — P-27 Guarded navigation | Dirty form intercepts in-app links | 001_BD E-07a, E-09 |
| 10 | `HealthIndicator` / `useSystemHealth` — P-28 Health polling | First check, 30 s chain, visibility pause | E-26–E-28 |
| 11 | `ChartDialog` — P-29 Maximized chart | Open, focus, restore | E-24, E-25 |
| 12 | `SystemController.Health` — request boundary | Auth, delegate, `no-store`, no renewal | 003_DD-FN §6–§7 |

### Reference documents

| No | Document | Purpose / use | Notes |
| --- | --- | --- | --- |
| 1 | 003_DD | Items, states, message catalog, module list | Parent document |
| 2 | 003_DD-API | The contract rendered below | |
| 3 | 003_DD-FN | Server-side steps | |
| 4 | 003_BD | D-01–D-09, M-11–M-19, E-20–E-23 | |

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
| 2 | `dashboardApi.getDashboard` | Typed wrapper over `apiClient` | 003_DD module 8 |
| 3 | `StatusTiles`, `DeliveryTiles`, `AttentionList`, `TopProducts`, `BarChart`, `MessageBanner` | Children | `MessageBanner` reused from 001_DD |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Set `document.title` to `ダッシュボード — ProductionManagementAI` (Dashboard) | — | — | — |
| 2 | Role gate on `user.roles` | Neither `Admin` nor `Operator` → stop | — | `forbidden` panel (MSG-E021), no API call |
| 3 | Render the heading and mount `HealthIndicator` (items 3, 26) immediately; the navbar is in the shared header | Neither depends on the snapshot, so navigation and health work in every state, including `error` | §10 | — |
| 4 | Set state `loading`; `GET /api/dashboard` with an `AbortSignal` tied to the component's lifetime | Unmount mid-request → the request is aborted and its result ignored | `getDashboard` | Skeleton tiles and widget frames, `aria-busy="true"` on the widget region; no previous figures |
| 5 | Handle the response | See §2 | §2 | `ready` / `error` / `forbidden` |
| 6 | **再試行** (Retry) (E-21) | Only in `error` | back to step 4 | — |

### 2. DashboardPage — P-21 Response handling

| Field | Value |
| --- | --- |
| Detail | One place mapping each outcome of the dashboard request to a screen state |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: there are only three outcomes. The response is either complete (`ready`), or there is nothing to show (`error`, `forbidden`). An empty system is a `ready` snapshot of zeros, and each widget renders its own empty value (003_BD D-01–D-09).

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | 200 | — | — | `ready`: render every widget from the snapshot; the snapshot-time line (item 4, M-17) is set, and its `role="status"` region announces the snapshot-time line itself, 「{YYYY/MM/DD HH:mm} 時点（Asia/Tokyo）」, once per load (WI-005 correction: the implementation never had a separate "Dashboard updated" text) |
| 2 | 401 | — | `apiClient` redirects | `/login` |
| 3 | 403 | — | — | `forbidden` panel MSG-E021 in place of the widgets; the health indicator unmounts (no polling); the navbar stays |
| 4 | Network failure or 5xx | — | — | `error`: `MessageBanner` with MSG-E013 and **再試行** (Retry), `role="alert"`; no figures and no skeleton |

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
| 1 | Status tiles: 「合計」 (Total), then 「下書き」, 「進行中」, 「完了」, 「取消」 in workflow order, labels from `statusLabels` | A 0 count renders "0" | `formatNumber` | Items 7–11 |
| 2 | 「今週の完了」 / 「今月の完了」 (Completed this week / this month): 「{n}件」, caption 「{q}個」; the tile states its window (「2026/09/21（月）から」 since Monday 2026-09-21, 「2026/09/01から」 since 2026-09-01) from `from` | — | `formatOrders`, `formatUnits` | Items 12, 13 |
| 3 | 「期限内完了率」 (On time): `formatRate(onTimeCount, completedCount)` → "77%", caption 「30件中 23件が期限内」 (23 of 30 on time) | `completedCount = 0` → "—" with caption MSG-I008 | `formatRate` | Item 14 (M-13) |
| 4 | 「平均リードタイム」 (Average lead time): 「{averageDays with one decimal}日」, caption 「{n}件に基づく」 (based on {n} orders) | `averageDays = null` → "—" with caption MSG-I008 | `formatLeadTime` | Item 15 (M-14) |

`formatRate` rounds half up: `Math.floor(100 * x / y + 0.5)` on integers. Values with an exact .5 then behave the same in every browser, and match the server's half-away-from-zero rule for non-negative numbers.

### 4. AttentionList — P-23 Overdue and due-soon groups

| Field | Value |
| --- | --- |
| Detail | Items 16–19: two groups, each a `<section>` whose `<h3>` carries the count (「納期遅れ（15件）」, 「7日以内に納期（8件）」) |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | `orders.length > 0` → at `sm` and above a `<table>` with a `<caption>` (visually hidden, same text as the heading) and columns 「指示番号」, 「製品」, 「数量」, 「納期」, 「ステータス」 (Order no., Product, Qty, Due date, Status), due dates as `YYYY/MM/DD`; below `sm` one `<li>` card per order | Cells are plain text — no link, no row handler (DEC-006) | `statusLabels`, `formatNumber`, `formatDate` | Rows in response order |
| 2 | `total > orders.length` → note 「{total}件中 10件を表示」 (Showing 10 of {total}) under the table | — | — | M-19 |
| 3 | `total = 0` → the group's empty message in place of the table | Overdue → MSG-I005; due soon → MSG-I006 | — | Heading still shows 「（0件）」 |

### 5. TopProducts — P-24 Ranked list

| Field | Value |
| --- | --- |
| Detail | Item 22: an `<ol>` of up to 10 products |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | One `<li>` per entry, in response order: rank, "{sku} — {name}", open quantity, 「({n}件)」 ({n} orders) | — | `formatNumber` | The rank comes from the `<ol>`; it is not a separate number to keep in step |
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
| 2 | Each bar: a `<rect>` in the chart's fill token, its value as `<text>` above it, its label (M-16) in the label band | The current-week bar and the 「納期遅れ」 (Overdue) bar use a second fill token **and** their label is bold, so the distinction never rests on color alone | `bucketLabel` | — |
| 3 | The `<svg>` has `role="img"` and `aria-label={summary}`; child text is `aria-hidden` (the summary already says it) | — | — | Screen readers hear one sentence, e.g. 「納期週別の未完了作業量：納期遅れ 15件、今週 6件、9/28 7件、…それ以降 2件」 (Open workload by due week: overdue 15, this week 6, week of 9/28 7, … later 2) |
| 4 | Responsiveness: the SVG scales to its card's width at `sm` and above. Below `sm` it keeps a minimum width of 480 and its card scrolls horizontally (`overflow-x-auto`, `tabIndex=0` with a label so keyboard users can scroll it). The page itself never scrolls sideways | — | — | 003_BD SP layout |
| 4a | **拡大** (Expand) (E-24, `variant="card"` only): an icon button in the heading row, accessible name 「{title}を拡大」 (Expand {title}) | — | §11 | Opens `ChartDialog` |
| 5 | **表で表示** / **表を隠す** (View / Hide table) (E-23): a `<button aria-expanded aria-controls>` toggling a `<table>` below the chart | Workload columns: 「区分」, 「期間」 (`weekRange`, e.g. `2026/09/22〜2026/09/27`), 「件数」, 「数量」 (Bucket, Dates, Orders, Quantity). Trend columns: 「週」, 「期間」, 「完了件数」 (Week, Dates, Orders completed) | `weekRange` | Collapsed by default; state is not persisted |

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
| 1 | `DashboardService.GetSnapshotAsync` | The use case | 003_DD-FN §1 |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Authorize | No session → 401; wrong role → 403 | `[Authorize(Policy = ProductionOrderEditor)]` on the controller | Empty body, as 001_DD-API |
| 2 | Delegate | Exception → global handler, 500 MSG-E013 | `GetSnapshotAsync` | — |
| 3 | Set `Cache-Control: no-store`; return 200 | — | `[ResponseCache(NoStore = true, Location = None)]` | — |

The action declares no parameter, so model binding has nothing to bind and a query string has no effect (003_DD-API).

### 8. AppNavbar — P-26 Navbar

| Field | Value |
| --- | --- |
| Detail | 003_BD H-2, H-4, H-5 inside the shared `AppHeader` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | Determine the current entry from `useLocation().pathname` | `/` → Dashboard; `/production-orders` or `/production-orders/{id}` → Production orders; `/production-orders/new` → New production order | — | `aria-current="page"` on that link, plus the current-entry style (underline + weight, not color alone) |
| 2 | `sm` and above: render a `<nav>` named 「メインメニュー」 (Main menu) with three `GuardedLink`s between the app name and the user | — | §9 | — |
| 3 | Below `sm`: render **メニュー** (Menu), 「閉じる」 (Close) while open (`aria-expanded`, `aria-controls`) instead; the panel is closed by default | — | — | — |
| 4 | Menu click | Toggle the panel | — | Panel with the three links, the user and 「ログアウト」 (Sign out) |
| 5 | Escape while the panel is open | — | — | Panel closes, focus returns to 「メニュー」 |
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
| 4 | Guard returns `true` → `preventDefault()` | — | — | Dialog 17 open (001_BD), focus on Keep editing |
| 5 | Discard | Navigate to `pendingTo` (or to `/production-orders` when opened by Cancel, as today) | `useNavigate` | Leaves without saving |
| 6 | Keep editing / Escape | Clear `pendingTo` | — | Stays, values kept; focus returns to the element that triggered the dialog |

### 10. HealthIndicator / useSystemHealth — P-28 Health polling

| Field | Value |
| --- | --- |
| Detail | Item 26; 003_BD HS-01–HS-05, E-26–E-28 |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Processing flow**

| Step | Description | Branch / condition | Calls | Result (state / redirect / render) |
| --- | --- | --- | --- | --- |
| 1 | On mount show 「確認中…」 (Checking…) and run a check | — | `GET /api/system/health` with `AbortSignal.any([unmountSignal, AbortSignal.timeout(5000)])` | — |
| 2 | 200 | `database = "ok"` → 「サーバー：正常」, 「データベース：正常」; `"unavailable"` → 「サーバー：正常」, 「データベース：利用不可」 | — | Statuses and 「HH:mm:ss に確認」 (Checked HH:mm:ss, from `checkedAt`, plant time) |
| 3 | Timeout, network failure or 5xx | — | — | 「サーバー：接続不可」, 「データベース：不明」; the time keeps the last successful check's, labelled 「最終応答 HH:mm:ss」 (Last answered) |
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
| 3 | Initial focus on **元に戻す** (Restore) | — | — | — |
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
| 2 | The cookie handler evaluates sliding expiration | Path is the health path → `ShouldRenew = false` | 003_DD-FN §7 | No `Set-Cookie` |
| 3 | Delegate | — | `SystemHealthService.CheckAsync` | — |
| 4 | 200, `Cache-Control: no-store` | — | `[ResponseCache(NoStore = true, Location = None)]` | — |

## Unresolved decisions

None.
