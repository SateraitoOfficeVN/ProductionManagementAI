using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ProductionManagementAI.Infrastructure.ProductionOrders;
using Xunit.Abstractions;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.ProductionOrders;

/// <summary>
/// 002_DD-API-CSV (API-PO-05) at integration level: real HTTP pipeline, real PostgreSQL, app running as pmai_app,
/// against the 124 seeded demo orders. WI-016 TC-451 to TC-455 and TC-457. Its own fixture, so orders created here
/// don't disturb other classes' counts.
/// </summary>
public class ProductionOrderExportEndpointTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string Export = "/api/production-orders/export";
    private const string Orders = "/api/production-orders";
    private const int SeededOrders = 124;

    // TC-451: the export is the list, all pages, same order — for several filter/sort combinations.
    [Theory]
    [InlineData("")]
    [InlineData("status=Draft&status=InProgress&sort=quantity&dir=desc")]
    [InlineData("sort=product")]
    [InlineData("sort=status&dir=desc")]
    [InlineData("sort=updatedAt")]
    [InlineData("sort=orderNumber&dir=desc&status=Completed")]
    public async Task Rows_AreTheListAcrossAllPages_InTheSameOrder(string query)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");

        var expected = await AllListedOrderNumbersAsync(client, query);
        var response = await client.GetAsync(Url(Export, query));
        var records = await CsvAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, records.Skip(1).Select(r => r[0]).ToArray());
        Assert.Equal(expected.Length.ToString(), Header(response, "X-Total-Count"));
        Assert.True(expected.Length > 20, "the case must span more than one list page");
    }

    // TC-452
    [Fact]
    public async Task Response_HasTheDesignedHeaders_AndABom()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var response = await client.GetAsync(Export);
        var bytes = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType.CharSet);
        var disposition = response.Content.Headers.ContentDisposition!;
        Assert.Equal("attachment", disposition.DispositionType);
        Assert.Matches(@"^""?production-orders_\d{8}-\d{4}\.csv""?$", disposition.FileName);
        Assert.Matches(@"^製造指示一覧_\d{8}-\d{4}\.csv$", disposition.FileNameStar);
        Assert.Contains("no-store", response.Headers.CacheControl!.ToString());
        // Other tests in this class may add orders, so the count is checked against the body, not a constant.
        var total = int.Parse(Header(response, "X-Total-Count"));
        Assert.True(total >= SeededOrders);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
        var text = Encoding.UTF8.GetString(bytes[3..]);
        Assert.StartsWith("指示番号,製品コード,製品名,生産ライン,数量,単位,納期,ステータス,納期遅れ,備考,作成日時,更新日時,完了日時\r\n", text);
        Assert.EndsWith("\r\n", text);
        Assert.Equal(total + 1, text.Split("\r\n").Length - 1);
    }

    // TC-453: one order with every awkward value, read back through an RFC 4180 reader.
    [Fact]
    public async Task Cells_AreFormattedEscapedAndNeutralised()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var lineId = await client.CreateEligibleLine(SteelBracket.ToString());
        var create = ValidCreate(notes: "=cmd \"A\",B\n2行目");
        create["lineId"] = lineId;
        var created = await (await client.PostJson(Orders, create)).Body();
        var id = created["id"]!.GetValue<string>();
        var orderNumber = created["orderNumber"]!.GetValue<string>();
        await fixture.ExecuteAsOwnerAsync($"UPDATE production_orders SET quantity = 12.5 WHERE id = '{id}'");
        await fixture.ExecuteAsOwnerAsync($"UPDATE production_lines SET is_active = false WHERE id = '{lineId}'");

        var records = await CsvAsync(await client.GetAsync($"{Export}?orderNumber={orderNumber}"));

        Assert.Equal(2, records.Count);
        var row = records[1];
        Assert.Equal(13, row.Count);
        Assert.Equal(orderNumber, row[0]);
        Assert.EndsWith(" (使用停止)", row[3]);
        Assert.Equal("12.5", row[4]);
        Assert.Equal("下書き", row[7]);
        Assert.Equal("'=cmd \"A\",B\n2行目", row[9]);
        Assert.Matches(@"^\d{4}/\d{2}/\d{2} \d{2}:\d{2}$", row[10]);
        Assert.Equal("", row[12]);

        // A seeded completed order carries its completion time; open orders never do.
        var completed = await CsvAsync(await client.GetAsync($"{Export}?status=Completed"));
        Assert.All(completed.Skip(1), r => Assert.Matches(@"^\d{4}/\d{2}/\d{2} \d{2}:\d{2}$", r[12]));
    }

    // TC-454: the list's validation, unchanged, with the same keys and message IDs.
    [Theory]
    [InlineData("status=Shipped", "status", "MSG-E018")]
    [InlineData("sort=price", "sort", "MSG-E019")]
    [InlineData("dir=up", "dir", "MSG-E019")]
    [InlineData("dueFrom=2026-10-31&dueTo=2026-10-01", "dueFrom", "MSG-E017")]
    [InlineData("dueTo=2026-13-01", "dueTo", "MSG-E016")]
    [InlineData("orderNumber=123456789012345678901", "orderNumber", "MSG-E015")]
    [InlineData("productId=0197e4a0-0000-7000-8000-00000000ffff", "productId", "MSG-E002")]
    public async Task InvalidParameters_Return400ProblemDetails_AndNoCsv(string query, string key, string messageId)
    {
        using var client = await fixture.CreateClientAsAsync("Operator");

        var response = await client.GetAsync($"{Export}?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var problem = await response.Body();
        Assert.Equal([messageId], problem["errors"]![key]!.AsArray().Select(n => n!.GetValue<string>()));
    }

    // TC-455
    [Fact]
    public async Task Export_RequiresSignInAndAnOrderRole()
    {
        using var anonymous = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(Export)).StatusCode);

        using var noRole = await fixture.CreateClientAsAsync(null);
        Assert.Equal(HttpStatusCode.Forbidden, (await noRole.GetAsync(Export)).StatusCode);
    }

    // TC-457: paging is not a parameter of the export.
    [Fact]
    public async Task PageAndPageSize_AreIgnored()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");

        var response = await client.GetAsync($"{Export}?page=3&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var total = int.Parse(Header(response, "X-Total-Count"));
        Assert.True(total >= SeededOrders);
        Assert.Equal(total + 1, (await CsvAsync(response)).Count);
    }

    private static async Task<string[]> AllListedOrderNumbersAsync(HttpClient client, string query)
    {
        var numbers = new List<string>();
        for (var page = 1; ; page++)
        {
            var paging = $"page={page}&pageSize=10";
            var response = await client.GetAsync(Url(Orders, query == "" ? paging : $"{query}&{paging}"));
            var body = await response.Body();
            var items = body["items"]!.AsArray();
            numbers.AddRange(items.Select(i => i!["orderNumber"]!.GetValue<string>()));
            if (numbers.Count >= body["total"]!.GetValue<int>() || items.Count == 0)
            {
                return [.. numbers];
            }
        }
    }

    private static string Url(string path, string query) => query == "" ? path : $"{path}?{query}";

    internal static string Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.Single() : throw new Xunit.Sdk.XunitException($"{name} missing");

    internal static async Task<List<List<string>>> CsvAsync(HttpResponseMessage response)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
        return ParseCsv(Encoding.UTF8.GetString(bytes[3..]));
    }

    /// <summary>A minimal RFC 4180 reader, independent of the writer under test.</summary>
    private static List<List<string>> ParseCsv(string text)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                else if (c == '"') { quoted = false; }
                else { field.Append(c); }
            }
            else if (c == '"') { quoted = true; }
            else if (c == ',') { record.Add(field.ToString()); field.Clear(); }
            else if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
            {
                record.Add(field.ToString()); field.Clear(); records.Add(record); record = []; i++;
            }
            else { field.Append(c); }
        }

        return records;
    }
}

