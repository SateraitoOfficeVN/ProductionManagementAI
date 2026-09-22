using System.Net;
using System.Text.Json.Nodes;
using Npgsql;
using ProductionManagementAI.Integration.Tests.ProductionOrders;

namespace ProductionManagementAI.Integration.Tests.Dashboard;

/// <summary>
/// TC-218 and TC-219: DB-004's demo seed on a fresh database with the real clock, and the partial indexes. Its own
/// fixture; it deletes nothing. Assertions are the relative properties DB-004 states, which hold whenever the seed ran
/// moments before the test.
/// </summary>
public class DashboardSeedTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    // TC-218
    [Fact]
    public async Task Seed_ProducesTheFiguresDb004States()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var response = await client.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Body();

        var counts = body["statusCounts"]!;
        Assert.Equal(124, counts["total"]!.GetValue<int>());
        Assert.Equal(35, counts["draft"]!.GetValue<int>());
        Assert.Equal(25, counts["inProgress"]!.GetValue<int>());
        Assert.Equal(56, counts["completed"]!.GetValue<int>());
        Assert.Equal(8, counts["cancelled"]!.GetValue<int>());

        Assert.All(Counts(body["completionTrend"]!), n => Assert.True(n >= 2, "every trend week has at least 2 completions"));
        Assert.All(Counts(body["workload"]!), n => Assert.True(n > 0, "every workload bucket is non-zero"));
        Assert.Equal(60, Counts(body["workload"]!).Sum());
        Assert.Equal(23, body["onTime"]!["onTimeCount"]!.GetValue<int>());
        Assert.Equal(30, body["onTime"]!["completedCount"]!.GetValue<int>());
        Assert.Equal(13.7, body["leadTime"]!["averageDays"]!.GetValue<double>());
        Assert.Equal(10, body["topProducts"]!.AsArray().Count);
        Assert.Equal(15, body["overdue"]!["total"]!.GetValue<int>());
        Assert.Equal(8, body["dueSoon"]!["total"]!.GetValue<int>());

        Assert.Equal(0L, await fixture.ScalarAsOwnerAsync<long>(
            "SELECT count(*) FROM production_orders WHERE completed_at_utc < created_at_utc"));
        Assert.Equal(124, await fixture.ScalarAsOwnerAsync<int>(
            "SELECT max(last_seq) FROM production_order_number_counters"));
    }

    // TC-219: the partial indexes can serve their queries (not that the planner picks them at demo volume).
    [Theory]
    [InlineData("SELECT id FROM production_orders WHERE status IN ('Draft', 'InProgress') AND due_date < current_date ORDER BY due_date, order_number LIMIT 10",
        "ix_production_orders_active_due_date")]
    [InlineData("SELECT count(*) FROM production_orders WHERE completed_at_utc >= now() - interval '84 days' AND completed_at_utc < now()",
        "ix_production_orders_completed_at_utc")]
    public async Task PartialIndexes_CanServeTheDashboardQueries(string query, string index)
    {
        await using var connection = new NpgsqlConnection(fixture.OwnerConnectionString);
        await connection.OpenAsync();

        // As WI-003's TC-119: at demo volume a sequential scan is genuinely cheaper, so seqscan is disabled for this
        // session to answer "can this index serve the query as the table grows?".
        foreach (var setup in new[] { "ANALYZE production_orders", "SET enable_seqscan = off" })
        {
            await using var command = new NpgsqlCommand(setup, connection);
            await command.ExecuteNonQueryAsync();
        }

        await using var explain = new NpgsqlCommand($"EXPLAIN {query}", connection);
        var plan = new List<string>();
        await using (var reader = await explain.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                plan.Add(reader.GetString(0));
            }
        }

        Assert.Contains(index, string.Join('\n', plan));
    }

    private static int[] Counts(JsonNode array) => [.. array.AsArray().Select(n => n!["orderCount"]!.GetValue<int>())];
}
