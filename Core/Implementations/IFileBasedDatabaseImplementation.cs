using System;
using System.IO;
using Database.Core.Extensions;

namespace Database.Core.Implementations
{
    /// <summary>
    /// A database implementation category that saves to a local file
    /// </summary>
    public interface IFileBasedDatabaseImplementation<T> : IDatabaseImplementation<T> where T : struct
    {
        string FilePath { set; }

        string Directory { set; }

        /// <summary>
        /// File extension for the to be applied to <see cref="FilePath"/>
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Read all data from the file at <see cref="FilePath"/>
        /// </summary>
        /// <returns></returns>
        Span<DatabaseElement<T>> ReadAllFromFile();

        /// <summary>
        /// Write all <see cref="values"/> data to the file at <see cref="FilePath"/>
        /// </summary>
        void WriteAllToFile(Span<DatabaseElement<T>> values);

        void IDatabaseImplementation<T>.SetPath(string path)
        {
            FilePath = Path.Combine(path, $"{typeof(T).Name}.{FileExtension}");
            Directory = path;
        }

        int IDatabaseImplementation<T>.GetLength() => ReadAllFromFile().Length;

        Span<DatabaseElement<T>> IDatabaseImplementation<T>.GetAll() => ReadAllFromFile();

        bool IDatabaseImplementation<T>.TryGet(ulong id, out DatabaseElement<T> element)
        {
            var db = ReadAllFromFile();
            for (int i = 0; i < db.Length; i++)
            {
                if (db[i].id == id)
                {
                    element = db[i];
                    return true;
                }
            }
            element = default;
            return false;
        }

        bool IDatabaseImplementation<T>.TryGet(int startIndex, int count, out Span<DatabaseElement<T>> elements)
        {
            elements = ReadAllFromFile();
            if (elements.Length <= count + startIndex)
            {
                return false;
            }
            elements = elements.Slice(startIndex, count);
            return true;
        }

        bool IDatabaseImplementation<T>.TryGet(int index, out DatabaseElement<T> element)
        {
            var elements = ReadAllFromFile();
            if (elements.Length <= index)
            {
                element = default;
                return false;
            }
            element = elements[index];
            return true;
        }

        void IDatabaseImplementation<T>.Set(DatabaseElement<T> value)
        {
            var db = ReadAllFromFile();
            for (int i = 0; i < db.Length; i++)
            {
                if (db[i].id == value.id)
                {
                    db[i] = value;
                    WriteAllToFile(db);
                    return;
                }
            }

            // if not found, add new item
            db = db.Extend(db.Length + 1);
            db[^1] = value;
            WriteAllToFile(db);
        }

        void IDatabaseImplementation<T>.Set(int startIndex, Span<DatabaseElement<T>> values)
        {
            var neededLength = startIndex + values.Length;
            var db = ReadAllFromFile();
            if (db.Length < neededLength)
            {
                db = db.Extend(neededLength);
            }
            values.CopyTo(db.Slice(startIndex, values.Length));
            WriteAllToFile(db);
        }

        void IDatabaseImplementation<T>.Set(int index, DatabaseElement<T> value)
        {
            var db = ReadAllFromFile();
            if (db.Length <= index)
            {
                db = db.Extend(index + 1);
            }
            db[index] = value;
            WriteAllToFile(db);
        }

        void IDatabaseImplementation<T>.Set(Span<DatabaseElement<T>> values) => WriteAllToFile(values);

        void IDatabaseImplementation<T>.Delete(int startIndex, int count)
        {
            var db = ReadAllFromFile();
            db = db.RemoveRange(startIndex, count);
            WriteAllToFile(db);
        }

        void IDatabaseImplementation<T>.Delete(int index)
        {
            var db = ReadAllFromFile();
            if (db.Length <= index)
            {
                return;
            }
            db = db.RemoveRange(index, 1);
            WriteAllToFile(db);
        }

        void IDatabaseImplementation<T>.Delete(ulong id)
        {
            var db = ReadAllFromFile();
            for (int i = 0; i < db.Length; i++)
            {
                if (db[i].id == id)
                {
                    db = db.RemoveRange(i, 1);
                    WriteAllToFile(db);
                    return;
                }
            }
        }

        void IDatabaseImplementation<T>.Delete() => WriteAllToFile(Span<DatabaseElement<T>>.Empty);
    }
}