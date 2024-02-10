using System.Text.Json;

namespace Database.Common.JsonSerializationImplementations;

/// <summary>
/// A JSON serializer that uses <see cref="JsonSerializer"/>.
/// </summary>
public sealed class SystemJsonSerializer<T> : IJsonSerializer<T>
{
    /// <summary>
    /// The options used for serialization and deserialization.
    /// </summary>
    public JsonSerializerOptions options = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString |
                         System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
        PreferredObjectCreationHandling = System.Text.Json.Serialization.JsonObjectCreationHandling.Populate,
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip
    };

    public T Deserialize(string value) => JsonSerializer.Deserialize<T>(value, options)!;

    public string Serialize(T value) => JsonSerializer.Serialize(value, options);
}