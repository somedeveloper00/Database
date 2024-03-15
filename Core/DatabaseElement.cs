using System;

namespace Database.Core
{
    /// <summary>
    /// Data type used for storing inside database
    /// </summary>
    [Serializable]
    public struct DatabaseElement<T>
    {
        public long id;
        public T value;

        public DatabaseElement(long id, T value)
        {
            this.id = id;
            this.value = value;
        }
    }
}