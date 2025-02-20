using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cassandra;

namespace OmniGraphInterview.Constants.JsonSerializerSettings;

/// <summary>
/// Serializers used in OmniPulse
/// </summary>
public static class Serializers
{
    /// <summary>
    /// Serializer used for REST API
    /// </summary>
    public static readonly JsonSerializerOptions RestApiSerializers;

    /// <summary>
    /// Serializer used for writing to Cassandra
    /// </summary>
    public static readonly JsonSerializerOptions CqlSerializers;

    /// <summary>
    /// Serializer used for writing to Algorithms
    /// </summary>
    public static readonly JsonSerializerOptions AlgorithmSerializer;

    /// <summary>
    /// Serializer used for Shopify integrations
    /// </summary>
    public static readonly JsonSerializerOptions ShopifySerializer;

    static Serializers()
    {
        CqlSerializers = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        AlgorithmSerializer = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        RestApiSerializers = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };
        ShopifySerializer = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        CqlSerializers.Converters.Add(new JsonStringEnumConverter());
        CqlSerializers.Converters.Add(new LocalDateSerializer());
        CqlSerializers.Converters.Add(new DecimalSerializer());
        CqlSerializers.Converters.Add(new DateTimeOffsetConverter());

        RestApiSerializers.Converters.Add(new LocalDateSerializer());
        RestApiSerializers.Converters.Add(new JsonStringEnumConverter());
        RestApiSerializers.Converters.Add(new DecimalSerializer());
        RestApiSerializers.Converters.Add(new DateTimeOffsetConverter());

        AlgorithmSerializer.Converters.Add(new JsonStringEnumConverter());
        AlgorithmSerializer.Converters.Add(new LocalDateSerializer());
        AlgorithmSerializer.Converters.Add(new DecimalSerializer());
        AlgorithmSerializer.Converters.Add(new DateTimeOffsetConverter());

        ShopifySerializer.Converters.Add(new JsonStringEnumConverter());
        ShopifySerializer.Converters.Add(new LocalDateSerializer());
        ShopifySerializer.Converters.Add(new DecimalSerializer());
        ShopifySerializer.Converters.Add(new DateTimeOffsetConverter());
        ShopifySerializer.Converters.Add(new ShopifyCurrencyAmountConverter());
    }

    /// <summary>
    /// Serializer used for writing LocalDate to JSON
    /// </summary>
    public class LocalDateSerializer : JsonConverter<LocalDate>
    {
        /// <inheritdoc />
        public override LocalDate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return LocalDate.Parse(reader.GetString());
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, LocalDate value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// Serializer used for writing decimal to JSON with two decimal places and dot notation (e.g. 1.00)
    /// </summary>
    public class DecimalSerializer : JsonConverter<decimal>
    {
        /// <inheritdoc />
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TryGetDecimal(out var dec) ? dec : default;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(value.ToString(format: Formats.DecimalFormat, provider: CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Serializer used for writing DateTimeOffset to JSON with ISO 8601 format
    /// </summary>
    public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        /// <inheritdoc />
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.GetString() is { } strDate)
                return DateTimeOffset.Parse(input: strDate, formatProvider: CultureInfo.InvariantCulture);

            return default;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString(format: Formats.DateTimeFormat,
                formatProvider: CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Serializer used for writing decimal to JSON with two decimal places and dot notation (e.g. 1.00)
    /// </summary>
    public class ShopifyCurrencyAmountConverter : JsonConverter<decimal?>
    {
        /// <inheritdoc />
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var strDecimal = reader.GetString();
            if (string.IsNullOrEmpty(strDecimal)) return null;

            return decimal.Parse(s: strDecimal, provider: CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Value.ToString(format: "F2", provider: CultureInfo.InvariantCulture));
        }
    }
}
