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
                    (int peer, byte type, ref Reader reader) =>
                    {
                        OnReceive(session.Key, peer, type, ref reader);
                    });
            }
        }

        private static void OnJoined(int connection)
        {
        }

        private static void OnReceive(int session, int peer, byte type, ref Reader reader)
        {
        }
    }
}