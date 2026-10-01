using System.Text.Json;
using ProductionManagementAI.Application.ProductionLines;

namespace ProductionManagementAI.Api.ProductionLines;

internal static class LineRequestReader
{
    internal const int MaximumBytes = 256 * 1024;
    internal static bool Query(HttpRequest request, params string[] allowed) =>
        request.Query.All(pair => allowed.Contains(pair.Key, StringComparer.Ordinal) && pair.Value.Count == 1);
    internal static bool Id(string? value, out Guid id) => Guid.TryParse(value, out id) && id != Guid.Empty;

    internal static async Task<LineResult<JsonElement>> Body(HttpRequest request, CancellationToken ct)
    {
        if (request.ContentType?.Split(';')[0].Trim().Equals("application/json", StringComparison.OrdinalIgnoreCase) != true)
            return LineResult<JsonElement>.Failure(415, "UNSUPPORTED_MEDIA_TYPE");
        if (request.ContentLength > MaximumBytes) return LineResult<JsonElement>.Failure(413, "REQUEST_TOO_LARGE");
        using var stream = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            var read = await request.Body.ReadAsync(buffer, ct);
            if (read == 0) break;
            if (stream.Length + read > MaximumBytes) return LineResult<JsonElement>.Failure(413, "REQUEST_TOO_LARGE");
            stream.Write(buffer, 0, read);
        }
        try
        {
            using var json = JsonDocument.Parse(stream.ToArray(), new JsonDocumentOptions { MaxDepth = 16 });
            return json.RootElement.ValueKind == JsonValueKind.Object
                ? LineResult<JsonElement>.Success(json.RootElement.Clone())
                : LineResult<JsonElement>.Failure(400, "VALIDATION", "body");
        }
        catch (JsonException) { return LineResult<JsonElement>.Failure(400, "VALIDATION", "body"); }
    }

    internal static bool Shape(JsonElement element, params string[] fields)
    {
        if (element.ValueKind != JsonValueKind.Object) return false;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
            if (!seen.Add(property.Name) || !fields.Contains(property.Name, StringComparer.Ordinal)) return false;
        return fields.All(seen.Contains);
    }
    internal static bool Text(JsonElement element, string key, out string value)
    {
        value = string.Empty;
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(key, out var property) || property.ValueKind != JsonValueKind.String) return false;
        value = property.GetString() ?? string.Empty;
        return true;
    }
    internal static bool Inputs(JsonElement root, string key, bool create, out IReadOnlyList<LineProductInput> values)
    {
        var rows = new List<LineProductInput>();
        values = rows;
        if (!root.TryGetProperty(key, out var array) || array.ValueKind != JsonValueKind.Array || array.GetArrayLength() > 1000) return false;
        foreach (var row in array.EnumerateArray())
        {
            var action = "add";
            if (!create && !Text(row, "action", out action)) return false;
            var fields = action == "retire" ? new[] { "action", "productId" }
                : create ? ["productId", "minutesPerUnit", "expectedUnit", "expectedUnitRevision", "confirmUnit"]
                : new[] { "action", "productId", "minutesPerUnit", "expectedUnit", "expectedUnitRevision", "confirmUnit" };
            if (!Shape(row, fields) || !Text(row, "productId", out var rawId) || !Id(rawId, out var id)) return false;
            if (action == "retire") { rows.Add(new(action, id)); continue; }
            if (action is not ("add" or "setTiming") || !Text(row, "minutesPerUnit", out var minutes) ||
                !Text(row, "expectedUnit", out var unit) || !Text(row, "expectedUnitRevision", out var revision)) return false;
            var confirmation = row.GetProperty("confirmUnit");
            if (confirmation.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) return false;
            rows.Add(new(action, id, minutes, unit, revision, confirmation.GetBoolean()));
        }
        return true;
    }
}
