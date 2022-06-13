using System;
using System.Collections.Generic;
using System.Text;

namespace Labyrinth
{
    using Bolt;

    public struct Flag : IRemote<byte>
    {
        public byte Value { get; }
        public Read Callback { get; }

    }
}
