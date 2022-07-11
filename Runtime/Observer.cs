using UnityEngine;

using System;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    // for relevance (client <-> server)
    [RequireComponent(typeof(Entity)), AddComponentMenu("Labyrinth/Observer")]
    public sealed class Observer : MonoBehaviour
    {
        internal static readonly Dictionary<int, HashSet<Observer>> n_observers = new Dictionary<int, HashSet<Observer>>();

        [SerializeField] private float m_radius = 100;
        [SerializeField] private Vector3 m_offset = Vector3.zero;
        [SerializeField] private Layers m_layers = new Layers(0);

        internal Instance n_attached;

        private void Start()
        {
            n_attached = GetComponent<Instance>();
            if (!n_observers.ContainsKey(n_attached.authority.Value))
            {
                n_observers.Add(n_attached.authority.Value, new HashSet<Observer>());
            }
            n_observers[n_attached.authority.Value].Add(this);
        }

        private void OnDestroy()
        {
            n_observers[n_attached.authority.Value].Remove(this);
            if (n_observers[n_attached.authority.Value].Count == 0)
            {
                n_observers.Remove(n_attached.authority.Value);
            }
        }

        public bool Contains(Layers layers, Vector3 point)
        {
            if (m_layers == layers)
                return Contains(transform.position + m_offset, m_radius, point);
            else
                return false;
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

        // loops through each observer
        public static void Each(Action<int, Observer> callback)
        {
            foreach (var authority in n_observers)
            {
                foreach (var observer in authority.Value)
                {
                    callback(authority.Key, observer);
                }
            }
        }

        // get all observers for a connection
        public static Observer[] All(int authority)
        {
            List<Observer> observers = new List<Observer>();
            if (n_observers.ContainsKey(authority))
            {
                observers.AddRange(n_observers[authority]);
            }
            return observers.ToArray();
        }

        // get all observers that exists
        public static Observer[] All()
        {
            List<Observer> observers = new List<Observer>();
            foreach (var observer in n_observers)
            {
                observers.AddRange(observer.Value);
            }
            return observers.ToArray();
        }
    }
}