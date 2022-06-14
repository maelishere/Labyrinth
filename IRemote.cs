namespace Labyrinth
{
    using Bolt;

    public interface IRemote<T> : IIdentifier<T>
    {
        Read Callback { get; }
    }
}