using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductionManagementAI.Infrastructure;
using Testcontainers.PostgreSql;

namespace ProductionManagementAI.Integration.Tests.Products;

/// <summary>Rehearses the WI-017 AddProductType upgrade on a populated pre-WI-017 database (004_DB-TYPE).</summary>
public sealed class ProductTypeMigrationTests
{
    [Fact]
    public async Task UpgradeAppliesDemoMappingAndFinishedForOtherRows()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17").Build();
        await postgres.StartAsync();
        var connection = postgres.GetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connection).UseSnakeCaseNamingConvention().Options;
        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync("20261002030411_ExpandPlantCalendar");
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO products (sku, name, unit) VALUES ('USER-1', '利用者登録品', '個')");
        await db.Database.ExecuteSqlRawAsync("UPDATE products SET name = '改訂' WHERE sku = 'P-1002'");

        await db.Database.MigrateAsync();

        Assert.Equal("PART", await Scalar<string>(connection, "SELECT product_type FROM products WHERE sku = 'P-1002'"));
        Assert.Equal("SEMI_FINISHED", await Scalar<string>(connection, "SELECT product_type FROM products WHERE sku = 'P-1006'"));
        Assert.Equal("FINISHED", await Scalar<string>(connection, "SELECT product_type FROM products WHERE sku = 'P-1001'"));
        Assert.Equal("FINISHED", await Scalar<string>(connection, "SELECT product_type FROM products WHERE sku = 'USER-1'"));
        Assert.Equal("13,5,12,0", await Scalar<string>(connection,
            """
            SELECT concat_ws(',',
                count(*) FILTER (WHERE product_type = 'FINISHED'),
                count(*) FILTER (WHERE product_type = 'SEMI_FINISHED'),
                count(*) FILTER (WHERE product_type = 'PART'),
                count(*) FILTER (WHERE product_type = 'RAW_MATERIAL'))
            FROM products WHERE sku LIKE 'P-10%'
            """));
        Assert.Null(await ScalarOrNull(connection,
            "SELECT column_default FROM information_schema.columns WHERE table_name = 'products' AND column_name = 'product_type'"));
        await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlRawAsync(
            "INSERT INTO products (sku, name, unit) VALUES ('USER-2', 'n', '個')"));
    }

    private static async Task<T> Scalar<T>(string connectionString, string sql) =>
        (T)(await ScalarOrNull(connectionString, sql))!;

    private static async Task<object?> ScalarOrNull(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is DBNull ? null : value;
    }
}
