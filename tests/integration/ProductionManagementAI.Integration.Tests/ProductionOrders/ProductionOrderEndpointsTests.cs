using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Npgsql;
using ProductionManagementAI.Application.ProductionOrders;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.ProductionOrders;

// 001_DD test viewpoints at integration level (I): real HTTP pipeline, real PostgreSQL, app running as pmai_app.
public class ProductionOrderEndpointsTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string Orders = "/api/production-orders";

    public static TheoryData<string, string> AllEndpoints => new()
    {
        { "GET", "/api/products" },
        { "POST", Orders },
        { "GET", $"{Orders}/{Guid.NewGuid()}" },
        { "PUT", $"{Orders}/{Guid.NewGuid()}" },
    };

    private static HttpRequestMessage Request(string method, string url) => new(new HttpMethod(method), url)
    {
        Content = method is "POST" or "PUT" ? new StringContent("{}", Encoding.UTF8, "application/json") : null,
    };

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task Unauthenticated_Is401_OnEveryEndpoint(string method, string url)
    {
        using var client = fixture.CreateClient();

        var response = await client.SendAsync(Request(method, url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task SignedInWithoutRole_Is403_OnEveryEndpoint(string method, string url)
    {
        using var client = await fixture.CreateClientAsAsync(role: null);

        var response = await client.SendAsync(Request(method, url));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Operator_ListsThe30SeededProducts_OrderedBySku()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");

        var products = (await client.GetFromJsonAsyncArray("/api/products")).ToArray();

        Assert.Equal(30, products.Length);
        Assert.Equal("P-1001", products[0]["sku"]!.GetValue<string>());
        Assert.Equal("P-1030", products[^1]["sku"]!.GetValue<string>());
        Assert.Equal(products.Select(p => p["sku"]!.GetValue<string>()).Order(StringComparer.Ordinal),
            products.Select(p => p["sku"]!.GetValue<string>()));
    }

    [Fact]
    public async Task Create_Valid_Returns201_DraftWithOrderNumber_AndLocation()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");

        var response = await client.PostJson(Orders, ValidCreate(notes: "  first run  "));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Body();
        Assert.Equal($"/api/production-orders/{order["id"]}", response.Headers.Location!.AbsolutePath);
        Assert.Matches($"^PO-{PlantToday.Year}-\\d{{5}}$", order["orderNumber"]!.GetValue<string>());
        Assert.Equal("Draft", order["status"]!.GetValue<string>());
        Assert.Equal(["InProgress", "Cancelled"], order["allowedNextStatuses"]!.AsArray().Select(s => s!.GetValue<string>()));
        Assert.True(order["isProductQuantityEditable"]!.GetValue<bool>());
        Assert.Equal("first run", order["notes"]!.GetValue<string>());
        Assert.True(order["version"]!.GetValue<uint>() > 0);
    }

    [Fact]
    public async Task Create_MissingFields_Is400_ProblemDetailsWithMessageIds_AndNoExceptionDetail()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var response = await client.PostRaw(Orders, "{}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var problem = await response.Body();
        Assert.Equal("urn:pmai:problem:validation", problem["type"]!.GetValue<string>());
        Assert.Equal("VALIDATION", problem["code"]!.GetValue<string>());
        Assert.NotNull(problem["traceId"]);
        Assert.Null(problem["exception"]);
        Assert.Equal(["MSG-E001"], problem.ErrorsFor("productId"));
        Assert.Equal(["MSG-E003"], problem.ErrorsFor("quantity"));
        Assert.Equal(["MSG-E004"], problem.ErrorsFor("dueDate"));
    }

    [Theory]
    [InlineData("\"quantity\": \"abc\"", "quantity", "MSG-E003")]
    [InlineData("\"quantity\": 2.5", "quantity", "MSG-E003")]
    [InlineData("\"dueDate\": \"not-a-date\"", "dueDate", "MSG-E004")]
    [InlineData("\"productId\": \"nope\"", "productId", "MSG-E001")]
    public async Task Create_MalformedJsonValues_Is400_WithMessageIds_NotFrameworkText(string member, string field, string code)
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var response = await client.PostRaw(Orders, "{" + member + "}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Body();
        Assert.Equal([code], problem.ErrorsFor(field));
    }

    [Theory]
    [InlineData(0, "quantity", "MSG-E003")]
    [InlineData(1_000_000_000, "quantity", "MSG-E010")]
    public async Task Create_QuantityOutOfRange_Is400(int quantity, string field, string code)
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var problem = await (await client.PostJson(Orders, ValidCreate(quantity: quantity))).Body();

        Assert.Equal([code], problem.ErrorsFor(field));
    }

    [Fact]
    public async Task Create_UnknownProduct_AndPastDueDate_Are400()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var response = await client.PostJson(Orders, ValidCreate(product: Guid.NewGuid(), due: PlantToday.AddDays(-1)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Body();
        Assert.Equal(["MSG-E002"], problem.ErrorsFor("productId"));
        Assert.Equal(["MSG-E005"], problem.ErrorsFor("dueDate"));
    }

    [Fact]
    public async Task Create_DueTodayInPlantTimezone_IsAccepted()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var response = await client.PostJson(Orders, ValidCreate(due: PlantToday));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Notes_500CodePointsIncludingEmoji_Accepted_501Rejected_EmptyStoredAsNull()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var ok = await client.PostJson(Orders, ValidCreate(notes: new string('a', 499) + "😀"));
        var tooLong = await client.PostJson(Orders, ValidCreate(notes: new string('a', 501)));
        var empty = await client.CreateOrder(ValidCreate(notes: "   "));

        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
        Assert.Equal(["MSG-E006"], (await tooLong.Body()).ErrorsFor("notes"));
        Assert.Null(empty["notes"]);
    }

    [Fact]
    public async Task NonJsonBody_Is415_AndNothingIsCreated()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var before = await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM production_orders");

        var response = await client.PostAsync(Orders, new StringContent("productId=x", Encoding.UTF8, "application/x-www-form-urlencoded"));

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Equal(before, await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM production_orders"));
    }

    [Fact]
    public async Task Get_UnknownOrNonGuidId_Is404()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var unknown = await client.GetAsync($"{Orders}/{Guid.NewGuid()}");
        var nonGuid = await client.GetAsync($"{Orders}/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("MSG-E011", (await unknown.Body())["code"]!.GetValue<string>());
        Assert.Equal(HttpStatusCode.NotFound, nonGuid.StatusCode);
    }

    [Fact]
    public async Task Update_DraftToInProgress_ThenLockedQuantityChange_IsRejectedAsAWhole()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var order = await client.CreateOrder();

        var startBody = UpdateFrom(order);
        startBody["status"] = "InProgress";
        startBody["productId"] = DriveShaft.ToString(); // allowed: still Draft when this request is evaluated
        var started = await (await client.PutJson($"{Orders}/{order["id"]}", startBody)).Body();

        Assert.Equal("InProgress", started["status"]!.GetValue<string>());
        Assert.False(started["isProductQuantityEditable"]!.GetValue<bool>());
        Assert.Equal(["Completed", "Cancelled"], started["allowedNextStatuses"]!.AsArray().Select(s => s!.GetValue<string>()));

        var lockedBody = UpdateFrom(started);
        lockedBody["quantity"] = 999;
        lockedBody["notes"] = "should not be saved";
        var rejected = await client.PutJson($"{Orders}/{order["id"]}", lockedBody);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, rejected.StatusCode);
        Assert.Equal("MSG-E008", (await rejected.Body())["code"]!.GetValue<string>());
        var reloaded = await (await client.GetAsync($"{Orders}/{order["id"]}")).Body();
        Assert.Equal(250, reloaded["quantity"]!.GetValue<int>());
        Assert.Equal("note", reloaded["notes"]!.GetValue<string>()); // DEC-007: nothing from the request applied
    }

    [Fact]
    public async Task Update_DisallowedTransition_Is422()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var order = await client.CreateOrder();
        var body = UpdateFrom(order);
        body["status"] = "Completed";

        var response = await client.PutJson($"{Orders}/{order["id"]}", body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("MSG-E007", (await response.Body())["code"]!.GetValue<string>());
    }

    [Fact]
    public async Task Update_UnknownStatusName_Is400()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var order = await client.CreateOrder();
        var body = UpdateFrom(order);
        body["status"] = "Shipped";

        var problem = await (await client.PutJson($"{Orders}/{order["id"]}", body)).Body();

        Assert.Equal(["MSG-E007"], problem.ErrorsFor("status"));
    }

    [Fact]
    public async Task Update_StaleVersion_Is409_AndFirstWriterWins()
    {
        using var alice = await fixture.CreateClientAsAsync("Operator");
        using var bob = await fixture.CreateClientAsAsync("Operator");
        var order = await alice.CreateOrder();

        var aliceBody = UpdateFrom(order);
        aliceBody["notes"] = "alice";
        var bobBody = UpdateFrom(order); // loaded the same version
        bobBody["notes"] = "bob";

        Assert.Equal(HttpStatusCode.OK, (await alice.PutJson($"{Orders}/{order["id"]}", aliceBody)).StatusCode);
        var bobResponse = await bob.PutJson($"{Orders}/{order["id"]}", bobBody);

        Assert.Equal(HttpStatusCode.Conflict, bobResponse.StatusCode);
        Assert.Equal("MSG-E009", (await bobResponse.Body())["code"]!.GetValue<string>());
        var reloaded = await (await alice.GetAsync($"{Orders}/{order["id"]}")).Body();
        Assert.Equal("alice", reloaded["notes"]!.GetValue<string>());
    }

    [Fact]
    public async Task Update_OverdueOrder_SavesWithoutMovingDueDate_ButCannotMoveItIntoThePast()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var order = await client.CreateOrder();
        var overdue = Date(PlantToday.AddDays(-5));
        await fixture.ExecuteAsOwnerAsync($"UPDATE production_orders SET due_date = '{overdue}' WHERE id = '{order["id"]}'");
        var current = await (await client.GetAsync($"{Orders}/{order["id"]}")).Body();

        var keep = UpdateFrom(current);
        keep["status"] = "InProgress";
        var saved = await client.PutJson($"{Orders}/{order["id"]}", keep);
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode); // DEC-009

        var move = UpdateFrom(await saved.Body());
        move["dueDate"] = Date(PlantToday.AddDays(-1));
        var rejected = await client.PutJson($"{Orders}/{order["id"]}", move);
        Assert.Equal(["MSG-E005"], (await rejected.Body()).ErrorsFor("dueDate"));
    }

    [Fact]
    public async Task RuntimeLogin_CannotDeleteOrdersOrRunDdl()
    {
        await using var connection = new NpgsqlConnection(fixture.AppConnectionString);
        await connection.OpenAsync();

        async Task<string?> SqlState(string sql)
        {
            try
            {
                await using var command = new NpgsqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync();
                return null;
            }
            catch (PostgresException ex)
            {
                return ex.SqlState;
            }
        }

        Assert.Equal("42501", await SqlState("DELETE FROM production_orders"));
        Assert.Equal("42501", await SqlState("DELETE FROM products"));
        Assert.Equal("42501", await SqlState("CREATE TABLE should_not_exist (id int)"));
        Assert.Equal("42501", await SqlState("TRUNCATE production_order_number_counters"));
    }

    [Fact]
    public async Task CreateAndUpdate_EmitSpansAndCounters()
    {
        var spans = new List<Activity>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == ProductionOrderTelemetry.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = a => { lock (spans) spans.Add(a); },
        };
        ActivitySource.AddActivityListener(activityListener);

        var counts = new List<(string Instrument, string? Outcome)>();
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == ProductionOrderTelemetry.Name) l.EnableMeasurementEvents(instrument);
            },
        };
        meterListener.SetMeasurementEventCallback<long>((instrument, _, tags, _) =>
        {
            string? outcome = null;
            foreach (var tag in tags)
            {
                if (tag.Key == "outcome") outcome = tag.Value as string;
            }

            lock (counts) counts.Add((instrument.Name, outcome));
        });
        meterListener.Start();

        using var client = await fixture.CreateClientAsAsync("Admin");
        var order = await client.CreateOrder();
        var body = UpdateFrom(order);
        body["status"] = "Cancelled";
        await client.PutJson($"{Orders}/{order["id"]}", body);

        var id = order["id"]!.GetValue<string>();
        lock (spans)
        {
            Assert.Contains(spans, s => s.OperationName == "ProductionOrder.Create" && s.GetTagItem("production_order.id")?.ToString() == id
                && (string?)s.GetTagItem("outcome") == "success");
            Assert.Contains(spans, s => s.OperationName == "ProductionOrder.Update" && s.GetTagItem("production_order.id")?.ToString() == id
                && (string?)s.GetTagItem("status.to") == "Cancelled");
        }

        lock (counts)
        {
            Assert.Contains(("pmai.production_orders.created", "success"), counts);
            Assert.Contains(("pmai.production_orders.updated", "success"), counts);
            Assert.Contains(counts, c => c.Instrument == "pmai.production_orders.status_transitions");
        }
    }
}

internal static class HttpClientJsonExtensions
{
    public static async Task<IEnumerable<JsonObject>> GetFromJsonAsyncArray(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var array = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsArray();
        return array.Select(n => n!.AsObject());
    }
}
