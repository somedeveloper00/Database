namespace Database.CacheImplementations;

/// <summary>
/// Saves the cached data in memory, managed and by a collection type of your choice. 
/// </summary>
public sealed class MemoryCache<T>(IList<T> cache, MemoryCache<T>.EnlargeCache enlargeCache) : ICacheMethod<T>
{
    /// <summary>
    /// Represents a method that enlarges the cache.
    /// </summary>
    public delegate IList<T> EnlargeCache(IList<T> oldCache, int size);

    /// <summary>
    /// The currenct cache in use.
    /// </summary>
    public IList<T> cache = cache;

    /// <summary>
    /// The method for enlarging the cache.
    /// </summary>
    public readonly MemoryCache<T>.EnlargeCache enlargeCache = enlargeCache;

    public void CachedRange(int start, int end, IList<T> values)
    {
        EnsureSize(end);
        for (int i = start; i < end; i++)
        {
            cache[i] = values[i - start];
        }
    }

    public void SetCached(int index, T value)
    {
        EnsureSize(index);
        cache[index] = value;
    }

    public bool TryGetCached(int index, out T value)
    {
        if (index < cache.Count)
        {
            value = cache[index];
            return true;
        }
        value = default!;
        return false;
    }

    private void EnsureSize(int end)
    {
        if (cache.Count < end)
        {
            cache = enlargeCache(cache, end);
        }
    }

    /// <summary>
    /// Quickly create a new <see cref="MemoryCache{T}"/> with <see cref="Array"/> as its cache collection type.
    /// </summary>
    public static MemoryCache<T> CreateArray(int size)
    {
        return new MemoryCache<T>(new T[size], (oldCache, newSize) =>
        {
            T[] newCache = new T[newSize];
            for (int i = 0; i < oldCache.Count; i++)
            {
                newCache[i] = oldCache[i];
            }
            return newCache;
        });
    }

    /// <summary>
    /// Quickly create a new <see cref="MemoryCache{T}"/> with <see cref="List{T}"/> as its cache collection type.
    /// </summary>
    public static MemoryCache<T> CreateList(int size)
    {
        // fill the list with default values
        var list = Enumerable.Repeat(default(T)!, size).ToList()!;
        return new MemoryCache<T>(list, (oldCache, newSize) =>
        {
            oldCache.Insert(newSize - 1, default!);
            return oldCache;
        });
    }
}