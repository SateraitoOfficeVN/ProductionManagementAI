using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Infrastructure.ProductionLines;

/// <summary>Maps line identity, exact numeric values and aggregate xmin.</summary>
internal sealed class ProductionLineConfiguration : IEntityTypeConfiguration<ProductionLine>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductionLine> b)
    {
        b.ToTable("production_lines", t =>
        {
            t.HasCheckConstraint("ck_production_lines_code_trimmed", "code = btrim(code) AND length(code) > 0");
            t.HasCheckConstraint("ck_production_lines_name_trimmed", "name = btrim(name) AND length(name) > 0");
            t.HasCheckConstraint("ck_production_lines_hours_range", "working_hours_per_day > 0 AND working_hours_per_day <= 24");
            t.HasCheckConstraint("ck_production_lines_hours_scale", "scale(working_hours_per_day) <= 3");
        });
        b.HasKey(x => x.Id).HasName("pk_production_lines");
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.WorkingHoursPerDay).HasColumnType("numeric");
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("now()");
        b.Property(x => x.RowVersion).IsRowVersion();
        // The lower(code) unique expression is installed explicitly by the owner migration.
    }
}

/// <summary>Maps durable pair identity and exact unit-confirmed timing.</summary>
internal sealed class ProductionLineProductConfiguration : IEntityTypeConfiguration<ProductionLineProduct>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductionLineProduct> b)
    {
        b.ToTable("production_line_products", t =>
        {
            t.HasCheckConstraint("ck_line_products_minutes_range", "minutes_per_unit > 0 AND minutes_per_unit <= 999999999.999");
            t.HasCheckConstraint("ck_line_products_minutes_scale", "scale(minutes_per_unit) <= 3");
            t.HasCheckConstraint("ck_line_products_confirmed_unit", "confirmed_unit IN ('個','本','枚','台','セット','kg','m')");
            t.HasCheckConstraint("ck_line_products_confirmed_revision", "confirmed_unit_revision >= 0");
        });
        b.HasKey(x => new { x.LineId, x.ProductId }).HasName("pk_production_line_products");
        b.Property(x => x.MinutesPerUnit).HasColumnType("numeric");
        b.Property(x => x.ConfirmedUnit).HasMaxLength(20).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("now()");
        b.HasOne<ProductionLine>().WithMany().HasForeignKey(x => x.LineId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_line_products_line");
        b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_line_products_product");
        b.HasIndex(x => new { x.ProductId, x.LineId }).HasDatabaseName("ix_line_products_product_line");
    }
}
