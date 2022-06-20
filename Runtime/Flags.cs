using UnityEngine;

namespace Labyrinth.Runtime
{
    public static class Flags
    {
        public const byte Procedure = 3;
        public const byte Signature = 4;
        public const byte Loaded = 5;
        public const byte Offloaded = 6;
        public const byte Create = 7;
        public const byte Destroy = 8;

        // register flags
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Network.Register(Signature, Instance.OnNetworkSignature);
            Network.Register(Procedure, Instance.OnNetworkProcedure);

            Network.Register(Loaded, World.OnNetworkLoaded);
            Network.Register(Offloaded, World.OnNetworkOffloaded);

            Network.Register(Create, Entity.OnNetworkCreate);
            Network.Register(Destroy, Entity.OnNetworkDestory);
        }
    }
}