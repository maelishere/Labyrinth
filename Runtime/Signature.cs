namespace Labyrinth.Runtime
{
    using Bolt;

    public struct Signature : ISynchronizer<byte>
    {
        public Signature(byte value, int rate, Rule control, bool relevance, Write sending, Read recieving)
        {
            Value = value;
            Rate = rate;
            Control = control;
            Relevance = relevance;
            Sending = sending;
            Recieving = recieving;
        }

        public byte Value { get; }
        public int Rate { get; }
        public Rule Control { get; }
        public bool Relevance { get; }
        public Write Sending { get; }
        public Read Recieving { get; }
    }
}