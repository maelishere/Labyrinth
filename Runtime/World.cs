using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    // server authority instances
    public sealed class World : Instance
    {
        // a world isn't spawned over the network
        // it must be already loaded by the client
        [SerializeField] private int m_scene;

        // need to fill this either through an Entity on Wake
        //      or find entities through scene.rootgameobjects
        private readonly HashSet<int> m_entities = new HashSet<int>();
        
        protected override void Wake()
        {
            if (Network.Internal(Host.Any))
            {
                if (!Create(m_scene, Network.Authority(true)))
                {
                    Debug.LogError($"Instance Idenitifier {m_scene} already exists");
                    Destroy(gameObject);
                    return;
                }
                Register(0, new Procedure(1, Procedure.Rule.Server,
                    (ref Reader reader) =>
                    {
                        int connection = reader.ReadInt();
                        Scene scene = SceneManager.GetSceneByBuildIndex(m_scene);
                        if (scene.isLoaded)
                        {
                            /*scene.GetRootGameObjects();*/
                            foreach (var instance in m_entities)
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
        }

        private void Start()
        {
            if (Network.Internal(Host.Client))
            {
                Remote(Network.Authority(true), 0, 1, 
                    (ref Writer writer)=>
                    {
                        writer.Write(Network.Authority());
                    });
            }
        }

        private void OnDestroy()
        {
            if (Network.Internal(Host.Any))
            {
                Destroy();
            }
        }

        public void Move(GameObject gameobject)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(m_scene);
            SceneManager.MoveGameObjectToScene(gameobject, scene);
        }
    }
}