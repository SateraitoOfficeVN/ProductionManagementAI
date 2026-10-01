namespace ProductionManagementAI.Domain.ProductionLines;

/// <summary>Stores a durable supported-product timing and its confirmed unit generation.</summary>
public sealed class ProductionLineProduct
{
    /// <summary>Gets the immutable line key.</summary>
    public Guid LineId { get; init; }
    /// <summary>Gets the immutable product key.</summary>
    public Guid ProductId { get; init; }
    /// <summary>Gets or sets the exact minutes required for one confirmed product unit.</summary>
    public decimal MinutesPerUnit { get; set; }
    /// <summary>Gets or sets the explicitly confirmed unit.</summary>
    public required string ConfirmedUnit { get; set; }
    /// <summary>Gets or sets the explicitly confirmed database unit generation.</summary>
    public long ConfirmedUnitRevision { get; set; }
    /// <summary>Gets or sets whether new orders may use this pair.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Gets the immutable creation time.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }
    /// <summary>Gets or sets the last configuration change time.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
