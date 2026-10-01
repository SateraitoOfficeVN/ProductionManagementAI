using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProductionManagementAI.Application.ProductionLines;

/// <summary>Validates exact feature scalars without floating-point rounding or generation-token coercion.</summary>
public static partial class LineValueRules
{
    /// <summary>Gets the fixed permitted product unit set.</summary>
    public static IReadOnlySet<string> Units { get; } = new HashSet<string>(StringComparer.Ordinal)
        { "個", "本", "枚", "台", "セット", "kg", "m" };

    /// <summary>Validates a positive bounded exact decimal string.</summary>
    /// <param name="text">The unmodified submitted decimal text.</param>
    /// <param name="maximum">The inclusive approved upper bound.</param>
    /// <param name="value">When this method returns, contains the parsed exact value.</param>
    /// <returns><see langword="true"/> if syntax and range are valid; otherwise, <see langword="false"/>.</returns>
    public static bool TryDecimal(string? text, decimal maximum, out decimal value)
    {
        value = 0;
        return text is not null && text.Length <= 32 && DecimalSyntax().IsMatch(text)
            && decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value)
            && value > 0 && value <= maximum;
    }

    /// <summary>Validates a canonical opaque PostgreSQL xmin string.</summary>
    /// <param name="text">The submitted generation token.</param>
    /// <param name="value">When this method returns, contains the parsed token.</param>
    /// <returns><see langword="true"/> if canonical and in range; otherwise, <see langword="false"/>.</returns>
    public static bool TryVersion(string? text, out uint value)
    {
        value = 0;
        return text is not null && text.Length <= 10 && IntegerSyntax().IsMatch(text)
            && uint.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Validates a canonical opaque product unit revision.</summary>
    /// <param name="text">The submitted generation token.</param>
    /// <param name="value">When this method returns, contains the parsed token.</param>
    /// <returns><see langword="true"/> if canonical and in range; otherwise, <see langword="false"/>.</returns>
    public static bool TryRevision(string? text, out long value)
    {
        value = 0;
        return text is not null && text.Length <= 19 && IntegerSyntax().IsMatch(text)
            && long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Normalizes bounded Unicode line text.</summary>
    /// <param name="text">The submitted text.</param>
    /// <param name="maximum">The inclusive Unicode code-point bound.</param>
    /// <param name="value">When this method returns, contains the trimmed text.</param>
    /// <returns><see langword="true"/> if nonblank and bounded; otherwise, <see langword="false"/>.</returns>
    public static bool TryText(string? text, int maximum, out string value)
    {
        value = text?.Trim() ?? string.Empty;
        return value.Length > 0 && value.EnumerateRunes().Count() <= maximum;
    }

    /// <summary>Formats a response decimal without exponent or unnecessary trailing zeroes.</summary>
    /// <param name="value">The exact stored value.</param>
    /// <returns>A canonical invariant decimal string.</returns>
    public static string Format(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    [GeneratedRegex(@"\A(?:0|[1-9][0-9]*)(?:\.[0-9]{1,3})?\z", RegexOptions.CultureInvariant)]
    private static partial Regex DecimalSyntax();
    [GeneratedRegex(@"\A(?:0|[1-9][0-9]*)\z", RegexOptions.CultureInvariant)]
    private static partial Regex IntegerSyntax();
}
