<!-- Based on ai/templates/DD/function-design.md (revision at commit c3747ed). -->

# Dashboard Snapshot — Function Design (機能設計)

DD-003-FN — used by DD-003 and DD-003-API, requirements REQ-028–REQ-042.

## Document control (改版履歴)

| Field | Value |
| --- | --- |
| Document ID | DD-003-FN |
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
| 2 | 2026-09-22 | Claude (for ThanhTN) | §6 `SystemHealthService.CheckAsync` and `IDatabasePing` (DEC-017, DEC-020); §7 the no-renew rule for the health path (DEC-019); observability extended |

## Overview and method index

| Field | Value |
| --- | --- |
| Module name | `DashboardService` and its reader port (`ProductionManagementAI.Application.Dashboard`, reader implemented in `ProductionManagementAI.Infrastructure.Dashboard`) |
| Overview | Turn "now" into the dashboard's figures: derive every window from the plant-local date, read DB-004's seven statements in one snapshot, and shape the result. A separate service from `ProductionOrderService`, because it shares no use case with it — only the clock, the telemetry source and the result conventions, which it reuses |

| No | Method name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `DashboardService.GetSnapshotAsync` | Read the clock once, build the window, read, shape, record telemetry | API-DSH-01 |
| 2 | `DashboardWindow.For` | Every date and instant the figures depend on, from T — a pure function | New |
| 3 | `IDashboardReader.ReadAsync` | DB-004 Q1–Q6 in one `REPEATABLE READ READ ONLY` transaction | New port; `DashboardReader` in Infrastructure |
| 4 | `DashboardMapper.ToResponse` | Raw rows → `DashboardResponse`: zero-filling, bucket order, lead-time rounding | New, pure |
| 5 | `IPlantClock.DateOf` / `StartOfDayUtc` / `TimeZoneId` | Plant-local calendar conversions the window needs | Additive members on the existing port (DD-001-FN §6) |
| 6 | `SystemHealthService.CheckAsync` | Ping the database with a 2 s timeout | API-SYS-01; port `IDatabasePing`, `DatabasePing` in Infrastructure |
| 7 | Cookie `OnCheckSlidingExpiration` | Health requests never renew the session | `DependencyInjection.cs` |

### Shared utility references

| No | Name | Overview | Notes |
| --- | --- | --- | --- |
| 1 | `IPlantClock` | Plant-local date and zone | Existing port; gains three members (§5). `Today` and `CurrentYear` are unchanged |
| 2 | `TimeProvider` | The single "now" read per snapshot | Existing; `FakeTimeProvider` in tests pins it |
| 3 | `ProductionOrderTelemetry` | `ActivitySource` + `Meter` named `ProductionManagementAI.ProductionOrders` | Existing; gains one counter (see Observability). Reusing it means `Program.cs` registers no new source or meter |
| 4 | `ILogger<DashboardService>` | Structured logging, `LoggerMessage` source-generated | |
| 5 | `AppDbContext` | EF Core context; the reader uses `Database.SqlQuery<T>(FormattableString)` | Interpolated values become bound parameters, never concatenated SQL (`ai/rules/database.md`) |

## Request data

Parameters sent to the database — exactly DB-004's parameter table, produced by `DashboardWindow` (§2). All are bound parameters.

| No | Name | Variable name | Type | Length | Used by (which method(s)) | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Plant today | `Today` | `DateOnly` | — | Q2, Q3 | T |
| 2 | Due-soon end | `SoonEnd` | `DateOnly` | — | Q3b | T + 7 (DEC-007) |
| 3 | Week 0 | `Week0` | `DateOnly` | — | Q2 | W(T), the Monday on or before T |
| 4 | Trend start | `TrendStartUtc` | `DateTimeOffset` | — | Q5, Q6 | Start of plant day W(T) − 77 |
| 5 | Week start | `WeekStartUtc` | `DateTimeOffset` | — | Q5 | Start of plant day W(T) |
| 6 | Month start | `MonthStartUtc` | `DateTimeOffset` | — | Q5 | Start of plant day 1 of T's month |
| 7 | Window start | `WindowStartUtc` | `DateTimeOffset` | — | Q5 | Start of plant day T − 29 |
| 8 | End | `EndUtc` | `DateTimeOffset` | — | Q5, Q6 | Start of plant day T + 1 |
| 9 | Zone | `TimeZoneId` | `string` | — | Q5, Q6 | `PlantOptions.TimeZone`, validated at startup |

