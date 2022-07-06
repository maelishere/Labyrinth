using UnityEngine;

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;
    using Labyrinth.Background;

    [DisallowMultipleComponent]
    public abstract class Instance : MonoBehaviour
    {
        private class Container
        {
            public int Next = 0;
            public uint Last = 0;

            public Container()
            {
                Sync = false;
                Send = null;
                Wait = 0;
            }

            public Container(Action send, int wait)
            {
                Send = send;
                Wait = wait;
                Sync = true;
            }

            public Action Send { get; }
            public int Wait { get; }
            public bool Sync { get; }

            public void Post(int time)
            {
                Next = time + Wait;
            }
        }

        private static readonly Stopwatch m_stopwatch = new Stopwatch();
        private static readonly Dictionary<int, Instance> m_instances = new Dictionary<int, Instance>();

        private Appendix[] m_appendices;

        private readonly Dictionary<ushort, Container> m_synchronous = new Dictionary<ushort, Container>();
        private readonly Dictionary<ushort, Signature> m_signatures = new Dictionary<ushort, Signature>();
        private readonly Dictionary<ushort, Procedure> m_procedures = new Dictionary<ushort, Procedure>();


        public Identity identity { get; private set; }
        public Identity authority { get; private set; }

        protected virtual void Awake()
        {
            m_appendices = GetComponentsInChildren<Appendix>();
            for (int i = 0; i < m_appendices.Length; i++)
            {
                // offset 0 belongs to the class inheriting from instance (World or Entity)
                //      therefore we start at offseting at 1
                m_appendices[i].n_offset = (byte)(i + 1);
                m_appendices[i].n_network = this;
            }
        }

        protected virtual void Start()
        {
            Synchronous(authority.Value);
        }

        internal bool Create(int identifier, int connection)
        {
            if (!m_instances.ContainsKey(identifier))
            {
                identity = new Identity(identifier);
                authority = new Identity(connection);
                m_instances.Add(identifier, this);

                /*Debug.Log($"Created Instance[{identifier}] authority: Host({authority.Value})");*/
                return true;
            }
            return false;
        }

        private void Synchronous(int connection)
        {
            foreach (var signature in m_signatures)
            {
                Container container = null;
                if (Signature.Valid(connection, signature.Value.Control))
                {
                    Write message = (ref Writer writer) =>
                    {
                        writer.WriteSync(identity.Value, signature.Key);
                        signature.Value.Sending(ref writer);
                    };

                    // if callback is still null before add, something is wrong
                    Action callback = null;

                    switch (signature.Value.Control)
                    {
                        case Signature.Rule.Round:
                            if (Network.Internal(Host.Server))
                            {
                                // send to all relavant connection including authority
                                callback = () =>
                                {
                                    Central.Relavant(transform.position, signature.Value.Relevancy,
                                        (a) => true, (c) => Network.Forward(c, Channels.Direct, Flags.Signature, message));
                                };
                            }
                            if (Network.Internal(Host.Client))
                            {
                                // [Client] send to server
                                callback = () =>
                                {
                                    Network.Forward(Channels.Direct, Flags.Signature, message);
                                };
                            }
                            break;
                        case Signature.Rule.Server:
                            if (Network.Internal(Host.Server))
                            {
                                // send to all relavant connection overriding authority
                                callback = () =>
                                {
                                    Central.Relavant(transform.position, signature.Value.Relevancy,
                                        (a) => true, (c) => Network.Forward(c, Channels.Direct, Flags.Signature, message));
                                };
                            }
                            break;
                        case Signature.Rule.Authority:
                            if (Network.Internal(Host.Server))
                            {
                                // send to all relavant connection excluding authority
                                callback = () =>
                                {
                                    Central.Relavant(transform.position, signature.Value.Relevancy,
                                        (a) => a != authority.Value, (c) => Network.Forward(c, Channels.Direct, Flags.Signature, message));
                                };
                            }
                            if (Network.Internal(Host.Client))
                            {
                                // [Client] send to server
                                callback = () =>
                                {
                                    Network.Forward(Channels.Direct, Flags.Signature, message);
                                };
                            }
                            break;
                    }
                    container = new Container(callback, (1000 / signature.Value.Rate));
                }
                m_synchronous.Add(signature.Key, container ?? new Container());
            }
        }

        internal bool Destroy()
        {
            m_synchronous.Clear();
            return m_instances.Remove(identity.Value);
        }

        internal bool Register(byte offset, Signature signature)
        {
            // combine (Extension) in the event two components have the same signature value
            //      or the instances of the same class are on the gameobject
            ushort key = offset.Combine(signature.Value);
            if (!m_signatures.ContainsKey(key))
            {
                m_signatures.Add(key, signature);
                return true;
            }
            return false;
        }

        internal bool Register(byte offset, Procedure procedure)
        {
            // combine (Extension) in the event two components have the same procedure value
            //      or the instances of the same class are on the gameobject
            ushort key = offset.Combine(procedure.Value);
            if (!m_procedures.ContainsKey(key))
            {
                m_procedures.Add(key, procedure);
                return true;
            }
            return false;
        }

        internal void Remote(int target, byte channel, byte offset, byte procedure, Write write)
        {
            if (target == Network.Authority())
            {
                UnityEngine.Debug.LogWarning($"Procedure call target is self");
                return;
            }

            ushort call = offset.Combine(procedure);
            if (target == Identity.Any || Network.Internal(Host.Client))
            {
                Network.Forward(
                    channel,
                    Flags.Procedure,
                    (ref Writer writer) =>
                    {
                        writer.WriteCall(target, identity.Value, call);
                        write(ref writer);
                    });
            }
            else if (Network.Internal(Host.Server))
            {
                /// make sure receivers are relevant
                Central.Relavant(transform.position, m_procedures[call].Relevancy,
                    (a) => true, (c) => Network.Forward(c, channel, Flags.Procedure,
                    (ref Writer writer) =>
                    {
                        writer.WriteCall(target, identity.Value, call);
                        write(ref writer);
                    }));
            }
        }

        private static void NetworkStartup(int socket)
        {
            m_stopwatch.Reset();
            m_stopwatch.Start();
        }

        private static void NetworkReset(int socket)
        {
            m_stopwatch.Stop();
            m_instances.Clear();
        }

        private static void NetworkUpdate()
        {
            int time = (int)m_stopwatch.ElapsedMilliseconds;
            foreach (var instance in m_instances)
            {
                foreach (var signature in instance.Value.m_synchronous)
                {
                    if (signature.Value.Sync)
                    {
                        if (time >= signature.Value.Next)
                        {
                            // if send is null, something is wrong
                            signature.Value.Send();
                            signature.Value.Post(time);
                        }
                    }
                }
            }
        }

        internal static void OnNetworkProcedure(int socket, int connection, uint timestamp, ref Reader reader)
        {
            Packets.Call call = reader.ReadCall();
            /*Debug.Log($"Received Call({call.Procedure}) [Target -> Host({call.Target})] for Instance({call.Identity})");*/
            if (m_instances.ContainsKey(call.Identity))
            {
                Instance instance = m_instances[call.Identity];
                /*Debug.Log($"Found Instance({call.Identity})");*/
                if (instance.m_procedures.ContainsKey(call.Procedure))
                {
                    /*Debug.Log($"Found Call({call.Procedure})");*/
                    if (Network.Internal(Host.Server))
                    {
                        byte[] parameters = reader.Peek(reader.Length - reader.Current);
                        // if the target is any connection, forward to the other clients
                        if (call.Target == Identity.Any)
                        {
                            /// make sure receivers are relevant excluding who sent it
                            Central.Relavant(instance.transform.position, instance.m_procedures[call.Procedure].Relevancy,
                                (a) => a != connection, (c) => Network.Forward(c, Channels.Irregular, Flags.Procedure,
                                (ref Writer writer) =>
                                {
                                    writer.WriteCall(call);
                                    // write the parameters without reading the buffer
                                    writer.Write(parameters);
                                }));
                        }
                        // if the server isn't the target, forward to the target client
                        else if (call.Target != Network.Authority())
                        {
                            /// make sure target is relevant
                            if (Central.Relevant(call.Target, instance.transform.position, instance.m_procedures[call.Procedure].Relevancy))
                            {
                                Network.Forward(call.Target, Channels.Irregular, Flags.Procedure,
                                (ref Writer writer) =>
                                {
                                    writer.WriteCall(call);
                                    // write the parameters without reading the buffer
                                    writer.Write(parameters);
                                });
                            }
                            // exit since server isn't the target
                            return;
                        }
                    }

                    if (Procedure.Valid(call.Target, instance.m_procedures[call.Procedure].Control))
                    {
                        instance.m_procedures[call.Procedure].Callback(ref reader);
                    }
                }
            }
        }

        internal static void OnNetworkSignature(int socket, int connection, uint timestamp, ref Reader reader)
        {
            Packets.Sync sync = reader.ReadSync();
            /*Debug.Log($"Receiving Sync({sync.Signature}) from Host({connection})");*/
            if (m_instances.ContainsKey(sync.Identity))
            {
                Instance instance = m_instances[sync.Identity];
                /*Debug.Log($"Found Instance({sync.Identity})");*/
                if (instance.m_signatures.ContainsKey(sync.Signature) && instance.m_synchronous.ContainsKey(sync.Signature))
                {
                    /*Debug.Log($"Found Sync({sync.Signature})");*/

                    // fliter out older packets
                    if (timestamp >= instance.m_synchronous[sync.Signature].Last)
                    {
                        instance.m_signatures[sync.Signature].Recieving(ref reader);
                        instance.m_synchronous[sync.Signature].Last = timestamp;
                    }
                }
            }
        }

        public static bool Find<T>(int identity, out T instance) where T : Instance
        {
            if (m_instances.TryGetValue(identity, out Instance value))
            {
                if (value as T)
                {
                    instance = (T)value;
                    return true;
                }
            }
            instance = null;
            return false;
        }

        public static void Find<T>(Action<T> callback) where T : Instance
        {
            foreach (var instance in m_instances)
            {
                if (instance.Value as T)
                {
                    callback((T)instance.Value);
                }
            }
        }

        public static T[] Find<T>() where T : Instance
        {
            List<T> instances = new List<T>();
            foreach (var instance in m_instances)
            {
                if (instance.Value as T)
                {
                    instances.Add((T)instance.Value);
                }
            }
            return instances.ToArray();
        }

        public static Identity Unique()
        {
            return Identity.Generate(
                (int value) =>
                {
                    return m_instances.ContainsKey(value) && (Central.n_instance?.NetworkScene(value) ?? true);
                });
        }

        static Instance()
        {
            NetworkLoop.LateUpdate += NetworkUpdate;
            Network.initialized.AddListener(NetworkStartup);
            Network.terminating.AddListener(NetworkReset);
        }
    }
}