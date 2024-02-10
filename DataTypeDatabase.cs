namespace Database;

/// <summary>
/// Represents the database for a specific type of data at a specific path. It uses interfaces so 
/// that you can treat it generally for any kind of I/O, be it to disk, to network etc. It also 
/// uses a cache method to optimize for reading.
/// </summary>
public sealed class TypeDatabase<T>(string path, IReadWriteMethod<T> readWriteMethod, ICacheMethod<T> cacheMethod)
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
    public async Task<T> GetAt(int index)
    {
        if (!cacheMethod.TryGetCached(index, out var result))
        {
            await readWriteMethod.BeginRead(path);
            await readWriteMethod.Read(index);
            result = (await readWriteMethod.EndRead())[0];
            cacheMethod.SetCached(index, result);
        }
        return result;
    }

    /// <summary>
    /// Gets the items at the given range, asynchronously. <paramref name="startIndex"/> is 
    /// inclusive.
    /// </summary>
    public async Task<T[]> GetRage(int startIndex, int count)
    {
        T[] result = new T[count];
        bool startedReading = false;
        for (int i = 0; i < count; i++)
        {
            int index = startIndex + i;
            if (cacheMethod.TryGetCached(index, out var value))
            {
                result[i] = value;
            }

            if (result[i] == null)
            {
                if (!startedReading)
                {
                    startedReading = true;
                    await readWriteMethod.BeginRead(path);
                }
                await readWriteMethod.Read(index);
            }
        }

        // merge read values
        if (startedReading)
        {
            var readElements = await readWriteMethod.EndRead();
            int index = 0;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] ??= readElements[index++];
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
    /// Delets the element at the specified index.
    /// </summary>
    public async Task DeleteAt(int index)
    {
        cacheMethod.RemoveCacheAt(index);
        await readWriteMethod.BeginDelete(path);
        await readWriteMethod.Delete(index);
        await readWriteMethod.EndDelete();
    }

    /// <summary>
    /// Deletes a range of elements
    /// </summary>
    public async Task DeleteRange(int startIndex, int count)
    {
        cacheMethod.RemoveCacheRange(startIndex, count);
        await readWriteMethod.BeginDelete(path);
        for (int i = 0; i < count; i++)
        {
            await readWriteMethod.Delete(startIndex + i);
        }
        await readWriteMethod.EndDelete();
    }

    /// <summary>
    /// Sets the range of values, asynchronously. <paramref name="startIndex"/> is inclusive 
    /// while <paramref name="endIndex"/> is exclusive.
    /// </summary>
    public async Task SetRange(IList<T> values, int startIndex)
    {
        await readWriteMethod.BeginWrite(path);
        for (int i = 0; i < values.Count; i++)
        {
            int dbIndex = i + startIndex;
            await readWriteMethod.Write(values[i], dbIndex);
            cacheMethod.SetCached(dbIndex, values[i]);
        }
        await readWriteMethod.EndWrite();
    }
}
