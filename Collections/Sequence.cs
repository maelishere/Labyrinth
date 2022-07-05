using System.Collections.Generic;

namespace Labyrinth.Collections
{
    // Network Sorted Set Equivalent
    public class Sequence<T> : Series<T>
    {
        public Sequence() : this(Comparer<T>.Default) { }
        public Sequence(IComparer<T> comparer) : base(new SortedSet<T>(comparer ?? Comparer<T>.Default)) 
        {
        }
    }
}