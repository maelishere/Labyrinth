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
            foreach(var batch in m_batches)
            {
                batch.Value[(Channel)channel].Enqueue(write);
            }
        }

        internal static void Queue(int connection, byte channel, Write write)
        {
            m_batches[connection][(Channel)channel].Enqueue(write);
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
            while(reader.Current < reader.Length - 1)
            {
                Network.Receive(socket, connection, timestamp, ref reader);
            }
        }

        internal static void Process()
        {
            foreach (var batch in m_batches)
            {
                Release(batch.Key, Channel.Ordered, batch.Value[Channel.Ordered]);
                Release(batch.Key, Channel.Irregular, batch.Value[Channel.Irregular]);
                Release(batch.Key, Channel.Direct, batch.Value[Channel.Direct]);
            }
        }

        private static void Release(int connection, Channel channel, Batch batch)
        {
            while (batch.Count > 0)
            {
                Send?.Invoke(connection, channel,
                    (ref Writer writer) =>
                    {
                        // check if there's enough space for the next write
                        // and make sure it's not an infinite loop
                        while ((writer.Length - writer.Current) < batch.Threshold && batch.Count > 0)
                        {
                            batch.Dequeue()(ref writer);
                        }
                    });
            }
        }
    }
}