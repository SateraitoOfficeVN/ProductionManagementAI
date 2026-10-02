using System.Diagnostics;
using OpenTelemetry;
using ProductionManagementAI.Application.PlantCalendar;

namespace ProductionManagementAI.Api.PlantCalendar;

/// <summary>Removes raw URL and SQL attributes from feature-scoped traces before export.</summary>
public sealed class CalendarTraceProcessor : BaseProcessor<Activity>
{
    /// <inheritdoc />
    public override void OnEnd(Activity activity)
    {
        if (!IsCalendar(activity)) return;
        foreach (var tag in activity.TagObjects.ToArray())
            if (tag.Key is "url.query" or "url.full" or "url.path" or "http.url" or "http.target"
                or "db.statement" or "db.query.text" or "db.query.summary" || tag.Key.StartsWith("db.operation.parameter.", StringComparison.Ordinal))
                activity.SetTag(tag.Key, null);
        if (activity.Source.Name == "Npgsql") activity.DisplayName = "plant-calendar.database";
    }
    private static bool IsCalendar(Activity activity)
    {
        for (Activity? current = activity; current is not null; current = current.Parent)
        {
            if (current.Source.Name == CalendarTelemetry.Name) return true;
            foreach (var key in new[] { "url.path", "http.route", "url.full", "http.url" })
                if (current.GetTagItem(key) is string value && value.Contains("/api/plant-calendar", StringComparison.Ordinal)) return true;
        }
        return false;
    }
}
