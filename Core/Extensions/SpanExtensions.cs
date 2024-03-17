using System;

namespace Database.Core.Extensions
{
    internal static class SpanExtensions
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
    }
}