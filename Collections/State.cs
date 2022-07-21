namespace Labyrinth.Collections
{
    using Bolt;

    public struct State
    {
        public State(Step action, Write callback)
        {
            Operation = action;
            Callback = callback;
        }

        public Step Operation { get; }
        public Write Callback { get; }
    }
}