using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    [AddComponentMenu("Network/Registry")]
    public class Registry : ScriptableObject
    {
        private static readonly Dictionary<int, Entity> m_resources = new Dictionary<int, Entity>();

        // where your prefabs are located inside the resources folder
        // unique idenitifer for a prefab
        // e.g. characters/player
        [SerializeField] private string[] m_prefabs;

        // this should only be called once because the network can always be restarted
        [RuntimeInitializeOnLoadMethod]
        internal static void Initialize()
        {
            // when assigning each entity prefab a idenitifer it must be preditable
            ushort identity = 1; // we start at 1 because an asset with 0 is an empty

            // find all registries at the root of resources folder
            Registry[] registries = Resources.LoadAll<Registry>("");
            for (int i = 0; i < registries.Length; i++)
            {
                for (ushort x = 0; x < registries[i].m_prefabs.Length; i++)
                {
                    Entity entity = Resources.Load<Entity>(registries[i].m_prefabs[x]);
                    if (entity)
                    {
                        entity.m_asset = identity.Combine(x);
                        m_resources.Add(entity.m_asset, entity);
                        identity++;
                    }
                    else
                    {
                        Debug.LogWarning($"Prefab located at {registries[i].m_prefabs[x]} isn't a network entity");
                    }
                }
            }
        }

        internal static bool Find(int asset, out Entity entity) => m_resources.TryGetValue(asset, out entity);
    }
}