using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductionManagementAI.Api.ProductionOrders;

/// <summary>Reads fixed-point JSON numbers exactly, retaining the written scale limit.</summary>
public sealed class ExactQuantityConverter : JsonConverter<decimal>
{
    /// <inheritdoc />
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException("Quantity must be a JSON number.");
        var token = Encoding.UTF8.GetString(reader.HasValueSequence
            ? reader.ValueSequence.ToArray() : reader.ValueSpan);
        var dot = token.IndexOf('.');
        if (token.Contains('e', StringComparison.OrdinalIgnoreCase) ||
            (dot >= 0 && token.Length - dot - 1 > 3) ||
            !decimal.TryParse(token, NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var quantity) ||
            quantity <= 0 || quantity > 999_999_999m)
            throw new JsonException("Quantity must be positive fixed-point with at most three decimal places.");
        return quantity;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);
}
