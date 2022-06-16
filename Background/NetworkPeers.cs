using System;
using System.Net;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Snare;

    public static class NetworkPeers
    {
        private readonly static Dictionary<int, Peer> m_sessions = new Dictionary<int, Peer>();

        public static bool Running => m_sessions.Count > 0;

        internal static void Connect(IPEndPoint session)
        {
            if (!NetworkSessions.Running)
            {
                Peer peer = new Peer(Family.IPV4, session, OnConnected);
                m_sessions.Add(peer.Remote, peer);
            }
            throw new InvalidOperationException($"Network Sessions is currently running");
        }

        internal static bool Diconnect(int session)
        {
            if (m_sessions.TryGetValue(session, out Peer peer))
            {
                peer.Disconnect();
                m_sessions.Remove(session);
                return true;
            }
            return false;
        }

        internal static void Update()
        {
            foreach(var session in m_sessions)
            {
                if (!session.Value.Update(0,
                    (ref Reader reader) =>
                    {
                        OnReceive(session.Key, ref reader);
                    }))
                {
                    // disconnected
                }
            }
        }

        internal static bool Send(int session, Write write)
        {
            if (m_sessions.TryGetValue(session, out Peer peer))
            {
                peer.Send(write);
                return true;
            }
            return false;
        }

        private static void OnConnected(int local, int remote)
        {
        }

        private static void OnReceive(int session, ref Reader reader)
        {
            Network.Receive(session, null, ref reader);
        }
    }
}