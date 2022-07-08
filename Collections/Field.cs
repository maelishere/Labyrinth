using System;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;

    // any variable that needs to be synced only when it's changed
    public class Field<T> : Unit
    {
        private T m_reference;
        private readonly IEqualityComparer<T> _comparer;

        public Field() : this(EqualityComparer<T>.Default, default(T)) { }
        public Field(T value) : this(EqualityComparer<T>.Default, value) { }
        public Field(IEqualityComparer<T> comparer, T value)
        {
            m_reference = value;
            _comparer = comparer ?? EqualityComparer<T>.Default;
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
                    Change(false, Step.Set, value);
                }
            }
        }

        public void Clear()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            m_reference = default;
            Change(false, Step.Clear);
        }

        protected override void Serialize(ref Writer writer)
        {
            writer.Write(m_reference);
        }

        protected override void Deserialize(ref Reader reader)
        {
            m_reference = reader.Read<T>();
        }

        protected override Action Deserialize(Step step, ref Reader reader)
        {
            switch(step)
            {
                case Step.Clear:
                    {
                        return () => m_reference = default;
                    }

                case Step.Set:
                    {
                        T value = reader.Read<T>();
                        return () => m_reference = value;
                    }
            }
            return null;
        }

        public static implicit operator T(Field<T> var)
        {
            return var.m_reference;
        }
    }
}