using System;
using System.Collections;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;

    // Network Hash Set Equivalent
    public class Series<T> : Unit, ISet<T>
    {
        private readonly ISet<T> m_reference;

        public Series() : this(EqualityComparer<T>.Default) { }
        public Series(IEqualityComparer<T> comparer) : this(new HashSet<T>(comparer ?? EqualityComparer<T>.Default)) { }
        protected Series(ISet<T> reference)
        {
            m_reference = reference;
        }

        public int Count => m_reference.Count;

        public bool Add(T item)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            if (m_reference.Add(item))
            {
                Change(true, Action.Add, item);
                return true;
            }

            return false;
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

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == this)
            {
                Clear();
                return;
            }

            foreach (T element in other)
            {
                Remove(element);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_reference.GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            List<T> list = new List<T>(m_reference);
            ISet<T> set = (other as ISet<T>) ?? new HashSet<T>(other);

            foreach (T element in list)
            {
                if (!set.Contains(element))
                {
                    Remove(element);
                }
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return m_reference.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return m_reference.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return m_reference.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return m_reference.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return m_reference.Overlaps(other);
        }

        public bool Remove(T item)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            if (m_reference.Remove(item))
            {
                Change(true, Action.Remove, item);
                return true;
            }
            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return m_reference.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == this)
            {
                Clear();
            }
            else
            {
                foreach (T element in other)
                {
                    if (!Remove(element))
                    {
                        Add(element);
                    }
                }
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other != this)
            {
                foreach (T element in other)
                {
                    Add(element);
                }
            }
        }

        void ICollection<T>.Add(T item)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_reference.GetEnumerator();
        }

        protected override void Serialize(ref Writer writer)
        {
            writer.Write(Count);
            foreach (var element in m_reference)
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
                        T value = reader.Read<T>();
                        m_reference.Remove(value);
                    }
                    break;
                case Action.Clear:
                    m_reference.Clear();
                    break;
                case Action.Add:
                    {
                        T value = reader.Read<T>();
                        m_reference.Add(value);
                    }
                    break;
            }
        }
    }
}
