using UnityEngine;

namespace Labyrinth.Runtime
{
    using Bolt;

    public sealed class Entity : Instance
    {
        // where the prefab is located in resources 
        // unique idenitifer for a prefab
        // e.g. characters/player
        [SerializeField] private string m_asset;

        internal static void OnCreate(int connection, int authority, int instance)
        {
            
        }

        internal static void OnDestroy(int connection, int instance)
        {

        }
    }
}