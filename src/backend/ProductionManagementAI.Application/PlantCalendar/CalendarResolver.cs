using System.Globalization;
using ProductionManagementAI.Domain.PlantCalendar;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;
using ProductionManagementAI.Application.ProductionLines;

namespace ProductionManagementAI.Application.PlantCalendar;

/// <summary>Resolves whole-winner calendar rules and exact conservative current capacity.</summary>
public static class CalendarResolver
{
    private sealed record Candidate(CalendarSource Source, bool Working, decimal? Hours);

    /// <summary>Resolves one captured day using line, plant and weekly precedence.</summary>
    public static DaySummary ResolveDay(DateOnly date, CalendarContext context, ProductionLine? line,
        WeeklyRevision? weekly, ExceptionRevision? plant, ExceptionRevision? lineException)
    {
        var none = new CalendarSource("None", null, null, null);
        if (context.ActivatedOn is not { } activation || date < activation)
            return new(date, "Unavailable", null, "Unavailable", none, [], false);
        var candidates = new List<Candidate>();
        void Add(ExceptionRevision? item, string kind)
        {
            if (item is not { IsCurrent: true, IsRemoved: false } || item.CalendarDate != date) return;
            if (item.IsWorking is null || (item.IsWorking == false && item.WorkingHours is not null))
                throw new InvalidOperationException("Invalid retained calendar payload.");
            candidates.Add(new(new(kind, item.Id, null, item.Reason), item.IsWorking.Value, item.WorkingHours));
        }
        if (line is not null && lineException?.LineId == line.Id) Add(lineException, "LineException");
        if (plant?.LineId is null) Add(plant, "PlantException");
        if (weekly is { IsCurrent: true, IsWithdrawn: false } && weekly.EffectiveFrom <= date)
        {
            if (weekly.WorkingWeekdays is not >= 0 or > 127) throw new InvalidOperationException("Invalid retained weekday mask.");
            var bit = ((int)date.DayOfWeek + 6) % 7;
            candidates.Add(new(new("Weekly", weekly.Id, weekly.EffectiveFrom, null), (weekly.WorkingWeekdays.Value & (1 << bit)) != 0, null));
        }
        if (candidates.Count == 0) return new(date, "Unavailable", null, "Unavailable", none, [], false);
        var winner = candidates[0];
        string? hours = null;
        var basis = "LineDependent";
        if (!winner.Working) { hours = "0"; basis = "Closed"; }
        else if (winner.Hours is { } explicitHours)
        {
            if (!CalendarValueRules.ValidDecimal(explicitHours, 24)) throw new InvalidOperationException("Invalid persisted hours.");
            hours = CalendarValueRules.Format(explicitHours); basis = "Explicit";
        }
        else if (line is not null)
        {
            if (!CalendarValueRules.ValidDecimal(line.WorkingHoursPerDay, 24)) throw new InvalidOperationException("Invalid line hours.");
            hours = CalendarValueRules.Format(line.WorkingHoursPerDay); basis = "LineCurrent";
        }
        return new(date, winner.Working ? "Working" : "Closed", hours, basis, winner.Source,
            candidates.Skip(1).Select(c => c.Source).ToArray(), date >= context.PlantToday && (line is null || line.IsActive));
    }

    /// <summary>Floors an exact ratio through scaled integer division, without intermediate rounding.</summary>
    public static string FloorCapacity(decimal hours, decimal minutes, string unit)
    {
        if (!CalendarValueRules.ValidDecimal(hours, 24, true) || !CalendarValueRules.ValidDecimal(minutes, 999999999.999m) || !LineValueRules.Units.Contains(unit))
            throw new InvalidOperationException("Invalid capacity inputs.");
        var h = checked((long)(hours * 1000m));
        var t = checked((long)(minutes * 1000m));
        var n = checked(h * 60L);
        if (unit is "kg" or "m") return CalendarValueRules.Format((checked(n * 1000L) / t) / 1000m);
        return (n / t).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Returns the first unavailable condition in the approved eligibility order.</summary>
    public static string UnavailableReason(CalendarContext context, ProductionLine line, Product product, ProductionLineProduct? pair, DaySummary day)
    {
        if (context.ActivatedOn is null) return "NotActivated";
        if (!line.IsActive) return "LineRetired";
        if (!product.IsActive) return "ProductRetired";
        if (pair is null) return "PairMissing";
        if (!pair.IsActive) return "PairRetired";
        if (pair.ConfirmedUnit != product.Unit || pair.ConfirmedUnitRevision != product.UnitRevision) return "UnitStale";
        return day.State == "Unavailable" ? "CalendarUnavailable" : "None";
    }
}
