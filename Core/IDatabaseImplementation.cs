using System;

namespace Database.Core
{
    /// <summary>
    /// Implementation of a database. Does not necessarily mean disk-based database, it could be network-based or other methods.
    /// </summary>
    public interface IDatabaseImplementation<T> where T : struct
    {
        /// <summary>
        /// Defines the path to use for this database implementation.
        /// </summary>
        public void SetPath(string path);

        /// <summary>
        /// Gets the length of the items stored in this database.
        /// </summary>
        public int GetLength();

        /// <summary>
        /// Get element by ID
        /// </summary>
        /// <param name="id">ID of the element</param>
        public DatabaseElement<T> Get(ulong id);

        /// <summary>
        /// Get elements at range
        /// </summary>
        /// <param name="startIndex">Starting, inclusive, index of elements.</param>
        /// <param name="count">Number of elements.</param>
        public Span<DatabaseElement<T>> Get(int startIndex, int count);

        /// <summary>
        /// Gets element by ID
        /// </summary>
        /// <param name="index">Index to get element at</param>
        public DatabaseElement<T> Get(int index);

        /// <summary>
        /// Gets all elements
        /// </summary>
        public Span<DatabaseElement<T>> GetAll();

        /// <summary>
        /// Sets elements at range
        /// </summary>
        /// <param name="startIndex">Starting, inclusive, index of elements.</param>
        /// <param name="values">Number of elements.</param>
        public void Set(int startIndex, Span<DatabaseElement<T>> values);

        /// <summary>
        /// Set element at index
        /// </summary>
        /// <param name="index">Index to set the element at</param>
        /// <param name="value">Element value</param>
        public void Set(int index, DatabaseElement<T> value);

        /// <summary>
        /// Sets element by ID. If not already exists, creates new.
        /// </summary>
        /// <param name="value">Value to store</param>
        public void Set(DatabaseElement<T> value);

        /// <summary>
        /// Sets all elements of this database
        /// </summary>
        /// <param name="values">Elements of this database</param>
        public void Set(Span<DatabaseElement<T>> values);

        /// <summary>
        /// Deletes elements at range.
        /// </summary>
        /// <param name="startIndex">Starting, inclusive, index of elements.</param>
        /// <param name="count">Number of elements.</param>
        public void Delete(int startIndex, int count);

        /// <summary>
        /// Deletes an element at index
        /// </summary>
        /// <param name="index">Index to delete the element at</param>
        public void Delete(int index);

        /// <summary>
        /// Deletes element by ID
        /// </summary>
        /// <param name="id">ID of the element to delete</param>
        public void Delete(ulong id);

        /// <summary>
        /// Deletes all elements in this database.
        /// </summary>
        public void Delete();
    }

    /// <summary>
    /// Represents when an element is not found by its ID.
    /// </summary>
    public sealed class IdNotFoundException : Exception
    {
        public IdNotFoundException(ulong id) : base("ID: " + id) { }
    }

    /// <summary>
    /// Represents when an element is not found by its index.
    /// </summary>
    public sealed class IndexNotFoundException : Exception
    {
        public IndexNotFoundException(int index) : base("Index: " + index) { }
    }
}