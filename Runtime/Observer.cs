using UnityEngine;

namespace Labyrinth.Runtime
{
    public sealed class Observer : MonoBehaviour
    {
        [SerializeField] private float m_relevance = 100;

        public bool Contains(Vector3 point)
        {
            return Vector3.Distance(transform.position, point) <= m_relevance;
        }
    }
}