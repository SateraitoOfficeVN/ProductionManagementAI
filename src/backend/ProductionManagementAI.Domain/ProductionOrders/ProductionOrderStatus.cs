namespace ProductionManagementAI.Domain.ProductionOrders;

/// <summary>Production order lifecycle (REQ-017, DEC-003). Persisted by name (DEC-015).</summary>
public enum ProductionOrderStatus
{
    Draft,
    InProgress,
    Completed,
    Cancelled,
}

public static class ProductionOrderStatusExtensions
{
    private static readonly IReadOnlyList<ProductionOrderStatus> None = [];

    // Single source of the transition table (BD-001 M-02 / V-06); the API returns it as allowedNextStatuses
    // so the frontend never duplicates it.
    private static readonly Dictionary<ProductionOrderStatus, IReadOnlyList<ProductionOrderStatus>> Next = new()
    {
        [ProductionOrderStatus.Draft] = [ProductionOrderStatus.InProgress, ProductionOrderStatus.Cancelled],
        [ProductionOrderStatus.InProgress] = [ProductionOrderStatus.Completed, ProductionOrderStatus.Cancelled],
        [ProductionOrderStatus.Completed] = None,
        [ProductionOrderStatus.Cancelled] = None,
    };

    public static IReadOnlyList<ProductionOrderStatus> AllowedNext(this ProductionOrderStatus status) =>
        Next.TryGetValue(status, out var next) ? next : None;

    public static bool CanTransitionTo(this ProductionOrderStatus from, ProductionOrderStatus to) =>
        from == to || from.AllowedNext().Contains(to);
}
