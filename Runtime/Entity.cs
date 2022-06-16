using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

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
                Create(id.Value, Network.Authority());
                // spawn over the network
                Network.Forward(Network.Reliable, Flags.Create, 
                    (ref Writer writer) =>
                    {
                        writer.WriteSpawn(this);
                    });
            }
            m_existence.Add(identity.Value);
        }

        private void OnDestroy()
        {
            if (authority == Network.Authority())
            {
                Network.Forward(Network.Reliable, Flags.Destroy,
                    (ref Writer writer) =>
                    {
                        writer.WriteCease(identity.Value);
                    });
            }
            m_existence.Remove(identity.Value);
            Destroy();
        }

        /*public T AddComponent<T>() where T : Appendix
        {
            return default;
        }*/

        internal static void OnNetworkCreate(int connection, object state, ref Reader reader)
        {
            Packets.Spawn spawn = reader.ReadSpawn(); 
            if (!m_existence.Contains(spawn.Identity))
            {
                if (Network.Internal(Host.Server))
                {
                    /*Network -> Send Other Connections Spawn();*/
                }
                if (Registry.Find(spawn.Asset, out Entity prefab))
                {
                    Entity entity = Instantiate(prefab, spawn.Position, Quaternion.Euler(spawn.Rotation));
                    entity.Create(spawn.Identity, Network.Authority());
                    entity.m_asset = spawn.Asset;
                }
            }
        }

        internal static void OnServerConnection(int connection)
        {
            // when the server get a new connection we need to send all current entities to it
            foreach (var instance in m_existence)
            {
                if (Find(instance, out Entity entity))
                {
                    
                }
            }
        }

        internal static void OnNetworkDestory(int connection, object state, ref Reader reader)
        {
            Packets.Cease cease = reader.ReadCease();
            if (m_existence.Contains(cease.Identity))
            {
                if (Find(cease.Identity, out Entity entity))
                {
                    Destroy(entity.gameObject);
                }
            }
        }
    }
}