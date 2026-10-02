using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionManagementAI.Domain.PlantCalendar;
using ProductionManagementAI.Domain.ProductionLines;

namespace ProductionManagementAI.Infrastructure.PlantCalendar;

/// <summary>Maps the owner-activated singleton and its restricted mutable columns.</summary>
public sealed class CalendarStateConfiguration : IEntityTypeConfiguration<CalendarState>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<CalendarState> b)
    {
        b.ToTable("plant_calendar_state", t => {
            t.HasCheckConstraint("ck_calendar_state_singleton", "id = 1");
            t.HasCheckConstraint("ck_calendar_state_revision", "revision >= 1");
            t.HasCheckConstraint("ck_calendar_state_zone", "time_zone_id = btrim(time_zone_id) AND length(time_zone_id) > 0");
            t.HasCheckConstraint("ck_calendar_state_date", "activated_on BETWEEN DATE '0001-01-01' AND DATE '9999-12-31'");
        });
        b.HasKey(x => x.Id).HasName("pk_plant_calendar_state");
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.TimeZoneId).HasMaxLength(100);
        b.Property(x => x.Revision).HasDefaultValue(1L);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("now()");
    }
}

/// <summary>Maps immutable weekly snapshots and current-head uniqueness.</summary>
public sealed class WeeklyRevisionConfiguration : IEntityTypeConfiguration<WeeklyRevision>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<WeeklyRevision> b)
    {
        b.ToTable("plant_calendar_weekly_revisions", t => {
            t.HasCheckConstraint("ck_calendar_weekly_date", "effective_from BETWEEN DATE '0001-01-01' AND DATE '9999-12-31'");
            t.HasCheckConstraint("ck_calendar_weekly_payload", "(is_withdrawn AND working_weekdays IS NULL) OR (NOT is_withdrawn AND working_weekdays IS NOT NULL AND working_weekdays BETWEEN 0 AND 127)");
            t.HasCheckConstraint("ck_calendar_weekly_revision", "commit_revision >= 1");
        });
        b.HasKey(x => x.Id).HasName("pk_calendar_weekly_revisions");
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.IsWithdrawn).HasDefaultValue(false);
        b.Property(x => x.IsCurrent).HasDefaultValue(true);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()");
        b.HasOne<CalendarState>().WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_calendar_weekly_state");
        b.HasIndex(x => new { x.CalendarId, x.EffectiveFrom }).IsDescending(false, true).IsUnique().HasFilter("is_current").HasDatabaseName("ux_calendar_weekly_current");
        b.HasIndex(x => new { x.CalendarId, x.CommitRevision }).IsUnique().HasDatabaseName("ux_calendar_weekly_commit");
    }
}

/// <summary>Maps retained plant-null and line date exceptions without numeric typmod rounding.</summary>
public sealed class ExceptionRevisionConfiguration : IEntityTypeConfiguration<ExceptionRevision>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ExceptionRevision> b)
    {
        b.ToTable("plant_calendar_exception_revisions", t => {
            t.HasCheckConstraint("ck_calendar_exception_date", "calendar_date BETWEEN DATE '0001-01-01' AND DATE '9999-12-31'");
            t.HasCheckConstraint("ck_calendar_exception_payload", "(is_removed AND is_working IS NULL AND working_hours IS NULL AND reason IS NULL) OR (NOT is_removed AND is_working IS NOT NULL AND (is_working OR working_hours IS NULL))");
            t.HasCheckConstraint("ck_calendar_exception_hours", "working_hours IS NULL OR (working_hours > 0 AND working_hours <= 24 AND scale(working_hours) <= 3)");
            t.HasCheckConstraint("ck_calendar_exception_revision", "commit_revision >= 1");
        });
        b.HasKey(x => x.Id).HasName("pk_calendar_exception_revisions");
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.WorkingHours).HasColumnType("numeric");
        b.Property(x => x.Reason).HasMaxLength(500);
        b.Property(x => x.IsRemoved).HasDefaultValue(false);
        b.Property(x => x.IsCurrent).HasDefaultValue(true);
        b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()");
        b.HasOne<CalendarState>().WithMany().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_calendar_exception_state");
        b.HasOne<ProductionLine>().WithMany().HasForeignKey(x => x.LineId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_calendar_exception_line");
        b.HasIndex(x => new { x.CalendarId, x.LineId, x.CalendarDate }).IsUnique().AreNullsDistinct(false).HasFilter("is_current").HasDatabaseName("ux_calendar_exception_current");
        b.HasIndex(x => new { x.CalendarId, x.CommitRevision }).IsUnique().HasDatabaseName("ux_calendar_exception_commit");
        b.HasIndex(x => x.LineId).HasFilter("line_id IS NOT NULL").HasDatabaseName("ix_calendar_exception_line");
    }
}
