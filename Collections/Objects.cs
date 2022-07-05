using System;
using System.Collections.Generic;

namespace Labyrinth.Collections
{
    using Bolt;

    public static class Objects
    {
        public const byte Find = 9;
        public const byte Link = 10;
        public const byte Reset = 11;
        public const byte Modify = 12;

        public struct Callbacks
        {
            public Callbacks(Write clone, Read apply, Write copy, Read paste)
            {
                Clone = clone;
                Apply = apply;
                Copy = copy;
                Paste = paste;
            }

            public Write Clone { get; }
            public Read Apply { get; }
            public Write Copy { get; }
            public Read Paste { get; }
        }

        private static readonly Dictionary<int, Callbacks> m_callbacks = new Dictionary<int, Callbacks>();

        public static void Duplicate(int idenitifier)
        {
            
        }

        public static bool Register<T>(int idenitifier, T unit) where T : Unit
        {
            if (!m_callbacks.ContainsKey(idenitifier))
            {
                m_callbacks.Add(idenitifier, new Callbacks(unit.Clone, unit.Apply, unit.Copy, unit.Paste));
                unit.destructor = () =>
                {
                    m_callbacks.Remove(idenitifier);
                };
                return true;
            }
            return false;
        }

        internal static void Update()
        {

        }

        internal static void OnNetworkLink(int socket, int connection, uint timestamp, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        internal static void OnNetworkReset(int socket, int connection, uint timestamp, ref Reader reader)
        {
            throw new NotImplementedException();
        }

        internal static void OnNetworkModify(int socket, int connection, uint timestamp, ref Reader reader)
        {
            throw new NotImplementedException();
        }
    }
}