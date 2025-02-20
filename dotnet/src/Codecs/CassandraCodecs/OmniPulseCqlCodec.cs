using System.Collections;
using System.Collections.Immutable;
using System.Text.Json;
using Cassandra.Mapping.TypeConversion;
using Microsoft.IdentityModel.Tokens;
using OmniGraphInterview.Constants.JsonSerializerSettings;

namespace OmniGraphInterview.Codecs.CassandraCodecs;

// Compiler can't tell if tp is nullable, but the database will be fine with null (not a primary key)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
/// <summary>
/// Codec for converting between Cassandra types and OmniModels types.
/// </summary>
public class OmniPulseCqlCodec : TypeConverter
{
    /// <inheritdoc />
    protected override Func<TDatabase, TPoco> GetUserDefinedFromDbConverter<TDatabase, TPoco>()
    {
        var isNullable = typeof(TPoco).IsGenericType &&
                         typeof(TPoco).GetGenericTypeDefinition() == typeof(Nullable<>);
        var targetType = isNullable ? Nullable.GetUnderlyingType(typeof(TPoco))! : typeof(TPoco);

        // Nested HashSet<string> within dictionary is stored as a map<text, text>
        if (typeof(TDatabase) == typeof(IDictionary<string, string>)
            && typeof(TPoco) == typeof(SortedDictionary<string, HashSet<string>>))
            return NestedHashSetDeserializer<TDatabase, TPoco>();

        // Json types are stored as strings in Cassandra and is converted using JsonSerializer.Deserialize()
        if (typeof(TDatabase) == typeof(string) &&
            IsJsonSerializable(isNullable ? Nullable.GetUnderlyingType(typeof(TPoco)) ?? typeof(TPoco) : typeof(TPoco)))
            return ConverterHelpers.NullableJsonConverter<TDatabase, TPoco>;

        // HashSet<string> is stored as IEnumerable<string> in Cassandra and is converted using .ToHashSet()
        if (typeof(TDatabase) == typeof(IEnumerable<string>) && typeof(TPoco) == typeof(HashSet<string>))
            return tdb => (TPoco)(object)((string[])(object)tdb).ToHashSet();

        // Float[] is stored as IEnumerable<float> in Cassandra and is converted using casting
        if (typeof(TDatabase) == typeof(IEnumerable<float>) && typeof(TPoco) == typeof(float[]))
            return tdb => (TPoco)(object)(float[])(object)tdb;


        // Nested HashSet<string> within sorted dictionary is stored as a map<text, text>
        if (typeof(TDatabase) == typeof(IDictionary<string, string>)
            && typeof(TPoco) == typeof(ImmutableSortedDictionary<string, HashSet<string>>))
            return tdb => (TPoco)(object)new SortedDictionary<string, HashSet<string>>(
                    ((SortedDictionary<string, string>)(object)tdb).ToDictionary(keySelector: kvp => kvp.Key,
                        elementSelector: kvp => JsonSerializer.Deserialize<HashSet<string>>(json: kvp.Value,
                            options: Serializers.CqlSerializers))!)
                .ToImmutableSortedDictionary();

        // Handle Enums as strings
        if (typeof(TDatabase) == typeof(string) && IsEnum(targetType))
            return tdb =>
            {
                if (tdb is string tdbString) return (TPoco)Enum.Parse(enumType: targetType, value: tdbString);

                return default;
            };

        // Json types are stored as strings in Cassandra and is converted using JsonSerializer.Serialize()
        if (typeof(TDatabase) == typeof(string) && IsJsonSerializable(targetType))
            return tdb =>
            {
                if (tdb is string tdbString)
                {
                    tdbString = tdbString.IsNullOrEmpty() ? "{}" : tdbString;
                    return JsonSerializer.Deserialize<TPoco>(json: tdbString, options: Serializers.CqlSerializers);
                }

                return default;
            };


        // Handle IEnumerable<string> as deserializes list of Json strings
        if (typeof(TDatabase) == typeof(IEnumerable<string>) && IsJsonSerializableList(targetType))
            return tdb =>
            {
                if (tdb is IEnumerable<string> tdbEnumerable)
                {
                    var innerType = GetEnumerableType(targetType);
                    return (TPoco)JsonSerializer.Deserialize<TPoco>(
                        JsonSerializer.Serialize(tdbEnumerable
                            .Select(s => s.IsNullOrEmpty() ? "{}" : s)
                            .Select(s =>
                                JsonSerializer.Deserialize(json: s, returnType: innerType,
                                    options: Serializers.CqlSerializers))));
                }

                return default;
            };

        // Handle IDictionary<string, string> as deserializes map of Json strings
        if (typeof(TDatabase) == typeof(SortedDictionary<string, string>)
            || typeof(TDatabase) == typeof(SortedDictionary<string, string?>)
            || typeof(TDatabase) == typeof(IDictionary<string, string>)
            || (typeof(TDatabase) == typeof(IDictionary<string, string?>)
                && IsJsonSerializableStringMap(targetType)))
            return tdb =>
            {
                try
                {
                    if (tdb is SortedDictionary<string, string> tdbDictionary)
                    {
                        var (keyType, valueType) = GetDictionaryTypes(typeof(TPoco));
                        if (keyType != typeof(string)) return default;
                        if (valueType == typeof(string))
                            return JsonSerializer.Deserialize<TPoco>(
                                json: JsonSerializer.Serialize(value: tdbDictionary,
                                    options: Serializers.CqlSerializers),
                                options: Serializers.CqlSerializers);

                        return JsonSerializer.Deserialize<TPoco>(
                            JsonSerializer.Serialize(
                                tdbDictionary.ToDictionary(
                                    keySelector: kvp => kvp.Key,
                                    elementSelector: kvp => JsonSerializer.Deserialize(json: kvp.Value,
                                        returnType: valueType, options: Serializers.CqlSerializers)
                                )));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }


                return default;
            };

        // If the database type is string and the Poco type is not string, try to deserialize the string
        if (typeof(TDatabase) == typeof(string) && !typeof(TPoco).IsAssignableFrom(typeof(string)))
            return tdb =>
            {
                if (tdb is string tdbString)
                    return JsonSerializer.Deserialize<TPoco>(json: tdbString, options: Serializers.CqlSerializers);

                return default;
            };

        return db => (TPoco)(object)db;
    }


    /// <inheritdoc />
    protected override Func<TPoco, TDatabase> GetUserDefinedToDbConverter<TPoco, TDatabase>()
    {
        var isNullable = typeof(TPoco).IsGenericType &&
                         typeof(TPoco).GetGenericTypeDefinition() == typeof(Nullable<>);
        var targetType = isNullable ? Nullable.GetUnderlyingType(typeof(TPoco))! : typeof(TPoco);

        // Nested HashSet<string> within sorted dictionary is stored as a map<text, text>
        if (typeof(TDatabase) == typeof(SortedDictionary<string, string>)
            && typeof(TPoco) == typeof(SortedDictionary<string, HashSet<string>>))
            return tdb => (TDatabase)(object)new SortedDictionary<string, string>(
                ((SortedDictionary<string, HashSet<string>>)(object)tdb).ToDictionary(keySelector: kvp => kvp.Key,
                    elementSelector: kvp =>
                        JsonSerializer.Serialize(value: kvp.Value, options: Serializers.CqlSerializers)));

        // Json types are stored as strings in Cassandra and is converted using JsonSerializer.Serialize()
        if (typeof(TDatabase) == typeof(string) && IsJsonSerializable(targetType))
            return tp =>
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - false positive
                (TDatabase)(object)(tp is null
                    ? null
                    : JsonSerializer.Serialize(value: tp, options: Serializers.CqlSerializers));

        if (typeof(TDatabase) == typeof(SortedDictionary<string, string>)
            && typeof(TPoco) == typeof(ImmutableSortedDictionary<string, HashSet<string>>))
            return tdb =>
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - false positive
                if (tdb == null) return (TDatabase)(object)null;

                return (TDatabase)(object)new SortedDictionary<string, string>(
                    ((ImmutableSortedDictionary<string, HashSet<string>>)(object)tdb).ToDictionary(
                        keySelector: kvp => kvp.Key,
                        elementSelector: kvp =>
                            JsonSerializer.Serialize(value: kvp.Value, options: Serializers.CqlSerializers)));
            };


        if (typeof(TDatabase) == typeof(string) && IsEnum(targetType))
            return tp =>
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - false positive
                if (tp is null) return (TDatabase)(object)null!;
                return (TDatabase)(object)tp.ToString();
            };

