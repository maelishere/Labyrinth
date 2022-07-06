using System;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;
    using Labyrinth.Background;

    // Base class for all collections types
    public abstract class Unit
    {
        private uint m_steps = 0/*total number of changes*/, m_count = 0/*number of changes since last network copy*/;
        private readonly Queue<Change> m_changes = new Queue<Change>();
        private readonly Dictionary<uint, Reader> m_pending = new Dictionary<uint, Reader>();

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
            {
                m_changes.Clear();
                m_steps -= m_count;
                m_count = 0;
            }

            m_changes.Enqueue(new Change(m_steps, action, null));

            if (additive)
            {
                m_steps++;
                m_count++;
            }
        }

        protected void Change<I>(bool additive, Action action, I arg)
        {
            if (!additive)
            {
                m_changes.Clear();
                m_steps -= m_count;
                m_count = 0;
            }

            m_changes.Enqueue(new Change(m_steps, action,
                (ref Writer writer) =>
                {
                    writer.Write(arg);
                }));

            if (additive)
            {
                m_steps++;
                m_count++;
            }
        }

        protected void Change<I, T>(bool additive, Action action, I arg1, T arg2)
        {
            if (!additive)
            {
                m_changes.Clear();
                m_steps -= m_count;
                m_count = 0;
            }

            m_changes.Enqueue(new Change(m_steps, action,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                }));

            if (additive)
            {
                m_steps++;
                m_count++;
            }
        }

        // for new clients connections
        internal void Clone(ref Writer writer)
        {
            writer.Write(m_steps);
            Serialize(ref writer);
        }

        internal void Apply(ref Reader reader)
        {
            m_steps = reader.ReadUInt();
            Deserialize(ref reader);
        }

        // for clients already connected
        internal void Copy(ref Writer writer)
        {
            m_count = 0;
            if (m_changes.Count > 0)
            {
                writer.Write(m_changes.Count);
                do
                {
                    Change state = m_changes.Dequeue();
                    writer.Write(state.Operation);
                    state.Callback?.Invoke(ref writer);
                } while (m_changes.Count > 0);
            }
        }

        internal void Paste(ref Reader reader)
        {
            int count = reader.ReadInt();
            while(count > 0)
            {
                Operation operation = reader.ReadOperation();
                if (operation.Step > m_steps)
                {
                    // add to pending
                    // i need to know how big the state of the operation is
                }
                else if (operation.Step == m_steps)
                {
                    Replicate(operation.Action, ref reader);
                    if (m_pending.Count > 0)
                    {
                        while (m_pending.ContainsKey(m_steps))
                        {
                            // 
                            /*Replicate();*/
                            m_steps++;
                        }
                    }
                }
                count--;
            }
        }

        private void Replicate(Action action, ref Reader reader)
        {
            Deserialize(action, ref reader);
            m_steps++;
        }

        protected abstract void Serialize(ref Writer writer);
        protected abstract void Deserialize(ref Reader reader);
        protected abstract void Deserialize(Action action, ref Reader reader);
    }
}