using System;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;
    using Labyrinth.Background;

    // Base class for all collections types
    //      these types aren't attached to any object
    //      uses irrgeular channel
    public abstract class Unit<T>
    {
        protected readonly IEqualityComparer<T> _comparer;

        private uint m_local = 0, m_remote;
        private readonly Queue<Change> m_changes = new Queue<Change>();
        private readonly SortedDictionary<uint, Reader> m_pending = new SortedDictionary<uint, Reader>();

        internal System.Action destructor { get; set; }

        protected Unit(IEqualityComparer<T> comparer)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        ~Unit()
        {
            destructor?.Invoke();
        }

        // these classes can only be edited by a server
        //      client only read from it
        public bool IsReadOnly => NetworkClient.Active;

        protected void Change(bool additive, Action action)
        {
            if (!additive)
                m_changes.Clear();

            m_changes.Enqueue(new Change(m_local++, action, null));
        }

        protected void Change(bool additive, Action action, T item)
        {
            if (!additive)
                m_changes.Clear();

            m_changes.Enqueue(new Change(m_local++, action,
                (ref Writer writer) =>
                {
                    writer.Write(item);
                }));
        }

        protected void Change<I>(bool additive, Action action, I index)
        {
            if (!additive)
                m_changes.Clear();

            m_changes.Enqueue(new Change(m_local++, action,
                (ref Writer writer) =>
                {
                    writer.Write(index);
                }));
        }

        protected void Change<I>(bool additive, Action action, I index, T item)
        {
            if (!additive)
                m_changes.Clear();

            m_changes.Enqueue(new Change(m_local++, action,
                (ref Writer writer) =>
                {
                    writer.Write(index);
                    writer.Write(item);
                }));
        }

        // for new clients connections
        internal void Clone(ref Writer writer)
        {
            writer.Write(m_local);
            Serialize(ref writer);
        }

        internal void Apply(ref Reader reader)
        {
            m_remote = reader.ReadUInt();
            Deserialize(ref reader);
        }

        // for clients already connected
        internal void Copy(ref Writer writer)
        {
            while (m_changes.Count > 0)
            {
                Change state = m_changes.Dequeue();
                writer.Write(state.Operation);
                state.Callback?.Invoke(ref writer);
            }
        }

        internal void Paste(ref Reader reader)
        {
            Operation operation = reader.ReadOperation();
            if (operation.Step > m_remote)
            {
                // add to pending
            }
            else if (operation.Step == m_remote)
            {
                Deserialize(operation.Action, ref reader);
                m_remote++;

                if (m_pending.Count > 0)
                {
                    while(m_pending.ContainsKey(m_remote))
                    {

                        m_remote++;
                    }
                }
            }
        }

        protected abstract void Serialize(ref Writer writer);
        protected abstract void Deserialize(ref Reader reader);
        protected abstract void Deserialize(Action action, ref Reader reader);
    }
}