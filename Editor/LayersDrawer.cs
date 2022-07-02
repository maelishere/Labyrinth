#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using System.Collections.Generic;

namespace Labyrinth.Editor
{
    using Labyrinth.Runtime;

    /// url: https://gist.github.com/EddieCameron/6c3d03732aecb2560cbfd9b82afe0199
    [CustomPropertyDrawer(typeof(Layers))]
    public class LayersDrawer : PropertyDrawer
    {
        private Dictionary<string, ReorderableList> m_lists = new Dictionary<string, ReorderableList>();

        private ReorderableList Find(SerializedProperty property)
        {
            SerializedProperty listProperty = property.FindPropertyRelative("Values");

            ReorderableList list;
            if (m_lists.TryGetValue(listProperty.propertyPath, out list))
            {
                return list;
            }

            list = new ReorderableList(listProperty.serializedObject, listProperty, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);
            m_lists[listProperty.propertyPath] = list;

            list.drawHeaderCallback += rect => {
                EditorGUI.LabelField(rect, property.displayName);
            };

            list.drawElementCallback += (rect, index, isActive, isFocused) => {
                SerializedProperty elementProp = list.serializedProperty.GetArrayElementAtIndex(index);
                if (elementProp.hasVisibleChildren)
                {
                    EditorGUI.PropertyField(rect, elementProp, includeChildren: true);
                }
                else
                {
                    EditorGUI.PropertyField(rect, elementProp, includeChildren: true, label: GUIContent.none);   // dont draw label if its a single line
                }
            };

            list.elementHeightCallback += idx => {
                SerializedProperty elementProp = listProperty.GetArrayElementAtIndex(idx);
                return EditorGUI.GetPropertyHeight(elementProp);
            };

            return list;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Find(property).GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Find(property).DoList(position);

            EditorGUI.EndProperty();
        }
    }
}
#endif