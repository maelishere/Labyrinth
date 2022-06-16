using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    [DisallowMultipleComponent]
    public abstract class Instance : MonoBehaviour
    {
        private static Dictionary<int, Instance> m_instances = new Dictionary<int, Instance>();

        private Appendix[] m_appendices;

        private readonly Dictionary<short, Signature> m_signatures = new Dictionary<short, Signature>();
        private readonly Dictionary<short, Procedure> m_procedures = new Dictionary<short, Procedure>();

        public Identity identity { get; private set; }

        public Identity authority { get; private set; }

        private void Awake()
        {
            m_appendices = GetComponentsInChildren<Appendix>();
            for (int i = 0; i < m_appendices.Length; i++)
            {
                m_appendices[i].m_offset = (byte)(i + 1); // offset 0 belongs to the class inheriting from instance
                m_appendices[i].m_network = this;
            }
        }

        internal bool Create(int identifier, int connection)
        {
            if (!m_instances.ContainsKey(identifier))
            {
                identity = new Identity(identifier);
                authority = new Identity(connection);
                m_instances.Add(identifier, this);

                /// after start synchronizing signatures
                return true;
            }
            return false;
        }

        internal bool Destroy()
        {
            /// before stop synchronizing signatures
            
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

        internal IEnumerator Synchronize()
        {
            // with reference signature settings
            // client to server 
            // server to clients
            yield return new WaitForSecondsRealtime(1.0f /*/ m_sync*/);
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
                    instance.m_procedures[call.Procedure].Callback(ref reader);
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