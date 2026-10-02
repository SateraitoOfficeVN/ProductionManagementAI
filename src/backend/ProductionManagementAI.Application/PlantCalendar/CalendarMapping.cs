using System.Globalization;
using ProductionManagementAI.Domain.PlantCalendar;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.PlantCalendar;

/// <summary>Projects retained entities to exact response contracts.</summary>
public static class CalendarMapping
{
    /// <summary>Projects a line without its edit token.</summary>
    public static CalendarLine Line(ProductionLine line) => new(line.Id, line.Code, line.Name, line.IsActive, CalendarValueRules.Format(line.WorkingHoursPerDay));
    /// <summary>Projects a product with its unit generation.</summary>
    public static CalendarProduct Product(Product product) => new(product.Id, product.Sku, product.Name, product.Unit, product.UnitRevision.ToString(CultureInfo.InvariantCulture));
    /// <summary>Projects a weekly head or retained marker.</summary>
    public static WeeklySnapshot Weekly(WeeklyRevision row) => new(row.Id, row.EffectiveFrom,
        row.WorkingWeekdays is { } mask ? CalendarValueRules.Weekdays.Where((_, i) => (mask & (1 << i)) != 0).ToArray() : null,
        row.IsWithdrawn, row.IsCurrent, row.CommitRevision.ToString(CultureInfo.InvariantCulture), row.CreatedAtUtc);
    /// <summary>Projects an exception payload or marker.</summary>
    public static ExceptionSnapshot Exception(ExceptionRevision row) => new(row.Id, row.LineId, row.CalendarDate, row.IsWorking,
        row.WorkingHours is { } hours ? CalendarValueRules.Format(hours) : null, row.Reason, row.IsRemoved, row.IsCurrent,
        row.CommitRevision.ToString(CultureInfo.InvariantCulture), row.CreatedAtUtc);
}
