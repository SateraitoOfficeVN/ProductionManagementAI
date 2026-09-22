using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;
using Npgsql;
using ProductionManagementAI.Integration.Tests.ProductionOrders;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.Dashboard;

/// <summary>
/// DD-003 test viewpoints at integration level: real pipeline, real PostgreSQL, the app as pmai_app, plant clock
/// pinned to 2031-06-11 (a Wednesday; its week starts Monday 2031-06-09). Each test clears the orders and inserts
/// exactly its own rows, so expected figures are exact. Boundaries are chosen where UTC and Asia/Tokyo disagree.
/// </summary>
public class DashboardEndpointTests(DashboardFixture fixture) : IClassFixture<DashboardFixture>, IAsyncLifetime
{
    private const string DashboardUrl = "/api/dashboard";
    private static readonly DateOnly T = new(2031, 6, 11);
    private static readonly DateOnly Monday = new(2031, 6, 9);
    private int _seq;

    public async Task InitializeAsync()
    {
        fixture.PingOverride = null;
        await fixture.ClearOrdersAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Inserts one order as the owner. Times are UTC; a completed order needs <paramref name="completed"/>.</summary>
    private async Task Insert(string status, DateOnly due, int quantity = 10, string sku = "P-1001",
        DateTimeOffset? completed = null, DateTimeOffset? created = null)
    {
        var createdAt = created ?? (completed ?? new DateTimeOffset(2031, 6, 1, 0, 0, 0, TimeSpan.Zero)).AddDays(-1);
        string Ts(DateTimeOffset t) => $"'{t.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}+00'";
        await fixture.ExecuteAsOwnerAsync(
            $"""
            INSERT INTO production_orders (id, order_year, order_seq, product_id, quantity, due_date, status,
                                           created_at_utc, updated_at_utc, completed_at_utc)
            SELECT gen_random_uuid(), 2031, {++_seq}, p.id, {quantity}, '{due:yyyy-MM-dd}', '{status}',
                   {Ts(createdAt)}, {Ts(completed ?? createdAt)}, {(completed is null ? "NULL" : Ts(completed.Value))}
            FROM products p WHERE p.sku = '{sku}';
            """);
    }

    private static DateTimeOffset Utc(int y, int mo, int d, int h, int mi = 0) => new(y, mo, d, h, mi, 0, TimeSpan.Zero);

    private async Task<JsonObject> Dashboard(HttpClient client)
    {
        var response = await client.GetAsync(DashboardUrl);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Body();
    }

    private static int[] Counts(JsonNode array, string field = "orderCount") =>
        [.. array.AsArray().Select(n => n![field]!.GetValue<int>())];

    // TC-203
    [Fact]
    public async Task StatusCounts_AreExact_ZeroFilled_AndSumToTheTotal()
    {
        await Insert("Draft", T.AddDays(3));
        await Insert("Draft", T.AddDays(4));
        await Insert("InProgress", T.AddDays(5));
        await Insert("Cancelled", T.AddDays(6));
        using var client = await fixture.CreateClientAsAsync("Operator");

        var counts = (await Dashboard(client))["statusCounts"]!;

        Assert.Equal(4, counts["total"]!.GetValue<int>());
        Assert.Equal(2, counts["draft"]!.GetValue<int>());
        Assert.Equal(1, counts["inProgress"]!.GetValue<int>());
        Assert.Equal(0, counts["completed"]!.GetValue<int>());
        Assert.Equal(1, counts["cancelled"]!.GetValue<int>());
    }

    // TC-204
    [Fact]
    public async Task OverdueAndDueSoon_UseTheBoundariesInD01AndD02()
    {
        await Insert("InProgress", T.AddDays(-1), sku: "P-1002");   // overdue
        await Insert("Draft", T, sku: "P-1003");                    // due soon (today)
        await Insert("Draft", T.AddDays(7), sku: "P-1004");         // due soon (last day)
        await Insert("Draft", T.AddDays(8), sku: "P-1005");         // neither
        await Insert("Completed", T.AddDays(-1), completed: Utc(2031, 6, 1, 3)); // terminal: neither
        await Insert("Cancelled", T.AddDays(-1));                   // terminal: neither
        using var client = await fixture.CreateClientAsAsync("Admin");

        var body = await Dashboard(client);

        Assert.Equal(1, body["overdue"]!["total"]!.GetValue<int>());
        Assert.Equal("P-1002", body["overdue"]!["orders"]![0]!["product"]!["sku"]!.GetValue<string>());
        Assert.Equal(2, body["dueSoon"]!["total"]!.GetValue<int>());
        Assert.Equal(["P-1003", "P-1004"], body["dueSoon"]!["orders"]!.AsArray().Select(o => o!["product"]!["sku"]!.GetValue<string>()));
    }

    // TC-204 (continued): at most 10 rows, in due-date then order-number order, with the full total.
    [Fact]
    public async Task OverdueGroup_ReturnsTheFirstTen_InOrder_WithTheFullTotal()
    {
        for (var i = 12; i >= 1; i--)
        {
            await Insert("Draft", T.AddDays(-i));
        }

        using var client = await fixture.CreateClientAsAsync("Admin");
        var overdue = (await Dashboard(client))["overdue"]!;

        Assert.Equal(12, overdue["total"]!.GetValue<int>());
        var due = overdue["orders"]!.AsArray().Select(o => o!["dueDate"]!.GetValue<string>()).ToArray();
        Assert.Equal(10, due.Length);
        Assert.Equal(due.Order(StringComparer.Ordinal), due);
        Assert.Equal(Date(T.AddDays(-12)), due[0]);
    }

    // TC-205
    [Fact]
    public async Task Workload_PutsEveryActiveOrderInExactlyOneOfTenBuckets()
    {
        await Insert("Draft", T.AddDays(-1));          // overdue
        await Insert("Draft", T);                      // week 0 (starts today)
        await Insert("InProgress", Monday.AddDays(6)); // week 0 (Sunday)
        await Insert("Draft", Monday.AddDays(7));      // week 1
        await Insert("Draft", Monday.AddDays(55));     // week 7 (its Sunday)
        await Insert("Draft", Monday.AddDays(56));     // later
        await Insert("Completed", T.AddDays(20), completed: Utc(2031, 6, 1, 3)); // not active: in no bucket
        using var client = await fixture.CreateClientAsAsync("Admin");

        var body = await Dashboard(client);
        var workload = body["workload"]!.AsArray();

        Assert.Equal([1, 2, 1, 0, 0, 0, 0, 0, 1, 1], Counts(workload));
        Assert.Equal(["overdue", "week", "week", "week", "week", "week", "week", "week", "week", "later"],
            workload.Select(b => b!["kind"]!.GetValue<string>()));
        Assert.Equal(Date(T), workload[1]!["weekStart"]!.GetValue<string>());
        Assert.Equal(Date(Monday.AddDays(7)), workload[2]!["weekStart"]!.GetValue<string>());
        var counts = body["statusCounts"]!;
        Assert.Equal(counts["draft"]!.GetValue<int>() + counts["inProgress"]!.GetValue<int>(), Counts(workload).Sum());
    }

    // TC-206
    [Fact]
    public async Task TopProducts_RanksTenByOpenQuantity_TiesBySku_ActiveOnly()
    {
        for (var i = 1; i <= 12; i++)
        {
            await Insert("Draft", T.AddDays(5), quantity: i == 6 ? 500 : 100 * i, sku: $"P-{1000 + i}");
        }

        await Insert("Completed", T.AddDays(5), quantity: 99_999, sku: "P-1020", completed: Utc(2031, 6, 1, 3));
        using var client = await fixture.CreateClientAsAsync("Admin");

        var top = (await Dashboard(client))["topProducts"]!.AsArray();

        Assert.Equal(10, top.Count);
        Assert.Equal(
            ["P-1012", "P-1011", "P-1010", "P-1009", "P-1008", "P-1007", "P-1005", "P-1006", "P-1004", "P-1003"],
            top.Select(t => t!["product"]!["sku"]!.GetValue<string>()));
        Assert.Equal(1200, top[0]!["openQuantity"]!.GetValue<long>());
        Assert.Equal(1, top[0]!["activeOrderCount"]!.GetValue<int>());
        Assert.DoesNotContain(top, t => t!["product"]!["sku"]!.GetValue<string>() == "P-1020");
    }

    // TC-209
    [Fact]
    public async Task CompletedThisWeekAndMonth_UsePlantMidnights()
    {
        await Insert("Completed", T.AddDays(9), quantity: 10, completed: Utc(2031, 6, 8, 15));     // Mon 06-09 00:00 JST
        await Insert("Completed", T.AddDays(9), quantity: 20, completed: Utc(2031, 6, 8, 14, 59)); // Sun 06-08 23:59 JST
        await Insert("Completed", T.AddDays(9), quantity: 30, completed: Utc(2031, 5, 31, 15));    // Sun 06-01 00:00 JST
        await Insert("Completed", T.AddDays(9), quantity: 40, completed: Utc(2031, 5, 31, 14, 59)); // Sat 05-31 23:59 JST
        using var client = await fixture.CreateClientAsAsync("Admin");

        var body = await Dashboard(client);

        Assert.Equal(1, body["completedThisWeek"]!["orderCount"]!.GetValue<int>());
        Assert.Equal(10, body["completedThisWeek"]!["quantity"]!.GetValue<long>());
        Assert.Equal(Date(Monday), body["completedThisWeek"]!["from"]!.GetValue<string>());
        Assert.Equal(3, body["completedThisMonth"]!["orderCount"]!.GetValue<int>());
        Assert.Equal(60, body["completedThisMonth"]!["quantity"]!.GetValue<long>());
    }

    // TC-210
    [Fact]
    public async Task OnTimeRate_ComparesThePlantCompletionDate_OverThirtyDays()
    {
        await Insert("Completed", T.AddDays(-1), completed: Utc(2031, 6, 10, 3));      // 06-10 JST, due 06-10: on time
        await Insert("Completed", T.AddDays(-1), completed: Utc(2031, 6, 10, 15, 30)); // 06-11 JST (UTC says 06-10), due 06-10: late
        await Insert("Completed", new DateOnly(2031, 6, 20), completed: Utc(2031, 6, 1, 3)); // early: on time
        await Insert("Completed", new DateOnly(2031, 5, 20), completed: Utc(2031, 5, 12, 15, 30)); // 05-13 00:30 JST: in the window
        await Insert("Completed", new DateOnly(2031, 5, 20), completed: Utc(2031, 5, 12, 14, 30)); // 05-12 23:30 JST: out
        await Insert("Cancelled", T.AddDays(-5));
        using var client = await fixture.CreateClientAsAsync("Admin");

        var onTime = (await Dashboard(client))["onTime"]!;

        Assert.Equal(3, onTime["onTimeCount"]!.GetValue<int>());
        Assert.Equal(4, onTime["completedCount"]!.GetValue<int>());
        Assert.Equal("2031-05-13", onTime["windowStart"]!.GetValue<string>());
    }

    // TC-211
    [Fact]
    public async Task Trend_HasTwelveWeeks_ZeroFilled_BucketedInPlantTime()
    {
        await Insert("Completed", T, completed: Utc(2031, 6, 8, 15));        // Mon 06-09 00:00 JST: this week
        await Insert("Completed", T, completed: Utc(2031, 5, 12, 3));        // week of 05-12
        await Insert("Completed", T, completed: Utc(2031, 3, 23, 15));       // Mon 03-24 00:00 JST: the oldest week
        await Insert("Completed", T, completed: Utc(2031, 3, 23, 14, 59));   // 13 weeks ago: excluded
        using var client = await fixture.CreateClientAsAsync("Admin");

        var trend = (await Dashboard(client))["completionTrend"]!.AsArray();

        Assert.Equal([1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1], Counts(trend));
        Assert.Equal("2031-03-24", trend[0]!["weekStart"]!.GetValue<string>());
        Assert.Equal(Date(Monday), trend[11]!["weekStart"]!.GetValue<string>());
    }

    // TC-212
    [Fact]
    public async Task LeadTime_IsTheMeanInDays_RoundedToOneDecimal()
    {
        var done = Utc(2031, 6, 5, 3);
        await Insert("Completed", T, completed: done, created: done.AddHours(-30));   // 1.25 days
        await Insert("Completed", T, completed: done, created: done.AddHours(-48));   // 2.0
        await Insert("Completed", T, completed: done, created: done.AddHours(-50.4)); // 2.1
        using var client = await fixture.CreateClientAsAsync("Admin");

        var lead = (await Dashboard(client))["leadTime"]!;

        Assert.Equal(1.8, lead["averageDays"]!.GetValue<double>());
        Assert.Equal(3, lead["orderCount"]!.GetValue<int>());
    }

    // TC-215
    [Fact]
    public async Task EmptySystem_Is200_WithZerosAndNoAverage()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var body = await Dashboard(client);

        Assert.Equal(0, body["statusCounts"]!["total"]!.GetValue<int>());
        Assert.Equal(0, body["overdue"]!["total"]!.GetValue<int>());
        Assert.Empty(body["overdue"]!["orders"]!.AsArray());
        Assert.All(Counts(body["workload"]!), n => Assert.Equal(0, n));
        Assert.All(Counts(body["completionTrend"]!), n => Assert.Equal(0, n));
        Assert.Empty(body["topProducts"]!.AsArray());
        Assert.Null(body["leadTime"]!["averageDays"]);
        Assert.Equal(0, body["onTime"]!["completedCount"]!.GetValue<int>());
        Assert.Equal("2031-06-11", body["today"]!.GetValue<string>());
        Assert.Equal("Asia/Tokyo", body["timeZone"]!.GetValue<string>());
    }

