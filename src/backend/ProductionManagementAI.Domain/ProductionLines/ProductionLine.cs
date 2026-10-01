namespace ProductionManagementAI.Domain.ProductionLines;

/// <summary>Stores a durable production line and its aggregate concurrency token.</summary>
public sealed class ProductionLine
{
    /// <summary>Gets the immutable identity.</summary>
    public Guid Id { get; init; }
    /// <summary>Gets the immutable normalized line code.</summary>
    public required string Code { get; init; }
    /// <summary>Gets or sets the current display name.</summary>
    public required string Name { get; set; }
    /// <summary>Gets or sets the exact working hours per day.</summary>
    public decimal WorkingHoursPerDay { get; set; }
    /// <summary>Gets or sets whether new orders may select this line.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Gets the immutable creation time.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }
    /// <summary>Gets or sets the aggregate audit time.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
    /// <summary>Gets or sets the PostgreSQL xmin aggregate token.</summary>
    public uint RowVersion { get; set; }
}
