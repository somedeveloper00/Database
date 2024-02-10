using Database.Common;

namespace Database.Extensions;

public static class StreamExtensions
{
    public static unsafe byte[] ReadAllBytes(this Stream stream, int bufferSize)
    {
        list<byte> buffer = new(bufferSize);
        int totalRead = 0;
        do
        {
            int c = stream.Read(buffer.AsSpan(totalRead, totalRead + bufferSize, true));
            if (c == 0) // end of stream
            {
                break;
            }
            totalRead += c;
        }
        while (true);
        return buffer.ToArray();
    }
}