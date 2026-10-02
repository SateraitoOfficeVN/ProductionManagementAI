using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ProductionManagementAI.Application.PlantCalendar;

/// <summary>Defines bounded feature instrumentation with one final request metric owner.</summary>
public static class CalendarTelemetry
{
    /// <summary>Gets the registered activity and meter name.</summary>
    public const string Name = "ProductionManagementAI.PlantCalendar";
    /// <summary>Gets the feature activity source.</summary>
    public static readonly ActivitySource Source = new(Name);
    /// <summary>Gets the feature meter.</summary>
    public static readonly Meter Meter = new(Name);
    private static readonly Counter<long> Count = Meter.CreateCounter<long>("plant_calendar_operations_total");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("plant_calendar_duration_ms", "ms");
    /// <summary>Records a single completed request using bounded operation and outcome labels.</summary>
    public static void Record(string operation, string outcome, double milliseconds)
    {
        var tags = new TagList { { "operation", operation }, { "outcome", outcome } };
        Count.Add(1, tags); Duration.Record(milliseconds, tags);
    }
}
