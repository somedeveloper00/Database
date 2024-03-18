using System;

namespace Database.Core.Extensions
{
    public static class SpanExtensions
    {
        /// <summary>
        /// Extends the span by the specified length. For performance reasons, it doesn't perform any checks.
        /// </summary>
        public static Span<T> Extend<T>(this Span<T> span, int length)
        {
            Span<T> altSpan = new T[length];
            span.CopyTo(altSpan);
            return altSpan;
        }

        /// <summary>
        /// Removes a range from the span
        /// </summary>
        public static Span<T> RemoveRange<T>(this Span<T> span, int startIndex, int count)
        {
            Span<T> newSpan = new T[span.Length - count];
            span[..startIndex].CopyTo(newSpan);
            span[(startIndex + count)..].CopyTo(newSpan[startIndex..]);
            return newSpan;
        }

        /// <summary>
        /// Adds new element to the given span, in a new instance and returning it. The original span will remain unchanged.
        /// </summary>
        public static Span<T> Add<T>(this Span<T> span, T element)
        {
            var newSpan = new T[span.Length + 1];
            span.CopyTo(newSpan);
            newSpan[^1] = element;
            return newSpan;
        }
    }
}