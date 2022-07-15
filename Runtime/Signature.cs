namespace Labyrinth.Runtime
{
    using Bolt;
    using Labyrinth.Background;

    public struct Signature
    {
        public enum Rule
        {
            Round,
            Server,
            Authority
        }

        public Signature(byte value, float rate, Rule control, Relevancy relevancy, Write sending, Read recieving)
        {
            Value = value;
            Rate = rate;
            Control = control;
            Relevancy = relevancy;
            Sending = sending;
            Recieving = recieving;
        }

        public byte Value { get; }
        public float Rate { get; }
        public Rule Control { get; }
        public Relevancy Relevancy { get; }
        public Write Sending { get; }
        public Read Recieving { get; }

        public static bool Valid(int authority, Rule rule)
        {
            if (NetworkServer.Active)
                return true;

            switch (rule)
            {
                case Rule.Round:
                case Rule.Authority:
                    return authority == Network.Authority(false);
            }

            return false;
        }
    }
}