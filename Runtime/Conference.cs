using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;
    using System;

    // represenation of a session
    public sealed class Conference : MonoBehaviour
    {
        internal static void OnNetworkText(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        internal static void OnNetworkSpeech(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }
    }
}