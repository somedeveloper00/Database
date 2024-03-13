using System.Text;
using Database.Common;
using Database.Common.JsonSerializationImplementations;
using Database.Extensions;

namespace Database.ReadWriteImplementations;

/// <summary>
/// A <see cref="IReadWriteMethod{T}"/> that writes to and reads from disk using JSON.
/// </summary>
public sealed class DiskReadWriteJson<T>(string rootPath, Encoding encoding, IJsonSerializer<List<T>> jsonSerializer)
    : DiskReadWriteBase<T>(
        rootPath,
        FileMode.OpenOrCreate, FileMode.OpenOrCreate, FileMode.OpenOrCreate,
        FileAccess.Read, FileAccess.ReadWrite, FileAccess.ReadWrite)
    where T : new()
{
    private readonly Encoding encoding = encoding;
    private readonly IJsonSerializer<List<T>> jsonSerializer = jsonSerializer;

    /// <summary>
    /// Simple constructor to use default parameters.
    /// </summary>
    public DiskReadWriteJson(string rootPath)
        : this(rootPath, Encoding.UTF8, new SystemJsonSerializer<List<T>>()) { }

    protected override async Task<T[]> ReadValuesFromFile(FileStream fileStream, List<int> indexes)
    {
        if (fileStream.Length == 0)
        {
            throw new Exception("file has no content");
        }

        // read
        var bytes = await fileStream.ReadAllBytesAsync((int)fileStream.Length);
        var text = encoding.GetString(bytes);
        var values = jsonSerializer.Deserialize(text);

        // select
        var result = new T[indexes.Count];
        for (int i = 0; i < indexes.Count; i++)
        {
            result[i] = values[indexes[i]];
        }

        return result;
    }

    protected override async Task WriteValuesToFile(FileStream fileStream, List<T> values, List<int> indexes)
    {
        // read
        var bytes = await fileStream.ReadAllBytesAsync((int)fileStream.Length);
        var text = bytes.Length == 0 ? string.Empty : encoding.GetString(bytes);
        var allElements = bytes.Length == 0 ? [] : jsonSerializer.Deserialize(text);

        // override elements
        for (int i = 0; i < allElements.Count; i++)
        {
            int index = indexes.BinarySearch(i);
            if (index >= 0)
            {
                allElements[i] = values[index];
            }
        }

        // new elements
        int maxIndex = indexes.Max();
        if (allElements.Capacity < maxIndex + 1)
            allElements.Capacity = maxIndex + 1;
        for (int i = allElements.Count; i <= maxIndex; i++)
        {
            int index = indexes.BinarySearch(i);
            allElements.Add(index >= 0 ? values[index] : new T());
        }

        // write
        text = jsonSerializer.Serialize(allElements);
        bytes = encoding.GetBytes(text);
        fileStream.Seek(0, SeekOrigin.Begin);
        fileStream.SetLength(fileStream.Position);
        await fileStream.WriteAsync(bytes);
    }

    protected override async Task DeleteValueFromFile(FileStream fileStream, List<int> indexes)
    {
        // read
        var bytes = await fileStream.ReadAllBytesAsync((int)fileStream.Length);
        var text = bytes.Length == 0 ? string.Empty : encoding.GetString(bytes);
        var allElements = bytes.Length == 0 ? [] : jsonSerializer.Deserialize(text);

        // remove elements
        for (int i = indexes.Count - 1; i >= 0; i--)
        {
            allElements.RemoveAt(i);
        }

        // write
        text = jsonSerializer.Serialize(allElements);
        bytes = encoding.GetBytes(text);
        fileStream.Seek(0, SeekOrigin.Begin);
        fileStream.SetLength(bytes.Length);
        await fileStream.WriteAsync(bytes);
    }
}