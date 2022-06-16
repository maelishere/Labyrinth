namespace Labyrinth.Runtime
{
    using Bolt;

    public struct Procedure : IRemote<byte>
    {
        public Procedure(byte value, Read callback)
        {
            Value = value;
            Callback = callback;
        }

        public byte Value { get; }
        public Read Callback { get; }
    }
}