using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProductionManagementAI.Integration.Tests.ProductionOrders;

/// <summary>Small JSON helpers so tests can send exactly the payloads 001_DD-API describes, including malformed ones.</summary>
internal static class ProductionOrderApi
{
    public static readonly Guid SteelBracket = Guid.Parse("0197e4a0-0000-7000-8000-000000001001");
    public static readonly Guid DriveShaft = Guid.Parse("0197e4a0-0000-7000-8000-000000001004");

    public static DateOnly PlantToday => DateOnly.FromDateTime(
        TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo")).DateTime);

    public static string Date(DateOnly d) => d.ToString("yyyy-MM-dd");

    public static JsonObject ValidCreate(Guid? product = null, int quantity = 250, DateOnly? due = null, string? notes = "note") => new()
    {
        ["productId"] = (product ?? SteelBracket).ToString(),
        ["quantity"] = quantity,
        ["dueDate"] = Date(due ?? PlantToday.AddDays(10)),
        ["notes"] = notes,
    };

    public static JsonObject UpdateFrom(JsonObject order) => new()
    {
        ["productId"] = order["productId"]!.GetValue<string>(),
        ["quantity"] = order["quantity"]!.GetValue<int>(),
        ["dueDate"] = order["dueDate"]!.GetValue<string>(),
        ["status"] = order["status"]!.GetValue<string>(),
        ["notes"] = order["notes"]?.GetValue<string>(),
        ["version"] = order["version"]!.GetValue<uint>(),
    };

    public static Task<HttpResponseMessage> PostJson(this HttpClient client, string url, JsonNode body) =>
        client.PostAsync(url, Json(body));

    public static Task<HttpResponseMessage> PutJson(this HttpClient client, string url, JsonNode body) =>
        client.PutAsync(url, Json(body));

    public static Task<HttpResponseMessage> PostRaw(this HttpClient client, string url, string json) =>
        client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

    public static async Task<JsonObject> Body(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonObject>())!;

    public static async Task<JsonObject> CreateOrder(this HttpClient client, JsonObject? body = null)
    {
        var response = await client.PostJson("/api/production-orders", body ?? ValidCreate());
        response.EnsureSuccessStatusCode();
        return await response.Body();
    }

    public static string[] ErrorsFor(this JsonObject problem, string field) =>
        problem["errors"]![field]!.AsArray().Select(e => e!.GetValue<string>()).ToArray();

    private static StringContent Json(JsonNode body) =>
        new(body.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json");
}
