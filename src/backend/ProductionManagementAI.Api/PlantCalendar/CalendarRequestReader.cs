using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using ProductionManagementAI.Application.PlantCalendar;

namespace ProductionManagementAI.Api.PlantCalendar;

/// <summary>Reads strictly bounded feature-only request scalars without changing old API parsing.</summary>
internal static class CalendarRequestReader
{
    internal static CalendarReadQuery Query(HttpRequest request, string operation)
    {
        string[] keys = operation switch {
            "Month" => ["month", "lineId"], "Day" => ["date", "lineId"],
            "Weekly" => ["from", "to", "view", "page", "snapshotVersion"],
            "ExceptionHistory" => ["date", "lineId", "page", "snapshotVersion"],
            "LineChoices" => ["q", "page"], "ProductChoices" => ["lineId", "q", "page"],
            "Capacity" => ["lineId", "productId", "date"], _ => [] };
        foreach (var pair in request.Query) if (!keys.Contains(pair.Key, StringComparer.Ordinal) || pair.Value.Count != 1) Bad();
        string? Get(string key) => request.Query.TryGetValue(key, out var value) ? value.ToString() : null;
        DateOnly Date(string key) { if (!CalendarValueRules.TryDate(Get(key), out var value)) Bad(key); return value; }
        Guid? Id(string key, bool required = false)
        {
            var text = Get(key); if (text is null && !required) return null;
            if (!CalendarValueRules.TryId(text, out var value)) Bad(key); return value;
        }
        var page = 1;
        if (Get("page") is { } pageText)
        {
            if (!CalendarValueRules.TryVersion(pageText, out var parsed) || parsed > 10000) Bad("page");
            page = (int)parsed;
        }
        if (!CalendarValueRules.TryText(Get("q"), 100, out var search)) Bad("q");
        var snapshot = Get("snapshotVersion");
        if (snapshot is not null && !CalendarValueRules.TryVersion(snapshot, out _)) Bad("snapshotVersion");
        if (operation is "Weekly" or "ExceptionHistory" && page > 1 && snapshot is null) Bad("snapshotVersion");
        var q = new CalendarReadQuery(Search: search, Page: page, SnapshotVersion: snapshot);
        return operation switch {
            "Month" => CalendarValueRules.TryMonth(Get("month"), out var month) ? q with { Date = month, LineId = Id("lineId") } : throw Invalid("month"),
            "Day" or "ExceptionHistory" => q with { Date = Date("date"), LineId = Id("lineId") },
            "Weekly" => Weekly(q, Date("from"), Date("to"), Get("view") ?? "Current"),
            "LineChoices" => q,
            "ProductChoices" => q with { LineId = Id("lineId", true) },
            "Capacity" => q with { LineId = Id("lineId", true), ProductId = Id("productId", true), Date = Date("date") },
            _ => throw Invalid() };
    }
    private static CalendarReadQuery Weekly(CalendarReadQuery q, DateOnly from, DateOnly to, string view)
    {
        if (from > to || to.DayNumber - from.DayNumber >= 366 || view is not ("Current" or "History")) Bad();
        return q with { From = from, To = to, View = view };
    }