        // Handle IEnumerable<string> as deserializes list of Json strings
        if (typeof(TDatabase) == typeof(IEnumerable<string>) && IsJsonSerializableList(targetType))
            return tp => (TDatabase)(object)(tp switch
            {
                null => null,
                IEnumerable<string> => tp,
                Array array => array.Cast<object>()
                    .Select(o => JsonSerializer.Serialize(value: o, options: Serializers.CqlSerializers)).ToList(),
                IEnumerable enumerable => enumerable.Cast<object>()
                    .Select(o => JsonSerializer.Serialize(value: o, options: Serializers.CqlSerializers)).ToList(),
                _ => null,
            })!;

        // Handle IDictionary<string, string> as deserializes map of Json strings
        if (typeof(TDatabase) == typeof(SortedDictionary<string, string>)
            || typeof(TDatabase) == typeof(SortedDictionary<string, string?>)
            || typeof(TDatabase) == typeof(IDictionary<string, string>)
            || (typeof(TDatabase) == typeof(IDictionary<string, string?>)
                && IsJsonSerializableStringMap(targetType)))
            return tp =>
            {
                return (TDatabase)(object)(tp switch
                {
                    null => null,
                    SortedDictionary<string, string> => tp,
                    IDictionary<string, string> d => new SortedDictionary<string, string>(d),
                    IDictionary dictionary => ConvertGenericDictionary(dictionary),
                    _ => null,
                });
            };

