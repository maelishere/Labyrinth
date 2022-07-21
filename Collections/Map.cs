using System.Collections.Generic;

namespace Labyrinth.Collections
{
    // Network SortedDictionary Equivalent
    public class Map<TKey, TValue> : Glossary<TKey, TValue>
    {
        public Map() : base(new SortedDictionary<TKey, TValue>()) { }
        public Map(IComparer<TKey> comparer) : base(new SortedDictionary<TKey, TValue>(comparer ?? Comparer<TKey>.Default)) { }
    }
}