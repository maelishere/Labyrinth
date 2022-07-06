using System;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice.Delivery;

    public class Batch : Queue<Writer>
    {
        private Writer m_current;

        public Batch()
        {
            m_current = new Writer(1024);
        }

        public void Queue()
        {
            if (m_current.Current > 0)
            {
                Enqueue(m_current);
                m_current = new Writer(1024);
            }
        }

        public void Queue(int threshold, Write write)
        {
            if  (m_current.Current > m_current.Length - threshold - 1)
            {
                Queue();
            }

            write?.Invoke(ref m_current);
        }
    }

    public class Batcher : Dictionary<Channel, Batch>
    {
        public Batcher()
        {
            this[Channel.Ordered] = new Batch();
            this[Channel.Irregular] = new Batch();
            this[Channel.Direct] = new Batch();
        }

        public void Queue()
        {
            this[Channel.Ordered].Queue();
            this[Channel.Irregular].Queue();
            this[Channel.Direct].Queue();
        }
    }

    public static class NetworkStream
    {
        internal static Action<int, Channel, Writer> Send { get; set; }

        private static readonly Dictionary<int, Batcher> m_batches = new Dictionary<int, Batcher>();

        internal static void Incoming(int connection)
        {
            m_batches.Add(connection, new Batcher());
        }

        internal static void Outgoing(int connection)
        {
            m_batches.Remove(connection);
        }

        internal static void Queue(byte channel, int threshold, Write write)
        {
            foreach(var batch in m_batches)
            {
                batch.Value[(Channel)channel].Queue(threshold, write);
            }
        }

        internal static void Queue(int connection, byte channel, int threshold, Write write)
        {
            m_batches[connection][(Channel)channel].Queue(threshold, write);
        }

        internal static void Queue(Func<int, bool> predicate, byte channel, int threshold, Write write)
        {
            foreach (var batch in m_batches)
            {
                if (predicate(batch.Key))
                {
                    batch.Value[(Channel)channel].Queue(threshold, write);
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
                batch.Value.Queue();
                Release(batch.Key, Channel.Ordered, batch.Value[Channel.Ordered]);
                Release(batch.Key, Channel.Irregular, batch.Value[Channel.Irregular]);
                Release(batch.Key, Channel.Direct, batch.Value[Channel.Direct]);
            }
        }

        private static void Release(int connection, Channel channel, Batch batch)
        {
            while (batch.Count > 0)
            {
                Send?.Invoke(connection, channel, batch.Dequeue());
            }
        }
    }
}