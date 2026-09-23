using Microsoft.EntityFrameworkCore;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.ProductionOrders;

/// <summary>
/// Filter and sort composition for the list query (002_DD-FN §4–§5). Shared by the count and the page query, so the
/// two can never drift apart.
/// </summary>
internal static class ProductionOrderQueryExtensions
{
    /// <summary>
    /// Composes only the filters the query actually carries. An unset filter contributes no SQL at all — a
    /// <c>(@p IS NULL OR col = @p)</c> predicate would defeat the indexes 002_DB relies on.
    /// </summary>
    public static IQueryable<ProductionOrder> ApplyFilters(
        this IQueryable<ProductionOrder> source, ProductionOrderListQuery query)
    {
        if (query.Statuses.Count > 0)
        {
            var statuses = query.Statuses;
            source = source.Where(o => statuses.Contains(o.Status));
        }

        if (query.ProductId is { } productId)
        {
            source = source.Where(o => o.ProductId == productId);
        }

        if (query.DueFrom is { } dueFrom)
        {
            source = source.Where(o => o.DueDate >= dueFrom);
        }

        if (query.DueTo is { } dueTo)
        {
            source = source.Where(o => o.DueDate <= dueTo);
        }

        if (query.OrderNumberPattern is { } pattern)
        {
            // Backed by the pg_trgm index (DEC-010); the pattern is already escaped and upper-cased.
            source = source.Where(o => EF.Functions.Like(o.OrderNumber, pattern, "\\"));
        }

        return source;
    }

    /// <summary>
    /// Applies the requested ordering and always ends on the unique order number, so paging is a stable total order
    /// (REQ-023). The direction applies to the chosen key only; the tie-breaker stays ascending.
    ///
    /// Ordering happens on the joined entities, before the final projection: EF Core cannot translate an ORDER BY
    /// that reads a member back out of a constructor projection, so projecting first and sorting after it fails.
    /// </summary>
    public static IOrderedQueryable<ProductionOrderJoin> ApplySort(
        this IQueryable<ProductionOrderJoin> source, ProductionOrderSort sort, SortDirection direction)
    {
        var descending = direction == SortDirection.Desc;

        return sort switch
        {
            ProductionOrderSort.OrderNumber => descending
                ? source.OrderByDescending(j => j.Order.OrderNumber)
                : source.OrderBy(j => j.Order.OrderNumber),
            ProductionOrderSort.Product => descending
                ? source.OrderByDescending(j => j.Product.Sku).ThenByDescending(j => j.Product.Name)
                    .ThenBy(j => j.Order.OrderNumber)
                : source.OrderBy(j => j.Product.Sku).ThenBy(j => j.Product.Name)
                    .ThenBy(j => j.Order.OrderNumber),
            ProductionOrderSort.Quantity => descending
                ? source.OrderByDescending(j => j.Order.Quantity).ThenBy(j => j.Order.OrderNumber)
                : source.OrderBy(j => j.Order.Quantity).ThenBy(j => j.Order.OrderNumber),
            ProductionOrderSort.Status => descending
                ? source.OrderByDescending(StatusRank).ThenBy(j => j.Order.OrderNumber)
                : source.OrderBy(StatusRank).ThenBy(j => j.Order.OrderNumber),
            ProductionOrderSort.UpdatedAt => descending
                ? source.OrderByDescending(j => j.Order.UpdatedAtUtc).ThenBy(j => j.Order.OrderNumber)
                : source.OrderBy(j => j.Order.UpdatedAtUtc).ThenBy(j => j.Order.OrderNumber),
            _ => descending
                ? source.OrderByDescending(j => j.Order.DueDate).ThenBy(j => j.Order.OrderNumber)
                : source.OrderBy(j => j.Order.DueDate).ThenBy(j => j.Order.OrderNumber),
        };
    }

    /// <summary>
    /// Workflow order, not alphabetical (002_DB sort-key mapping): alphabetically the statuses would read
    /// "Cancelled, Completed, Draft, In progress", which means nothing to a planner. Translated as a SQL CASE.
    /// </summary>
    private static System.Linq.Expressions.Expression<Func<ProductionOrderJoin, int>> StatusRank => join =>
        join.Order.Status == ProductionOrderStatus.Draft ? 1
        : join.Order.Status == ProductionOrderStatus.InProgress ? 2
        : join.Order.Status == ProductionOrderStatus.Completed ? 3
        : 4;
}

/// <summary>
/// The joined order and product, before the list projection. A named type with settable members rather than a
/// positional record, so EF Core can see through it when it translates ORDER BY.
/// </summary>
internal sealed class ProductionOrderJoin
{
    public required ProductionOrder Order { get; init; }

    public required Product Product { get; init; }
}
