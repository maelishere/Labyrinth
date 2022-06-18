using UnityEngine;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    // server authority script
    public sealed class Station : MonoBehaviour
    {
        // for relevance
        [SerializeField] private float m_cell = 100;
        [SerializeField] private Vector3 m_offset = Vector3.zero;
        [SerializeField] private Vector3 m_size = Vector3.one;

        public Bounds bounds => new Bounds(transform.position + m_offset, m_size);

        private void Start()
        {
            /*Vector3Int bound = new Vector3Int(
                Mathf.RoundToInt(Mathf.Max(0, m_size.x / m_cell)),
                Mathf.RoundToInt(Mathf.Max(0, m_size.y / m_cell)),
                Mathf.RoundToInt(Mathf.Max(0, m_size.z / m_cell)));
            Vector3Int count = new Vector3Int(
                (bound.x + bound.x + 1) * (bound.x + bound.x + 1),
                (bound.y + bound.y + 1) * (bound.y + bound.y + 1),
                (bound.z + bound.z + 1) * (bound.z + bound.z + 1));*/
        }

        private int GetValue(float value)
        {
            return Mathf.RoundToInt(value / m_cell);
        }

        public Vector3Int GetCell(Vector3 point)
        {
            return new Vector3Int(GetValue(point.x), GetValue(point.y), GetValue(point.z));
        }

        public Vector3 GetCenter(Vector3Int cell)
        {
            return transform.position + (Vector3.right * m_cell * cell.x) + (Vector3.up * m_cell * cell.y) + (Vector3.forward * m_cell * cell.z);
        }

        public bool Contains(Vector3 position)
        {
            return bounds.Contains(position);
        }
    }
}