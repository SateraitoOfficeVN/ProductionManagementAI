namespace ProductionManagementAI.Domain.ProductionOrders;

/// <summary>Classifies a product from the fixed list in 004_BD-TYPE §3.</summary>
public enum ProductType
{
    Finished,
    SemiFinished,
    Part,
    RawMaterial,
}

/// <summary>Maps product types to the stable codes used by the API and database (004_DD-API-TYPE).</summary>
public static class ProductTypeCodes
{
    private static readonly Dictionary<ProductType, string> Codes = new()
    {
        [ProductType.Finished] = "FINISHED",
        [ProductType.SemiFinished] = "SEMI_FINISHED",
        [ProductType.Part] = "PART",
        [ProductType.RawMaterial] = "RAW_MATERIAL",
    };

    private static readonly Dictionary<string, ProductType> Types =
        Codes.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);

    /// <summary>Gets every code in display order.</summary>
    public static IReadOnlyCollection<string> All => Codes.Values;

    /// <summary>Gets the stable code of a product type.</summary>
    public static string ToCode(ProductType type) => Codes[type];

    /// <summary>Parses an exact, case-sensitive code.</summary>
    public static bool TryParse(string? code, out ProductType type)
    {
        type = default;
        return code is not null && Types.TryGetValue(code, out type);
    }
}
