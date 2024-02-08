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
    void SetCached(int index, T value);

    /// <summary>
    /// Caches the range of values.
    /// </summary>
    void CachedRange(int start, int end, IList<T> values);
}
