using UnityEngine;

namespace Labyrinth.Runtime
{
    public static class Flags
    {
        // primary in game (udp)
        public const byte Procedure = 3;
        public const byte Signature = 4;
        public const byte Create = 5;
        public const byte Destroy = 6;

        // secondary chat rooms (tcp)
        public const byte Message = 9;

        static Flags()
        {
            // register flags
            Network.Register(Signature, Instance.OnNetworkSignature);
            Network.Register(Procedure, Instance.OnNetworkProcedure);

            Network.Register(Create, Entity.OnNetworkCreate);
            Network.Register(Destroy, Entity.OnNetworkDestory);

            Network.Register(Message, Conference.OnNetworkMessage);
        }
    }
}