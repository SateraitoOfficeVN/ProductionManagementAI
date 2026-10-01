using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductionManagementAI.Domain.ProductionOrders;
using ProductionManagementAI.Infrastructure;
using Testcontainers.PostgreSql;

namespace ProductionManagementAI.Integration.Tests.ProductionLines;

/// <summary>Rehearses WI-009 migration, restricted grants, unit generations and owner recovery on disposable databases.</summary>
public sealed class ProductionLineMigrationTests
{
    /// <summary>Verifies TC-358 preserves baseline history and validates additive schema.</summary>
    [Fact]
    public async Task UpgradePreservesHistoryAndValidatesSchema()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17").Build();
        await postgres.StartAsync();
        await using var db = Context(postgres.GetConnectionString());
        await db.Database.MigrateAsync("20260930022739_ConvertOrderQuantityToNumeric");
        var before = await Scalar<string>(postgres.GetConnectionString(),
            "SELECT md5(string_agg(id::text || product_id::text || quantity::text || status || updated_at_utc::text, ',' ORDER BY id)) FROM production_orders");
        await db.Database.MigrateAsync();
        Assert.Equal(before, await Scalar<string>(postgres.GetConnectionString(),
            "SELECT md5(string_agg(id::text || product_id::text || quantity::text || status || updated_at_utc::text, ',' ORDER BY id)) FROM production_orders"));
        Assert.Equal(0L, await Scalar<long>(postgres.GetConnectionString(), "SELECT count(*) FROM production_orders WHERE line_id IS NOT NULL"));
        Assert.True(await Scalar<bool>(postgres.GetConnectionString(), "SELECT indisvalid AND indisready FROM pg_index WHERE indexrelid='ix_orders_line_product'::regclass"));
        Assert.Equal(2L, await Scalar<long>(postgres.GetConnectionString(), "SELECT count(*) FROM pg_constraint WHERE conname IN ('fk_orders_line_product','ck_products_unit_revision') AND convalidated"));
        Assert.Equal(0L, await Scalar<long>(postgres.GetConnectionString(), "SELECT count(*) FROM production_lines"));
        Assert.Equal(0L, await Scalar<long>(postgres.GetConnectionString(), "SELECT count(*) FROM production_line_products"));
    }

    /// <summary>Verifies TC-350/357 uses generated ABA revisions without runtime revision or immutable-column access.</summary>
    [Fact]
    public async Task FreshSchemaUsesRestrictedRuntimeRoleAndGeneratedUnitRevision()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17").Build();
        await postgres.StartAsync();
        await using var owner = Context(postgres.GetConnectionString());
        await owner.Database.MigrateAsync();
        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync();
        await new NpgsqlCommand("SET ROLE pmai_app", connection).ExecuteNonQueryAsync();
        await using var runtime = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connection).UseSnakeCaseNamingConvention().Options);
        var product = new Product { Id = Guid.NewGuid(), Sku = "UNIT-ABA", Name = "Unit fixture", Unit = "個" };
        runtime.Products.Add(product);
        await runtime.SaveChangesAsync();
        Assert.Equal(0, product.UnitRevision);
        product.Unit = "kg";
        await runtime.SaveChangesAsync();
        Assert.Equal(1, product.UnitRevision);
        product.Unit = "個";
        await runtime.SaveChangesAsync();
        Assert.Equal(2, product.UnitRevision);
        product.Name = "Updated fixture";
        await runtime.SaveChangesAsync();
        Assert.Equal(2, product.UnitRevision);
        await new NpgsqlCommand("UPDATE products SET unit = unit WHERE sku = 'UNIT-ABA'", connection).ExecuteNonQueryAsync();
        Assert.Equal(2L, await Scalar<long>(postgres.GetConnectionString(), "SELECT unit_revision FROM products WHERE sku='UNIT-ABA'"));
        foreach (var sql in new[] {
            "UPDATE products SET unit_revision = 0 WHERE sku='UNIT-ABA'",
            "UPDATE production_lines SET code = 'forged'",
            "UPDATE production_line_products SET product_id = product_id",
            "DELETE FROM production_lines",
            "CREATE TABLE unauthorized_fixture(id int)" })
        {
            var error = await Assert.ThrowsAsync<PostgresException>(() => new NpgsqlCommand(sql, connection).ExecuteNonQueryAsync());
            Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, error.SqlState);
        }
    }

    /// <summary>Verifies TC-349 rejects invalid numeric values before rounding.</summary>
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("24.001")]
    [InlineData("1.2340")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public async Task NumericConstraintRejectsInvalidHours(string hours)
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17").Build();
        await postgres.StartAsync();
        await using var db = Context(postgres.GetConnectionString());
        await db.Database.MigrateAsync();
        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "INSERT INTO production_lines(code,name,working_hours_per_day) VALUES ('INVALID','Fixture',CAST(@hours AS numeric))", connection);
        command.Parameters.AddWithValue("hours", hours);
        var error = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }

    /// <summary>Verifies TC-359 refuses an actual failed concurrent index and requires explicit isolated owner repair.</summary>
    [Fact]
    public async Task FailedConcurrentIndexRequiresRepairAndUnsafeDownIsRejected()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17").Build();
        await postgres.StartAsync();
        await using var db = Context(postgres.GetConnectionString());
        var expand = db.Database.GetMigrations().Single(m => m.EndsWith("_ExpandProductionLines", StringComparison.Ordinal));
        await db.Database.MigrateAsync(expand);
        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync();
        // Seeded quantities contain duplicates: a failed unique build leaves a real invalid index.
        await Assert.ThrowsAsync<PostgresException>(() => new NpgsqlCommand(
            "CREATE UNIQUE INDEX CONCURRENTLY ix_orders_line_product ON production_orders(quantity)", connection).ExecuteNonQueryAsync());
        Assert.False(await Scalar<bool>(postgres.GetConnectionString(), "SELECT indisvalid FROM pg_index WHERE indexrelid='ix_orders_line_product'::regclass"));
        var failure = await Assert.ThrowsAsync<PostgresException>(() => db.Database.MigrateAsync());
        Assert.Contains("owner-reviewed repair", failure.MessageText);
        await new NpgsqlCommand("DROP INDEX CONCURRENTLY ix_orders_line_product", connection).ExecuteNonQueryAsync();
        await db.Database.MigrateAsync();
        Assert.True(await Scalar<bool>(postgres.GetConnectionString(), "SELECT indisvalid AND indisready FROM pg_index WHERE indexrelid='ix_orders_line_product'::regclass"));
        await new NpgsqlCommand("INSERT INTO production_lines(code,name,working_hours_per_day) VALUES ('USED','Fixture',8)", connection).ExecuteNonQueryAsync();
        await Assert.ThrowsAsync<NotSupportedException>(() => db.Database.MigrateAsync(expand));
        Assert.Equal(1L, await Scalar<long>(postgres.GetConnectionString(), "SELECT count(*) FROM production_lines WHERE code='USED'"));
    }


    /// <summary>Checks timing range, exact scale and special numerics on a real unconstrained numeric column.</summary>
    [Fact]
    public async Task NumericConstraintRejectsInvalidMinutesAndPreservesAcceptedCeiling()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17").Build(); await postgres.StartAsync();
        await using var db = Context(postgres.GetConnectionString()); await db.Database.MigrateAsync();
        await using var connection = new NpgsqlConnection(postgres.GetConnectionString()); await connection.OpenAsync();
        var lineId = Guid.NewGuid(); var productId = Guid.NewGuid();
        await new NpgsqlCommand($"INSERT INTO production_lines(id,code,name,working_hours_per_day) VALUES ('{lineId}','NUMERIC','fixture',8); INSERT INTO products(id,sku,name,unit,is_active) VALUES ('{productId}','NUMERIC','fixture','kg',true)", connection).ExecuteNonQueryAsync();
        foreach (var minutes in new[] { "0", "-1", "0.0001", "1.2340", "1000000000", "NaN", "Infinity", "-Infinity" })
        {
            await using var command = new NpgsqlCommand($"INSERT INTO production_line_products(line_id,product_id,minutes_per_unit,confirmed_unit,confirmed_unit_revision) VALUES ('{lineId}','{productId}',CAST(@minutes AS numeric),'kg',0)", connection);
            command.Parameters.AddWithValue("minutes", minutes);
            var error = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync()); Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
        }
        await new NpgsqlCommand($"INSERT INTO production_line_products(line_id,product_id,minutes_per_unit,confirmed_unit,confirmed_unit_revision) VALUES ('{lineId}','{productId}',999999999.999,'kg',0)", connection).ExecuteNonQueryAsync();
        Assert.Equal(999999999.999m, await Scalar<decimal>(postgres.GetConnectionString(), $"SELECT minutes_per_unit FROM production_line_products WHERE line_id='{lineId}'"));
    }

    private static AppDbContext Context(string connectionString) => new(
        new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).UseSnakeCaseNamingConvention().Options);

    private static async Task<T> Scalar<T>(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException("Expected a scalar fixture result."));
    }
}
