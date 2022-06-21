using UnityEngine;

using System;
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
        internal static readonly HashSet<Sector> n_sectors = new HashSet<Sector>();
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

        // *** redo ****
        public static bool Relevant(int authority, Vector3 point, Relevance relevancy)
        {
            switch (relevancy)
            {
                case Relevance.None:
                    return true;
                case Relevance.General:
                    return Relevant(authority, point, Relevance.Sectors) || Relevant(authority, point, Relevance.Observers);
            }

            if (n_observers.ContainsKey(authority))
            {
                foreach (var observer in n_observers[authority])
                {
                    switch (relevancy)
                    {
                        case Relevance.Sectors:
                            foreach (var station in n_sectors)
                            {
                                if (station.Overlap(observer.transform.position, point))
                                {
                                    return true;
                                }
                            }
                            break;
                        case Relevance.Observers:
                            return observer.Contains(point);
                    }
                }
            }

            return false;
        }

        public static void Relavant(Vector3 point, Relevance relevancy, Action<int> callback, Func<int, bool> filter = null)
        {
            foreach (var connections in n_observers)
            {
                if (filter?.Invoke(connections.Key) ?? true)
                {
                    if (Relevant(connections.Key, point, relevancy))
                    {
                        callback(connections.Key);
                    }
                }
            }
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