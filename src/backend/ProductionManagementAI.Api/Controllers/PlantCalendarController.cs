using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Api.PlantCalendar;
using ProductionManagementAI.Api.ProductionOrders;
using ProductionManagementAI.Application.PlantCalendar;

namespace ProductionManagementAI.Api.Controllers;

/// <summary>Exposes the eleven role-gated calendar operations without persistence logic.</summary>
[ApiController]
[Route("api/plant-calendar")]
[Authorize(Policy = AuthorizationPolicies.ProductionOrderEditor)]
public sealed class PlantCalendarController(PlantCalendarService service) : ControllerBase
{
    /// <summary>Reads one coherent month.</summary>
    [HttpGet("month")] public Task<IActionResult> Month(CancellationToken ct) => Get("Month", service.MonthAsync, ct);
    /// <summary>Reads one day and exact scope target.</summary>
    [HttpGet("day")] public Task<IActionResult> Day(CancellationToken ct) => Get("Day", service.DayAsync, ct);
    /// <summary>Reads a bounded current or retained weekly range.</summary>
    [HttpGet("weekly")] public Task<IActionResult> Weekly(CancellationToken ct) => Get("Weekly", service.WeeklyAsync, ct);
    /// <summary>Saves one weekly definition.</summary>
    [HttpPut("weekly/{effectiveFrom}")] public Task<IActionResult> SaveWeekly(string effectiveFrom, CancellationToken ct) => Write("SaveWeekly", effectiveFrom, service.SaveWeeklyAsync, ct);
    /// <summary>Withdraws one future current weekly definition.</summary>
    [HttpPost("weekly/{effectiveFrom}/withdraw")] public Task<IActionResult> WithdrawWeekly(string effectiveFrom, CancellationToken ct) => Write("WithdrawWeekly", effectiveFrom, service.WithdrawWeeklyAsync, ct);
    /// <summary>Saves one plant or line date exception.</summary>
    [HttpPut("exceptions")] public Task<IActionResult> SaveException(CancellationToken ct) => Write("SaveException", null, service.SaveExceptionAsync, ct);
    /// <summary>Removes one current exception while retaining history.</summary>
    [HttpPost("exceptions/remove")] public Task<IActionResult> RemoveException(CancellationToken ct) => Write("RemoveException", null, service.RemoveExceptionAsync, ct);
    /// <summary>Reads active line choices.</summary>
    [HttpGet("line-choices")] public Task<IActionResult> LineChoices(CancellationToken ct) => Get("LineChoices", service.LineChoicesAsync, ct);
    /// <summary>Reads current eligible product choices for a line.</summary>
    [HttpGet("product-choices")] public Task<IActionResult> ProductChoices(CancellationToken ct) => Get("ProductChoices", service.ProductChoicesAsync, ct);
    /// <summary>Reads current dated descriptive capacity.</summary>
    [HttpGet("capacity")] public Task<IActionResult> Capacity(CancellationToken ct) => Get("Capacity", service.CapacityAsync, ct);
    /// <summary>Reads retained date history as an observation, not commit proof.</summary>
    [HttpGet("exception-history")] public Task<IActionResult> ExceptionHistory(CancellationToken ct) => Get("ExceptionHistory", service.ExceptionHistoryAsync, ct);

    private async Task<IActionResult> Get<T>(string operation, Func<CalendarReadQuery, CancellationToken, Task<CalendarResult<T>>> call, CancellationToken ct) where T : class
    {
        try { return Result(await call(CalendarRequestReader.Query(Request, operation), ct)); }
        catch (CalendarRuleException e) { return Failure(e.Failure with { WriteOutcome = null }); }
    }
    private async Task<IActionResult> Write(string operation, string? effectiveFrom, Func<CalendarCommand, CancellationToken, Task<CalendarResult<MutationResult>>> call, CancellationToken ct)
    {
        try { return Result(await call(await CalendarRequestReader.Command(Request, operation, effectiveFrom, ct), ct)); }
        catch (CalendarRuleException e) { return Failure(e.Failure); }
        catch (OperationCanceledException) { return Failure(new("CALENDAR_BUSY", 503, "NotApplied")); }
    }
    private IActionResult Result<T>(CalendarResult<T> result) where T : class
    {
        if (result.Failure is { } failure) return Failure(failure);
        var outcome = result.Value is CapacityResult { Availability: "Unavailable" } ? "unavailable" : "success";
        HttpContext.Items["plant-calendar-outcome"] = outcome;
        return Ok(result.Value);
    }
    private IActionResult Failure(CalendarFailure failure)
    {
        HttpContext.Items["plant-calendar-outcome"] = failure.WriteOutcome == "Unknown" ? "unknown" : failure.Code switch {
            "VALIDATION" or "REQUEST_TOO_LARGE" or "UNSUPPORTED_MEDIA_TYPE" => "validation",
            "NOT_FOUND" => "not_found", "CALENDAR_BUSY" => "busy", "UNEXPECTED" => "unexpected", "CALENDAR_TIMEZONE_MISMATCH" => "unavailable", _ => "conflict" };
        var problem = new ProblemDetails { Type = "urn:pmai:problem:plant-calendar:" + failure.Code.ToLowerInvariant().Replace('_', '-'),
            Status = failure.Status, Title = "Calendar request could not be completed." };
        problem.Extensions["code"] = failure.Code; problem.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        if (failure.Errors is not null) problem.Extensions["errors"] = failure.Errors;
        if (failure.WriteOutcome is not null) problem.Extensions["writeOutcome"] = failure.WriteOutcome;
        if (failure.Code == "CALENDAR_BUSY" && failure.WriteOutcome == "NotApplied") Response.Headers.RetryAfter = "1";
        return new ObjectResult(problem) { StatusCode = failure.Status, ContentTypes = { "application/problem+json" } };
    }
}