/// <summary>
/// WI-016 TC-456 and the plan's performance step: the 10,000-row boundary on its own database, because it bulk-inserts
/// thousands of orders. 124 seeded + 9,876 inserted = exactly 10,000 (allowed, timed); one more = 10,001 (refused).
/// </summary>
public class ProductionOrderExportLimitTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    : IClassFixture<IntegrationTestFixture>
{
    private const string Export = "/api/production-orders/export";

    [Fact]
    public async Task TenThousandRows_Export_AndOneMoreIsRefusedWith422()
    {
        using var client = await fixture.CreateClientAsAsync("Admin");
        var seeded = await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM production_orders");
        var toInsert = ProductionManagementAI.Application.ProductionOrders.ProductionOrderExport.MaxRows - (int)seeded;
        await fixture.ExecuteAsOwnerAsync($"""
            INSERT INTO production_orders (order_year, order_seq, product_id, quantity, due_date, status, notes)
            SELECT 2099, s, '{SteelBracket}', 1 + s % 7, DATE '2099-01-01' + (s % 300), 'Draft', '一括データ ' || s
            FROM generate_series(1, {toInsert}) AS s
            """);

        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync(Export, HttpCompletionOption.ResponseHeadersRead);
        var records = await ProductionOrderExportEndpointTests.CsvAsync(response);
        stopwatch.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("10000", ProductionOrderExportEndpointTests.Header(response, "X-Total-Count"));
        Assert.Equal(10_001, records.Count); // header + 10,000
        output.WriteLine($"PERF: 10,000-row export in {stopwatch.ElapsedMilliseconds} ms");
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"10,000 rows took {stopwatch.ElapsedMilliseconds} ms (target 5 s)");

