using System;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Snare;

    public static class NetworkSessions
    {
        public class Meeting
        {
            public readonly Session Handle;
            public readonly HashSet<int> Peers;

            public Meeting(Family family, int port, int max)
            {
                Handle = new Session(Family.IPV4, port, max);
                Peers = new HashSet<int>();
            }
        }

        private readonly static Dictionary<int, Meeting> m_sessions = new Dictionary<int, Meeting>();

        public static bool Running => m_sessions.Count > 0;

        internal static void Listen(int port)
        {
            if (!NetworkPeers.Running)
            {
                m_sessions.Add(port, new Meeting(Family.IPV4, port, 60)); ;
            }
            throw new InvalidOperationException($"Network Peers is currently running");
        }

        internal static bool Destroy(int session)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                meeting.Handle.Close();
                m_sessions.Remove(session);
                return true;
            }
            return false;
        }

        internal static void Update()
        {
            foreach (var session in m_sessions)
            {
                session.Value.Handle.Update(0,
                    (int connection) => OnConnected(session.Key, connection),
                    (int peer, ref Reader reader) =>
                    {
                        OnReceive(session.Key, peer, ref reader);
                    }, (int connection) => OnDisconnected(session.Key, connection));
            }
        }

        internal static bool Send(int session, Write write)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                foreach (var peer in meeting.Peers)
                {
                    meeting.Handle.Send(peer, write);
                }
                return true;
            }
            return false;
        }

        internal static bool Send(int session, int peer, Write write)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                meeting.Handle.Send(peer, write);
                return true;
            }
            return false;
        }

        internal static bool Send(int session, Func<int, bool> predicate, Write write)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                foreach (var peer in meeting.Peers)
                {
                    if (predicate(peer))
                    {
                        meeting.Handle.Send(peer, write);
                    }
                }
                return true;
            }
            return false;
        }

        private static void OnConnected(int session, int connection)
        {
        }

        private static void OnDisconnected(int session, int connection)
        {
        }

        private static void OnReceive(int session, int connection, ref Reader reader)
        {
            Network.Receive(connection, session, ref reader);
        }
    }
}