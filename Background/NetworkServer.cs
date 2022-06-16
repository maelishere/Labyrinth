using System;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice;
    using Lattice.Delivery;

    public static class NetworkServer
    {
        internal static Server m_server;
        private static readonly HashSet<int> m_connections = new HashSet<int>();

        public static bool Running => m_server != null;

        internal static void Listen(int port)
        {
            if (!NetworkClient.Running)
            {
                if (!Running)
                {
                    m_connections.Clear();
                    m_server = new Server(port, Mode.IPV4);
                    return;
                }
                throw new InvalidOperationException($"Network Server was already running");
            }
            throw new InvalidOperationException($"Network Client is currently running");
        }

        internal static void Destroy()
        {
            m_connections.Clear();
            m_server.Close();
            m_server = null;
        }

        internal static void Update()
        {
            m_server?.Update(OnReceive, OnRequest, OnAcknowledge, OnError);
        }

        internal static void Send(Channel channel, Write write)
        {
            if (m_server != null)
            {
                foreach (var connection in m_connections)
                {
                    m_server.Send(connection, channel, write);
                }
            }
        }

        internal static bool Send(int connection, Channel channel, Write write)
        {
            if (m_server != null && m_connections.Contains(connection))
            {
                m_server.Send(connection, channel, write);
                return true;
            }
            return false;
        }

        internal static void Send(Func<int, bool> predicate, Channel channel, Write write)
        {
            if (m_server != null)
            {
                foreach (var connection in m_connections)
                {
                    if (predicate(connection))
                    {
                        m_server.Send(connection, channel, write);
                    }
                }
            }
        }

        private static void OnError(int connection, Error error)
        {
            switch (error)
            {
                case Error.Timeout:
                    m_connections.Remove(connection);
                    break;
            }
        }

        // request that was pushed from there(Client)
        private static void OnRequest(int connection, uint ts, Request request)
        {
            switch (request)
            {
                case Request.Connect:
                    if (m_connections.Add(connection))
                    {
                        // new connection
                    }
                    break;
                case Request.Disconnect:
                    m_connections.Remove(connection);
                    break;
            }
        }

        // acknowledge of a connect or disconnect request that was pushed from here(Server)
        private static void OnAcknowledge(int connection, Request request, uint rtt)
        {
            switch (request)
            {
                case Request.Ping:
                case Request.Connect:
                    if (m_connections.Add(connection))
                    {
                        // new connection
                    }
                    break;
                case Request.Disconnect:
                    m_connections.Remove(connection);
                    break;
            }
        }

        private static void OnReceive(int connection, uint timestamp, ref Reader reader)
        {
            Network.Receive(connection, timestamp, ref reader);
        }
    }
}