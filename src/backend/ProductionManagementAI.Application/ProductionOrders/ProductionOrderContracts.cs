using ProductionManagementAI.Domain.ProductionOrders;
using System.Text.Json.Serialization;

namespace ProductionManagementAI.Application.ProductionOrders;

// Request/response shapes for 001_DD-API. Request fields are nullable so a missing field is reported as
// "required" with its message ID instead of failing model binding.

public sealed record CreateProductionOrderRequest(Guid? ProductId, decimal? Quantity, DateOnly? DueDate, string? Notes, Guid? LineId = null);

public sealed record UpdateProductionOrderRequest(
    Guid? ProductId,
    decimal? Quantity,
    DateOnly? DueDate,
    ProductionOrderStatus? Status,
    string? Notes,
    uint? Version)
{
    private Guid? lineId;
    /// <summary>Gets or sets the requested assignment, preserving omission separately from explicit null.</summary>
    public Guid? LineId { get => lineId; init { lineId = value; HasLineId = true; } }
    /// <summary>Gets a value that indicates whether the payload explicitly supplies lineId.</summary>
    [JsonIgnore]
    public bool HasLineId { get; private init; }
}

/// <summary>Returns current assigned line identity, including historical retired lines.</summary>
public sealed record OrderLineResponse(Guid Id, string Code, string Name, bool IsActive);

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
    string Unit,
    OrderLineResponse? Line = null);

public static class ProductionOrderMapper
{
    public static ProductionOrderResponse ToResponse(ProductionOrder order, string unit, OrderLineResponse? line = null) => new(
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
        unit,
        line);
}
