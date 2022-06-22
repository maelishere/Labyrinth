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
        internal static bool n_connected;

        public static bool Active => n_client != null;

        private static void Close()
        {
            Network.Outgoing(n_client.Local, n_client.Remote);
            Network.terminating.Invoke(n_client.Local);
            n_connected = false;
            n_client.Close();
            n_client = null;
        }

        internal static void Connect(IPEndPoint endpoint)
        {
            if (!NetworkServer.Active)
            {
                if (!Active)
                {
                    n_connected = false;
                    n_client = new Client(Mode.IPV4, endpoint, OnReceive, OnRequest, OnAcknowledge, OnError);
                    Network.initialized.Invoke(n_client.Local);
                    NetworkThread.Run();
                    return;
                }
                throw new InvalidOperationException($"Network Client was already running");
            }
            throw new InvalidOperationException($"Network Server is currently running");
        }

        internal static void Disconnect()
        {
            n_client?.Disconnect();
        }

        internal static void Tick()
        {
            n_client?.Tick(OnError);
        }

        internal static void Update()
        {
            n_client?.Update(OnError);
        }

        internal static void Send(Channel channel, Write write)
        {
            n_client.Send(channel, write);
        }

        private static void OnError(Error error)
        {
            switch (error)
            {
                case Error.Send:
                case Error.Recieve:
                case Error.Timeout:
                    NetworkLoop.n_callbacks.Enqueue(() => Close());
                    break;
            }
        }

        // request that was pushed from there(Server)
        private static void OnRequest(uint ts, Request request)
        {
            switch (request)
            {
                case Request.Connect:
                    if (!n_connected)
                    {
                        n_connected = true;
                        NetworkLoop.n_callbacks.Enqueue(() => Network.Incoming(n_client.Local, n_client.Remote));
                    }
                    break;
                case Request.Disconnect:
                    NetworkLoop.n_callbacks.Enqueue(() => Close());
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
                    if (!n_connected)
                    {
                        n_connected = true;
                        NetworkLoop.n_callbacks.Enqueue(() => Network.Incoming(n_client.Local, n_client.Remote));
                    }
                    break;
                case Request.Disconnect:
                    NetworkLoop.n_callbacks.Enqueue(() => Close());
                    break;
            }
        }

        private static void OnReceive(uint timestamp, ref Reader reader)
        {
            /*Network.Receive(n_client.Local, n_client.Remote, timestamp, ref reader);*/
            NetworkLoop.n_received.Enqueue(new State(n_client.Local, n_client.Remote, timestamp, reader));
        }
    }
}