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
        string FilePath { get; set; }

        string Directory { set; }

        /// <summary>
        /// File extension for the to be applied to <see cref="FilePath"/>
        /// </summary>
        string FileExtension { get; }

        DateTime LastReadDate { get; set; }

        DatabaseElement<T>[] LastReadData { get; set; }

        /// <summary>
        /// Read all data from the file at <see cref="FilePath"/>
        /// </summary>
        /// <returns></returns>
        DatabaseElement<T>[] ReadAllFromExistingFile();

        /// <summary>
        /// Write all <see cref="values"/> data to the file at <see cref="FilePath"/>
        /// </summary>
        void WriteAllToFile(Span<DatabaseElement<T>> values);

        void IDatabaseImplementation<T>.SetPath(string path)
        {
            FilePath = Path.Combine(path, $"{typeof(T).Name}.{FileExtension}");
            Directory = path;
        }

        int IDatabaseImplementation<T>.GetLength() => TryReadFile() ? LastReadData.Length : 0;

        Span<DatabaseElement<T>> IDatabaseImplementation<T>.GetAll() => TryReadFile() ? LastReadData.AsSpan() : Span<DatabaseElement<T>>.Empty;

        bool IDatabaseImplementation<T>.TryGet(ulong id, out DatabaseElement<T> element)
        {
            if (!TryReadFile())
            {
                element = default;
                return false;
            }

            for (int i = 0; i < LastReadData.Length; i++)
            {
                if (LastReadData[i].id == id)
                {
                    element = LastReadData[i];
                    return true;
                }
            }
            element = default;
            return false;
        }

        bool IDatabaseImplementation<T>.TryGet(int startIndex, int count, out Span<DatabaseElement<T>> elements)
        {
            if (!TryReadFile())
            {
                elements = default;
                return false;
            }

            if (LastReadData.Length <= count + startIndex)
            {
                elements = default;
                return false;
            }
            elements = LastReadData[startIndex..(startIndex + count)];
            return true;
        }

        bool IDatabaseImplementation<T>.TryGet(int index, out DatabaseElement<T> element)
        {
            if (!TryReadFile())
            {
                element = default;
                return false;
            }
            if (LastReadData.Length <= index || index < 0)
            {
                element = default;
                return false;
            }
            element = LastReadData[index];
            return true;
        }

        void IDatabaseImplementation<T>.Set(DatabaseElement<T> value)
        {
            TryReadFile();
            var data = LastReadData;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].id == value.id)
                {
                    data[i] = value;
                    WriteAllToFile(data);
                    return;
                }
            }

            // if not found, add new item
            Array.Resize(ref data, data.Length + 1);
            data[^1] = value;
            WriteAllToFile(data);
        }

        void IDatabaseImplementation<T>.Set(int startIndex, Span<DatabaseElement<T>> values)
        {
            TryReadFile();
            var data = LastReadData;

            var neededLength = startIndex + values.Length;
            if (data.Length < neededLength)
            {
                Array.Resize(ref data, neededLength);
            }
            values.CopyTo(data[startIndex..values.Length]);
            WriteAllToFile(data);
        }

        void IDatabaseImplementation<T>.Set(int index, DatabaseElement<T> value)
        {
            TryReadFile();
            var data = LastReadData;

            if (data.Length <= index)
            {
                Array.Resize(ref data, index + 1);
            }
            data[index] = value;
            WriteAllToFile(data);
        }

        void IDatabaseImplementation<T>.Set(Span<DatabaseElement<T>> values) => WriteAllToFile(values);

        void IDatabaseImplementation<T>.Delete(int startIndex, int count)
        {
            TryReadFile();
            var data = LastReadData;
            data = data.AsSpan().RemoveRange(startIndex, count).ToArray();
            WriteAllToFile(data);
        }

        void IDatabaseImplementation<T>.Delete(int index)
        {
            TryReadFile();
            var data = LastReadData;
            if (data.Length <= index)
            {
                return;
            }
            data = data.AsSpan().RemoveRange(index, 1).ToArray();
            WriteAllToFile(data);
        }

        void IDatabaseImplementation<T>.Delete(ulong id)
        {
            TryReadFile();
            var data = LastReadData;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].id == id)
                {
                    data = data.AsSpan().RemoveRange(i, 1).ToArray();
                    WriteAllToFile(data);
                    return;
                }
            }
        }

        bool TryReadFile()
        {
            var fileInfo = GetFileInfo();
            if (!fileInfo.Exists)
            {
                return false;
            }
            if (LastReadDate != fileInfo.LastWriteTimeUtc)
            {
                LastReadDate = DateTime.UtcNow;
                LastReadData = ReadAllFromExistingFile();
            }
            return true;
        }

        FileInfo GetFileInfo() => new(FilePath);

        void IDatabaseImplementation<T>.Delete() => WriteAllToFile(Span<DatabaseElement<T>>.Empty);
    }
}