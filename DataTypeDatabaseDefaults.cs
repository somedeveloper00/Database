using Database.CacheImplementations;
using Database.ReadWriteimplementations;

namespace Database;

public sealed class DataTypeDatabaseDefaults
{
    /// <summary>
    /// Creates a new instance of <see cref="TypeDatabase{T}"/> with the default JSON read-write method 
    /// (<see cref="DiskReadWriteJson{T}.CreateDefault(string)"/>) and the default memory cache method 
    /// (<see cref="MemoryCache{T}.CreateList(int)"/>). The path is the <see cref="Type.Name"/> of the type.
    /// </summary>
    /// <param name="rootFolder"></param>
    /// <returns></returns>
    public static TypeDatabase<T> DefaultJsonDatabase<T>(string rootFolder)
        where T : new()
    {
        return new TypeDatabase<T>($"{typeof(T).Name}.json",
            DiskReadWriteJson<T>.CreateDefault(rootFolder),
            MemoryCache<T>.CreateList(1024));
    }

    /// <summary>
    /// Creates a new instance of <see cref="TypeDatabase{T}"/> with the default JSON read-write method 
    /// (<see cref="DiskReadWriteJson{T}.CreateDefault(string)"/>) and without caching 
    /// (<see cref="NoCache{T}.Instance"/>). The path is the <see cref="Type.Name"/> of the type.
    /// </summary>
    public static TypeDatabase<T> DefaultJsonDatabaseNoCache<T>(string rootFolder)
        where T : new()
    {
        return new TypeDatabase<T>($"{typeof(T).Name}.json",
            DiskReadWriteJson<T>.CreateDefault(rootFolder),
            NoCache<T>.Instance);
    }

    /// <summary>
    /// Creates a new instance of <see cref="TypeDatabase{T}"/> with the default binary read-write method 
    /// (<see cref="DiskReadWriteBinary{T}.CreateDefault(string)"/>) and the default memory cache method 
    /// (<see cref="MemoryCache{T}.CreateList(int)"/>). The path is the <see cref="Type.Name"/> of the type.
    /// </summary>
    public static TypeDatabase<T> DefaultBinaryDatabase<T>(string rootFolder)
        where T : unmanaged
    {
        return new TypeDatabase<T>($"{typeof(T).Name}.bin",
            DiskReadWriteBinary<T>.CreateDefault(rootFolder),
            MemoryCache<T>.CreateList(1024));
    }

    /// <summary>
    /// Creates a new instance of <see cref="TypeDatabase{T}"/> with the default binary read-write method 
    /// (<see cref="DiskReadWriteBinary{T}.CreateDefault(string)"/>) and without caching 
    /// (<see cref="NoCache{T}.Instance"/>). The path is the <see cref="Type.Name"/> of the type.
    /// </summary>
    public static TypeDatabase<T> DefaultBinaryDatabaseNoCache<T>(string rootFolder)
        where T : unmanaged
    {
        return new TypeDatabase<T>($"{typeof(T).Name}.bin",
            DiskReadWriteBinary<T>.CreateDefault(rootFolder),
            NoCache<T>.Instance);
    }
}