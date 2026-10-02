using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.PlantCalendar;

/// <summary>Hosts calendar requests against a disposable migrated database and a controlled plant clock.</summary>
public sealed class CalendarFixture : IntegrationTestFixture
{
    /// <summary>Gets the deterministic plant date source.</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2031, 6, 11, 3, 0, 0, TimeSpan.Zero));
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services => { services.RemoveAll<TimeProvider>(); services.AddSingleton<TimeProvider>(Time); });
    }
}

/// <summary>Exercises actual HTTP parsing, role checks, retained transitions and restricted PostgreSQL grants.</summary>
public sealed class CalendarApiTests(CalendarFixture fixture) : IClassFixture<CalendarFixture>
{
    private const string Root = "/api/plant-calendar";
    private static readonly (HttpMethod Method, string Path, string? Body)[] Operations =
    [
        (HttpMethod.Get, "/month?month=2031-06", null), (HttpMethod.Get, "/day?date=2031-06-11", null),
        (HttpMethod.Get, "/weekly?from=2031-06-01&to=2031-06-30", null),
        (HttpMethod.Put, "/weekly/2031-06-11", "{}"), (HttpMethod.Post, "/weekly/2031-06-12/withdraw", "{}"),
        (HttpMethod.Put, "/exceptions", "{}"), (HttpMethod.Post, "/exceptions/remove", "{}"),
        (HttpMethod.Get, "/line-choices", null), (HttpMethod.Get, "/product-choices?lineId=11111111-1111-1111-1111-111111111111", null),
        (HttpMethod.Get, "/capacity?lineId=11111111-1111-1111-1111-111111111111&productId=22222222-2222-2222-2222-222222222222&date=2031-06-11", null),
        (HttpMethod.Get, "/exception-history?date=2031-06-11", null)
    ];

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("", HttpStatusCode.Forbidden)]
    public async Task EveryOperationRejectsMissingRoleWithoutParsing(string? role, HttpStatusCode expected)
    {
        using var client = role is null ? fixture.CreateClient() : await fixture.CreateClientAsAsync(null);
        foreach (var operation in Operations)
        {
            using var request = new HttpRequestMessage(operation.Method, Root + operation.Path);
            if (operation.Body is not null) request.Content = new StringContent(operation.Body, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            Assert.Equal(expected, response.StatusCode);
            Assert.Equal("", await response.Content.ReadAsStringAsync());
            Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        }
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Operator")]
    public async Task AuthorizedRolesReachAllElevenContracts(string role)
    {
        using var client = await fixture.CreateClientAsAsync(role);
        foreach (var operation in Operations)
        {
            using var request = new HttpRequestMessage(operation.Method, Root + operation.Path);
            if (operation.Body is not null) request.Content = new StringContent(operation.Body, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            var expected = operation.Body is not null ? HttpStatusCode.BadRequest : operation.Path.StartsWith("/capacity", StringComparison.Ordinal) || operation.Path.StartsWith("/product-choices", StringComparison.Ordinal) ? HttpStatusCode.NotFound : HttpStatusCode.OK;
            Assert.Equal(expected,response.StatusCode);
            Assert.Equal("no-store",response.Headers.CacheControl?.ToString());
        }
    }

    [Theory]
    [InlineData("0001-01",31)]
    [InlineData("2000-02",29)]
    [InlineData("1900-02",28)]
    [InlineData("9999-12",31)]
    public async Task SupportedMonthBoundsHaveExactlyTheirRealDates(string month,int count)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var response = await client.GetAsync(Root + "/month?month=" + month);response.EnsureSuccessStatusCode();
        var days = (await response.Body())["days"]!.AsArray();Assert.Equal(count,days.Count);
        for (var i=0;i<count;i++)Assert.Equal(month+"-"+(i+1).ToString("D2",System.Globalization.CultureInfo.InvariantCulture),days[i]!["date"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("/month?month=2031-06&month=2031-07")]
    [InlineData("/month?month=2031-06&extra=x")]
    [InlineData("/month?month=2031-6")]
    [InlineData("/day?date=2031-02-29")]
    [InlineData("/line-choices?page=01")]
    [InlineData("/weekly?from=2031-01-01&to=2032-01-02")]
    [InlineData("/weekly?from=2031-06-01&to=2031-06-30&page=2")]
    public async Task InvalidQueriesAreFeatureProblems(string path)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var response = await client.GetAsync(Root + path);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("VALIDATION", (await response.Body())["code"]!.GetValue<string>());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("{\"version\":\"1\",\"version\":\"1\",\"targetRevisionId\":null,\"workingDays\":[]}")]
    [InlineData("{\"version\":1,\"targetRevisionId\":null,\"workingDays\":[]}")]
    [InlineData("{\"version\":\"1\",\"targetRevisionId\":null,\"workingDays\":[\"Mon\",\"Mon\"]}")]
    [InlineData("{\"version\":\"1\",\"targetRevisionId\":null,\"workingDays\":[],\"extra\":true}")]
    public async Task InvalidBodiesCannotReachStorage(string body)
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var response = await client.PutAsync(Root + "/weekly/2031-06-11", new StringContent(body, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Body();
        Assert.Equal("VALIDATION", problem["code"]!.GetValue<string>());
        Assert.Equal("NotApplied", problem["writeOutcome"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("text/plain", "{}", HttpStatusCode.UnsupportedMediaType, "UNSUPPORTED_MEDIA_TYPE")]
    [InlineData("application/json; charset=ascii", "{}", HttpStatusCode.UnsupportedMediaType, "UNSUPPORTED_MEDIA_TYPE")]
    [InlineData("application/json", "{", HttpStatusCode.BadRequest, "VALIDATION")]
    public async Task InvalidMediaAndJsonAreKnownRejections(string media, string body, HttpStatusCode status, string code)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        using var content = new StringContent(body);
        content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(media);
        var response = await client.PutAsync(Root + "/weekly/2031-06-11", content);
        Assert.Equal(status, response.StatusCode);
        var problem = await response.Body(); Assert.Equal(code, problem["code"]!.GetValue<string>());
        Assert.Equal("NotApplied", problem["writeOutcome"]!.GetValue<string>());
    }

    [Fact]
    public async Task OversizedBodyIsRejectedBeforeStorage()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var response = await client.PutAsync(Root + "/weekly/2031-06-11", new StringContent(new string(' ', 8193), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("NotApplied", (await response.Body())["writeOutcome"]!.GetValue<string>());
    }

    [Fact]
    public async Task ExplicitActivationRetainedHistoryPrecedenceNoopAndCapacityUseRestrictedLogin()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var unconfigured = await (await client.GetAsync(Root + "/month?month=2031-06")).Body();
        Assert.Null(unconfigured["context"]!["version"]);
        Assert.All(unconfigured["days"]!.AsArray(), day => Assert.Equal("Unavailable", day!["state"]!.GetValue<string>()));
        Assert.Equal(0L, await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_state"));
        await fixture.ExecuteAsOwnerAsync("BEGIN; INSERT INTO plant_calendar_state (id,activated_on,time_zone_id,revision) VALUES (1,'2031-06-10','Asia/Tokyo',1); INSERT INTO plant_calendar_weekly_revisions (calendar_id,effective_from,working_weekdays,is_withdrawn,is_current,commit_revision) VALUES (1,'2031-06-10',31,false,true,1); COMMIT;");
        var productResponse = await client.PostJson("/api/product-master", new JsonObject { ["sku"] = "CAL-" + Guid.NewGuid().ToString("N"), ["name"] = "calendar fixture", ["unit"] = "kg" });
        productResponse.EnsureSuccessStatusCode();
        var product = await productResponse.Body();
        var productId = product["id"]!.GetValue<string>();
        var lineId = await client.CreateEligibleLine(productId);
        var save = new { version = "1", targetRevisionId = (string?)null, lineId = (string?)null, date = "2031-06-11", isWorking = true, workingHours = "2.125", reason = "  fixture  " };
        var first = await client.PutAsJsonAsync(Root + "/exceptions", save); first.EnsureSuccessStatusCode();
        var result = await first.Body();
        Assert.Equal("2", result["context"]!["version"]!.GetValue<string>());
        var targetId = result["target"]!["id"]!.GetValue<string>();
        Assert.Equal("fixture", result["target"]!["reason"]!.GetValue<string>());
        var noop = await client.PutAsJsonAsync(Root + "/exceptions", new { version = "2", targetRevisionId = targetId, lineId = (string?)null, date = "2031-06-11", isWorking = true, workingHours = "2.125", reason = "fixture" });
        noop.EnsureSuccessStatusCode(); Assert.False((await noop.Body())["changed"]!.GetValue<bool>());
        var stale = await client.PutAsJsonAsync(Root + "/exceptions", save);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("CALENDAR_STALE", (await stale.Body())["code"]!.GetValue<string>());
        var overrideResponse = await client.PutAsJsonAsync(Root + "/exceptions", new { version = "2", targetRevisionId = (string?)null, lineId, date = "2031-06-11", isWorking = true, workingHours = (string?)null, reason = (string?)null });
        overrideResponse.EnsureSuccessStatusCode();
        var lineTarget = (await overrideResponse.Body())["target"]!["id"]!.GetValue<string>();
        var day = await (await client.GetAsync(Root + $"/day?date=2031-06-11&lineId={lineId}")).Body();
        Assert.Equal("8", day["day"]!["hours"]!.GetValue<string>());
        var capacity = await client.GetAsync(Root + $"/capacity?lineId={lineId}&productId={productId}&date=2031-06-11");
        capacity.EnsureSuccessStatusCode(); Assert.Equal("Available", (await capacity.Body())["availability"]!.GetValue<string>());
        var remove = await client.PostAsJsonAsync(Root + "/exceptions/remove", new { version = "3", targetRevisionId = lineTarget, lineId, date = "2031-06-11" });
        remove.EnsureSuccessStatusCode();
        var marker = (await remove.Body())["target"]!["id"]!.GetValue<string>();
        var fallback = await (await client.GetAsync(Root + $"/day?date=2031-06-11&lineId={lineId}")).Body();
        Assert.Equal("2.125", fallback["day"]!["hours"]!.GetValue<string>());
        var missingMarker = await client.PutAsJsonAsync(Root + "/exceptions", new { version = "4", targetRevisionId = (string?)null, lineId, date = "2031-06-11", isWorking = false, workingHours = (string?)null, reason = (string?)null });
        Assert.Equal(HttpStatusCode.Conflict, missingMarker.StatusCode);
        var history = await (await client.GetAsync(Root + $"/exception-history?date=2031-06-11&lineId={lineId}")).Body();
        Assert.Equal(2, history["totalCount"]!.GetValue<int>());
        Assert.Equal(marker, history["current"]!["id"]!.GetValue<string>());
        var weeklyCreate = await client.PutAsJsonAsync(Root + "/weekly/2031-06-12", new { version = "4", targetRevisionId = (string?)null, workingDays = Array.Empty<string>() });
        weeklyCreate.EnsureSuccessStatusCode();
        var weeklyId = (await weeklyCreate.Body())["target"]!["id"]!.GetValue<string>();
        var closed = await (await client.GetAsync(Root + $"/capacity?lineId={lineId}&productId={productId}&date=2031-06-12")).Body();
        Assert.Equal("0", closed["quantity"]!.GetValue<string>());
        var withdraw = await client.PostAsJsonAsync(Root + "/weekly/2031-06-12/withdraw", new { version = "5", targetRevisionId = weeklyId });
        withdraw.EnsureSuccessStatusCode();
        var withdrawnId = (await withdraw.Body())["target"]!["id"]!.GetValue<string>();
        var weeklyRange = await (await client.GetAsync(Root + "/weekly?from=2031-06-12&to=2031-06-12")).Body();
        Assert.True(weeklyRange["items"]![0]!["isWithdrawn"]!.GetValue<bool>());
        Assert.Equal("2031-06-10", weeklyRange["applicableBeforeFrom"]!["effectiveFrom"]!.GetValue<string>());
        var readd = await client.PutAsJsonAsync(Root + "/weekly/2031-06-12", new { version = "6", targetRevisionId = withdrawnId, workingDays = new[] { "Thu" } });
        readd.EnsureSuccessStatusCode();
        var past = await client.PutAsJsonAsync(Root + "/weekly/2031-06-09", new { version = "7", targetRevisionId = (string?)null, workingDays = Array.Empty<string>() });
        Assert.Equal("CALENDAR_PAST_DATE", (await past.Body())["code"]!.GetValue<string>());
        var stalePage = await client.GetAsync(Root + "/weekly?from=2031-06-01&to=2031-06-30&page=2&snapshotVersion=6");
        Assert.Equal(HttpStatusCode.Conflict, stalePage.StatusCode);
        Assert.Equal(7L, await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state"));
        foreach (var sql in new[] { "UPDATE plant_calendar_state SET activated_on='2031-06-09'", "DELETE FROM plant_calendar_exception_revisions", "UPDATE plant_calendar_weekly_revisions SET working_weekdays=127", "INSERT INTO plant_calendar_state (id,activated_on,time_zone_id) VALUES (1,'2031-06-10','Asia/Tokyo')" })
        {
            await using var connection = new NpgsqlConnection(fixture.AppConnectionString); await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            var error = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
            Assert.Equal("42501", error.SqlState);
        }
        // TC-407: an explicit closure must not become an order scheduling gate.
        var closeToday = await client.PutAsJsonAsync(Root + "/exceptions", new { version = "7", targetRevisionId = targetId, lineId = (string?)null, date = "2031-06-11", isWorking = false, workingHours = (string?)null, reason = "fixture closure" });
        closeToday.EnsureSuccessStatusCode();
        Assert.Equal("Closed", (await (await client.GetAsync(Root + "/day?date=2031-06-11")).Body())["day"]!["state"]!.GetValue<string>());
        var orderLine = await client.CreateEligibleLine(SteelBracket.ToString());
        var draft = ValidCreate(due: new DateOnly(2031, 6, 11)); draft["lineId"] = orderLine;
        var order = await client.CreateOrder(draft);
        var start = UpdateFrom(order); start["status"] = "InProgress";
        var started = await client.PutJson($"/api/production-orders/{order["id"]}", start);
        started.EnsureSuccessStatusCode();
        Assert.Equal("InProgress", (await started.Body())["status"]!.GetValue<string>());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostJson("/api/production-orders", ValidCreate(due: new DateOnly(2031, 6, 10)))).StatusCode);
    }
}
