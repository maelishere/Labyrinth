using System;
using System.Collections.Generic;

namespace Labyrinth
{
    using Bolt;
    using Background;

    public static class Network
    {
        private static readonly Dictionary<byte, Flag> m_callbacks = new Dictionary<byte, Flag>();
        private static readonly Dictionary<int, Comms> m_connections = new Dictionary<int, Comms>();

        public static bool Register(byte flag, Read callback)
        {
            if (!m_callbacks.ContainsKey(flag))
            {
                m_callbacks.Add(flag, new Flag(flag, callback));
                return true;
            }
            return false;
        }

        // we need to deceive if this is a dedicated server or just a client
        public static void Initialize()
        {

        }

        public static void Terminate()
        {

        }

        private static void OnNetworkConnected(ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkCreate(ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkDestory(ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkProcedure(ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkSignature(ref Reader reader)
        {
            throw new NotImplementedException();
        }


        private static void OnUserJoint(ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnUserLeft(ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkVoice(ref Reader reader)
        {
            throw new NotImplementedException();
        }

        private static void OnNetworkMessage(ref Reader reader)
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
            m_callbacks.Add(Flag.Joint, new Flag(Flag.Connected, OnUserJoint));
            m_callbacks.Add(Flag.Disconnected, new Flag(Flag.Disconnected, OnUserLeft));
            m_callbacks.Add(Flag.Voice, new Flag(Flag.Voice, OnNetworkVoice));
            m_callbacks.Add(Flag.Message, new Flag(Flag.Message, OnNetworkMessage));
        }
    }
}