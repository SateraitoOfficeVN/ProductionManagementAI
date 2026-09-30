using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Products;

/// <summary>Supplies Product master persistence and transaction operations.</summary>
public interface IProductMasterRepository
{
    /// <summary>Reads one validated catalog page and its filtered total.</summary>
    Task<PagedResult<ProductMasterItem>> ListAsync(ProductMasterListQuery query, CancellationToken cancellationToken);
    /// <summary>Reads a product and its current reference indicator.</summary>
    Task<ProductMasterItem?> GetAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Returns a write transaction that holds product-row locks through commit.</summary>
    Task<IProductionOrderTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    /// <summary>Locks one product row for edit or retirement.</summary>
    Task<Product?> LockAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Indicates whether an order references the locked product.</summary>
    Task<bool> HasOrdersAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Tracks a new product row.</summary>
    void Add(Product product);
    /// <summary>Persists tracked rows, translating SKU and version conflicts.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
    /// <summary>Projects the just-saved row with its latest version and reference state.</summary>
    Task<ProductMasterItem> ReadSavedAsync(Guid id, CancellationToken cancellationToken);
}

/// <summary>Reports a case-insensitive SKU uniqueness conflict from PostgreSQL.</summary>
public sealed class ProductSkuConflictException(Exception? inner = null)
    : Exception("The product SKU is already in use.", inner);

/// <summary>Reports a stale Product master row version.</summary>
public sealed class ProductVersionConflictException(Exception? inner = null)
    : Exception("The product changed since it was loaded.", inner);
