using UnityEditor;
using UnityEngine;

namespace Labyrinth
{
    [System.Serializable]
    public struct Context
    {
        public string Name;
        public BuildTarget Target;
        public BuildOptions Options;
        public string[] Definitions;

        public Context(string name, BuildTarget target, BuildOptions options, string[] definitions)
        {
            Name = name;
            Target = target;
            Options = options;
            Definitions = definitions;
        }
    }
}