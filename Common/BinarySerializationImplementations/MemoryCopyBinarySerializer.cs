using System.Runtime.InteropServices;
using System.Text;

namespace Database.Common.BinarySerializationImplementations;

/// <summary>
/// A binary serializer that serializes and deserializes a type <typeparamref name="T"/> fieldwise. Only uses value types that 
/// are compatible with <see cref="BitConverter"/> and <see cref="Encoding.UTF8"/>'s <see cref="Encoding.GetBytes(string)"/>.
/// </summary>
public sealed class MemoryCopyBinarySerializer<T> : IBinarySerializer<T> where T : struct
{
    public unsafe byte[] Serialize(T value)
    {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        Span<byte> bytes = stackalloc byte[sizeof(T)];
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        MemoryMarshal.Write(bytes, ref value);
        return bytes.ToArray();
    }

    public unsafe T Deserialize(byte[] bytes)
    {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        Span<byte> readingSpan = stackalloc byte[sizeof(T)];
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        for (int i = 0; i < bytes.Length; i++)
        {
            readingSpan[i] = bytes[i];
        }
        return MemoryMarshal.Read<T>(readingSpan);
    }
}