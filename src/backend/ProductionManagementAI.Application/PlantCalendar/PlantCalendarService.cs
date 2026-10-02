using System.Diagnostics;
using System.Globalization;
using ProductionManagementAI.Domain.ProductionLines;
using ProductionManagementAI.Domain.PlantCalendar;

namespace ProductionManagementAI.Application.PlantCalendar;

/// <summary>Coordinates eleven typed calendar use cases without storage or HTTP coupling.</summary>
public sealed class PlantCalendarService(ICalendarRepository repository)
{
    /// <summary>Reads the real dates and batched rule heads in one month.</summary>
    public Task<CalendarResult<MonthResult>> MonthAsync(CalendarReadQuery q, CancellationToken ct) => Read("Month", q, async (r, context, token) =>
    {
        var first = new DateOnly(q.Date.Year, q.Date.Month, 1);
        var last = new DateOnly(first.Year, first.Month, DateTime.DaysInMonth(first.Year, first.Month));
        var line = await Line(r, q.LineId, token);
        var weekly = await r.ApplicableWeeklyAsync(first, token);
        var starts = await r.MonthWeeklyAsync(first, last, token);
        var exceptions = await r.MonthExceptionsAsync(first, last, q.LineId, token);
        var heads = exceptions.ToDictionary(x => (x.LineId, x.CalendarDate));
        var days = new List<DaySummary>();
        var index = 0;
        for (var number = 1; number <= last.Day; number++)
        {
            var date = new DateOnly(first.Year, first.Month, number);
            while (index < starts.Count && starts[index].EffectiveFrom <= date) weekly = starts[index++];
            heads.TryGetValue((null, date), out var plant);
            heads.TryGetValue((q.LineId, date), out var scoped);
            days.Add(ResolveDay(date, context, line, weekly, plant, q.LineId is null ? null : scoped));
        }
        return new MonthResult(context, Scope(q.LineId, line), first.ToString("yyyy-MM", CultureInfo.InvariantCulture), days);
    }, ct);

    /// <summary>Reads an exact date and its scope target, including a removal marker.</summary>
    public Task<CalendarResult<DayResult>> DayAsync(CalendarReadQuery q, CancellationToken ct) => Read("Day", q, async (r, context, token) =>
    {
        var line = await Line(r, q.LineId, token);
        var weekly = await r.ApplicableWeeklyAsync(q.Date, token);
        var plant = await r.ExceptionAsync(q.Date, null, token);
        var target = q.LineId is null ? plant : await r.ExceptionAsync(q.Date, q.LineId, token);
        return new DayResult(context, Scope(q.LineId, line), ResolveDay(q.Date, context, line, weekly, plant, q.LineId is null ? null : target),
            target is null ? null : CalendarMapping.Exception(target));
    }, ct);

    /// <summary>Reads a bounded version-bound range and its applicable predecessor.</summary>
    public Task<CalendarResult<WeeklyPage>> WeeklyAsync(CalendarReadQuery q, CancellationToken ct) => Read("Weekly", q, async (r, context, token) =>
    {
        if (q.From > q.To || q.To.DayNumber - q.From.DayNumber >= 366 || q.View is not ("Current" or "History")) Invalid();
        Snapshot(q, context);
        var page = await r.WeeklyPageAsync(q, token);
        var predecessor = q.From == DateOnly.MinValue ? null : await r.ApplicableWeeklyAsync(q.From.AddDays(-1), token);
        return new WeeklyPage(context, context.Version, q.Page, 20, page.Total, Pages(page.Total, 20),
            page.Items.Select(CalendarMapping.Weekly).ToArray(), predecessor is null ? null : CalendarMapping.Weekly(predecessor));
    }, ct);

