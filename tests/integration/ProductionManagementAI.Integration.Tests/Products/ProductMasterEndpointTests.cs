using System.Net;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Collections.Concurrent;
using ProductionManagementAI.Application.Products;
using ProductionManagementAI.Application.ProductionOrders;
using Npgsql;
using System.Text.Json.Nodes;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.Products;

/// <summary>WI-006 Product master HTTP, role, concurrency, and order-reference integration coverage.</summary>
public sealed class ProductMasterEndpointTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string Path = "/api/product-master";

    private static JsonObject Draft(string sku, string unit = "kg") => new()
    {
        ["sku"] = sku, ["name"] = "テスト製品", ["unit"] = unit, ["drawingNumber"] = " D-001 ",
    };

    private static async Task<JsonObject> Created(HttpClient client, string? unit = null)
    {
        var response = await client.PostJson(Path, Draft($"T-{Guid.NewGuid():N}", unit ?? "kg"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Body();
    }

    [Fact]
    public async Task CatalogIsPagedAndSearchesEscapedSkuWithoutHidingRetiredRows()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var product = await Created(client);
        var sku = product["sku"]!.GetValue<string>();
        var response = await client.GetAsync($"{Path}?q={Uri.EscapeDataString(sku)}&state=active&page=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Body();
        Assert.Equal(20, page["pageSize"]!.GetValue<int>());
        Assert.Contains(page["items"]!.AsArray(), item => item!["sku"]!.GetValue<string>() == sku);
        Assert.Equal("kg", product["unit"]!.GetValue<string>());
        Assert.False(product["unitLocked"]!.GetValue<bool>());
        Assert.Equal("D-001", product["drawingNumber"]!.GetValue<string>());

        var retired = await client.PostJson($"{Path}/{product["id"]}/retire",
            new JsonObject { ["version"] = product["version"]!.GetValue<uint>() });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);
        var detail = await client.GetAsync($"{Path}/{product["id"]}");
        Assert.False((await detail.Body())["isActive"]!.GetValue<bool>());
        var retiredPage = await (await client.GetAsync($"{Path}?q={Uri.EscapeDataString(sku)}&state=retired")).Body();
        Assert.Single(retiredPage["items"]!.AsArray());
    }

    [Fact]
    public async Task DuplicateSkuStaleVersionAndReferencedUnitAreRejected()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Created(client);
        var id = product["id"]!.GetValue<string>();
        var sku = product["sku"]!.GetValue<string>();
        var duplicate = await client.PostJson(Path, Draft(sku.ToUpperInvariant()));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("PRODUCT_SKU_CONFLICT", (await duplicate.Body())["code"]!.GetValue<string>());

        var edit = Draft(sku);
        edit["name"] = "改訂製品";
        edit["version"] = product["version"]!.GetValue<uint>();
        var saved = await client.PutJson($"{Path}/{id}", edit);
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var stale = await client.PutJson($"{Path}/{id}", edit);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("PRODUCT_STALE", (await stale.Body())["code"]!.GetValue<string>());

        var orderJson = $"{{\"productId\":\"{id}\",\"quantity\":0.125,\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}";
        var order = await client.PostRaw("/api/production-orders", orderJson);
        Assert.Equal(HttpStatusCode.Created, order.StatusCode);
        var locked = await (await client.GetAsync($"{Path}/{id}")).Body();
        Assert.True(locked["unitLocked"]!.GetValue<bool>());
        edit["unit"] = "m";
        edit["version"] = locked["version"]!.GetValue<uint>();
        var changedUnit = await client.PutJson($"{Path}/{id}", edit);
        Assert.Equal(HttpStatusCode.Conflict, changedUnit.StatusCode);
        Assert.Equal("PRODUCT_UNIT_LOCKED", (await changedUnit.Body())["code"]!.GetValue<string>());
    }

    [Fact]
    public async Task ConcurrentEquivalentSkuCreatesHaveOneWinnerAndOneConflict()
    {
        using var first = await fixture.CreateClientAsAsync("Operator");
        using var second = await fixture.CreateClientAsAsync("Admin");
        var sku = $"CASE-{Guid.NewGuid():N}";
        var responses = await Task.WhenAll(first.PostJson(Path, Draft(sku)),
            second.PostJson(Path, Draft($"  {sku.ToLowerInvariant()}  ")));
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
        var exact = await first.PostJson(Path, Draft(sku));
        Assert.Equal(HttpStatusCode.Conflict, exact.StatusCode);
        Assert.Equal("PRODUCT_SKU_CONFLICT", (await exact.Body())["code"]!.GetValue<string>());
    }

    [Fact]
    public async Task InvalidProductFieldsChangedSkuAndUnknownBodyPropertiesAreRejected()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var invalid = await client.PostJson(Path, new JsonObject
        {
            ["sku"] = "  ", ["name"] = "", ["unit"] = "litre", ["drawingNumber"] = new string('D', 101),
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var errors = (await invalid.Body())["errors"]!.AsObject();
        Assert.Contains("sku", errors.Select(x => x.Key));
        Assert.Contains("name", errors.Select(x => x.Key));
        Assert.Contains("unit", errors.Select(x => x.Key));
        Assert.Contains("drawingNumber", errors.Select(x => x.Key));

        var product = await Created(client);
        var changed = Draft("CHANGED-SKU");
        changed["version"] = product["version"]!.GetValue<uint>();
        var response = await client.PutJson($"{Path}/{product["id"]}", changed);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull((await response.Body())["errors"]!["sku"]);

        var unknown = await client.PostRaw(Path,
            "{\"sku\":\"T-1\",\"name\":\"Name\",\"unit\":\"kg\",\"isActive\":false}");
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
    }

    [Fact]
    public async Task RetiredProductCannotBeNewSelectionButExistingOrderKeepsIt()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var product = await Created(client, "個");
        var id = product["id"]!.GetValue<string>();
        var order = await client.PostRaw("/api/production-orders",
            $"{{\"productId\":\"{id}\",\"quantity\":2,\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}");
        Assert.Equal(HttpStatusCode.Created, order.StatusCode);
        var retired = await client.PostJson($"{Path}/{id}/retire",
            new JsonObject { ["version"] = product["version"]!.GetValue<uint>() });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);

        var rejected = await client.PostRaw("/api/production-orders",
            $"{{\"productId\":\"{id}\",\"quantity\":2,\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}");
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Equal("PRODUCT_INACTIVE", (await rejected.Body())["code"]!.GetValue<string>());
        var detail = await order.Body();
        var historical = await (await client.GetAsync($"/api/production-orders/{detail["id"]}")).Body();
        Assert.Equal(id, historical["productId"]!.GetValue<string>());
        Assert.Equal("個", historical["unit"]!.GetValue<string>());

        var update = new JsonObject
        {
            ["productId"] = id,
            ["quantity"] = 2,
            ["dueDate"] = Date(PlantToday.AddDays(11)),
            ["status"] = "Draft",
            ["notes"] = "Historical correction",
            ["version"] = historical["version"]!.GetValue<uint>(),
        };
        var unchangedProduct = await client.PutJson($"/api/production-orders/{detail["id"]}", update);
        Assert.Equal(HttpStatusCode.OK, unchangedProduct.StatusCode);
    }

    [Theory]
    [InlineData("個")]
    [InlineData("本")]
    [InlineData("枚")]
    [InlineData("台")]
    [InlineData("セット")]
    public async Task DiscreteUnitRejectsFractionalQuantityButMeasuredUnitKeepsExactValue(string unit)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var discrete = await Created(client, unit);
        var rejected = await client.PostRaw("/api/production-orders",
            $"{{\"productId\":\"{discrete["id"]}\",\"quantity\":1.25,\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}");
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Equal("QUANTITY_UNIT_INVALID", (await rejected.Body())["code"]!.GetValue<string>());

        var measured = await Created(client, "m");
        var accepted = await client.PostRaw("/api/production-orders",
            $"{{\"productId\":\"{measured["id"]}\",\"quantity\":1.234,\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}");
        Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        var saved = await accepted.Body();
        Assert.Equal(1.234m, saved["quantity"]!.GetValue<decimal>());
        Assert.Equal("m", saved["unit"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("1.2340")]
    [InlineData("1e1")]
    [InlineData("0.0001")]
    public async Task ExactQuantityRejectsUnsupportedTokens(string token)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Created(client);
        var order = await client.PostRaw("/api/production-orders",
            $"{{\"productId\":\"{product["id"]}\",\"quantity\":{token},\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}");
        Assert.Equal(HttpStatusCode.BadRequest, order.StatusCode);
        Assert.Equal("QUANTITY_UNIT_INVALID", (await order.Body())["code"]!.GetValue<string>());
    }

    [Fact]
    public async Task OrderCreateWaitsForRetirementLockAndRejectsCommittedRetirement()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Created(client);
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString);
        await owner.OpenAsync();
        await using var transaction = await owner.BeginTransactionAsync();
        await using var command = new NpgsqlCommand(
            "UPDATE products SET is_active = false WHERE id = @id", owner, transaction);
        command.Parameters.AddWithValue("id", Guid.Parse(product["id"]!.GetValue<string>()));
        await command.ExecuteNonQueryAsync();
        var pending = client.PostRaw("/api/production-orders",
            $"{{\"productId\":\"{product["id"]}\",\"quantity\":1.234,\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}");
        await WaitForProductLockAsync();
        Assert.False(pending.IsCompleted);
        await transaction.CommitAsync();
        var response = await pending.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("PRODUCT_INACTIVE", (await response.Body())["code"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProductMutationWaitsForOrderShareLockAndChecksCommittedReference(bool retire)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Created(client);
        var id = Guid.Parse(product["id"]!.GetValue<string>());
        await using var owner = new NpgsqlConnection(fixture.OwnerConnectionString);
        await owner.OpenAsync();
        await using var transaction = await owner.BeginTransactionAsync();
        await using (var locked = new NpgsqlCommand("SELECT id FROM products WHERE id = @id FOR SHARE", owner, transaction))
        {
            locked.Parameters.AddWithValue("id", id);
            await locked.ExecuteScalarAsync();
        }
        var edit = Draft(product["sku"]!.GetValue<string>(), "m");
        edit["version"] = product["version"]!.GetValue<uint>();
        var pending = retire
            ? client.PostJson($"{Path}/{id}/retire", new JsonObject { ["version"] = product["version"]!.GetValue<uint>() })
            : client.PutJson($"{Path}/{id}", edit);
        await WaitForProductLockAsync();
        Assert.False(pending.IsCompleted);
        await using (var insert = new NpgsqlCommand(
            "INSERT INTO production_orders (id, order_year, order_seq, product_id, quantity, due_date, status, created_at_utc, updated_at_utc) VALUES (@order, 2098, @sequence, @id, 1.234, @due, 'Draft', now(), now())", owner, transaction))
        {
            insert.Parameters.AddWithValue("order", Guid.NewGuid());
            insert.Parameters.AddWithValue("sequence", retire ? 2 : 1);
            insert.Parameters.AddWithValue("id", id);
            insert.Parameters.AddWithValue("due", PlantToday.AddDays(10));
            await insert.ExecuteNonQueryAsync();
        }
        await transaction.CommitAsync();
        var response = await pending.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(retire ? HttpStatusCode.OK : HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Body();
        if (retire) { Assert.False(body["isActive"]!.GetValue<bool>()); Assert.True(body["unitLocked"]!.GetValue<bool>()); }
        else Assert.Equal("PRODUCT_UNIT_LOCKED", body["code"]!.GetValue<string>());
    }

    private async Task WaitForProductLockAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var waiting = await fixture.ScalarAsOwnerAsync<long>(
                "SELECT count(*) FROM pg_stat_activity WHERE usename = 'pmai_app' AND wait_event_type = 'Lock' AND query LIKE '%products%FOR %'");
            if (waiting > 0) return;
            await Task.Delay(25);
        }
        Assert.Fail("The product operation did not reach its expected PostgreSQL row lock.");
    }

    [Fact]
    public async Task MixedUnitDashboardKeepsExactSubtotalsAndCountBasedRanking()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var before = await (await client.GetAsync("/api/dashboard")).Body();
        static decimal Total(JsonObject snapshot, string unit) => snapshot["workload"]!.AsArray()
            .SelectMany(b => b!["unitQuantities"]!.AsArray()).Where(q => q!["unit"]!.GetValue<string>() == unit)
            .Sum(q => q!["quantity"]!.GetValue<decimal>());
        var kg = await Created(client, "kg");
        var metres = await Created(client, "m");
        foreach (var (product, quantity) in new[] { (kg, "0.001"), (kg, "1.234"), (metres, "999999999") })
        {
            var response = await client.PostRaw("/api/production-orders",
                $"{{\"productId\":\"{product["id"]}\",\"quantity\":{quantity},\"dueDate\":\"{Date(PlantToday.AddDays(3))}\"}}");
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        var dashboard = await (await client.GetAsync("/api/dashboard")).Body();
        var top = dashboard["topProducts"]!.AsArray();
        var counts = top.Select(p => p!["activeOrderCount"]!.GetValue<int>()).ToArray();
        Assert.Equal(counts.OrderDescending(), counts);
        Assert.Equal(999999999m, Total(dashboard, "m") - Total(before, "m"));
        Assert.Equal(1.235m, Total(dashboard, "kg") - Total(before, "kg"));
        Assert.Null(dashboard["completedThisWeek"]!["quantity"]);
    }

    [Theory]
    [InlineData("GET", "/api/product-master")]
    [InlineData("GET", "/api/product-master/00000000-0000-0000-0000-000000000001")]
    [InlineData("POST", "/api/product-master")]
    [InlineData("PUT", "/api/product-master/00000000-0000-0000-0000-000000000001")]
    [InlineData("POST", "/api/product-master/00000000-0000-0000-0000-000000000001/retire")]
    public async Task EveryProductEndpointRequiresAnEditorRole(string method, string path)
    {
        using var anonymous = fixture.CreateClient();
        using var unprivileged = await fixture.CreateClientAsAsync(null);
        foreach (var (client, expected) in new[] { (anonymous, HttpStatusCode.Unauthorized), (unprivileged, HttpStatusCode.Forbidden) })
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), path);
            if (method != "GET") request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            Assert.Equal(expected, (await client.SendAsync(request)).StatusCode);
        }
    }

    [Fact]
    public async Task ProductAndUnitTelemetryUseOnlyBoundedTags()
    {
        var spans = new ConcurrentBag<Activity>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name is ProductMasterService.TelemetryName or ProductionOrderTelemetry.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = spans.Add,
        };
        ActivitySource.AddActivityListener(activityListener);
        var measures = new ConcurrentBag<(string Name, string? Outcome, string[] Keys)>();
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Name is "pmai.products.requests" or "pmai.orders.unit_validation")
                    listener.EnableMeasurementEvents(instrument);
            },
        };
        meterListener.SetMeasurementEventCallback<long>((instrument, _, tags, _) =>
        {
            var keys = new List<string>();
            string? outcome = null;
            foreach (var tag in tags) { keys.Add(tag.Key); if (tag.Key == "outcome") outcome = tag.Value as string; }
            measures.Add((instrument.Name, outcome, keys.ToArray()));
        });
        meterListener.Start();
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await Created(client);
        await client.PostRaw("/api/production-orders",
            $"{{\"productId\":\"{product["id"]}\",\"quantity\":0.001,\"dueDate\":\"{Date(PlantToday.AddDays(10))}\"}}");
        Assert.Contains(spans, span => span.OperationName == "Product.Create");
        Assert.Contains(spans, span => span.OperationName == "ProductionOrder.Create" && (string?)span.GetTagItem("unit_validation") == "valid");
        Assert.Contains(measures, m => m.Name == "pmai.products.requests" && m.Outcome == "success");
        Assert.Contains(measures, m => m.Name == "pmai.orders.unit_validation" && m.Outcome == "valid");
        Assert.All(measures, m => Assert.All(m.Keys, key => Assert.Contains(key, new[] { "operation", "outcome" })));
        Assert.All(spans.Where(span => span.Source.Name == ProductMasterService.TelemetryName),
            span => Assert.DoesNotContain(span.Tags, tag => tag.Value == product["sku"]!.GetValue<string>()));
    }

    [Fact]
    public async Task InvalidIdentifiersQueryAndContentTypeReturnSafeErrors()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Path}/not-a-guid")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Path}/{Guid.NewGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"{Path}?state=invalid&page=0")).StatusCode);
        var content = new StringContent("sku=test", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, (await client.PostAsync(Path, content)).StatusCode);
    }

    [Fact]
    public async Task AnonymousAndUnprivilegedUsersCannotReadProductIds()
    {
        using var anonymous = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Path)).StatusCode);
        using var unprivileged = await fixture.CreateClientAsAsync(null);
        Assert.Equal(HttpStatusCode.Forbidden, (await unprivileged.GetAsync($"{Path}/{Guid.NewGuid()}")).StatusCode);
    }
}
