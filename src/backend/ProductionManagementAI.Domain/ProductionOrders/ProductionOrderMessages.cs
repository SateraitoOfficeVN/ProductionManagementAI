namespace ProductionManagementAI.Domain.ProductionOrders;

/// <summary>
/// Message IDs from DD-001's message catalog. The API returns IDs, never text; the frontend maps them to wording.
/// </summary>
public static class ProductionOrderMessages
{
    public const string ProductRequired = "MSG-E001";
    public const string ProductNotFound = "MSG-E002";
    public const string QuantityInvalid = "MSG-E003";
    public const string DueDateRequired = "MSG-E004";
    public const string DueDateInPast = "MSG-E005";
    public const string NotesTooLong = "MSG-E006";
    public const string StatusTransitionNotAllowed = "MSG-E007";
    public const string LockedFieldChanged = "MSG-E008";
    public const string ConcurrencyConflict = "MSG-E009";
    public const string QuantityTooLarge = "MSG-E010";
    public const string OrderNotFound = "MSG-E011";
    public const string Unexpected = "MSG-E013";
}
