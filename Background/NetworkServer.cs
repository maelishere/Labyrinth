using System;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice;
    using Lattice.Delivery;

    public static class NetworkServer
    {
        internal static Server n_server;
        private static readonly HashSet<int> m_connections = new HashSet<int>();

        public static bool Active => n_server != null;

        public static void Each(Func<int, bool> fliter, Action<int> callback)
        {
            foreach (var connection in m_connections)
            {
                if (fliter(connection))
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
                    n_server = new Server(Mode.IPV4, port, OnReceive, OnRequest, OnAcknowledge, OnError);
                    Network.initialized.Invoke(n_server.Listen);
                    return;
                }
                throw new InvalidOperationException($"Network Server was already running");
            }
            throw new InvalidOperationException($"Network Client is currently running");
        }

        public static void Close()
        {
            if (n_server != null)
            {
                Network.terminating.Invoke(n_server.Listen);
                m_connections.Clear();

                /*n_server.Close();*/
                n_server = null;
            }
        }

        internal static void Receive()
        {
            n_server?.Receive();
        }

        internal static void Update()
        {
            n_server?.Update();
        }

        internal static void Send(Channel channel, Write write)
        {
            if (n_server != null)
            {
                n_server.Send(channel, write);
            }
        }

        internal static bool Send(int connection, Channel channel, Write write)
        {
            return n_server.Send(connection, channel, write);
        }

        internal static void Send(Func<int, bool> predicate, Channel channel, Write write)
        {
            if (n_server != null)
            {
                n_server.Send(predicate, channel, write);
            }
        }

        private static void Outgoing(int connection)
        {
            if (m_connections.Remove(connection))
            {
                NetworkStream.Outgoing(connection);
                Network.Outgoing(n_server.Listen, connection);
            }
        }

        private static void Incoming(int connection)
        {
            if (m_connections.Add(connection))
            {
                // new connection
                NetworkStream.Incoming(connection);
                Network.Incoming(n_server.Listen, connection);
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

        // acknowledge of a connect or disconnect request that was pushed from here(Server)
        private static void OnAcknowledge(int connection, Request request, uint rtt)
        {
            switch (request)
            {
                case Request.Ping:
                    Network.pinged.Invoke(n_server.Listen, connection, rtt);
                    break;
                case Request.Connect:
                    // reconnection
                    Incoming(connection);
                    break;
                case Request.Disconnect:
                    Outgoing(connection);
                    break;
            }
        }

        private static void OnReceive(int connection, uint timestamp, ref Reader reader)
        {
            Network.Receive(n_server.Listen, connection, timestamp, ref reader);
        }
    }
}