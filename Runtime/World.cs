﻿using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    // server authority instances
    [AddComponentMenu("Labyrinth/World")]
    public sealed class World : Instance
    {
        internal static int n_active;

        // network representation of a scene
        // it must be already loaded by the client
        [SerializeField] private int m_scene;

        // which client has loaded in this scene
        internal readonly HashSet<int> n_network = new HashSet<int>();

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

        private void Start()
        {
            if (Central.n_instance)
            {
                if (!Central.n_instance.NetworkScene(n_active))
                {
                    Anchor();
                }
            }
            if (Network.Internal(Host.Client))
            {
                // Request for entities within this scene
                Network.Forward(Channels.Ordered, Flags.Loaded,
                    (ref Writer writer) =>
                    {
                        writer.WriteSection(m_scene, Network.Authority());
                    });
            }
        }

        private void OnDestroy()
        {
            if (Network.Internal(Host.Client))
            {
                // let server know this scene is unloaded
                Network.Forward(Channels.Ordered, Flags.Offloaded,
                    (ref Writer writer) =>
                    {
                        writer.WriteSection(m_scene, Network.Authority());
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

        // this flag always comes from clients to server
        internal static void OnNetworkLoaded(int socket, int connection, uint timestamp, ref Reader reader)
        {
            Packets.Section section = reader.ReadSection();
            /*Debug.Log($"Client({section.Client}) loaded scene {section.Scene}");*/
            if (Find(section.Scene, out World world))
            {
                world.n_network.Add(section.Client);
                // Debug.Log($"Sending {world.n_entities.Count} entities to Client({section.Client})");
                foreach (var instance in world.n_entities)
                {
                    if (Find(instance, out Entity entity))
                    {
                        // send client all entities within the scene
                        Network.Forward(section.Client, Channels.Ordered, Flags.Create,
                            (ref Writer writer) =>
                            {
                                writer.WriteSpawn(entity);
                            });
                        /*Debug.Log($"Sending Entity({instance}) to Client({section.Client})");*/
                    }
                }
            }
        }

        // this flag always comes from clients to server
        internal static void OnNetworkOffloaded(int socket, int connection, uint timestamp, ref Reader reader)
        {
            Packets.Section section = reader.ReadSection();
            if (Find(section.Scene, out World world))
            {
                world.n_network.Remove(section.Client);
                // Debug.Log($"Client({section.Client}) unloaded scene {section.Scene}");
            }
        }
    }
}