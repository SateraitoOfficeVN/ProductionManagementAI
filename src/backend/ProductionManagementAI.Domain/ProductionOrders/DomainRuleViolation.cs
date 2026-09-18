namespace ProductionManagementAI.Domain.ProductionOrders;

/// <summary>A business invariant was violated; <see cref="Code"/> is a DD-001 message ID (e.g. MSG-E007, MSG-E008).</summary>
public sealed class DomainRuleViolation(string code) : Exception($"Domain rule violated: {code}")
{
    public string Code { get; } = code;
}
