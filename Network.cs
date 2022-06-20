using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Labyrinth
{
    using Bolt;
    using Background;
    using Lattice.Delivery;

    public static class Network
    {
        // so you don't need to use "using Lattice.Delivery" every time you need to send
        public const Channel Fickle = Channel.Direct;
        public const Channel Abnormal = Channel.Irregular;
        public const Channel Reliable = Channel.Ordered;

        private static readonly Dictionary<byte, Flag> m_callbacks = new Dictionary<byte, Flag>();

        public static UnityEvent<int> initialized { get; } = new UnityEvent<int>();
        public static UnityEvent<int> terminating { get; } = new UnityEvent<int>();
        public static UnityEvent<int, int> connected { get; } = new UnityEvent<int, int>();
        public static UnityEvent<int, int> disconnected { get; } = new UnityEvent<int, int>();

        private static Write Pack(byte flag, Write write)
        {
            return (ref Writer writer) =>
            {
                writer.Write(flag);
                write?.Invoke(ref writer);
            };
        }

        internal static void Incoming(int socket, int connection)
        {
            if (NetworkServer.Running)
            {
                if (socket == NetworkServer.n_server.Listen)
                {
                    Forward((c) => c != connection, Reliable,
                        Flag.Connected, (ref Writer writer) => writer.Write(connection));
                }
            }

            connected.Invoke(socket, connection);
        }

        internal static void Outgoing(int socket, int connection)
        {
            if (NetworkServer.Running)
            {
                if (socket == NetworkServer.n_server.Listen)
                {
                    Forward((c) => c != connection, Reliable,
                        Flag.Disconnected, (ref Writer writer) => writer.Write(connection));
                }
            }

            disconnected.Invoke(socket, connection);
        }

        internal static void Receive(int socket, int connection, object state, ref Reader reader)
        {
            byte flag = reader.Read();
            if (m_callbacks.ContainsKey(flag))
            {
                m_callbacks[flag].Callback(socket, connection, state, ref reader);
                return;
            }
            Debug.LogError($"Network received invalid flag [{flag}]");
        }

        // add a custom callback for a network flag
        public static bool Register(byte flag, Flag.Recieved callback)
        {
            if (!m_callbacks.ContainsKey(flag))
            {
                m_callbacks.Add(flag, new Flag(flag, callback));
                return true;
            }
            return false;
        }

        public static IPEndPoint Resolve(string host, int port)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length > 1)
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    if (addresses[i].AddressFamily == AddressFamily.InterNetwork)
                        return new IPEndPoint(addresses[i], port);
                }
            }
            throw new ArgumentException($"{host} host not found");
        }

        public static void Forward(Channel channel, byte flag, Write write)
        {
            if (NetworkServer.Running)
            {
                NetworkServer.Send(channel, Pack(flag, write));
                return;
            }
            if (NetworkClient.Running)
            {
                NetworkClient.Send(channel, Pack(flag, write));
            }
        }

        public static void Forward(int connection, Channel channel, byte flag, Write write)
        {
            if (NetworkServer.Running)
            {
                NetworkServer.Send(connection, channel, Pack(flag, write));
            }
        }

        public static void Forward(Func<int, bool> predicate, Channel channel, byte flag, Write write)
        {
            if (NetworkServer.Running)
            {
                NetworkServer.Send(predicate, channel, Pack(flag, write));
            }
        }

        public static int Authority(bool remote = false)
        {
            if (NetworkServer.Running)
                return NetworkServer.n_server.Listen;
            if (NetworkClient.Running)
            {
                if (remote)
                    return NetworkClient.n_client.Remote;
                else
                    return NetworkClient.n_client.Local;
            }
            return Identity.Any;
        }

        public static bool Authority(int connection, bool remote = false)
        {
            return connection == Identity.Any ? false : connection == Authority(remote);
        }

        public static bool Internal(Host local)
        {
            switch (local)
            {
                default:
                case Host.Any: return NetworkServer.Running | NetworkClient.Running;
                case Host.Client: return NetworkClient.Running;
                case Host.Server: return NetworkServer.Running;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(Flag.Connected, OnNetworkConnected);
            Register(Flag.Disconnected, OnNetworkDisconnected);
        }

        /// from server [to client] about new connections
        private static void OnNetworkConnected(int socket, int connection, object state, ref Reader reader)
        {
            connected.Invoke(connection, reader.ReadInt());
        }

        /// from server [to client] about diconnections
        private static void OnNetworkDisconnected(int socket, int connection, object state, ref Reader reader)
        {
            disconnected.Invoke(connection, reader.ReadInt());
        }
    }
}