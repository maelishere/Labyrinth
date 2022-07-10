using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Labyrinth
{
    using Bolt;
    using Labyrinth.Runtime;
    using Labyrinth.Collections;
    using Labyrinth.Background;

    public static class Network
    {
        public const byte Connected = byte.MinValue;
        public const byte Disconnected = byte.MaxValue;

        public delegate void Recieved(int socket, int connection, uint timestamp, ref Reader reader);

        private static int m_buffer = 1024;

        public static int Buffer
        {
            get => m_buffer;
            set
            {
                if (value > 1024)
                    m_buffer = 1024;
                else if (value < 255)
                    m_buffer = 255;
                else
                    m_buffer = value;
            }
        }

        private static readonly Dictionary<byte, Recieved> m_callbacks = new Dictionary<byte, Recieved>()
        {
            [Connected] = OnNetworkConnected,
            [Disconnected] = OnNetworkDisconnected,

            [Objects.Find] = Objects.OnNetworkFind,
            [Objects.Link] = Objects.OnNetworkLink,
            [Objects.Modify] = Objects.OnNetworkModify,
            [Objects.Ignore] = Objects.OnNetworkIgnore,

            [Flags.Procedure] = Instance.OnNetworkProcedure,
            [Flags.Signature] = Instance.OnNetworkSignature,
            [Flags.Loaded] = World.OnNetworkLoaded,
            [Flags.Offloaded] = World.OnNetworkOffloaded,
            [Flags.Create] = Entity.OnNetworkCreate,
            [Flags.Destroy] = Entity.OnNetworkDestory,
            [Flags.Ownership] = Entity.OnNetworkOwnership,
            [Flags.Transition] = Entity.OnNetworkTransition
        };

        public static bool Client => NetworkClient.Active;
        public static bool Server => NetworkServer.Active;
        public static bool Running => NetworkServer.Active || NetworkClient.Active;

        public static UnityEvent<int> initialized { get; } = new UnityEvent<int>();
        public static UnityEvent<int> terminating { get; } = new UnityEvent<int>();
        public static UnityEvent<int, int> connected { get; } = new UnityEvent<int, int>();
        public static UnityEvent<int, int> disconnected { get; } = new UnityEvent<int, int>();
        public static UnityEvent<int, int, int> pinged { get; } = new UnityEvent<int, int, int>();

        private static Write Pack(byte flag, Write write, out int size)
        {
            Writer other = new Writer(Buffer);
            other.Write(flag);
            // check if null not all messages need data
            write?.Invoke(ref other);
            size = 4 + other.Current; // int is 4 bytes
            return (ref Writer writer) =>
            {
                writer.Write(other.Current);
                writer.Write(other.ToSegment());
            };
        }

        internal static void Incoming(int socket, int connection)
        {
            if (NetworkServer.Active)
            {
                /// tell every client a new client connected
                Forward((c) => c != connection, Channels.Ordered,
                    Connected, (ref Writer writer) => writer.Write(connection));

                /// tell the new client about every current connection
                NetworkServer.Each((a) => a != connection,
                    (c) => Forward(connection, Channels.Ordered,
                    Connected, (ref Writer writer) => writer.Write(c)));

                Objects.Connected(connection);
            }

            /*if (NetworkClient.Active)
            {
            }*/

            connected.Invoke(socket, connection);
        }

        internal static void Outgoing(int socket, int connection)
        {
            if (NetworkServer.Active)
            {
                /// tell ever client someone disconnected
                Forward((c) => c != connection, Channels.Ordered,
                    Disconnected, (ref Writer writer) => writer.Write(connection));

                Objects.Disconnected(connection);
            }

            /*if (NetworkClient.Active)
            {
            }*/

            disconnected.Invoke(socket, connection);
        }

        internal static void Receive(int socket, int connection, uint timestamp, ref Reader reader)
        {
            try
            {
                int lenght = reader.ReadInt();
                Segment segment = reader.ReadSegment(lenght);

                Reader other = new Reader(segment);
                byte flag = other.Read();
                if (m_callbacks.ContainsKey(flag))
                {
                    m_callbacks[flag](socket, connection, timestamp, ref other);
                    return;
                }

                Debug.LogError($"Network received invalid flag [{flag}]");
            }
            catch(Exception e)
            {
                // messages are in batches
                //      exceptions wouldn't allow the
                //      remaining messages to be received
                Debug.LogError(e);
            }
        }

        // add a custom callback for a network flag
        public static bool Register(byte flag, Recieved callback)
        {
            if (!m_callbacks.ContainsKey(flag))
            {
                m_callbacks.Add(flag, callback);
                return true;
            }
            return false;
        }

        public static IPEndPoint Resolve(string host, int port)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length > 1)
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    if (addresses[i].AddressFamily == AddressFamily.InterNetwork)
                        return new IPEndPoint(addresses[i], port);
                }
            }
            throw new ArgumentException($"{host} host not found");
        }

        public static void Forward(byte channel, byte flag, Write write)
        {
            try
            {
                Write callback = Pack(flag, write, out int size);
                NetworkStream.Queue(channel, size, callback);
            }
            catch (Exception e)
            {
                // there could be an exception when writing in Pack();
                //      it shouldn't interupt the current frame
                //      so messages are sent
                Debug.LogError(e);
            }
        }

        public static void Send<T>(byte channel, byte flag, T value)
        {
            Forward(channel, flag,
                (ref Writer writer) =>
                {
                    writer.Write(value);
                });
        }

        public static void Forward(int connection, byte channel, byte flag, Write write)
        {
            try
            {
                Write callback = Pack(flag, write, out int size);
                NetworkStream.Queue(connection, channel, size, callback);
            }
            catch (Exception e)
            {
                // there could be an exception when writing in Pack();
                //      it shouldn't interupt the current frame
                //      so messages are sent
                Debug.LogError(e);
            }
        }

        public static void Send<T>(int connection, byte channel, byte flag, T value)
        {
            Forward(connection, channel, flag,
                (ref Writer writer) =>
                {
                    writer.Write(value);
                });
        }

        public static void Forward(Func<int, bool> predicate, byte channel, byte flag, Write write)
        {
            try
            {
                Write callback = Pack(flag, write, out int size);
                NetworkStream.Queue(predicate, channel, size, callback);
            }
            catch (Exception e)
            {
                // there could be an exception when writing in Pack();
                //      it shouldn't interupt the current frame
                //      so messages are sent
                Debug.LogError(e);
            }
        }

        public static void Send<T>(Func<int, bool> predicate, byte channel, byte flag, T value)
        {
            Forward(predicate, channel, flag,
                (ref Writer writer) =>
                {
                    writer.Write(value);
                });
        }

        public static int Authority(bool remote = false)
        {
            if (NetworkServer.Active)
                return NetworkServer.n_server.Listen;
            if (NetworkClient.Active)
            {
                if (remote)
                    return NetworkClient.n_client.Remote;
                else
                    return NetworkClient.n_client.Local;
            }
            return Identity.Any;
        }

        public static bool Authority(int connection, bool remote = false)
        {
            return connection == Identity.Any ? false : connection == Authority(remote);
        }

        // allows usage of custom struct or classes being send over the network
        public static void CustomWriter<T>(Extension.Generic<T>.Writing writing)
        {
            Extension.Generic<T>.SetWrite(writing);
        }

        // allows usage of custom struct or classes being recieved over the network
        public static void CustomReader<T>(Extension.Generic<T>.Reading reading)
        {
            Extension.Generic<T>.SetRead(reading);
        }

        /// from server [to client] about new connections
        private static void OnNetworkConnected(int socket, int connection, uint timestamp, ref Reader reader)
        {
            int client = reader.ReadInt();
            connected.Invoke(connection, client);
        }

        /// from server [to client] about diconnections
        private static void OnNetworkDisconnected(int socket, int connection, uint timestamp, ref Reader reader)
        {
            int client = reader.ReadInt();
            disconnected.Invoke(connection, client);
        }
    }
}