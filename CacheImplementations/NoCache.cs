
namespace Database.CacheImplementations;

/// <summary>
/// A <see cref="ICacheMethod{T}"/> that does not cache anything.
/// </summary>
public sealed class NoCache<T> : ICacheMethod<T>
{
    public void SetCachedRange(int start, int end, IList<T> values) { }

    public void SetCacheAt(int index, T value) { }

    public bool TryGetCached(int index, out T value)
    {
        value = default!;
        return false;
    }

    public void SetCacheRange(IList<T> values, int start, int count) { }

    public void RemoveCacheAt(int index) { }

    public void RemoveCacheRange(int startIndex, int count) { }
}