using UnityEngine;

namespace Labyrinth.Components
{
    using Labyrinth.Runtime;

    [RequireComponent(typeof(Entity))]
    public class NetworkTransform : Appendix
    {
        [SerializeField] private int m_rate = 10;
        [SerializeField] private bool m_relevance = true;
        [SerializeField] private float m_snapThreshold = 3;

        /*public bool Parent, Position, Rotation, Scale;*/

        private Instance m_parent;
        private Vector3 m_position, m_rotation, m_scale;

        private void Awake()
        {
            Var(1, m_rate, Signature.Rule.Round, m_relevance,
                () =>
                {
                    return m_parent.identity.Value;
                },
                (int parent) =>
                {
                    if (Instance.Find(parent, out Instance instance))
                    {
                        m_parent = instance;
                    }
                });

            Var(2, m_rate, Signature.Rule.Round, m_relevance,
                () =>
                {
                    return transform.position;
                },
                (Vector3 position) =>
                {
                    m_position = position;
                });

            Var(3, m_rate, Signature.Rule.Round, m_relevance,
                () =>
                {
                    return transform.rotation.eulerAngles;
                },
                (Vector3 rotation) =>
                {
                    m_rotation = rotation;
                });

            Var(3, m_rate, Signature.Rule.Round, m_relevance,
                () =>
                {
                    return transform.lossyScale;
                },
                (Vector3 scale) =>
                {
                    m_scale = scale;
                });
        }

        private void Update()
        {
            //if network isn't the server
            if (Network.Authority(true) != authority)
            {

            }
        }
    }
}