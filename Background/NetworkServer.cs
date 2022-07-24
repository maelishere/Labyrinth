using System;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice;
    using Lattice.Delivery;

    public static class NetworkServer
    {
        private static Server m_server;
        private static readonly HashSet<int> m_connections = new HashSet<int>();

        public static bool Active => m_server != null;

        public static int Local => m_server.Listen;

        public static void Each(Action<int> callback)
        {
            foreach (var connection in m_connections)
            {
                callback(connection);
            }
        }

        public static void Each(Func<int, bool> predicate, Action<int> callback)
        {
            foreach (var connection in m_connections)
            {
                if (predicate(connection))
                {
                    callback(connection);
                }
            }
        }

        public static void Listen(int port)
        {
            if (!NetworkClient.Active)
            {
                if (!Active)
                {
                    m_connections.Clear();
                    m_server = new Server(Mode.IPV4, port, OnReceive, OnRequest, OnAcknowledge, OnError);
                    NetworkStream.Send = (int connection, Channel channel, Write write) =>
                    {
                        m_server.Send(connection, channel, write);
                    };
                    Network.initialized.Invoke(m_server.Listen);
                    return;
                }
                throw new InvalidOperationException($"Network Server was already running");
            }
            throw new InvalidOperationException($"Network Client is currently running");
        }

        public static void Close()
        {
            if (m_server != null)
            {
                NetworkStream.Clear();
                Network.terminating.Invoke(m_server.Listen);
                m_connections.Clear();

                /*n_server.Close();*/
                m_server = null;
            }
        }

        internal static void Receive()
        {
            m_server?.Receive();
        }

        internal static void Update()
        {
            m_server?.Update();
        }

        internal static void Send(Channel channel, Write write)
        {
            if (m_server != null)
            {
                m_server.Send(channel, write);
            }
        }

        internal static bool Send(int connection, Channel channel, Write write)
        {
            return m_server.Send(connection, channel, write);
        }

        internal static void Send(Func<int, bool> predicate, Channel channel, Write write)
        {
            if (m_server != null)
            {
                m_server.Send(predicate, channel, write);
            }
        }

        private static void Outgoing(int connection)
        {
            if (m_connections.Remove(connection))
            {
                NetworkStream.Outgoing(connection);
                Network.Outgoing(m_server.Listen, connection);
            }
        }

        private static void Incoming(int connection)
        {
            if (m_connections.Add(connection))
            {
                // new connection
                NetworkStream.Incoming(connection);
                Network.Incoming(m_server.Listen, connection);
            }
        }

        private static void OnError(int connection, Error error)
        {
            switch (error)
            {
                case Error.Send:
                case Error.Recieve:
                case Error.Timeout:
                    Outgoing(connection);
                    break;
            }
        }

        // request that was pushed from there(Client)
        private static void OnRequest(int connection, uint ts, Request request)
        {
            switch (request)
            {
                case Request.Connect:
                    Incoming(connection);
                    break;
                case Request.Disconnect:
                    Outgoing(connection);
                    break;
            }
        }

        // acknowledge of a request that was pushed from here(Server)
        private static void OnAcknowledge(int connection, Request request, uint rtt)
        {
            switch (request)
            {
                case Request.Ping:
                    {
                        // not sure if i'm calculating this right
                        int ping = (int)(rtt - NetworkDebug.Delta);
                        ping = ping < 0 ? 0 : ping;

                        Network.pinged.Invoke(m_server.Listen, connection, ping);
                    }
                    break;
                case Request.Connect:
                    // reconnection (not implemented yet!)
                    Incoming(connection);
                    break;
                case Request.Disconnect:
                    Outgoing(connection);
                    break;
            }
        }

        private static void OnReceive(int connection, uint timestamp, ref Reader reader)
        {
            NetworkStream.Receive(m_server.Listen, connection, timestamp, ref reader);
        }
    }
}