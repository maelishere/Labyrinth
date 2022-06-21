using UnityEngine;

namespace Labyrinth.Components
{
    using Labyrinth.Runtime;

    [RequireComponent(typeof(Entity))]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Labyrinth/Components/Network Rigidbody")]
    public class NetworkRigidbody : Appendix
    {
        [SerializeField] private int m_rate = 10;
        [SerializeField] private float m_smoothing = 10.0f;
        [SerializeField] private Relevance m_relevance = Relevance.General;

        private Rigidbody m_rigidbody;
        private Vector3 m_position, m_rotation;

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();

            Var(1, m_rate, Signature.Rule.Round, m_relevance,
                () =>
                {
                    return m_rigidbody.position;
                },
                (Vector3 position) =>
                {
                    m_position = position;
                });

            Var(2, m_rate, Signature.Rule.Round, m_relevance,
                () =>
                {
                    return m_rigidbody.rotation.eulerAngles;
                },
                (Vector3 rotation) =>
                {
                    m_rotation = rotation;
                });
        }

        private void FixedUpdate()
        {
            if (!owner)
            {
                m_rigidbody.MovePosition(Vector3.Lerp(m_rigidbody.position, m_position, m_smoothing * Time.deltaTime));
                m_rigidbody.MoveRotation(Quaternion.Lerp(m_rigidbody.rotation, Quaternion.Euler(m_rotation), m_smoothing * Time.deltaTime));
            }
        }
    }
}
