using System;
using System.Collections.Generic;
using System.Text;

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
    }
}