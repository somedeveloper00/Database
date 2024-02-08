namespace Database.Common;

/// <summary>
/// Represents a JSON serializer.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serializes the value to a JSON string.
    /// </summary>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes the JSON string to a value.
    /// </summary>
    T Deserialize<T>(string value);
}