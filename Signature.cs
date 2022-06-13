namespace Labyrinth
{
    using Bolt;

    public struct Signature : ISynchronizer<short>
    {
        public Signature(short value, Rule control, bool relevance, Write sending, Read recieving)
        {
            Value = value;
            Control = control;
            Relevance = relevance;
            Sending = sending;
            Recieving = recieving;
        }

        public short Value { get; }
        public Rule Control { get; }
        public bool Relevance { get; }
        public Write Sending { get; }
        public Read Recieving { get; }
    }
}