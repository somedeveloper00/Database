namespace Database.ReadWriteimplementations;

/// <summary>
/// a <see cref="IReadWriteMethod{T}"/> that writes to and reads from disk
/// </summary>
public abstract class DiskReadWriteBase<T>(string rootPath) : IReadWriteMethod<T>
    where T : new()
{
    public readonly string rootPath = rootPath;
    private FileStream? _readFile;
    private FileStream? _writeFile;
    private readonly List<int> _readIndexes = new(1024);
    private readonly List<int> _writeIndexes = new(1024);
    private readonly List<T> _writeValues = new(1024);

    public async Task BeginRead(string filePath)
    {
        filePath = GetFullPath(filePath);
        if (_readFile is not null)
        {
            await _readFile.DisposeAsync();
        }
        _readFile = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
        _readIndexes.Clear();
    }

    public Task Read(int index)
    {
        int i = _readIndexes.BinarySearch(index);
        if (i < 0)
        {
            i = ~i;
            _readIndexes.Insert(i, index);
        }
        return Task.CompletedTask;
    }

    public async Task<T[]> EndRead()
    {
        if (_readFile is null || !_readFile.CanRead || !_readFile.CanSeek)
        {
            throw new FileLoadException("The file is not open for reading.");
        }

        var result = new T[_readIndexes.Count];
        if (_readIndexes.Count > 0)
        {
            result = await ReadValuesFromFile(_readFile, _readIndexes);
        }

        await _readFile.DisposeAsync();
        return result;
    }

    public async Task BeginWrite(string filePath)
    {
        filePath = GetFullPath(filePath);
        if (_writeFile is not null)
        {
            await _writeFile.DisposeAsync();
        }
        _writeFile = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        _writeIndexes.Clear();
        _writeValues.Clear();
    }

    public Task Write(T value, int index)
    {
        int i = _writeIndexes.BinarySearch(index);
        if (i < 0)
        {
            i = ~i;
            _writeIndexes.Insert(i, index);
            _writeValues.Insert(i, value);
        }
        return Task.CompletedTask;
    }

    public async Task EndWrite()
    {
        if (_writeFile is null || !_writeFile.CanWrite || !_writeFile.CanSeek)
        {
            throw new FileLoadException("The file is not open for writing.");
        }
        if (_writeIndexes.Count > 0)
        {
            await WriteValuesToFile(_writeFile, _writeValues, _writeIndexes);
        }

        await _writeFile.DisposeAsync();
    }

    private string GetFullPath(string relativePath) => Path.Combine(rootPath, relativePath);

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
}