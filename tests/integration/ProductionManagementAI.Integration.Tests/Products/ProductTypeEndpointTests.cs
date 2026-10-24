using System.Net;
using System.Text.Json.Nodes;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.Products;

/// <summary>WI-017 product type on the Product master API (004_DD-API-TYPE).</summary>
public sealed class ProductTypeEndpointTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string Path = "/api/product-master";

    private static JsonObject Draft(string sku, string? type) => new()
    {
        ["sku"] = sku, ["name"] = "種別テスト", ["type"] = type, ["unit"] = "個",
    };

    private static async Task<JsonObject> Created(HttpClient client, string type, string? sku = null)
    {
        var response = await client.PostJson(Path, Draft(sku ?? $"TY-{Guid.NewGuid():N}", type));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Body();
    }

    [Theory]
    [InlineData("FINISHED")]
    [InlineData("SEMI_FINISHED")]
    [InlineData("PART")]
    [InlineData("RAW_MATERIAL")]
    public async Task EachTypeIsCreatedAndReadBack(string type)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Created(client, type);
        Assert.Equal(type, product["type"]!.GetValue<string>());
        var detail = await (await client.GetAsync($"{Path}/{product["id"]}")).Body();
        Assert.Equal(type, detail["type"]!.GetValue<string>());
    }

    [Fact]
    public async Task MissingNullOrUnknownTypeIsAFieldError()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var missing = Draft($"TY-{Guid.NewGuid():N}", null);
        missing.Remove("type");
        foreach (var body in new[] { missing, Draft($"TY-{Guid.NewGuid():N}", null), Draft($"TY-{Guid.NewGuid():N}", "part") })
        {
            var response = await client.PostJson(Path, body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull((await response.Body())["errors"]!["type"]);
        }
        var number = await client.PostRaw(Path,
            $"{{\"sku\":\"TY-{Guid.NewGuid():N}\",\"name\":\"n\",\"type\":123,\"unit\":\"個\"}}");
        Assert.Equal(HttpStatusCode.BadRequest, number.StatusCode);
    }

    [Fact]
    public async Task TypeChangesOnRetiredUnitLockedProductAndBumpsVersion()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Created(client, "PART");
        var id = product["id"]!.GetValue<string>();
        var order = await client.PostRaw("/api/production-orders",
            $"{{\"productId\":\"{id}\",\"quantity\":1,\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}");
        Assert.Equal(HttpStatusCode.Created, order.StatusCode);
        var retired = await (await client.PostJson($"{Path}/{id}/retire",
            new JsonObject { ["version"] = product["version"]!.GetValue<uint>() })).Body();
        Assert.Equal("PART", retired["type"]!.GetValue<string>());
        Assert.True(retired["unitLocked"]!.GetValue<bool>());

        var edit = Draft(product["sku"]!.GetValue<string>(), "SEMI_FINISHED");
        edit["version"] = retired["version"]!.GetValue<uint>();
        var saved = await client.PutJson($"{Path}/{id}", edit);
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var body = await saved.Body();
        Assert.Equal("SEMI_FINISHED", body["type"]!.GetValue<string>());
        Assert.NotEqual(retired["version"]!.GetValue<uint>(), body["version"]!.GetValue<uint>());
        Assert.False(body["isActive"]!.GetValue<bool>());
    }

    [Fact]
    public async Task InvalidTypeIsReportedBeforeStaleVersion()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Created(client, "PART");
        var edit = Draft(product["sku"]!.GetValue<string>(), "X");
        edit["version"] = product["version"]!.GetValue<uint>() + 99;
        var response = await client.PutJson($"{Path}/{product["id"]}", edit);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull((await response.Body())["errors"]!["type"]);
    }

    [Fact]
    public async Task ListFiltersByTypeWithSearchStateAndPageSize()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var prefix = $"TF-{Guid.NewGuid():N}";
        await Created(client, "PART", $"{prefix}-1");
        await Created(client, "PART", $"{prefix}-2");
        var raw = await Created(client, "RAW_MATERIAL", $"{prefix}-3");

        var parts = await (await client.GetAsync($"{Path}?q={prefix}&type=PART&pageSize=10")).Body();
        Assert.Equal(2, parts["total"]!.GetValue<int>());
        Assert.All(parts["items"]!.AsArray(), item => Assert.Equal("PART", item!["type"]!.GetValue<string>()));
        Assert.Equal(10, parts["pageSize"]!.GetValue<int>());

        await client.PostJson($"{Path}/{raw["id"]}/retire",
            new JsonObject { ["version"] = raw["version"]!.GetValue<uint>() });
        var retired = await (await client.GetAsync($"{Path}?q={prefix}&type=RAW_MATERIAL&state=retired")).Body();
        Assert.Equal(1, retired["total"]!.GetValue<int>());
        var activeRaw = await (await client.GetAsync($"{Path}?q={prefix}&type=RAW_MATERIAL&state=active")).Body();
        Assert.Equal(0, activeRaw["total"]!.GetValue<int>());
        var all = await (await client.GetAsync($"{Path}?q={prefix}&type=")).Body();
        Assert.Equal(3, all["total"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("part")]
    public async Task UnknownListTypeIsRejected(string type)
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var response = await client.GetAsync($"{Path}?type={type}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull((await response.Body())["errors"]!["type"]);
    }

    [Fact]
    public async Task RetireRejectsATypeProperty()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var product = await Created(client, "PART");
        var response = await client.PostRaw($"{Path}/{product["id"]}/retire",
            $"{{\"version\":{product["version"]},\"type\":\"PART\"}}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AppRoleMayUpdateTypeButTheDatabaseRejectsUnknownCodes()
    {
        var granted = await fixture.ScalarAsOwnerAsync<long>(
            "SELECT count(*) FROM information_schema.column_privileges WHERE grantee = 'pmai_app' AND table_name = 'products' AND column_name = 'product_type' AND privilege_type = 'UPDATE'");
        Assert.Equal(1L, granted);
        await Assert.ThrowsAsync<Npgsql.PostgresException>(() => fixture.ExecuteAsOwnerAsync(
            "UPDATE products SET product_type = 'OTHER' WHERE sku = 'P-1001'"));
    }
}
