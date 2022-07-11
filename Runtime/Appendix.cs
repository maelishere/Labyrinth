using System;
using UnityEngine;

namespace Labyrinth.Runtime
{
    using Bolt;

    [RequireComponent(typeof(Instance))]
    public class Appendix : MonoBehaviour
    {
        [Serializable]
        public struct Variable
        {
            public float Rate;
            public Signature.Rule Control;
            public Relevancy Relevancy;

            public Variable(float rate, Signature.Rule control, Relevancy relevancy)
            {
                Rate = rate;
                Control = control;
                Relevancy = relevancy;
            }
        }

        [Serializable]
        public struct Function
        {
            public Procedure.Rule Control;
            public Relevancy Relevancy;

            public Function(Procedure.Rule control, Relevancy relevancy)
            {
                Control = control;
                Relevancy = relevancy;
            }
        }

        internal byte n_offset;
        internal Instance n_network;

        public int identity => n_network.identity.Value;
        public int authority => n_network.authority.Value;
        public bool owner => Network.Authority(authority);

        public bool Var<T>(byte signature, float rate, Signature.Rule control, Relevancy relevancy, Func<T> get, Action<T> set) where T : struct
        {
            return n_network.Register(n_offset,
                new Signature(signature, rate, control, relevancy,
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

        public bool Var<T>(byte signature, float rate, Signature.Rule control, Relevance relevance, Layers layers, Func<T> get, Action<T> set) where T : struct
        {
            return Var(signature, rate, control, new Relevancy(relevance, layers), get, set);
        }

        public bool Var<T>(byte signature, float rate, Signature.Rule control, Relevance relevance, Func<T> get, Action<T> set) where T : struct
        {
            return Var(signature, rate, control, new Relevancy(relevance), get, set);
        }

        public bool Var<T>(byte signature, Variable variable, Func<T> get, Action<T> set) where T : struct
        {
            return Var(signature, variable.Rate, variable.Control, variable.Relevancy, get, set);
        }

        public bool Method(byte procedure, Procedure.Rule control, Relevancy relevancy, Action method)
        {
            return n_network.Register(n_offset,
                new Procedure(procedure, control, relevancy,
                (ref Reader reader) =>
                {
                    method?.Invoke();
                }));
        }

        public bool Method(byte procedure, Procedure.Rule control, Relevance relevance, Layers layers, Action method)
        {
            return Method(procedure, control, new Relevancy(relevance, layers), method);
        }

        public bool Method(byte procedure, Procedure.Rule control, Relevance relevance, Action method)
        {
            return Method(procedure, control, new Relevancy(relevance), method);
        }

        public bool Method(byte procedure, Function function, Action method)
        {
            return Method(procedure, function.Control, function.Relevancy, method);
        }

        public bool Method<T>(byte procedure, Procedure.Rule control, Relevancy relevancy, Action<T> method)
        {
            return n_network.Register(n_offset, 
                new Procedure(procedure, control, relevancy,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T>());
                }));
        }

        public bool Method<T>(byte procedure, Procedure.Rule control, Relevance relevance, Layers layers, Action<T> method)
        {
            return Method(procedure, control, new Relevancy(relevance, layers), method);
        }

        public bool Method<T>(byte procedure, Procedure.Rule control, Relevance relevance, Action<T> method)
        {
            return Method(procedure, control, new Relevancy(relevance), method);
        }

        public bool Method<T>(byte procedure, Function function, Action<T> method)
        {
            return Method(procedure, function.Control, function.Relevancy, method);
        }

        public bool Method<T1, T2>(byte procedure, Procedure.Rule control, Relevancy relevancy, Action<T1, T2> method)
        {
            return n_network.Register(n_offset, 
                new Procedure(procedure, control, relevancy,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T1>(), reader.Read<T2>());
                }));
        }

        public bool Method<T1, T2>(byte procedure, Procedure.Rule control, Relevance relevance, Layers layers, Action<T1, T2> method)
        {
            return Method(procedure, control, new Relevancy(relevance, layers), method);
        }

        public bool Method<T1, T2>(byte procedure, Procedure.Rule control, Relevance relevance, Action<T1, T2> method)
        {
            return Method(procedure, control, new Relevancy(relevance), method);
        }

        public bool Method<T1, T2>(byte procedure, Function function, Action<T1, T2> method)
        {
            return Method(procedure, function.Control, function.Relevancy, method);
        }

        public bool Method<T1, T2, T3>(byte procedure, Procedure.Rule control, Relevancy relevancy, Action<T1, T2, T3> method)
        {
            return n_network.Register(n_offset, 
                new Procedure(procedure, control, relevancy,
                (ref Reader reader) =>
                {
                    method?.Invoke(reader.Read<T1>(), reader.Read<T2>(), reader.Read<T3>());
                }));
        }

        public bool Method<T1, T2, T3>(byte procedure, Procedure.Rule control, Relevance relevance, Layers layers, Action<T1, T2, T3> method)
        {
            return Method(procedure, control, new Relevancy(relevance, layers), method);
        }

        public bool Method<T1, T2, T3>(byte procedure, Procedure.Rule control, Relevance relevance, Action<T1, T2, T3> method)
        {
            return Method(procedure, control, new Relevancy(relevance), method);
        }

        public bool Method<T1, T2, T3>(byte procedure, Function function, Action<T1, T2, T3> method)
        {
            return Method(procedure, function.Control, function.Relevancy, method);
        }

        public void RPC(byte channel, byte procedure)
        {
            n_network.Remote(Identity.Any, channel, n_offset, procedure, null);
        }

        public void RPC<T>(byte channel, byte procedure, T arg)
        {
            n_network.Remote(Identity.Any, channel, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg);
                });
        }

        public void RPC<T1, T2>(byte channel, byte procedure, T1 arg1, T2 arg2)
        {
            n_network.Remote(Identity.Any, channel, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                });
        }

        public void RPC<T1, T2, T3>(byte channel, byte procedure, T1 arg1, T2 arg2, T3 arg3)
        {
            n_network.Remote(Identity.Any, channel, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                    writer.Write(arg3);
                });
        }

        public void RPC(int connection, byte channel, byte procedure)
        {
            n_network.Remote(connection, channel, n_offset, procedure,null);
        }

        public void RPC<T>(int connection, byte channel, byte procedure, T arg)
        {
            n_network.Remote(connection, channel, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg);
                });
        }

        public void RPC<T1, T2>(int connection, byte channel, byte procedure, T1 arg1, T2 arg2)
        {
            n_network.Remote(connection, channel, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                });
        }

        public void RPC<T1, T2, T3>(int connection, byte channel, byte procedure, T1 arg1, T2 arg2, T3 arg3)
        {
            n_network.Remote(connection, channel, n_offset, procedure,
                (ref Writer writer) =>
                {
                    writer.Write(arg1);
                    writer.Write(arg2);
                    writer.Write(arg3);
                });
        }
    }
}