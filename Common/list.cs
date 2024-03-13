#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS9081 // A result of a stackalloc expression of this type in this context may be exposed outside of the containing method

using System.Runtime.CompilerServices;

namespace Database.Common;

/// <summary>
/// A list implementation inside stack.
/// </summary>
public unsafe ref struct list<T>(int capacity) where T : unmanaged
{
    private Span<T> _values = stackalloc T[capacity];
    private int _length;

    public T this[int index]
    {
        readonly get => _values[ThrowIfOutOfBounds(index)];
        set => _values[ThrowIfOutOfBounds(index)] = value;
    }

    public readonly int Count => _length;

    /// <summary>
    /// Adds the new element at the last position of the list
    /// </summary>
    public void Add(T value)
    {
        if (_length == _values.Length)
        {
            ChangeCapacity(_values.Length * 2);
        }
        _values[_length++] = value;
    }

    /// <summary>
    /// Insert the new element into the list. It can also be inserted at the very tip/length 
    /// of the list 
    /// </summary>
    public void Insert(int index, T value)
    {
        if (_length == _values.Length)
        {
            ChangeCapacity(_values.Length * 2);
        }

        if (index > 0 & index <= _length)
        {
            // push elements forward
            for (int i = _length - 1; i > index; i--)
            {
                _values[i + 1] = _values[i];
            }
            _values[index] = value;
            return;
        }
        throw new IndexOutOfRangeException(index.ToString());
    }

    /// <summary>
    /// Removes the first equal value in the list. Returns whether or not an element got removed.
    /// </summary>
    public bool Remove(T value)
    {
        for (int i = 0; i < _length; i++)
        {
            if (_values[i].Equals(value))
            {
                RemoveAt(i);
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Removes element at the specified index
    /// </summary>
    public void RemoveAt(int index)
    {
        ThrowIfOutOfBounds(index);
        for (int i = index; i < _length - 1; i++)
        {
            _values[i] = _values[i + 1];
        }
        _length--;
    }


    /// <summary>
    /// Ensure that the capacity of the backing array is sufficient for the given 
    /// <paramref name="capacity"/>
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        if (_values.Length < capacity)
        {
            ChangeCapacity(capacity);
        }
    }

    /// <summary>
    /// Returns a <see cref="Span{T}"/> of the list. if <paramref name="ensureSize"/> is true, 
    /// the list will enlarge itself automatically to contain the requested range. Be wary of 
    /// <see cref="StackOverflowException"/> when setting it to true.
    /// </summary>
    public Span<T> AsSpan(int startIndex, int count, bool ensureSize)
    {
        if (!ensureSize)
        {
            ThrowIfOutOfBounds(startIndex + count - 1);
        }
        else
        {
            EnsureCapacity(startIndex + count);
            if (startIndex + count > _length)
            {
                _length = startIndex + count;
            }
        }
        return _values.Slice(startIndex, count);
    }

    /// <summary>
    /// Generates a new instance of <see cref="Array"/> of the list and returns it.
    /// </summary>
    public readonly T[] ToArray()
    {
        var result = new T[_length];
        _values.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Changes the legntgh of <see cref="_values"/>, while copying old data along with it
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ChangeCapacity(int newCapacity)
    {
        var old = _values;
        _values = stackalloc T[newCapacity];
        old.CopyTo(_values);
    }

    /// <summary>
    /// Checks the range of <paramref name="index"/> and either throws 
    /// <see cref="IndexOutOfRangeException"/> or returns the input index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int ThrowIfOutOfBounds(int index)
    {
        return index < 0 | index >= _length
            ? throw new IndexOutOfRangeException(index.ToString())
            : index;
    }
}