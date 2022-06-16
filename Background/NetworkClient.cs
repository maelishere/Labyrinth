using System;
using System.Net;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice;
    using Lattice.Delivery;

    public static class NetworkClient
    {
        private static int m_server;
        private static Client m_client;
        private static bool m_connected;

        public static bool Running => m_client != null;

        private static void Close()
        {
            m_connected = false;
            m_client.Close();
            m_client = null;
            m_server = 0;
        }

        internal static void Connect(IPEndPoint endpoint)
        {
            m_connected = false;
            m_server = endpoint.Serialize().GetHashCode();
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

        internal static void Send(Channel channel, Write write)
        {
            m_client?.Send(channel, write);
        }

        private static void OnError(Error error)
        {
            switch (error)
            {
                case Error.Timeout:
                case Error.Disconnected:
                    Close();
                    break;
            }
        }

        // acknowledge of a connect or disconnect request that was pushed from here(Client)
        private static void OnSync(Sync sync, uint rtt)
        {
            switch(sync)
            {
                case Sync.Ping:
                case Sync.Connect:
                    if (!m_connected)
                    {
                        m_connected = true;
                    }
                    break;
                case Sync.Disconnect:
                    Close();
                    break;
            }
        }

        private static void OnReceive(uint timestamp, ref Reader reader)
        {
            Network.Receive(m_server, timestamp, ref reader);
        }
    }
}