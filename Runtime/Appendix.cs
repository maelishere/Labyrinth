using System;
using UnityEngine;

namespace Labyrinth.Runtime
{
    using Bolt;

    [RequireComponent(typeof(Instance))]
    public abstract class Appendix : MonoBehaviour
    {
        internal byte m_offset;
        internal Instance m_network;

        private bool Constraint(Local host)
        {
            /*if (NetworkHost.Running)
            {
                if (NetworkHost.Constraint(host))*/
            return true;
            /*}
            else
            {
                notRunning?.Invoke();
            }*/
            /*return false;*/
        }

        public bool Var<T>(byte signature, int sync, Rule control, bool relevance, Func<T> get, Action<T> set)
        {
            return m_network.Register(m_offset,
                new Signature(signature, sync, control, relevance,
                (ref Writer writer) =>
                {
                    if (get != null)
                    {
                        writer.Write(get());
                    }
                },
                (ref Reader reader) =>
                {
                    set?.Invoke(reader.Read<T>());
                }));
        }

        public bool Method(byte procedure, Action method)
        {
            return m_network.Register(m_offset, 
                new Procedure(procedure,
                (ref Reader reader) =>
                {
                    method?.Invoke();
                }));
        }

        public bool Method<T>(byte procedure, Action<T> method)
        {
            return m_network.Register(m_offset, 
                new Procedure(procedure,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T>());
                }));
        }

        public bool Method<T1, T2>(byte procedure, Action<T1, T2> method)
        {
            return m_network.Register(m_offset, 
                new Procedure(procedure,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T1>(), reader.Read<T2>());
                }));
        }

        public bool Method<T1, T2, T3>(byte procedure, Action<T1, T2, T3> method)
        {
            return m_network.Register(m_offset, 
                new Procedure(procedure,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T1>(), reader.Read<T2>(), reader.Read<T3>());
                }));
        }

        public void RPC(byte procedure, Local host = Local.Any, Action notRunning = null)
        {
            if (Constraint(host))
            {
                /*m_network.RemoteCall(signature, null);*/
            }
            else
            {
                notRunning?.Invoke();
            }
        }

        public void RPC<T>(byte procedure, T arg, Local host = Local.Any, Action<T> notRunning = null)
        {
            if (Constraint(host))
            {
                /*m_network.RemoteCall(signature,
                    (ref Writer writer) =>
                    {
                        writer.Write(arg);
                    });*/
            }
            else
            {
                notRunning?.Invoke(arg);
            }
        }

        public void RPC<T1, T2>(byte procedure, T1 arg1, T2 arg2, Local host = Local.Any, Action<T1, T2> notRunning = null)
        {
            if (Constraint(host))
            {
                /*m_network.RemoteCall(signature,
                    (ref BufferWriter writer) =>
                    {
                        writer.Write(arg1);
                        writer.Write(arg2);
                    });*/
            }
            else
            {
                notRunning?.Invoke(arg1, arg2);
            }
        }

        public void RPC<T1, T2, T3>(byte procedure, T1 arg1, T2 arg2, T3 arg3, Local host = Local.Any, Action<T1, T2, T3> notRunning = null)
        {
            if (Constraint(host))
            {
                /*m_network.RemoteCall(signature,
                    (ref BufferWriter writer) =>
                    {
                        writer.Write(arg1);
                        writer.Write(arg2);
                        writer.Write(arg3);
                    });*/
            }
            else
            {
                notRunning?.Invoke(arg1, arg2, arg3);
            }
        }

        public void RPC(int connection, int signature, Local host = Local.Any, Action notRunning = null)
        {
            if (Constraint(host))
            {
                /*m_network.RemoteCallTarget(connection, signature, null);*/
            }
            else
            {
                notRunning?.Invoke();
            }
        }

        public void RPC<T>(int connection, int signature, T arg, Local host = Local.Any, Action<T> notRunning = null)
        {
            if (Constraint(host))
            {
                /*m_network.RemoteCallTarget(connection, signature,
                    (ref BufferWriter writer) =>
                    {
                        writer.Write(arg);
                    });*/
            }
            else
            {
                notRunning?.Invoke(arg);
            }
        }

        public void RPC<T1, T2>(int connection, int signature, T1 arg1, T2 arg2, Local host = Local.Any, Action<T1, T2> notRunning = null)
        {
            if (Constraint(host))
            {
                /*m_network.RemoteCallTarget(connection, signature,
                    (ref BufferWriter writer) =>
                    {
                        writer.Write(arg1);
                        writer.Write(arg2);
                    });*/
            }
            else
            {
                notRunning?.Invoke(arg1, arg2);
            }
        }

        public void RPC<T1, T2, T3>(int connection, int signature, T1 arg1, T2 arg2, T3 arg3, Local host = Local.Any, Action<T1, T2, T3> notRunning = null)
        {
            if (Constraint(host))
            {
                /*m_network.RemoteCallTarget(connection, signature,
                    (ref BufferWriter writer) =>
                    {
                        writer.Write(arg1);
                        writer.Write(arg2);
                        writer.Write(arg3);
                    });*/
            }
            else
            {
                notRunning?.Invoke(arg1, arg2, arg3);
            }
        }
    }
}