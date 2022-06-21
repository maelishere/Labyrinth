using System;
using UnityEngine;

namespace Labyrinth.Runtime
{
    // for relevance (server side)
    [AddComponentMenu("Labyrinth/Sector")]
    public sealed class Sector : MonoBehaviour
    {
        [SerializeField] private Vector3 m_cell = Vector3.one;
        [SerializeField] private Vector3 m_offset = Vector3.zero;
        [SerializeField] private Vector3Int m_bound = Vector3Int.one;

        public Vector3 size => new Vector3(m_cell.x + (m_cell.x * m_bound.x * 2), 
                                           m_cell.y + (m_cell.y * m_bound.y * 2), 
                                           m_cell.z + (m_cell.z * m_bound.z * 2));
        public Vector3 center => transform.position + m_offset;
        public Bounds bounds => new Bounds(center, size);

        private void Awake()
        {
            Central.n_sectors.Add(this);
        }

        private void OnDestroy()
        {
            Central.n_sectors.Remove(this);
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
            return center + (Vector3.right * m_cell.x * cell.x) + (Vector3.up * m_cell.y * cell.y) + (Vector3.forward * m_cell.z * cell.z);
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
            Gizmos.color = Color.black;
            for (int x = -m_bound.x; x <= m_bound.x; x++)
            {
                for (int y = -m_bound.y; y <= m_bound.y; y++)
                {
                    for (int z = -m_bound.z; z <= m_bound.z; z++)
                    {
                        Gizmos.DrawWireCube(GetCenter(new Vector3Int(x, y, z)), m_cell);
                    }
                }
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, size);
        }
    }
}