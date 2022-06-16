using System.Collections.Generic;

namespace Labyrinth
{
    public class Comms
    {
        public int Primary;
        public HashSet<int> Secondary;

        public Comms()
        {
            Secondary = new HashSet<int>();
        }
    }
}