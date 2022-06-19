using UnityEngine;

namespace Labyrinth.Runtime
{
    public static class Flags
    {
        public const byte Create = 3;
        public const byte Destroy = 4;
        public const byte Procedure = 5;
        public const byte Signature = 6;

        // register flags
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Network.Register(Create, Central.OnNetworkCreate);
            Network.Register(Destroy, Central.OnNetworkDestory);

            Network.Register(Signature, Instance.OnNetworkSignature);
            Network.Register(Procedure, Instance.OnNetworkProcedure);
        }
    }
}