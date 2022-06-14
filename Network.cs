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
        private static readonly Dictionary<byte, Flag> m_callbacks = new Dictionary<byte, Flag>();
        private static readonly Dictionary<int, Comms> m_connections = new Dictionary<int, Comms>();

        /*public static bool running => ?;*/

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
            NetworkServer.Create(port);
            NetworkSessions.Create(port);
        }

        // intitialize a client and connect to server
        public static void Initialize(string host, int port)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length < 1) 
                throw new ArgumentException($"{host} host not found");

            IPEndPoint endpoint = new IPEndPoint(addresses[0], port);
            NetworkClient.Connect(endpoint);
            NetworkPeers.Join(endpoint);
        }

        public static void Terminate()
        {

        }

        private static void OnNetworkConnected(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkCreate(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkDestory(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkProcedure(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkSignature(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }


        private static void OnNetworkJoint(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkLeft(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkVoice(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkMessage(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        static Network()
        {
            // register built-in flags
            m_callbacks.Add(Flag.Connected, new Flag(Flag.Connected, OnNetworkConnected));
            m_callbacks.Add(Flag.Disconnected, new Flag(Flag.Disconnected, OnNetworkConnected));
            m_callbacks.Add(Flag.Create, new Flag(Flag.Connected, OnNetworkCreate));
            m_callbacks.Add(Flag.Destroy, new Flag(Flag.Disconnected, OnNetworkDestory));
            m_callbacks.Add(Flag.Procedure, new Flag(Flag.Connected, OnNetworkProcedure));
            m_callbacks.Add(Flag.Signature, new Flag(Flag.Disconnected, OnNetworkSignature));
            // others
            m_callbacks.Add(Flag.Joint, new Flag(Flag.Connected, OnNetworkJoint));
            m_callbacks.Add(Flag.Disconnected, new Flag(Flag.Disconnected, OnNetworkLeft));
            m_callbacks.Add(Flag.Voice, new Flag(Flag.Voice, OnNetworkVoice));
            m_callbacks.Add(Flag.Message, new Flag(Flag.Message, OnNetworkMessage));
        }
    }
}