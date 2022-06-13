using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Labyrinth
{
    using Bolt;

    [Serializable]
    public sealed class Identity
    {
        private static Dictionary<int, Identity> m_identities = new Dictionary<int, Identity>();

        [SerializeField, Range(1, 60)] private int m_sync = 10;

        private readonly Dictionary<short, Signature> m_signatures = new Dictionary<short, Signature>();
        private readonly Dictionary<short, Procedure> m_procedures = new Dictionary<short, Procedure>();

        public Instance instance { get; private set; }
        public Instance authority { get; private set; }

        public bool Create(int identifier, int connection)
        {
            if (!m_identities.ContainsKey(identifier))
            {
                instance = new Instance(identifier);
                authority = new Instance(connection);
                m_identities.Add(identifier, this);
                return true;
            }
            return false;
        }

        public bool Destroy()
        {
            return m_identities.Remove(instance.Value);
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

        public static Instance Unique()
        {
            return Instance.Generate(
                (int value) =>
                {
                    return m_identities.ContainsKey(value);
                });
        }

        internal static void OnProcedure(int connection, int instance, short procedure, ref Reader reader)
        {
            if (m_identities.ContainsKey(instance))
            {
                Identity identity = m_identities[instance];
                if (identity.m_procedures.ContainsKey(procedure))
                {
                    identity.m_procedures[procedure].Callback(ref reader);
                }
            }
        }

        internal static void OnSignature(int connection, int instance, short signature, ref Reader reader)
        {
            if (m_identities.ContainsKey(instance))
            {
                Identity identity = m_identities[instance];
                if (identity.m_signatures.ContainsKey(signature))
                {
                    identity.m_signatures[signature].Recieving(ref reader);
                }
            }
        }
    }
}
