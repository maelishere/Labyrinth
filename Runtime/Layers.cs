using System;
using System.Collections.Generic;

namespace Labyrinth.Runtime
{
    [Serializable]
    public struct Layers : IEquatable<Layers>
    {
        public static readonly Layers All = new Layers();

        public Bits[] Values;

        public Layers(int layers)
        {
            Values = new Bits[layers];
        }

        public Layers(Bits[] layers)
        {
            Values = layers;
        }

        public bool this[int layer, int index]
        {
            get
            {
                if (Values == null)
                    throw new InvalidOperationException($"Layers values is null");
                else if (layer > Values.Length - 1)
                    throw new ArgumentOutOfRangeException($"Layers only contains {Values.Length} values: {layer} is over");
                else if (layer < 0)
                    throw new ArgumentOutOfRangeException($"Negative layer");

                return Values[layer][index];
            }

            set
            {
                if (Values == null)
                    throw new InvalidOperationException($"Layers values is null");
                else if (layer > Values.Length - 1)
                    throw new ArgumentOutOfRangeException($"Layers only contains {Values.Length} values: {layer} is over");
                else if (layer < 0)
                    throw new ArgumentOutOfRangeException($"Negative layer");

                Values[layer][index] = value;
            }
        }

        public bool Equals(Layers other)
        {
            if (Values != null && other.Values != null)
            {
                if (Values.Length != 0 && other.Values.Length != 0)
                {
                    for (int i = 0; i < Values.Length && i < other.Values.Length; i++)
                    {
                        if (Values[i].Equals(other.Values[i]))
                            return true;
                    }
                    return false;
                }
            }
            // null or size 0 indicates everything
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == GetType())
                return Equals((Layers)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return 1291433875 + EqualityComparer<Bits[]>.Default.GetHashCode(Values);
        }

        public override string ToString()
        {
            string output = "";
            foreach (var value in Values)
                output += $"{value}";
            return output;
        }

        public static bool operator ==(Layers a, Layers b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Layers a, Layers b)
        {
            return !a.Equals(b);
        }
    }
}