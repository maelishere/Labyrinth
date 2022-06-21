using UnityEngine;

namespace Labyrinth.Components
{
    using Labyrinth.Runtime;

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Entity))]
    [AddComponentMenu("Labyrinth/Components/Network Animator")]
    public class NetworkAnimator : Appendix
    {
        public struct Parameter
        {
            public string Name;
            public AnimatorControllerParameterType Type;
            public object State;

            public Parameter(string name, AnimatorControllerParameterType type, object state)
            {
                Name = name;
                Type = type;
                State = state;
            }
        }

        [SerializeField] private int m_rate = 10;
        [SerializeField] private float m_smoothing = 10.0f;
        [SerializeField] private Relevance m_relevance = Relevance.Sectors;

        private Animator m_animator;
        private Parameter[] m_parameters;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            AnimatorControllerParameter[] parameters = m_animator.parameters;
            if (parameters.Length < byte.MaxValue)
            {
                m_parameters = new Parameter[parameters.Length];
                for (byte i = 0; i < parameters.Length; i++)
                {
                    switch (parameters[i].type)
                    {
                        case AnimatorControllerParameterType.Int:
                            m_parameters[i] = new Parameter(parameters[i].name, parameters[i].type, (int)0);

                            Var(i, m_rate, Signature.Rule.Authority, m_relevance,
                                () =>
                                {
                                    return m_animator.GetInteger(m_parameters[i].Name);
                                },
                                (value) =>
                                {
                                    m_parameters[i].State = value;
                                });
                            break;

                        case AnimatorControllerParameterType.Float:
                            m_parameters[i] = new Parameter(parameters[i].name, parameters[i].type, 0.0f);

                            Var(i, m_rate, Signature.Rule.Authority, m_relevance,
                                () =>
                                {
                                    return m_animator.GetFloat(m_parameters[i].Name);
                                },
                                (value) =>
                                {
                                    m_parameters[i].State = value;
                                });
                            break;

                        case AnimatorControllerParameterType.Bool:
                            m_parameters[i] = new Parameter(parameters[i].name, parameters[i].type, false);

                            Var(i, m_rate, Signature.Rule.Authority, m_relevance,
                                () =>
                                {
                                    return m_animator.GetBool(m_parameters[i].Name);
                                },
                                (value) =>
                                {
                                    m_parameters[i].State = value;
                                });
                            break;
                    }
                }
                return;
            }
            Debug.LogError($"animator has too many parameters ({parameters.Length})");
        }

        private void Update()
        {
            if (!owner)
            {
                for (byte i = 0; i < m_parameters.Length; i++)
                {
                    switch (m_parameters[i].Type)
                    {
                        case AnimatorControllerParameterType.Int:
                            {
                                int value = (int)Mathf.Lerp(m_animator.GetInteger(m_parameters[i].Name), (int)m_parameters[i].State, m_smoothing * Time.deltaTime);
                                m_animator.SetInteger(m_parameters[i].Name, value);
                            }
                            break;

                        case AnimatorControllerParameterType.Float:
                            {
                                float value = Mathf.Lerp(m_animator.GetFloat(m_parameters[i].Name), (float)m_parameters[i].State, m_smoothing * Time.deltaTime);
                                m_animator.SetFloat(m_parameters[i].Name, value);
                            }
                            break;

                        case AnimatorControllerParameterType.Bool:
                            m_animator.SetBool(m_parameters[i].Name, (bool)m_parameters[i].State);
                            break;
                    }
                }
            }
        }
    }
}
