using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    [RequireComponent(typeof(Entity)), AddComponentMenu("Labyrinth/Observer")]
    public sealed class Observer : MonoBehaviour
    {
        [SerializeField] private float m_radius = 100;
        [SerializeField] private Vector3 m_offset = Vector3.zero;

        internal Instance n_attached;

        private void Awake()
        {
            n_attached = GetComponent<Instance>();
            if (Central.n_observers.ContainsKey(n_attached.authority.Value))
            {
                Central.n_observers[n_attached.authority.Value].Add(this);
            }
        }

        private void OnDestroy()
        {
            if (Central.n_observers.ContainsKey(n_attached.authority.Value))
            {
                Central.n_observers[n_attached.authority.Value].Remove(this);
            }
        }

        public bool Contains(Vector3 point)
        {
            return Contains(transform.position + m_offset, m_radius, point);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + m_offset, m_radius);
        }

        internal static bool Contains(Vector3 center, float radius, Vector3 point)
        {
            return Vector3.Distance(center, point) <= radius;
        }
    }
}