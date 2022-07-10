using UnityEngine;

namespace Labyrinth.Runtime
{
    using Bolt;
    using Labyrinth.Background;
    using System;

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
                if (Create(id.Value, Network.Authority()))
                {
                    n_world = World.n_active;

                    // spawn over the network
                    Network.Forward(Channels.Ordered, Flags.Create,
                        (ref Writer writer) =>
                        {
                            writer.WriteSpawn(this);
                        });
                }
                else
                {
                    // maybe you have way too many network instances
                    //      though this will unlikely never get called
                    //      here just incase of a bug
                    Debug.LogError($"Instance({id.Value}) already exists");
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            if (Find(n_world, out World world))
            {
                world.n_entities.Add(identity.Value);
                world.Move(gameObject);
            }
            else
            {
                Debug.LogError($"World({n_world}) wasn't loaded destroying Entity({identity.Value})");
                Destroy(gameObject);
            }
        }

        // changes the authority of this on all client to connection
        public void Advocate(int connection)
        {
            if (NetworkClient.Active)
            {
                Debug.LogError($"Only server can request a change of authority");
                return;
            }
            // only send if server actually is running
            if (NetworkServer.Active)
            {
                Synchronous(connection);
                Network.Forward(Channels.Ordered, Flags.Ownership,
                    (ref Writer writer) =>
                    {
                        writer.Write(identity.Value);
                        writer.Write(connection);
                    });
            }
        }

        private void OnDestroy()
        {
            if (Find(n_world, out World world))
            {
                world.n_entities.Remove(identity.Value);
            }
            if (!m_networkCeasing && (authority == Network.Authority() || NetworkServer.Active))
            {
                Network.Forward(Channels.Ordered, Flags.Destroy,
                    (ref Writer writer) =>
                    {
                        writer.Write(identity.Value);
                    });
            }
            Destroy();
        }

        internal static void OnNetworkCreate(int socket, int connection, uint timestamp, ref Reader reader)
        {
            Packets.Spawn spawn = reader.ReadSpawn();
            NetworkDebug.Slient($"Creating Entity({spawn.Identity})");
            if (Registry.Find(spawn.Asset, out Entity prefab))
            {
                if (NetworkServer.Active)
                {
                    /// Server -> Send Other Connections Spawn() that have loaded the world;
                    /// the server should have all the worlds loaded or at least before any client
                    if (Find(spawn.World, out World world))
                    {
                        Network.Forward((c) => spawn.Authority != c && world.n_network.Contains(spawn.Authority),
                            Channels.Ordered, Flags.Create, (ref Writer writer) => writer.WriteSpawn(spawn));
                    }
                    else
                    {
                        Debug.LogWarning($"Server({socket}) hasn't loaded in World({spawn.World}) can't created Entity({spawn.Identity})");
                        return;
                    }
                }

                m_networkSpawning = true;
                Entity entity = Instantiate(prefab, spawn.Position, Quaternion.Euler(spawn.Rotation));
                // attached network scripts should have registered their variables and function on awake (inside Instantiate)
                entity.Create(spawn.Identity, spawn.Authority);
                entity.n_world = spawn.World;
                m_networkSpawning = false;

                NetworkDebug.Slient($"Created Entity({spawn.Identity})");
            }
            else
            {
                Debug.LogWarning($"Network was trying to create an entity that wasn't in a registry");
            }
        }

        internal static void OnNetworkDestory(int socket, int connection, uint timestamp, ref Reader reader)
        {
            int identity = reader.ReadInt();
            NetworkDebug.Slient($"Destroying Entity({identity})");
            if (Find(identity, out Entity entity))
            {
                if (connection == entity.authority.Value || connection == Network.Authority(true))
                {
                    if (NetworkServer.Active)
                    {
                        /// Server -> Send Other Connections Cease() that have loaded the world;
                        /// the server should have all the worlds loaded or at least before any client
                        if (Find(entity.n_world, out World world))
                        {
                            Network.Forward((c) => entity.authority.Value != c && world.n_network.Contains(entity.authority.Value),
                                Channels.Ordered, Flags.Create, (ref Writer writer) => writer.Write(identity));
                        }
                        else
                        {
                            Debug.LogWarning($"Server({socket}) hasn't loaded in World({entity.n_world}) can't destroy Entity({identity})");
                            return;
                        }
                    }

                    /// ensures it doesn't send a network message when OnDestroy is called
                    ///  (Server to Clients) server can destroy any entity even if it doesn't have authority
                    entity.m_networkCeasing = true;
                    Destroy(entity.gameObject);
                }
                else
                {
                    // incase of a bug
                    Debug.LogWarning($"Client({connection}) was trying to destroy Entity({identity}) but its authority is Client({entity.authority.Value})");
                }
            }
            else
            {
                Debug.LogWarning($"Network was trying to destroy an entity doesn't exist");
            }
        }

        // always from server
        internal static void OnNetworkOwnership(int socket, int connection, uint timestamp, ref Reader reader)
        {
            int identity = reader.ReadInt();
            int authority = reader.ReadInt();
            if (Find(identity, out Entity entity))
            {
                entity.Synchronous(authority);
            }
            else
            {
                Debug.LogWarning($"Network was trying to access an entity doesn't exist");
            }
        }
    }
}