## Response / value mapping

`DashboardResponse` (field catalog in DD-003-API) is built by `DashboardMapper.ToResponse` (§4) from `DashboardRaw`, the reader's result:

| No | Name | Variable name | Type | Length | Required | Value mapping | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Status counts | `StatusCounts` | `(string status, int count)[]` | ≤ 4 | yes | Missing statuses → 0; `Total` = sum | Q1 |
| 2 | Workload rows | `Workload` | `(int bucket, int count, long quantity)[]` | ≤ 10 | yes | `bucket` −1 → `overdue`, 0–7 → `week` k, 8 → `later`; missing buckets → 0 | Q2 |
| 3 | Overdue / due-soon rows | `Overdue`, `DueSoon` | `(DashboardOrderRow[] rows, int total)` | ≤ 10 rows | yes | `total` from `count(*) OVER ()` of the first row, 0 when no row | Q3a, Q3b |
| 4 | Top products | `TopProducts` | `TopProductRow[]` | ≤ 10 | yes | As returned, already ordered | Q4 |
| 5 | Delivery row | `Delivery` | `DeliveryRow` | 1 | yes | `NULL` sums → 0; `NULL` average → `null` | Q5 |
| 6 | Trend rows | `Trend` | `(DateOnly weekStart, int count)[]` | ≤ 12 | yes | Laid onto the 12 expected Mondays; missing → 0 | Q6 |

## Method design

### 1. `DashboardService.GetSnapshotAsync`

