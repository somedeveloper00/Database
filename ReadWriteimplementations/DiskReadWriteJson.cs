using System.Text;
using Database.Common;
using Database.Common.JsonSerializationImplementations;

namespace Database.ReadWriteimplementations;

/// <summary>
/// A <see cref="IReadWriteMethod{T}"/> that writes to and reads from disk using JSON.
/// </summary>
public sealed class DiskReadWriteJson<T>
    (string rootPath, Encoding encoding, IJsonSerializer<List<T>> jsonSerializer)
    : DiskReadWriteBase<T>(
        rootPath,
        FileMode.OpenOrCreate, FileMode.OpenOrCreate, FileMode.OpenOrCreate,
        FileAccess.Read, FileAccess.ReadWrite, FileAccess.ReadWrite)
    where T : new()
{
    private readonly Encoding encoding = encoding;
    private readonly IJsonSerializer<List<T>> jsonSerializer = jsonSerializer;

    protected override async Task<T[]> ReadValuesFromFile(FileStream fileStream, List<int> indexes)
    {
        var result = new T[indexes.Count];

        // read all file
        var buffer = new byte[fileStream.Length];
        int count = await fileStream.ReadAsync(buffer);
        if (count < buffer.Length)
        {
            throw new IOException("Could not read the whole file.");
        }

        // deserialize 
        var text = encoding.GetString(buffer);
        var values = jsonSerializer.Deserialize(text);
        for (int i = 0; i < indexes.Count; i++)
        {
            result[i] = values[indexes[i]];
        }

        return result;
    }

    protected override async Task WriteValuesToFile(FileStream fileStream, List<T> values, List<int> indexes)
    {
        var bytes = await ReadAllAsync(fileStream);
        var text = bytes.Length == 0 ? string.Empty : encoding.GetString(bytes);
        var allElements = bytes.Length == 0 ? [] : jsonSerializer.Deserialize(text);
        for (int i = 0; i < allElements.Count; i++)
        {
            int index = indexes.BinarySearch(i);
            if (index >= 0)
            {
                allElements[i] = values[index];
            }
        }
        // add new elements
        int maxIndex = indexes.Max();
        for (int i = allElements.Count; i < maxIndex + 1; i++)
        {
            int index = indexes.BinarySearch(i);
            if (index >= 0)
            {
                allElements.Add(values[index]);
            }
            else
            {
                allElements.Add(new T());
            }
        }
        text = jsonSerializer.Serialize(allElements);
        bytes = encoding.GetBytes(text);
        fileStream.Seek(0, SeekOrigin.Begin);
        await fileStream.WriteAsync(bytes);
        fileStream.SetLength(fileStream.Position);
    }

    private static async Task<byte[]> ReadAllAsync(FileStream fileStream)
    {
        List<byte> result = new(1024);
        byte[] buffer = new byte[1024];
        while (fileStream.Position < fileStream.Length)
        {
            int count = await fileStream.ReadAsync(buffer);
            result.AddRange(buffer[..count]);
        }
        return [.. result];
    }

    /// <summary>
    /// Creates a new instance of <see cref="DiskReadWriteJson{T}"/> with the <see cref="Encoding.UTF8"/> encoding and 
    /// <see cref="SystemJsonSerializer{T}"/> serializer.
    /// </summary>
    public static DiskReadWriteJson<T> CreateDefault(string rootPath)
    {
        return new(rootPath, Encoding.UTF8, new SystemJsonSerializer<List<T>>());
    }
}
