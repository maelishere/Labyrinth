namespace Labyrinth.Runtime
{
    using Bolt;

    public struct Procedure : IRemote<byte>
    {
        public enum Rule
        {
            Any,
            Server,
            Client
        }

        public Procedure(byte value, Rule control, Read callback)
        {
            Value = value;
            Control = control;
            Callback = callback;
        }
        public byte Value { get; }
        public Rule Control { get; }
        public Read Callback { get; }
    }
}