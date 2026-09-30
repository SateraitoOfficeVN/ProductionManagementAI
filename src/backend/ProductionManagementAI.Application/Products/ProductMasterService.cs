using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Products;

/// <summary>Maintains the product catalog with version checks and serialized unit changes (004_DD-FN).</summary>
public sealed class ProductMasterService(IProductMasterRepository repository, TimeProvider timeProvider)
{
    /// <summary>Gets the Product master telemetry source name.</summary>
    public const string TelemetryName = "ProductionManagementAI.Products";
    private static readonly ActivitySource Source = new(TelemetryName);
    private static readonly Meter Meter = new(TelemetryName);
    private static readonly Counter<long> Requests = Meter.CreateCounter<long>("pmai.products.requests");

    private static readonly HashSet<string> Units =
        ["個", "本", "枚", "台", "セット", "kg", "m"];

    /// <summary>Lists a bounded, filtered page of catalog entries.</summary>
    public async Task<Result<PagedResult<ProductMasterItem>>> ListAsync(
        ProductMasterListRequest request, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Product.List");
        var parsed = ProductMasterListQuery.TryCreate(request);
        if (parsed is not Result<ProductMasterListQuery>.Ok valid)
        {
            Record("list", "validation");
            return new Result<PagedResult<ProductMasterItem>>.Invalid(
                ((Result<ProductMasterListQuery>.Invalid)parsed).Errors);
        }
        var page = await repository.ListAsync(valid.Value, cancellationToken);
        Record("list", "success");
        return new Result<PagedResult<ProductMasterItem>>.Ok(page);
    }

    /// <summary>Gets one active or retired product, including its unit-lock indicator.</summary>
    public async Task<Result<ProductMasterItem>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Product.Get");
        var item = await repository.GetAsync(id, cancellationToken);
        Record("get", item is null ? "not_found" : "success");
        return item is null ? new Result<ProductMasterItem>.NotFound() : new Result<ProductMasterItem>.Ok(item);
    }

    /// <summary>Creates a product after normalizing and validating its mutable fields.</summary>
    public async Task<Result<ProductMasterItem>> CreateAsync(
        CreateProductRequest request, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Product.Create");
        var sku = request.Sku?.Trim();
        var name = request.Name?.Trim();
        var drawing = NormalizeDrawing(request.DrawingNumber);
        var errors = Validate(sku, name, request.Unit, drawing);
        if (errors.Count > 0)
        {
            Record("create", "validation");
            return new Result<ProductMasterItem>.Invalid(errors);
        }

        var now = timeProvider.GetUtcNow();
        var product = new Product
        {
            Id = Guid.CreateVersion7(now),
            Sku = sku!,
            Name = name!,
            Unit = request.Unit!,
            DrawingNumber = drawing,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        repository.Add(product);
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (ProductSkuConflictException)
        {
            Record("create", "sku_conflict");
            return Conflict("PRODUCT_SKU_CONFLICT", "sku");
        }

        Record("create", "success");
        return new Result<ProductMasterItem>.Ok(
            await repository.ReadSavedAsync(product.Id, cancellationToken));
    }

    /// <summary>Updates mutable fields while preventing a referenced product's unit from changing.</summary>
    public async Task<Result<ProductMasterItem>> UpdateAsync(
        Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Product.Update");
        var name = request.Name?.Trim();
        var drawing = NormalizeDrawing(request.DrawingNumber);
        var errors = Validate(request.Sku, name, request.Unit, drawing);
        if (request.Version is null) errors["version"] = ["VALIDATION"];
        if (errors.Count > 0)
        {
            Record("update", "validation");
            return new Result<ProductMasterItem>.Invalid(errors);
        }

        await using var transaction = await repository.BeginTransactionAsync(cancellationToken);
        var product = await repository.LockAsync(id, cancellationToken);
        if (product is null)
        {
            Record("update", "not_found");
            return new Result<ProductMasterItem>.NotFound();
        }
        if (product.RowVersion != request.Version)
        {
            Record("update", "stale");
            return Conflict("PRODUCT_STALE", "version");
        }
        if (product.Sku != request.Sku)
        {
            Record("update", "validation");
            return new Result<ProductMasterItem>.Invalid(Error("sku", "VALIDATION"));
        }
        if (product.Unit != request.Unit)
        {
            using var lockActivity = Source.StartActivity("Product.UnitLockCheck");
            if (await repository.HasOrdersAsync(id, cancellationToken))
            {
                Record("update", "unit_locked");
                return Conflict("PRODUCT_UNIT_LOCKED", "unit");
            }
        }

        product.Name = name!;
        product.Unit = request.Unit!;
        product.DrawingNumber = drawing;
        product.UpdatedAtUtc = timeProvider.GetUtcNow();
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (ProductVersionConflictException)
        {
            Record("update", "stale");
            return Conflict("PRODUCT_STALE", "version");
        }
        Record("update", "success");
        return new Result<ProductMasterItem>.Ok(await repository.ReadSavedAsync(id, cancellationToken));
    }

    /// <summary>Retires a product without deleting it or its historical orders.</summary>
    public async Task<Result<ProductMasterItem>> RetireAsync(
        Guid id, RetireProductRequest request, CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Product.Retire");
        if (request.Version is null)
        {
            Record("retire", "validation");
            return new Result<ProductMasterItem>.Invalid(Error("version", "VALIDATION"));
        }
        await using var transaction = await repository.BeginTransactionAsync(cancellationToken);
        var product = await repository.LockAsync(id, cancellationToken);
        if (product is null)
        {
            Record("retire", "not_found");
            return new Result<ProductMasterItem>.NotFound();
        }
        if (product.RowVersion != request.Version)
        {
            Record("retire", "stale");
            return Conflict("PRODUCT_STALE", "version");
        }
        if (!product.IsActive)
        {
            Record("retire", "validation");
            return new Result<ProductMasterItem>.Invalid(Error("isActive", "VALIDATION"));
        }
        product.IsActive = false;
        product.UpdatedAtUtc = timeProvider.GetUtcNow();
        try
        {
            await repository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (ProductVersionConflictException)
        {
            Record("retire", "stale");
            return Conflict("PRODUCT_STALE", "version");
        }
        Record("retire", "success");
        return new Result<ProductMasterItem>.Ok(await repository.ReadSavedAsync(id, cancellationToken));
    }

    private static Dictionary<string, string[]> Validate(
        string? sku, string? name, string? unit, string? drawing)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(sku) || RuneCount(sku) > 50) errors["sku"] = ["VALIDATION"];
        if (string.IsNullOrWhiteSpace(name) || RuneCount(name) > 200) errors["name"] = ["VALIDATION"];
        if (unit is null || !Units.Contains(unit)) errors["unit"] = ["VALIDATION"];
        if (drawing is not null && RuneCount(drawing) > 100) errors["drawingNumber"] = ["VALIDATION"];
        return errors;
    }

    private static string? NormalizeDrawing(string? drawing)
    {
        var trimmed = drawing?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static int RuneCount(string text)
    {
        var count = 0;
        foreach (var _ in text.EnumerateRunes()) count++;
        return count;
    }

    private static Dictionary<string, string[]> Error(string field, string code) =>
        new(StringComparer.Ordinal) { [field] = [code] };

    private static Result<ProductMasterItem> Conflict(string code, string field) =>
        new Result<ProductMasterItem>.FieldConflict(code, Error(field, code));

    private static void Record(string operation, string outcome) =>
        Requests.Add(1,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("outcome", outcome));
}
