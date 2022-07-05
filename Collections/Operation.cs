namespace Labyrinth.Collections
{
    public struct Operation
    {
        public Operation(uint step, Action action)
        {
            Step = step;
            Action = action;
        }

        // order of changes
        public uint Step { get; }
        public Action Action { get; }
    }
}