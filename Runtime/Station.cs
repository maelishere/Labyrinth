using System;
using UnityEngine;

namespace Labyrinth.Runtime
{
    public sealed class Station : MonoBehaviour
    {
        // for relevance (this code is wrong and very buggy, gonna re-write)
        [SerializeField] private Vector3 m_cell = new Vector3(0.1f, 0.1f, 0.1f);
        [SerializeField] private Vector3 m_offset = Vector3.zero;
        [SerializeField] private Vector3 m_size = Vector3.one;

        public Bounds bounds => new Bounds(transform.position + m_offset, m_size);
        public Vector3Int cells => new Vector3Int(
                Mathf.RoundToInt(Mathf.Max(0, m_size.x / m_cell.x)),
                Mathf.RoundToInt(Mathf.Max(0, m_size.y / m_cell.y)),
                Mathf.RoundToInt(Mathf.Max(0, m_size.z / m_cell.z)));

        internal Instance n_attached;

        private void Awake()
        {
            n_attached = GetComponent<Instance>();
            Central.n_stations.Add(this);
        }

        private void OnDestroy()
        {
            Central.n_stations.Remove(this);
        }

        private void OnValidate()
        {
            if (m_cell.x > m_size.x)
                m_cell.x = m_size.x * .9f;
            if (m_cell.y > m_size.y)
                m_cell.y = m_size.y * .9f;
            if (m_cell.z > m_size.z)
                m_cell.z = m_size.z * .9f;
        }

        private int GetValue(float value, float size)
        {
            return Mathf.RoundToInt(value / size);
        }

        private Vector3Int GetIndex(Vector3 vector)
        {
            return new Vector3Int(GetValue(vector.x, m_cell.x), GetValue(vector.y, m_cell.y), GetValue(vector.z, m_cell.z));
        }

        private Vector3Int GetCell(Vector3 point)
        {
            return GetIndex(point - transform.position);
        }

        private Vector3 GetCenter(Vector3Int cell)
        {
            return transform.position + (Vector3.right * m_cell.x * cell.x) + (Vector3.up * m_cell.y * cell.y) + (Vector3.forward * m_cell.z * cell.z);
        }

        private bool Contains(Vector3 position)
        {
            return bounds.Contains(position);
        }

        public bool Overlap(Vector3 a, Vector3 b)
        {
            if (Contains(a) && Contains(b))
            {
                // a and b should be on the same cell within the station
                return GetCell(a) == GetCell(b);
            }
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            // Vector3Int bound = cells;
            // Gizmos.color = Color.cyan;
            // for (int x = -bound.x; x <= bound.y; x++)
            // {
            //     for (int y = -bound.y; y <= bound.z; y++)
            //     {
            //         for (int z = -bound.z; z <= bound.z; z++)
            //         {
            //             Gizmos.DrawWireCube(GetCenter(new Vector3Int(x, y, z)), m_cell);
            //         }
            //     }
            // }

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + m_offset, m_size);
        }
    }
}