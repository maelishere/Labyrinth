using System.Net;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Snare;

    public static class NetworkPeers
    {
        public class Meeting
        {
            public int Session;
            public Peer Member;

            public Meeting(int session, Peer member)
            {
                Session = session;
                Member = member;
            }
        }

        private readonly static Dictionary<int, Meeting> m_sessions = new Dictionary<int, Meeting>();

        public static bool Running => m_sessions.Count > 0;

        internal static void Join(IPEndPoint endpoint)
        {
            m_sessions.Add(endpoint.Port, new Meeting(endpoint.Serialize().GetHashCode(), new Peer(endpoint)));
        }

        internal static bool Leave(int session)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                meeting.Member.Leave();
                m_sessions.Remove(session);
                return true;
            }
            return false;
        }

        internal static void Update()
        {
            foreach(var session in m_sessions)
            {
                session.Value.Member.Update(0,
                    (ref Reader reader) =>
                    {
                        OnReceive(session.Key, ref reader);
                    });
            }
        }

        private static void OnError()
        {

        }

        private static void OnReceive(int session, ref Reader reader)
        {
            Network.Receive(m_sessions[session].Session, session, ref reader);
        }
    }
}