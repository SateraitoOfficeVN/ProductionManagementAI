using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using ProductionManagementAI.Application.ProductionLines;
using ProductionManagementAI.Infrastructure;
using static ProductionManagementAI.Integration.Tests.ProductionOrders.ProductionOrderApi;

namespace ProductionManagementAI.Integration.Tests.ProductionLines;

/// <summary>Interleaves real database commits after a snapshot read establishes visibility.</summary>
public sealed class LineSnapshotFixture : IntegrationTestFixture
{
    /// <summary>Gets the single-use database observation hook.</summary>
    public SnapshotObserver Observer { get; } = new();
    /// <summary>Gets the pinned audit clock used for same-time aggregate updates.</summary>
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2031, 6, 11, 3, 0, 0, TimeSpan.Zero));
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services => {
            services.AddDbContext<AppDbContext>(options => options.AddInterceptors(Observer));
            services.RemoveAll<TimeProvider>(); services.AddSingleton<TimeProvider>(Time);
        });
    }

    /// <summary>Runs an independent commit after a matching reader executes.</summary>
    public sealed class SnapshotObserver : DbCommandInterceptor
    {
        /// <summary>Gets or sets the SQL fragment that establishes the test snapshot.</summary>
        public string? Fragment { get; set; }
        /// <summary>Gets or sets the independent write to run once.</summary>
        public Func<Task>? AfterRead { get; set; }
        /// <summary>Gets the observed SQL commands.</summary>
        public List<string> Commands { get; } = [];
        /// <summary>Gets or sets a one-time reader SQL replacement for timeout rehearsal.</summary>
        public string? NextReaderSql { get; set; }
        /// <inheritdoc />
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (NextReaderSql is { } sql && command.CommandText.Contains("production_lines", StringComparison.Ordinal)) { NextReaderSql = null; command.CommandText = sql; }
            return ValueTask.FromResult(result);
        }
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText);
            if (AfterRead is { } action && Fragment is { } fragment && command.CommandText.Contains(fragment, StringComparison.Ordinal))
            {
                AfterRead = null;
                await action();
            }
            return result;
        }
    }
}

/// <summary>Isolates process-wide meter listeners from unrelated parallel HTTP fixtures.</summary>
[CollectionDefinition("Line telemetry isolation", DisableParallelization = true)]
public sealed class LineTelemetryCollection;

