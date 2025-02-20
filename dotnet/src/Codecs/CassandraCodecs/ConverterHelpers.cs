using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using OmniGraphInterview.Constants.JsonSerializerSettings;

namespace OmniGraphInterview.Codecs.CassandraCodecs;

/// <summary>
/// Helper methods for converting between database and generic types.
/// </summary>
public static class ConverterHelpers
{
    /// <summary>
    /// Converts a string to a nullable enum value.
    /// </summary>
    /// <param name="tdb"></param>
    /// <typeparam name="TDatabase"></typeparam>
    /// <typeparam name="TPoco"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TPoco NullableEnumConverter<TDatabase, TPoco>(TDatabase tdb)
    {
        if (tdb is null) return (TPoco)(object)null!;

        if (tdb is not string serializedEnum)
            throw new InvalidCastException($"Field of type {tdb.GetType()} cannot be converted to string");

        return (TPoco)Enum.Parse(enumType: Nullable.GetUnderlyingType(typeof(TPoco)) ?? typeof(TPoco),
            value: serializedEnum);
    }

    /// <summary>
    /// Converts a string to a nullable JSON value.
    /// </summary>
    /// <param name="tdb"></param>
    /// <typeparam name="TDatabase"></typeparam>
    /// <typeparam name="TPoco"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TPoco NullableJsonConverter<TDatabase, TPoco>(TDatabase tdb)
    {
        if (tdb is null) return default!;

        if (tdb is not string serializedJson)
            throw new InvalidCastException($"Field of type {tdb.GetType()} cannot be converted to string");
        if (serializedJson.IsNullOrEmpty())
            return default!;
        return (TPoco)(JsonSerializer.Deserialize(json: serializedJson, returnType: typeof(TPoco),
            options: Serializers.CqlSerializers) ?? default!);
    }
}
