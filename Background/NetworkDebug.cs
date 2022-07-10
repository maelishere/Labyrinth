using UnityEngine;
using System.Diagnostics;

namespace Labyrinth.Background
{
    using Lattice;

    // i know this needs work
    // it's not accraute (don't trust it)
    public static class NetworkDebug
    {
        private static int m_losing;
        private static int m_sending;
        private static int m_receiving;

        private static readonly Stopwatch m_stopwatch = new Stopwatch();

        // How many retransimisions per second
        public static int Loss => m_losing;
        // How many bytes are sent per second
        public static int Sent => m_sending;
        // How many byte are received per second
        public static int Received => m_receiving;

        public static bool DebugSlient { get; set; } = false;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Log.POut = UnityEngine.Debug.Log;
            Log.WOut = UnityEngine.Debug.LogWarning;
            Log.EOut = UnityEngine.Debug.LogError;

            // called on update
            /*Log.Loss = () =>
            {
                m_losing++;
            };*/

            // called on receive
            Log.Lost = (int amount) =>
            {
                m_losing += amount;
            };

            // call depends 
            Log.Sent = (int amount) =>
            {
                m_sending += amount;
            };

            // called on receive
            Log.Received = (int amount) =>
            {
                m_receiving += amount;
            };

            m_stopwatch.Start();
        }

        internal static void EarlyReset()
        {
            if (m_stopwatch.ElapsedMilliseconds % 1000 == 0)
            {
                m_sending = 0;
            }
        }

        internal static void LateReset()
        {
            if (m_stopwatch.ElapsedMilliseconds % 1000 == 0)
            {
                m_losing = 0;
                m_receiving = 0;
            }
        }

        internal static void Slient(object message)
        {
            if (DebugSlient)
            {
                UnityEngine.Debug.Log(message);
            }
        }
    }
}
