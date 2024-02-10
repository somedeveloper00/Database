using System.Buffers;

namespace Database.Extensions;

public static class StreamExtensions
{
    private const int DefaultBufferSize = 1024;

    /// <summary>
    /// Reads all bytes from the given <paramref name="stream"/> asynchronously
    /// </summary>
    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream, int bufferSize = DefaultBufferSize)
    {
        if (stream.Length == 0)
        {
            return [];
        }
        List<byte> bytes = new(bufferSize);
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        do
        {
            int c = await stream.ReadAsync(buffer.AsMemory(0, bufferSize));
            if (c == 0) // end of stream
            {
                break;
            }
            bytes.AddRange(buffer);
        }
        while (true);
        ArrayPool<byte>.Shared.Return(buffer);
        return [.. bytes];
    }
}