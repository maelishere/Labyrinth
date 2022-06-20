using UnityEngine;
using UnityEditor;

namespace Labyrinth.Editor
{
    using Labyrinth.Runtime;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Central))]
    public class CentralEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Local: {Network.Authority()}", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Remote: {Network.Authority(true)}", EditorStyles.helpBox);
            EditorGUILayout.EndVertical();
        }
    }
}
