using System;

namespace Database.Core.Implementations
{
    /// <summary>
    /// Represents an implementation for a parser that can read and write objects to string.
    /// </summary>
    public interface IObj2StrSerializer
    {
        /// <summary>
        /// Extension of the file used for this parser.
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Read the containing of the <param name="str"></param> and parse then and return the objects.
        /// </summary>
        Span<DatabaseElement<T>> Read<T>(string str) where T : struct;

        /// <summary>
        /// Write the <param name="values"></param> to string and return it.
        /// </summary>
        string Write<T>(Span<DatabaseElement<T>> values) where T : struct;
    }
}