using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.ProductionOrders;

// Request/response shapes for 001_DD-API. Request fields are nullable so a missing field is reported as
// "required" with its message ID instead of failing model binding.

public sealed record CreateProductionOrderRequest(Guid? ProductId, int? Quantity, DateOnly? DueDate, string? Notes);

public sealed record UpdateProductionOrderRequest(
    Guid? ProductId,
    int? Quantity,
    DateOnly? DueDate,
    ProductionOrderStatus? Status,
    string? Notes,
    uint? Version);

public sealed record ProductResponse(Guid Id, string Sku, string Name);

public sealed record ProductionOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid ProductId,
    int Quantity,
    DateOnly DueDate,
    ProductionOrderStatus Status,
    IReadOnlyList<ProductionOrderStatus> AllowedNextStatuses,
    bool IsProductQuantityEditable,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    uint Version);

public static class ProductionOrderMapper
{
    public static ProductionOrderResponse ToResponse(ProductionOrder order) => new(
        order.Id,
        order.OrderNumber,
        order.ProductId,
        order.Quantity,
        order.DueDate,
        order.Status,
        order.Status.AllowedNext(),
        order.IsProductQuantityEditable,
        order.Notes,
        order.CreatedAtUtc,
        order.UpdatedAtUtc,
        order.RowVersion);
}
