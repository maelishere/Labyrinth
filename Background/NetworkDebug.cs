using UnityEngine;

namespace Labyrinth.Background
{
    using Lattice;

    public static class NetworkDebug
    {
        private static int m_losing;

        public static int PacketLoss => m_losing;

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
        }

        internal static void Reset()
        {
            m_losing = 0;
        }
    }
}
