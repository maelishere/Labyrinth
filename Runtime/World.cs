using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    // server authority instances
    public sealed class World : Instance
    {
        internal static readonly Dictionary<int, World> n_existence = new Dictionary<int, World>();

        // network representation of a scene
        // it must be already loaded by the client
        [SerializeField] private int m_scene;

        // fill this either through an Entity on Wake
        //      or find entities through scene.GetRootGameObjects()
        internal readonly HashSet<int> n_entities = new HashSet<int>();

        private void Awake()
        {
            if (!Create(m_scene, Network.Authority(true)))
            {
                Debug.LogError($"Instance Idenitifier {m_scene} already exists");
                Destroy(gameObject);
                return;
            }
            n_existence.Add(m_scene, this);
            Register(0, new Procedure(1, Procedure.Rule.Server,
                (ref Reader reader) =>
                {
                    int connection = reader.ReadInt();
                    Scene scene = SceneManager.GetSceneByBuildIndex(m_scene);
                    if (scene.isLoaded)
                    {
                            /*scene.GetRootGameObjects();*/
                        foreach (var instance in n_entities)
                        {
                            if (Find(instance, out Entity entity))
                            {

                                Network.Forward(connection, Network.Reliable, Flags.Create,
                                    (ref Writer writer) =>
                                    {
                                        writer.WriteSpawn(entity);
                                    });
                            }
                        }
                    }
                }));
        }

        private void Start()
        {
            Remote(Network.Authority(true), 0, 1,
                (ref Writer writer) =>
                {
                    writer.Write(Network.Authority());
                });
        }

        private void OnDestroy()
        {
            n_existence.Remove(m_scene);
            Destroy();
        }

        public void Move(GameObject gameobject)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(m_scene);
            SceneManager.MoveGameObjectToScene(gameobject, scene);
        }

        /*[RuntimeInitializeOnLoadMethod]
        private static void RuntimeLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Network.Internal(Host.Any))
            {
            }
        }

        private static void OnSceneUnLoaded(Scene scene)
        {
            if (Network.Internal(Host.Any))
            {
            }
        }*/
    }
}