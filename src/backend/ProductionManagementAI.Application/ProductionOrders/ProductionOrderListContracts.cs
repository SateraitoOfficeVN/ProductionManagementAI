using ProductionManagementAI.Domain.ProductionOrders;
using Msg = ProductionManagementAI.Domain.ProductionOrders.ProductionOrderMessages;

namespace ProductionManagementAI.Application.ProductionOrders;

// Request/response shapes and the query validator for DD-002-API (GET /api/production-orders).

/// <summary>Sort keys allowed by DD-002-API. An enum, not a string, so no client value reaches the ordering.</summary>
public enum ProductionOrderSort
{
    DueDate,
    OrderNumber,
    Product,
    Quantity,
    Status,
    UpdatedAt,
}

public enum SortDirection
{
    Asc,
    Desc,
}

/// <summary>
/// Raw query string, bound as strings only: every value is parsed by <see cref="ProductionOrderListQuery.TryCreate"/>
/// so each failure carries its designed message ID instead of a generic model-binding error (DD-002-API request fields).
/// </summary>
public sealed record ProductionOrderListRequest(
    string[]? Status,
    string? ProductId,
    string? DueFrom,
    string? DueTo,
    string? OrderNumber,
    string? Sort,
    string? Dir,
    string? Page,
    string? PageSize);

/// <summary>A validated, normalized list query (DD-002 module 5). Constructing one is the only way to reach the repository.</summary>
public sealed record ProductionOrderListQuery(
    IReadOnlyList<ProductionOrderStatus> Statuses,
    Guid? ProductId,
    DateOnly? DueFrom,
    DateOnly? DueTo,
    string? OrderNumberPattern,
    ProductionOrderSort Sort,
    SortDirection Direction,
    int Page,
    int PageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxOrderNumberFilterLength = 20;
    public const int MaxPage = 100_000;

    /// <summary>Page sizes the API accepts (DEC-002). Anything else is rejected, never rounded to the nearest.</summary>
    public static readonly int[] AllowedPageSizes = [10, 20, 50, 100];

    public int Skip => (Page - 1) * PageSize;

    public bool HasFilter =>
        Statuses.Count > 0 || ProductId is not null || DueFrom is not null || DueTo is not null
        || OrderNumberPattern is not null;

    /// <summary>
    /// Validates and normalizes a raw query. Absent values take their defaults; a value that is present but invalid is
    /// rejected rather than defaulted (BD-002 V-09–V-13), so a crafted request cannot widen the result set. Every
    /// offending parameter is reported together.
    /// </summary>
    public static Result<ProductionOrderListQuery> TryCreate(ProductionOrderListRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        var statuses = ParseStatuses(request.Status, errors);
        var productId = ParseProductId(request.ProductId, errors);
        var dueFrom = ParseDate(request.DueFrom, "dueFrom", errors);
        var dueTo = ParseDate(request.DueTo, "dueTo", errors);
        if (dueFrom is { } from && dueTo is { } to && from > to)
        {
            errors["dueFrom"] = [Msg.DueDateRangeInverted];
        }

        var pattern = ParseOrderNumberPattern(request.OrderNumber, errors);
        var sort = ParseEnum<ProductionOrderSort>(request.Sort, "sort", ProductionOrderSort.DueDate, errors);
        var direction = ParseEnum<SortDirection>(request.Dir, "dir", SortDirection.Asc, errors);
        var page = ParseBoundedInt(request.Page, "page", 1, 1, MaxPage, errors);
        var pageSize = ParsePageSize(request.PageSize, errors);

        return errors.Count > 0
            ? new Result<ProductionOrderListQuery>.Invalid(errors)
            : new Result<ProductionOrderListQuery>.Ok(new ProductionOrderListQuery(
                statuses, productId, dueFrom, dueTo, pattern, sort, direction, page, pageSize));
    }

    private static IReadOnlyList<ProductionOrderStatus> ParseStatuses(
        string[]? values, Dictionary<string, string[]> errors)
    {
        if (values is null || values.Length == 0)
        {
            return [];
        }

        var parsed = new List<ProductionOrderStatus>(values.Length);
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!TryParseName<ProductionOrderStatus>(value, out var status))
            {
                errors["status"] = [Msg.StatusFilterUnknown];
                return [];
            }

            if (!parsed.Contains(status))
            {
                parsed.Add(status);
            }
        }

        return parsed;
    }

    private static Guid? ParseProductId(string? value, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Guid.TryParse(value, out var productId))
        {
            // Same message as an unknown product: from the user's side both mean "that product isn't there".
            errors["productId"] = [Msg.ProductNotFound];
            return null;
        }

        return productId;
    }

    private static DateOnly? ParseDate(string? value, string field, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", out var date))
        {
            errors[field] = [Msg.DateFilterInvalid];
            return null;
        }

        return date;
    }

    /// <summary>
    /// Trim → length check → upper-case → escape → wrap (DEC-010). <c>order_number</c> is generated and always
    /// upper-case, so upper-casing the input makes the match case-insensitive while keeping the trigram index usable.
    /// </summary>
    private static string? ParseOrderNumberPattern(string? value, Dictionary<string, string[]> errors)
    {
        var fragment = value?.Trim();
        if (string.IsNullOrEmpty(fragment))
        {
            return null;
        }

        if (fragment.Length > MaxOrderNumberFilterLength)
        {
            errors["orderNumber"] = [Msg.OrderNumberFilterTooLong];
            return null;
        }

        var escaped = fragment
            .ToUpperInvariant()
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

        return $"%{escaped}%";
    }

    private static TEnum ParseEnum<TEnum>(
        string? value, string field, TEnum fallback, Dictionary<string, string[]> errors)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (!TryParseName<TEnum>(value, out var parsed))
        {
            errors[field] = [Msg.SortOrPagingUnsupported];
            return fallback;
        }

        return parsed;
    }

    /// <summary>
    /// Matches an enum by name only. Enum.TryParse would also accept the numeric value ("3" for Cancelled), which
    /// would let a crafted request slip past an allow-list that is meant to be a list of names.
    /// </summary>
    private static bool TryParseName<TEnum>(string value, out TEnum parsed)
        where TEnum : struct, Enum
    {
        foreach (var name in Enum.GetNames<TEnum>())
        {
            if (string.Equals(name, value, StringComparison.OrdinalIgnoreCase))
            {
                parsed = Enum.Parse<TEnum>(name);
                return true;
            }
        }

        parsed = default;
        return false;
    }

    private static int ParseBoundedInt(
        string? value, string field, int fallback, int min, int max, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (!int.TryParse(value, out var parsed) || parsed < min || parsed > max)
        {
            errors[field] = [Msg.SortOrPagingUnsupported];
            return fallback;
        }

        return parsed;
    }

    private static int ParsePageSize(string? value, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DefaultPageSize;
        }

        if (!int.TryParse(value, out var parsed) || !AllowedPageSizes.Contains(parsed))
        {
            errors["pageSize"] = [Msg.SortOrPagingUnsupported];
            return DefaultPageSize;
        }

        return parsed;
    }
}

