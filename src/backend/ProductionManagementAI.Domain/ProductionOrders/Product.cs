namespace ProductionManagementAI.Domain.ProductionOrders;

/// <summary>Stores one maintainable product while retaining its identity for historical orders (004_DB).</summary>
public sealed class Product
{
    public Guid Id { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; set; }
    /// <summary>Gets or sets the approved unit used by all orders that reference this product.</summary>
    public string Unit { get; set; } = string.Empty;
    /// <summary>Gets or sets the optional drawing reference.</summary>
    public string? DrawingNumber { get; set; }
    /// <summary>Gets or sets a value that indicates whether new orders may select this product.</summary>
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; init; }
    /// <summary>Gets or sets the last catalog-change time in UTC.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
    /// <summary>Gets or sets the PostgreSQL optimistic concurrency token.</summary>
    public uint RowVersion { get; set; }
}
