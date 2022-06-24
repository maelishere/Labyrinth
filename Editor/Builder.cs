using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

using System.Collections.Generic;

namespace Labyrinth.Editor
{
    public class Builder : EditorWindow
    {
        private static string m_server_definition = "SERVER_BUILD";
        [SerializeField] private Context m_server_context;

        private static string m_client_definition = "CLIENT_BUILD";
        [SerializeField] private Context m_client_context;

        private int m_disabled;
        private ReorderableList scenes_list;
        private bool m_build, m_type;

        private string server_path => $"Build/{Application.version}/Server/{m_server_context.Target.ToString().Replace("Standalone", "")}/{m_server_context.Name}";
        private string client_path => $"Build/{Application.version}/Client/{m_client_context.Target.ToString().Replace("Standalone", "")}/{m_client_context.Name}";


        [MenuItem("Window/Labyrinth/Builder")]
        public static void Display()
        {
            Builder wnd = GetWindow<Builder>();
            wnd.titleContent = new GUIContent("Builder");
        }

        public void Awake()
        {
        }

        void OnEnable()
        {
            m_server_context = new Context($"{Application.productName}_Server.x86_64",
            BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode,
            new string[] {m_server_definition});

            m_client_context = new Context($"{Application.productName}_Client.exe",
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildOptions.None,
            new string[] {m_client_definition});

            string json = EditorPrefs.GetString("BuilderWindow", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(json, this);

            scenes_list = new ReorderableList(EditorBuildSettings.scenes, typeof(EditorBuildSettingsScene), true, true, true, true);
            scenes_list.drawElementCallback = DrawSceneItem; // Delegate to draw the elements on the list
            scenes_list.drawHeaderCallback = DrawSceneHeader;
        }

        private void OnDisable()
        {
            EditorPrefs.SetString("BuilderWindow", JsonUtility.ToJson(this, true));
        }

        private void DrawSceneItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect toggle = new Rect(rect.x, rect.y, rect.width * 0.05f, rect.height * 0.8f);
            EditorBuildSettings.scenes[index].enabled = EditorGUI.Toggle(toggle, EditorBuildSettings.scenes[index].enabled);

            if (!EditorBuildSettings.scenes[index].enabled)
            {
                m_disabled++;
            }

            var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[index].path);

            EditorGUI.BeginChangeCheck();

            Rect field = new Rect(toggle.x + toggle.width, rect.y, rect.width * 0.85f, rect.height * 0.8f);
            var newScene = EditorGUI.ObjectField(field, oldScene, typeof(SceneAsset), false) as SceneAsset;

            if (EditorGUI.EndChangeCheck())
            {
                var newPath = AssetDatabase.GetAssetPath(newScene);
                EditorBuildSettings.scenes[index].path = newPath;
            }

            Rect label = new Rect(field.x + field.width, rect.y, rect.width * 0.1f, rect.height * 0.8f);
            EditorGUI.LabelField(label, $"{index - m_disabled}");
        }

        //Draws the header
        private void DrawSceneHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Scenes");
        }

        private BuildPlayerOptions Get(string path, Context context)
        {
            BuildPlayerOptions player = new BuildPlayerOptions();

            List<string> scenes = new List<string>();
            foreach(var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            player.scenes = scenes.ToArray();

            player.locationPathName = path;
            player.targetGroup = context.Group;
            player.target = context.Target;
            player.options = context.Options;

            List<string> definitions = new List<string>();
            PlayerSettings.GetScriptingDefineSymbolsForGroup(context.Group, out string[] defines);
            definitions.AddRange(context.Definitions);
            definitions.AddRange(defines);
            player.extraScriptingDefines = definitions.ToArray();

            return player;
        }

        private void Update()
        {
            if (m_build)
            {
                if (m_type)
                {
                    BuildPipeline.BuildPlayer(Get(server_path, m_server_context));
                }
                else
                {
                    BuildPipeline.BuildPlayer(Get(client_path, m_client_context));
                }
                m_build = false;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build Settings");
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("You can only add scenes in the Build Settings window");
            EditorGUILayout.LabelField("Each scene should have the same index on both server and client");
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            m_disabled = 0;
            scenes_list?.DoLayoutList();

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Server Build");
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            m_server_context.Name = EditorGUILayout.TextField("Name", m_server_context.Name);
            m_server_context.Target = (BuildTarget)EditorGUILayout.EnumPopup("Target", m_server_context.Target);
            m_server_context.Options = (BuildOptions)EditorGUILayout.EnumFlagsField("Options", m_server_context.Options);
            // m_server_context.Definitions = EditorGUILayout.TextField("Defintions", m_server_definition + m_server_context.Definitions?.Replace(m_server_definition, "") ?? "");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Build Path: {server_path}");
            EditorGUILayout.Space();

            // server_def_list?.DoLayoutList();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Server"))
            {
                m_build = true;
                m_type = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Client Build");
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            m_client_context.Name = EditorGUILayout.TextField("Name", m_client_context.Name);
            m_client_context.Target = (BuildTarget)EditorGUILayout.EnumPopup("Target", m_client_context.Target);
            m_client_context.Options = (BuildOptions)EditorGUILayout.EnumFlagsField("Options", m_client_context.Options);
            // m_client_context.Definitions = EditorGUILayout.TextField("Defintions", m_client_definition + m_client_context.Definitions?.Replace(m_client_definition, "") ?? "");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Build Path: {client_path}");
            EditorGUILayout.Space();

            // client_def_list?.DoLayoutList();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Client"))
            {
                m_build = true;
                m_type = false;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
        }
    }
}