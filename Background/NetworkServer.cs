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

        public static bool Running => m_server != null;

        internal static void Create(int port)
        {
            m_connections.Clear();
            m_server = new Server(port, Mode.IPV4);
        }

        internal static void Destroy()
        {
            m_connections.Clear();
            m_server.Close();
            m_server = null;
        }

        internal static void Update()
        {
            m_server?.Update(OnReceive, OnSync, OnError);
        }

        private static void OnError(int connection, Error error)
        {
            switch (error)
            {
                case Error.Timeout:
                case Error.Disconnected:

                    break;
            }
        }

        // acknowledge of a connect or disconnect request that was pushed from here(Server)
        private static void OnSync(int connection, Sync sync, uint rtt)
        {
            switch (sync)
            {
                case Sync.Ping:
                    if (!m_connections.Contains(connection))
                    {
                        // new connection
                        m_connections.Add(connection);
                    }
                    break;
                case Sync.Connect:
                    break;
                case Sync.Disconnect:
                    break;
            }
        }

        private static void OnReceive(int connection, uint timestamp, ref Reader reader)
        {
            Network.Receive(connection, timestamp, ref reader);
        }
    }
}