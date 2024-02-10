namespace Database.CacheImplementations;

/// <summary>
/// Saves the cached data in memory, managed and by a collection type of your choice. 
/// </summary>
public sealed class MemoryCache<T>(int capacity) : ICacheMethod<T>
{
    /// <summary>
    /// The currenct cache in use.
    /// </summary>
    public List<Slot> cache = new(capacity);

    public void SetCacheRange(IList<T> values, int start, int count)
    {
        EnsureIndexExists(start + count - 1);
        for (int i = 0; i < count; i++)
        {
            cache[i + start] = NewSlot(values[i]);
        }
    }

    public void SetCacheAt(int index, T value)
    {
        EnsureIndexExists(index);
        cache[index] = NewSlot(value);
    }

    public bool TryGetCached(int index, out T value)
    {
        if (index < cache.Count)
        {
            value = cache[index].value;
            return !cache[index].empty;
        }
        value = default!;
        return false;
    }

    public void RemoveCacheAt(int index)
    {
        if (index < cache.Count)
        {
            cache[index] = EmptySlot;
        }
    }

    public void RemoveCacheRange(int startIndex, int count)
    {
        int maxIndex = Math.Min(startIndex + count, cache.Count);
        for (int i = startIndex; i < maxIndex; i++)
        {
            cache[i] = EmptySlot;
        }
    }

    private void EnsureIndexExists(int end)
    {
        while (cache.Count <= end)
        {
            cache.Add(EmptySlot);
        }
    }

    private static readonly Slot EmptySlot = new(true, default!);

    private static Slot NewSlot(T value) => new(false, value);

    public readonly struct Slot(bool empty, T value)
    {
        public readonly bool empty = empty;
        public readonly T value = value;
    }
}
