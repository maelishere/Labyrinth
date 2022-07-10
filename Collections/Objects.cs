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
        public const byte Find = 1;
        public const byte Link = 2;
        public const byte Modify = 3;
        public const byte Ignore = 4;

        private static readonly Dictionary<ulong, Unit> m_units = new Dictionary<ulong, Unit>();
        private static readonly Dictionary<int, HashSet<ulong>> m_queries = new Dictionary<int, HashSet<ulong>>(); /*the objects each client is looking for*/
        private static readonly Dictionary<ulong, HashSet<int>> m_listeners = new Dictionary<ulong, HashSet<int>>(); /*the clients that have created each object*/

        private static void OnNetworkShutdown(int sokcet)
        {
            m_units.Clear();
            m_queries.Clear();
            m_queries.Clear();
        }

        internal static bool Add(string type, ushort instance, ushort member, Unit unit)
        {
            ulong identifier = Generate(type.Hash(), instance, member);
            if (!m_units.ContainsKey(identifier))
            {
                unit.identifier = identifier;
                m_units.Add(identifier, unit);
                if (NetworkServer.Active)
                {
                    m_listeners.Add(identifier, new HashSet<int>());
                }
                if (NetworkClient.Active)
                {
                    // send find to server
                    // it doesn't matter if this is received orderedly
                    // the server won't sync the object until you send this
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
            if (m_units[identifier] != null)
            {
                m_units[identifier].identifier = 0;
            }
            m_units.Remove(identifier);
            if (NetworkServer.Active)
            {
                m_listeners.Remove(identifier);
            }
            if (NetworkClient.Active)
            {
                // send ignore to server
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
                List<KeyValuePair<ulong, int>> cloned = new List<KeyValuePair<ulong, int>>();
                foreach (var query in m_queries)
                {
                    HashSet<ulong> found = new HashSet<ulong>();

                    foreach (var identifier in query.Value)
                    {
                        if (m_units.ContainsKey(identifier))
                        {
                            if (m_units[identifier] != null)
                            {
                                NetworkDebug.Slient($"Found Object({identifier}) For Client({query.Key})");

                                found.Add(identifier);
                                cloned.Add(new KeyValuePair<ulong, int>(identifier, query.Key));
                                // send clone to client (Link)
                                Network.Forward(query.Key, Channels.Irregular, Link, (ref Writer writer) =>
                                {
                                    writer.Write(identifier);
                                    m_units[identifier].Clone(ref writer);
                                });
                            }
                        }
                    }

                    foreach (var identifier in found)
                    {
                        query.Value.Remove(identifier);
                    }
                }

                HashSet<ulong> missing = new HashSet<ulong>();
                foreach (var unit in m_units)
                {
                    if (unit.Value != null)
                    {
                        if (unit.Value.changed)
                        {
                            NetworkDebug.Slient($"Object({unit.Key}) has changed");

                            bool listeners = m_listeners[unit.Key].Count > 0;

                            /// copy can only be called once 
                            ///     to capture all changes
                            ///     (too much data shouldn't be sent recklessly)
                            Writer buffer = listeners ? new Writer(Network.Buffer - 1) : null;
                            unit.Value.Copy(ref buffer, listeners);

                            foreach (var connection in m_listeners[unit.Key])
                            {
                                // send changes to clients (Modifiy)
                                NetworkDebug.Slient($"Sending changes made to Object({unit.Key}) to Client({connection})");
                                Network.Forward(connection, Channels.Irregular, Modify, (ref Writer writer) =>
                                {
                                    writer.Write(unit.Key);
                                    writer.Write(buffer.ToSegment());
                                });
                            }
                        }
                    }
                    else
                    {
                        missing.Add(unit.Key);
                    }
                }

                // we don't need to send it a copy when we sent a clone
                foreach (var clone in cloned)
                {
                    m_listeners[clone.Key].Add(clone.Value);
                }

                // when you forget to call Unit.Destroy
                //      before it's removed from system memory
                foreach (var unit in missing)
                {
                    Remove(unit);
                }
            }
        }

        // from client to server
        internal static void OnNetworkFind(int socket, int connection, uint timestamp, ref Reader reader)
        {
            ulong identifier = reader.ReadULong();
            NetworkDebug.Slient($"Client({connection}) looking for Object({identifier})");
            if (m_queries.ContainsKey(connection))
            {
                m_queries[connection].Add(identifier);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Client({connection}) isn't connected");
            }
        }

        // from server to clients
        internal static void OnNetworkLink(int socket, int connection, uint timestamp, ref Reader reader)
        {
            ulong identifier = reader.ReadULong();
            NetworkDebug.Slient($"Server({connection}) found Object({identifier})");
            if (m_units.ContainsKey(identifier))
            {
                m_units[identifier].Apply(ref reader);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Object({identifier}) wasn't created or was already destoryed on Client({socket})");
            }
        }

        // from server to clients
        internal static void OnNetworkModify(int socket, int connection, uint timestamp, ref Reader reader)
        {
            ulong identifier = reader.ReadULong();
            NetworkDebug.Slient($"Server({connection}) modifiying Object({identifier})");
            if (m_units.ContainsKey(identifier))
            {
                m_units[identifier].Paste(ref reader);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Object({identifier}) wasn't created or was already destoryed on Client({socket})");
            }
        }

        // from client to server
        internal static void OnNetworkIgnore(int socket, int connection, uint timestamp, ref Reader reader)
        {
            ulong identifier = reader.ReadULong();
            NetworkDebug.Slient($"Client({connection}) deleted Object({identifier})");
            if (m_units.ContainsKey(identifier))
            {
                m_listeners[identifier].Remove(connection);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Object({identifier}) wasn't created or was already destoryed on Client({socket})");
            }
        }

        private static ulong Generate(uint name, ushort index, ushort offset)
        {
            return Combine(name, Combine(index, offset));
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