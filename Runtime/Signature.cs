namespace Labyrinth.Runtime
{
    using Bolt;

    public struct Signature : ISynchronizer<byte>
    {
        public enum Rule
        {
            None,
            Round,
            Server,
            Authority
        }

        public Signature(byte value, int rate, Rule control, Relevance relevancy, Write sending, Read recieving)
        {
            Value = value;
            Rate = rate;
            Control = control;
            Relevancy = relevancy;
            Sending = sending;
            Recieving = recieving;
        }

        public byte Value { get; }
        public int Rate { get; }
        public Rule Control { get; }
        public Relevance Relevancy { get; }
        public Write Sending { get; }
        public Read Recieving { get; }
    }
}