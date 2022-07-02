/*#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Labyrinth.Editor
{
    using Labyrinth.Runtime;

    /*[CustomPropertyDrawer(typeof(Relevance))]
    public class RelevancyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect headerRect = new Rect(position.x, position.y, position.width, 20);
            EditorGUI.LabelField(headerRect, new GUIContent(property.displayName));

            Rect topRect = new Rect(position.x, position.y + 20, position.width, 20);
            EditorGUI.PropertyField(topRect, property.FindPropertyRelative("Relevance"), new GUIContent("Relevance"));

            Rect bottomRect = new Rect(position.x, position.y + 40, position.width, position.height - 40);
            EditorGUI.PropertyField(bottomRect, property.FindPropertyRelative("Layers"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
#endif */