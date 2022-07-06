using System;
using System.Collections.Generic;

namespace Labyrinth.Stations
{
    using Bolt;

    // state machine mamanger of current session
    //      [important] the current aivities/states in game/evironment
    public static class Session
    {
        public const byte Update = 12;

        private static readonly Dictionary<int, IMachine> m_statemachines = new Dictionary<int, IMachine>();

        internal static void Entered(int id)
        {

        }

        internal static void Exited(int id)
        {

        }

        internal static void OnNetworkUpdate(int socket, int connection, uint timestamp, ref Reader reader)
        {
            throw new NotImplementedException();
        }
    }
}