using UnityEngine;
using UnityEditor;

namespace Labyrinth.Editor
{
    using Labyrinth.Runtime;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Entity))]
    public class EntityEditor : InstanceEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}