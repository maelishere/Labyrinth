using UnityEngine;

namespace Labyrinth.Runtime
{
    public sealed class Station : MonoBehaviour
    {
        [SerializeField] private float m_cell = 100;
        [SerializeField] private Vector3 m_size = Vector3.one;

        private Instance m_network = new Instance();

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

        public int GetValue(float value)
        {
            return Mathf.RoundToInt(value / m_cell);
        }

        public Vector3Int GetClosest(Vector3 point)
        {
            return new Vector3Int(GetValue(point.x), 0, GetValue(point.z));
        }

        public Vector3 GetCenter(Vector3Int index)
        {
            return transform.position + (Vector3.right * m_cell * index.x) + (Vector3.forward * m_cell * index.z);
        }

        public bool Contains(Vector3 position)
        {
            return new Bounds(transform.position, m_size).Contains(position);
        }
    }
}