/// <summary>One projected row of the list query (DB-003 read projection). Never carries notes or the row version.</summary>
public sealed record ProductionOrderListRow(
    Guid Id,
    string OrderNumber,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    int Quantity,
    DateOnly DueDate,
    ProductionOrderStatus Status,
    DateTimeOffset UpdatedAt);

public sealed record ProductSummary(Guid Id, string Sku, string Name);

public sealed record ProductionOrderListItem(
    Guid Id,
    string OrderNumber,
    ProductSummary Product,
    int Quantity,
    DateOnly DueDate,
    ProductionOrderStatus Status,
    bool IsOverdue,
    DateTimeOffset UpdatedAt);

/// <param name="Total">Exact number of matches, independent of the page (REQ-024).</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize,
    string Sort,
    string Dir);

public static class ProductionOrderListMapper
{
    /// <summary>
    /// Row → list item. <paramref name="plantToday"/> is read once per request, so every row on a page judges
    /// "today" identically, and the function stays pure and unit-testable (BD-002 M-08, FN-013).
    /// </summary>
    public static ProductionOrderListItem ToListItem(ProductionOrderListRow row, DateOnly plantToday) => new(
        row.Id,
        row.OrderNumber,
        new ProductSummary(row.ProductId, row.ProductSku, row.ProductName),
        row.Quantity,
        row.DueDate,
        row.Status,
        row.DueDate < plantToday
            && row.Status is ProductionOrderStatus.Draft or ProductionOrderStatus.InProgress,
        row.UpdatedAt);

    /// <summary>camelCase API spelling of a sort key, echoed in the response so the client renders what it got.</summary>
    public static string ToApiValue(this ProductionOrderSort sort) => sort switch
    {
        ProductionOrderSort.DueDate => "dueDate",
        ProductionOrderSort.OrderNumber => "orderNumber",
        ProductionOrderSort.Product => "product",
        ProductionOrderSort.Quantity => "quantity",
        ProductionOrderSort.Status => "status",
        ProductionOrderSort.UpdatedAt => "updatedAt",
        _ => "dueDate",
    };

    public static string ToApiValue(this SortDirection direction) =>
        direction == SortDirection.Desc ? "desc" : "asc";
}
