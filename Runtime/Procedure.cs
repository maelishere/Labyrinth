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

            bool run = false;
            bool isTarget = target == Identity.Any || target == Network.Authority();

            if (Network.Internal(Host.Server))
            {
                switch (rule)
                {
                    case Rule.Any:
                    case Rule.Server:
                        run = isTarget;
                        break;
                }
            }

            if (Network.Internal(Host.Client))
            {
                switch (rule)
                {
                    case Rule.Any:
                    case Rule.Client:
                        run = isTarget;
                        break;
                }
            }

            return run;
        }
    }
}