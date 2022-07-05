using System;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;
    using Labyrinth.Background;

    // Base class for all collections types
    //      uses irrgeular channel
    //      mostly for static classes
    //      using network instance, can't use instance identity (mutiple appendices would collide)
    //      (Extensions.Combine + Identity.Unique) not sure how that would work yet
    public abstract class Unit
    {
        private uint m_counter = 0;
        private readonly Queue<Change> m_changes = new Queue<Change>();
        private readonly SortedDictionary<uint, Reader> m_pending = new SortedDictionary<uint, Reader>();

        internal System.Action destructor { get; set; }

        protected Unit()
        {

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

            m_changes.Enqueue(new Change(m_counter, action, null));

            if (additive)
                m_counter++;
        }

        protected void Change<I>(bool additive, Action action, I arg)
        {
            if (!additive)
                m_changes.Clear();

            m_changes.Enqueue(new Change(m_counter, action,
                (ref Writer writer) =>
                {
                    writer.Write(arg);
                }));

            if (additive)
                m_counter++;
        }

        protected void Change<I, T>(bool additive, Action action, I arg1, T arg2)
        {
            if (!additive)
                m_changes.Clear();

            m_changes.Enqueue(new Change(m_counter, action,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                }));

            if (additive)
                m_counter++;
        }

        // for new clients connections
        internal void Clone(ref Writer writer)
        {
            writer.Write(m_counter);
            Serialize(ref writer);
        }

        internal void Apply(ref Reader reader)
        {
            m_counter = reader.ReadUInt();
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
            if (operation.Step > m_counter)
            {
                // add to pending
                // i need to know how big the state of the operation is
            }
            else if (operation.Step == m_counter)
            {
                Replicate(operation.Action, ref reader);
                if (m_pending.Count > 0)
                {
                    while (m_pending.ContainsKey(m_counter))
                    {
                        // 
                        /*Replicate();*/
                        m_counter++;
                    }
                }
            }
        }

        private void Replicate(Action action, ref Reader reader)
        {
            Deserialize(action, ref reader);
            m_counter++;
        }

        protected abstract void Serialize(ref Writer writer);
        protected abstract void Deserialize(ref Reader reader);
        protected abstract void Deserialize(Action action, ref Reader reader);
    }
}