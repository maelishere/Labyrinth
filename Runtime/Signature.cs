namespace Labyrinth.Runtime
{
    using Bolt;

    public struct Signature : ISynchronizer<byte>
    {
        public enum Rule
        {
            Round,
            Server,
            Authority
        }

        public Signature(byte value, int rate, Rule control, Relevancy relevancy, Write sending, Read recieving)
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
        public Relevancy Relevancy { get; }
        public Write Sending { get; }
        public Read Recieving { get; }

        public static bool Valid(int authority, Rule rule)
        {
            if (Network.Internal(Host.Server))
                return true;

            switch (rule)
            {
                case Rule.Round:
                case Rule.Authority:
                    return authority == Network.Authority(false);
            }

            // it would only down here if network isn't running
            return false;
        }
    }
}