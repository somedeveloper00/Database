using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Database.Common;
using Database.Common.BinarySerializationImplementations;

namespace Database.ReadWriteImplementations;

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
///     <item> N * <c>sizeof(int)</c> bytes: the position of each element in 
///     file, as <c>sizeof(int)</c> bytes integers.</item>
///     
///     <item> The elements themselves.</item>
///     
///     </list>
/// </para>
/// </summary>
public sealed class DiskReadWriteBinary<T>(string rootPath, IBinarySerializer<T> binarySerializer)
    : DiskReadWriteBase<T>(rootPath,
        FileMode.OpenOrCreate, FileMode.OpenOrCreate, FileMode.OpenOrCreate,
        FileAccess.Read, FileAccess.ReadWrite, FileAccess.ReadWrite)
    where T : unmanaged
{
    /// <summary>
    /// The file mark that is used to check if the file format is correct.
    /// </summary>
    public static readonly byte[] FileMark = [0x00, 0x11, 0x22, 0x33, 0x33, 0x22, 0x11, 0x00];

    /// <summary>
    /// The serializer used for serializing and deserializing the elements to and from disk.
    /// </summary>
    private readonly IBinarySerializer<T> binarySerializer = binarySerializer;

    /// <summary>
    /// Simple constructor for default values
    /// </summary>
    public DiskReadWriteBinary(string rootPath)
        : this(rootPath, new MemoryCopyBinarySerializer<T>()) { }

    protected override async Task<T[]> ReadValuesFromFile(FileStream fileStream, List<int> indexes)
    {
        // read
        if (fileStream.Length == 0)
        {
            throw new Exception("file has no content");
        }
        await CheckMarkAndThrowIfWrong(fileStream);
        var positions = await DiskReadWriteBinary<T>.ReadElementPositions(fileStream);
        var allElementsBytes = await ReadAllElements(fileStream, positions);

        // select
        T[] result = new T[indexes.Count];
        for (int i = 0; i < indexes.Count; i++)
        {
            result[i] = binarySerializer.Deserialize(allElementsBytes[indexes[i]]);
        }

        return result;
    }

    protected override async Task WriteValuesToFile(FileStream fileStream, List<T> values, List<int> indexes)
    {
        // read
        await CheckMarkAndThrowIfWrong(fileStream);
        List<byte[]> allElementsBytes;
        if (fileStream.Length > 0)
        {
            var positions = await DiskReadWriteBinary<T>.ReadElementPositions(fileStream);
            allElementsBytes = await ReadAllElements(fileStream, positions);
        }
        else
        {
            allElementsBytes = [];
        }

        // override elements
        for (int i = 0; i < allElementsBytes.Count; i++)
        {
            int v = indexes.BinarySearch(i);
            if (v >= 0)
            {
                allElementsBytes[i] = binarySerializer.Serialize(values[indexes[v]]);
            }
        }

        // new elements
        int maxIndex = indexes.Max();
        if (allElementsBytes.Capacity < maxIndex + 1)
            allElementsBytes.Capacity = maxIndex + 1;
        for (int i = allElementsBytes.Count; i <= maxIndex; i++)
        {
            int v = indexes.BinarySearch(i);
            if (v >= 0)
            {
                allElementsBytes.Add([]);
            }
            else
            {
                v = ~v;
                allElementsBytes.Add(binarySerializer.Serialize(values[v]));
            }
        }

        // write
        await WriteAllToFile(fileStream, allElementsBytes);
    }

    protected override async Task DeleteValueFromFile(FileStream fileStream, List<int> indexes)
    {
        if (fileStream.Length == 0)
        {
            throw new Exception("file has no content");
        }

        // read
        await CheckMarkAndThrowIfWrong(fileStream);
        var positions = await DiskReadWriteBinary<T>.ReadElementPositions(fileStream);
        var allElementsBytes = await ReadAllElements(fileStream, positions);

        // remove elements
        for (int i = indexes.Count - 1; i >= 0; i--)
        {
            allElementsBytes.RemoveAt(indexes[i]);
        }

        // write
        await WriteAllToFile(fileStream, allElementsBytes);
    }

    /// <summary>
    /// Writes to the whole file, the file mark and positions and elements; everything
    /// </summary>
    private static async Task WriteAllToFile(FileStream fileStream, List<byte[]> elements)
    {
        // write file mark
        fileStream.Seek(0, SeekOrigin.Begin);
        await fileStream.WriteAsync(FileMark);

        // write positions
        int elementPosition = (int)fileStream.Position + elements.Count * sizeof(int);
        byte[] buffer = new byte[sizeof(int)];
        for (int i = 0; i < elements.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer, elementPosition);
            await fileStream.WriteAsync(buffer);
            elementPosition += elements[i].Length;
        }

        // write elements
        for (int i = 0; i < elements.Count; i++)
        {
            await fileStream.WriteAsync(elements[i]);
        }
    }

    /// <summary>
    /// reads byte arrays from the file.
    /// </summary>
    private static async Task<List<byte[]>> ReadAllElements(FileStream fileStream, int[] allPositions)
    {
        List<byte[]> result = new(allPositions.Length);
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
            if (length > 0)
            {
                byte[] buffer = new byte[length];
                int c = await fileStream.ReadAsync(buffer.AsMemory(0));
                if (c != length)
                {
                    ThrowFileFormatException();
                }
                result.Add(buffer);
            }
            else
            {
                result.Add([]);
            }
        }
        return result;
    }

    /// <summary>
    /// Reads the positions of the elements from the file.
    /// </summary>
    private static async Task<int[]> ReadElementPositions(FileStream fileStream)
    {
        byte[] buffer = new byte[sizeof(int)];
        int c = await fileStream.ReadAsync(buffer);
        if (c != buffer.Length)
        {
            ThrowFileFormatException();
        }
        int firstElementPosition = BinaryPrimitives.ReadInt32LittleEndian(buffer);

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
            buffer = new byte[firstElementPosition - FileMark.Length - sizeof(int)];
            c = await fileStream.ReadAsync(buffer);
            if (c != buffer.Length)
            {
                ThrowFileFormatException();
            }
            for (int i = 0; i < positions.Length - 1; i++)
            {
                positions[i + 1] = BitConverter.ToInt32(buffer.AsSpan(i * sizeof(int), sizeof(int)));
            }
        }

        return positions;
    }

    /// <summary>
    /// Checks file mark by <see cref="CheckFileMark(FileStream)"/>. Throws 
    /// if the file format is not correct, using <see cref="ThrowFileFormatException"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task CheckMarkAndThrowIfWrong(FileStream fileStream)
    {
        if (fileStream.Length > 0 && !await CheckFileMark(fileStream))
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
        if (a1.Length != a2.Length)
        {
            return false;
        }
        for (int i = 0; i < a1.Length; i++)
        {
            if (!a1[i]!.Equals(a2[i]))
            {
                return false;
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowFileFormatException(string? message = null)
    {
        throw new($"The file format is not correct.{(string.IsNullOrEmpty(message) ? string.Empty : $" {message}")}");
    }
}