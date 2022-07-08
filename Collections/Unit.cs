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
        private uint m_marker = 0/*frist step of changes that will be sent*/;
        private uint m_count = 0/*number of changes since last network copy*/;
        private readonly Queue<Change> m_changes = new Queue<Change>();
        private readonly Dictionary<uint, Action> m_pending = new Dictionary<uint, Action>();

        public ulong identifier { get; internal set; }
        public bool Pending => m_pending.Count > 0;

        // for testing (will remove later)
        public bool Valid { get; private set; }

        protected Unit()
        {
            Valid = true;
        }

        // instance id for an instance (clone) of a class (for static classes 0)
        // member differentiates between each unit within an instance
        // (Static classes)
        //          for server use Network.initialized,
        //          clients use Network.connected
        //          (check if the connection is actually the server)
        public bool Create(string type, ushort instance, ushort member)
        {
            return Objects.Add(type, instance, member, this);
        }

        // (Static classes)
        //          for server use Network.terminating,
        //          clients use Network.disconnected
        //          (check if the connection is actually the server)
        public void Destroy()
        {
            Objects.Remove(identifier);
        }

        // these classes can only be edited by a server
        //      client only read from it
        public bool IsReadOnly => NetworkClient.Active;

        protected void Change(bool additive, Step action)
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

        protected void Change<I>(bool additive, Step action, I arg)
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

        protected void Change<I, T>(bool additive, Step action, I arg1, T arg2)
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
            UnityEngine.Debug.Log($"Applying Step({m_steps}) for Object{identifier}");
            Deserialize(ref reader);
            m_pending.Clear();
        }

        // for clients already connected
        internal void Copy(ref Writer writer)
        {
            m_count = 0;
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

        internal void Paste(ref Reader reader)
        {
            uint marker = reader.ReadUInt();
            int count = reader.ReadInt();

            // add to pending
            while (count > 0)
            {
                Step step = (Step)reader.Read();
                // the args would have been read (and still be somewhere memory) 
                // call the action when we get there
                Action action = Deserialize(step, ref reader);
                m_pending.Add(marker, action);
                marker++;
                count--;
            }

            Release();

            /*if it recieved a step before it's current step*/
            if (marker < m_steps)
            {
                /// then this unit is invalid
                /// this shouldn't be a posibility (for testing)
                Valid = false;
            }
        }

        private void Release()
        {
            // release the next steps that are pending
            while (m_pending.ContainsKey(m_steps))
            {
                UnityEngine.Debug.Log($"Releasing Step({m_steps}) for Object{identifier}");

                m_pending[m_steps]();
                m_pending.Remove(m_steps);
                m_steps++;
            }
        }

        protected abstract void Serialize(ref Writer writer);
        protected abstract void Deserialize(ref Reader reader);
        protected abstract Action Deserialize(Step step, ref Reader reader);
    }
}