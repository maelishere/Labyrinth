using System.Net;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Snare;
    using System;

    public static class NetworkPeers
    {
        private readonly static Dictionary<int, Peer> m_sessions = new Dictionary<int, Peer>();

        internal static void Join(IPEndPoint endpoint)
        {
            m_sessions.Add(endpoint.Port, new Peer(endpoint));
        }

        internal static bool Leave(int session)
        {
            if (m_sessions.TryGetValue(session, out Peer peer))
            {
                peer.Leave();
                m_sessions.Remove(session);
                return true;
            }
            return false;
        }

        internal static void Update()
        {
            foreach(var session in m_sessions)
            {
                session.Value.Update(0,
                    (byte type, ref Reader reader) =>
                    {
                        OnReceive(session.Key, type, ref reader);
                    });
            }
        }

        private static void OnReceive(int session, byte type, ref Reader reader)
        {

        }
    }
}
