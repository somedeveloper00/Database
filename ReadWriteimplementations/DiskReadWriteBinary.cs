using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Database.Common;

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
///     <item> N * <see cref="DiskReadWriteBinary{T}.IntByteLength"/> bytes: the position of each element in 
///     file, as <see cref="DiskReadWriteBinary{T}.IntByteLength"/> bytes integers.</item>
///     
///     <item> The elements themselves.</item>
///     
///     </list>
/// </para>
/// </summary>
public sealed class DiskReadWriteBinary<T> : DiskReadWriteBase<T>
    where T : new()
{
    /// <summary>
    /// The file mark that is used to check if the file format is correct.
    /// </summary>
    public static readonly byte[] FileMark = [0x00, 0x11, 0x22, 0x33, 0x33, 0x22, 0x11, 0x00];

    /// <summary>
    /// The size of each integer in file format, in bytes.
    /// </summary>
    public const int IntByteLength = 4;

    /// <summary>
    /// buffer used for reading and writing.
    /// </summary>
    private readonly byte[] _buffer;

    /// <summary>
    /// buffer used for reading elements.
    /// </summary>
    private readonly List<byte> _elementBuffer;

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
        int size;
        if (typeof(T).IsValueType)
        {
            size = Marshal.SizeOf<T>();
        }
        else
        {
            size = GetApproximateSizeOfT();
        }
        _buffer = new byte[1024 * size];
        _elementBuffer = new(1024 * size);
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
            for (int i = 0; i < positions.Length; i++)
            {
                int nextPosition = (int)(i == positions.Length - 1 ? fileStream.Length : positions[i + 1]);
                int remainingBytes = nextPosition - positions[i];
                _elementBuffer.Clear();

                while (remainingBytes > 0)
                {
                    int v = await fileStream.ReadAsync(_buffer.AsMemory(0, Math.Min(remainingBytes, _buffer.Length)));
                    remainingBytes -= v;
                    _elementBuffer.AddRange(_buffer.AsSpan(0, v));
                }

                result[i] = binarySerializer.Deserialize([.. _elementBuffer]);
                _elementBuffer.Clear();
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
            await CheckMarkAndThrowIfWrong(fileStream);
            var positions = await ReadElementPositions(fileStream);

            // get the writing bytes ready
            byte[][] writingBytes = new byte[values.Count][];
            for (int i = 0; i < values.Count; i++)
            {
                writingBytes[i] = binarySerializer.Serialize(values[i]);
            }

            // find new positions of the elements
            var newPositions = new int[values.Count];
            newPositions[0] = positions.Length > 0 ? positions[0] : FileMark.Length + IntByteLength;
            for (int i = 1; i < positions.Length; i++)
            {
                newPositions[i] = writingBytes[i].Length + newPositions[i - 1];
            }

            // read all elements
            byte[][] oldBytes = new byte[positions.Length][];
            for (int i = 0; i < positions.Length; i++)
            {
                int nextPosition = (int)(i == positions.Length - 1 ? fileStream.Length : positions[i + 1]);
                int remainingBytes = nextPosition - positions[i];
                _elementBuffer.Clear();

                while (remainingBytes > 0)
                {
                    int v = await fileStream.ReadAsync(_buffer.AsMemory(0, Math.Min(remainingBytes, _buffer.Length)));
                    remainingBytes -= v;
                    _elementBuffer.AddRange(_buffer.AsSpan(0, v));
                }

                oldBytes[i] = [.. _elementBuffer];
                _elementBuffer.Clear();
            }

            // write new elements and update the positions
            fileStream.Seek(newPositions[0], SeekOrigin.Begin);
            for (int i = 0; i < newPositions.Length; i++)
            {
                int index = indexes.IndexOf(i);
                if (index != -1)
                {
                    await fileStream.WriteAsync(writingBytes[index]);
                }
                else
                {
                    await fileStream.WriteAsync(oldBytes[i]);
                }
            }
        }
        finally
        {
            _busy = false;
        }
    }

    /// <summary>
    /// Reads the positions of the elements from the file.
    /// </summary>
    private async Task<int[]> ReadElementPositions(FileStream fileStream)
    {
        int c = await fileStream.ReadAsync(_buffer.AsMemory(0, IntByteLength));
        if (c != 1)
        {
            ThrowFileFormatException();
        }
        int firstElementPosition = BitConverter.ToInt32(_buffer.AsSpan(0, IntByteLength));

        // check if the first element's position seems correct
        if (firstElementPosition - 8 % IntByteLength != 0)
        {
            ThrowFileFormatException();
        }

        int count = (firstElementPosition - 8) / IntByteLength;
        int[] positions = new int[count];

        // read all element's positions
        if (count > 1)
        {
            c = await fileStream.ReadAsync(_buffer.AsMemory(0, IntByteLength * (count - 1)));
            if (c != IntByteLength * count)
            {
                ThrowFileFormatException();
            }
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = BitConverter.ToInt32(_buffer.AsSpan(i * IntByteLength, IntByteLength));
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
    /// Reads the first bytes of the file looking for <see cref="FileMark"/>. Throws 
    /// if the file format is not correct, using <see cref="ThrowFileFormatException"/>
    /// </summary>
    private static async Task CheckMarkAndThrowIfWrong(FileStream fileStream)
    {
        byte[] buffer = new byte[FileMark.Length];
        int c = await fileStream.ReadAsync(buffer);
        if (c != buffer.Length || !AreArraysEqual(buffer, FileMark))
        {
            ThrowFileFormatException();
        }
    }

    /// <summary>
    /// Checks if two arrays are equal, disregarding nullability of either arrays.
    /// </summary>
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
    private static void ThrowFileFormatException()
    {
        throw new Exception("The file format is not correct.");
    }
}
