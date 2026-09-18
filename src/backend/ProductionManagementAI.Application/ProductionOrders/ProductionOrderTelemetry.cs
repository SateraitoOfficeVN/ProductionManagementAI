using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ProductionManagementAI.Application.ProductionOrders;

/// <summary>Spans and counters specified in DD-001 "Observability".</summary>
public static class ProductionOrderTelemetry
{
    public const string Name = "ProductionManagementAI.ProductionOrders";

    public static readonly ActivitySource Source = new(Name);

    private static readonly Meter Meter = new(Name);

    public static readonly Counter<long> Created =
        Meter.CreateCounter<long>("pmai.production_orders.created", description: "Production order create attempts by outcome.");

    public static readonly Counter<long> Updated =
        Meter.CreateCounter<long>("pmai.production_orders.updated", description: "Production order update attempts by outcome.");

    public static readonly Counter<long> StatusTransitions =
        Meter.CreateCounter<long>("pmai.production_orders.status_transitions", description: "Successful status changes.");

    public static class Outcomes
    {
        public const string Success = "success";
        public const string ValidationFailed = "validation_failed";
        public const string RuleViolation = "rule_violation";
        public const string Conflict = "conflict";
        public const string NotFound = "not_found";
        public const string Error = "error";
    }
}
