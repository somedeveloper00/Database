using System;

namespace Database.Core
{
    /// <summary>
    /// Data type used for storing inside database
    /// </summary>
    [Serializable]
    public struct DatabaseElement<T>
    {
        public ulong id;
        public T value;

        public DatabaseElement(ulong id, T value)
        {
            this.id = id;
            this.value = value;
        }
    }
}