using System.Net;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Npgsql;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.ProductionLines;

/// <summary>Synchronizes independent PostgreSQL connections to exercise real lock waits and serialization.</summary>
public sealed class LineConcurrencyTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private static async Task WaitForLock(IntegrationTestFixture fixture, string fragment)
    {
        var watch = Stopwatch.StartNew();
        while (watch.Elapsed < TimeSpan.FromSeconds(4))
        {
            if (await fixture.ScalarAsOwnerAsync<long>($"SELECT count(*) FROM pg_stat_activity WHERE usename = 'pmai_app' AND wait_event_type = 'Lock' AND query LIKE '%{fragment}%'") > 0) return;
            await Task.Delay(30);
        }
        Assert.Fail("The expected independent connection did not reach its row lock.");
    }
    private static JsonObject LineDraft(string code) => new() { ["code"] = code, ["name"] = "同期ライン", ["workingHoursPerDay"] = "8", ["products"] = new JsonArray() };
    [Fact]
    public async Task ConcurrentEquivalentCodesHaveExactlyOneWinner()
    {
        using var first = await fixture.CreateClientAsAsync("Admin"); using var second = await fixture.CreateClientAsAsync("Operator");
        var code = $"RACE-{Guid.NewGuid():N}";
        var replies = await Task.WhenAll(first.PostJson("/api/production-lines", LineDraft(code)), second.PostJson("/api/production-lines", LineDraft(" " + code.ToLowerInvariant() + " ")));
        Assert.Single(replies, r => r.StatusCode == HttpStatusCode.Created); Assert.Single(replies, r => r.StatusCode == HttpStatusCode.Conflict);
    }
    [Fact]
    public async Task ActualLockTimeoutIsBusyOnlyAfterRollbackAndDoesNotChangeAggregate()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var created = await client.PostJson("/api/production-lines", LineDraft($"BUSY-{Guid.NewGuid():N}")); created.EnsureSuccessStatusCode(); var line = await created.Body();
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString); await owner.OpenAsync();
        await using var transaction = await owner.BeginTransactionAsync();
        await using var command = new NpgsqlCommand($"SELECT id FROM production_lines WHERE id = '{line["id"]}' FOR UPDATE", owner, transaction);
        await command.ExecuteScalarAsync();
        var edit = new JsonObject { ["name"] = "must not persist", ["workingHoursPerDay"] = "7", ["version"] = line["version"]?.DeepClone(), ["productChanges"] = new JsonArray() };
        var pending = client.PutJson($"/api/production-lines/{line["id"]}", edit);
        await WaitForLock(fixture, "FROM production_lines l");
        var response = await pending;
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode); Assert.Equal("LINE_BUSY", (await response.Body())["code"]?.GetValue<string>());
        Assert.Equal("1", Assert.Single(response.Headers.GetValues("Retry-After")));
        await transaction.RollbackAsync();
        Assert.Equal(line.ToJsonString(), (await (await client.GetAsync($"/api/production-lines/{line["id"]}")).Body()).ToJsonString());
    }
    [Fact]
    public async Task CommittedRetirementBeforeSharedEligibilityLockRejectsDraftStart()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var lineId = await client.CreateEligibleLine(SteelBracket.ToString());
        var draft = ValidCreate(); draft["lineId"] = lineId; var order = await client.CreateOrder(draft);
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString); await owner.OpenAsync();
        await using var transaction = await owner.BeginTransactionAsync();
        await using var command = new NpgsqlCommand($"UPDATE production_lines SET is_active = false WHERE id = '{lineId}'", owner, transaction); await command.ExecuteNonQueryAsync();
        var start = UpdateFrom(order); start["status"] = "InProgress";
        var pending = client.PutJson($"/api/production-orders/{order["id"]}", start);
        await WaitForLock(fixture, "FROM production_lines l"); await transaction.CommitAsync();
        var response = await pending; Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("LINE_INELIGIBLE", (await response.Body())["code"]?.GetValue<string>());
        Assert.Equal("Draft", (await (await client.GetAsync($"/api/production-orders/{order["id"]}")).Body())["status"]?.GetValue<string>());
    }
    [Fact]
    public async Task OrderSharedLocksHoldThroughCommitThenRetirementPreservesHistory()
    {
        using var starter = await fixture.CreateClientAsAsync("Operator"); using var retireClient = await fixture.CreateClientAsAsync("Admin");
        var lineId = await starter.CreateEligibleLine(SteelBracket.ToString()); var draft = ValidCreate(); draft["lineId"] = lineId; var order = await starter.CreateOrder(draft);
        var line = await (await starter.GetAsync($"/api/production-lines/{lineId}")).Body();
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString); await owner.OpenAsync();
        await using var transaction = await owner.BeginTransactionAsync();
        await using var command = new NpgsqlCommand($"SELECT id FROM production_orders WHERE id = '{order["id"]}' FOR UPDATE", owner, transaction); await command.ExecuteScalarAsync();
        var start = UpdateFrom(order); start["status"] = "InProgress";
        var pendingStart = starter.PutJson($"/api/production-orders/{order["id"]}", start);
        await WaitForLock(fixture, "UPDATE production_orders");
        var pendingRetire = retireClient.PostJson($"/api/production-lines/{lineId}/retire", new JsonObject { ["version"] = line["version"]?.DeepClone() });
        await WaitForLock(fixture, "FROM production_lines l");
        await transaction.CommitAsync();
        Assert.Equal(HttpStatusCode.OK, (await pendingStart).StatusCode); Assert.Equal(HttpStatusCode.OK, (await pendingRetire).StatusCode);
        var saved = await (await starter.GetAsync($"/api/production-orders/{order["id"]}")).Body();
        Assert.Equal("InProgress", saved["status"]?.GetValue<string>()); Assert.Equal(lineId, saved["line"]?["id"]?.GetValue<string>());
        Assert.False(saved["line"]?["isActive"]?.GetValue<bool>() ?? true);
    }
    [Fact]
    public async Task TimingEditReadsFreshUnitRevisionAfterWaitingForConcurrentUnitUpdate()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var response = await client.PostJson("/api/product-master", new JsonObject { ["sku"] = $"UNIT-RACE-{Guid.NewGuid():N}", ["name"] = "fixture", ["unit"] = "kg" }); response.EnsureSuccessStatusCode();
        var product = await response.Body(); var productId = product["id"]!.GetValue<string>(); var lineId = await client.CreateEligibleLine(productId);
        var line = await (await client.GetAsync($"/api/production-lines/{lineId}")).Body(); var observation = line["pairs"]!["items"]![0]!["product"]!;
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString); await owner.OpenAsync(); await using var transaction = await owner.BeginTransactionAsync();
        await using var command = new NpgsqlCommand($"UPDATE products SET unit = 'm' WHERE id = '{productId}'", owner, transaction); await command.ExecuteNonQueryAsync();
        var edit = new JsonObject { ["name"] = line["name"]!.DeepClone(), ["workingHoursPerDay"] = "8", ["version"] = line["version"]!.DeepClone(), ["productChanges"] = new JsonArray(new JsonObject { ["action"] = "setTiming", ["productId"] = productId, ["minutesPerUnit"] = "0.125", ["expectedUnit"] = observation["unit"]!.DeepClone(), ["expectedUnitRevision"] = observation["unitRevision"]!.DeepClone(), ["confirmUnit"] = true }) };
        var pending = client.PutJson($"/api/production-lines/{lineId}", edit);
        await WaitForLock(fixture, "FROM products p"); await transaction.CommitAsync();
        var reply = await pending; Assert.Equal(HttpStatusCode.Conflict, reply.StatusCode); Assert.Equal("LINE_UNIT_STALE", (await reply.Body())["code"]!.GetValue<string>());
        var saved = await (await client.GetAsync($"/api/production-lines/{lineId}")).Body();
        Assert.Equal(line["version"]!.GetValue<string>(), saved["version"]!.GetValue<string>());
        Assert.Equal("1", saved["pairs"]!["items"]![0]!["minutesPerUnit"]!.GetValue<string>());
        Assert.True(saved["pairs"]!["items"]![0]!["requiresUnitConfirmation"]!.GetValue<bool>());
    }

    [Fact]
    public async Task ConcurrentPairRetirementBeforeEligibilityLockRejectsStart()
    {
        using var client = await fixture.CreateClientAsAsync("Operator"); var lineId = await client.CreateEligibleLine(SteelBracket.ToString());
        var draft = ValidCreate(); draft["lineId"] = lineId; var order = await client.CreateOrder(draft);
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString); await owner.OpenAsync(); await using var transaction = await owner.BeginTransactionAsync();
        await using var command = new NpgsqlCommand($"UPDATE production_line_products SET is_active = false WHERE line_id = '{lineId}'", owner, transaction); await command.ExecuteNonQueryAsync();
        var input = UpdateFrom(order); input["status"] = "InProgress"; var pending = client.PutJson($"/api/production-orders/{order["id"]}", input);
        await WaitForLock(fixture, "FROM production_line_products p"); await transaction.CommitAsync();
        var reply = await pending; Assert.Equal(HttpStatusCode.BadRequest, reply.StatusCode); Assert.Equal("LINE_INELIGIBLE", (await reply.Body())["code"]!.GetValue<string>());
        Assert.Equal("Draft", (await (await client.GetAsync($"/api/production-orders/{order["id"]}")).Body())["status"]!.GetValue<string>());
    }

    [Fact]
    public async Task ConcurrentOrderStatusChangeWinsXminWhileStartWaitsForItsUpdate()
    {
        using var client = await fixture.CreateClientAsAsync("Operator"); var lineId = await client.CreateEligibleLine(SteelBracket.ToString());
        var draft = ValidCreate(); draft["lineId"] = lineId; var order = await client.CreateOrder(draft);
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString); await owner.OpenAsync(); await using var transaction = await owner.BeginTransactionAsync();
        await using var command = new NpgsqlCommand($"UPDATE production_orders SET status = 'Cancelled' WHERE id = '{order["id"]}'", owner, transaction); await command.ExecuteNonQueryAsync();
        var input = UpdateFrom(order); input["status"] = "InProgress"; var pending = client.PutJson($"/api/production-orders/{order["id"]}", input);
        await WaitForLock(fixture, "UPDATE production_orders"); await transaction.CommitAsync();
        Assert.Equal(HttpStatusCode.Conflict, (await pending).StatusCode);
        Assert.Equal("Cancelled", (await (await client.GetAsync($"/api/production-orders/{order["id"]}")).Body())["status"]!.GetValue<string>());
    }

    [Fact]
    public async Task CancelledHttpWriteReleasesItsWaitAndPreservesAggregate()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var created = await client.PostJson("/api/production-lines", LineDraft($"CANCEL-{Guid.NewGuid():N}")); created.EnsureSuccessStatusCode(); var line = await created.Body();
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString); await owner.OpenAsync(); await using var transaction = await owner.BeginTransactionAsync();
        await using var command = new NpgsqlCommand($"SELECT id FROM production_lines WHERE id = '{line["id"]}' FOR UPDATE", owner, transaction); await command.ExecuteScalarAsync();
        var edit = new JsonObject { ["name"] = "cancelled draft", ["workingHoursPerDay"] = "7", ["version"] = line["version"]!.DeepClone(), ["productChanges"] = new JsonArray() };
        using var cancellation = new CancellationTokenSource();
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/production-lines/{line["id"]}") { Content = new StringContent(edit.ToJsonString(), System.Text.Encoding.UTF8, "application/json") };
        var pending = client.SendAsync(request, cancellation.Token);
        await WaitForLock(fixture, "FROM production_lines l"); cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
        var watch = Stopwatch.StartNew();
        while (await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM pg_stat_activity WHERE usename='pmai_app' AND wait_event_type='Lock' AND query LIKE '%FROM production_lines l%'") > 0 && watch.Elapsed < TimeSpan.FromSeconds(4)) await Task.Delay(30);
        Assert.Equal(0L, await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM pg_stat_activity WHERE usename='pmai_app' AND wait_event_type='Lock' AND query LIKE '%FROM production_lines l%'"));
        await transaction.RollbackAsync();
        Assert.Equal(line.ToJsonString(), (await (await client.GetAsync($"/api/production-lines/{line["id"]}")).Body()).ToJsonString());
    }

}
