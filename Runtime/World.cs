using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    // server authority instances
    [AddComponentMenu("Labyrinth/World")]
    public sealed class World : Instance
    {
        // network representation of a scene
        // it must be already loaded by the client
        [SerializeField] private int m_scene;

        // which client has loaded in this scene
        internal readonly HashSet<int> n_network = new HashSet<int>();

        // fill this either through an Entity on their Start()
        //      or find entities through scene.GetRootGameObjects()
        internal readonly HashSet<int> n_entities = new HashSet<int>();

        private void OnClientRequest(int connection)
        {
            Debug.Log($"Client({connection})");
            if (n_network.Add(connection))
            {
                /*scene.GetRootGameObjects();*/
                foreach (var instance in n_entities)
                {
                    if (Find(instance, out Entity entity))
                    {
                        // send connection all entities within the scene
                        Network.Forward(connection, Network.Reliable, Flags.Create,
                            (ref Writer writer) =>
                            {
                                writer.WriteSpawn(entity);
                            });
                    }
                }
                Debug.Log($"Client({connection}) loaded scene {m_scene}");
            }
            else if (n_network.Remove(connection))
            {
                /// connection unload scene
                Debug.Log($"Client({connection}) unloaded scene {m_scene}");
            }
        }

        private void Awake()
        {
            // We can register a prodecure or signature on awake
            //      However remote should be called after Instance.Create
            Register(0, new Procedure(1, Procedure.Rule.Server,
                (ref Reader reader) =>
                {
                    OnClientRequest(reader.ReadInt());
                }));
        }

        private void Start()
        {
            // We assign Instance an identifier (scene build number) and the server identity
            if (m_scene == 0 || !Create(m_scene, Network.Authority(true)))
            {
                Debug.LogError($"World instance idenitifier {m_scene} is invalid or already exists");
                Destroy(gameObject);
                return;
            }

            if (Network.Internal(Host.Client))
            {
                // Remote should be called after Instance.Create
                //      (or else the instance never receives the message)
                // Request for entities within this scene
                Remote(Network.Authority(true), 0, 1,
                    (ref Writer writer) =>
                    {
                        writer.Write(Network.Authority());
                    });
            }
        }

        private void OnDestroy()
        {
            if (Network.Internal(Host.Client))
            {
                // let server know this scene is unloaded
                Remote(Network.Authority(true), 0, 1,
                       (ref Writer writer) =>
                       {
                           writer.Write(Network.Authority());
                       });
            }

            Destroy();
        }

        public void Move(GameObject gameobject)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(m_scene);
            SceneManager.MoveGameObjectToScene(gameobject, scene);
        }
    }
}