using System;
using System.Collections.Generic;

namespace Labyrinth.Background
{
    using Bolt;
    using Lattice.Delivery;

    public readonly struct Packet
    {
        public Packet(int size, Write send)
        {
            Size = size;
            Send = send;
        }

        public int Size { get; }
        public Write Send { get; }
    }

    public class Batch : Dictionary<Channel, Queue<Packet>>
    {
        public Batch()
        {
            this[Channel.Ordered] = new Queue<Packet>();
            this[Channel.Irregular] = new Queue<Packet>();
            this[Channel.Direct] = new Queue<Packet>();
        }
    }

    public static class NetworkStream
    {
        internal static Action<int, Channel, Write> Send { get; set; }

        private static readonly Dictionary<int, Batch> m_batches = new Dictionary<int, Batch>();

        internal static void Incoming(int connection)
        {
            m_batches.Add(connection, new Batch());
        }

        internal static void Outgoing(int connection)
        {
            m_batches.Remove(connection);
        }

        internal static void Clear()
        {
            Send = null; 
            m_batches.Clear();
        }

        internal static void Queue(byte channel, int size, Write write)
        {
            foreach(var connection in m_batches)
            {
                connection.Value[(Channel)channel].Enqueue(new Packet(size, write));
            }
        }

        internal static void Queue(int connection, byte channel, int size, Write write)
        {
            if (m_batches.ContainsKey(connection))
            {
                m_batches[connection][(Channel)channel].Enqueue(new Packet(size, write));
            }
        }

        internal static void Queue(Func<int, bool> predicate, byte channel, int size, Write write)
        {
            foreach (var batch in m_batches)
            {
                if (predicate(batch.Key))
                {
                    batch.Value[(Channel)channel].Enqueue(new Packet(size, write));
                }
            }
        }

        internal static void Receive(int socket, int connection, uint timestamp, ref Reader reader)
        {
            do
            {
                Network.Receive(socket, connection, timestamp, ref reader);
            } while (reader.Current < reader.Length - 1);
        }

        internal static void Process()
        {
            if (Send != null)
            {
                foreach (var connection in m_batches)
                {
                    Release(connection.Key, Channel.Ordered);
                    Release(connection.Key, Channel.Irregular);
                    Release(connection.Key, Channel.Direct);
                }
            }
        }

        private static void Release(int connection, Channel channel)
        {
            Packet packet;
            if (m_batches[connection][channel].Count > 0)
            {
                packet = m_batches[connection][channel].Dequeue();
                do
                {
                    Send(connection, channel,
                        (ref Writer writer) =>
                        {
                            // check if there's enough space for the next packet
                            while (packet.Size <= (writer.Length - writer.Current))
                            {
                                packet.Send(ref writer);

                                // check if that's all
                                if (m_batches[connection][channel].Count == 0)
                                {
                                    break;
                                }

                                packet = m_batches[connection][channel].Dequeue();
                            }
                        });
                } while (m_batches[connection][channel].Count > 0);
            }
        }
    }
}