| Field | Value |
| --- | --- |
| Description | Build the complete dashboard snapshot for the current moment |
| Return type | `Task<DashboardResponse>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `CancellationToken` | `cancellationToken` | |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `DashboardResponse` | snapshot | Always complete. The method has no business failure — no input to reject, nothing to find — so it returns the response directly rather than a `Result<T>`; unexpected exceptions propagate to the global Problem Details handler (500, MSG-E013), as in DD-002-FN |

Processing overview: the clock is read **once**, and T, every window, the `asOf` time and the response's `today` all derive from that one reading. A dashboard computed across midnight therefore cannot mix two "todays".

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Start span `ProductionOrder.Dashboard` | `ProductionOrderTelemetry.Source` |
| 2 | `utcNow = timeProvider.GetUtcNow()`; `today = plantClock.DateOf(utcNow)` | `TimeProvider`, `IPlantClock` (§5) |
| 3 | `window = DashboardWindow.For(today, plantClock)` | §2 |
| 4 | `raw = await reader.ReadAsync(window, ct)` | §3 |
| 5 | `response = DashboardMapper.ToResponse(window, raw, utcNow, plantClock.TimeZoneId)` | §4 |
| 6 | Tag the span (`dashboard.today`, `dashboard.active_orders` = draft + inProgress, `dashboard.window_completed` = onTime.completedCount); counter `dashboard_loaded{outcome=success}`; return | telemetry |
| 7 | On exception: counter `dashboard_loaded{outcome=error}`, span status `Error`, log `DashboardSnapshotFailed` with the exception, rethrow | telemetry, logger |

### 2. `DashboardWindow.For`

| Field | Value |
| --- | --- |
| Description | Compute every date and UTC instant that BD-003 D-01–D-09 and DB-004 Q1–Q6 depend on, from the plant-local date T |
| Return type | `DashboardWindow` (immutable record) |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `DateOnly` | `today` | T |
| 2 | `IPlantClock` | `clock` | For `StartOfDayUtc` and `TimeZoneId` only |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `DashboardWindow` | window | The nine fields in Request data, plus `MonthStart` (date), `WindowStart` (T − 29) and `TrendWeeks`, the 12 Mondays W(T) − 77 … W(T) |

Processing overview: a pure function of T and the zone. Every calendar rule lives here once, so it can be unit-tested for every weekday and month boundary without a database (TC-205, TC-209).

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | `Week0 = today.AddDays(-((7 + (int)today.DayOfWeek - 1) % 7))` — the Monday on or before T (Sunday maps to 6 days back) | — |
| 2 | `SoonEnd = today + 7`; `WindowStart = today − 29`; `MonthStart = new DateOnly(today.Year, today.Month, 1)` | — |
| 3 | `TrendWeeks = [Week0 − 77, Week0 − 70, …, Week0]` (12 Mondays) | — |
| 4 | Convert each date lower bound to the start of that plant day in UTC: `TrendStartUtc = StartOfDayUtc(TrendWeeks[0])`, `WeekStartUtc = StartOfDayUtc(Week0)`, `MonthStartUtc`, `WindowStartUtc`, `EndUtc = StartOfDayUtc(today + 1)` | `IPlantClock.StartOfDayUtc` |
| 5 | `TimeZoneId = clock.TimeZoneId` | — |

### 3. `IDashboardReader.ReadAsync` (`DashboardReader`)

| Field | Value |
| --- | --- |
| Description | Run DB-004 Q1–Q6 (seven statements) inside one read-only snapshot transaction |
| Return type | `Task<DashboardRaw>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `DashboardWindow` | `window` | Parameters |
| 2 | `CancellationToken` | `cancellationToken` | |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `DashboardRaw` | raw | The seven results, unshaped |

Processing overview: raw SQL, because Q2's bucket expression, Q3's window count, Q5's `FILTER` aggregates and Q6's `date_trunc … AT TIME ZONE` have no clean LINQ translation. Each statement is DB-004's text, written as a C# interpolated `FormattableString`, so every `{window.…}` becomes a bound parameter. The statements are the SQL DB-004 reviewed, not a LINQ approximation of it. Results are mapped to small keyless row types (`StatusCountRow`, `WorkloadRow`, `DashboardOrderRow`, `TopProductRow`, `DeliveryRow`, `TrendRow`) via `Database.SqlQuery<T>`.

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | `await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct)` | EF Core / Npgsql |
| 2 | `SET TRANSACTION READ ONLY` — the first statement in the transaction, before any query, as PostgreSQL requires. Any write in this transaction would now fail, which guards the dashboard's "changes no data" (REQ-039) at the database | `ExecuteSqlRawAsync` (constant text, no input) |
| 3 | Q1 status counts | `SqlQuery<StatusCountRow>` |
| 4 | Q2 workload buckets with `Today`, `Week0` | `SqlQuery<WorkloadRow>` |
| 5 | Q3a overdue (`due_date < Today`), Q3b due soon (`due_date BETWEEN Today AND SoonEnd`) | `SqlQuery<DashboardOrderRow>` × 2 |
| 6 | Q4 top products | `SqlQuery<TopProductRow>` |
| 7 | Q5 delivery row with the five instants and the zone | `SqlQuery<DeliveryRow>` |
| 8 | Q6 trend with `TrendStartUtc`, `EndUtc` and the zone | `SqlQuery<TrendRow>` |
| 9 | `await tx.CommitAsync(ct)` (a read-only commit releases the snapshot) and return `DashboardRaw` | — |

The statements run sequentially on one connection; a transaction cannot run them in parallel, and at DB-004's volumes the total is a few milliseconds. The Npgsql command timeout stays at its 30 s default, as in DD-002.

### 4. `DashboardMapper.ToResponse`

