using System.Net;
using System.Text.Json.Nodes;
using Npgsql;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.ProductionOrders;

/// <summary>
/// 002_DD test viewpoints at integration level (I): real HTTP pipeline, real PostgreSQL, app running as pmai_app,
/// against the 124 seeded demo orders (80 from 002_DB, 44 more from 003_DB). Assertions are on counts, statuses and offsets from today — never on
/// absolute dates, because the seed's due dates are relative to the migration's run date (DEC-011).
/// Its own fixture, so the seeded data isn't disturbed by orders other test classes create.
/// </summary>
public class ProductionOrderListEndpointTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string Orders = "/api/production-orders";
    private const int SeededOrders = 124; // 002_DB's 80 + 003_DB's 44 (WI-004 DEC-013, DEC-014)

    private static async Task<JsonObject> ListAsync(HttpClient client, string query = "")
    {
        var response = await client.GetAsync(query == "" ? Orders : $"{Orders}?{query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Body();
    }

    private static JsonArray Items(JsonObject page) => page["items"]!.AsArray();

    private static string[] OrderNumbers(JsonObject page) =>
        [.. Items(page).Select(item => item!["orderNumber"]!.GetValue<string>())];

    private static async Task<JsonObject> ProblemAsync(HttpClient client, string query)
    {
        var response = await client.GetAsync($"{Orders}?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        return await response.Body();
    }

    // TC-101
    [Fact]
    public async Task DefaultView_ReturnsFirstPageInDueDateOrder_WithEveryStatusAndTheTotal()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");

        var page = await ListAsync(client);

        Assert.Equal(SeededOrders, page["total"]!.GetValue<int>());
        Assert.Equal(20, Items(page).Count);
        Assert.Equal(1, page["page"]!.GetValue<int>());
        Assert.Equal("dueDate", page["sort"]!.GetValue<string>());
        Assert.Equal("asc", page["dir"]!.GetValue<string>());

        var dueDates = Items(page).Select(item => item!["dueDate"]!.GetValue<string>()).ToArray();
        Assert.Equal(dueDates.Order(StringComparer.Ordinal), dueDates);

        // No filter is pre-applied, so terminal statuses are listed too (DEC-006).
        var all = await ListAsync(client, "pageSize=100");
        var statuses = Items(all).Select(item => item!["status"]!.GetValue<string>()).Distinct().Order();
        Assert.Equal(["Cancelled", "Completed", "Draft", "InProgress"], statuses);
    }

    // TC-102 (server half): the row carries what the screen shows, and nothing it doesn't.
    [Fact]
    public async Task Row_CarriesTheDisplayedFieldsOnly()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var row = Items(await ListAsync(client, "pageSize=10"))[0]!.AsObject();

        Assert.Equal(
            ["dueDate", "id", "isOverdue", "orderNumber", "product", "quantity", "status", "updatedAt"],
            row.Select(pair => pair.Key).Order().ToArray());
        Assert.Matches(@"^PO-\d{4}-\d{5}$", row["orderNumber"]!.GetValue<string>());
        Assert.Equal(
            ["id", "name", "sku"],
            row["product"]!.AsObject().Select(pair => pair.Key).Order().ToArray());
    }

    // TC-103 (server half): overdue is decided server-side against the plant date.
    [Fact]
    public async Task OverdueFlag_IsSetOnlyForPastDueActiveOrders()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var today = Date(PlantToday);

        var page = await ListAsync(client, "pageSize=100");

        foreach (var item in Items(page))
        {
            var dueDate = item!["dueDate"]!.GetValue<string>();
            var status = item["status"]!.GetValue<string>();
            var expected = string.CompareOrdinal(dueDate, today) < 0 && status is "Draft" or "InProgress";
            Assert.Equal(expected, item["isOverdue"]!.GetValue<bool>());
        }

        // The seed is built so both halves of the rule are actually exercised (002_DB).
        Assert.Contains(Items(page), item => item!["isOverdue"]!.GetValue<bool>());
        Assert.Contains(
            Items(page),
            item => string.CompareOrdinal(item!["dueDate"]!.GetValue<string>(), today) < 0
                && !item["isOverdue"]!.GetValue<bool>());
    }

    // TC-104
    [Fact]
    public async Task StatusFilter_IsMultiSelect_AndAbsenceMeansNoRestriction()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var drafts = await ListAsync(client, "status=Draft&pageSize=100");
        var active = await ListAsync(client, "status=Draft&status=InProgress&pageSize=100");
        var all = await ListAsync(client, "pageSize=100");

        Assert.All(Items(drafts), item => Assert.Equal("Draft", item!["status"]!.GetValue<string>()));
        Assert.All(
            Items(active),
            item => Assert.Contains(item!["status"]!.GetValue<string>(), new[] { "Draft", "InProgress" }));
        Assert.Equal(35, drafts["total"]!.GetValue<int>());
        Assert.Equal(60, active["total"]!.GetValue<int>());
        Assert.Equal(SeededOrders, all["total"]!.GetValue<int>());

        // A repeated value collapses rather than duplicating rows.
        var repeated = await ListAsync(client, "status=Draft&status=Draft&pageSize=100");
        Assert.Equal(drafts["total"]!.GetValue<int>(), repeated["total"]!.GetValue<int>());
    }

    // TC-105
    [Fact]
    public async Task ProductDueRangeAndFragmentFilters_EachReturnExactlyTheMatchingRows()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var byProduct = await ListAsync(client, $"productId={DriveShaft}&pageSize=100");
        Assert.NotEmpty(Items(byProduct));
        Assert.All(Items(byProduct), item => Assert.Equal("P-1004", item!["product"]!["sku"]!.GetValue<string>()));

        // Inclusive at both ends: a single-day range returns that day's orders.
        var day = Items(await ListAsync(client, "pageSize=10"))[0]!["dueDate"]!.GetValue<string>();
        var singleDay = await ListAsync(client, $"dueFrom={day}&dueTo={day}&pageSize=100");
        Assert.NotEmpty(Items(singleDay));
        Assert.All(Items(singleDay), item => Assert.Equal(day, item!["dueDate"]!.GetValue<string>()));

        // Open-ended range: only one end set.
        var fromOnly = await ListAsync(client, $"dueFrom={day}&pageSize=100");
        Assert.All(
            Items(fromOnly),
            item => Assert.True(string.CompareOrdinal(item!["dueDate"]!.GetValue<string>(), day) >= 0));

        // Fragment: case-insensitive, matches mid-string, not just a prefix.
        var fragment = await ListAsync(client, "orderNumber=00042&pageSize=100");
        Assert.Single(Items(fragment));
        Assert.EndsWith("00042", OrderNumbers(fragment)[0], StringComparison.Ordinal);
        Assert.Equal(
            OrderNumbers(fragment),
            OrderNumbers(await ListAsync(client, "orderNumber=po-2026-00042&pageSize=100")));
    }

    // TC-106
    [Fact]
    public async Task Filters_CombineWithAnd_AndTheTotalMatchesTheRowCount()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var today = Date(PlantToday);

        var page = await ListAsync(client, $"status=Draft&dueFrom={today}&pageSize=100");

        Assert.All(Items(page), item =>
        {
            Assert.Equal("Draft", item!["status"]!.GetValue<string>());
            Assert.True(string.CompareOrdinal(item["dueDate"]!.GetValue<string>(), today) >= 0);
        });
        Assert.Equal(Items(page).Count, page["total"]!.GetValue<int>());

        var draftsOnly = await ListAsync(client, "status=Draft&pageSize=100");
        Assert.True(page["total"]!.GetValue<int>() < draftsOnly["total"]!.GetValue<int>());
    }

    // TC-107
    [Theory]
    [InlineData("status=Shipped", "status", "MSG-E018")]
    [InlineData("status=3", "status", "MSG-E018")]
    [InlineData("productId=not-a-guid", "productId", "MSG-E002")]
    [InlineData("dueFrom=2026-13-01", "dueFrom", "MSG-E016")]
    [InlineData("dueFrom=2026-10-01&dueTo=2026-09-01", "dueFrom", "MSG-E017")]
    [InlineData("orderNumber=XXXXXXXXXXXXXXXXXXXXX", "orderNumber", "MSG-E015")]
    [InlineData("sort=notes", "sort", "MSG-E019")]
    [InlineData("dir=sideways", "dir", "MSG-E019")]
    [InlineData("page=0", "page", "MSG-E019")]
    [InlineData("pageSize=25", "pageSize", "MSG-E019")]
    [InlineData("pageSize=1000000", "pageSize", "MSG-E019")]
    public async Task InvalidQueryParameter_Is400_WithItsMessageIdOnThatField(string query, string field, string code)
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var problem = await ProblemAsync(client, query);

        Assert.Equal("VALIDATION", problem["code"]!.GetValue<string>());
        Assert.Equal([code], problem.ErrorsFor(field));
        Assert.Null(problem["detail"]);
        Assert.DoesNotContain("Postgres", await Task.FromResult(problem.ToJsonString()), StringComparison.OrdinalIgnoreCase);
    }

    // TC-107: an unknown but well-formed product is rejected too, and every offending parameter is reported together.
    [Fact]
    public async Task UnknownProduct_Is400_AndAllOffendingParametersAreReportedTogether()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        Assert.Equal(["MSG-E002"], (await ProblemAsync(client, $"productId={Guid.NewGuid()}")).ErrorsFor("productId"));

        var problem = await ProblemAsync(client, "status=Nope&sort=notes&pageSize=7");
        Assert.Equal(["pageSize", "sort", "status"], problem["errors"]!.AsObject().Select(p => p.Key).Order().ToArray());
    }

    // TC-108
    [Fact]
    public async Task WildcardsInTheFragment_AreMatchedLiterally_SoASearchCannotWidenItself()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        Assert.Empty(Items(await ListAsync(client, "orderNumber=%")));
        Assert.Empty(Items(await ListAsync(client, "orderNumber=_")));
        Assert.Empty(Items(await ListAsync(client, "orderNumber=%2026%")));
        // The same search without wildcards does match, so the emptiness above is the escaping, not a broken filter.
        Assert.NotEmpty(Items(await ListAsync(client, "orderNumber=2026")));
    }

    // TC-109
    [Fact]
    public async Task EverySortKey_OrdersTheWholeResultSet_AndTheDirectionReverses()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        // Every page, both directions: the seed (124 since WI-004) is larger than one page.
        async Task<string[]> AllPages(string query)
        {
            var all = new List<string>();
            for (var page = 1; ; page++)
            {
                var numbers = OrderNumbers(await ListAsync(client, $"{query}&pageSize=100&page={page}"));
                if (numbers.Length == 0) return [.. all];
                all.AddRange(numbers);
            }
        }

        var byNumber = await AllPages("sort=orderNumber");
        Assert.Equal(SeededOrders, byNumber.Length);
        Assert.Equal(byNumber.Order(StringComparer.Ordinal), byNumber);
        Assert.Equal(byNumber.Reverse(), await AllPages("sort=orderNumber&dir=desc"));

        var quantities = Items(await ListAsync(client, "sort=quantity&pageSize=100"))
            .Select(item => item!["quantity"]!.GetValue<int>()).ToArray();
        Assert.Equal(quantities.Order(), quantities);

        var skus = Items(await ListAsync(client, "sort=product&pageSize=100"))
            .Select(item => item!["product"]!["sku"]!.GetValue<string>()).ToArray();
        Assert.Equal(skus.Order(StringComparer.Ordinal), skus);

        var updated = Items(await ListAsync(client, "sort=updatedAt&dir=desc&pageSize=100"))
            .Select(item => item!["updatedAt"]!.GetValue<string>()).ToArray();
        Assert.Equal(updated.OrderDescending(StringComparer.Ordinal), updated);

        // Status sorts in workflow order, not alphabetically (002_DB sort-key mapping).
        var rank = new Dictionary<string, int> { ["Draft"] = 1, ["InProgress"] = 2, ["Completed"] = 3, ["Cancelled"] = 4 };
        var ranks = Items(await ListAsync(client, "sort=status&pageSize=100"))
            .Select(item => rank[item!["status"]!.GetValue<string>()]).ToArray();
        Assert.Equal(ranks.Order(), ranks);
    }

    // TC-110
    [Fact]
    public async Task Paging_IsStable_SoEveryRowAppearsExactlyOnceAcrossPages()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var collected = new List<string>();
        for (var page = 1; page <= (SeededOrders + 9) / 10; page++)
        {
            collected.AddRange(OrderNumbers(await ListAsync(client, $"pageSize=10&page={page}")));
        }

        Assert.Equal(SeededOrders, collected.Count);
        Assert.Equal(SeededOrders, collected.Distinct().Count());

        // Orders sharing a due date are what makes the tie-breaker necessary; the seed has several.
        var dueDates = Items(await ListAsync(client, "pageSize=100"))
            .Select(item => item!["dueDate"]!.GetValue<string>()).ToArray();
        Assert.True(dueDates.Length > dueDates.Distinct().Count(), "seed should contain shared due dates");
    }

    // TC-111
    [Fact]
    public async Task PageSizeAndPageBounds_BehaveAsDesigned()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        foreach (var size in new[] { 10, 20, 50, 100 })
        {
            var page = await ListAsync(client, $"pageSize={size}");
            Assert.Equal(Math.Min(size, SeededOrders), Items(page).Count);
            Assert.Equal(size, page["pageSize"]!.GetValue<int>());
        }

        // A page past the last one is valid and empty, and still reports the real total (REQ-024).
        var beyond = await ListAsync(client, "page=9&pageSize=20");
        Assert.Empty(Items(beyond));
        Assert.Equal(SeededOrders, beyond["total"]!.GetValue<int>());
        Assert.Equal(9, beyond["page"]!.GetValue<int>());
    }

    // TC-114
    [Fact]
    public async Task Unauthenticated_Is401_AndAUserWithoutARole_Is403_WithNoOrderData()
    {
        using var anonymous = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Orders)).StatusCode);

        using var roleless = await fixture.CreateClientAsAsync(role: null);
        var forbidden = await roleless.GetAsync(Orders);

        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.DoesNotContain("PO-", await forbidden.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    // TC-119: the queries behind the screen are index-backed (002_DB), not sequential scans.
    [Theory]
    [InlineData(
        "SELECT o.id FROM production_orders o ORDER BY o.due_date, o.order_number LIMIT 20",
        "ix_production_orders_due_date_order_number")]
    [InlineData(
        "SELECT o.id FROM production_orders o WHERE o.order_number LIKE '%00042%' ESCAPE '\\'",
        "ix_production_orders_order_number_trgm")]
    public async Task ListQueries_UseTheirIndex(string sql, string expectedIndex)
    {
        await using var connection = new NpgsqlConnection(fixture.OwnerConnectionString);
        await connection.OpenAsync();

        // What this asserts: the index can serve the query — not that the planner picks it at demo volume. On 80 rows
        // a sequential scan is genuinely cheaper, so the planner is right to choose one; disabling seqscan for this
        // session is what makes the question "is this query index-backed as the table grows?" answerable now.
        foreach (var setup in new[] { "ANALYZE production_orders", "SET enable_seqscan = off" })
        {
            await using var command = new NpgsqlCommand(setup, connection);
            await command.ExecuteNonQueryAsync();
        }

        await using var explain = new NpgsqlCommand($"EXPLAIN {sql}", connection);
        var plan = new List<string>();
        await using (var reader = await explain.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                plan.Add(reader.GetString(0));
            }
        }

        Assert.Contains(expectedIndex, string.Join('\n', plan));
    }
}
