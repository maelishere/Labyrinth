using System;
using System.Net;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice;
    using Lattice.Delivery;

    public static class NetworkClient
    {
        internal static Client m_client;
        internal static bool m_connected;

        public static bool Running => m_client != null;

        private static void Close()
        {
            m_connected = false;
            m_client.Close();
            m_client = null;
        }

        internal static void Connect(IPEndPoint endpoint)
        {
            if (!NetworkServer.Running)
            {
                if (!Running)
                {
                    m_connected = false;
                    m_client = new Client(Mode.IPV4, endpoint, OnReceive, OnRequest, OnAcknowledge);
                    return;
                }
                throw new InvalidOperationException($"Network Client was already running");
            }
            throw new InvalidOperationException($"Network Server is currently running");
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
                    Close();
                    break;
            }
        }

        // request that was pushed from there(Server)
        private static void OnRequest(uint ts, Request request)
        {
            switch (request)
            {
                case Request.Connect:
                    if (!m_connected)
                    {
                        m_connected = true;
                    }
                    break;
                case Request.Disconnect:
                    Close();
                    break;
            }
        }

        // acknowledge of a connect or disconnect request that was pushed from here(Client)
        private static void OnAcknowledge(Request request, uint rtt)
        {
            switch(request)
            {
                case Request.Ping:
                case Request.Connect:
                    if (!m_connected)
                    {
                        m_connected = true;
                    }
                    break;
                case Request.Disconnect:
                    Close();
                    break;
            }
        }

        private static void OnReceive(uint timestamp, ref Reader reader)
        {
            Network.Receive(m_client.Remote, timestamp, ref reader);
        }
    }
}