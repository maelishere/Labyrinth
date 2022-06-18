using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    public sealed class Entity : Instance
    {
        [SerializeField] private int m_world;

        // unique idenitfier for the asset when spwaning through the network
        [HideInInspector, SerializeField] internal int n_asset;

        private void Awake()
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
            if (World.n_existence.ContainsKey(m_world))
            {
                World.n_existence[m_world].n_entities.Add(identity.Value);
                World.n_existence[m_world].Move(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (World.n_existence.ContainsKey(m_world))
            {
                World.n_existence[m_world].n_entities.Remove(identity.Value);
            }
            if (authority == Network.Authority())
            {
                Network.Forward(Network.Reliable, Flags.Destroy,
                    (ref Writer writer) =>
                    {
                        writer.WriteCease(identity.Value);
                    });
            }
            Destroy();
        }

        internal static void OnNetworkCreate(int connection, object state, ref Reader reader)
        {
            Packets.Spawn spawn = reader.ReadSpawn();
            if (Registry.Find(spawn.Asset, out Entity prefab))
            {
                Entity entity = Instantiate(prefab, spawn.Position, Quaternion.Euler(spawn.Rotation));
                entity.Create(spawn.Identity, Network.Authority());
                entity.n_asset = spawn.Asset;

                if (Network.Internal(Host.Server))
                {
                    /*Network -> Send Other Connections Spawn();*/
                    Network.Forward((c) => spawn.Authority != c, Network.Reliable, Flags.Create, (ref Writer writer) => writer.WriteSpawn(entity));
                }
            }
        }

        internal static void OnNetworkDestory(int connection, object state, ref Reader reader)
        {
            Packets.Cease cease = reader.ReadCease();
            if (Find(cease.Identity, out Entity entity))
            {
                Destroy(entity.gameObject);
            }
        }
    }
}