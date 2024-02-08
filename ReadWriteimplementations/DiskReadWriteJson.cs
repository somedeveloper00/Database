using System.Text;
using Database.Common;

namespace Database.ReadWriteimplementations;

/// <summary>
/// A <see cref="IReadWriteMethod{T}"/> that writes to and reads from disk using JSON.
/// </summary>
public sealed class DiskReadWriteJson<T>(string rootPath, Encoding encoding, IJsonSerializer jsonSerializer) : DiskReadWriteBase<T>(rootPath)
{
    private readonly Encoding encoding = encoding;
    private readonly IJsonSerializer jsonSerializer = jsonSerializer;

    protected override async Task<T[]> ReadValuesFromFile(FileStream fileStream, List<int> indexes)
    {
        var result = new T[indexes.Count];

        // read all file
        var buffer = new byte[fileStream.Length];
        int count = await fileStream.ReadAsync(buffer);
        if (count < buffer.Length)
        {
            buffer = buffer[..count];
        }

        // deserialize 
        var text = encoding.GetString(buffer);
        var values = jsonSerializer.Deserialize<List<T>>(text);
        for (int i = 0; i < indexes.Count; i++)
        {
            result[i] = values[indexes[i]];
        }

        return result;
    }

    protected override async Task WriteValuesToFile(FileStream fileStream, List<T> values, List<int> indexes)
    {
        var text = jsonSerializer.Serialize(values);
        var buffer = encoding.GetBytes(text);
        await fileStream.WriteAsync(buffer);
    }
}