| Field | Value |
| --- | --- |
| Description | Shape the raw rows into the API response |
| Return type | `DashboardResponse` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `DashboardWindow` | `window` | Bucket dates and window starts |
| 2 | `DashboardRaw` | `raw` | Reader output |
| 3 | `DateTimeOffset` | `asOf` | The single clock reading |
| 4 | `string` | `timeZoneId` | Echoed as `timeZone` |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `DashboardResponse` | response | See DD-003-API |

Processing overview: a pure function. Every "missing means zero" rule, and the one rounding rule the server owns, live here, so they are unit-tested without a database.

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Status counts: look up each of the four statuses in Q1's rows, 0 when absent; `total` = their sum | — |
| 2 | Workload: build exactly 10 entries in display order — `overdue` (bucket −1); 8 × `week` for k = 0…7 with `weekStart` = `Week0 + 7k`, except k = 0, where it is `Today`, and `weekEnd` = `Week0 + 7k + 6`; then `later` (bucket 8). Each takes Q2's count and quantity for its bucket, 0 when absent | — |
| 3 | Overdue / due soon: `total` = the first row's window count, or 0 when there is no row; `orders` = the rows mapped to `DashboardOrder` | — |
| 4 | Top products: map as returned | — |
| 5 | Completed this week / month: counts, `NULL` sums → 0, with `from` = `Week0` / `MonthStart` | — |
| 6 | On time: `{ onTimeCount = x, completedCount = y, windowStart = WindowStart }` | — |
| 7 | Lead time: `averageDays = y == 0 ? null : Math.Round(avg, 1, MidpointRounding.AwayFromZero)`, `orderCount = y` | — |
| 8 | Trend: for each of the 12 `TrendWeeks`, the Q6 count whose `week_start` equals it, 0 when absent; `weekEnd = weekStart + 6` | — |
| 9 | `asOf`, `today`, `timeZone` | — |

An integrity guard is asserted in the unit tests, not at run time: the workload counts sum to `draft + inProgress`, and Q3a's total equals the `overdue` bucket's count. Both follow from the single snapshot (DEC-015).

### 5. `IPlantClock` — additive members

| Field | Value |
| --- | --- |
| Description | Three members the dashboard's calendar needs, added to the existing port without changing `Today` or `CurrentYear` |
| Return type | see below |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments / return values**

| Member | Signature | Behavior |
| --- | --- | --- |
| `DateOf` | `DateOnly DateOf(DateTimeOffset utc)` | The plant-local date of an instant. `Today` becomes `DateOf(timeProvider.GetUtcNow())`, so the two can never disagree |
| `StartOfDayUtc` | `DateTimeOffset StartOfDayUtc(DateOnly date)` | The UTC instant at which that plant-local date begins: local midnight converted with `TimeZoneInfo`. When a DST gap removes local midnight, the first valid local time of that day is used. `Asia/Tokyo` has no DST; the rule exists so a reconfigured plant zone stays correct |
| `TimeZoneId` | `string TimeZoneId { get; }` | The configured IANA id (`PlantOptions.TimeZone`), validated at startup. Passed to PostgreSQL as `@zone`: PostgreSQL and .NET both read IANA tzdata, so they agree on the zone's rules |

Processing overview: `PlantClock` already resolves the `TimeZoneInfo` once. These members reuse it, so no other class converts time zones.

### 6. `SystemHealthService.CheckAsync`

| Field | Value |
| --- | --- |
| Description | Report whether the database answers `SELECT 1` within 2 seconds |
| Return type | `Task<SystemHealthResponse>` |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

**Arguments**

| No | Type | Name | Description |
| --- | --- | --- | --- |
| 1 | `CancellationToken` | `cancellationToken` | The request's token |

**Return value**

| Type | Name | Description |
| --- | --- | --- |
| `SystemHealthResponse` | health | `{ Database = "ok" \| "unavailable", CheckedAt }` — never an exception for a failed ping |

