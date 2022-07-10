namespace Labyrinth.Runtime
{
    using Bolt;
    using Labyrinth.Background;

    public struct Procedure
    {
        public enum Rule
        {
            Any,
            Server,
            Client
        }

        public Procedure(byte value, Rule control, Relevancy relevancy, Read callback)
        {
            Value = value;
            Control = control;
            Relevancy = relevancy;
            Callback = callback;
        }

        public byte Value { get; }
        public Rule Control { get; }
        public Relevancy Relevancy { get; }
        public Read Callback { get; }

        public static bool Valid(int target, Rule rule)
        {
            /// before i can call the procedure, check:
            /// if the network is a client or server
            /// if this prodecure can run on client or server or both
            /// if the network is the target

            bool isTarget = target == Identity.Any || target == Network.Authority();

            if (NetworkServer.Active)
            {
                switch (rule)
                {
                    case Rule.Any:
                    case Rule.Server:
                        return isTarget;
                }
            }

            if (NetworkClient.Active)
            {
                switch (rule)
                {
                    case Rule.Any:
                    case Rule.Client:
                        return isTarget;
                }
            }

            return false;
        }
    }
}