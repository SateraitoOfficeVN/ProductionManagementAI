using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductionManagementAI.Infrastructure;
using Testcontainers.PostgreSql;

namespace ProductionManagementAI.Integration.Tests.PlantCalendar;

/// <summary>Rehearses additive upgrade, exact constraints and protected rollback on an isolated owner database.</summary>
public sealed class CalendarMigrationTests
{
    [Fact]
    public async Task UpgradePreservesExistingRowsAndDownRefusesActivatedHistory()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17").Build();await postgres.StartAsync();
        var connection = postgres.GetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection).UseSnakeCaseNamingConvention().Options;
        await using var db = new AppDbContext(options);
        const string baseline = "20261001042938_IndexProductionLineAssignments";
        await db.Database.MigrateAsync(baseline);
        await db.Database.ExecuteSqlRawAsync("INSERT INTO production_lines (id,code,name,working_hours_per_day,is_active,created_at_utc,updated_at_utc) VALUES ('11111111-1111-1111-1111-111111111111','CAL-MIG','migration fixture',7.125,true,now(),now()); INSERT INTO production_line_products (line_id,product_id,minutes_per_unit,confirmed_unit,confirmed_unit_revision,is_active,created_at_utc,updated_at_utc) SELECT '11111111-1111-1111-1111-111111111111',id,0.125,unit,unit_revision,true,now(),now() FROM products ORDER BY id LIMIT 1;");
        var checksums = new Dictionary<string,string>();
        foreach (var table in new[] { "products", "production_orders", "production_lines", "production_line_products" })
            checksums[table] = await Scalar<string>(connection, $"SELECT md5(string_agg(to_jsonb(row)::text,',' ORDER BY to_jsonb(row)::text)) FROM public.{table} row");
        await db.Database.MigrateAsync();
        foreach (var (table,checksum) in checksums)
            Assert.Equal(checksum,await Scalar<string>(connection,$"SELECT md5(string_agg(to_jsonb(row)::text,',' ORDER BY to_jsonb(row)::text)) FROM public.{table} row"));
        Assert.Equal(0L,await Scalar<long>(connection,"SELECT count(*) FROM plant_calendar_state"));
        Assert.Equal(-1,await Scalar<int>(connection,"SELECT atttypmod FROM pg_attribute WHERE attrelid='plant_calendar_exception_revisions'::regclass AND attname='working_hours'"));
        Assert.True(await Scalar<bool>(connection,"SELECT indisunique AND indisvalid AND indnullsnotdistinct FROM pg_index WHERE indexrelid='ux_calendar_exception_current'::regclass"));
        await db.Database.MigrateAsync(baseline);
        Assert.True(await Scalar<bool>(connection,"SELECT to_regclass('public.plant_calendar_state') IS NULL"));
        await db.Database.MigrateAsync();
        await db.Database.ExecuteSqlRawAsync("BEGIN; INSERT INTO plant_calendar_state (id,activated_on,time_zone_id,revision) VALUES (1,'2031-06-10','Asia/Tokyo',1); INSERT INTO plant_calendar_weekly_revisions (calendar_id,effective_from,working_weekdays,is_withdrawn,is_current,commit_revision) VALUES (1,'2031-06-10',31,false,true,1); COMMIT;");
        await db.Database.ExecuteSqlRawAsync("INSERT INTO plant_calendar_exception_revisions (calendar_id,calendar_date,is_working,working_hours,is_removed,is_current,commit_revision) VALUES (1,'2031-06-11',true,0.125,false,true,2)");
        var duplicate = await Assert.ThrowsAsync<PostgresException>(()=>db.Database.ExecuteSqlRawAsync("INSERT INTO plant_calendar_exception_revisions (calendar_id,calendar_date,is_working,is_removed,is_current,commit_revision) VALUES (1,'2031-06-11',false,false,true,3)"));
        Assert.Equal("23505",duplicate.SqlState);Assert.Equal("ux_calendar_exception_current",duplicate.ConstraintName);
        var scale = await Assert.ThrowsAsync<PostgresException>(()=>db.Database.ExecuteSqlRawAsync("INSERT INTO plant_calendar_exception_revisions (calendar_id,calendar_date,is_working,working_hours,is_removed,is_current,commit_revision) VALUES (1,'2031-06-12',true,0.1251,false,true,3)"));
        Assert.Equal("23514",scale.SqlState);Assert.Equal("ck_calendar_exception_hours",scale.ConstraintName);
        var unsafeDown = await Assert.ThrowsAsync<PostgresException>(()=>db.Database.MigrateAsync(baseline));
        Assert.Equal("P0001",unsafeDown.SqlState);
        Assert.Equal(1L,await Scalar<long>(connection,"SELECT count(*) FROM plant_calendar_state"));
        Assert.Equal(1L,await Scalar<long>(connection,"SELECT count(*) FROM plant_calendar_exception_revisions"));
        foreach (var (table,checksum) in checksums)
            Assert.Equal(checksum,await Scalar<string>(connection,$"SELECT md5(string_agg(to_jsonb(row)::text,',' ORDER BY to_jsonb(row)::text)) FROM public.{table} row"));
    }
    private static async Task<T> Scalar<T>(string connectionString,string sql)
    {
        await using var connection=new NpgsqlConnection(connectionString);await connection.OpenAsync();
        await using var command=new NpgsqlCommand(sql,connection);
        var value=await command.ExecuteScalarAsync();return value is T result?result:throw new InvalidOperationException("Unexpected fixture scalar type");
    }
}
