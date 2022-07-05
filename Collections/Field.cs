using System;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;

    // any variable that needs to be synced only when it's changed
    public class Field<T> : Unit<T>
    {
        private T m_reference;

        public Field() : this(EqualityComparer<T>.Default, default(T)) { }
        public Field(T value) : this(EqualityComparer<T>.Default, value) { }
        public Field(IEqualityComparer<T> comparer, T value) : base(comparer)
        {
            m_reference = value;
        }

        public T value
        {
            get => m_reference;
            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Network collection can only be modified by the server");
                }

                if (!_comparer.Equals(value, m_reference))
                {
                    m_reference = value;
                    Change(false, Action.Set, value);
                }
            }
        }

        protected override void Serialize(ref Writer writer)
        {
            writer.Write(m_reference);
        }

        protected override void Deserialize(ref Reader reader)
        {
            m_reference = reader.Read<T>();
        }

        protected override void Deserialize(Action action, ref Reader reader)
        {
            switch(action)
            {
                case Action.Clear:
                    m_reference = default;
                    break;

                case Action.Set:
                    m_reference = reader.Read<T>();
                    break;
            }
        }

        public static implicit operator T(Field<T> var)
        {
            return var.m_reference;
        }
    }
}