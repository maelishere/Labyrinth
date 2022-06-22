namespace Labyrinth
{
    using Bolt;

    internal struct State
    {
        public int Socket;
        public int Connection;
        public uint Timestamp;
        public Reader Reader;

        public State(int socket, int connection, uint timestamp, Reader reader)
        {
            Socket = socket;
            Connection = connection;
            Timestamp = timestamp;
            Reader = reader;
        }
    }
}