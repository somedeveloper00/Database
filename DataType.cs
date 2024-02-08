using Database;

namespace FastDatabase;

/// <summary>
/// Represents the database for a specific type of data at a specific path. It uses interfaces so that you can 
/// treat it generally for any kind of I/O, be it to disk, to network etc. It also uses a cache method to 
/// optimize for reading.
/// </summary>
public sealed class DataType<T>(string path, IReadWriteMethod<T> readWriteMethod, ICacheMethod<T> cacheMethod)
{
    /// <summary>
    /// The path of the database. Not necessarily a file path, but a path to the data.
    /// </summary>
    public readonly string path = path;

    /// <summary>
    /// The method for reading and writing data.
    /// </summary>
    public readonly IReadWriteMethod<T> readWriteMethod = readWriteMethod;

    /// <summary>
    /// The method for caching data for fast read.
    /// </summary>
    public readonly ICacheMethod<T> cacheMethod = cacheMethod;

    /// <summary>
    /// Gets the file at the given index, asynchronously.
    /// </summary>
    public async Task<T> GetItemAt(int index)
    {
        if (cacheMethod.TryGetCached(index, out var result))
        {
            return result;
        }

        await readWriteMethod.BeginRead(path);
        await readWriteMethod.Read(index);
        var value = (await readWriteMethod.EndRead())[0];
        cacheMethod.SetCached(index, value);
        return result;
    }

    /// <summary>
    /// Gets the items at the given range, asynchronously. <paramref name="startIndex"/> is inclusive, 
    /// <paramref name="endIndex"/> is exclusive.
    /// </summary>
    public async Task<T[]> GetItemsAt(int startIndex, int endIndex)
    {
        T[] result = new T[endIndex - startIndex];
        List<int> toReadIndexes = null;
        for (int i = startIndex; i < endIndex; i++)
        {
            if (cacheMethod.TryGetCached(i, out var value))
            {
                result[i - startIndex] = value;
            }
            else
            {
                if (toReadIndexes == null)
                {
                    await readWriteMethod.BeginRead(path);
                    toReadIndexes = new List<int>(endIndex - i);
                }
                await readWriteMethod.Read(i);
                toReadIndexes.Add(i);
            }
        }
        if (toReadIndexes is not null)
        {
            var values = await readWriteMethod.EndRead();
            for (int i = 0; i < values.Length; i++)
            {
                result[toReadIndexes[i] - startIndex] = values[i];
                cacheMethod.SetCached(toReadIndexes[i], values[i]);
            }
        }
        return result;
    }

    /// <summary>
    /// Sets the value at the specified index, asynchronously.
    /// </summary>
    public async Task SetAt(T value, int index)
    {
        await readWriteMethod.BeginWrite(path);
        await readWriteMethod.Write(value, index);
        await readWriteMethod.EndWrite();
        cacheMethod.SetCached(index, value);
    }

    /// <summary>
    /// Sets the range of values, asynchronously.
    /// </summary>
    public async Task SetRange(IList<T> values, int startIndex, int endIndex)
    {
        await readWriteMethod.BeginWrite(path);
        for (int i = startIndex; i < endIndex; i++)
        {
            await readWriteMethod.Write(values[i], i);
            cacheMethod.SetCached(i, values[i]);
        }
        await readWriteMethod.EndWrite();
    }
}
