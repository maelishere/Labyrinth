#if UNITY_EDITOR
using UnityEditor;

namespace Labyrinth.Editor
{
    using Labyrinth.Runtime;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(World))]
    public class WorldEditor : InstanceEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif