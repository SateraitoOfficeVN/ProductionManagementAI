using ProductionManagementAI.Application.PlantCalendar;
using ProductionManagementAI.Domain.PlantCalendar;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.ProductionOrders;

namespace ProductionManagementAI.Application.Tests.PlantCalendar;

/// <summary>Verifies exact calendar boundaries, whole-winner precedence and conservative unit arithmetic.</summary>
public sealed class CalendarRulesTests
{
    [Theory]
    [InlineData("0001-01-01", true)] [InlineData("9999-12-31", true)]
    [InlineData("2024-02-29", true)] [InlineData("2026-02-29", false)]
    [InlineData("2026-2-01", false)] [InlineData("2026-01-01 ", false)]
    [InlineData("2026-01-01T00:00:00", false)] [InlineData("10000-01-01", false)]
    public void ExactDates(string text, bool valid) => Assert.Equal(valid, CalendarValueRules.TryDate(text, out _));

    [Theory]
    [InlineData(" 0.001 ", true)] [InlineData("24", true)] [InlineData("24.000", true)]
    [InlineData("0", false)] [InlineData("24.001", false)] [InlineData("1.0000", false)]
    [InlineData("+1", false)] [InlineData("01", false)] [InlineData("1e0", false)]
    [InlineData(".5", false)] [InlineData("1,5", false)] [InlineData("NaN", false)]
    public void ExactHours(string text, bool valid) => Assert.Equal(valid, CalendarValueRules.TryHours(text, out _));

    [Theory]
    [InlineData("1", true)] [InlineData("9007199254740993", true)] [InlineData("9223372036854775807", true)]
    [InlineData("0", false)] [InlineData("01", false)] [InlineData("+1", false)] [InlineData(" 1", false)]
    [InlineData("9223372036854775808", false)]
    public void OpaqueVersion(string text, bool valid) => Assert.Equal(valid, CalendarValueRules.TryVersion(text, out _));

    [Fact]
    public void ReasonCountsUnicodeAndRejectsMalformedText()
    {
        var text = string.Concat(Enumerable.Repeat("😀", 500));
        Assert.True(CalendarValueRules.TryText(" \u0085" + text + " ", 500, out var normalized));
        Assert.Equal(text, normalized);
        Assert.False(CalendarValueRules.TryText(text + "a", 500, out _));
        Assert.False(CalendarValueRules.TryText("\ud800", 500, out _));
        Assert.True(CalendarValueRules.TryText(" a  b ", 500, out normalized)); Assert.Equal("a  b", normalized);
        Assert.True(CalendarValueRules.TryText(" \t ", 500, out normalized)); Assert.Null(normalized);
    }

    [Fact]
    public void AllClosedAndEveryWeekdayMasksAreExact()
    {
        Assert.True(CalendarValueRules.TryMask([], out var empty)); Assert.Equal(0, empty);
        Assert.True(CalendarValueRules.TryMask(CalendarValueRules.Weekdays, out var all)); Assert.Equal(127, all);
        Assert.False(CalendarValueRules.TryMask(["Mon", "Mon"], out _));
        Assert.False(CalendarValueRules.TryMask(["monday"], out _));
    }

    [Theory]
    [InlineData("4", "0.125", "個", "1920")]
    [InlineData("0.001", "0.007", "個", "8")]
    [InlineData("0.001", "0.007", "kg", "8.571")]
    [InlineData("1", "60.001", "個", "0")]
    [InlineData("0.001", "60", "m", "0.001")]
    [InlineData("0.001", "60.001", "m", "0")]
    [InlineData("24", "0.001", "kg", "1440000")]
    [InlineData("0", "0.125", "本", "0")]
    [InlineData("1", "7", "枚", "8")]
    [InlineData("1", "7", "台", "8")]
    [InlineData("1", "7", "セット", "8")]
    public void ExactConservativeFloor(string h, string t, string unit, string expected) => Assert.Equal(expected,
        CalendarResolver.FloorCapacity(decimal.Parse(h, System.Globalization.CultureInfo.InvariantCulture), decimal.Parse(t, System.Globalization.CultureInfo.InvariantCulture), unit));

    [Fact]
    public void PersistedInvalidInputsAreNotClamped()
    {
        Assert.Throws<InvalidOperationException>(() => CalendarResolver.FloorCapacity(24.001m, 1m, "個"));
        Assert.Throws<InvalidOperationException>(() => CalendarResolver.FloorCapacity(1m, 0m, "個"));
        Assert.Throws<InvalidOperationException>(() => CalendarResolver.FloorCapacity(1m, 1m, "invalid"));
        Assert.Throws<InvalidOperationException>(() => CalendarResolver.FloorCapacity(1.0000m, 1m, "個"));
    }

    private static readonly DateOnly Date = new(2026, 10, 12);
    private static readonly CalendarContext Context = new(new(2026, 10, 2), "Asia/Tokyo", new(2026, 10, 2), "1");
    private static ProductionLine Line(bool active = true) => new() { Id = Guid.NewGuid(), Code = "L-001", Name = "Line", WorkingHoursPerDay = 8m, IsActive = active };
    private static WeeklyRevision Weekly(short? mask = 31, bool withdrawn = false) => new() { Id = Guid.NewGuid(), EffectiveFrom = new(2026, 10, 2), WorkingWeekdays = mask, IsWithdrawn = withdrawn, CommitRevision = 1 };
    private static ExceptionRevision Exception(Guid? lineId, bool working, decimal? hours = null) => new() { Id = Guid.NewGuid(), LineId = lineId, CalendarDate = Date, IsWorking = working, WorkingHours = hours, CommitRevision = 2 };

    [Fact]
    public void LineReopensPlantClosureAndWinsWholly()
    {
        var line = Line(); var plant = Exception(null, false); var scoped = Exception(line.Id, true, 4m);
        var day = CalendarResolver.ResolveDay(Date, Context, line, Weekly(), plant, scoped);
        Assert.Equal("Working", day.State); Assert.Equal("4", day.Hours); Assert.Equal("Explicit", day.HoursBasis);
        Assert.Equal("LineException", day.Source.Kind); Assert.Equal(new[] { "PlantException", "Weekly" }, day.Fallback.Select(x => x.Kind));
        var inherited = CalendarResolver.ResolveDay(Date, Context, line, Weekly(), Exception(null, true, 2m), Exception(line.Id, true));
        Assert.Equal("8", inherited.Hours); Assert.Equal("LineCurrent", inherited.HoursBasis);
    }

    [Fact]
    public void ClosedUnavailableAndPlantDependentAreDistinct()
    {
        var closed = CalendarResolver.ResolveDay(Date, Context, null, Weekly(0), null, null);
        Assert.Equal("Closed", closed.State); Assert.Equal("0", closed.Hours);
        var dependent = CalendarResolver.ResolveDay(Date, Context, null, Weekly(), null, null);
        Assert.Equal("Working", dependent.State); Assert.Null(dependent.Hours); Assert.Equal("LineDependent", dependent.HoursBasis);
        var unavailable = CalendarResolver.ResolveDay(new(2026, 10, 1), Context, null, Weekly(), null, null);
        Assert.Equal("Unavailable", unavailable.State); Assert.Null(unavailable.Hours); Assert.False(unavailable.Editable);
        Assert.Equal("Unavailable", CalendarResolver.ResolveDay(Date, Context with { ActivatedOn = null, Version = null }, null, Weekly(), null, null).State);
    }

    [Fact]
    public void RemovedHeadAndWithdrawalDoNotWin()
    {
        var removed = Exception(null, true, 4m);
        removed = new ExceptionRevision { Id = removed.Id, CalendarDate = Date, IsRemoved = true, IsCurrent = true, CommitRevision = 3 };
        var day = CalendarResolver.ResolveDay(Date, Context, null, Weekly(), removed, null);
        Assert.Equal("Weekly", day.Source.Kind);
        Assert.Equal("Unavailable", CalendarResolver.ResolveDay(Date, Context, null, Weekly(null, true), null, null).State);
    }

    [Fact]
    public void RetiredAndPastRestrictionsKeepReadableRules()
    {
        var line = Line(false); var day = CalendarResolver.ResolveDay(Date, Context, line, Weekly(), null, null);
        Assert.Equal("Working", day.State); Assert.False(day.Editable);
        day = CalendarResolver.ResolveDay(Date, Context with { PlantToday = Date.AddDays(1) }, Line(), Weekly(), null, null);
        Assert.False(day.Editable); Assert.Equal("LineCurrent", day.HoursBasis);
    }

    [Fact]
    public void EligibilityPriorityChecksGenerationEvenWhenUnitChangesBack()
    {
        var line = Line(); var product = new Product { Id = Guid.NewGuid(), Sku = "P", Name = "Part", Unit = "個" };
        var day = CalendarResolver.ResolveDay(Date, Context, line, Weekly(0), null, null);
        Assert.Equal("PairMissing", CalendarResolver.UnavailableReason(Context, line, product, null, day));
        var pair = new ProductionLineProduct { LineId = line.Id, ProductId = product.Id, MinutesPerUnit = 1m, ConfirmedUnit = "個", ConfirmedUnitRevision = 1 };
        Assert.Equal("UnitStale", CalendarResolver.UnavailableReason(Context, line, product, pair, day));
        pair.ConfirmedUnitRevision = 0; Assert.Equal("None", CalendarResolver.UnavailableReason(Context, line, product, pair, day));
        pair.IsActive = false; Assert.Equal("PairRetired", CalendarResolver.UnavailableReason(Context, line, product, pair, day));
        product.IsActive = false; Assert.Equal("ProductRetired", CalendarResolver.UnavailableReason(Context, line, product, pair, day));
        line.IsActive = false; Assert.Equal("LineRetired", CalendarResolver.UnavailableReason(Context, line, product, pair, day));
        Assert.Equal("NotActivated", CalendarResolver.UnavailableReason(Context with { ActivatedOn = null }, line, product, pair, day));
    }
}
