using UnityEditor;
using UnityEngine;

namespace Labyrinth
{
    [System.Serializable]
    public struct Context
    {
        public string Name;
        public BuildTargetGroup Group;
        public BuildTarget Target;
        public BuildOptions Options;
        public string[] Definitions;

        public Context(string name, BuildTargetGroup group, BuildTarget target, BuildOptions options, string[] definitions)
        {
            Name = name;
            Group = group;
            Target = target;
            Options = options;
            Definitions = definitions;
        }
    }
}