    internal static async Task<CalendarCommand> Command(HttpRequest request, string kind, string? effectiveFrom, CancellationToken ct)
    {
        if (request.Query.Count != 0) Bad();
        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var media) || !string.Equals(media.MediaType, "application/json", StringComparison.OrdinalIgnoreCase)
            || media.Parameters.Any(p => !string.Equals(p.Name, "charset", StringComparison.OrdinalIgnoreCase))
            || media.Parameters.Count > 1 || media.CharSet is { } charset && !string.Equals(charset.Trim('"'), "utf-8", StringComparison.OrdinalIgnoreCase))
            throw new CalendarRuleException(new("UNSUPPORTED_MEDIA_TYPE", 415, "NotApplied"));
        if (request.ContentLength is > 8192) Large();
        using var memory = new MemoryStream(); var buffer = new byte[1024];
        while (true)
        {
            var length = await request.Body.ReadAsync(buffer, ct); if (length == 0) break;
            if (memory.Length + length > 8192) Large(); memory.Write(buffer, 0, length);
        }
        try
        {
            using var document = JsonDocument.Parse(memory.ToArray(), new JsonDocumentOptions { MaxDepth = 4, AllowTrailingCommas = false });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) Bad();
            string[] keys = kind switch {
                "SaveWeekly" => ["version", "targetRevisionId", "workingDays"],
                "WithdrawWeekly" => ["version", "targetRevisionId"],
                "SaveException" => ["version", "targetRevisionId", "lineId", "date", "isWorking", "workingHours", "reason"],
                "RemoveException" => ["version", "targetRevisionId", "lineId", "date"], _ => [] };
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in root.EnumerateObject()) if (!keys.Contains(property.Name, StringComparer.Ordinal) || !seen.Add(property.Name)) Bad();
            if (seen.Count != keys.Length) Bad();
            string? Text(string name, bool nullable = false)
            {
                var item = root.GetProperty(name);
                if (nullable && item.ValueKind == JsonValueKind.Null) return null;
                if (item.ValueKind != JsonValueKind.String) Bad(name);
                var text = item.GetString();
                if (!CalendarValueRules.TryText(text, int.MaxValue, out _)) Bad(name);
                return text;
            }
            Guid? Id(string name, bool nullable)
            {
                var text = Text(name, nullable); if (text is null && nullable) return null;
                if (!CalendarValueRules.TryId(text, out var id)) Bad(name); return id;
            }
            if (!CalendarValueRules.TryVersion(Text("version"), out var version)) Bad("version");
            var target = Id("targetRevisionId", kind is "SaveWeekly" or "SaveException");
            var dateText = kind is "SaveWeekly" or "WithdrawWeekly" ? effectiveFrom : Text("date");
            if (!CalendarValueRules.TryDate(dateText, out var date)) Bad("date");
            short? mask = null; bool? working = null; decimal? hours = null; string? reason = null; Guid? lineId = null;
            if (kind == "SaveWeekly")
            {
                var array = root.GetProperty("workingDays"); if (array.ValueKind != JsonValueKind.Array) Bad("workingDays");
                var days = new List<string>();
                foreach (var day in array.EnumerateArray()) { if (day.ValueKind != JsonValueKind.String) Bad("workingDays"); days.Add(day.GetString() ?? string.Empty); }
                if (!CalendarValueRules.TryMask(days, out var parsed)) Bad("workingDays"); mask = parsed;
            }
            if (kind is "SaveException" or "RemoveException") lineId = Id("lineId", true);
            if (kind == "SaveException")
            {
                var item = root.GetProperty("isWorking"); if (item.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) Bad("isWorking"); working = item.GetBoolean();
                if (Text("workingHours", true) is { } text)
                {
                    if (!CalendarValueRules.TryHours(text, out var parsed)) Bad("workingHours"); hours = parsed;
                }
                if (working == false && hours is not null) Bad("workingHours");
                if (!CalendarValueRules.TryText(Text("reason", true), 500, out reason)) Bad("reason");
            }
            return new(kind, date, version, target, lineId, mask, working, hours, reason);
        }
        catch (JsonException) { throw Invalid(); }
        catch (InvalidOperationException) { throw Invalid(); }
        catch (ArgumentException) { throw Invalid(); }
    }
    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static void Bad(string? field = null) => throw Invalid(field);
    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static void Large() => throw new CalendarRuleException(new("REQUEST_TOO_LARGE", 413, "NotApplied"));
    private static CalendarRuleException Invalid(string? field = null) => new(new("VALIDATION", 400, "NotApplied",
        field is null ? null : new Dictionary<string, string[]> { [field] = ["VALIDATION"] }));
}
