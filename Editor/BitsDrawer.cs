#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace Labyrinth.Editor
{
    using Labyrinth.Runtime;

    [Flags]
    public enum BitsFlags
    {
        None = 0,
        Atto = 1 << 0,
        Pico = 1 << 1,
        Nano = 1 << 2,
        Mili = 1 << 3,
        Kilo = 1 << 4,
        Mega = 1 << 5,
        Giga = 1 << 6,
        Tera = 1 << 7,
        All = 255,
    }

    [CustomPropertyDrawer(typeof(Bits))]
    public class BitsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect nameRect = new Rect(position.x, position.y, position.width * 0.5f, position.height * 0.9f);
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("Name"), GUIContent.none);

            SerializedProperty value = property.FindPropertyRelative("Value");
            Rect bitsRect = new Rect(nameRect.x + nameRect.width, position.y, position.width * 0.5f, position.height);
            value.intValue = (byte)(BitsFlags)EditorGUI.EnumFlagsField(bitsRect, (BitsFlags)value.intValue);
            
            EditorGUI.EndProperty();
        }
    }
}
#endif