using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class Builder : EditorWindow
{
    [MenuItem("Window/UI Toolkit/Builder")]
    public static void ShowExample()
    {
        Builder wnd = GetWindow<Builder>();
        wnd.titleContent = new GUIContent("Builder");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);
    }
}