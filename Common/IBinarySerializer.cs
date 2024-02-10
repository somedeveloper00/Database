namespace Database.Common;

/// <summary>
/// A serializer that can serialize and deserialize objects to and from bytes.
/// </summary>
public interface IBinarySerializer<T> where T : new()
{
    /// <summary>
    /// Serializes the given value to bytes. Should keep in mind that the deserialization 
    /// method should result in in the same value in different times and processors.
    /// </summary>
    byte[] Serialize(T value);

    /// <summary>
    /// Deserializes the given bytes to a value of type <typeparamref name="T"/>. 
    /// Should keep in mind that the deserialization method should result in in the same 
    /// value in different times and processors.
    /// </summary>
    T Deserialize(byte[] bytes);
}