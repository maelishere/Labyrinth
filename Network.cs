using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Labyrinth
{
    using Bolt;
    using Lattice.Delivery;
    using Labyrinth.Runtime;
    using Labyrinth.Stations;
    using Labyrinth.Collections;
    using Labyrinth.Background;

    public static class Network
    {
        private static readonly Dictionary<byte, Flag> m_callbacks = new Dictionary<byte, Flag>()
        {
            [Flag.Connected] = new Flag(Flag.Connected, OnNetworkConnected),
            [Flag.Disconnected] = new Flag(Flag.Disconnected, OnNetworkDisconnected),

            [Lobby.Update] = new Flag(Objects.Link, Lobby.OnNetworkUpdate),

            [Objects.Link] = new Flag(Objects.Link, Objects.OnNetworkLink),
            [Objects.Reset] = new Flag(Objects.Link, Objects.OnNetworkReset),
            [Objects.Modify] = new Flag(Objects.Modify, Objects.OnNetworkModify),

            [Flags.Procedure] = new Flag(Flags.Procedure, Instance.OnNetworkProcedure),
            [Flags.Signature] = new Flag(Flags.Signature, Instance.OnNetworkSignature),
            [Flags.Loaded] = new Flag(Flags.Loaded, World.OnNetworkLoaded),
            [Flags.Offloaded] = new Flag(Flags.Offloaded, World.OnNetworkOffloaded),
            [Flags.Create] = new Flag(Flags.Create, Entity.OnNetworkCreate),
            [Flags.Destroy] = new Flag(Flags.Destroy, Entity.OnNetworkDestory),
        };

        public static bool Running => NetworkServer.Active || NetworkClient.Active;

        public static UnityEvent<int> initialized { get; } = new UnityEvent<int>();
        public static UnityEvent<int> terminating { get; } = new UnityEvent<int>();
        public static UnityEvent<int, int> connected { get; } = new UnityEvent<int, int>();
        public static UnityEvent<int, int> disconnected { get; } = new UnityEvent<int, int>();
        public static UnityEvent<int, int, uint> pinged { get; } = new UnityEvent<int, int, uint>();

        private static Write Pack(byte flag, Write write)
        {
            return (ref Writer writer) =>
            {
                writer.Write(flag);

                try
                {
                    write?.Invoke(ref writer);
                }
                catch(Exception e)
                {
                    /// if there was an exception thrown when writing or reading, 
                    ///             it could affect the connection (the custom protcols)
                    Debug.LogWarning(e);
                }
            };
        }

        internal static void Incoming(int socket, int connection)
        {
            if (NetworkServer.Active)
            {
                /// tell every client a new client connected
                Forward((c) => c != connection, Channels.Ordered,
                    Flag.Connected, (ref Writer writer) => writer.Write(connection));

                /// tell the new client about every current connection
                NetworkServer.Each((a) => a != connection,
                    (c) => Forward(connection, Channels.Ordered,
                    Flag.Connected, (ref Writer writer) => writer.Write(c)));
            }

            if (NetworkClient.Active)
            {
                Lobby.Entered(connection);
            }

            connected.Invoke(socket, connection);
        }

        internal static void Outgoing(int socket, int connection)
        {
            if (NetworkServer.Active)
            {
                /// tell ever client someone disconnected
                Forward((c) => c != connection, Channels.Ordered,
                    Flag.Disconnected, (ref Writer writer) => writer.Write(connection));
            }

            if (NetworkClient.Active)
            {
                Lobby.Exited(connection);
            }

            disconnected.Invoke(socket, connection);
        }

        internal static void Receive(int socket, int connection, uint timestamp, ref Reader reader)
        {
            try
            {
                byte flag = reader.Read();
                /*Debug.Log($"Received {flag}");*/
                if (m_callbacks.ContainsKey(flag))
                {
                    m_callbacks[flag].Callback(socket, connection, timestamp, ref reader);
                    return;
                }
                Debug.LogError($"Network received invalid flag [{flag}]");
            }
            catch (Exception e)
            {
                /// if there was an exception thrown when writing or reading, 
                ///             it could affect the connection (the custom protcols)
                Debug.LogWarning(e);
            }
        }

        // add a custom callback for a network flag
        public static bool Register(byte flag, Flag.Recieved callback)
        {
            if (flag != Identity.Any & !m_callbacks.ContainsKey(flag))
            {
                m_callbacks.Add(flag, new Flag(flag, callback));
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
            if (NetworkServer.Active)
            {
                NetworkServer.Send((Channel)channel, Pack(flag, write));
                return;
            }
            if (NetworkClient.Active)
            {
                NetworkClient.Send((Channel)channel, Pack(flag, write));
            }
        }

        public static void Forward(int connection, byte channel, byte flag, Write write)
        {
            if (NetworkServer.Active)
            {
                NetworkServer.Send(connection, (Channel)channel, Pack(flag, write));
            }
        }

        public static void Forward(Func<int, bool> predicate, byte channel, byte flag, Write write)
        {
            if (NetworkServer.Active)
            {
                NetworkServer.Send(predicate, (Channel)channel, Pack(flag, write));
            }
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

        public static bool Internal(Host local)
        {
            switch (local)
            {
                default:
                case Host.Any: return Running;
                case Host.Client: return NetworkClient.Active;
                case Host.Server: return NetworkServer.Active;
            }
        }

        public static void CustomWriter<T>(Extension.Generic<T>.Writing writing)
        {
            Extension.Generic<T>.SetWrite(writing);
        }

        public static void CustomReader<T>(Extension.Generic<T>.Reading reading)
        {
            Extension.Generic<T>.SetRead(reading);
        }

        /// from server [to client] about new connections
        private static void OnNetworkConnected(int socket, int connection, uint timestamp, ref Reader reader)
        {
            int client = reader.ReadInt();

            Lobby.Entered(client);
            connected.Invoke(connection, client);
        }

        /// from server [to client] about diconnections
        private static void OnNetworkDisconnected(int socket, int connection, uint timestamp, ref Reader reader)
        {
            int client = reader.ReadInt();

            Lobby.Exited(client);
            disconnected.Invoke(connection, client);
        }
    }
}