using System;
using System.Net;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice;
    using Lattice.Delivery;

    public static class NetworkClient
    {
        private static Client m_client;
        private static bool m_connected, m_disconnecting;

        public static bool Active => m_client != null;

        public static int Local => m_client.Local;
        public static int Remote => m_client.Remote;

        public static void Connect(IPEndPoint endpoint)
        {
            if (!NetworkServer.Active)
            {
                if (!Active)
                {
                    m_connected = false;
                    m_disconnecting = false;
                    m_client = new Client(Mode.IPV4, endpoint, OnReceive, OnRequest, OnAcknowledge, OnError);
                    NetworkStream.Send = (int connection, Channel channel, Write write) =>
                    {
                        m_client.Send(channel, write);
                    };
                    Network.initialized.Invoke(m_client.Local);
                    return;
                }
                throw new InvalidOperationException($"Network Client was already running");
            }
            throw new InvalidOperationException($"Network Server is currently running");
        }

        public static void Disconnect()
        {
            if (!m_disconnecting)
            {
                // inside the client(Host): it will stop pinging; if it times out, would still count as disconnected
                m_client?.Disconnect();
                m_disconnecting = true;
            }
        }

        internal static void Close()
        {
            if (m_client != null)
            {
                Outgoing();
                m_disconnecting = false;
                NetworkStream.Clear();
                Network.terminating.Invoke(m_client.Local);

                /*n_client.Close();*/
                m_client = null;
            }
        }

        internal static void Receive()
        {
            m_client?.Receive();
        }

        internal static void Update()
        {
            m_client?.Update();
        }

        internal static void Send(Channel channel, Write write)
        {
            if (!m_disconnecting)
            {
                m_client.Send(channel, write);
            }
        }

        private static void Outgoing()
        {
            if (m_connected)
            {
                m_connected = false;
                NetworkStream.Outgoing(m_client.Remote);
                Network.Outgoing(m_client.Local, m_client.Remote);
            }
        }

        private static void Incoming()
        {
            if (!m_connected)
            {
                m_connected = true;
                NetworkStream.Incoming(m_client.Remote);
                Network.Incoming(m_client.Local, m_client.Remote);
            }
        }

        private static void OnError(Error error)
        {
            switch (error)
            {
                case Error.Send:
                case Error.Recieve:
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
                    // server is reconnecting
                    Incoming();
                    break;
                case Request.Disconnect:
                    Close();
                    break;
            }
        }

        // acknowledge of a request that was pushed from here(Client)
        private static void OnAcknowledge(Request request, uint rtt)
        {
            switch(request)
            {
                case Request.Ping:
                    {
                        // not sure if i'm calculating this right
                        int ping = (int)(rtt - NetworkDebug.Delta);
                        ping = ping < 0 ? 0 : ping;

                        Network.pinged.Invoke(m_client.Local, m_client.Remote, ping);
                    }
                    break;
                case Request.Connect:
                    // handshake was successful
                    Incoming();
                    break;
                case Request.Disconnect:
                    Close();
                    break;
            }
        }

        private static void OnReceive(uint timestamp, ref Reader reader)
        {
            NetworkStream.Receive(m_client.Local, m_client.Remote, timestamp, ref reader);
        }
    }
}