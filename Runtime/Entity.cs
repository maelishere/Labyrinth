using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    using Bolt;

    [AddComponentMenu("Labyrinth/Entity")]
    public sealed class Entity : Instance
    {
        [SerializeField] private int m_world;

        // unique idenitfier for the asset when spwaning through the network
        [HideInInspector, SerializeField] internal int n_asset;

        public int PreferredScene => m_world;

        private void Start()
        {
            // if it was instantiated locally the identitifer would be 0
            if (identity.Value == Identity.Any)
            {
                Identity id = Unique();
                Create(id.Value, Network.Authority());
                // spawn over the network
                Network.Forward(Network.Reliable, Flags.Create,
                    (ref Writer writer) =>
                    {
                        writer.WriteSpawn(this);
                    });
            }
            if (Find(m_world, out World world))
            {
                world.n_entities.Add(identity.Value);
                world.Move(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Find(m_world, out World world))
            {
                world.n_entities.Remove(identity.Value);
            }
            if (authority == Network.Authority())
            {
                Network.Forward(Network.Reliable, Flags.Destroy,
                    (ref Writer writer) =>
                    {
                        writer.WriteCease(identity.Value, authority.Value);
                    });
            }
            Destroy();
        }
    }
}