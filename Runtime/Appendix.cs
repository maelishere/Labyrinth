using System;
using UnityEngine;

namespace Labyrinth.Runtime
{
    using Bolt;

    [RequireComponent(typeof(Instance))]
    public class Appendix : MonoBehaviour
    {
        [Serializable]
        public struct Variable<T> where T : struct
        {
            public int Rate;
            public bool Relevance;
            [HideInInspector] public T State;
        }

        internal byte n_offset;
        internal Instance n_network;

        public int identity => n_network.identity.Value;
        public int authority => n_network.authority.Value;
        public bool owner => authority == Network.Authority();

        public bool Var<T>(byte signature, int rate, Signature.Rule control, bool relevance, Func<T> get, Action<T> set)
        {
            return n_network.Register(n_offset,
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

        public bool Method(byte procedure, Procedure.Rule control, Action method)
        {
            return n_network.Register(n_offset, 
                new Procedure(procedure, control,
                (ref Reader reader) =>
                {
                    method?.Invoke();
                }));
        }

        public bool Method<T>(byte procedure, Procedure.Rule control, Action<T> method)
        {
            return n_network.Register(n_offset, 
                new Procedure(procedure, control,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T>());
                }));
        }

        public bool Method<T1, T2>(byte procedure, Procedure.Rule control, Action<T1, T2> method)
        {
            return n_network.Register(n_offset, 
                new Procedure(procedure, control,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T1>(), reader.Read<T2>());
                }));
        }

        public bool Method<T1, T2, T3>(byte procedure, Procedure.Rule control, Action<T1, T2, T3> method)
        {
            return n_network.Register(n_offset, 
                new Procedure(procedure, control,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T1>(), reader.Read<T2>(), reader.Read<T3>());
                }));
        }

        public void RPC(byte procedure, Host host = Host.Any)
        {
            n_network.Remote(Identity.Any, n_offset, procedure, null);
        }

        public void RPC<T>(byte procedure, T arg, Host host = Host.Any)
        {
            n_network.Remote(Identity.Any, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg);
                });
        }

        public void RPC<T1, T2>(byte procedure, T1 arg1, T2 arg2, Host host = Host.Any)
        {
            n_network.Remote(Identity.Any, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                });
        }

        public void RPC<T1, T2, T3>(byte procedure, T1 arg1, T2 arg2, T3 arg3, Host host = Host.Any)
        {
            n_network.Remote(Identity.Any, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                    writer.Write(arg3);
                });
        }

        public void RPC(int connection, byte procedure, Host host = Host.Any)
        {
            n_network.Remote(connection, n_offset, procedure, null);
        }

        public void RPC<T>(int connection, byte procedure, T arg, Host host = Host.Any)
        {
            n_network.Remote(connection, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg);
                });
        }

        public void RPC<T1, T2>(int connection, byte procedure, T1 arg1, T2 arg2, Host host = Host.Any)
        {
            n_network.Remote(connection, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                });
        }

        public void RPC<T1, T2, T3>(int connection, byte procedure, T1 arg1, T2 arg2, T3 arg3, Host host = Host.Any)
        {
            n_network.Remote(connection, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                    writer.Write(arg3);
                });
        }
    }
}