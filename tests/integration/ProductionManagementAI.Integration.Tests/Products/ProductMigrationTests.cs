using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductionManagementAI.Infrastructure;
using Testcontainers.PostgreSql;

namespace ProductionManagementAI.Integration.Tests.Products;

/// <summary>Rehearses the WI-006 upgrade against an isolated pre-WI-006 schema and demo history.</summary>
public sealed class ProductMigrationTests
{
    [Fact]
    public async Task UpgradePreservesSeedIdsEditedNamesAndIntegerOrderValues()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.GetConnectionString()).UseSnakeCaseNamingConvention().Options;
        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync("20260923053923_LocalizeDemoDataToJapanese");

        const string productId = "0197e4a0-0000-7000-8000-000000001001";
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE products SET name = '改訂済み製品' WHERE id = '0197e4a0-0000-7000-8000-000000001001'");
        var oldCount = await Scalar<long>(postgres.GetConnectionString(),
            "SELECT count(*) FROM production_orders");
        var oldQuantity = await Scalar<int>(postgres.GetConnectionString(),
            "SELECT quantity FROM production_orders ORDER BY id LIMIT 1");
        var oldRows = await Scalar<string>(postgres.GetConnectionString(),
            "SELECT md5(string_agg(id::text || product_id::text || quantity::text, ',' ORDER BY id)) FROM production_orders");
        await db.Database.MigrateAsync();
        Assert.Equal(oldRows, await Scalar<string>(postgres.GetConnectionString(),
            "SELECT md5(string_agg(id::text || product_id::text || quantity::text, ',' ORDER BY id)) FROM production_orders"));
        Assert.True(await Scalar<bool>(postgres.GetConnectionString(),
            "SELECT indisvalid AND indisunique FROM pg_index WHERE indexrelid = 'ux_products_sku_lower'::regclass"));

        Assert.Equal(30L, await Scalar<long>(postgres.GetConnectionString(), "SELECT count(*) FROM products"));
        Assert.Equal(oldCount, await Scalar<long>(postgres.GetConnectionString(),
            "SELECT count(*) FROM production_orders"));
        Assert.Equal("改訂済み製品", await Scalar<string>(postgres.GetConnectionString(),
            $"SELECT name FROM products WHERE id = '{productId}'"));
        Assert.Equal("個", await Scalar<string>(postgres.GetConnectionString(),
            $"SELECT unit FROM products WHERE id = '{productId}'"));
        Assert.Equal((decimal)oldQuantity, await Scalar<decimal>(postgres.GetConnectionString(),
            "SELECT quantity FROM production_orders ORDER BY id LIMIT 1"));
        Assert.Equal("numeric", await Scalar<string>(postgres.GetConnectionString(),
            "SELECT data_type FROM information_schema.columns WHERE table_name = 'production_orders' AND column_name = 'quantity'"));
        Assert.Equal(30L, await Scalar<long>(postgres.GetConnectionString(),
            "SELECT count(*) FROM products WHERE unit IS NOT NULL"));
        Assert.Equal(0L, await Scalar<long>(postgres.GetConnectionString(),
            "SELECT count(*) FROM production_orders o LEFT JOIN products p ON p.id = o.product_id WHERE p.id IS NULL OR p.unit IS NULL"));
    }

    private static async Task<T> Scalar<T>(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync())!;
    }
}
