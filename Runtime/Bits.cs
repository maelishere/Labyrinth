using System;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    [Serializable]
    public struct Bits : IEquatable<Bits>
    {
        public const int Length = 8;

        public string Name;
        public byte Value;

        public Bits(string name, byte value)
        {
            Name = name;
            Value = value;
        }

        public bool this[int index]
        {
            get
            {
                if (index > Length - 1)
                    throw new ArgumentOutOfRangeException($"Mask only contains {Length} bits: {index} is over");
                else if (index < 0)
                    throw new ArgumentOutOfRangeException($"Negative index");

                return (Value & (1 << index)) != 0;
            }

            set
            {
                if (index > Length - 1)
                    throw new ArgumentOutOfRangeException($"Mask only contains {Length} bits: {index} is over");
                else if (index < 0)
                    throw new ArgumentOutOfRangeException($"Negative index");

                if (value)
                    Value = (byte)(Value | (1 << index));
                else
                    Value = (byte)(Value & (1 << index));
            }
        }

        public bool Equals(Bits other)
        {
            if (Name == other.Name)
            {
                return (Value & other.Value) != 0;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == GetType())
                return Equals((Bits)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            int hashCode = -244751520;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"[{Name}, ({Convert.ToString(Value, 2).PadLeft(Length, '0')})]";
        }

        public static bool operator ==(Bits a, Bits b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Bits a, Bits b)
        {
            return !a.Equals(b);
        }
    }
}