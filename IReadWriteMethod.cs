namespace Database;

/// <summary>
/// Represents a method that can read and write data.
/// </summary>
public interface IReadWriteMethod<T>
{
    /// <summary>
    /// Starts writing process.
    /// </summary>
    Task BeginWrite(string path);

    /// <summary>
    /// Marks the element at the specified index for writing.
    /// </summary>
    Task Write(T value, int index);

    /// <summary>
    /// Writes all elements marked for writing.
    /// </summary>
    Task EndWrite();

    /// <summary>
    /// Starts reading process.
    /// </summary>
    Task BeginRead(string path);

    /// <summary>
    /// Marks the element at the specified index for read.
    /// </summary>
    Task Read(int index);

    /// <summary>
    /// Reads all elements marked for reading.
    /// </summary>
    Task<T[]> EndRead();

    /// <summary>
    /// Starts deletion process.
    /// </summary>
    Task BeginDelete(string path);

    /// <summary>
    /// Marks the element at the spciefied index for deletion.
    /// </summary>
    Task Delete(int index);

    /// <summary>
    /// Deletes all elements marked for deletion.
    /// </summary>
    /// <returns></returns>
    Task EndDelete();
}
