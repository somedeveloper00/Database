#pragma warning disable IDE1006 // Naming Styles

using System.Collections;
using System.Runtime.CompilerServices;

namespace Database.Common;

/// <summary>
/// An unmanaged string with the size of 32
/// </summary>
public readonly struct ustr_32 : IEnumerable<char>
{
    private readonly UnmanagedArray_32<char> _buffer;

    public ustr_32(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 32; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 32)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_32 ustr) => ustr.ToString();

    public static implicit operator ustr_32(string str) => new(str);
}

/// <summary>
/// An unmanaged string with the size of 64
/// </summary>
public readonly struct ustr_64 : IEnumerable<char>
{
    private readonly UnmanagedArray_64<char> _buffer;

    public ustr_64(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 64; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 64)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_64 ustr) => ustr.ToString();

    public static implicit operator ustr_64(string str) => new(str);
}

/// <summary>
/// An unmanaged string with the size of 128
/// </summary>
public readonly struct ustr_128 : IEnumerable<char>
{
    private readonly UnmanagedArray_128<char> _buffer;

    public ustr_128(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 128; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 128)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_128 ustr) => ustr.ToString();

    public static implicit operator ustr_128(string str) => new(str);
}

/// <summary>
/// An unmanaged string with the size of 256
/// </summary>
public readonly struct ustr_256 : IEnumerable<char>
{
    private readonly UnmanagedArray_256<char> _buffer;

    public ustr_256(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 256; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 256)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_256 ustr) => ustr.ToString();

    public static implicit operator ustr_256(string str) => new(str);
}

/// <summary>
/// An unmanaged string with the size of 512
/// </summary>
public readonly struct ustr_512 : IEnumerable<char>
{
    private readonly UnmanagedArray_512<char> _buffer;

    public ustr_512(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 512; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 512)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_512 ustr) => ustr.ToString();

    public static implicit operator ustr_512(string str) => new(str);
}

/// <summary>
/// An unmanaged string with the size of 1024
/// </summary>
public readonly struct ustr_1024 : IEnumerable<char>
{
    private readonly UnmanagedArray_1024<char> _buffer;

    public ustr_1024(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 1024; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 1024)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_1024 ustr) => ustr.ToString();

    public static implicit operator ustr_1024(string str) => new(str);
}

/// <summary>
/// An unmanaged string with the size of 2048
/// </summary>
public readonly struct ustr_2048 : IEnumerable<char>
{
    private readonly UnmanagedArray_2048<char> _buffer;

    public ustr_2048(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 2048; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 2048)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_2048 ustr) => ustr.ToString();

    public static implicit operator ustr_2048(string str) => new(str);
}

/// <summary>
/// An unmanaged string with the size of 4096
/// </summary>
public readonly struct ustr_4096 : IEnumerable<char>
{
    private readonly UnmanagedArray_4096<char> _buffer;

    public ustr_4096(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 4096; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 4096)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_4096 ustr) => ustr.ToString();

    public static implicit operator ustr_4096(string str) => new(str);
}

/// <summary>
/// An unmanaged string with the size of 1048576
/// </summary>
public readonly struct ustr_1048576 : IEnumerable<char>
{
    private readonly UnmanagedArray_1048576<char> _buffer;

    public ustr_1048576(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            _buffer[i] = str[i];
        }
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = 0; i < 1048576; i++)
        {
            if (_buffer[i] == 0)
            {
                yield break;
            }
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public readonly char this[int index] => _buffer[ThrowIfOutIfBounds(index)];

    public override readonly string ToString() => new(_buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ThrowIfOutIfBounds(int index)
    {
        if (index < 0 | index >= 1048576)
            throw new IndexOutOfRangeException(index.ToString());
        return index;
    }

    public static implicit operator string(ustr_1048576 ustr) => ustr.ToString();

    public static implicit operator ustr_1048576(string str) => new(str);
}