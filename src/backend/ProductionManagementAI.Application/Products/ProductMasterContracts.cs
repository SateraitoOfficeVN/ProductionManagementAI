using ProductionManagementAI.Application.ProductionOrders;
using System.Text.Json.Serialization;
using System.Text;

namespace ProductionManagementAI.Application.Products;

/// <summary>Defines the submitted Product master search and paging values.</summary>
public sealed record ProductMasterListRequest(string? Q, string? State, string? Page, string? PageSize = null);

/// <summary>Defines a validated Product master list query.</summary>
public sealed record ProductMasterListQuery(string? Search, string State, int Page, int PageSize = ProductMasterListQuery.DefaultPageSize)
{
    /// <summary>Gets the catalog page size used when none is requested.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>Gets the page sizes a client may request, the same choices as the order list (WI-013 DEC-007).</summary>
    public static readonly IReadOnlyList<int> PageSizes = [10, 20, 50, 100];

    /// <summary>Validates list filters without widening an invalid query.</summary>
    public static Result<ProductMasterListQuery> TryCreate(ProductMasterListRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var search = request.Q?.Trim();
        if (search is not null && search.EnumerateRunes().Count() > 100) errors["q"] = ["VALIDATION"];
        var state = request.State ?? "all";
        if (state is not ("all" or "active" or "retired")) errors["state"] = ["VALIDATION"];
        if (!int.TryParse(request.Page ?? "1", out var page) || page < 1)
            errors["page"] = ["VALIDATION"];
        var pageSize = DefaultPageSize;
        if (request.PageSize is not null && (!int.TryParse(request.PageSize, out pageSize) || !PageSizes.Contains(pageSize)))
            errors["pageSize"] = ["VALIDATION"];
        return errors.Count > 0
            ? new Result<ProductMasterListQuery>.Invalid(errors)
            : new Result<ProductMasterListQuery>.Ok(new ProductMasterListQuery(
                string.IsNullOrEmpty(search) ? null : search, state, page, pageSize));
    }
}

/// <summary>Returns one product and its current concurrency and unit-lock state.</summary>
public sealed record ProductMasterItem(
    Guid Id, string Sku, string Name, string Unit, string? DrawingNumber, bool IsActive,
    DateTimeOffset UpdatedAt, uint Version, bool UnitLocked);

/// <summary>Defines required fields for creating a product.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateProductRequest(string? Sku, string? Name, string? Unit, string? DrawingNumber);

/// <summary>Defines a full-replacement edit and its expected version.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateProductRequest(
    string? Sku, string? Name, string? Unit, string? DrawingNumber, uint? Version);

/// <summary>Defines a versioned retirement command.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RetireProductRequest(uint? Version);
