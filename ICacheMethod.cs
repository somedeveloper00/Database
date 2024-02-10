namespace Database;

/// <summary>
/// Represents the method for caching data
/// </summary>
public interface ICacheMethod<T>
{
    /// <summary>
    /// Tries to get the cached value at the specified index.
    /// </summary>
    bool TryGetCached(int index, out T value);

    /// <summary>
    /// Gets the cached value at the specified index.
    /// </summary>
    void SetCacheAt(int index, T value);

    /// <summary>
    /// Caches the range of values.
    /// </summary>
    void SetCacheRange(IList<T> values, int start, int count);

    /// <summary>
    /// Removes cached element at the specified index.
    /// </summary>
    void RemoveCacheAt(int index);

    /// <summary>
    /// Removes cached elements at the specified range.
    /// </summary>
    void RemoveCacheRange(int startIndex, int count);
}
