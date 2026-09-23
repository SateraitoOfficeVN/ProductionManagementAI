using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ProductionManagementAI.Application.ProductionOrders;

/// <summary>Spans and counters specified in 001_DD "Observability".</summary>
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

    // 002_DD-FN "Observability".
    public static readonly Counter<long> Listed =
        Meter.CreateCounter<long>("pmai.production_orders.listed", description: "Production order list queries by outcome.");

    public static readonly Histogram<int> ListResultSize = Meter.CreateHistogram<int>(
        "pmai.production_orders.list_result_size", description: "Rows returned on a production order list page.");

    // 003_DD-FN "Observability".
    public static readonly Counter<long> DashboardLoaded = Meter.CreateCounter<long>(
        "pmai.production_orders.dashboard_loaded", description: "Dashboard snapshot requests by outcome.");

    public static readonly Counter<long> HealthChecks = Meter.CreateCounter<long>(
        "pmai.system.health_checks", description: "Health checks by database status.");

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
