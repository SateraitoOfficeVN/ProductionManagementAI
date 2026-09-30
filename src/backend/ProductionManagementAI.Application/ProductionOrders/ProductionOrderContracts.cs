using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.ProductionOrders;

// Request/response shapes for 001_DD-API. Request fields are nullable so a missing field is reported as
// "required" with its message ID instead of failing model binding.

public sealed record CreateProductionOrderRequest(Guid? ProductId, decimal? Quantity, DateOnly? DueDate, string? Notes);

public sealed record UpdateProductionOrderRequest(
    Guid? ProductId,
    decimal? Quantity,
    DateOnly? DueDate,
    ProductionOrderStatus? Status,
    string? Notes,
    uint? Version);

public sealed record ProductResponse(Guid Id, string Sku, string Name, string Unit, bool IsActive);

public sealed record ProductionOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid ProductId,
    decimal Quantity,
    DateOnly DueDate,
    ProductionOrderStatus Status,
    IReadOnlyList<ProductionOrderStatus> AllowedNextStatuses,
    bool IsProductQuantityEditable,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    uint Version,
    string Unit);

public static class ProductionOrderMapper
{
    public static ProductionOrderResponse ToResponse(ProductionOrder order, string unit) => new(
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
        order.RowVersion,
        unit);
}
