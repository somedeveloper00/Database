using System;
using System.Collections.Generic;
using UnityEngine;

namespace Database.Essentials.Unity
{
    /// <summary>
    /// A Unity-serializable <see cref="List{T}"/> that can be used in a <see cref="UnityCsvParser"/>.
    /// </summary>
    [Serializable]
    public sealed class CsvList<T> : List<T>, ISerializationCallbackReceiver
    {
        [SerializeField] private T[] elements;

        public void OnAfterDeserialize()
        {
            Clear();
            AddRange(elements);
        }

        public void OnBeforeSerialize() => elements = ToArray();
    }
}