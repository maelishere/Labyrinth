using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    [CreateAssetMenu(fileName = "Registry", menuName = "Labyrith/Registry", order = 100)]
    public class Registry : ScriptableObject
    {
        private static readonly Dictionary<uint, Entity> m_resources = new Dictionary<uint, Entity>();

        internal static bool Find(uint asset, out Entity prefab) => m_resources.TryGetValue(asset, out prefab);

        // where your prefabs are located inside the resources folder
        // unique idenitifer for a prefab
        // e.g. characters/player
        [SerializeField] private string[] m_prefabs = new string[0];

        // incase you don't want a potential impact on initial performance
        // comment out Initialize below (and call this, only once on startup, before you start up the network)
        public void Load()
        {
            for (ushort i = 0; i < m_prefabs.Length; i++)
            {
                Entity entity = Resources.Load<Entity>(m_prefabs[i]);
                if (entity)
                {
                    entity.n_asset = m_prefabs[i].Hash();
                    m_resources.Add(entity.n_asset, entity);
                }
                else
                {
                    Debug.LogWarning($"Prefab located at {m_prefabs[i]} isn't a network entity");
                }
            }
        }

        // this should only be called once because the network can always be restarted
        // i encourage you to think about using the function above and commenting this one out
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            // when assigning each entity prefab a idenitifer it must be preditable
            // find all registries at the root of resources folder
            Registry[] registries = Resources.LoadAll<Registry>(""); /*Ram (Overkill)*/
            for (ushort i = 0; i < registries.Length; i++)
            {
                for (ushort x = 0; x < registries[i].m_prefabs.Length; x++)
                {
                    Entity entity = Resources.Load<Entity>(registries[i].m_prefabs[x]);
                    if (entity)
                    {
                        entity.n_asset = i.Combine(x);
                        m_resources.Add(entity.n_asset, entity);
                    }
                    else
                    {
                        Debug.LogWarning($"Prefab located at {registries[i].m_prefabs[x]} isn't a network entity");
                    }
                }
            }
        }
    }
}