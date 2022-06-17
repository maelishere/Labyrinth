using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;

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

        internal static Write Pack(byte flag, Write write)
        {
            return (ref Writer writer) =>
            {
                writer.Write(flag);
                write?.Invoke(ref writer);
            };
        }

        internal static void Receive(int connection, object state, ref Reader reader)
        {
            byte flag = reader.Read();
            if (m_callbacks.ContainsKey(flag))
            {
                m_callbacks[flag].Callback(connection, state, ref reader);
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
                return new IPEndPoint(addresses[0], port);
            }
            throw new ArgumentException($"{host} host not found");
        }

        public static void Stream(int session, Write write)
        {

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
                return NetworkServer.m_server.Listen;
            if (NetworkClient.Running)
            {
                if (remote)
                    return NetworkClient.m_client.Remote;
                else
                    return NetworkClient.m_client.Local;
            }
            return Identity.Any;
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

        /// from server or session about new connections or existing connections
        private static void OnNetworkConnected(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        /// from server or session about diconnections
        private static void OnNetworkDisconnected(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        static Network()
        {
            Register(Flag.Connected, OnNetworkConnected);
            Register(Flag.Disconnected, OnNetworkDisconnected);
        }
    }
}