    // TC-213
    [Fact]
    public async Task Unauthenticated_Is401_AndNoRole_Is403()
    {
        using var anonymous = fixture.CreateClient();
        using var noRole = await fixture.CreateClientAsAsync(role: null);

        foreach (var url in new[] { DashboardUrl, "/api/system/health" })
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(url)).StatusCode);
            var forbidden = await noRole.GetAsync(url);
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
            Assert.Equal(string.Empty, await forbidden.Content.ReadAsStringAsync());
        }
    }

    // TC-214
    [Fact]
    public async Task Snapshot_IsOneReadOnlyTransaction_AndItsFiguresAgree()
    {
        await Insert("Draft", T.AddDays(-2));
        await Insert("InProgress", T.AddDays(3));
        await Insert("Draft", T.AddDays(70));
        using var client = await fixture.CreateClientAsAsync("Admin");

        var statements = new List<string>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name.StartsWith("Npgsql", StringComparison.Ordinal),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                var text = activity.GetTagItem("db.query.text") ?? activity.GetTagItem("db.statement");
                if (text is string sql)
                {
                    lock (statements) statements.Add(sql);
                }
            },
        };
        ActivitySource.AddActivityListener(listener);

        var body = await Dashboard(client);

        List<string> seen;
        lock (statements) seen = [.. statements];
        var readOnly = seen.FindIndex(s => s.Contains("SET TRANSACTION READ ONLY", StringComparison.Ordinal));
        var dashboard = seen.Where(s => s.Contains("FROM production_orders", StringComparison.Ordinal)).ToList();
        Assert.True(readOnly >= 0, "the reader marks its transaction read-only");
        Assert.Equal(7, dashboard.Count);
        Assert.True(seen.FindIndex(s => s.Contains("FROM production_orders", StringComparison.Ordinal)) > readOnly,
            "SET TRANSACTION READ ONLY runs before every query");

        var counts = body["statusCounts"]!;
        var workload = Counts(body["workload"]!);
        Assert.Equal(counts["draft"]!.GetValue<int>() + counts["inProgress"]!.GetValue<int>(), workload.Sum());
        Assert.Equal(body["overdue"]!["total"]!.GetValue<int>(), workload[0]);
    }

    // TC-221
    [Fact]
    public async Task QueryString_IsIgnored()
    {
        await Insert("Draft", T.AddDays(1));
        using var client = await fixture.CreateClientAsAsync("Admin");

        var plain = await (await client.GetAsync(DashboardUrl)).Content.ReadAsStringAsync();
        var crafted = await (await client.GetAsync($"{DashboardUrl}?today=2020-01-01&top=1000&zone=UTC")).Content.ReadAsStringAsync();

        Assert.Equal(plain, crafted);
    }

    // TC-224
    [Fact]
    public async Task Health_ReportsOnlyTheDatabaseVerdict_AndIsNotCached()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");

        var ok = await client.GetAsync("/api/system/health");
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Contains("no-store", ok.Headers.CacheControl!.ToString(), StringComparison.Ordinal);
        var body = await ok.Body();
        Assert.Equal(["checkedAt", "database"], body.Select(p => p.Key).Order().ToArray());
        Assert.Equal("ok", body["database"]!.GetValue<string>());

        fixture.PingOverride = _ => throw new NpgsqlException("connection refused to db-host:5432 as postgres");
        var failed = await client.GetAsync("/api/system/health");
        Assert.Equal(HttpStatusCode.OK, failed.StatusCode);
        var failedText = await failed.Content.ReadAsStringAsync();
        Assert.Contains("\"unavailable\"", failedText, StringComparison.Ordinal);
        Assert.DoesNotContain("db-host", failedText, StringComparison.Ordinal);

        fixture.PingOverride = ct => Task.Delay(Timeout.Infinite, ct);
        var stalled = await (await client.GetAsync("/api/system/health")).Body();
        Assert.Equal("unavailable", stalled["database"]!.GetValue<string>());
    }

    // TC-207 (integration)
    [Fact]
    public async Task CompletingThroughTheApi_RecordsTheSaveTime_AndARequestCannotSetIt()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var created = await client.CreateOrder(ValidCreate(due: T.AddDays(10)));
        var id = created["id"]!.GetValue<string>();

        var start = UpdateFrom(created);
        start["status"] = "InProgress";
        start["completedAtUtc"] = "2000-01-01T00:00:00Z"; // not a request field: must be ignored
        var started = await client.PutJson($"/api/production-orders/{id}", start);
        Assert.Equal(HttpStatusCode.OK, started.StatusCode);
        Assert.Null(await fixture.ScalarAsOwnerAsync<DateTime?>($"SELECT completed_at_utc FROM production_orders WHERE id = '{id}'"));

        var finish = UpdateFrom(await started.Body());
        finish["status"] = "Completed";
        var finished = await client.PutJson($"/api/production-orders/{id}", finish);
        Assert.Equal(HttpStatusCode.OK, finished.StatusCode);
        Assert.DoesNotContain("completed", (await finished.Content.ReadAsStringAsync()).ToLowerInvariant().Replace("\"status\":\"completed\"", ""), StringComparison.Ordinal);

        var same = await fixture.ScalarAsOwnerAsync<bool>(
            $"SELECT completed_at_utc = updated_at_utc AND completed_at_utc IS NOT NULL FROM production_orders WHERE id = '{id}'");
        Assert.True(same);
    }

    // TC-208
    [Theory]
    [InlineData("UPDATE production_orders SET completed_at_utc = NULL WHERE status = 'Completed'")]
    [InlineData("UPDATE production_orders SET completed_at_utc = now() WHERE status = 'Draft'")]
    [InlineData("UPDATE production_orders SET completed_at_utc = created_at_utc - interval '1 day' WHERE status = 'Completed'")]
    public async Task CompletionChecks_RejectInconsistentRows(string sql)
    {
        await Insert("Completed", T, completed: Utc(2031, 6, 5, 3));
        await Insert("Draft", T.AddDays(3));

        var error = await Assert.ThrowsAsync<PostgresException>(() => fixture.ExecuteAsOwnerAsync(sql));

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }
}
