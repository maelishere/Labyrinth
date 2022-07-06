using System;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;
    using Labyrinth.Background;

    // Base class for all collections types
    public abstract class Unit
    {
        private uint m_steps = 0/*total number of changes*/;
        private uint m_marker = 0/*frist step of changes that was sent*/;
        private uint m_count = 0/*number of changes since last network copy*/;
        private readonly Queue<Change> m_changes = new Queue<Change>();
        private readonly Dictionary<uint, Reader> m_pending = new Dictionary<uint, Reader>();

        internal System.Action destructor { get; set; }
        internal bool Pending => m_pending.Count > 0;

        // for testing (will remove later)
        public bool Valid { get; private set; }

        protected Unit()
        {
            Valid = true;
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

            m_changes.Enqueue(new Change(action, 
                (ref Writer writer) =>
                {
                    writer.Write(0);
                }));

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

            m_changes.Enqueue(new Change(action,
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

            m_changes.Enqueue(new Change(action,
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
                writer.Write(m_marker);
                writer.Write(m_changes.Count);
                do
                {
                    Change state = m_changes.Dequeue();
                    writer.Write((byte)state.Operation);
                    state.Callback?.Invoke(ref writer);
                } while (m_changes.Count > 0);
                m_marker = m_steps;
            }
        }

        internal void Paste(ref Reader reader)
        {
            uint marker = reader.ReadUInt();

            // i need to know the size of the state
            //      (Network Stream batches could pack different types of messages)
            if (marker > m_steps)
            {
                // add to pending

            }
            else if (marker == m_steps)
            {
                /*Replicate(ref reader);*/

                if (m_pending.Count > 0)
                {
                    while (m_pending.ContainsKey(m_steps))
                    {
                        // 
                        /*Replicate();*/
                    }
                }
            }
            else /*if it recieved a step before it's current step*/
            {
                /// then this unit is invalid
                /// this shouldn't be a posibility (for testing)
                Valid = false;
            }
        }

        private void Replicate(ref Reader reader)
        {
            int count = reader.ReadInt();
            while (count > 0)
            {
                Action operation = (Action)reader.Read();
                Deserialize(operation, ref reader);
                m_steps++;
                count--;
            }
        }

        protected abstract void Serialize(ref Writer writer);
        protected abstract void Deserialize(ref Reader reader);
        protected abstract void Deserialize(Action action, ref Reader reader);
    }
}