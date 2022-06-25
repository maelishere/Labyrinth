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
                    n_server = new Server(port, Mode.IPV4);
                    Network.initialized.Invoke(n_server.Listen);
                    NetworkThread.Run();
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
                n_server.Close();
                n_server = null;
            }
        }

        internal static void Tick()
        {
            n_server?.Tick(OnReceive, OnRequest, OnAcknowledge, OnError);
        }

        internal static void Update()
        {
            n_server?.Update(OnError);
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

        private static void Remove(int connection)
        {
            if (m_connections.Remove(connection))
            {
                Network.Outgoing(n_server.Listen, connection);
            }
        }

        private static void Add(int connection)
        {
            if (m_connections.Add(connection))
            {
                // new connection
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
                    /*NetworkLoop.n_callbacks.Enqueue(() => );*/
                    Remove(connection);
                    break;
            }
        }

        // request that was pushed from there(Client)
        private static void OnRequest(int connection, uint ts, Request request)
        {
            switch (request)
            {
                case Request.Connect:
                    /*NetworkLoop.n_callbacks.Enqueue(() => Add(connection));*/
                    break;
                case Request.Disconnect:
                    /*NetworkLoop.n_callbacks.Enqueue(() => Remove(connection));*/
                    break;
            }
        }

        // acknowledge of a connect or disconnect request that was pushed from here(Server)
        private static void OnAcknowledge(int connection, Request request, uint rtt)
        {
            switch (request)
            {
                case Request.Ping:
                    break;
                case Request.Connect:
                    /*NetworkLoop.n_callbacks.Enqueue(() => Add(connection));*/
                    Add(connection);
                    break;
                case Request.Disconnect:
                    /*NetworkLoop.n_callbacks.Enqueue(() => Remove(connection));*/
                    Remove(connection);
                    break;
            }
        }

        private static void OnReceive(int connection, uint timestamp, ref Reader reader)
        {
            Network.Receive(n_server.Listen, connection, timestamp, ref reader);
            /*NetworkLoop.n_received.Enqueue(new State(n_server.Listen, connection, timestamp, reader));*/
        }
    }
}