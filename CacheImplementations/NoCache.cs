
namespace Database.CacheImplementations;

/// <summary>
/// A <see cref="ICacheMethod{T}"/> that does not cache anything.
/// </summary>
public sealed class NoCache<T> : ICacheMethod<T>
{
    public void CachedRange(int start, int end, IList<T> values) { }
    public void SetCached(int index, T value) { }
    public bool TryGetCached(int index, out T value)
    {
        value = default!;
        return false;
    }

    /// <summary>
    /// An instance of <see cref="NoCache{T}"/>, to avoid extra heap allocation, since it doesn't 
    /// do anything, it doesn't need sepaarte instances and is thread-safe.
    /// </summary>
    public static NoCache<T> Instance { get; } = new();
}