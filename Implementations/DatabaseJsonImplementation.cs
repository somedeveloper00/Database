using System;
using System.IO;
using Database.Core;
using Newtonsoft.Json;

namespace Database.Implementations
{
    /// <summary>
    /// A database implementation that uses JsonConvert to serialize and deserialize data to and from a file.
    /// </summary>
    public struct DatabaseJsonImplementation<T> : IFileBasedDatabaseImplementation<T> where T : struct
    {
        public string filePath { get; set; }
        public string directory { get; set; }

        public string FileExtension => "json";

        public Span<DatabaseElement<T>> ReadAllFromFile()
        {
            if (!File.Exists(filePath))
            {
                return Span<DatabaseElement<T>>.Empty;
            }
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<DatabaseElement<T>[]>(json);
        }

        public void WriteAllToFile(Span<DatabaseElement<T>> values)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(filePath, JsonConvert.SerializeObject(values.ToArray()));
        }
    }
}