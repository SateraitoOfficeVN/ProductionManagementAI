using Microsoft.EntityFrameworkCore;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.ProductionOrders;

/// <summary>
/// Filter and sort composition for the list query (DD-002-FN §4–§5). Shared by the count and the page query, so the
/// two can never drift apart.
/// </summary>
internal static class ProductionOrderQueryExtensions
{
    /// <summary>
    /// Composes only the filters the query actually carries. An unset filter contributes no SQL at all — a
    /// <c>(@p IS NULL OR col = @p)</c> predicate would defeat the indexes DB-003 relies on.
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
    /// </summary>
    public static IOrderedQueryable<ProductionOrderListRow> ApplySort(
        this IQueryable<ProductionOrderListRow> source, ProductionOrderSort sort, SortDirection direction)
    {
        var descending = direction == SortDirection.Desc;

        return sort switch
        {
            ProductionOrderSort.OrderNumber => descending
                ? source.OrderByDescending(r => r.OrderNumber)
                : source.OrderBy(r => r.OrderNumber),
            ProductionOrderSort.Product => descending
                ? source.OrderByDescending(r => r.ProductSku).ThenByDescending(r => r.ProductName)
                    .ThenBy(r => r.OrderNumber)
                : source.OrderBy(r => r.ProductSku).ThenBy(r => r.ProductName).ThenBy(r => r.OrderNumber),
            ProductionOrderSort.Quantity => descending
                ? source.OrderByDescending(r => r.Quantity).ThenBy(r => r.OrderNumber)
                : source.OrderBy(r => r.Quantity).ThenBy(r => r.OrderNumber),
            ProductionOrderSort.Status => descending
                ? source.OrderByDescending(StatusRank).ThenBy(r => r.OrderNumber)
                : source.OrderBy(StatusRank).ThenBy(r => r.OrderNumber),
            ProductionOrderSort.UpdatedAt => descending
                ? source.OrderByDescending(r => r.UpdatedAt).ThenBy(r => r.OrderNumber)
                : source.OrderBy(r => r.UpdatedAt).ThenBy(r => r.OrderNumber),
            _ => descending
                ? source.OrderByDescending(r => r.DueDate).ThenBy(r => r.OrderNumber)
                : source.OrderBy(r => r.DueDate).ThenBy(r => r.OrderNumber),
        };
    }

    /// <summary>
    /// Workflow order, not alphabetical (DB-003 sort-key mapping): alphabetically the statuses would read
    /// "Cancelled, Completed, Draft, In progress", which means nothing to a planner. Translated as a SQL CASE.
    /// </summary>
    private static System.Linq.Expressions.Expression<Func<ProductionOrderListRow, int>> StatusRank => row =>
        row.Status == ProductionOrderStatus.Draft ? 1
        : row.Status == ProductionOrderStatus.InProgress ? 2
        : row.Status == ProductionOrderStatus.Completed ? 3
        : 4;
}
