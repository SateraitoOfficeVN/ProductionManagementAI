namespace ProductionManagementAI.Domain.ProductionOrders;

/// <summary>Minimal product reference data, seeded; no catalog UI (DEC-005, 001_DB).</summary>
public sealed class Product
{
    public Guid Id { get; init; }
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
