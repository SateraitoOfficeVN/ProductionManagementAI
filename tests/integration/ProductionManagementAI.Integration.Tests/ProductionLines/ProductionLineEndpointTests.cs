using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.ProductionLines;

/// <summary>Exercises WI-009 strict boundaries, real aggregate writes and preserved unit generations.</summary>
public sealed class ProductionLineEndpointTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string Path = "/api/production-lines";
    private static JsonObject Draft(string? code = null, JsonArray? products = null) => new() {
        ["code"] = code ?? $"L-{Guid.NewGuid():N}", ["name"] = "生産ライン", ["workingHoursPerDay"] = "8.000", ["products"] = products ?? new() };
    private static JsonObject Edit(JsonObject line, JsonArray? changes = null) => new() {
        ["name"] = "更新ライン", ["workingHoursPerDay"] = "7.5", ["version"] = line["version"]?.DeepClone(), ["productChanges"] = changes ?? new() };
    private static JsonObject Input(JsonObject product, string? action = null, bool confirm = true) {
        var row = new JsonObject { ["productId"] = product["id"]?.DeepClone(), ["minutesPerUnit"] = "0.125",
            ["expectedUnit"] = product["unit"]?.DeepClone(), ["expectedUnitRevision"] = product["unitRevision"]?.DeepClone(), ["confirmUnit"] = confirm };
        if (action is not null) row["action"] = action;
        return row;
    }
    private static async Task<JsonObject> Create(HttpClient client, JsonObject draft) {
        var response = await client.PostJson(Path, draft);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        return await response.Body();
    }
    private static async Task<JsonObject> Product(HttpClient client) {
        var response = await client.PostJson("/api/product-master", new JsonObject {
            ["sku"] = $"LP-{Guid.NewGuid():N}", ["name"] = "製品", ["unit"] = "kg", ["drawingNumber"] = null });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Body();
        var choices = await (await client.GetAsync($"{Path}/product-choices?q={product["sku"]}")).Body();
        return (Assert.Single(choices["items"]?.AsArray() ?? throw new InvalidOperationException())
            ?? throw new InvalidOperationException("Product choice is null.")).AsObject();
    }
    [Fact]
    public async Task RealRestrictedRoleCreatesUpdatesAndRetiresAnAggregate() {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Product(client);
        var line = await Create(client, Draft(products: new(Input(product))));
        Assert.Equal("8", line["workingHoursPerDay"]?.GetValue<string>());
        Assert.Equal("0.125", line["pairs"]?["items"]?[0]?["minutesPerUnit"]?.GetValue<string>());
        Assert.EndsWith("Z", line["updatedAt"]?.GetValue<string>());
        var savedResponse = await client.PutJson($"{Path}/{line["id"]}", Edit(line));
        Assert.Equal(HttpStatusCode.OK, savedResponse.StatusCode);
        var saved = await savedResponse.Body();
        Assert.NotEqual(line["version"]?.GetValue<string>(), saved["version"]?.GetValue<string>());
        Assert.Single(saved["pairs"]?["items"]?.AsArray() ?? throw new InvalidOperationException());
        var eligible = await (await client.GetAsync($"{Path}/eligible?productId={product["id"]}")).Body();
        Assert.Contains(eligible["items"]?.AsArray() ?? throw new InvalidOperationException(), row => row?["id"]?.GetValue<string>() == line["id"]?.GetValue<string>());
        var retired = await client.PostJson($"{Path}/{line["id"]}/retire", new JsonObject { ["version"] = saved["version"]?.DeepClone() });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);
        var inactive = await retired.Body();
        Assert.False(inactive["isActive"]?.GetValue<bool>() ?? true);
        var again = await client.PostJson($"{Path}/{line["id"]}/retire", new JsonObject { ["version"] = inactive["version"]?.DeepClone() });
        Assert.Equal("LINE_ALREADY_RETIRED", (await again.Body())["code"]?.GetValue<string>());
        var corrected = await client.PutJson($"{Path}/{line["id"]}", Edit(inactive));
        Assert.Equal(HttpStatusCode.OK, corrected.StatusCode);
        Assert.False((await corrected.Body())["isActive"]?.GetValue<bool>() ?? true);
    }
    [Fact]
    public async Task AtomicChangesPreserveOmittedPairsAndRetiredPairsCannotBeReadded() {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var first = await Product(client); var second = await Product(client);
        var line = await Create(client, Draft(products: new(Input(first))));
        var invalid = Input(second, "add"); invalid["expectedUnitRevision"] = "1";
        var changes = new JsonArray(Input(first, "setTiming"), invalid);
        var response = await client.PutJson($"{Path}/{line["id"]}", Edit(line, changes));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("LINE_UNIT_STALE", (await response.Body())["code"]?.GetValue<string>());
        var unchanged = await (await client.GetAsync($"{Path}/{line["id"]}")).Body();
        Assert.Equal(line.ToJsonString(), unchanged.ToJsonString());
        var retirement = new JsonObject { ["action"] = "retire", ["productId"] = first["id"]?.DeepClone() };
        var retired = await client.PutJson($"{Path}/{line["id"]}", Edit(line, new(retirement)));
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);
        var saved = await retired.Body();
        Assert.False(saved["pairs"]?["items"]?[0]?["isActive"]?.GetValue<bool>() ?? true);
        Assert.NotEqual(line["version"]?.GetValue<string>(), saved["version"]?.GetValue<string>());
        var readd = await client.PutJson($"{Path}/{line["id"]}", Edit(saved, new(Input(first, "add"))));
        Assert.Equal("LINE_PAIR_INVALID", (await readd.Body())["code"]?.GetValue<string>());
        var choices = await (await client.GetAsync($"{Path}/product-choices?lineId={line["id"]}&q={first["sku"]}")).Body();
        Assert.Empty(choices["items"]?.AsArray() ?? throw new InvalidOperationException());
    }
    [Fact]
    public async Task AbaUnitChangeRequiresFreshObservationAndExplicitReconfirmation() {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var product = await Product(client);
        var line = await Create(client, Draft(products: new(Input(product))));
        var id = product["id"]?.GetValue<string>() ?? throw new InvalidOperationException();
        await fixture.ExecuteAsOwnerAsync($"UPDATE products SET unit = 'm' WHERE id = '{id}'; UPDATE products SET unit = 'kg' WHERE id = '{id}'");
        var eligible = await (await client.GetAsync($"{Path}/eligible?productId={id}")).Body();
        Assert.Empty(eligible["items"]?.AsArray() ?? throw new InvalidOperationException());
        var stale = await client.PutJson($"{Path}/{line["id"]}", Edit(line, new(Input(product, "setTiming"))));
        Assert.Equal("LINE_UNIT_STALE", (await stale.Body())["code"]?.GetValue<string>());
        product["unitRevision"] = "2";
        var unconfirmed = await client.PutJson($"{Path}/{line["id"]}", Edit(line, new(Input(product, "setTiming", false))));
        Assert.Equal("LINE_UNIT_CONFIRMATION_REQUIRED", (await unconfirmed.Body())["code"]?.GetValue<string>());
        var confirmed = await client.PutJson($"{Path}/{line["id"]}", Edit(line, new(Input(product, "setTiming"))));
        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
        Assert.False((await confirmed.Body())["pairs"]?["items"]?[0]?["requiresUnitConfirmation"]?.GetValue<bool>() ?? true);
    }
    [Fact]
    public async Task ImmutableCodeUniquenessAndVersionPrecedenceAreEnforced() {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var line = await Create(client, Draft());
        var duplicate = await client.PostJson(Path, Draft("  " + line["code"]?.GetValue<string>().ToLowerInvariant() + "  "));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("LINE_CODE_CONFLICT", (await duplicate.Body())["code"]?.GetValue<string>());
        Assert.Equal(HttpStatusCode.OK, (await client.PutJson($"{Path}/{line["id"]}", Edit(line))).StatusCode);
        var nonexistent = new JsonObject { ["action"] = "retire", ["productId"] = Guid.NewGuid().ToString() };
        var stale = await client.PutJson($"{Path}/{line["id"]}", Edit(line, new(nonexistent)));
        Assert.Equal("LINE_STALE", (await stale.Body())["code"]?.GetValue<string>());
    }
    [Theory]
    [InlineData("{\"code\":\"A\",\"code\":\"B\",\"name\":\"Name\",\"workingHoursPerDay\":\"8\",\"products\":[]}")]
    [InlineData("{\"code\":\"A\",\"name\":\"Name\",\"workingHoursPerDay\":8,\"products\":[]}")]
    [InlineData("{\"code\":\"A\",\"name\":\"Name\",\"workingHoursPerDay\":\"8\",\"products\":[1]}")]
    [InlineData("{\"code\":\"A\",\"name\":\"Name\",\"workingHoursPerDay\":\"8\",\"products\":null}")]
    [InlineData("{\"code\":\"A\",\"name\":\"Name\",\"workingHoursPerDay\":\"8\",\"products\":[],\"isActive\":true}")]
    public async Task StrictMutationBodyRejectsDuplicateUnknownAndWrongScalar(string json) {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var response = await client.PostRaw(Path, json);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION", (await response.Body())["code"]?.GetValue<string>());
    }
    [Fact]
    public async Task RoutesQueryLimitsBodyLimitsAndAuthorizationAreFeatureScoped() {
        using var anonymous = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Path)).StatusCode);
        using var forbidden = await fixture.CreateClientAsAsync(null);
        Assert.Equal(HttpStatusCode.Forbidden, (await forbidden.PostJson(Path, Draft())).StatusCode);
        using var client = await fixture.CreateClientAsAsync("Operator");
        foreach (var query in new[] { "?page=10001", "?page=1&page=2", "?pageSize=10", "/bad-id", "?state=Active" })
            Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync(Path + query)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(Path + "/product-choices")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(Path + "/eligible?productId=" + Guid.NewGuid())).StatusCode);
        var media = await client.PostAsync(Path, new StringContent("{}", Encoding.UTF8, "text/plain"));
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, media.StatusCode);
        var oversize = await client.PostRaw(Path, new string(' ', 256 * 1024 + 1));
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversize.StatusCode);
    }
    [Theory]
    [InlineData("GET", "")]
    [InlineData("GET", "/0197e4a0-0000-7000-8000-000000009001")]
    [InlineData("GET", "/product-choices")]
    [InlineData("GET", "/eligible?productId=0197e4a0-0000-7000-8000-000000001001")]
    [InlineData("POST", "")]
    [InlineData("PUT", "/0197e4a0-0000-7000-8000-000000009001")]
    [InlineData("POST", "/0197e4a0-0000-7000-8000-000000009001/retire")]
    public async Task AllSevenOperationsDenyAnonymousAndUnprivilegedUsersWithNoStore(string method, string suffix)
    {
        using var anonymous = fixture.CreateClient(); using var forbidden = await fixture.CreateClientAsAsync(null);
        foreach (var entry in new[] { (anonymous, HttpStatusCode.Unauthorized), (forbidden, HttpStatusCode.Forbidden) })
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), Path + suffix) { Content = method == "GET" ? null : new StringContent("{}", Encoding.UTF8, "application/json") };
            using var response = await entry.Item1.SendAsync(request);
            Assert.Equal(entry.Item2, response.StatusCode);
            Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        }
    }

}
