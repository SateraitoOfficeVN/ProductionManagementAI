using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using ProductionManagementAI.Application.Products;
using ProductionManagementAI.Application.ProductionOrders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.Products;

internal sealed class ProductMasterRepository(AppDbContext db) : IProductMasterRepository
{
    public async Task<PagedResult<ProductMasterItem>> ListAsync(
        ProductMasterListQuery query, CancellationToken cancellationToken)
    {
        var products = db.Products.AsNoTracking().AsQueryable();
        if (query.State == "active") products = products.Where(p => p.IsActive);
        if (query.State == "retired") products = products.Where(p => !p.IsActive);
        if (query.Search is not null)
        {
            var escaped = query.Search.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("%", "\\%", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal);
            var pattern = $"%{escaped}%";
            products = products.Where(p => EF.Functions.ILike(p.Sku, pattern, "\\") ||
                                           EF.Functions.ILike(p.Name, pattern, "\\"));
        }

        var total = await products.CountAsync(cancellationToken);
        if (query.Page > int.MaxValue / ProductMasterListQuery.PageSize)
            return new PagedResult<ProductMasterItem>([], total, query.Page,
                ProductMasterListQuery.PageSize, "sku", "asc");
        var items = await products.OrderBy(p => p.Sku).ThenBy(p => p.Id)
            .Skip((query.Page - 1) * ProductMasterListQuery.PageSize)
            .Take(ProductMasterListQuery.PageSize)
            .Select(p => new ProductMasterItem(p.Id, p.Sku, p.Name, p.Unit,
                p.DrawingNumber, p.IsActive, p.UpdatedAtUtc, p.RowVersion,
                db.ProductionOrders.Any(o => o.ProductId == p.Id)))
            .ToListAsync(cancellationToken);
        return new PagedResult<ProductMasterItem>(items, total, query.Page,
            ProductMasterListQuery.PageSize, "sku", "asc");
    }

    public Task<ProductMasterItem?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Products.AsNoTracking().Where(p => p.Id == id)
            .Select(p => new ProductMasterItem(p.Id, p.Sku, p.Name, p.Unit,
                p.DrawingNumber, p.IsActive, p.UpdatedAtUtc, p.RowVersion,
                db.ProductionOrders.Any(o => o.ProductId == p.Id)))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IProductionOrderTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        new Transaction(await db.Database.BeginTransactionAsync(cancellationToken));

    public async Task<Product?> LockAsync(Guid id, CancellationToken cancellationToken)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("A product row lock requires a transaction.");
        var rows = await db.Products.FromSqlInterpolated(
            $"SELECT p.*, p.xmin FROM products p WHERE p.id = {id} FOR UPDATE").ToListAsync(cancellationToken);
        return rows.SingleOrDefault();
    }

    public Task<bool> HasOrdersAsync(Guid id, CancellationToken cancellationToken) =>
        db.ProductionOrders.AnyAsync(o => o.ProductId == id, cancellationToken);

    public void Add(Product product) => db.Products.Add(product);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ProductVersionConflictException(ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_products_sku_lower" or "ix_products_sku" })
        {
            throw new ProductSkuConflictException(ex);
        }
    }

    public async Task<ProductMasterItem> ReadSavedAsync(Guid id, CancellationToken cancellationToken) =>
        await GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("A saved product could not be read.");

    private sealed class Transaction(IDbContextTransaction inner) : IProductionOrderTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken) => inner.CommitAsync(cancellationToken);
        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
