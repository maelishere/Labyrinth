using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;
    using System;

    public sealed class Entity : Instance
    {
        private static readonly HashSet<int> m_existence = new HashSet<int>();

        // unique idenitfier for the asset when spwaning through the network
        [HideInInspector, SerializeField] internal int m_asset;

        private void Start()
        {
            // if it was instantiated locally the identitifer would be 0
            if (identity.Value == Identity.Any)
            {
                Identity id = Unique();
                Create(id.Value, 0/* change to local */);
            }
        }

        private void OnDestroy()
        {
            Destroy();
        }

        /*public T AddComponent<T>() where T : Appendix
        {
            return default;
        }*/

        internal static void OnNetworkCreate(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        internal static void HandleCreate(int connection, int authority, int instance, int asset)
        {
            
        }

        internal static void OnNetworkDestory(int connection, object state, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        internal static void HandleDestroy(int connection, int instance)
        {

        }
    }
}