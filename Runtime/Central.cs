using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    // processed mainly on the server 
    // main network manager and for relevance
    [RequireComponent(typeof(World)), AddComponentMenu("Labyrinth/Central")]
    public sealed class Central : MonoBehaviour
    {
        internal static Central n_instance;
        internal static readonly HashSet<Station> n_stations = new HashSet<Station>();
        internal static readonly Dictionary<int, HashSet<Observer>> n_observers = new Dictionary<int, HashSet<Observer>>();

        [SerializeField] private int[] m_networkedScenes = new int[0];

        private void Awake()
        {
            if (n_instance)
            {
                Debug.LogError("There should only be 1 Central inbetween all loaded scenes");
                Destroy(gameObject);
                return;
            }
            n_instance = this;
        }

        private void OnDestroy()
        {
            n_instance = null;
        }

        internal bool NetworkScene(int identity)
        {
            for (int i = 0; i < m_networkedScenes.Length; i++)
            {
                if (m_networkedScenes[i] == identity)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Relevant(int authority, Vector3 point, Relevance relevancy)
        {
            if (n_observers.ContainsKey(authority))
            {
                foreach (var observer in n_observers[authority])
                {
                    if (observer.Contains(point))
                    {
                        // until i re-edit Station class
                        /* switch (relevancy)
                        {
                            case Relevance.General:
                                foreach (var station in n_stations)
                                {
                                    if (station.Overlap(observer.transform.position, point))
                                    {
                                        return true;
                                    }
                                }
                                break;
                            case Relevance.Authority:
                                return true;
                        } */

                        return true;
                    }
                }
            }
            return false;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Network.connected.AddListener(Connection);
            Network.disconnected.AddListener(Disconnection);
        }

        private static void Connection(int socket, int connection)
        {
            n_observers.Add(connection, new HashSet<Observer>());
        }

        private static void Disconnection(int socket, int connection)
        {
            n_observers.Remove(connection);
        }
    }
}