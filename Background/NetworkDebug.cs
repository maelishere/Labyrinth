using UnityEngine;

namespace Labyrinth.Background
{
    using Lattice;

    // i know this needs work
    public static class NetworkDebug
    {
        private static int m_losing;
        private static int m_sending;
        private static int m_receiving;

        // How many packets were lost relative to the previous frame
        public static int Loss => m_losing;
        // How many bytes were sent during the current frame
        public static int Sent => m_sending;
        // How many byte were received during the current frame
        public static int Received => m_receiving;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Log.POut = Debug.Log;
            Log.WOut = Debug.LogWarning;
            Log.EOut = Debug.LogError;

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
        }

        internal static void EarlyReset()
        {
            m_sending = 0;
        }

        internal static void LateReset()
        {
            m_losing = 0;
            m_receiving = 0;
        }
    }
}
