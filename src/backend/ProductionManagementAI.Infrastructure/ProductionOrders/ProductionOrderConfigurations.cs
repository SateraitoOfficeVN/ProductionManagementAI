using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.ProductionOrders;

// Physical schema per docs/en/database/001/001_DB_製造指示登録・編集.md (001_DB). Column names are snake-cased
// by EFCore.NamingConventions; constraint names are set explicitly to match the document.

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id).HasName("pk_products");
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Sku).HasMaxLength(50);
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.CreatedAtUtc).HasDefaultValueSql("now()");
        builder.HasIndex(p => p.Sku).IsUnique().HasDatabaseName("ix_products_sku");
        builder.HasData(ProductSeed.All);
    }
}

internal sealed class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("production_orders", table =>
        {
            table.HasCheckConstraint("ck_production_orders_quantity_positive", "quantity > 0");
            table.HasCheckConstraint(
                "ck_production_orders_status", "status IN ('Draft', 'InProgress', 'Completed', 'Cancelled')");
            table.HasCheckConstraint("ck_production_orders_order_seq_range", "order_seq BETWEEN 1 AND 99999");
            table.HasCheckConstraint("ck_production_orders_order_year_range", "order_year BETWEEN 2000 AND 9999");

            // 003_DB: a completed order always has a completion time and no other order has one; and no lead time
            // can be negative, whatever wrote the row.
            table.HasCheckConstraint(
                "ck_production_orders_completed_at_matches_status",
                "(status = 'Completed') = (completed_at_utc IS NOT NULL)");
            table.HasCheckConstraint(
                "ck_production_orders_completed_at_not_before_created",
                "completed_at_utc IS NULL OR completed_at_utc >= created_at_utc");
        });

        builder.HasKey(o => o.Id).HasName("pk_production_orders");
        builder.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");

        // Explicit ::text casts: generation expressions must be immutable (001_DB operational note).
        builder.Property(o => o.OrderNumber)
            .HasMaxLength(13)
            .HasComputedColumnSql("'PO-' || order_year::text || '-' || lpad(order_seq::text, 5, '0')", stored: true);

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(ProductionOrderStatus.Draft)
            .HasSentinel((ProductionOrderStatus)(-1)); // always send the value; the DB default is for manual inserts only
        builder.Property(o => o.Notes).HasMaxLength(ProductionOrder.MaxNotesLength);
        builder.Property(o => o.CreatedAtUtc).HasDefaultValueSql("now()");
        builder.Property(o => o.UpdatedAtUtc).HasDefaultValueSql("now()");

        // PostgreSQL xmin as the optimistic-concurrency token (DEC-014); no declared column.
        builder.Property(o => o.RowVersion).IsRowVersion();

        builder.Ignore(o => o.IsProductQuantityEditable);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_production_orders_products_product_id");

        // EF's foreign-key convention always indexes product_id (it re-creates the index if removed); kept per DEC-029.
        builder.HasIndex(o => o.ProductId).HasDatabaseName("ix_production_orders_product_id");

        builder.HasIndex(o => new { o.OrderYear, o.OrderSeq })
            .IsUnique()
            .HasDatabaseName("ix_production_orders_order_year_order_seq");

        // 002_DB (Screen B): the default sort and the due-date range in one scan, with the unique order number as
        // the tie-breaker that keeps paging stable.
        builder.HasIndex(o => new { o.DueDate, o.OrderNumber })
            .HasDatabaseName("ix_production_orders_due_date_order_number");

        // The order-number filter is a "contains" match, which no btree can serve (DEC-010).
        builder.HasIndex(o => o.OrderNumber)
            .HasDatabaseName("ix_production_orders_order_number_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        // 003_DB (Screen C): the open workload stays small while history grows, so the active-order queries read a
        // partial index in due-date order; the delivery figures read recent completions only.
        // Named overload: the same columns as 002_DB's index, so an unnamed HasIndex would reconfigure that one.
        builder.HasIndex(o => new { o.DueDate, o.OrderNumber }, "ix_production_orders_active_due_date")
            .HasDatabaseName("ix_production_orders_active_due_date")
            .HasFilter("status IN ('Draft', 'InProgress')");
        builder.HasIndex(o => o.CompletedAtUtc, "ix_production_orders_completed_at_utc")
            .HasDatabaseName("ix_production_orders_completed_at_utc")
            .HasFilter("completed_at_utc IS NOT NULL");
    }
}

/// <summary>Per-year counter used to issue order numbers (001_DB, DEC-013). Infrastructure-only; no domain meaning.</summary>
internal sealed class ProductionOrderNumberCounter
{
    public short OrderYear { get; set; }

    public int LastSeq { get; set; }
}

internal sealed class ProductionOrderNumberCounterConfiguration : IEntityTypeConfiguration<ProductionOrderNumberCounter>
{
    public void Configure(EntityTypeBuilder<ProductionOrderNumberCounter> builder)
    {
        builder.ToTable("production_order_number_counters", table =>
            table.HasCheckConstraint(
                "ck_production_order_number_counters_last_seq_range", "last_seq BETWEEN 1 AND 99999"));
        builder.HasKey(c => c.OrderYear).HasName("pk_production_order_number_counters");
        builder.Property(c => c.OrderYear).ValueGeneratedNever();
    }
}
