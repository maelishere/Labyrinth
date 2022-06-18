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
            public readonly HashSet<int> Members;

            public Meeting(Family family, int port, int max)
            {
                Handle = new Session(Family.IPV4, port, max);
                Members = new HashSet<int>();
            }
        }

        private readonly static Dictionary<int, Meeting> m_sessions = new Dictionary<int, Meeting>();

        public static bool Running => m_sessions.Count > 0;

        internal static void Listen(int port)
        {
            if (!NetworkMembers.Running)
            {
                m_sessions.Add(port, new Meeting(Family.IPV4, port, 60)); ;
            }
            throw new InvalidOperationException($"Network Members is currently running");
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
                    (int member, ref Reader reader) =>
                    {
                        OnReceive(session.Key, member, ref reader);
                    }, (int connection) => OnDisconnected(session.Key, connection));
            }
        }

        internal static bool Send(int session, Write write)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                foreach (var member in meeting.Members)
                {
                    meeting.Handle.Send(member, write);
                }
                return true;
            }
            return false;
        }

        internal static bool Send(int session, int member, Write write)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                meeting.Handle.Send(member, write);
                return true;
            }
            return false;
        }

        internal static bool Send(int session, Func<int, bool> predicate, Write write)
        {
            if (m_sessions.TryGetValue(session, out Meeting meeting))
            {
                foreach (var member in meeting.Members)
                {
                    if (predicate(member))
                    {
                        meeting.Handle.Send(member, write);
                    }
                }
                return true;
            }
            return false;
        }

        private static void OnConnected(int session, int connection)
        {
            m_sessions[session].Members.Add(connection);
        }

        private static void OnDisconnected(int session, int connection)
        {
            m_sessions[session].Members.Remove(connection);
        }

        private static void OnReceive(int session, int connection, ref Reader reader)
        {
            // session send to all members imideatily 
            byte[] bytes = reader.Read(reader.Length - reader.Current);
            Send(session, (c) => c != connection, (ref Writer writer) => writer.Write(bytes));

            // incase server has to process something
            Network.Receive(connection, session, ref reader);
        }
    }
}