/// <summary>Checks coherent count/rows and real bounded telemetry completion.</summary>
[Collection("Line telemetry isolation")]
public sealed class LineSnapshotTelemetryTests(LineSnapshotFixture fixture) : IClassFixture<LineSnapshotFixture>
{
    [Fact]
    public async Task ListUsesOneSnapshotAndLiteralSearchWithConstantQueryCount()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var code = $"SNAP%_{Guid.NewGuid():N}";
        fixture.Observer.Commands.Clear();
        fixture.Observer.Fragment = "count(*)";
        fixture.Observer.AfterRead = () => fixture.ExecuteAsOwnerAsync($"INSERT INTO production_lines (id,code,name,working_hours_per_day,is_active,created_at_utc,updated_at_utc) VALUES ('{Guid.NewGuid()}', '{code}', 'snapshot', 8, true, now(), now())");
        var first = await (await client.GetAsync("/api/production-lines?q=" + Uri.EscapeDataString(code))).Body();
        Assert.Null(fixture.Observer.AfterRead);
        Assert.Equal(0, first["total"]?.GetValue<int>());
        Assert.Empty(first["items"]!.AsArray());
        Assert.Equal(2, fixture.Observer.Commands.Count(sql => sql.Contains("production_lines", StringComparison.Ordinal)));
        var next = await (await client.GetAsync("/api/production-lines?q=" + Uri.EscapeDataString(code))).Body();
        Assert.Equal(1, next["total"]?.GetValue<int>());
        Assert.Equal(code, Assert.Single(next["items"]!.AsArray())!["code"]!.GetValue<string>());
    }

    [Fact]
    public async Task EligibleProductAndRowsRetainTheSameSnapshotAcrossUnitChange()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var product = await client.PostJson("/api/product-master", new System.Text.Json.Nodes.JsonObject { ["sku"] = $"SNAP-{Guid.NewGuid():N}", ["name"] = "snapshot", ["unit"] = "kg" });
        product.EnsureSuccessStatusCode(); var item = await product.Body(); var id = item["id"]!.GetValue<string>();
        var lineId = await client.CreateEligibleLine(id);
        fixture.Observer.Fragment = "FROM products";
        fixture.Observer.AfterRead = () => fixture.ExecuteAsOwnerAsync($"UPDATE products SET unit = 'm' WHERE id = '{id}'");
        var first = await (await client.GetAsync($"/api/production-lines/eligible?productId={id}")).Body();
        Assert.Null(fixture.Observer.AfterRead);
        Assert.Equal("kg", first["product"]!["unit"]!.GetValue<string>());
        Assert.Contains(first["items"]!.AsArray(), row => row!["id"]!.GetValue<string>() == lineId);
        var next = await (await client.GetAsync($"/api/production-lines/eligible?productId={id}")).Body();
        Assert.Equal("m", next["product"]!["unit"]!.GetValue<string>());
        Assert.DoesNotContain(next["items"]!.AsArray(), row => row!["id"]!.GetValue<string>() == lineId);
    }




    [Fact]
    public async Task DetailParentAndPairRowsDoNotMixConcurrentAggregateVersions()
    {
        using var client = await fixture.CreateClientAsAsync("Operator"); var id = await client.CreateEligibleLine(SteelBracket.ToString());
        var before = await (await client.GetAsync($"/api/production-lines/{id}")).Body();
        fixture.Observer.Fragment = "FROM production_lines";
        fixture.Observer.AfterRead = () => fixture.ExecuteAsOwnerAsync($"BEGIN; UPDATE production_lines SET name = 'after snapshot' WHERE id = '{id}'; UPDATE production_line_products SET minutes_per_unit = 0.125 WHERE line_id = '{id}'; COMMIT;");
        var first = await (await client.GetAsync($"/api/production-lines/{id}")).Body();
        Assert.Equal(before.ToJsonString(), first.ToJsonString()); Assert.Null(fixture.Observer.AfterRead);
        var next = await (await client.GetAsync($"/api/production-lines/{id}")).Body();
        Assert.Equal("after snapshot", next["name"]!.GetValue<string>()); Assert.Equal("0.125", next["pairs"]!["items"]![0]!["minutesPerUnit"]!.GetValue<string>());
    }

    [Fact]
    public async Task ProductChoiceCountAndRowsUseOneSnapshot()
    {
        using var client = await fixture.CreateClientAsAsync("Operator"); var code = $"CHOICE-{Guid.NewGuid():N}";
        fixture.Observer.Fragment = "count(*)";
        fixture.Observer.AfterRead = () => fixture.ExecuteAsOwnerAsync($"INSERT INTO products (id,sku,name,unit,is_active) VALUES ('{Guid.NewGuid()}','{code}','snapshot','kg',true)");
        var first = await (await client.GetAsync("/api/production-lines/product-choices?q=" + code)).Body();
        Assert.Equal(0, first["total"]!.GetValue<int>()); Assert.Empty(first["items"]!.AsArray()); Assert.Null(fixture.Observer.AfterRead);
        var next = await (await client.GetAsync("/api/production-lines/product-choices?q=" + code)).Body();
        Assert.Equal(1, next["total"]!.GetValue<int>()); Assert.Single(next["items"]!.AsArray());
    }

    [Fact]
    public async Task FixedPagesHaveStableCodeOrderingNoOverlapAndEmptyBeyondTotal()
    {
        using var client = await fixture.CreateClientAsAsync("Operator"); var prefix = $"PAGE-{Guid.NewGuid():N}";
        await fixture.ExecuteAsOwnerAsync($"INSERT INTO production_lines(code,name,working_hours_per_day) SELECT '{prefix}' || lpad(n::text,2,'0'), 'page fixture', 8 FROM generate_series(1,51) n");
        var first = await (await client.GetAsync("/api/production-lines?q=" + prefix)).Body();
        var second = await (await client.GetAsync("/api/production-lines?q=" + prefix + "&page=2")).Body();
        Assert.Equal(51, first["total"]!.GetValue<int>()); Assert.Equal(50, first["items"]!.AsArray().Count);
        Assert.Equal(prefix + "51", Assert.Single(second["items"]!.AsArray())!["code"]!.GetValue<string>());
        Assert.Equal(Enumerable.Range(1,50).Select(i => prefix + i.ToString("00")), first["items"]!.AsArray().Select(row => row!["code"]!.GetValue<string>()));
        var beyond = await (await client.GetAsync("/api/production-lines?q=" + prefix + "&page=10000")).Body(); Assert.Empty(beyond["items"]!.AsArray()); Assert.Equal(51, beyond["total"]!.GetValue<int>());
    }

    [Fact]
    public async Task ActualStatementTimeoutHasBoundedCleanupAndAllowsNextSnapshot()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        fixture.Observer.NextReaderSql = "SELECT count(*)::int FROM pg_sleep(11)";
        var watch = Stopwatch.StartNew();
        var response = await client.GetAsync("/api/production-lines");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("LINE_BUSY", (await response.Body())["code"]!.GetValue<string>());
        Assert.InRange(watch.Elapsed.TotalSeconds, 9, 15);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/production-lines")).StatusCode);
    }

    [Fact]
    public async Task PairOnlyUpdateAdvancesParentXminEvenWhenAuditClockDoesNotMove()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        var id = await client.CreateEligibleLine(SteelBracket.ToString());
        var original = await (await client.GetAsync($"/api/production-lines/{id}")).Body();
        var pair = original["pairs"]!["items"]![0]!; var product = pair["product"]!;
        var input = new System.Text.Json.Nodes.JsonObject { ["name"] = original["name"]!.DeepClone(), ["workingHoursPerDay"] = original["workingHoursPerDay"]!.DeepClone(), ["version"] = original["version"]!.DeepClone(),
            ["productChanges"] = new System.Text.Json.Nodes.JsonArray(new System.Text.Json.Nodes.JsonObject { ["action"] = "setTiming", ["productId"] = product["id"]!.DeepClone(), ["minutesPerUnit"] = "0.125", ["expectedUnit"] = product["unit"]!.DeepClone(), ["expectedUnitRevision"] = product["unitRevision"]!.DeepClone(), ["confirmUnit"] = false }) };
        var updated = await client.PutJson($"/api/production-lines/{id}", input); updated.EnsureSuccessStatusCode(); var saved = await updated.Body();
        Assert.Equal(original["updatedAt"]!.GetValue<string>(), saved["updatedAt"]!.GetValue<string>());
        Assert.NotEqual(original["version"]!.GetValue<string>(), saved["version"]!.GetValue<string>());
        Assert.Equal("0.125", saved["pairs"]!["items"]![0]!["minutesPerUnit"]!.GetValue<string>());
        var stale = await client.PutJson($"/api/production-lines/{id}", input);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode); Assert.Equal("LINE_STALE", (await stale.Body())["code"]!.GetValue<string>());
    }

    [Fact]
    public async Task EachHttpOutcomeRecordsOneCounterAndDurationWithBoundedTagsAndRedactedSpans()
    {
        using var client = await fixture.CreateClientAsAsync("Operator");
        using var anonymous = fixture.CreateClient();
        var samples = new ConcurrentQueue<(string Instrument, KeyValuePair<string, object?>[] Tags)>();
        using var meter = new MeterListener();
        meter.InstrumentPublished = (instrument, listener) => { if (instrument.Meter.Name == ProductionLineService.TelemetryName) listener.EnableMeasurementEvents(instrument); };
        meter.SetMeasurementEventCallback<long>((instrument, value, tags, _) => { Assert.Equal(1, value); samples.Enqueue((instrument.Name, tags.ToArray())); });
        meter.SetMeasurementEventCallback<double>((instrument, value, tags, _) => { Assert.InRange(value, 0, 20); samples.Enqueue((instrument.Name, tags.ToArray())); });
        meter.Start();
        var activities = new ConcurrentQueue<Activity>();
        using var listener = new ActivityListener {
            ShouldListenTo = source => source.Name == ProductionLineService.TelemetryName || source.Name == "Microsoft.AspNetCore",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Enqueue };
        ActivitySource.AddActivityListener(listener);
        var marker = $"PRIVATE-{Guid.NewGuid():N}";
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/production-lines?q=" + marker)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/production-lines?page=0")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/production-lines?q=" + marker)).StatusCode);
        Assert.Equal(3, samples.Count(sample => sample.Instrument.EndsWith("requests", StringComparison.Ordinal)));
        Assert.Equal(3, samples.Count(sample => sample.Instrument.EndsWith("duration", StringComparison.Ordinal)));
        Assert.All(samples, sample => {
            Assert.Equal(2, sample.Tags.Length);
            Assert.Equal("list", Assert.Single(sample.Tags, tag => tag.Key == "operation").Value);
            Assert.Contains(Assert.Single(sample.Tags, tag => tag.Key == "outcome").Value, new object[] { "success", "validation", "forbidden" });
        });
        Assert.Equal(2, activities.Count(activity => activity.Source.Name == ProductionLineService.TelemetryName));
        Assert.All(activities, activity => Assert.DoesNotContain(marker, string.Join(";", activity.TagObjects.Select(tag => $"{tag.Key}={tag.Value}"))));
        Assert.All(activities.Where(activity => activity.Source.Name == ProductionLineService.TelemetryName), activity => Assert.Equal(2, activity.TagObjects.Count()));
    }
}