Processing overview: the port `IDatabasePing.PingAsync(ct)` is implemented by `DatabasePing` as `db.Database.ExecuteSqlRawAsync("SELECT 1", ct)` (constant text, no input). The service links the request token with a 2-second `CancellationTokenSource`. The port exists so the integration tests can replace it with a failing or a slow implementation (TC-224) without stopping the shared database container.

**Processing flow**

| Step | Description | Calls |
| --- | --- | --- |
| 1 | Start span `System.Health` | `ProductionOrderTelemetry.Source` |
| 2 | `using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(2 s)` | — |
| 3 | `await ping.PingAsync(timeout.Token)` → `database = "ok"` | `IDatabasePing` |
| 4 | On `OperationCanceledException` **caused by the timeout** (not by `ct`), or on `DbException`/`NpgsqlException`/`InvalidOperationException` from the connection → `database = "unavailable"`; log `Warning` `DatabasePingFailed` with the exception **type** only; span status `Error` | logger |
| 5 | If `ct` itself was cancelled (the client went away), rethrow — nothing to report | — |
| 6 | Counter `health_checks{database}`; return `{ database, CheckedAt = timeProvider.GetUtcNow() }` | telemetry, `TimeProvider` |

### 7. Session renewal rule (`OnCheckSlidingExpiration`)

| Field | Value |
| --- | --- |
| Description | Requests to `/api/system/health` never renew the authentication cookie (DEC-019) |
| Return type | — (cookie options event) |
| Created by / date | Claude / 2026-09-22 |
| Last modified by / date | — |

Processing overview: in `ConfigureApplicationCookie`, alongside the existing `OnRedirectToLogin`/`OnRedirectToAccessDenied` handlers, add `Events.OnCheckSlidingExpiration = ctx => { if (ctx.HttpContext.Request.Path.StartsWithSegments("/api/system/health")) ctx.ShouldRenew = false; return Task.CompletedTask; }`. Nothing else in the cookie configuration changes: expiry, sliding for every other path, `HttpOnly`, `SameSite=Lax`. The path is a constant shared with the controller's route, so the two cannot drift apart. Verified by TC-225 with the cookie handler's `TimeProvider` advanced past half the lifetime.

## Observability

Extends the existing `ProductionManagementAI.ProductionOrders` source and meter, per `ai/rules/backend.md`.

| Signal | Name | Type | Attributes / buckets |
| --- | --- | --- | --- |
| Span | `ProductionOrder.Dashboard` | Activity | `dashboard.today` (plant date), `dashboard.active_orders`, `dashboard.window_completed`; status `Error` on failure. The Npgsql instrumentation already in `Program.cs` adds a child span per statement, which shows the seven queries' individual cost |
| Metric | `pmai.production_orders.dashboard_loaded` | Counter | `outcome` = `success` \| `error` |
| Span | `System.Health` | Activity | status `Error` when the ping fails |
| Metric | `pmai.system.health_checks` | Counter | `database` = `ok` \| `unavailable` — an outage shows as the `unavailable` share rising, with volume bounded by open dashboards ÷ 30 s |

Logging: `Error` with the exception on failure (`DashboardSnapshotFailed`); `Warning` `DatabasePingFailed` with the exception type only (never the message, which can contain host or credential fragments from a connection error). Nothing on success — the counter and span cover it, and a dashboard load is not an auditable event. No figure or order data is logged.

## Unresolved decisions

None. The behavior follows BD-003 D-01–D-09, DB-004 Q1–Q6, and DEC-008, DEC-009, DEC-011 and DEC-015. One implementation note is recorded rather than decided: if EF Core's `SqlQuery<T>` cannot map a composite row cleanly (for example, the nested product in `DashboardOrderRow`), the row type stays flat (`ProductId`, `Sku`, `Name`) and the mapper nests it. That changes no contract.
