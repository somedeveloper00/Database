using System;

namespace Database.Core
{
    /// <summary>
    /// Database for a type that works in the current thread synchronously
    /// </summary>
    public readonly struct TypeDatabase<T> where T : struct
    {
        /// <summary>
        /// Path for the database. The meaning of the path depends on the <see cref="implementation"/>
        /// </summary>
        public readonly string path;

        /// <summary>
        /// The implementation of the database.
        /// </summary>
        public readonly IDatabaseImplementation<T> implementation;

        public TypeDatabase(string path, IDatabaseImplementation<T> implementation)
        {
            this.path = path;
            this.implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
            this.implementation.SetPath(this.path);
        }

        /// <summary>
        /// Gets the length of items in the database
        /// </summary>
        public int GetLength() => implementation.GetLength();

        /// <summary>
        /// Gets all elements
        /// </summary>
        public Span<DatabaseElement<T>> GetAll() => implementation.GetAll();

        /// <summary>
        /// Gets the item with the specified ID
        /// </summary>
        /// <param name="id">The ID of the element</param>
        /// <param name="element">element found at id</param>
        public bool TryGetById(ulong id, out DatabaseElement<T> element) => implementation.TryGet(id, out element);

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">Index of the element</param>
        /// <param name="element">element found at index</param>
        public bool TryGetAt(int index, out DatabaseElement<T> element) => implementation.TryGet(index, out element);

        /// <summary>
        /// Gets the items at the specified range
        /// </summary>
        /// <param name="startIndex">Starting, inclusive, index of items.</param>
        /// <param name="count">Number of items</param>
        /// <param name="elements">elements found at range</param>
        public bool TryGetRange(int startIndex, int count, out Span<DatabaseElement<T>> elements) =>
            implementation.TryGet(startIndex, count, out elements);

        /// <summary>
        /// Sets all elements of this database, <b><i>replacing any previous data</i></b>!
        /// </summary>
        /// <param name="values">Elements of this database</param>
        public void SetAll(Span<DatabaseElement<T>> values) => implementation.Set(values);

        /// <summary>
        /// Sets element by ID.
        /// </summary>
        /// <param name="value">Element with ID to be set</param>
        public void SetById(DatabaseElement<T> value) => implementation.Set(value);

        /// <summary>
        /// Sets the item at the specified index
        /// </summary>
        /// <param name="index">The index to set the element to</param>
        /// <param name="value">The element value</param>
        public void SetAt(int index, DatabaseElement<T> value) => implementation.Set(index, value);

        /// <summary>
        /// Sets the items at the specified range
        /// </summary>
        /// <param name="startIndex">Starting, inclusive, index of items.</param>
        /// <param name="values">The elements values.</param>
        public void SetRange(int startIndex, Span<DatabaseElement<T>> values) => implementation.Set(startIndex, values);

        /// <summary>
        /// Deletes an item from database at the specified index.
        /// </summary>
        /// <param name="index">The index to delete the element from.</param>
        public void Delete(int index) => implementation.Delete(index);

        /// <summary>
        /// Deletes an item from database by ID.
        /// </summary>
        /// <param name="id">The ID of the element to delete</param>
        public void DeleteById(long id) => implementation.Delete((ulong)id);

        /// <summary>
        /// Deletes items from the specified range.
        /// </summary>
        /// <param name="startIndex">Starting, inclusive, index of items</param>
        /// <param name="count">Number of items</param>
        public void Delete(int startIndex, int count) => implementation.Delete(startIndex, count);
    }
}