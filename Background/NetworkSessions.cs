using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Snare;

    public static class NetworkSessions
    {
        private readonly static Dictionary<int, Session> m_sessions = new Dictionary<int, Session>();

        public static bool Running => m_sessions.Count > 0;

        internal static void Create(int port)
        {
            m_sessions.Add(port, new Session(AddressFamily.InterNetwork, new IPEndPoint(IPAddress.Any, port), 60));
        }

        internal static bool Destroy(int session)
        {
            if (m_sessions.TryGetValue(session, out Session meeting))
            {
                meeting.Close();
                m_sessions.Remove(session);
                return true;
            }
            return false;
        }

        internal static void Update()
        {
            foreach (var session in m_sessions)
            {
                session.Value.Update(0,
                    OnJoined,
                    (int peer, ref Reader reader) =>
                    {
                        OnReceive(session.Key, peer, ref reader);
                    });
            }
        }

        private static void OnInternalError()
        {

        }

        private static void OnError(int connection)
        {

        }

        private static void OnLeft(int connection)
        {
        }

        private static void OnJoined(int connection)
        {
        }

        private static void OnReceive(int session, int connection, ref Reader reader)
        {
            Network.Receive(connection, session, ref reader);
        }
    }
}