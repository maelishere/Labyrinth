using System;
using System.Net;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice;
    using Lattice.Delivery;

    public static class NetworkClient
    {
        internal static Client n_client;
        private static bool m_connected, m_disconnecting;

        public static bool Active => n_client != null;

        public static void Connect(IPEndPoint endpoint)
        {
            if (!NetworkServer.Active)
            {
                if (!Active)
                {
                    m_connected = false;
                    m_disconnecting = false;
                    n_client = new Client(Mode.IPV4, endpoint, OnReceive, OnRequest, OnAcknowledge, OnError);
                    NetworkStream.Send = (int connection, Channel channel, Write write) =>
                    {
                        n_client.Send(channel, write);
                    };
                    Network.initialized.Invoke(n_client.Local);
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
                n_client?.Disconnect();
                m_disconnecting = true;
            }
        }

        internal static void Close()
        {
            if (n_client != null)
            {
                Outgoing();
                m_disconnecting = false;
                NetworkStream.Clear();
                Network.terminating.Invoke(n_client.Local);

                /*n_client.Close();*/
                n_client = null;
            }
        }

        internal static void Receive()
        {
            n_client?.Receive();
        }

        internal static void Update()
        {
            n_client?.Update();
        }

        internal static void Send(Channel channel, Write write)
        {
            if (!m_disconnecting)
            {
                n_client.Send(channel, write);
            }
        }

        private static void Outgoing()
        {
            if (m_connected)
            {
                m_connected = false;
                NetworkStream.Outgoing(n_client.Remote);
                Network.Outgoing(n_client.Local, n_client.Remote);
            }
        }

        private static void Incoming()
        {
            if (!m_connected)
            {
                m_connected = true;
                NetworkStream.Incoming(n_client.Remote);
                Network.Incoming(n_client.Local, n_client.Remote);
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

        // acknowledge of a connect or disconnect request that was pushed from here(Client)
        private static void OnAcknowledge(Request request, uint rtt)
        {
            switch(request)
            {
                case Request.Ping:
                    Network.pinged.Invoke(n_client.Local, n_client.Remote, rtt);
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
            NetworkStream.Receive(n_client.Local, n_client.Remote, timestamp, ref reader);
        }
    }
}