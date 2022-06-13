using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Labyrinth
{
    using Bolt;

    [Serializable]
    public sealed class Instance
    {
        private static Dictionary<int, Instance> m_instances = new Dictionary<int, Instance>();

        [SerializeField, Range(1, 60)] private int m_sync = 10;

        private readonly Dictionary<short, Signature> m_signatures = new Dictionary<short, Signature>();
        private readonly Dictionary<short, Procedure> m_procedures = new Dictionary<short, Procedure>();

        public Identity identity { get; private set; }
        public Identity authority { get; private set; }

        public bool Create(int identifier, int connection)
        {
            if (!m_instances.ContainsKey(identifier))
            {
                identity = new Identity(identifier);
                authority = new Identity(connection);
                m_instances.Add(identifier, this);
                return true;
            }
            return false;
        }

        public bool Destroy()
        {
            return m_instances.Remove(identity.Value);
        }

        internal bool Register(Signature signature)
        {
            if (!m_signatures.ContainsKey(signature.Value))
            {
                m_signatures.Add(signature.Value, signature);
                return true;
            }
            return false;
        }

        internal bool Register(Procedure procedure)
        {
            if (!m_procedures.ContainsKey(procedure.Value))
            {
                m_procedures.Add(procedure.Value, procedure);
                return true;
            }
            return false;
        }

        internal IEnumerator Synchronize()
        {
            // with reference to the rule
            // client to server 
            // server to clients
            yield return new WaitForSecondsRealtime(1.0f / m_sync);
        }

        public static Identity Unique()
        {
            return Identity.Generate(
                (int value) =>
                {
                    return m_instances.ContainsKey(value);
                });
        }

        internal static void OnProcedure(int connection, int identity, short procedure, ref Reader reader)
        {
            if (m_instances.ContainsKey(identity))
            {
                Instance instance = m_instances[identity];
                if (instance.m_procedures.ContainsKey(procedure))
                {
                    instance.m_procedures[procedure].Callback(ref reader);
                }
            }
        }

        internal static void OnSignature(int connection, int identity, short signature, ref Reader reader)
        {
            if (m_instances.ContainsKey(identity))
            {
                Instance instance = m_instances[identity];
                if (instance.m_signatures.ContainsKey(signature))
                {
                    instance.m_signatures[signature].Recieving(ref reader);
                }
            }
        }
    }
}
