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
    [Theory]
    [InlineData("10")]
    [InlineData("20")]
    [InlineData("50")]
    [InlineData("100")]
    public async Task ListDetailAndChoicesAcceptAllowListedPageSizes(string size) {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var line = await Create(client, Draft());
        var list = await client.GetAsync($"{Path}?state=all&pageSize={size}");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var page = await list.Body();
        Assert.Equal(int.Parse(size), page["pageSize"]?.GetValue<int>());
        Assert.True((page["items"]?.AsArray().Count ?? 0) <= int.Parse(size));
        var detail = await (await client.GetAsync($"{Path}/{line["id"]}?pairsPageSize={size}")).Body();
        Assert.Equal(int.Parse(size), detail["pairs"]?["pageSize"]?.GetValue<int>());
        var choices = await client.GetAsync($"{Path}/product-choices?pageSize={size}");
        Assert.Equal(HttpStatusCode.OK, choices.StatusCode);
        var choicePage = await choices.Body();
        Assert.Equal(int.Parse(size), choicePage["pageSize"]?.GetValue<int>());
        Assert.True((choicePage["items"]?.AsArray().Count ?? 0) <= int.Parse(size));
    }
    [Fact]
    public async Task OmittedPageSizesKeepTheFiftyRowDefault() {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var line = await Create(client, Draft());
        Assert.Equal(50, (await (await client.GetAsync(Path)).Body())["pageSize"]?.GetValue<int>());
        Assert.Equal(50, (await (await client.GetAsync($"{Path}/{line["id"]}")).Body())["pairs"]?["pageSize"]?.GetValue<int>());
        Assert.Equal(50, (await (await client.GetAsync($"{Path}/product-choices")).Body())["pageSize"]?.GetValue<int>());
    }
    [Fact]
    public async Task PairPagesHonourTheRequestedSize() {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var products = new JsonArray();
        for (var i = 0; i < 3; i++) products.Add(Input(await Product(client)));
        var line = await Create(client, Draft(products: products));
        var first = await (await client.GetAsync($"{Path}/{line["id"]}?pairsPage=1&pairsPageSize=10")).Body();
        Assert.Equal(3, first["pairs"]?["total"]?.GetValue<int>());
        Assert.Equal(3, first["pairs"]?["items"]?.AsArray().Count);
        var beyond = await (await client.GetAsync($"{Path}/{line["id"]}?pairsPage=2&pairsPageSize=10")).Body();
        Assert.Empty(beyond["pairs"]?["items"]?.AsArray() ?? throw new InvalidOperationException());
    }
    [Theory]
    [InlineData("", "pageSize=25", "pageSize")]
    [InlineData("", "pageSize=abc", "pageSize")]
    [InlineData("/product-choices", "pageSize=0", "pageSize")]
    [InlineData("/product-choices", "pageSize=1000", "pageSize")]
    public async Task RejectedPageSizesNameTheirField(string suffix, string query, string field) {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var response = await client.GetAsync($"{Path}{suffix}?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Body();
        Assert.Equal("VALIDATION", body["code"]?.GetValue<string>());
        Assert.NotNull(body["errors"]?[field]);
    }
    [Fact]
    public async Task RejectedPairPageSizeNamesItsField() {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var line = await Create(client, Draft());
        foreach (var size in new[] { "25", "0", "x" }) {
            var response = await client.GetAsync($"{Path}/{line["id"]}?pairsPageSize={size}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull((await response.Body())["errors"]?["pairsPageSize"]);
        }
        var page = await client.GetAsync($"{Path}/{line["id"]}?pairsPage=0&pairsPageSize=20");
        Assert.NotNull((await page.Body())["errors"]?["pairsPage"]);
    }
    [Fact]
    public async Task EligibleLinesStillRejectAPageSize() {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Product(client);
        var response = await client.GetAsync($"{Path}/eligible?productId={product["id"]}&pageSize=20");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task RoutesQueryLimitsBodyLimitsAndAuthorizationAreFeatureScoped() {
        using var anonymous = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Path)).StatusCode);
        using var forbidden = await fixture.CreateClientAsAsync(null);
        Assert.Equal(HttpStatusCode.Forbidden, (await forbidden.PostJson(Path, Draft())).StatusCode);
        using var client = await fixture.CreateClientAsAsync("Operator");
        foreach (var query in new[] { "?page=10001", "?page=1&page=2", "?pageSize=25", "?pageSize=20&pageSize=50", "/bad-id", "?state=Active" })
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
