using System;
using Database.Core;
using Database.Core.Implementations;
using UnityEngine;

namespace Database.Essentials.Unity
{
    /// <summary>
    /// A simple JsonUtility json parser
    /// </summary>
    public readonly struct JsonParser : IObj2StrSerializer
    {
        public readonly string FileExtension => "json";

        public readonly Span<DatabaseElement<T>> Read<T>(string str) where T : struct => JsonUtility.FromJson<DatabaseElement<T>[]>(str);

        public string Write<T>(Span<DatabaseElement<T>> values) where T : struct => JsonUtility.ToJson(values.ToArray(), false);
    }
}