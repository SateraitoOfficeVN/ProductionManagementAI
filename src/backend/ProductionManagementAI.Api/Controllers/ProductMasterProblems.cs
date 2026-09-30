using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Application.ProductionOrders;

namespace ProductionManagementAI.Api.Products;

/// <summary>Maps Product master outcomes to stable RFC 9457 responses.</summary>
public static class ProductMasterProblems
{
    /// <summary>Renders a product operation without exposing exception or submitted data.</summary>
    public static ActionResult ToActionResult<T>(
        ControllerBase controller, Result<T> result, Func<T, ActionResult> onOk) => result switch
    {
        Result<T>.Ok ok => onOk(ok.Value),
        Result<T>.Invalid invalid => Problem(controller.HttpContext, 400, "validation", "VALIDATION", invalid.Errors),
        Result<T>.FieldProblem field => Problem(controller.HttpContext, 400, "validation", field.Code, field.Errors),
        Result<T>.FieldConflict field => Problem(controller.HttpContext, 409, "conflict", field.Code, field.Errors),
        Result<T>.NotFound => Problem(controller.HttpContext, 404, "not-found", "NOT_FOUND"),
        _ => throw new UnreachableException($"Unhandled result {result.GetType().Name}"),
    };

    private static ObjectResult Problem(HttpContext http, int status, string type, string code,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var problem = new ProblemDetails
        {
            Type = $"urn:pmai:problem:{type}",
            Title = status switch
            {
                400 => "One or more fields are invalid.",
                404 => "Product not found.",
                _ => "The requested change conflicts with current data.",
            },
            Status = status,
        };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? http.TraceIdentifier;
        if (errors is not null) problem.Extensions["errors"] = errors;
        return new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
    }
}
