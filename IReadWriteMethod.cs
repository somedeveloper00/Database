namespace Database;

/// <summary>
/// Represents a method that can read and write data.
/// </summary>
public interface IReadWriteMethod<T>
{
    /// <summary>
    /// Optimizes for writing.
    /// </summary>
    Task BeginWrite(string filePath);

    /// <summary>
    /// Ends the write optimization.
    /// </summary>
    Task EndWrite();

    /// <summary>
    /// Writes the value at the specified index. The results may not be immediately saved, 
    /// for that you should call <see cref="EndWrite(string)"/>.
    /// </summary>
    Task Write(T value, int index);

    /// <summary>
    /// Optimizes for reading.
    /// </summary>
    Task BeginRead(string filePath);

    /// <summary>
    /// Ends the read optimization.
    /// </summary>
    Task<T[]> EndRead();

    /// <summary>
    /// Reads the value at the specified index. Saves the result locally. You can fetch the results by calling 
    /// <see cref="EndRead"/>
    /// </summary>
    Task Read(int index);
}
