namespace Labyrinth
{
    using Bolt;

    public struct Signature : ISynchronizer<byte>
    {
        public Signature(byte value, int sync, Rule control, bool relevance, Write sending, Read recieving)
        {
            Value = value;
            Sync = sync;
            Control = control;
            Relevance = relevance;
            Sending = sending;
            Recieving = recieving;
        }

        public byte Value { get; }
        public int Sync { get; }
        public Rule Control { get; }
        public bool Relevance { get; }
        public Write Sending { get; }
        public Read Recieving { get; }
    }
}