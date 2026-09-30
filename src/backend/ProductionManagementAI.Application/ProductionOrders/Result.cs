namespace ProductionManagementAI.Application.ProductionOrders;

/// <summary>
/// Outcome of a production-order use case (001_DD-FN). The API maps each variant to one HTTP status (DEC-023):
/// Ok → 200/201, Invalid → 400, NotFound → 404, Conflict → 409, RuleViolation → 422.
/// </summary>
public abstract record Result<T>
{
    private Result()
    {
    }

    public sealed record Ok(T Value) : Result<T>;

    /// <param name="Errors">Field name (camelCase, as in the API) → message IDs.</param>
    public sealed record Invalid(IReadOnlyDictionary<string, string[]> Errors) : Result<T>;

    /// <summary>Reports a field validation failure with a stable WI-006 problem code.</summary>
    public sealed record FieldProblem(string Code, IReadOnlyDictionary<string, string[]> Errors) : Result<T>;

    /// <summary>Reports a version or uniqueness conflict with a stable code and field.</summary>
    public sealed record FieldConflict(string Code, IReadOnlyDictionary<string, string[]> Errors) : Result<T>;

    public sealed record NotFound : Result<T>;

    public sealed record Conflict : Result<T>;

    /// <param name="Code">001_DD message ID, e.g. MSG-E007 or MSG-E008.</param>
    public sealed record RuleViolation(string Code) : Result<T>;
}
