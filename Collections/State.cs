namespace Labyrinth.Collections
{
    using Bolt;

    public struct Change
    {
        public Change(Action action, Write callback)
        {
            Operation = action;
            Callback = callback;
        }

        public Action Operation { get; }
        public Write Callback { get; }
    }
}