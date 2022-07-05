namespace Labyrinth.Collections
{
    using Bolt;

    public struct Change
    {
        public Change(uint step, Action action, Write callback)
        {
            Operation = new Operation(step, action);
            Callback = callback;
        }

        public Change(Operation change, Write callback)
        {
            Operation = change;
            Callback = callback;
        }

        public Operation Operation { get; }
        public Write Callback { get; }
    }
}