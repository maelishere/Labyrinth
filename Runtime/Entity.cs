using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    [AddComponentMenu("Labyrinth/Entity")]
    public sealed class Entity : Instance
    {
        /// the scene this entity belongs in
        [SerializeField] private int m_world;

        /// the path to find the entity in the resources folder
        // [SerializeField] private string m_path;

        // unique idenitfier for the asset when spwaning through the network
        [HideInInspector, SerializeField] internal int n_asset;

        public int PreferredScene => m_world;

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
            if (Find(m_world, out World world))
            {
                world.n_entities.Add(identity.Value);
                world.Move(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Find(m_world, out World world))
            {
                world.n_entities.Remove(identity.Value);
            }
            if (authority == Network.Authority())
            {
                Network.Forward(Network.Reliable, Flags.Destroy,
                    (ref Writer writer) =>
                    {
                        writer.WriteCease(identity.Value, authority.Value);
                    });
            }
            Destroy();
        }

        internal static void OnNetworkCreate(int socket, int connection, uint timestamp, ref Reader reader)
        {
            Packets.Spawn spawn = reader.ReadSpawn();
            // Debug.Log($"Creating Entity({spawn.Identity})");
            if (Registry.Find(spawn.Asset, out Entity prefab))
            {
                Entity entity = Instantiate(prefab, spawn.Position, Quaternion.Euler(spawn.Rotation));
                entity.Create(spawn.Identity, spawn.Authority);

                if (Network.Internal(Host.Server))
                {
                    /// Server -> Send Other Connections Spawn() that have loaded the world;
                    /// the server should have all the worlds loaded
                    Instance.Find(entity.PreferredScene, out World world);
                    Network.Forward((c) => spawn.Authority != c && world.n_network.Contains(spawn.Authority),
                        Network.Reliable, Flags.Create, (ref Writer writer) => writer.WriteSpawn(entity));
                }

                // Debug.Log($"Created Entity({spawn.Identity})");
            }
        }

        internal static void OnNetworkDestory(int socket, int connection, uint timestamp, ref Reader reader)
        {
            Packets.Cease cease = reader.ReadCease();
            if (Instance.Find(cease.Identity, out Entity entity))
            {
                if (Network.Internal(Host.Server))
                {
                    /// Server -> Send Other Connections Cease() that have loaded the world;
                    /// the server should have all the worlds loaded
                    Instance.Find(entity.PreferredScene, out World world);
                    Network.Forward((c) => cease.Authority != c && world.n_network.Contains(cease.Authority),
                        Network.Reliable, Flags.Create, (ref Writer writer) => writer.WriteSpawn(entity));
                }

                Destroy(entity.gameObject);
            }
        }
    }
}