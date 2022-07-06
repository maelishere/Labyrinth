namespace Labyrinth
{
    using Bolt;

    /// please don't mistake this for a bit mask flags
    /// sometimes you should point out the obvious
    public struct Flag : IIdentifier<byte>
    {
        public const byte Connected = 1;
        public const byte Disconnected = 2;

        public delegate void Recieved(int socket, int connection, uint timestamp, ref Reader reader);

        public Flag(byte value, Recieved callback)
        {
            Value = value;
            Callback = callback;
        }

        public byte Value { get; }
        public Recieved Callback { get; }
    }
}
