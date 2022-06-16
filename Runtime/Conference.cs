using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;
    using System;

    public sealed class Conference : MonoBehaviour
    {
        private readonly Dictionary<int, Conference> m_conferences = new Dictionary<int, Conference>();

        internal static void OnNetworkMessage(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        internal static void HandleMessage(int session, int connection, byte type, ref Reader reader)
        {

        }
    }
}