using System;
using System.Collections.Generic;

namespace Labyrinth
{
    using Bolt;

    public struct Procedure : IRemote<short>
    {
        public Procedure(short value, Read callback)
        {
            Value = value;
            Callback = callback;
        }

        public short Value { get; }
        public Read Callback { get; }
    }
}