    /// <summary>Saves a retained weekly definition.</summary>
    public Task<CalendarResult<MutationResult>> SaveWeeklyAsync(CalendarCommand command, CancellationToken ct) => Write("SaveWeekly", command with { Kind = "SaveWeekly" }, ct);
    /// <summary>Withdraws a future current weekly definition.</summary>
    public Task<CalendarResult<MutationResult>> WithdrawWeeklyAsync(CalendarCommand command, CancellationToken ct) => Write("WithdrawWeekly", command with { Kind = "WithdrawWeekly" }, ct);
    /// <summary>Saves one date exception with exact identity and revision.</summary>
    public Task<CalendarResult<MutationResult>> SaveExceptionAsync(CalendarCommand command, CancellationToken ct) => Write("SaveException", command with { Kind = "SaveException" }, ct);
    /// <summary>Removes one existing exception while retaining its payload.</summary>
    public Task<CalendarResult<MutationResult>> RemoveExceptionAsync(CalendarCommand command, CancellationToken ct) => Write("RemoveException", command with { Kind = "RemoveException" }, ct);

    /// <summary>Reads a bounded active-line choice page.</summary>
    public Task<CalendarResult<LineChoicePage>> LineChoicesAsync(CalendarReadQuery q, CancellationToken ct) => Read("LineChoices", q, async (r, context, token) =>
    {
        var page = await r.LineChoicesAsync(q, token);
        return new LineChoicePage(context, q.Page, 50, page.Total, Pages(page.Total, 50), page.Items.Select(CalendarMapping.Line).ToArray());
    }, ct);

    /// <summary>Reads a bounded current eligible-product choice page.</summary>
    public Task<CalendarResult<ProductChoicePage>> ProductChoicesAsync(CalendarReadQuery q, CancellationToken ct) => Read("ProductChoices", q, async (r, context, token) =>
    {
        var line = await Line(r, q.LineId, token);
        if (line is null) Invalid();
        var page = await r.ProductChoicesAsync(q, token);
        return new ProductChoicePage(context, q.Page, 50, page.Total, Pages(page.Total, 50), page.Items.Select(CalendarMapping.Product).ToArray());
    }, ct);

    /// <summary>Reads descriptive current capacity with exact unit eligibility and floor.</summary>
    public Task<CalendarResult<CapacityResult>> CapacityAsync(CalendarReadQuery q, CancellationToken ct) => Read("Capacity", q, async (r, context, token) =>
    {
        if (q.LineId is not { } lineId || q.ProductId is not { } productId || q.Date < context.PlantToday) { Invalid(); throw new UnreachableException(); }
        var line = await r.LineAsync(lineId, token) ?? throw Missing();
        var product = await r.ProductAsync(productId, token) ?? throw Missing();
        var pair = await r.PairAsync(lineId, productId, token);
        var day = ResolveDay(q.Date, context, line, await r.ApplicableWeeklyAsync(q.Date, token),
            await r.ExceptionAsync(q.Date, null, token), await r.ExceptionAsync(q.Date, lineId, token));
        var reason = CalendarResolver.UnavailableReason(context, line, product, pair, day);
        string? quantity = null, minutes = null;
        if (reason == "None" && pair is not null && day.Hours is not null)
        {
            var hours = decimal.Parse(day.Hours, CultureInfo.InvariantCulture);
            quantity = CalendarResolver.FloorCapacity(hours, pair.MinutesPerUnit, product.Unit);
            minutes = CalendarValueRules.Format(pair.MinutesPerUnit);
        }
        return new CapacityResult(context, CalendarMapping.Line(line), CalendarMapping.Product(product), q.Date,
            reason == "None" ? "Available" : "Unavailable", reason, day, minutes, quantity);
    }, ct);

    /// <summary>Reads retained history without proving the originating client's commit.</summary>
    public Task<CalendarResult<ExceptionPage>> ExceptionHistoryAsync(CalendarReadQuery q, CancellationToken ct) => Read("ExceptionHistory", q, async (r, context, token) =>
    {
        Snapshot(q, context);
        var line = await Line(r, q.LineId, token);
        var page = await r.ExceptionPageAsync(q, token);
        var head = await r.ExceptionAsync(q.Date, q.LineId, token);
        return new ExceptionPage(context, Scope(q.LineId, line), q.Date, context.Version, q.Page, 20, page.Total, Pages(page.Total, 20),
            page.Items.Select(CalendarMapping.Exception).ToArray(), head is null ? null : CalendarMapping.Exception(head));
    }, ct);

