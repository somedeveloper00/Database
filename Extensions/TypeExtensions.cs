using System.Reflection;

namespace Database.Extensions;

public static class TypeExtensions
{
    /// <summary>
    /// Gets all the fields of a type, including the fields of its fields, and so on.
    /// </summary>
    public static IEnumerable<FieldInfo> GetPrimitiveFieldInfoRecursive(this Type type)
    {
        if (type.IsPrimitive || type == typeof(string))
        {
            yield return null!;
        }

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType == typeof(string))
            {
                yield return fieldInfo;
            }
            else
            {
                foreach (var subfieldInfo in GetPrimitiveFieldInfoRecursive(fieldInfo.FieldType))
                {
                    yield return subfieldInfo;
                }
            }
        }
    }
}