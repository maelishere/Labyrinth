#if UNITY_EDITOR
using UnityEditor;

namespace Labyrinth.Editor
{
    using Labyrinth.Runtime;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Instance))]
    public class InstanceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Instance instance = (Instance)target;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Identity: {instance.identity.Value}", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Authority: {instance.authority.Value}", EditorStyles.helpBox);
            EditorGUILayout.EndVertical();
        }
    }
}
#endif