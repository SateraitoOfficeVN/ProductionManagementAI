using Microsoft.EntityFrameworkCore;
using ProductionManagementAI.Application.Health;

namespace ProductionManagementAI.Infrastructure.Health;

/// <summary>Constant text, no input; reads no table and needs no grant (003_DB, 003_DD-FN §6).</summary>
internal sealed class DatabasePing(AppDbContext db) : IDatabasePing
{
    public Task PingAsync(CancellationToken cancellationToken) =>
        db.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
}
