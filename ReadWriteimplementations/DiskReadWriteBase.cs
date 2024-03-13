namespace Database.ReadWriteImplementations;

/// <summary>
/// a <see cref="IReadWriteMethod{T}"/> that writes to and reads from disk
/// </summary>
public abstract class DiskReadWriteBase<T>(string rootPath) : IReadWriteMethod<T>
    where T : new()
{
    public readonly string directoryPath = rootPath;
    protected readonly FileMode ReadFileMode = FileMode.OpenOrCreate;
    protected readonly FileMode WriteFileMode = FileMode.OpenOrCreate;
    protected readonly FileMode DeleteFileMode = FileMode.OpenOrCreate;
    protected readonly FileAccess ReadFileAccess = FileAccess.ReadWrite;
    protected readonly FileAccess WriteFileAccess = FileAccess.ReadWrite;
    protected readonly FileAccess DeleteFileAccess = FileAccess.ReadWrite;
    private FileStream _stream = null!;
    private readonly List<int> _indexes = new(1024);
    private readonly List<T> _writeValues = new(1024);

    /// <summary>
    /// constructor for inherited classes to use custom <see cref="FileMode"/>s and 
    /// <see cref="FileAccess"/>es.
    /// </summary>
    protected DiskReadWriteBase(string rootPath, FileMode readFileMode, FileMode writeFileMode,
        FileMode deleteFileMode, FileAccess readFileAccess, FileAccess writeFileAccess,
        FileAccess deleteFileAccess) : this(rootPath)
    {
        ReadFileMode = readFileMode;
        WriteFileMode = writeFileMode;
        DeleteFileMode = deleteFileMode;
        ReadFileAccess = readFileAccess;
        WriteFileAccess = writeFileAccess;
        DeleteFileAccess = deleteFileAccess;
    }

    public async Task BeginRead(string filePath)
    {
        await InitializeStreamSafe(filePath, ReadFileMode, ReadFileAccess);
        _indexes.Clear();
    }

    public Task Read(int index)
    {
        if (TryGetNewSortedIndex(index, out int pos))
        {
            _indexes.Insert(pos, index);
        }
        return Task.CompletedTask;
    }

    public async Task<T[]> EndRead()
    {
        ThrowIfStreamIsNotOpenedProperly(
            canRead: ReadFileAccess is FileAccess.Read or FileAccess.ReadWrite,
            canWrite: ReadFileAccess is FileAccess.Write or FileAccess.ReadWrite);

        var result = new T[_indexes.Count];
        if (_indexes.Count > 0)
        {
            result = await ReadValuesFromFile(_stream, _indexes);
        }

        await _stream.DisposeAsync();
        return result;
    }

    public async Task BeginWrite(string filePath)
    {
        Directory.CreateDirectory(directoryPath);
        await InitializeStreamSafe(filePath, WriteFileMode, WriteFileAccess);
        _indexes.Clear();
        _writeValues.Clear();
    }

    public Task Write(T value, int index)
    {
        if (TryGetNewSortedIndex(index, out var pos))
        {
            _indexes.Insert(pos, index);
            _writeValues.Insert(pos, value);
        }
        return Task.CompletedTask;
    }

    public async Task EndWrite()
    {
        ThrowIfStreamIsNotOpenedProperly(
            canRead: WriteFileAccess is FileAccess.Read or FileAccess.ReadWrite,
            canWrite: WriteFileAccess is FileAccess.Write or FileAccess.ReadWrite);
        if (_indexes.Count > 0)
        {
            await WriteValuesToFile(_stream, _writeValues, _indexes);
        }

        await _stream.DisposeAsync();
    }

    public async Task BeginDelete(string filePath)
    {
        await InitializeStreamSafe(filePath, DeleteFileMode, DeleteFileAccess);
        _indexes.Clear();
    }

    public Task Delete(int index)
    {
        if (TryGetNewSortedIndex(index, out var pos))
        {
            _indexes.Insert(pos, index);
        }
        return Task.CompletedTask;
    }

    public async Task EndDelete()
    {
        ThrowIfStreamIsNotOpenedProperly(
            canRead: DeleteFileAccess is FileAccess.Read or FileAccess.ReadWrite,
            canWrite: DeleteFileAccess is FileAccess.Write or FileAccess.ReadWrite);
        if (_indexes.Count > 0)
        {
            await DeleteValueFromFile(_stream, _indexes);
        }
    }

    /// <summary>
    /// Finds where the <paramref name="index"/> can be put in <see cref="_indexes"/> where it'll 
    /// be considered sorted, and returns whether or not the <paramref name="index"/> is a new index 
    /// in the list, while populating <paramref name="sortedIndex"/> with the desired sorted position.
    /// </summary>
    private bool TryGetNewSortedIndex(in int index, out int sortedIndex)
    {
        sortedIndex = _indexes.BinarySearch(index);
        if (sortedIndex < 0)
        {
            sortedIndex = ~sortedIndex;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Throws if <see cref="_stream"/> cannot operate the desired read/write operations or is null
    /// </summary>
    private void ThrowIfStreamIsNotOpenedProperly(bool canRead, bool canWrite)
    {
        if (_stream is null || _stream.CanRead != canRead || _stream.CanWrite != canWrite)
        {
            throw new Exception("File not opened properly");
        }
    }

    /// <summary>
    /// Initializes <see cref="_stream"/> safely, closing any previous strem.
    /// </summary>
    private async Task InitializeStreamSafe(string filePath, FileMode fileMode, FileAccess fileAccess)
    {
        if (_stream is not null)
        {
            await _stream.DisposeAsync();
        }
        _stream = new FileStream(GetFullPath(filePath), fileMode, fileAccess);
    }

    private string GetFullPath(string relativePath) => Path.Combine(directoryPath, relativePath);

    /// <summary>
    /// Reads the value from the file stream at the specified index. <paramref name="indexes"/> is 
    /// sorted. position of <paramref name="fileStream"/> is zero.
    /// </summary>
    protected abstract Task<T[]> ReadValuesFromFile(FileStream fileStream, List<int> indexes);

    /// <summary>
    /// Writes the value to the file stream at the specified index. <paramref name="indexes"/> is 
    /// sorted, and <paramref name="values"/> is in the same order as <paramref name="indexes"/>. 
    /// position of <paramref name="fileStream"/> is zero.
    /// </summary>
    protected abstract Task WriteValuesToFile(FileStream fileStream, List<T> values, List<int> indexes);

    /// <summary>
    /// Deletes values at the specified indexes. <paramref name="indexes"/> is sorted. 
    /// position of <paramref name="fileStream"/> is zero.
    /// </summary>
    protected abstract Task DeleteValueFromFile(FileStream fileStream, List<int> indexes);
}