namespace Labyrinth.Collections
{
    using Bolt;

    public struct Change
    {
        public Change(Step action, Write callback)
        {
            Operation = action;
            Callback = callback;
        }

        public Step Operation { get; }
        public Write Callback { get; }
    }
}