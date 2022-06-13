using System;
using System.Security.Cryptography;

namespace Labyrinth
{
    public struct Instance : IIdentifier<int>, IEquatable<int>, IEquatable<Instance>
    {
        private static readonly RNGCryptoServiceProvider m_randomiser = new RNGCryptoServiceProvider();

        public Instance(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public static Instance Generate(Func<int, bool> filter)
        {
            int value = 0;
            do
            {
                byte[] buffer = new byte[sizeof(int)];
                m_randomiser.GetNonZeroBytes(buffer);
                value = BitConverter.ToInt32(buffer, 0);
            }
            while (filter?.Invoke(value) ?? false || value == 0);
            return new Instance(value);
        }

        public bool Equals(int value)
        {
            return Value == value;
        }

        public bool Equals(Instance other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object other)
        {
            if (other.GetType() == GetType())
                return Equals((Instance)other);
            else if (other.GetType() == Value.GetType())
                return Equals((int)other);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"Instance[{Value}]";
        }
    }
}