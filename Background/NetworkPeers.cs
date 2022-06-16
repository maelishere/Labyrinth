using System.Net;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Snare;
    using System;

    public static class NetworkPeers
    {
        public class Meeting
        {
            public readonly int Session;
            public readonly Peer Handle;

            public Meeting(Family family, IPEndPoint remote, Action connected)
            {
                Session = remote.Serialize().GetHashCode();
                Handle = new Peer(Family.IPV4, remote, () => { OnConnected(remote.Port); });
            }
        }

        private readonly static Dictionary<int, Meeting> m_sessions = new Dictionary<int, Meeting>();

        public static bool Running => m_sessions.Count > 0;

        internal static void Connect(IPEndPoint endpoint)
        {
            m_sessions.Add(endpoint.Port, new Meeting(Family.IPV4, endpoint, ()=> { OnConnected(endpoint.Port); }));
        }

        internal static bool Diconnect(int session)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                meeting.Handle.Disconnect();
                m_sessions.Remove(session);
                return true;
            }
            return false;
        }

        internal static void Update()
        {
            foreach(var session in m_sessions)
            {
                if (!session.Value.Handle.Update(0,
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
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                meeting.Handle.Send(write);
                return true;
            }
            return false;
        }

        private static void OnConnected(int session)
        {
        }

        private static void OnReceive(int session, ref Reader reader)
        {
            Network.Receive(m_sessions[session].Session, session, ref reader);
        }
    }
}