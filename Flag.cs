﻿namespace Labyrinth
{
    using Bolt;

    public struct Flag : IIdentifier<byte>
    {
        // primary in game (udp)
        public const byte Connected = 1;
        public const byte Disconnected = 2;
        public const byte Create = 5;
        public const byte Destroy = 6;
        public const byte Procedure = 7;
        public const byte Signature = 8;

        // primary chat rooms (tcp)
        public const byte Joint = 3;
        public const byte Left = 4;
        public const byte Voice = 9;
        public const byte Message = 10;

        public delegate void Recieved(int connection, object state, ref Reader reader);

        public Flag(byte value, Recieved callback)
        {
            Value = value;
            Callback = callback;
        }

        public byte Value { get; }
        public Recieved Callback { get; }
    }
}
