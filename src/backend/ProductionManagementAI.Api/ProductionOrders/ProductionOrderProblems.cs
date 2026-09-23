using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductionManagementAI.Application.ProductionOrders;
using Msg = ProductionManagementAI.Domain.ProductionOrders.ProductionOrderMessages;

namespace ProductionManagementAI.Api.ProductionOrders;

/// <summary>
/// RFC 9457 Problem Details for the production-order API (001_DD-API, DEC-023). Every body carries
/// <c>type</c>, <c>title</c>, <c>status</c>, <c>code</c> and <c>traceId</c>; 400 also carries <c>errors</c>
/// (field → message IDs). No exception text is ever included.
/// </summary>
public static class ProductionOrderProblems
{
    public const string ValidationCode = "VALIDATION";

    // 001_DD-API field → message ID used when JSON binding itself fails (e.g. "quantity": "abc").
    private static readonly Dictionary<string, string> BindingErrorMessage = new(StringComparer.OrdinalIgnoreCase)
    {
        ["productId"] = Msg.ProductRequired,
        ["quantity"] = Msg.QuantityInvalid,
        ["dueDate"] = Msg.DueDateRequired,
        ["status"] = Msg.StatusTransitionNotAllowed,
        ["notes"] = Msg.NotesTooLong,
        ["version"] = Msg.ConcurrencyConflict,
    };

    public static ActionResult ToActionResult<T>(
        ControllerBase controller, Result<T> result, Func<T, ActionResult> onOk) => result switch
    {
        Result<T>.Ok ok => onOk(ok.Value),
        Result<T>.Invalid invalid => Validation(controller.HttpContext, invalid.Errors),
        Result<T>.NotFound => Problem(controller.HttpContext, StatusCodes.Status404NotFound, "not-found",
            "Production order not found.", Msg.OrderNotFound),
        Result<T>.Conflict => Problem(controller.HttpContext, StatusCodes.Status409Conflict, "conflict",
            "The production order was changed by someone else.", Msg.ConcurrencyConflict),
        Result<T>.RuleViolation violation => Problem(controller.HttpContext, StatusCodes.Status422UnprocessableEntity,
            "rule-violation", "The change breaks a production order rule.", violation.Code),
        _ => throw new UnreachableException($"Unhandled result {result.GetType().Name}"),
    };

    /// <summary>Replaces MVC's default model-state response so binding errors use message IDs, never framework text.</summary>
    public static IActionResult FromModelState(ActionContext context)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var (key, entry) in context.ModelState)
        {
            if (entry.Errors.Count == 0)
            {
                continue;
            }

            var field = NormalizeKey(key);
            errors[field] = [BindingErrorMessage.GetValueOrDefault(field, Msg.Unexpected)];
        }

        return Validation(context.HttpContext, errors);
    }

    private static string NormalizeKey(string key)
    {
        // "$.quantity" / "request.Quantity" / "Quantity" → "quantity"; the whole body → "body".
        var name = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key;
        var dot = name.LastIndexOf('.');
        if (dot >= 0)
        {
            name = name[(dot + 1)..];
        }

        if (name is "" or "$" or "request")
        {
            return "body";
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static ObjectResult Validation(HttpContext http, IReadOnlyDictionary<string, string[]> errors)
    {
        var result = Problem(http, StatusCodes.Status400BadRequest, "validation",
            "One or more fields are invalid.", ValidationCode);
        ((ProblemDetails)result.Value!).Extensions["errors"] = errors;
        return result;
    }

    private static ObjectResult Problem(HttpContext http, int status, string type, string title, string code)
    {
        var problem = new ProblemDetails
        {
            Type = $"urn:pmai:problem:{type}",
            Title = title,
            Status = status,
            Extensions =
            {
                ["code"] = code,
                ["traceId"] = Activity.Current?.Id ?? http.TraceIdentifier,
            },
        };

        return new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
    }
}
