using System.Globalization;
using System.Text;

namespace ProductionManagementAI.Application.ProductionLines;

/// <summary>Normalizes feature inputs while retaining stable submitted row paths.</summary>
public static class LineValidation
{
    /// <summary>Validates literal search, state and ASCII bounded paging.</summary>
    public static LineResult<LineQuery> Query(string? q, string? page, string? state = null, Guid? targetId = null)
    {
        var search = q?.Trim();
        if (search is not null && search.EnumerateRunes().Count() > 100)
            return LineResult<LineQuery>.Failure(400, "VALIDATION", "q");
        var rawPage = page ?? "1";
        if (rawPage.Length == 0 || rawPage.Any(c => c is < '0' or > '9') ||
            !int.TryParse(rawPage, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number is < 1 or > 10000)
            return LineResult<LineQuery>.Failure(400, "VALIDATION", "page");
        var filter = state ?? "active";
        if (filter is not ("active" or "retired" or "all"))
            return LineResult<LineQuery>.Failure(400, "VALIDATION", "state");
        return LineResult<LineQuery>.Success(new(string.IsNullOrEmpty(search) ? null : search, number, filter, targetId));
    }

    /// <summary>Validates a creation command and explicit unit confirmations.</summary>
    public static LineResult<LineCommand> Create(CreateLineRequest request) =>
        Command(request.Code, request.Name, request.WorkingHoursPerDay, "0", request.Products, true);

    /// <summary>Validates an aggregate edit without altering omitted pairs.</summary>
    public static LineResult<LineCommand> Update(UpdateLineRequest request) =>
        Command(string.Empty, request.Name, request.WorkingHoursPerDay, request.Version, request.ProductChanges, false);

    private static LineResult<LineCommand> Command(string? code, string? name, string? hours, string? version,
        IReadOnlyList<LineProductInput>? inputs, bool create)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var normalizedCode = string.Empty;
        if (create && !LineValueRules.TryText(code, 50, out normalizedCode)) errors["code"] = ["VALIDATION"];
        if (!LineValueRules.TryText(name, 200, out var normalizedName)) errors["name"] = ["VALIDATION"];
        if (!LineValueRules.TryDecimal(hours, 24, out var workingHours)) errors["workingHoursPerDay"] = ["VALIDATION"];
        if (!LineValueRules.TryVersion(version, out var expectedVersion)) errors["version"] = ["VALIDATION"];
        var arrayField = create ? "products" : "productChanges";
        var changes = new List<LineProductChange>();
        var ids = new HashSet<Guid>();
        if (inputs is null || inputs.Count > 1000) errors[arrayField] = ["VALIDATION"];
        else for (var i = 0; i < inputs.Count; i++)
        {
            var row = inputs[i];
            var field = $"{arrayField}[{i}]";
            if (row.ProductId == Guid.Empty || !ids.Add(row.ProductId)) errors[$"{field}.productId"] = ["VALIDATION"];
            if (row.Action is not ("add" or "setTiming" or "retire") || (create && row.Action != "add"))
                errors[$"{field}.action"] = ["VALIDATION"];
            decimal minutes = 0;
            long revision = 0;
            if (row.Action == "retire")
            {
                if (row.MinutesPerUnit is not null || row.ExpectedUnit is not null || row.ExpectedUnitRevision is not null || row.ConfirmUnit is not null)
                    errors[$"{field}.action"] = ["VALIDATION"];
            }
            else
            {
                if (!LineValueRules.TryDecimal(row.MinutesPerUnit, 999999999.999m, out minutes)) errors[$"{field}.minutesPerUnit"] = ["VALIDATION"];
                if (row.ExpectedUnit is null || !LineValueRules.Units.Contains(row.ExpectedUnit)) errors[$"{field}.expectedUnit"] = ["VALIDATION"];
                if (!LineValueRules.TryRevision(row.ExpectedUnitRevision, out revision)) errors[$"{field}.expectedUnitRevision"] = ["VALIDATION"];
                if (row.ConfirmUnit is null || (row.Action == "add" && row.ConfirmUnit != true)) errors[$"{field}.confirmUnit"] = ["VALIDATION"];
            }
            changes.Add(new(row.Action, row.ProductId, minutes, row.ExpectedUnit ?? string.Empty, revision, row.ConfirmUnit == true, field));
        }
        return errors.Count > 0
            ? new(default, new(400, "VALIDATION", errors))
            : LineResult<LineCommand>.Success(new(normalizedCode, normalizedName, workingHours, expectedVersion, changes));
    }
}
