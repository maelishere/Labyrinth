using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    // processed mainly on the server 
    // main network manager and for relevance
    [RequireComponent(typeof(World))]
    public sealed class Central : MonoBehaviour
    {
        internal static readonly HashSet<Station> n_stations = new HashSet<Station>();
        internal static readonly Dictionary<int, HashSet<Observer>> n_observers = new Dictionary<int, HashSet<Observer>>();
        
        private void Awake()
        {
            if (FindObjectOfType<Central>() == this)
            {
                Debug.LogError("There should only be 1 Central inbetween all loaded scenes");
                Destroy(gameObject);
                return;
            }
        }

        public static bool Relevant(int authority, Vector3 point, Relevance relevancy)
        {
            if (n_observers.ContainsKey(authority))
            {
                foreach (var observer in n_observers[authority])
                {
                    if (observer.Contains(point))
                    {
                        switch (relevancy)
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
                        }
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

        internal static void OnNetworkCreate(int socket, int connection, object state, ref Reader reader)
        {
            Packets.Spawn spawn = reader.ReadSpawn();
            if (Registry.Find(spawn.Asset, out Entity prefab))
            {
                Entity entity = Instantiate(prefab, spawn.Position, Quaternion.Euler(spawn.Rotation));
                entity.Create(spawn.Identity, spawn.Authority);

                if (Network.Internal(Host.Server))
                {
                    /// Server -> Send Other Connections Spawn() that have loaded the world;
                    /// the server should have all the worlds loaded
                    Instance.Find(entity.PreferredScene, out World world);
                    Network.Forward((c) => spawn.Authority != c && world.n_network.Contains(spawn.Authority), 
                        Network.Reliable, Flags.Create, (ref Writer writer) => writer.WriteSpawn(entity));
                }
            }
        }

        internal static void OnNetworkDestory(int socket, int connection, object state, ref Reader reader)
        {
            Packets.Cease cease = reader.ReadCease();
            if (Instance.Find(cease.Identity, out Entity entity))
            {
                if (Network.Internal(Host.Server))
                {
                    /// Server -> Send Other Connections Cease() that have loaded the world;
                    /// the server should have all the worlds loaded
                    Instance.Find(entity.PreferredScene, out World world);
                    Network.Forward((c) => cease.Authority != c && world.n_network.Contains(cease.Authority),
                        Network.Reliable, Flags.Create, (ref Writer writer) => writer.WriteSpawn(entity));
                }

                Destroy(entity.gameObject);
            }
        }
    }
}