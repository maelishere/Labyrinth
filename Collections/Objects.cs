using System;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;
    using Labyrinth.Background;

    // Objects that need to be synced over the netork
    //      uses irrgeular channel
    //      only use for class members (static or non-static instances)
    //      doesn't require using network instance
    public static class Objects
    {
        public const byte Find = 9;
        public const byte Link = 10;
        public const byte Modify = 11;

        public struct Callbacks
        {
            public Callbacks(Write clone, Read apply, Write copy, Read paste)
            {
                Clone = clone;
                Apply = apply;
                Copy = copy;
                Paste = paste;
            }

            public Write Clone { get; }
            public Read Apply { get; }
            public Write Copy { get; }
            public Read Paste { get; }
        }

        private static readonly Dictionary<ulong, Callbacks> m_callbacks = new Dictionary<ulong, Callbacks>();
        private static readonly Dictionary<int, HashSet<ulong>> m_queries = new Dictionary<int, HashSet<ulong>>(); /*the objects each client is looking for*/
        private static readonly Dictionary<ulong, HashSet<int>> m_listeners = new Dictionary<ulong, HashSet<int>>(); /*the clients that have created each object*/

        // only call this when the network is running
        // index id for an instance (clone) of a class (for static classes 0)
        // offset differentiates between each unit within an instance
        public static bool Register<T>(ushort index, ushort offset, Unit unit) where T : class
        {
            ulong idenitifier = Generate(typeof(T).FullName.Hash(), index, offset);
            if (!m_callbacks.ContainsKey(idenitifier))
            {
                m_listeners.Add(idenitifier, new HashSet<int>());
                m_callbacks.Add(idenitifier, new Callbacks(unit.Clone, unit.Apply, unit.Copy, unit.Paste));
                unit.destructor = () =>
                {
                    m_listeners.Remove(idenitifier);
                    m_callbacks.Remove(idenitifier);
                };
                if (NetworkClient.Active)
                {
                    // send find to server
                }
                return true;
            }
            return false;
        }

        internal static void Connected(int connection)
        {
            m_queries.Add(connection, new HashSet<ulong>());
        }

        internal static void Disconnected(int connection)
        {
            m_queries.Remove(connection);
        }

        internal static void Update()
        {
            if (NetworkServer.Active)
            {
                HashSet<ulong> found = new HashSet<ulong>();
                foreach(var query in m_queries)
                {
                    foreach (var identifier in query.Value)
                    {
                        if (m_callbacks.ContainsKey(identifier))
                        {
                            found.Add(identifier);
                            m_listeners[identifier].Add(query.Key);
                            /*callback.Value.Clone*/
                            // send duplicate to client (Link)
                        }
                    }

                    foreach (var identifier in found)
                    {
                        query.Value.Remove(identifier);
                    }
                }

                foreach (var callback in m_callbacks)
                {
                    /*callback.Value.Copy*/
                    foreach (var connection in m_listeners[callback.Key])
                    {
                        // send changes to clients (Modifiy)
                    }
                }
            }
        }

        // from client to server
        internal static void OnNetworkFind(int socket, int connection, uint timestamp, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        // from server to clients
        internal static void OnNetworkLink(int socket, int connection, uint timestamp, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        // from server to clients
        internal static void OnNetworkModify(int socket, int connection, uint timestamp, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static ulong Generate(uint instance, ushort index, ushort offset)
        {
            return Combine(instance, Combine(index, offset));
        }

        /// inserts a into the frist 16 bits of a uint and b into the last 16
        private static uint Combine(ushort a, ushort b)
        {
            uint value = a;
            value <<= 16;
            value |= b;
            return value;
        }

        /// inserts a into the frist 32 bits of a ulong and b into the last 32
        private static ulong Combine(uint a, uint b)
        {
            ulong value = a;
            value <<= 32;
            value |= b;
            return value;
        }

        static Objects()
        {
            NetworkLoop.LateUpdate += Update;
        }
    }
}