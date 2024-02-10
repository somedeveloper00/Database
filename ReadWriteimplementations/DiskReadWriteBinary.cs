using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Database.Common;
using Database.Common.BinarySerializationImplementations;

namespace Database.ReadWriteimplementations;

/// <summary>
/// A <see cref="IReadWriteMethod{T}"/> that writes to and reads from disk using binary serialization. 
/// It saves data in a custom format that allows for quick access to specific elements.
/// 
/// <para>
///     The file format is as follows:
///     <list type="bullet">
///     
///     <item> 8 bytes: a file mark that is used to check if the file format is correct. 
///     comes from <see cref="DiskReadWriteBinary{T}.FileMark"/></item>
///     
///     <item> N * <see cref="DiskReadWriteBinary{T}.sizeof(int)"/> bytes: the position of each element in 
///     file, as <see cref="DiskReadWriteBinary{T}.sizeof(int)"/> bytes integers.</item>
///     
///     <item> The elements themselves.</item>
///     
///     </list>
/// </para>
/// </summary>
public sealed class DiskReadWriteBinary<T> : DiskReadWriteBase<T>
    where T : unmanaged
{
    /// <summary>
    /// The file mark that is used to check if the file format is correct.
    /// </summary>
    public static readonly byte[] FileMark = [0x00, 0x11, 0x22, 0x33, 0x33, 0x22, 0x11, 0x00];

    /// <summary>
    /// buffer used for reading and writing.
    /// </summary>
    private byte[] _buffer;

    /// <summary>
    /// The serializer used for serializing and deserializing the elements to and from disk.
    /// </summary>
    private readonly IBinarySerializer<T> binarySerializer;

    /// <summary>
    /// A value indicating whether the instance is busy.
    /// </summary>
    private bool _busy;

    public DiskReadWriteBinary(string rootPath, IBinarySerializer<T> binarySerializer) : base(rootPath)
    {
        _buffer = new byte[1024 * Marshal.SizeOf<T>()];
        this.binarySerializer = binarySerializer;
    }

    protected override async Task<T[]> ReadValuesFromFile(FileStream fileStream, List<int> indexes)
    {
        try
        {
            SetBusy_ThrowIfAlreadyBusy();
            await CheckMarkAndThrowIfWrong(fileStream);
            var positions = await ReadElementPositions(fileStream);

            // allocate necessary memories
            T[] result = new T[indexes.Count];

            // read elements
            for (int i = 0; i < indexes.Count; i++)
            {
                int index = indexes[i];
                int nextPosition = (int)(index >= positions.Length - 1 ? fileStream.Length : positions[index + 1]);
                int length = nextPosition - positions[index];

                if (length > 0)
                {
                    if (_buffer.Length <= length)
                    {
                        Array.Resize(ref _buffer, length);
                    }
                    var c = await fileStream.ReadAsync(_buffer.AsMemory(0, length));
                    if (c != length)
                    {
                        ThrowFileFormatException($"could not read element at index {index}. read bytes:{c}, expected: {length}");
                    }
                    result[index] = binarySerializer.Deserialize(_buffer[..length]);
                }
                else
                {
                    result[index] = default;
                }
            }
            return result;
        }
        finally
        {
            _busy = false;
        }
    }

    protected override async Task WriteValuesToFile(FileStream fileStream, List<T> values, List<int> indexes)
    {
        try
        {
            SetBusy_ThrowIfAlreadyBusy();
            byte[][] db = null!;

            if (fileStream.Length != 0)
            {
                // check file mark and read all the elements
                await CheckMarkAndThrowIfWrong(fileStream);
                var positions = await ReadElementPositions(fileStream);
                db = await ReadAllElements(fileStream, positions);
            }
            else
            {
                // write mark
                await fileStream.WriteAsync(FileMark);
                db = [];
            }

            // insert new values to db
            int maxIndex = indexes.Max();
            if (db.Length <= maxIndex)
            {
                Array.Resize(ref db, maxIndex + 1);
            }
            for (int i = 0; i < indexes.Count; i++)
            {
                db[indexes[i]] = binarySerializer.Serialize(values[i]);
            }

            // write new positions
            fileStream.Seek(FileMark.Length, SeekOrigin.Begin);
            int elementPosition = (int)fileStream.Position + db.Length * sizeof(int);
            for (int i = 0; i < db.Length; i++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan(0, sizeof(int)), elementPosition);
                await fileStream.WriteAsync(_buffer.AsMemory(0, sizeof(int)));
                elementPosition += db[i]?.Length ?? 0;
            }

            // write all elements
            for (int i = 0; i < db.Length; i++)
            {
                if (db[i] is not null && db[i].Length != 0)
                    await fileStream.WriteAsync(db[i]);
            }
            fileStream.SetLength(fileStream.Position);
        }
        finally
        {
            _busy = false;
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="DiskReadWriteBinary{T}"/> with the <see cref="MemoryCopyBinarySerializer{T}"/> serializer.
    /// </summary>
    public static DiskReadWriteBinary<T> CreateDefault(string rootPath)
    {
        return new(rootPath, new MemoryCopyBinarySerializer<T>());
    }

    /// <summary>
    /// reads byte arrays from the file.
    /// </summary>
    private static async Task<byte[][]> ReadAllElements(FileStream fileStream, int[] allPositions)
    {
        byte[][] result = new byte[allPositions.Length][];
        fileStream.Seek(allPositions[0], SeekOrigin.Begin);
        for (int i = 0; i < allPositions.Length; i++)
        {
            // seek
            int position = allPositions[i];
            int nextPosition = i == allPositions.Length - 1
                ? (int)fileStream.Length
                : allPositions[i + 1];
            fileStream.Seek(position - fileStream.Position, SeekOrigin.Current);

            // read
            int length = nextPosition - position;
            result[i] = new byte[length];
            int c = await fileStream.ReadAsync(result[i].AsMemory(0));
            if (c != length)
            {
                ThrowFileFormatException();
            }
        }
        return result;
    }

    /// <summary>
    /// Reads the positions of the elements from the file.
    /// </summary>
    private async Task<int[]> ReadElementPositions(FileStream fileStream)
    {
        int c = await fileStream.ReadAsync(_buffer.AsMemory(0, sizeof(int)));
        if (c != sizeof(int))
        {
            ThrowFileFormatException();
        }
        int firstElementPosition = BinaryPrimitives.ReadInt32LittleEndian(_buffer.AsSpan(0, sizeof(int)));

        // check if the first element's position seems correct
        if ((firstElementPosition - FileMark.Length) % sizeof(int) != 0)
        {
            ThrowFileFormatException();
        }

        int count = (firstElementPosition - FileMark.Length) / sizeof(int);
        int[] positions = new int[count];
        positions[0] = firstElementPosition;

        // read all element's positions
        if (count > 1)
        {
            int size = sizeof(int) * (count - 1);
            c = await fileStream.ReadAsync(_buffer.AsMemory(0, size));
            if (c != size)
            {
                ThrowFileFormatException();
            }
            for (int i = 0; i < positions.Length - 1; i++)
            {
                positions[i + 1] = BitConverter.ToInt32(_buffer.AsSpan(i * sizeof(int), sizeof(int)));
            }
        }
        else
        {
            positions[0] = firstElementPosition;
        }

        return positions;
    }

    /// <summary>
    /// Gets an approximate size of <typeparamref name="T"/> in bytes by doing a test.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoOptimization)]
    private static int GetApproximateSizeOfT()
    {
        const int TestCount = 5;
        long before = GC.GetTotalMemory(false);
        for (int i = 0; i < TestCount; i++)
        {
            new T();
        }
        long after = GC.GetTotalMemory(false);
        return (int)(after - before) / TestCount;
    }

    /// <summary>
    /// Checks file mark by <see cref="CheckFileMark(FileStream)"/>. Throws 
    /// if the file format is not correct, using <see cref="ThrowFileFormatException"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task CheckMarkAndThrowIfWrong(FileStream fileStream)
    {
        if (!await CheckFileMark(fileStream))
        {
            ThrowFileFormatException();
        }
    }

    /// <summary>
    /// Reads the first bytes of the file looking for <see cref="FileMark"/>. Returns whether or not the file 
    /// format is correct.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task<bool> CheckFileMark(FileStream fileStream)
    {
        byte[] buffer = new byte[FileMark.Length];
        var c = await fileStream.ReadAsync(buffer.AsMemory(0, FileMark.Length));
        if (c != FileMark.Length || !AreArraysEqual(buffer, FileMark))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if two arrays are equal, disregarding nullability of either arrays.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreArraysEqual<F>(F[] a1, F[] a2)
    {
        if (a1!.Length != a2!.Length)
        {
            return false;
        }
        for (int i = 0; i < a1.Length; i++)
        {
            if (!a1![i]!.Equals(a2![i]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Throws an exception if the instance is busy. (<seealso cref="_busy"/>)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBusy_ThrowIfAlreadyBusy()
    {
        if (_busy)
        {
            throw new Exception("The instance is busy.");
        }
        _busy = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowFileFormatException(string? message = null)
    {
        throw new Exception($"The file format is not correct.{(string.IsNullOrEmpty(message) ? string.Empty : $" {message}")}");
    }
}
