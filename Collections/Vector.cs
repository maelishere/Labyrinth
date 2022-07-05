using System;
using System.Collections;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;

    // Network List Equivalent
    public class Vector<T> : Unit<T>, IList<T>, IReadOnlyList<T>
    {
        private readonly IList<T> m_reference;

        public Vector() : this(EqualityComparer<T>.Default) { }
        public Vector(IEqualityComparer<T> comparer) : this(comparer, new List<T>()) { }
        public Vector(IEqualityComparer<T> comparer, IList <T> list) : base(comparer) 
        {
            m_reference = list;
        }

        public T this[int index]
        {
            get => m_reference[index];
            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Synclists can only be modified at the server");
                }

                if (!_comparer.Equals(value, m_reference[index]))
                {
                    m_reference[index] = value;
                    Change(true, Action.Set, index, value);
                }
            }
        }

        public int Count => m_reference.Count;

        public void Add(T item)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            m_reference.Add(item);
            Change(true, Action.Add, item);
        }

        public void Clear()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            m_reference.Clear();
            Change(true, Action.Clear);
        }

        public bool Contains(T item)
        {
            return m_reference.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_reference.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_reference.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return m_reference.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            m_reference.Insert(index, item);
            Change(true, Action.Insert, index, item);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            /*T value = m_reference[index];*/
            m_reference.RemoveAt(index);
            Change(true, Action.Remove, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_reference.GetEnumerator();
        }

        protected override void Serialize(ref Writer writer)
        {
            writer.Write(Count);
            foreach(var element in m_reference)
            {
                writer.Write(element);
            }
        }

        protected override void Deserialize(ref Reader reader)
        {
            int count = reader.ReadInt();
            m_reference.Clear();
            for (int i = 0; i < count; i++)
            {
                T value = reader.Read<T>();
                m_reference.Add(value);
            }
        }

        protected override void Deserialize(Action action, ref Reader reader)
        {
            switch (action)
            {
                case Action.Remove:
                    {
                        int index = reader.ReadInt();
                        m_reference.RemoveAt(index);
                    }
                    break;
                case Action.Clear:
                    m_reference.Clear();
                    break;
                case Action.Insert:
                    {
                        int index = reader.ReadInt();
                        T value = reader.Read<T>();
                        m_reference.Insert(index, value);
                    }
                    break;
                case Action.Add:
                    {
                        T value = reader.Read<T>();
                        m_reference.Add(value);
                    }
                    break;
                case Action.Set:
                    {
                        int index = reader.ReadInt();
                        T value = reader.Read<T>();
                        m_reference[index] = value;
                    }
                    break;
            }
        }
    }
}
