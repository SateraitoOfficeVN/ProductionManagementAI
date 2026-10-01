using System.Net;
using System.Text.Json.Nodes;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.ProductionLines;

/// <summary>Verifies WI-009 omitted/null history semantics, eligibility and original Draft locks through HTTP.</summary>
public sealed class OrderLineEndpointTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task DraftPresencePreservesClearsAndStartRequiresEligibleAssignment()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var order = await client.CreateOrder();
        Assert.True(order.ContainsKey("line")); Assert.Null(order["line"]);
        var id = order["id"]?.GetValue<string>() ?? throw new InvalidOperationException();
        var start = UpdateFrom(order); start["status"] = "InProgress";
        var missing = await client.PutJson($"/api/production-orders/{id}", start);
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal("LINE_REQUIRED", (await missing.Body())["code"]?.GetValue<string>());
        var lineId = await client.CreateEligibleLine(order["productId"]?.GetValue<string>() ?? throw new InvalidOperationException());
        var assign = UpdateFrom(order); assign["lineId"] = lineId;
        var assigned = await (await client.PutJson($"/api/production-orders/{id}", assign)).Body();
        Assert.Equal(lineId, assigned["line"]?["id"]?.GetValue<string>());
        var omitted = UpdateFrom(assigned); omitted["notes"] = "preserve";
        var preserved = await (await client.PutJson($"/api/production-orders/{id}", omitted)).Body();
        Assert.Equal(lineId, preserved["line"]?["id"]?.GetValue<string>());
        var clear = UpdateFrom(preserved); clear["lineId"] = null;
        var cleared = await (await client.PutJson($"/api/production-orders/{id}", clear)).Body(); Assert.Null(cleared["line"]);
        var begin = UpdateFrom(cleared); begin["lineId"] = lineId; begin["status"] = "InProgress";
        var begunResponse = await client.PutJson($"/api/production-orders/{id}", begin); Assert.Equal(HttpStatusCode.OK, begunResponse.StatusCode);
        var begun = await begunResponse.Body();
        var locked = UpdateFrom(begun); locked["lineId"] = null;
        Assert.Equal("LINE_LOCKED", (await (await client.PutJson($"/api/production-orders/{id}", locked)).Body())["code"]?.GetValue<string>());
        var list = await (await client.GetAsync($"/api/production-orders?orderNumber={begun["orderNumber"]}")).Body();
        Assert.Equal(lineId, list["items"]?[0]?["line"]?["id"]?.GetValue<string>());
    }
    [Fact]
    public async Task HistoricalRetiredPairMayBeKeptButDraftStartRechecksAndLegacyNullSurvives()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var lineId = await client.CreateEligibleLine(SteelBracket.ToString());
        var draft = ValidCreate(); draft["lineId"] = lineId;
        var order = await client.CreateOrder(draft);
        var line = await (await client.GetAsync($"/api/production-lines/{lineId}")).Body();
        var retired = await client.PostJson($"/api/production-lines/{lineId}/retire", new JsonObject { ["version"] = line["version"]?.DeepClone() });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);
        var keptResponse = await client.PutJson($"/api/production-orders/{order["id"]}", UpdateFrom(order)); Assert.Equal(HttpStatusCode.OK, keptResponse.StatusCode);
        var kept = await keptResponse.Body(); Assert.False(kept["line"]?["isActive"]?.GetValue<bool>() ?? true);
        var start = UpdateFrom(kept); start["status"] = "InProgress";
        var blocked = await client.PutJson($"/api/production-orders/{order["id"]}", start);
        Assert.Equal("LINE_INELIGIBLE", (await blocked.Body())["code"]?.GetValue<string>());
        var newDraft = ValidCreate(); newDraft["lineId"] = lineId;
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostJson("/api/production-orders", newDraft)).StatusCode);
        var legacy = await client.CreateOrder();
        await fixture.ExecuteAsOwnerAsync($"UPDATE production_orders SET status = 'InProgress' WHERE id = '{legacy["id"]}'");
        legacy = await (await client.GetAsync($"/api/production-orders/{legacy["id"]}")).Body();
        var finish = UpdateFrom(legacy); finish["status"] = "Completed";
        var saved = await client.PutJson($"/api/production-orders/{legacy["id"]}", finish);
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode); Assert.Null((await saved.Body())["line"]);
    }
    [Fact]
    public async Task DraftProductChangeRejectsRetainedIncompatibleLineAndWrongTypedLineIsInvalid()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var lineId = await client.CreateEligibleLine(SteelBracket.ToString());
        var draft = ValidCreate(); draft["lineId"] = lineId;
        var order = await client.CreateOrder(draft);
        var changed = UpdateFrom(order); changed["productId"] = DriveShaft.ToString();
        var response = await client.PutJson($"/api/production-orders/{order["id"]}", changed);
        Assert.Equal("LINE_INELIGIBLE", (await response.Body())["code"]?.GetValue<string>());
        changed["lineId"] = null;
        Assert.Equal(HttpStatusCode.OK, (await client.PutJson($"/api/production-orders/{order["id"]}", changed)).StatusCode);
        var invalid = ValidCreate(); invalid["lineId"] = 123;
        var bad = await client.PostJson("/api/production-orders", invalid); Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        Assert.NotNull((await bad.Body())["errors"]?["lineId"]);
    }
}
