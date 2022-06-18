using UnityEngine;

namespace Labyrinth.Components
{
    using Labyrinth.Runtime;

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Entity))]
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

            public static Parameter Create<T>(string name, AnimatorControllerParameterType type) where T : struct
            {
                return new Parameter(name, type, default);
            }
        }

        [SerializeField] private int m_rate = 10;
        [SerializeField] private bool m_relevance = true;
        [SerializeField] private float m_smoothing = 10.0f;

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
                            m_parameters[i] = Parameter.Create<int>(parameters[i].name, parameters[i].type);

                            Var(i, m_rate, Signature.Rule.Server, m_relevance,
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
                            m_parameters[i] = Parameter.Create<float>(parameters[i].name, parameters[i].type);

                            Var(i, m_rate, Signature.Rule.Server, m_relevance,
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
                            m_parameters[i] = Parameter.Create<bool>(parameters[i].name, parameters[i].type);

                            Var(i, m_rate, Signature.Rule.Server, m_relevance,
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
            //if network isn't the server
            if (Network.Authority(true) != authority)
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
