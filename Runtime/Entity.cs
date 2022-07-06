using UnityEngine;
using UnityEngine.SceneManagement;

namespace Labyrinth.Runtime
{
    using Bolt;

    [AddComponentMenu("Labyrinth/Entity")]
    public sealed class Entity : Instance
    {
        private static bool m_networkSpawning = false;
        private bool m_networkCeasing = false;

        /// the path to find the entity in the resources folder
        // [SerializeField] private string m_path;

        // unique idenitfier for the asset when spwaning through the network
        [HideInInspector, SerializeField] internal uint n_asset;

        internal int n_world;

        protected override void Awake()
        {
            base.Awake();
            // check if it was instantiated (just now) locally the identitifer would be 0
            // and make sure it wasn't the network isn't creating it
            if (identity.Value == Identity.Any && !m_networkSpawning)
            {
                Identity id = Unique();
                Create(id.Value, Network.Authority());
                n_world = World.n_active;

                // spawn over the network
                Network.Forward(Channels.Ordered, Flags.Create,
                    (ref Writer writer) =>
                    {
                        writer.WriteSpawn(this);
                    });
            }
        }

        protected override void  Start()
        {
            base.Start();
            if (Find(n_world, out World world))
            {
                world.n_entities.Add(identity.Value);
                world.Move(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Find(n_world, out World world))
            {
                world.n_entities.Remove(identity.Value);
            }
            if (!m_networkCeasing && (authority == Network.Authority() || Network.Internal(Host.Server)))
            {
                Network.Forward(Channels.Ordered, Flags.Destroy,
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
            /*Debug.Log($"Creating Entity({spawn.Identity})");*/
            if (Registry.Find(spawn.Asset, out Entity prefab))
            {
                m_networkSpawning = true;
                Entity entity = Instantiate(prefab, spawn.Position, Quaternion.Euler(spawn.Rotation));
                // attached network scripts should have registered their variables and function on awake (inside Instantiate)
                entity.Create(spawn.Identity, spawn.Authority);
                entity.n_world = spawn.World;
                m_networkSpawning = false;

                if (Network.Internal(Host.Server))
                {
                    /// Server -> Send Other Connections Spawn() that have loaded the world;
                    /// the server should have all the worlds loaded or at least before any client
                    Find(spawn.World, out World world);
                    Network.Forward((c) => spawn.Authority != c && world.n_network.Contains(spawn.Authority),
                        Channels.Ordered, Flags.Create, (ref Writer writer) => writer.WriteSpawn(entity));
                }

                /*Debug.Log($"Created Entity({spawn.Identity})");*/
            }
            else
            {
                Debug.LogWarning($"Network was trying to create an entity that wasn't in a registry");
            }
        }

        internal static void OnNetworkDestory(int socket, int connection, uint timestamp, ref Reader reader)
        {
            Packets.Cease cease = reader.ReadCease();
            if (Find(cease.Identity, out Entity entity))
            {
                if (Network.Internal(Host.Server))
                {
                    /// Server -> Send Other Connections Cease() that have loaded the world;
                    /// the server should have all the worlds loaded or at least before any client
                    Find(entity.n_world, out World world);
                    Network.Forward((c) => cease.Authority != c && world.n_network.Contains(cease.Authority),
                        Channels.Ordered, Flags.Create, (ref Writer writer) => writer.WriteSpawn(entity));
                }

                /// ensures it doesn't send a network message when OnDestroy is called
                entity.m_networkCeasing = true;
                Destroy(entity.gameObject);
            }
        }
    }
}