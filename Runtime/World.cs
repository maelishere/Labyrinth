using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;
    using Labyrinth.Background;

    // server authority instances
    [AddComponentMenu("Labyrinth/World")]
    public sealed class World : Instance
    {
        // virtual marker for which world the next locally spawn entity belongs to
        internal static int n_active;

        // which scenes are loaded on each client (Server Only)
        private static readonly Dictionary<int, HashSet<int>> m_network = new Dictionary<int, HashSet<int>>();

        // network representation of a scene
        // it must be already loaded by the client
        [SerializeField] private int m_scene;

        // fill this either through an Entity on their Start()
        //      or find entities through scene.GetRootGameObjects()
        internal readonly HashSet<int> n_entities = new HashSet<int>();

        protected override void Awake()
        {
            base.Awake();
            // We assign Instance an identifier (scene build number) and the server identity
            if (m_scene == 0 || !Create(m_scene, Network.Authority(true)))
            {
                Debug.LogError($"World instance idenitifier {m_scene} is invalid or already exists");
                Destroy(gameObject);
                return;
            }
        }

        protected override void Start()
        {
            base.Start();
            if (Central.n_instance)
            {
                if (!Central.n_instance.NetworkScene(n_active))
                {
                    Anchor();
                }
            }
            if (NetworkClient.Active)
            {
                // Request for entities within this scene
                Network.Forward(Channels.Ordered, Flags.Loaded,
                    (ref Writer writer) =>
                    {
                        writer.Write(m_scene);
                    });
            }
        }

        private void OnDestroy()
        {
            if (NetworkClient.Active)
            {
                // let server know this scene is unloaded
                Network.Forward(Channels.Ordered, Flags.Offloaded,
                    (ref Writer writer) =>
                    {
                        writer.Write(m_scene);
                    });
            }
            Destroy();
        }

        // any entity created locally after this function call will belong to this world
        public void Anchor()
        {
            n_active = m_scene;
        }

        public void Move(GameObject gameobject)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(m_scene);
            SceneManager.MoveGameObjectToScene(gameobject, scene);
        }

        public static bool Instantiate<T>(int scene, T original, Vector3 position, Quaternion rotation, out T clone) where T : Object
        {
            if (Find(scene, out World world))
            {
                world.Anchor();
                clone = Instantiate(original, position, rotation);
                return true;
            }

            clone = null;
            return false;
        }

        // has a client loaded in this world (Server Only)
        public static bool Loaded(int client, int world)
        {
            if (m_network.ContainsKey(client))
                return m_network[client].Contains(world);
            else
                return false;
        }

        // have all client and the server loaded in this world (Server Only)
        public static bool Loaded(int world)
        {
            if (Find(world, out World _))
            {
                foreach (var client in m_network)
                {
                    if (!client.Value.Contains(world))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        // this flag always comes from clients to server
        internal static void OnNetworkLoaded(int socket, int connection, uint timestamp, ref Reader reader)
        {
            int scene = reader.ReadInt();
            NetworkDebug.Slient($"Client({connection}) loaded scene {scene}");
            if (Find(scene, out World world))
            {
                m_network[connection].Add(scene);
                NetworkDebug.Slient($"Sending {world.n_entities.Count} entities to Client({connection})");
                foreach (var instance in world.n_entities)
                {
                    if (Find(instance, out Entity entity))
                    {
                        // send client all entities within the scene
                        Network.Forward(connection, Channels.Ordered, Flags.Create,
                            (ref Writer writer) =>
                            {
                                writer.WriteSpawn(entity);
                            });
                        NetworkDebug.Slient($"Sending Entity({instance}) to Client({connection})");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"World({scene}) isn't loaded on Server({socket})");
            }
        }

        // this flag always comes from clients to server
        internal static void OnNetworkOffloaded(int socket, int connection, uint timestamp, ref Reader reader)
        {
            int scene = reader.ReadInt();
            NetworkDebug.Slient($"Client({connection}) unloaded scene {scene}");
            if (Find(scene, out World world))
            {
                m_network[connection].Remove(scene);
            }
            else
            {
                Debug.LogWarning($"World({scene}) isn't loaded on Server({socket})");
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Network.connected.AddListener(OnNetworkConnected);
            Network.disconnected.AddListener(OnNetworkDisconnected);
        }

        private static void OnNetworkConnected(int socket, int connection)
        {
            if (NetworkServer.Active)
            {
                m_network.Add(connection, new HashSet<int>());
            }
        }

        private static void OnNetworkDisconnected(int socket, int connection)
        {
            if (NetworkClient.Active)
            {
                m_network.Remove(connection);
            }
        }
    }
}