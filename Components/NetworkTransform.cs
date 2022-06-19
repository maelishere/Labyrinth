using UnityEngine;
using System.Collections;

namespace Labyrinth.Components
{
    using Labyrinth.Runtime;

    [RequireComponent(typeof(Entity))]
    [AddComponentMenu("Labyrinth/Components/Network Transform")]
    public class NetworkTransform : Appendix
    {
        [SerializeField] private int m_rate = 10;
        [SerializeField] private float m_smoothing = 3;

        private Instance m_parent;
        private Vector3 m_position, m_rotation, m_scale;

        private void Awake()
        {
            Method<int>(1, Procedure.Rule.Any, OnNetworkParent);

            Var(2, m_rate, Signature.Rule.Round, Relevance.Authority,
                () =>
                {
                    return transform.position;
                },
                (Vector3 position) =>
                {
                    m_position = position;
                });

            Var(3, m_rate, Signature.Rule.Round, Relevance.Authority,
                () =>
                {
                    return transform.rotation.eulerAngles;
                },
                (Vector3 rotation) =>
                {
                    m_rotation = rotation;
                });

            Var(3, m_rate, Signature.Rule.Round, Relevance.Authority,
                () =>
                {
                    return transform.localScale;
                },
                (Vector3 scale) =>
                {
                    m_scale = scale;
                });

            StartCoroutine(CheckParent());
        }

        private IEnumerator CheckParent()
        {
            while (Network.Internal(Host.Any))
            {
                if (m_parent?.transform ?? null != transform.parent)
                {
                    m_parent = transform.parent.GetComponent<Instance>();
                    RPC(1, m_parent?.identity.Value ?? Identity.Any);
                }
                yield return new WaitForSecondsRealtime(1);
            }
        }

        private void OnNetworkParent(int instance)
        {
            if (Instance.Find(instance, out Instance parent))
            {
                m_parent = parent;
            }
        }

        private void Update()
        {
            if (!owner)
            {
                transform.SetParent(m_parent?.transform ?? null);
                transform.position = Vector3.Lerp(transform.position, m_position, m_smoothing * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(m_rotation), m_smoothing * Time.deltaTime);
                transform.localScale = Vector3.Lerp(transform.localScale, m_scale, m_smoothing * Time.deltaTime);
            }
        }
    }
}