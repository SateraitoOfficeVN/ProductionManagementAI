using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Application.ProductionLines;

namespace ProductionManagementAI.Api.ProductionLines;

internal static class LineProblems
{
    internal static ActionResult Render<T>(ControllerBase controller, LineResult<T> result, Func<T, ActionResult> success)
    {
        controller.HttpContext.Items["production-lines-counted"] = true;
        return result.Problem is { } problem ? Problem(controller.HttpContext, problem)
            : result.Value is { } value ? success(value) : Problem(controller.HttpContext, new(500, "UNEXPECTED"));
    }
    internal static ObjectResult Problem(HttpContext http, LineProblem failure)
    {
        if (failure.Status == 503) http.Response.Headers.RetryAfter = "1";
        var suffix = failure.Code switch {
            "LINE_PRODUCT_INVALID" => "product-invalid", "LINE_PAIR_INVALID" => "pair-invalid",
            "LINE_ALREADY_RETIRED" => "already-retired", "LINE_CODE_CONFLICT" => "code-conflict",
            "LINE_STALE" => "stale", "LINE_UNIT_STALE" => "unit-stale",
            "LINE_UNIT_CONFIRMATION_REQUIRED" => "unit-confirmation", "NOT_FOUND" => "not-found",
            "REQUEST_TOO_LARGE" => "request-too-large", "UNSUPPORTED_MEDIA_TYPE" => "unsupported-media-type",
            "LINE_BUSY" => "busy", "UNEXPECTED" => "internal", _ => "validation" };
        var problem = new ProblemDetails { Type = $"urn:pmai:problem:production-lines:{suffix}", Status = failure.Status,
            Title = failure.Status switch { 400 => "One or more fields are invalid.", 404 => "Requested target not found.",
                409 => "The requested change conflicts with current data.", 413 => "Request body is too large.",
                415 => "JSON content is required.", 503 => "The operation is temporarily busy.", _ => "Something went wrong." } };
        problem.Extensions["code"] = failure.Code;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? http.TraceIdentifier;
        if (failure.Errors is not null) problem.Extensions["errors"] = failure.Errors;
        return new ObjectResult(problem) { StatusCode = failure.Status, ContentTypes = { "application/problem+json" } };
    }
}
