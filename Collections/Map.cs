using System;
using System.Collections.Generic;
using System.Text;

namespace Labyrinth.Collections
{
    // Network SortedDictionary Set Equivalent
    public class Map<TKey, TValue> : Glossary<TKey, TValue>
    {
        public Map() : base(new SortedDictionary<TKey, TValue>()) { }
        public Map(IComparer<TKey> comparer) : base(new SortedDictionary<TKey, TValue>(comparer ?? Comparer<TKey>.Default)) { }
    }
}