using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProductionManagementAI.Application.PlantCalendar;

/// <summary>Validates calendar scalars without date-zone or floating-point coercion.</summary>
public static partial class CalendarValueRules
{
    /// <summary>Gets the seven ordered weekday transport tokens.</summary>
    public static IReadOnlyList<string> Weekdays { get; } = Array.AsReadOnly(new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" });

    /// <summary>Parses one exact supported Gregorian date.</summary>
    public static bool TryDate(string? text, out DateOnly value) => DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

    /// <summary>Parses an exact supported month to its first date.</summary>
    public static bool TryMonth(string? text, out DateOnly value) => TryDate(text is { Length: 7 } ? text + "-01" : null, out value);

    /// <summary>Parses a positive opaque bigint revision.</summary>
    public static bool TryVersion(string? text, out long value)
    {
        value = 0;
        return text is { Length: > 0 and <= 19 } && Integer().IsMatch(text)
            && long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value) && value > 0;
    }

    /// <summary>Parses a nonzero canonical hyphenated reference.</summary>
    public static bool TryId(string? text, out Guid value) => Guid.TryParseExact(text, "D", out value) && value != Guid.Empty;

    /// <summary>Parses exact positive bounded hours after Unicode whitespace trimming.</summary>
    public static bool TryHours(string? text, out decimal value)
    {
        value = 0;
        text = text?.Trim();
        return text is { Length: > 0 and <= 32 } && Decimal().IsMatch(text)
            && decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value) && value > 0 && value <= 24;
    }

    /// <summary>Rejects malformed Unicode and normalizes bounded optional plain text.</summary>
    public static bool TryText(string? text, int maximum, out string? normalized)
    {
        normalized = null;
        if (text is null) return true;
        var rest = text.AsSpan();
        while (!rest.IsEmpty)
        {
            if (Rune.DecodeFromUtf16(rest, out _, out var consumed) != OperationStatus.Done) return false;
            rest = rest[consumed..];
        }
        var trimmed = text.Trim();
        if (trimmed.EnumerateRunes().Count() > maximum) return false;
        normalized = trimmed.Length == 0 ? null : trimmed;
        return true;
    }

    /// <summary>Maps distinct approved weekday tokens to a Monday-based mask.</summary>
    public static bool TryMask(IReadOnlyList<string>? days, out short mask)
    {
        mask = 0;
        if (days is null || days.Count > 7) return false;
        foreach (var day in days)
        {
            var bit = Array.IndexOf(Weekdays.ToArray(), day);
            if (bit < 0 || (mask & (1 << bit)) != 0) return false;
            mask |= (short)(1 << bit);
        }
        return true;
    }

    /// <summary>Formats an exact validated decimal with no unnecessary zeroes.</summary>
    public static string Format(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>Validates the persisted decimal scale and approved range without rounding.</summary>
    public static bool ValidDecimal(decimal value, decimal maximum, bool allowZero = false) =>
        value <= maximum && (allowZero ? value >= 0 : value > 0) && ((decimal.GetBits(value)[3] >> 16) & 255) <= 3;

    [GeneratedRegex(@"\A[1-9][0-9]*\z", RegexOptions.CultureInvariant)]
    private static partial Regex Integer();
    [GeneratedRegex(@"\A(?:0|[1-9][0-9]*)(?:\.[0-9]{1,3})?\z", RegexOptions.CultureInvariant)]
    private static partial Regex Decimal();
}