        await fixture.ExecuteAsOwnerAsync($"""
            INSERT INTO production_orders (order_year, order_seq, product_id, quantity, due_date, status)
            VALUES (2099, {toInsert + 1}, '{SteelBracket}', 1, DATE '2099-12-31', 'Draft')
            """);

        var refused = await client.GetAsync(Export);

        Assert.Equal((HttpStatusCode)422, refused.StatusCode);
        Assert.Equal("application/problem+json", refused.Content.Headers.ContentType!.MediaType);
        var problem = await refused.Body();
        Assert.Equal("MSG-E024", problem["code"]!.GetValue<string>());
        Assert.False(refused.Headers.Contains("X-Total-Count"));

        // A filter that brings the count back under the limit exports again.
        var filtered = await client.GetAsync($"{Export}?dueFrom=2099-12-31");
        Assert.Equal(HttpStatusCode.OK, filtered.StatusCode);
        Assert.Equal("1", ProductionOrderExportEndpointTests.Header(filtered, "X-Total-Count"));
    }
}

/// <summary>WI-016 TC-448: <see cref="PlantClock.ToPlantTime"/> (002_DD-FN-CSV §5).</summary>
public class PlantClockPlantTimeTests
{
    [Fact]
    public void ToPlantTime_IsTheTokyoWallClock_AndDateOfAgrees()
    {
        var clock = new PlantClock(new FakeTimeProvider(), Options.Create(new PlantOptions { TimeZone = "Asia/Tokyo" }));
        var utc = new DateTimeOffset(2026, 10, 6, 15, 30, 0, TimeSpan.Zero);

        Assert.Equal(new DateTime(2026, 10, 7, 0, 30, 0), clock.ToPlantTime(utc));
        Assert.Equal(new DateOnly(2026, 10, 7), clock.DateOf(utc));
        Assert.Equal(new DateOnly(2026, 10, 6), clock.DateOf(utc.AddMinutes(-31)));
    }
}
