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
        public const byte Text = 9;
        public const byte Speech = 10;

        // register flags
        static Flags()
        {
            Network.Register(Signature, Instance.OnNetworkSignature);
            Network.Register(Procedure, Instance.OnNetworkProcedure);

            Network.Register(Create, Entity.OnNetworkCreate);
            Network.Register(Destroy, Entity.OnNetworkDestory);

            Network.Register(Text, Conference.OnNetworkText);
            Network.Register(Speech, Conference.OnNetworkSpeech);
        }
    }
}