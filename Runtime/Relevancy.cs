using System;

namespace Labyrinth.Runtime
{
    [Serializable]
    public struct Relevancy
    {
        public Relevance Relevance;
        public Layers Layers;

        public Relevancy(Relevance relevance)
        {
            Relevance = relevance;
            Layers = Layers.All;
        }

        public Relevancy(Relevance relevance, Layers layers)
        {
            Relevance = relevance;
            Layers = layers;
        }
    }
}