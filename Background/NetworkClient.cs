using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice;
    using Lattice.Delivery;

    public static class NetworkClient
    {
        private static Client m_client;

        internal static void Connect(IPEndPoint endpoint)
        {
            m_client = new Client(Mode.IPV4, endpoint, OnReceive, OnSync, OnError);
        }

        internal static void Disconnect()
        {
            m_client?.Disconnect();
        }

        internal static void Update()
        {
            m_client?.Update(OnError);
        }

        private static void OnError(Error error)
        {
        }

        private static void OnSync(Sync sync, uint rtt)
        {
        }

        private static void OnReceive(uint timestamp, ref Reader reader)
        {
        }
    }
}