using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Dashboard;

/// <summary>
/// Shapes the reader's raw rows into the API response (DD-003-FN §4): every "missing means zero" rule and the one
/// rounding rule the server owns. Pure, so it is unit-tested without a database.
/// </summary>
public static class DashboardMapper
{
    public static DashboardResponse ToResponse(DashboardWindow window, DashboardRaw raw, DateTimeOffset asOf)
    {
        int CountOf(ProductionOrderStatus status) =>
            raw.StatusCounts.FirstOrDefault(r => r.Status == status.ToString())?.Count ?? 0;

        var draft = CountOf(ProductionOrderStatus.Draft);
        var inProgress = CountOf(ProductionOrderStatus.InProgress);
        var completed = CountOf(ProductionOrderStatus.Completed);
        var cancelled = CountOf(ProductionOrderStatus.Cancelled);

        var delivery = raw.Delivery;
        var averageDays = delivery.WindowCount == 0 || delivery.WindowLeadDays is null
            ? (double?)null
            : Math.Round(delivery.WindowLeadDays.Value, 1, MidpointRounding.AwayFromZero);

        return new DashboardResponse(
            asOf,
            window.Today,
            window.TimeZoneId,
            new StatusCounts(draft + inProgress + completed + cancelled, draft, inProgress, completed, cancelled),
            ToGroup(raw.Overdue),
            ToGroup(raw.DueSoon),
            ToWorkload(window, raw.Workload),
            raw.TopProducts
                .Select(r => new TopProduct(new ProductSummary(r.ProductId, r.Sku, r.Name), r.OpenQuantity, r.ActiveOrders))
                .ToArray(),
            new CompletedInPeriod(delivery.WeekCount, delivery.WeekQuantity, window.Week0),
            new CompletedInPeriod(delivery.MonthCount, delivery.MonthQuantity, window.MonthStart),
            new OnTimeFigure(delivery.WindowOnTime, delivery.WindowCount, window.WindowStart),
            new LeadTimeFigure(averageDays, delivery.WindowCount, window.WindowStart),
            window.TrendWeeks
                .Select(week => new TrendWeek(
                    week,
                    week.AddDays(6),
                    raw.Trend.FirstOrDefault(r => r.WeekStart == week)?.Count ?? 0))
                .ToArray());
    }

    private static OrderGroup ToGroup(IReadOnlyList<DashboardOrderRow> rows) =>
        new(
            rows.Count == 0 ? 0 : rows[0].Total,
            rows.Select(r => new DashboardOrder(
                    r.Id,
                    r.OrderNumber,
                    new ProductSummary(r.ProductId, r.Sku, r.Name),
                    r.Quantity,
                    r.DueDate,
                    Enum.Parse<ProductionOrderStatus>(r.Status)))
                .ToArray());

    /// <summary>Exactly ten buckets in display order: overdue, eight weeks, later (BD-003 D-03, DEC-009).</summary>
    private static WorkloadBucket[] ToWorkload(DashboardWindow window, IReadOnlyList<WorkloadRow> rows)
    {
        (int Count, long Quantity) Of(int bucket)
        {
            var row = rows.FirstOrDefault(r => r.Bucket == bucket);
            return row is null ? (0, 0) : (row.Count, row.Quantity);
        }

        var buckets = new List<WorkloadBucket>(DashboardWindow.LookAheadWeeks + 2);
        var overdue = Of(-1);
        buckets.Add(new WorkloadBucket("overdue", null, null, overdue.Count, overdue.Quantity));
        for (var k = 0; k < DashboardWindow.LookAheadWeeks; k++)
        {
            var monday = window.Week0.AddDays(7 * k);
            var week = Of(k);

            // The current week starts today: its earlier days are already overdue (D-03).
            buckets.Add(new WorkloadBucket("week", k == 0 ? window.Today : monday, monday.AddDays(6), week.Count, week.Quantity));
        }

        var later = Of(DashboardWindow.LookAheadWeeks);
        buckets.Add(new WorkloadBucket("later", null, null, later.Count, later.Quantity));
        return buckets.ToArray();
    }
}
