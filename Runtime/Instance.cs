using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    [DisallowMultipleComponent]
    public class Instance : MonoBehaviour
    {
        private static Dictionary<int, Instance> m_instances = new Dictionary<int, Instance>();

        private Appendix[] m_appendices;

        private readonly Dictionary<short, Signature> m_signatures = new Dictionary<short, Signature>();
        private readonly Dictionary<short, Procedure> m_procedures = new Dictionary<short, Procedure>();

        public Identity identity { get; private set; }
        public Identity authority { get; private set; }

        internal bool Create(int identifier, int connection)
        {
            if (!m_instances.ContainsKey(identifier))
            {
                m_appendices = GetComponentsInChildren<Appendix>();
                for (int i = 0; i < m_appendices.Length; i++)
                {
                    m_appendices[i].n_offset = (byte)(i + 1); // offset 0 belongs to the class inheriting from instance
                    m_appendices[i].n_network = this;
                }

                identity = new Identity(identifier);
                authority = new Identity(connection);
                m_instances.Add(identifier, this);

                /// after start synchronizing signatures
                foreach (var signature in m_signatures)
                    StartCoroutine(Synchronize(signature.Key, signature.Value));

                return true;
            }
            return false;
        }

        internal bool Destroy()
        {
            /// before stop synchronizing signatures
            StopAllCoroutines();
            return m_instances.Remove(identity.Value);
        }

        internal bool Register(byte offset, Signature signature)
        {
            if (!m_signatures.ContainsKey(signature.Value))
            {
                // combine (Extension) in the event two components have the same signature value
                //      or the instances of the same class are on the gameobject
                m_signatures.Add(offset.Combine(signature.Value), signature);
                return true;
            }
            return false;
        }

        internal bool Register(byte offset, Procedure procedure)
        {
            if (!m_procedures.ContainsKey(procedure.Value))
            {
                // combine (Extension) in the event two components have the same procedure value
                //      or the instances of the same class are on the gameobject
                m_procedures.Add(offset.Combine(procedure.Value), procedure);
                return true;
            }
            return false;
        }

        internal IEnumerator Synchronize(short key, Signature signature)
        {
            // with reference signature rule
            //              client to server 
            //              server to clients

            Func<bool> relevant = () =>
            {
                if (signature.Relevance)
                {

                }
                return true;
            };

            Action send = () =>
            {
                if (relevant())
                {
                    Network.Forward(Network.Fickle, Flags.Signature,
                        (ref Writer writer) =>
                        {
                            writer.WriteSync(identity.Value, key);
                            signature.Sending(ref writer);
                        });
                }
            };

            while (Network.Internal(Host.Any))
            {
                switch (signature.Control)
                {
                    case Signature.Rule.Round:
                        if (Network.Internal(Host.Server))
                        {
                            send();
                        }
                        if (Network.Internal(Host.Client) && authority.Value == Network.Authority())
                        {
                            send();
                        }
                        break;
                    case Signature.Rule.Server:
                        if (Network.Internal(Host.Server))
                        {
                            send();
                        }
                        break;
                    case Signature.Rule.Authority:
                        if (authority.Value == Network.Authority())
                        {
                            send();
                        }
                        else if (Network.Internal(Host.Server) && relevant())
                        {
                            Network.Forward((c) => c != authority.Value,
                                Network.Fickle, Flags.Signature,
                                (ref Writer writer) =>
                                {
                                    writer.WriteSync(identity.Value, key);
                                    signature.Sending(ref writer);
                                });
                        }
                        break;
                }
                yield return new WaitForSecondsRealtime(1.0f / signature.Rate);
            }
        }

        internal void Remote(int target, byte offset, byte procedure, Write write)
        {
            Network.Forward(
                Network.Abnormal, 
                Flags.Procedure,
                (ref Writer writer) =>
                {
                    writer.WriteCall(target, identity.Value, offset.Combine(procedure));
                    write(ref writer);
                });
        }

        public static Identity Unique()
        {
            return Identity.Generate(
                (int value) =>
                {
                    return m_instances.ContainsKey(value);
                });
        }

        internal static void OnNetworkProcedure(int connection, object state, ref Reader reader)
        {
            Packets.Call call = reader.ReadCall();
            if (m_instances.ContainsKey(call.Identity))
            {
                Instance instance = m_instances[call.Identity];
                if (instance.m_procedures.ContainsKey(call.Procedure))
                {
                    /// before i can call the procedure, check:
                    /// if this prodecure can run on client or server or both
                    /// if the network is a client or server
                    /// if the network is the target

                    Procedure procedure = instance.m_procedures[call.Procedure];

                    switch (procedure.Control)
                    {
                        case Procedure.Rule.Both:
                            break;
                        case Procedure.Rule.Server:
                            break;
                        case Procedure.Rule.Client:
                            break;
                    }

                    if (call.Target == Identity.Any)
                    {
                    }
                    if (call.Target == Network.Authority())
                    {
                    }
                    if (Network.Internal(Host.Server))
                    {
                        Network.Forward((int c) => c != connection, Network.Abnormal, Flags.Procedure, (ref Writer writer) => writer.WriteCall(call));
                    }
                    if (Network.Internal(Host.Client))
                    {
                    }

                    /*instance.m_procedures[call.Procedure].Callback(ref reader);*/
                }
            }
        }

        internal static void OnNetworkSignature(int connection, object state, ref Reader reader)
        {
            Packets.Sync sync = reader.ReadSync();
            if (m_instances.ContainsKey(sync.Identity))
            {
                Instance instance = m_instances[sync.Identity];
                if (instance.m_signatures.ContainsKey(sync.Signature))
                {
                    instance.m_signatures[sync.Signature].Recieving(ref reader);
                }
            }
        }

        public static bool Find<T>(int identity, out T instance) where T : Instance
        {
            if (m_instances.TryGetValue(identity, out Instance value))
            {
                instance = (T)value;
                return true;
            }
            instance = null;
            return false;
        }
    }
}