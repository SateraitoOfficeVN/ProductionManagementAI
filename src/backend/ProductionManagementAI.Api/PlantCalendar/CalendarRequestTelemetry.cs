using System.Diagnostics;
using ProductionManagementAI.Application.PlantCalendar;

namespace ProductionManagementAI.Api.PlantCalendar;

/// <summary>Owns one bounded final measurement for each calendar request, including rejected requests.</summary>
public sealed class CalendarRequestTelemetry(RequestDelegate next)
{
    /// <summary>Measures feature requests without retaining identifiers, query values or request bodies.</summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <returns>The asynchronous middleware operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/plant-calendar")) { await next(context); return; }
        context.Response.Headers.CacheControl = "no-store";
        var originalCancellation = context.RequestAborted;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(originalCancellation);
        deadline.CancelAfter(TimeSpan.FromSeconds(20));
        context.RequestAborted = deadline.Token;
        var started = Stopwatch.GetTimestamp();
        var completed = false;
        try { await next(context); completed = true; }
        finally
        {
            context.RequestAborted = originalCancellation;
            Activity.Current?.SetTag("url.query", null);
            Activity.Current?.SetTag("http.url", null);
            var outcome = context.Items["plant-calendar-outcome"] as string ??
                (completed ? context.Response.StatusCode switch
                {
                    401 => "unauthorized", 403 => "forbidden", 400 or 413 or 415 => "validation",
                    404 => "not_found", >= 200 and < 300 => "success", _ => "unexpected"
                } : context.Request.Method is "PUT" or "POST" ? "unknown" : "unexpected");
            if (Operation(context.Request) is { } operation)
                CalendarTelemetry.Record(operation, outcome, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
    }

    private static string? Operation(HttpRequest request)
    {
        var parts = request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (parts.Length < 3) return null;
        return (request.Method, parts[2], parts.Length) switch
        {
            ("GET", "month", 3) => "Month", ("GET", "day", 3) => "Day",
            ("GET", "weekly", 3) => "Weekly", ("PUT", "weekly", 4) => "SaveWeekly",
            ("POST", "weekly", 5) when parts[4] == "withdraw" => "WithdrawWeekly",
            ("PUT", "exceptions", 3) => "SaveException",
            ("POST", "exceptions", 4) when parts[3] == "remove" => "RemoveException",
            ("GET", "line-choices", 3) => "LineChoices", ("GET", "product-choices", 3) => "ProductChoices",
            ("GET", "capacity", 3) => "Capacity", ("GET", "exception-history", 3) => "ExceptionHistory", _ => null
        };
    }
}
