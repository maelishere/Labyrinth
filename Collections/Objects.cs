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
        public const byte Ignore = 12;

        public struct Callbacks
        {
            public Callbacks(Write clone, Read apply, Func<bool> pending, Write copy, Read paste)
            {
                Clone = clone;
                Apply = apply;
                Pending = pending;
                Copy = copy;
                Paste = paste;
            }

            public Write Clone { get; }
            public Read Apply { get; }
            public Func<bool> Pending { get; }
            public Write Copy { get; }
            public Read Paste { get; }
        }

        private static readonly Dictionary<ulong, Callbacks> m_callbacks = new Dictionary<ulong, Callbacks>();
        private static readonly Dictionary<int, HashSet<ulong>> m_queries = new Dictionary<int, HashSet<ulong>>(); /*the objects each client is looking for*/
        private static readonly Dictionary<ulong, HashSet<int>> m_listeners = new Dictionary<ulong, HashSet<int>>(); /*the clients that have created each object*/

        private static void OnNetworkShutdown(int sokcet)
        {
            m_callbacks.Clear();
            m_queries.Clear();
            m_queries.Clear();
        }

        internal static bool Add<T>(ushort index, ushort offset, Unit unit) where T : class
        {
            ulong identifier = Generate(typeof(T).FullName.Hash(), index, offset);
            if (!m_callbacks.ContainsKey(identifier))
            {
                unit.identifier = identifier;
                m_listeners.Add(identifier, new HashSet<int>());
                m_callbacks.Add(identifier, new Callbacks(unit.Clone, unit.Apply, () => unit.Pending, unit.Copy, unit.Paste));
                if (NetworkClient.Active)
                {
                    // send find to server
                    Network.Forward(Channels.Irregular, Find, (ref Writer writer) =>
                    {
                        writer.Write(identifier);
                    });
                }
                return true;
            }
            return false;
        }

        internal static void Remove(ulong identifier)
        {
            m_listeners.Remove(identifier);
            m_callbacks.Remove(identifier);
            if (NetworkClient.Active)
            {
                // send find to server
                Network.Forward(Channels.Irregular, Ignore, (ref Writer writer) =>
                {
                    writer.Write(identifier);
                });
            }
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
                foreach(var query in m_queries)
                {
                    HashSet<ulong> found = new HashSet<ulong>();

                    foreach (var identifier in query.Value)
                    {
                        if (m_callbacks.ContainsKey(identifier))
                        {
                            found.Add(identifier);
                            m_listeners[identifier].Add(query.Key);
                            // send clone to client (Link)
                            Network.Forward(Channels.Irregular, Link, (ref Writer writer) =>
                            {
                                writer.Write(identifier);
                                m_callbacks[identifier].Clone(ref writer);
                            });
                        }
                    }

                    foreach (var identifier in found)
                    {
                        query.Value.Remove(identifier);
                    }
                }

                foreach (var callback in m_callbacks)
                {
                    if (callback.Value.Pending())
                    {
                        foreach (var connection in m_listeners[callback.Key])
                        {
                            /// copy can only be called once 
                            ///     to capture all changes
                            Writer buffer = new Writer();
                            callback.Value.Copy(ref buffer);

                            // send changes to clients (Modifiy)
                            Network.Forward(Channels.Irregular, Modify, (ref Writer writer) =>
                            {
                                writer.Write(callback.Key);
                                writer.Write(buffer.ToSegment());
                            });
                        }
                    }
                }
            }
        }

        // from client to server
        internal static void OnNetworkFind(int socket, int connection, uint timestamp, ref Reader reader)
        {
            ulong identifier = reader.ReadULong();
            m_queries[connection].Add(identifier);
        }

        // from server to clients
        internal static void OnNetworkLink(int socket, int connection, uint timestamp, ref Reader reader)
        {
            ulong identifier = reader.ReadULong();
            m_callbacks[identifier].Apply(ref reader);
            // don't check if it contains it would mess with remaining data in the buffer
        }

        // from server to clients
        internal static void OnNetworkModify(int socket, int connection, uint timestamp, ref Reader reader)
        {
            ulong identifier = reader.ReadULong();
            m_callbacks[identifier].Paste(ref reader);
            // don't check if it contains it would mess with remaining data in the buffer
        }

        // from client to server
        internal static void OnNetworkIgnore(int socket, int connection, uint timestamp, ref Reader reader)
        {
            ulong identifier = reader.ReadULong();
            m_listeners[identifier].Remove(connection);
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
            Network.terminating.AddListener(OnNetworkShutdown);
        }
    }
}