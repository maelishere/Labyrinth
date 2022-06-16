using UnityEngine;
using System.Collections.Generic;
using Bolt;
using System;

namespace Labyrinth.Runtime
{
    public static class Central
    {
        static Central()
        {
            // register flags
            Network.Register(Flags.Signature, Instance.OnNetworkSignature);
            Network.Register(Flags.Procedure, Instance.OnNetworkProcedure);

            Network.Register(Flags.Create, Entity.OnNetworkCreate);
            Network.Register(Flags.Destroy, Entity.OnNetworkDestory);

            Network.Register(Flags.Message, Conference.OnNetworkMessage);
        }
    }
}