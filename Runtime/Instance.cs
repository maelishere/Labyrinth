using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;
    using Labyrinth.Background;

    [DisallowMultipleComponent]
    public class Instance : MonoBehaviour
    {
        private static Dictionary<int, Instance> m_instances = new Dictionary<int, Instance>();

        private Appendix[] m_appendices;

        private readonly Dictionary<short, uint> m_synced = new Dictionary<short, uint>();
        private readonly Dictionary<short, Signature> m_signatures = new Dictionary<short, Signature>();
        private readonly Dictionary<short, Procedure> m_procedures = new Dictionary<short, Procedure>();

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

        internal bool Destroy()
        {
            return m_instances.Remove(identity.Value);
        }

        internal bool Register(byte offset, Signature signature)
        {
            // combine (Extension) in the event two components have the same signature value
            //      or the instances of the same class are on the gameobject
            short key = offset.Combine(signature.Value);
            if (!m_signatures.ContainsKey(key))
            {
                m_signatures.Add(key, signature);
                StartCoroutine(Synchronize(key));
                return true;
            }
            return false;
        }

        internal bool Register(byte offset, Procedure procedure)
        {
            // combine (Extension) in the event two components have the same procedure value
            //      or the instances of the same class are on the gameobject
            short key = offset.Combine(procedure.Value);
            if (!m_procedures.ContainsKey(key))
            {
                m_procedures.Add(key, procedure);
                return true;
            }
            return false;
        }

        internal IEnumerator Synchronize(short signature)
        {
            /*Debug.Log($"Syncing ({signature}) for Instance({identity.Value})");*/

            // we wait till create is called (incase)
            while(identity.Value == Identity.Any || authority.Value == Identity.Any)
            {
                yield return null;
            }

            Write write = (ref Writer writer) =>
            {
                writer.WriteSync(identity.Value, signature);
                m_signatures[signature].Sending(ref writer);
            };

            // with reference signature rule
            //              client to server 
            //              server to clients

            float wait = 1.0f / m_signatures[signature].Rate;
            while (Network.Running)
            {
                switch (m_signatures[signature].Control)
                {
                    case Signature.Rule.Round:
                        if (Network.Internal(Host.Server))
                        {
                            // send to all relavant connection including authority
                            Central.Relavant(transform.position, m_signatures[signature].Relevancy,
                                (a) => true, (c) => Network.Forward(c, Channels.Direct, Flags.Signature, write));
                        }
                        if (Network.Internal(Host.Client) && Network.Authority(authority.Value))
                        {
                            // [Client] send to server
                            Network.Forward(Channels.Direct, Flags.Signature, write);
                        }
                        break;
                    case Signature.Rule.Server:
                        if (Network.Internal(Host.Server))
                        {
                            // send to all relavant connection overriding authority
                            Central.Relavant(transform.position, m_signatures[signature].Relevancy,
                                (a) => true, (c) => Network.Forward(c, Channels.Direct, Flags.Signature, write));
                        }
                        break;
                    case Signature.Rule.Authority:
                        if (Network.Internal(Host.Server))
                        {
                            // send to all relavant connection excluding authority
                            Central.Relavant(transform.position, m_signatures[signature].Relevancy,
                                (a) => a != authority.Value, (c) => Network.Forward(c, Channels.Direct, Flags.Signature, write));
                        }
                        if (Network.Internal(Host.Client) && Network.Authority(authority.Value))
                        {
                            // [Client] send to server
                            Network.Forward(Channels.Direct, Flags.Signature, write);
                        }
                        break;
                }
                yield return new WaitForSecondsRealtime(wait);
            }
        }

        internal void Remote(int target, byte channel, byte offset, byte procedure, Write write)
        {
            if (target == Network.Authority())
            {
                Debug.LogWarning($"Procedure call target is self");
                return;
            }

            if (target == Identity.Any || Network.Internal(Host.Client))
            {
                Network.Forward(
                    channel,
                    Flags.Procedure,
                    (ref Writer writer) =>
                    {
                        writer.WriteCall(target, identity.Value, offset.Combine(procedure));
                        write(ref writer);
                    });
            }
            else if (Network.Internal(Host.Server))
            {
                /// make sure receivers are relevant
                Central.Relavant(transform.position, m_procedures[offset.Combine(procedure)].Relevancy,
                    (a) => true, (c) => Network.Forward(c, channel, Flags.Procedure,
                    (ref Writer writer) =>
                    {
                        writer.WriteCall(target, identity.Value, offset.Combine(procedure));
                        write(ref writer);
                    }));
            }
        }

        public static Identity Unique()
        {
            return Identity.Generate(
                (int value) =>
                {
                    return m_instances.ContainsKey(value) && (Central.n_instance?.NetworkScene(value) ?? true);
                });
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
                        Reader reference = reader;
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
                                    writer.Write(reference.Peek(reference.Length - reference.Current));
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
                                    writer.Write(reference.Peek(reference.Length - reference.Current));
                                });
                            }
                            // exit since server isn't the target
                            return;
                        }
                    }

                    /// before i can call the procedure, check:
                    /// if the network is a client or server
                    /// if this prodecure can run on client or server or both
                    /// if the network is the target

                    bool run = false;
                    bool target = call.Target == Identity.Any || call.Target == Network.Authority();

                    if (Network.Internal(Host.Server))
                    {
                        switch (instance.m_procedures[call.Procedure].Control)
                        {
                            case Procedure.Rule.Any:
                            case Procedure.Rule.Server:
                                run = target;
                                break;
                        }
                    }

                    if (Network.Internal(Host.Client))
                    {
                        switch (instance.m_procedures[call.Procedure].Control)
                        {
                            case Procedure.Rule.Any:
                            case Procedure.Rule.Client:
                                run = target;
                                break;
                        }
                    }

                    if (run) instance.m_procedures[call.Procedure].Callback(ref reader);
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
                if (instance.m_signatures.ContainsKey(sync.Signature))
                {
                    /*Debug.Log($"Found Sync({sync.Signature})");*/

                    if (!instance.m_synced.ContainsKey(sync.Signature))
                    {
                        instance.m_synced.Add(sync.Signature, timestamp);
                    }

                    // fliter out duplicate or older packets
                    if (timestamp >= instance.m_synced[sync.Signature])
                    {
                        instance.m_signatures[sync.Signature].Recieving(ref reader);
                    }

                    instance.m_synced[sync.Signature] = timestamp;
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
    }
}