using UnityEngine;

namespace Labyrinth.Components
{
    using Labyrinth.Runtime;

    [RequireComponent(typeof(Entity))]
    public class NetworkRigidbody : Appendix
    {
        [SerializeField] private int m_rate = 10;
        [SerializeField] private bool m_relevance = true;
        [SerializeField] private float m_smoothing = 10.0f;

        private Vector3 m_position, m_rotation;

        private void Awake()
        {
            Var(1, m_rate, Signature.Rule.Round, m_relevance,
                () =>
                {
                    return transform.position;
                },
                (Vector3 position) =>
                {
                    m_position = position;
                });

            Var(2, m_rate, Signature.Rule.Round, m_relevance,
                () =>
                {
                    return transform.rotation.eulerAngles;
                },
                (Vector3 rotation) =>
                {
                    m_rotation = rotation;
                });
        }

        private void FixedUpdate()
        {
            //if network isn't the server
            if (Network.Authority(true) != authority)
            {

            }
        }
    }
}
