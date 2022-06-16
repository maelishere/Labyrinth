using UnityEngine;

namespace Labyrinth.Runtime
{
    [RequireComponent(typeof(Entity))]
    public sealed class Observer : Appendix
    {
        [SerializeField] private float m_relevance = 100;
    }
}