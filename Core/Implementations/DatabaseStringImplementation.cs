using System;
using System.IO;

namespace Database.Core.Implementations
{
    /// <summary>
    /// A database implementation that saves database objects as text in a file.
    /// </summary>
    public struct DatabaseStringImplementation<T1, T2> : IFileBasedDatabaseImplementation<T1>
        where T1 : struct
        where T2 : struct, IObj2StrSerializer
    {
        public string FilePath { get; set; }
        public string Directory { get; set; }
        public DateTime LastReadDate { get; set; }
        public DatabaseElement<T1>[] LastReadData { get; set; }

        public readonly string FileExtension => new T2().FileExtension;

        public readonly DatabaseElement<T1>[] ReadAllFromExistingFisle()
        {
            var text = File.ReadAllText(FilePath);
            return new T2().Read<T1>(text);
        }

        public readonly void WriteAllToFile(Span<DatabaseElement<T1>> values)
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }
            File.WriteAllText(FilePath, new T2().Write(values));
        }
    }
}