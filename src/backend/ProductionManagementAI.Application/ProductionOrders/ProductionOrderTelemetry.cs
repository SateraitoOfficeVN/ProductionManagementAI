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

    /// <summary>Counts bounded unit-validation outcomes for WI-006 order writes.</summary>
    public static readonly Counter<long> UnitValidation = Meter.CreateCounter<long>(
        "pmai.orders.unit_validation", description: "Unit-aware order validation by outcome.");

    /// <summary>Records a bounded outcome without product identity or submitted quantity.</summary>
    public static void RecordUnitValidation(Activity? activity, string outcome)
    {
        activity?.SetTag("unit_validation", outcome);
        UnitValidation.Add(1, new KeyValuePair<string, object?>("outcome", outcome));
    }

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

    // 002_DD-FN-CSV §1 (WI-016): CSV export.
    public static readonly Counter<long> Exported = Meter.CreateCounter<long>(
        "pmai.production_orders.exported", description: "Production order CSV exports by outcome.");

    public static readonly Histogram<int> ExportRows = Meter.CreateHistogram<int>(
        "pmai.production_orders.export_rows", description: "Rows written by a successful production order CSV export.");

    public static class Outcomes
    {
        public const string Success = "success";
        public const string ValidationFailed = "validation_failed";
        public const string RuleViolation = "rule_violation";
        public const string Conflict = "conflict";
        public const string NotFound = "not_found";
        public const string Error = "error";
        public const string Cancelled = "cancelled";
    }
}
