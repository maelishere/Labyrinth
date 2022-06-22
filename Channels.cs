namespace Labyrinth
{
    using Lattice.Delivery;

    public class Channels
    {
        // so you don't need to use "using Lattice.Delivery" every time you need to send
        public const byte Direct = (byte)Channel.Direct; // not guaranteed
        public const byte Irregular = (byte)Channel.Irregular; // not guaranteed but not in order
        public const byte Ordered = (byte)Channel.Ordered; // not guaranteed in order
    }
}