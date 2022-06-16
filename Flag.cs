namespace Labyrinth
{
    using Bolt;

    public struct Flag : IIdentifier<byte>
    {
        public const byte Connected = 1;
        public const byte Disconnected = 2;

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
