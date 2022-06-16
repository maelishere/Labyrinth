using System;
using System.Net;
using System.Collections.Generic;

using UnityEngine;

namespace Labyrinth
{
    using Bolt;
    using Background;

    public static class Network
    {
        internal static Comms m_local = new Comms();
        private static readonly Dictionary<byte, Flag> m_callbacks = new Dictionary<byte, Flag>();

        internal static void Receive(int connection, object state, ref Reader reader)
        {
            byte flag = reader.Read();
            if (m_callbacks.ContainsKey(flag))
            {
                m_callbacks[flag].Callback(connection, state, ref reader);
                return;
            }
            Debug.LogError($"Network received invalid flag [{flag}]");
        }

        // add a custom callback for a network flag
        public static bool Register(byte flag, Flag.Recieved callback)
        {
            if (!m_callbacks.ContainsKey(flag))
            {
                m_callbacks.Add(flag, new Flag(flag, callback));
                return true;
            }
            return false;
        }

        // intitialize a dedicated server or just a client
        public static void Initialize(int port)
        {
            NetworkServer.Listen(port);
            NetworkSessions.Listen(port);
        }

        // intitialize a client and connect to server
        public static void Initialize(string host, int port)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length < 1) 
                throw new ArgumentException($"{host} host not found");

            IPEndPoint endpoint = new IPEndPoint(addresses[0], port);
            NetworkClient.Connect(endpoint);
            NetworkPeers.Connect(endpoint);
        }

        public static void Transmit(byte flag, Write write)
        {

        }

        public static void Forward(int session, byte flag, Write write)
        {

        }

        public static void Terminate()
        {

        }

        private static void OnNetworkConnected(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkDisconnected(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        static Network()
        {
            Network.Register(Flag.Connected, OnNetworkConnected);
            Network.Register(Flag.Disconnected, OnNetworkDisconnected);
        }
    }
}