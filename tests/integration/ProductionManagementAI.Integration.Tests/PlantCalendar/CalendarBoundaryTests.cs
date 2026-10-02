using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.PlantCalendar;

/// <summary>Verifies the streamed body boundary without a Content-Length shortcut.</summary>
public sealed class CalendarStreamBoundaryTests(CalendarFixture fixture) : IClassFixture<CalendarFixture>
{
    /// <summary>Checks that an exact-limit request reaches validation and an extra byte cannot reach storage.</summary>
    /// <param name="length">The complete streamed UTF-8 body size.</param>
    /// <param name="expected">The expected HTTP result.</param>
    [Theory]
    [InlineData(8192, HttpStatusCode.Conflict)]
    [InlineData(8193, HttpStatusCode.RequestEntityTooLarge)]
    public async Task StreamedBodyHonorsExactByteLimit(int length, HttpStatusCode expected)
    {
        const string json = "{\"version\":\"1\",\"targetRevisionId\":null,\"workingDays\":[]}";
        var bytes = Encoding.UTF8.GetBytes(json.PadRight(length));
        Assert.Equal(length, bytes.Length);
        using var client = await fixture.CreateClientAsAsync("Operator");
        using var content = new ChunkedContent(bytes);
        Assert.Null(content.Headers.ContentLength);
        var response = await client.PutAsync("/api/plant-calendar/weekly/2031-06-11", content);
        Assert.Equal(expected, response.StatusCode);
        var problem = await response.Body();
        Assert.Equal(length == 8192 ? "CALENDAR_NOT_ACTIVATED" : "REQUEST_TOO_LARGE", problem["code"]!.GetValue<string>());
        Assert.Equal("NotApplied", problem["writeOutcome"]!.GetValue<string>());
        Assert.Equal(0L, await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_state"));
    }

    private sealed class ChunkedContent : HttpContent
    {
        private readonly byte[] bytes;
        internal ChunkedContent(byte[] bytes)
        {
            this.bytes = bytes;
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            for (var offset = 0; offset < bytes.Length; offset += 127)
                await stream.WriteAsync(bytes.AsMemory(offset, Math.Min(127, bytes.Length - offset)));
        }
    }
}

/// <summary>Verifies real retained multi-page history and bounded active-line choices.</summary>
public sealed class CalendarPagingTests(CalendarFixture fixture) : IClassFixture<CalendarFixture>
{
    /// <summary>Checks sorted non-overlapping pages, retained markers and stale snapshot rejection after a new commit.</summary>
    [Fact]
    public async Task HistoryAndChoicesHaveStableBoundedPagesAndHistoryRejectsStaleToken()
    {
        await fixture.ExecuteAsOwnerAsync("BEGIN; INSERT INTO plant_calendar_state (id,activated_on,time_zone_id,revision) VALUES (1,'2031-06-10','Asia/Tokyo',1); INSERT INTO plant_calendar_weekly_revisions (calendar_id,effective_from,working_weekdays,is_withdrawn,is_current,commit_revision) VALUES (1,'2031-06-10',31,false,true,1); COMMIT;");
        using var client = await fixture.CreateClientAsAsync("Admin");
        string version = "1";
        string? target = null;
        for (var number = 0; number < 23; number++)
        {
            var response = number == 22
                ? await client.PostAsJsonAsync("/api/plant-calendar/exceptions/remove", new { version, targetRevisionId = target, lineId = (string?)null, date = "2031-06-12" })
                : await client.PutAsJsonAsync("/api/plant-calendar/exceptions", new { version, targetRevisionId = target, lineId = (string?)null, date = "2031-06-12", isWorking = false, workingHours = (string?)null, reason = "history " + number.ToString(CultureInfo.InvariantCulture) });
            response.EnsureSuccessStatusCode();
            var body = await response.Body();
            version = body["context"]!["version"]!.GetValue<string>();
            target = body["target"]!["id"]!.GetValue<string>();
        }
        const string history = "/api/plant-calendar/exception-history?date=2031-06-12";
        var first = await (await client.GetAsync(history)).Body();
        Assert.Equal(23, first["totalCount"]!.GetValue<int>());
        Assert.Equal(2, first["totalPages"]!.GetValue<int>());
        Assert.Equal(20, first["items"]!.AsArray().Count);
        Assert.True(first["current"]!["isRemoved"]!.GetValue<bool>());
        Assert.Equal(target, first["current"]!["id"]!.GetValue<string>());
        var second = await (await client.GetAsync(history + "&page=2&snapshotVersion=" + version)).Body();
        Assert.Equal(3, second["items"]!.AsArray().Count);
        var rows = first["items"]!.AsArray().Concat(second["items"]!.AsArray()).ToArray();
        Assert.Equal(23, rows.Select(row => row!["id"]!.GetValue<string>()).Distinct().Count());
        Assert.Equal(Enumerable.Range(2, 23).Reverse().Select(value => value.ToString(CultureInfo.InvariantCulture)), rows.Select(row => row!["commitRevision"]!.GetValue<string>()));
        var beyond = await (await client.GetAsync(history + "&page=3&snapshotVersion=" + version)).Body();
        Assert.Empty(beyond["items"]!.AsArray());
        var changed = await client.PutAsJsonAsync("/api/plant-calendar/weekly/2031-06-13", new { version, targetRevisionId = (string?)null, workingDays = Array.Empty<string>() });
        changed.EnsureSuccessStatusCode();
        var stale = await client.GetAsync(history + "&page=2&snapshotVersion=" + version);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("CALENDAR_STALE", (await stale.Body())["code"]!.GetValue<string>());

        var sql = new StringBuilder("INSERT INTO production_lines (id,code,name,working_hours_per_day,is_active,created_at_utc,updated_at_utc) VALUES ");
        for (var number = 0; number < 52; number++)
        {
            if (number > 0) sql.Append(',');
            sql.Append(CultureInfo.InvariantCulture, $"('{Guid.NewGuid()}','CAL-PAGE-{number:D3}','Pagination fixture',8,true,now(),now())");
        }
        await fixture.ExecuteAsOwnerAsync(sql.ToString());
        var choices1 = await (await client.GetAsync("/api/plant-calendar/line-choices?q=CAL-PAGE-&page=1")).Body();
        var choices2 = await (await client.GetAsync("/api/plant-calendar/line-choices?q=CAL-PAGE-&page=2")).Body();
        Assert.Equal(52, choices1["totalCount"]!.GetValue<int>());
        Assert.Equal(50, choices1["items"]!.AsArray().Count);
        Assert.Equal(2, choices2["items"]!.AsArray().Count);
        var codes = choices1["items"]!.AsArray().Concat(choices2["items"]!.AsArray()).Select(row => row!["code"]!.GetValue<string>()).ToArray();
        Assert.Equal(Enumerable.Range(0, 52).Select(value => "CAL-PAGE-" + value.ToString("D3", CultureInfo.InvariantCulture)), codes);
    }
}

/// <summary>Verifies checked bigint exhaustion preserves every persisted row and revision.</summary>
public sealed class CalendarOverflowTests(CalendarFixture fixture) : IClassFixture<CalendarFixture>
{
    /// <summary>Checks that a changed payload cannot wrap the global revision or append partial history.</summary>
    [Fact]
    public async Task RevisionOverflowIsUnknownWithoutPartialWriteOrReset()
    {
        await fixture.ExecuteAsOwnerAsync("BEGIN; INSERT INTO plant_calendar_state (id,activated_on,time_zone_id,revision) VALUES (1,'2031-06-10','Asia/Tokyo',9223372036854775807); INSERT INTO plant_calendar_weekly_revisions (calendar_id,effective_from,working_weekdays,is_withdrawn,is_current,commit_revision) VALUES (1,'2031-06-10',31,false,true,1); COMMIT;");
        using var client = await fixture.CreateClientAsAsync("Admin");
        var response = await client.PutAsJsonAsync("/api/plant-calendar/exceptions", new { version = "9223372036854775807", targetRevisionId = (string?)null, lineId = (string?)null, date = "2031-06-12", isWorking = false, workingHours = (string?)null, reason = (string?)null });
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var problem = await response.Body();
        Assert.Equal("UNEXPECTED", problem["code"]!.GetValue<string>());
        Assert.Equal("Unknown", problem["writeOutcome"]!.GetValue<string>());
        Assert.Equal(long.MaxValue, await fixture.ScalarAsOwnerAsync<long>("SELECT revision FROM plant_calendar_state"));
        Assert.Equal(0L, await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_exception_revisions"));
        Assert.Equal(1L, await fixture.ScalarAsOwnerAsync<long>("SELECT count(*) FROM plant_calendar_weekly_revisions WHERE is_current"));
    }
}
