using System;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice.Delivery;

    public class Batch : Queue<Write>
    {
        public int Threshold { get; }

        public Batch(int threshold)
        {
            Threshold = threshold;
        }
    }

    public class Batcher : Dictionary<Channel, Batch>
    {
        public Batcher()
        { 
            // always make sure that there's at least 200 bytes
            // (might change later really depends)
            this[Channel.Ordered] = new Batch(200);
            this[Channel.Irregular] = new Batch(200);
            this[Channel.Direct] = new Batch(200);
        }
    }

    public static class NetworkStream
    {
        internal static Action<int, Channel, Write> Send { get; set; }

        private static readonly Dictionary<int, Batcher> m_batches = new Dictionary<int, Batcher>();

        internal static void Incoming(int connection)
        {
            m_batches.Add(connection, new Batcher());
        }

        internal static void Outgoing(int connection)
        {
            m_batches.Remove(connection);
        }

        internal static void Queue(byte channel, Write write)
        {
            foreach(var connection in m_batches)
            {
                connection.Value[(Channel)channel].Enqueue(write);
            }
        }

        internal static void Queue(int connection, byte channel, Write write)
        {
            if (m_batches.ContainsKey(connection))
            {
                m_batches[connection][(Channel)channel].Enqueue(write);
            }
        }

        internal static void Queue(Func<int, bool> predicate, byte channel, Write write)
        {
            foreach (var batch in m_batches)
            {
                if (predicate(batch.Key))
                {
                    batch.Value[(Channel)channel].Enqueue(write);
                }
            }
        }

        internal static void Receive(int socket, int connection, uint timestamp, ref Reader reader)
        {
            do
            {
                Network.Receive(socket, connection, timestamp, ref reader);
                // i need to remove what's left in the reader (for the next flag) beacuse the messages are packed
                // this causes invalid flag or the wrong callback being called
            } while (reader.Current < reader.Length - 1);
        }

        internal static void Process()
        {
            foreach (var connection in m_batches)
            {
                Release(connection.Key, Channel.Ordered, connection.Value[Channel.Ordered]);
                Release(connection.Key, Channel.Irregular, connection.Value[Channel.Irregular]);
                Release(connection.Key, Channel.Direct, connection.Value[Channel.Direct]);
            }
        }

        private static void Release(int connection, Channel channel, Batch batch)
        {
            while (Send != null & batch.Count > 0)
            {
                Send(connection, channel,
                    (ref Writer writer) =>
                    {
                        do
                        {
                            Write write = batch.Dequeue();
                            write(ref writer);

                            // check if that's all
                            if (batch.Count == 0)
                            {
                                break;
                            }

                            // check if there's enough space for the next write
                        } while (batch.Threshold <= (writer.Length - writer.Current));
                    });
            }
        }
    }
}