using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using ProductionManagementAI.Application.PlantCalendar;

namespace ProductionManagementAI.Integration.Tests.PlantCalendar;

/// <summary>Isolates global listeners from unrelated concurrently running fixture requests.</summary>
[CollectionDefinition("Calendar telemetry isolation", DisableParallelization = true)]
public sealed class CalendarTelemetryCollection;

/// <summary>Checks one bounded outcome measurement per request and sanitized actual feature traces.</summary>
[Collection("Calendar telemetry isolation")]
public sealed class CalendarTelemetryTests(CalendarFixture fixture) : IClassFixture<CalendarFixture>
{
    [Fact]
    public async Task RequestMetricsIncludeParserAndAuthorizationExactlyOnceWithoutRawValues()
    {
        var metrics = new ConcurrentQueue<(string Instrument, string Operation, string Outcome)>();
        var traces = new ConcurrentQueue<Activity>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, owner) => { if (instrument.Meter.Name == CalendarTelemetry.Name) owner.EnableMeasurementEvents(instrument); };
        void Record<T>(Instrument instrument, T value, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
        {
            var labels = tags.ToArray();
            Assert.Equal(2, labels.Length);
            Assert.All(labels, tag => Assert.Contains(tag.Key, new[] { "operation", "outcome" }));
            metrics.Enqueue((instrument.Name, labels.Single(x => x.Key == "operation").Value?.ToString() ?? "", labels.Single(x => x.Key == "outcome").Value?.ToString() ?? ""));
        }
        listener.SetMeasurementEventCallback<long>(Record);listener.SetMeasurementEventCallback<double>(Record);listener.Start();
        using var activities = new ActivityListener { ShouldListenTo = source => source.Name == CalendarTelemetry.Name || source.Name == "Npgsql" || source.Name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => traces.Enqueue(activity) };
        ActivitySource.AddActivityListener(activities);
        using var anonymous = fixture.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/plant-calendar/month?month=2031-06")).StatusCode);
        using var denied = await fixture.CreateClientAsAsync(null);
        Assert.Equal(HttpStatusCode.Forbidden, (await denied.GetAsync("/api/plant-calendar/month?month=2031-06")).StatusCode);
        using var client = await fixture.CreateClientAsAsync("Operator");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/plant-calendar/month?month=secret-input&extra=private-note")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/plant-calendar/month?month=2031-06")).StatusCode);
        var counts = metrics.Where(x => x.Instrument == "plant_calendar_operations_total").ToArray();
        var durations = metrics.Where(x => x.Instrument == "plant_calendar_duration_ms").ToArray();
        Assert.Equal(4, counts.Length);Assert.Equal(4, durations.Length);
        Assert.All(counts, metric => Assert.Equal("Month", metric.Operation));
        Assert.Equal(new[] { "unauthorized", "forbidden", "validation", "success" }, counts.Select(x => x.Outcome));
        Assert.Contains(traces, activity => activity.DisplayName == "plant-calendar.Month");
        Assert.Contains(traces, activity => activity.DisplayName == "plant-calendar.read");
        Assert.Contains(traces, activity => activity.DisplayName == "plant-calendar.resolve");
        var feature = traces.Where(activity => activity.Source.Name == CalendarTelemetry.Name || AncestorIsCalendar(activity)).ToArray();
        Assert.NotEmpty(feature);
        Assert.All(feature, activity => {
            Assert.Null(activity.GetTagItem("db.statement"));Assert.Null(activity.GetTagItem("db.query.text"));
            Assert.Null(activity.GetTagItem("url.query"));Assert.Null(activity.GetTagItem("url.full"));
            Assert.DoesNotContain(activity.TagObjects, tag => tag.Value?.ToString()?.Contains("private-note", StringComparison.Ordinal) == true);
        });
    }
    private static bool AncestorIsCalendar(Activity activity)
    {
        for (Activity? parent = activity.Parent; parent is not null; parent = parent.Parent)
            if (parent.Source.Name == CalendarTelemetry.Name) return true;
        return false;
    }
}