    private async Task<CalendarResult<T>> Read<T>(string operation, CalendarReadQuery q,
        Func<ICalendarReader, CalendarContext, CancellationToken, Task<T>> project, CancellationToken ct) where T : class
    {
        using var activity = CalendarTelemetry.Source.StartActivity("plant-calendar." + operation);
        try
        {
            if (q.Page is < 1 or > 10000) Invalid();
            if (!CalendarValueRules.TryText(q.Search, 100, out var search)) Invalid();
            q = q with { Search = search };
            var result = await repository.ReadSnapshotAsync((r, context, token) => project(r, context, token), ct);
            return new(result, null);
        }
        catch (CalendarRuleException e) { return new(null, e.Failure with { WriteOutcome = null }); }
        catch (OperationCanceledException) { return new(null, new("CALENDAR_BUSY", 503)); }
        catch (Exception) { return new(null, new("UNEXPECTED", 500)); }
    }

    private async Task<CalendarResult<MutationResult>> Write(string operation, CalendarCommand command, CancellationToken ct)
    {
        using var activity = CalendarTelemetry.Source.StartActivity("plant-calendar." + operation);
        try
        {
            if (command.Version <= 0 || command.TargetRevisionId == Guid.Empty || command.LineId == Guid.Empty) Invalid();
            if (command.Kind == "SaveWeekly" && command.Mask is not >= 0 or > 127) Invalid();
            if (command.Kind is "WithdrawWeekly" or "RemoveException" && command.TargetRevisionId is null) Invalid();
            if (command.Kind == "SaveException")
            {
                if (command.IsWorking is null || command.IsWorking == false && command.Hours is not null
                    || command.Hours is { } hours && !CalendarValueRules.ValidDecimal(hours, 24)
                    || !CalendarValueRules.TryText(command.Reason, 500, out var reason)) Invalid();
                else command = command with { Reason = reason };
            }
            return new(await repository.WriteTransitionAsync(command, ct), null);
        }
        catch (CalendarRuleException e) { return new(null, e.Failure); }
        catch (Exception) { return new(null, new("UNEXPECTED", 500, "Unknown")); }
    }

    private static DaySummary ResolveDay(DateOnly date, CalendarContext context, ProductionLine? line,
        WeeklyRevision? weekly, ExceptionRevision? plant, ExceptionRevision? scoped)
    {
        using var activity = CalendarTelemetry.Source.StartActivity("plant-calendar.resolve");
        return CalendarResolver.ResolveDay(date, context, line, weekly, plant, scoped);
    }

    private static CalendarScope Scope(Guid? id, ProductionLine? line) => new(id, line is null ? null : CalendarMapping.Line(line));
    private static async Task<ProductionLine?> Line(ICalendarReader reader, Guid? id, CancellationToken ct) => id is { } key ? await reader.LineAsync(key, ct) ?? throw Missing() : null;
    private static int Pages(int total, int size) => (int)(((long)total + size - 1) / size);
    private static void Snapshot(CalendarReadQuery q, CalendarContext context)
    {
        if (q.Page > 1 && q.SnapshotVersion is null) Invalid();
        if (q.SnapshotVersion is not null && (!CalendarValueRules.TryVersion(q.SnapshotVersion, out _) || q.SnapshotVersion != context.Version))
            throw new CalendarRuleException(new("CALENDAR_STALE", 409, "NotApplied"));
    }
    private static CalendarRuleException Missing() => new(new("NOT_FOUND", 404, "NotApplied"));
    private static void Invalid() => throw new CalendarRuleException(new("VALIDATION", 400, "NotApplied"));
}
