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
            if (n_network.Add(connection))
            {
                Scene scene = SceneManager.GetSceneByBuildIndex(m_scene);
                if (scene.isLoaded)
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
                }
            }
            else if (n_network.Remove(connection))
            {
                /// connection unload scene
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
            if (!Create(m_scene, Network.Authority(true)))
            {
                Debug.LogError($"Instance Idenitifier {m_scene} already exists");
                Destroy(gameObject);
                return;
            }

            // Remote should be called after Instance.Create
            //      (or else the instance never receives the message)
            // Request for entities within this scene
            Remote(Network.Authority(true), 0, 1,
                (ref Writer writer) =>
                {
                    writer.Write(Network.Authority());
                });
        }

        private void OnDestroy()
        {
            // let server know this scene is unloaded
            Remote(Network.Authority(true), 0, 1,
                   (ref Writer writer) =>
                   {
                       writer.Write(Network.Authority());
                   });
            Destroy();
        }

        public void Move(GameObject gameobject)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(m_scene); 
            if (scene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(gameobject, scene);
            }
        }
    }
}