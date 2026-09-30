using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Dashboard;

// Response contract of GET /api/dashboard (003_DD-API §1). Every figure comes from one snapshot (WI-004 DEC-011).

public sealed record DashboardResponse(
    DateTimeOffset AsOf,
    DateOnly Today,
    string TimeZone,
    StatusCounts StatusCounts,
    OrderGroup Overdue,
    OrderGroup DueSoon,
    IReadOnlyList<WorkloadBucket> Workload,
    IReadOnlyList<TopProduct> TopProducts,
    CompletedInPeriod CompletedThisWeek,
    CompletedInPeriod CompletedThisMonth,
    OnTimeFigure OnTime,
    LeadTimeFigure LeadTime,
    IReadOnlyList<TrendWeek> CompletionTrend);

public sealed record StatusCounts(int Total, int Draft, int InProgress, int Completed, int Cancelled);

/// <param name="Total">Every order in the group, not only the at most 10 returned (003_BD D-01, D-02).</param>
public sealed record OrderGroup(int Total, IReadOnlyList<DashboardOrder> Orders);

public sealed record DashboardOrder(
    Guid Id,
    string OrderNumber,
    ProductSummary Product,
    decimal Quantity,
    DateOnly DueDate,
    ProductionOrderStatus Status);

/// <param name="Kind"><c>overdue</c>, <c>week</c> or <c>later</c> (003_BD D-03, DEC-009).</param>
/// <param name="WeekStart">First date the bucket covers — <c>today</c> for the current week; null for overdue/later.</param>
/// <summary>Holds a quantity subtotal for exactly one product unit.</summary>
public sealed record UnitQuantity(string Unit, decimal Quantity);

/// <summary>Holds an order-count bucket and separate quantity subtotals by unit.</summary>
public sealed record WorkloadBucket(
    string Kind, DateOnly? WeekStart, DateOnly? WeekEnd, int OrderCount,
    IReadOnlyList<UnitQuantity> UnitQuantities);

public sealed record TopProduct(ProductSummary Product, decimal OpenQuantity, int ActiveOrderCount);

public sealed record CompletedInPeriod(int OrderCount, DateOnly From);

/// <remarks>The rate itself is derived by the client (003_BD M-13), so one rounding rule lives in one place.</remarks>
public sealed record OnTimeFigure(int OnTimeCount, int CompletedCount, DateOnly WindowStart);

/// <param name="AverageDays">Mean lead time rounded half away from zero to one decimal; null when nothing completed.</param>
public sealed record LeadTimeFigure(double? AverageDays, int OrderCount, DateOnly WindowStart);

public sealed record TrendWeek(DateOnly WeekStart, DateOnly WeekEnd, int OrderCount);

// Raw reader output (003_DD-FN "Response / value mapping"), shaped by DashboardMapper.

public sealed record DashboardRaw(
    IReadOnlyList<StatusCountRow> StatusCounts,
    IReadOnlyList<WorkloadRow> Workload,
    IReadOnlyList<DashboardOrderRow> Overdue,
    IReadOnlyList<DashboardOrderRow> DueSoon,
    IReadOnlyList<TopProductRow> TopProducts,
    DeliveryRow Delivery,
    IReadOnlyList<TrendRow> Trend);

public sealed record StatusCountRow(string Status, int Count);

/// <param name="Bucket">−1 overdue, 0–7 week k, 8 later (003_DB Q2).</param>
public sealed record WorkloadRow(int Bucket, string Unit, int Count, decimal Quantity);

/// <param name="Total">The group's window count, repeated on every row (003_DB Q3).</param>
public sealed record DashboardOrderRow(
    Guid Id,
    string OrderNumber,
    decimal Quantity,
    DateOnly DueDate,
    string Status,
    Guid ProductId,
    string Sku,
    string Name,
    string Unit,
    int Total);

public sealed record TopProductRow(Guid ProductId, string Sku, string Name, string Unit, decimal OpenQuantity, int ActiveOrders);

public sealed record DeliveryRow(
    int WeekCount,
    int MonthCount,
    int WindowCount,
    int WindowOnTime,
    double? WindowLeadDays);

public sealed record TrendRow(DateOnly WeekStart, int Count);
