using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using ProductionManagementAI.Application.Dashboard;

namespace ProductionManagementAI.Infrastructure.Dashboard;

/// <summary>
/// 003_DB Q1–Q6 — seven statements — in one <c>REPEATABLE READ READ ONLY</c> transaction, so every figure comes
/// from one snapshot (WI-004 DEC-015). The SQL is 003_DB's reviewed text. Commands run on the EF connection and
/// transaction through ADO.NET with bound parameters (WI-004 DEC-024): nothing is concatenated into SQL.
/// </summary>
internal sealed class DashboardReader(AppDbContext db) : IDashboardReader
{
    private const string ActiveStatuses = "('Draft', 'InProgress')";

    public async Task<DashboardRaw> ReadAsync(DashboardWindow window, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
        var connection = db.Database.GetDbConnection();
        var tx = transaction.GetDbTransaction();

        // Must be the first statement of the transaction; any write in it would now fail (REQ-039).
        await ExecuteAsync(connection, tx, "SET TRANSACTION READ ONLY", [], cancellationToken);

        var statusCounts = await QueryAsync(connection, tx,
            "SELECT status, count(*)::int FROM production_orders GROUP BY status",
            [],
            r => new StatusCountRow(r.GetString(0), r.GetInt32(1)),
            cancellationToken);

        var workload = await QueryAsync(connection, tx,
            $"""
            SELECT CASE
                     WHEN due_date <  @today       THEN -1
                     WHEN due_date >= @week0 + 56  THEN 8
                     ELSE (due_date - @week0) / 7
                   END AS bucket,
                   count(*)::int, sum(quantity)::bigint
            FROM production_orders
            WHERE status IN {ActiveStatuses}
            GROUP BY bucket
            """,
            [Param("today", window.Today), Param("week0", window.Week0)],
            r => new WorkloadRow(r.GetInt32(0), r.GetInt32(1), r.GetInt64(2)),
            cancellationToken);

        var overdue = await QueryAsync(connection, tx, GroupSql("o.due_date < @today"),
            [Param("today", window.Today), Param("limit", DashboardWindow.GroupRowLimit)], ReadOrder, cancellationToken);

        var dueSoon = await QueryAsync(connection, tx, GroupSql("o.due_date BETWEEN @today AND @soonEnd"),
            [Param("today", window.Today), Param("soonEnd", window.SoonEnd), Param("limit", DashboardWindow.GroupRowLimit)],
            ReadOrder, cancellationToken);

        var topProducts = await QueryAsync(connection, tx,
            $"""
            SELECT p.id, p.sku, p.name, sum(o.quantity)::bigint AS open_quantity, count(*)::int
            FROM production_orders o
            JOIN products p ON p.id = o.product_id
            WHERE o.status IN {ActiveStatuses}
            GROUP BY p.id, p.sku, p.name
            ORDER BY open_quantity DESC, p.sku
            LIMIT @limit
            """,
            [Param("limit", DashboardWindow.TopProductLimit)],
            r => new TopProductRow(r.GetGuid(0), r.GetString(1), r.GetString(2), r.GetInt64(3), r.GetInt32(4)),
            cancellationToken);

        var delivery = (await QueryAsync(connection, tx,
            """
            SELECT
              count(*)      FILTER (WHERE completed_at_utc >= @weekStart)::int,
              coalesce(sum(quantity) FILTER (WHERE completed_at_utc >= @weekStart), 0)::bigint,
              count(*)      FILTER (WHERE completed_at_utc >= @monthStart)::int,
              coalesce(sum(quantity) FILTER (WHERE completed_at_utc >= @monthStart), 0)::bigint,
              count(*)      FILTER (WHERE completed_at_utc >= @windowStart)::int,
              count(*)      FILTER (WHERE completed_at_utc >= @windowStart
                                      AND (completed_at_utc AT TIME ZONE @zone)::date <= due_date)::int,
              (avg(extract(epoch FROM completed_at_utc - created_at_utc) / 86400.0)
                            FILTER (WHERE completed_at_utc >= @windowStart))::double precision
            FROM production_orders
            WHERE completed_at_utc >= @trendStart
              AND completed_at_utc <  @end
            """,
            [
                Param("weekStart", window.WeekStartUtc), Param("monthStart", window.MonthStartUtc),
                Param("windowStart", window.WindowStartUtc), Param("trendStart", window.TrendStartUtc),
                Param("end", window.EndUtc), Param("zone", window.TimeZoneId),
            ],
            r => new DeliveryRow(
                r.GetInt32(0), r.GetInt64(1), r.GetInt32(2), r.GetInt64(3), r.GetInt32(4), r.GetInt32(5),
                r.IsDBNull(6) ? null : r.GetDouble(6)),
            cancellationToken)).Single();

        var trend = await QueryAsync(connection, tx,
            """
            SELECT date_trunc('week', completed_at_utc AT TIME ZONE @zone)::date AS week_start, count(*)::int
            FROM production_orders
            WHERE completed_at_utc >= @trendStart
              AND completed_at_utc <  @end
            GROUP BY week_start
            """,
            [Param("trendStart", window.TrendStartUtc), Param("end", window.EndUtc), Param("zone", window.TimeZoneId)],
            r => new TrendRow(DateOnly.FromDateTime(r.GetDateTime(0)), r.GetInt32(1)),
            cancellationToken);

        // A read-only commit only releases the snapshot.
        await transaction.CommitAsync(cancellationToken);
        return new DashboardRaw(statusCounts, workload, overdue, dueSoon, topProducts, delivery, trend);
    }

    private static string GroupSql(string predicate) =>
        $"""
        SELECT o.id, o.order_number, o.quantity, o.due_date, o.status, p.id, p.sku, p.name,
               (count(*) OVER ())::int AS total
        FROM production_orders o
        JOIN products p ON p.id = o.product_id
        WHERE o.status IN {ActiveStatuses}
          AND {predicate}
        ORDER BY o.due_date, o.order_number
        LIMIT @limit
        """;

    private static DashboardOrderRow ReadOrder(DbDataReader r) =>
        new(
            r.GetGuid(0),
            r.GetString(1),
            r.GetInt32(2),
            DateOnly.FromDateTime(r.GetDateTime(3)),
            r.GetString(4),
            r.GetGuid(5),
            r.GetString(6),
            r.GetString(7),
            r.GetInt32(8));

    private static NpgsqlParameter Param(string name, object value) => new(name, value);

    private static async Task ExecuteAsync(
        DbConnection connection, DbTransaction tx, string sql, NpgsqlParameter[] parameters, CancellationToken ct)
    {
        await using var command = CreateCommand(connection, tx, sql, parameters);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task<IReadOnlyList<T>> QueryAsync<T>(
        DbConnection connection,
        DbTransaction tx,
        string sql,
        NpgsqlParameter[] parameters,
        Func<DbDataReader, T> map,
        CancellationToken ct)
    {
        await using var command = CreateCommand(connection, tx, sql, parameters);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var rows = new List<T>();
        while (await reader.ReadAsync(ct))
        {
            rows.Add(map(reader));
        }

        return rows;
    }

    private static DbCommand CreateCommand(DbConnection connection, DbTransaction tx, string sql, NpgsqlParameter[] parameters)
    {
        var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        return command;
    }
}
