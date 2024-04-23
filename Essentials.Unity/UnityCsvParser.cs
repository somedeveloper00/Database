using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Database.Core;
using Database.Core.Implementations;
using Database.Essentials.Unity.Extensions;
using UnityEngine;

namespace Database.Essentials.Unity
{
    /// <summary>
    /// A simple custom CSV parser
    /// </summary>
    public struct UnityCsvParser : IObj2StrSerializer
    {
        private static class TypeHelper<T>
        {
            public static readonly FieldInfo[] FieldInfos =
                typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default);
            public static readonly string[] FieldNames = FieldInfos.Select(f => f.Name).ToArray();
        }

        public string FileExtension => "csv";

        public Span<DatabaseElement<T>> Read<T>(string str) where T : struct
        {
            var charArray = str.ToCharArray();
            var rows = new ArraySegment<char>(charArray).SplitToSegments('\r', '\n');
            var table = new List<ArraySegment<char>>[rows.Length];
            var results = new DatabaseElement<T>[rows.Length - 1];

            // resolve table
            for (int i = 0; i < rows.Length; i++)
            {
                table[i] = rows[i].SplitToSegmentsNonEscapeNonQuote(TypeHelper<T>.FieldInfos.Length, ',');
            }

            if (table.Length > 0)
            {
                // resolve values
                for (int i = 0; i < table[0].Count; i++)
                {
                    FieldInfo fieldInfo = null;
                    bool isId = false;

                    // find field info
                    if (table[0][i].AsSpan().SequenceEqual("id".AsSpan()))
                    {
                        isId = true;
                    }
                    else
                    {
                        for (int j = 0; j < TypeHelper<T>.FieldNames.Length; j++)
                        {
                            if (TypeHelper<T>.FieldNames[j].AsSpan().SequenceEqual(table[0][i].AsSpan()))
                            {
                                fieldInfo = TypeHelper<T>.FieldInfos[j];
                                break;
                            }
                        }
                    }

                    if (fieldInfo == null && !isId)
                    {
                        continue;
                    }

                    // assign values
                    if (isId)
                    {
                        for (int j = 1; j < table.Length; j++)
                        {
                            var value = new string(table[j][i]);
                            results[j - 1].id = Convert.ToUInt64(value);
                        }
                    }
                    else
                    {
                        for (int j = 1; j < table.Length; j++)
                        {
                            var value = new string(table[j][i]);
                            var changeType = ConvertFromString(value, fieldInfo.FieldType);
                            var typedReference = __makeref(results[j - 1].value);
                            fieldInfo.SetValueDirect(typedReference, changeType);
                        }
                    }
                }
            }

            return results;
        }

        public string Write<T>(Span<DatabaseElement<T>> values) where T : struct
        {
            var sb = new StringBuilder(128 * values.Length * TypeHelper<T>.FieldInfos.Length);

            // write headers
            sb.Append(nameof(DatabaseElement<T>.id)).Append(',');
            for (int i = 0; i < TypeHelper<T>.FieldNames.Length; i++)
            {
                sb.Append(TypeHelper<T>.FieldNames[i]);
                if (i != TypeHelper<T>.FieldNames.Length - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append('\n');

            // write values
            for (int i = 0; i < values.Length; i++)
            {
                sb.Append(values[i].id).Append(',');
                for (int j = 0; j < TypeHelper<T>.FieldInfos.Length; j++)
                {
                    var value = TypeHelper<T>.FieldInfos[j].GetValueDirect(__makeref(values[i].value));
                    var serialized = ConvertToString(value);
                    sb.Append(serialized);
                    if (j != TypeHelper<T>.FieldInfos.Length - 1)
                    {
                        sb.Append(',');
                    }
                }
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        private static string ConvertToString(object obj)
        {
            if (obj is null)
                return " ";
            if (obj.GetType().IsPrimitive)
            {
                return Convert.ToString(obj);
            }
            else if (obj is string objstr)
            {
                return "\"" + objstr.Replace("\"", "\\\"") + "\"";
            }
            var str = JsonUtility.ToJson(obj);
            str = str.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"\"{str}\"";
        }

        private static object ConvertFromString(string str, Type type)
        {
            if (string.IsNullOrEmpty(str))
                return default;
            if (type.IsPrimitive)
            {
                return Convert.ChangeType(str, type);
            }
            else if (type == typeof(string))
            {
                return str[1..^1].Replace("\\\"", "\"");
            }
            str = str.Replace("\\\\", "\\").Replace("\\\"", "\"");
            str = str[
                (str[0] == '\"' ? 1 : 0)
                ..
                (str[^1] == '\"' ? ^1 : str.Length)];
            return JsonUtility.FromJson(str, type);
        }
    }
}