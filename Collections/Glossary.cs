using System;
using System.Collections;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;

    // Network Dictionary Set Equivalent
    public class Glossary<TKey, TValue> : Unit<TValue>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        /*private readonly IEqualityComparer<TKey> m_comparer;*/
        private readonly IDictionary<TKey, TValue> m_reference;

        public Glossary() : this(new Dictionary<TKey, TValue>()) { }
        public Glossary(IEqualityComparer<TKey> comparer) : this(new Dictionary<TKey, TValue>(comparer)) { }
        public Glossary(IDictionary<TKey, TValue> dictionary) : base(EqualityComparer<TValue>.Default)
        {
            m_reference = dictionary;
        }

        public TValue this[TKey key]
        {
            get => m_reference[key];
            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Network collection can only be modified by the server");
                }

                if (ContainsKey(key))
                {
                    m_reference[key] = value;
                    Change(true, Action.Set, key, value);
                }
                else
                {
                    m_reference[key] = value;
                    Change(true, Action.Add, key, value);
                }
            }
        }

        public int Count => m_reference.Count;

        public ICollection<TKey> Keys => m_reference.Keys;

        public ICollection<TValue> Values => m_reference.Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => m_reference.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => m_reference.Values;

        public void Add(TKey key, TValue value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            m_reference.Add(key, value);
            Change(true, Action.Add, key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            m_reference.Clear();
            Change<TKey>(true, Action.Clear, default, default);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return m_reference.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return m_reference.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            m_reference.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return m_reference.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            if (m_reference.Remove(key))
            {
                Change(true, Action.Remove, key);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Network collection can only be modified by the server");
            }

            if (m_reference.Remove(item.Key))
            {
                Change(true, Action.Remove, item.Key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_reference.TryGetValue(key, out value);
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
                writer.Write(element.Key);
                writer.Write(element.Value);
            }
        }

        protected override void Deserialize(ref Reader reader)
        {
            int count = reader.ReadInt();
            m_reference.Clear();
            for (int i = 0; i < count; i++)
            {
                TKey index = reader.Read<TKey>();
                TValue value = reader.Read<TValue>();
                m_reference.Add(index, value);
            }
        }

        protected override void Deserialize(Action action, ref Reader reader)
        {
            switch (action)
            {
                case Action.Remove:
                    {
                        TKey index = reader.Read<TKey>();
                        m_reference.Remove(index);
                    }
                    break;
                case Action.Clear:
                    m_reference.Clear();
                    break;
                case Action.Add:
                    {
                        TKey index = reader.Read<TKey>();
                        TValue value = reader.Read<TValue>();
                        m_reference.Add(index, value);
                    }
                    break;
                case Action.Set:
                    {
                        TKey index = reader.Read<TKey>();
                        TValue value = reader.Read<TValue>();
                        m_reference[index] = value;
                    }
                    break;
            }
        }
    }
}