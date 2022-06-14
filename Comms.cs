using System.Collections.Generic;

namespace Labyrinth
{
    public class Comms
    {
        public int Primary { get; }
        public readonly HashSet<int> Secondary = new HashSet<int>();

        public Comms(int primary)
        {
            Primary = primary;
        }
    }
}