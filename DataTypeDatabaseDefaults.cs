using Database.CacheImplementations;
using Database.ReadWriteImplementations;

namespace Database;

public sealed class DataTypeDatabaseDefaults
{
    /// <summary>
    /// Creates a new instance of <see cref="TypeDatabase{T}"/> with the default JSON 
    /// read-write method and the default memory cache method. The path is the 
    /// <see cref="Type.Name"/> of the type.
    /// </summary>
    public static TypeDatabase<T> DefaultJsonDatabase<T>(string rootFolder)
        where T : new()
    {
        return new TypeDatabase<T>($"{typeof(T).Name}.json",
            new DiskReadWriteJson<T>(rootFolder),
            new MemoryCache<T>(1024));
    }

    /// <summary>
    /// Creates a new instance of <see cref="TypeDatabase{T}"/> with the default JSON 
    /// read-write method and without caching. The path is the <see cref="Type.Name"/> of 
    /// the type.
    /// </summary>
    public static TypeDatabase<T> DefaultJsonDatabaseNoCache<T>(string rootFolder)
        where T : new()
    {
        return new TypeDatabase<T>($"{typeof(T).Name}.json",
            new DiskReadWriteJson<T>(rootFolder),
            new NoCache<T>());
    }

    /// <summary>
    /// Creates a new instance of <see cref="TypeDatabase{T}"/> with the default binary 
    /// read-write method and the default memory cache method. The path is the 
    /// <see cref="Type.Name"/> of the type.
    /// </summary>
    public static TypeDatabase<T> DefaultBinaryDatabase<T>(string rootFolder)
        where T : unmanaged
    {
        return new TypeDatabase<T>($"{typeof(T).Name}.bin",
            new DiskReadWriteBinary<T>(rootFolder),
            new MemoryCache<T>(1024));
    }

    /// <summary>
    /// Creates a new instance of <see cref="TypeDatabase{T}"/> with the default binary 
    /// read-write method and without caching. The path is the <see cref="Type.Name"/> of 
    /// the type.
    /// </summary>
    public static TypeDatabase<T> DefaultBinaryDatabaseNoCache<T>(string rootFolder)
        where T : unmanaged
    {
        return new TypeDatabase<T>($"{typeof(T).Name}.bin",
            new DiskReadWriteBinary<T>(rootFolder),
            new NoCache<T>());
    }
}