        // If the database type is string and the Poco type is not string, try to serialize the string
        if (typeof(TDatabase) == typeof(string) && !typeof(TPoco).IsAssignableFrom(typeof(string)))
            return tp =>
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - false positive
                if (tp is null) return (TDatabase)(object)null!;
                return (TDatabase)(object)JsonSerializer.Serialize(value: tp, options: Serializers.CqlSerializers);
            };

        return tp => (TDatabase)(object)tp;
    }

    private static bool IsEnum(Type type)
    {
        return type.IsEnum;
    }

    private static SortedDictionary<string, string?> ConvertGenericDictionary(IDictionary dictionary)
    {
        var result = new SortedDictionary<string, string?>();
        foreach (DictionaryEntry entry in dictionary)
            result.Add(key: entry.Key.ToString() ?? string.Empty,
                value: entry.Value == null
                    ? null
                    : JsonSerializer.Serialize(value: entry.Value, options: Serializers.CqlSerializers));

        return result;
    }

    private static bool IsJsonSerializable(Type type)
    {
        if (type == typeof(JsonElement)) return true;
        var parent = GetParentClass(type);
        if (parent == typeof(OmniPulseJsonStoredObject) || parent == typeof(JsonElement)) return true;

        return parent is not null && IsJsonSerializable(parent);
    }

    private static bool IsJsonSerializableList(Type? type)
    {
        if (type == typeof(string)) return false;
        return typeof(IEnumerable).IsAssignableFrom(type) && IsJsonSerializable(GetEnumerableType(type));
    }

    private static bool IsJsonSerializableStringMap(Type? type)
    {
        if (!typeof(IDictionary).IsAssignableFrom(type)) return false;


        var (keyType, valueType) = GetDictionaryTypes(type);
        if (keyType != typeof(string)) return false;
        if (valueType == typeof(string)) return false; // Cast instead of serialize

        return IsJsonSerializable(valueType)
               || IsJsonSerializableList(valueType)
               || IsJsonSerializableStringMap(valueType);
    }

    private static Type? GetParentClass(Type? type)
    {
        return type?.BaseType;
    }

    private static Type GetEnumerableType(Type type)
    {
        if (type.IsArray)
            return type.GetInterfaces()
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(t => t.GetGenericArguments()[0]).FirstOrDefault()!;

        if (typeof(IEnumerable).IsAssignableFrom(type) && type.GetGenericArguments().Length == 1)
            return type.GetGenericArguments()[0];

        throw new ArgumentException("Type is not IEnumerable<T>");
    }

    private static (Type key, Type value) GetDictionaryTypes(Type type)
    {
        if (!typeof(IDictionary).IsAssignableFrom(type) || !type.IsGenericType ||
            type.GetGenericArguments().Length != 2)
            throw new ArgumentException("Type is not a IDictionary<TKey, TValue>");

        var keyType = type.GetGenericArguments()[0];
        var valueType = type.GetGenericArguments()[1];
        return (keyType, valueType);
    }

    private static Func<TDatabase, TPoco> NestedHashSetDeserializer<TDatabase, TPoco>()
    {
        return tdb => (TPoco)(object)new SortedDictionary<string, HashSet<string>>(
            ((SortedDictionary<string, string>)(object)tdb!).ToDictionary(keySelector: kvp => kvp.Key,
                elementSelector: kvp =>
                    JsonSerializer.Deserialize<HashSet<string>>(json: kvp.Value,
                        options: Serializers.CqlSerializers))!);
    }
}
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
