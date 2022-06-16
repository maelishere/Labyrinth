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

        public bool Var<T>(byte signature, int rate, Rule control, bool relevance, Func<T> get, Action<T> set)
        {
            return m_network.Register(m_offset,
                new Signature(signature, rate, control, relevance,
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

        public void RPC(byte procedure, Host host = Host.Any)
        {
            m_network.Remote(Identity.Any, m_offset, procedure, null);
        }

        public void RPC<T>(byte procedure, T arg, Host host = Host.Any)
        {
            m_network.Remote(Identity.Any, m_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg);
                });
        }

        public void RPC<T1, T2>(byte procedure, T1 arg1, T2 arg2, Host host = Host.Any)
        {
            m_network.Remote(Identity.Any, m_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                });
        }

        public void RPC<T1, T2, T3>(byte procedure, T1 arg1, T2 arg2, T3 arg3, Host host = Host.Any)
        {
            m_network.Remote(Identity.Any, m_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                    writer.Write(arg3);
                });
        }

        public void RPC(int connection, byte procedure, Host host = Host.Any)
        {
            m_network.Remote(connection, m_offset, procedure, null);
        }

        public void RPC<T>(int connection, byte procedure, T arg, Host host = Host.Any)
        {
            m_network.Remote(connection, m_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg);
                });
        }

        public void RPC<T1, T2>(int connection, byte procedure, T1 arg1, T2 arg2, Host host = Host.Any)
        {
            m_network.Remote(connection, m_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                });
        }

        public void RPC<T1, T2, T3>(int connection, byte procedure, T1 arg1, T2 arg2, T3 arg3, Host host = Host.Any)
        {
            m_network.Remote(connection, m_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                    writer.Write(arg3);
                });
        }
    }
}