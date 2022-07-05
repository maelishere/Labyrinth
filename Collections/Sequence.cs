using System.Collections.Generic;

namespace Labyrinth.Collections
{
    // Network Sorted Set Equivalent
    public class Sequence<T> : Series<T>
    {
        /*private readonly IComparer<T> m_comparer;*/

        public Sequence() : this(Comparer<T>.Default) { }
        public Sequence(IComparer<T> comparer) : base(null, new SortedSet<T>(comparer ?? Comparer<T>.Default)) 
        {
            /*m_comparer = comparer;*/
        }
    }
}