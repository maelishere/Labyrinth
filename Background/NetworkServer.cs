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

        internal static void Create(int port)
        {
            m_connections.Clear();
            m_server = new Server(port, Mode.IPV4);
        }

        internal static void Destroy()
        {
            m_server.Close();
            m_connections.Clear();
            m_server = null;
        }

        internal static void Update()
        {
            m_server?.Update(OnReceive, OnSync, OnError);
        }

        private static void OnError(int connection, Error error)
        {
        }

        private static void OnSync(int connection, Sync sync, uint rtt)
        {
        }

        private static void OnReceive(int connection, uint timestamp, ref Reader reader)
        {
        }
    }
}