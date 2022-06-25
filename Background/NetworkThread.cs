using System;
using System.Threading;

namespace Labyrinth.Background
{
    public static class NetworkThread
    {
        private static bool m_abort;
        /*private static Thread m_worker;*/

        // how many times a second it checks for new packets
        public static int Tick { get; set; } = 64;
        public static bool Ticking { get; private set; }

        internal static void Run()
        {
            if (Ticking)
            {
                throw new InvalidOperationException($"Network thread is already ticking, abort frist");
            }

            m_abort = false;
            /*m_worker = new Thread(Running);
            m_worker.Start();*/
        }

        internal static void Abort()
        {
            m_abort = true;
        }

        private static void Running()
        {
            Ticking = true;
            while (Network.Running && !m_abort)
            {
                // receive from sockets 
                /*NetworkServer.Tick();
                NetworkClient.Tick();*/
                Thread.Sleep(1000 / Tick);
            }
            Ticking = false;
            /*m_worker = null;*/
            m_abort = false;
        }
    }
}