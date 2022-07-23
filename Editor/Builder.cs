#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

using System.Collections.Generic;
using System;

namespace Labyrinth.Editor
{
    public class Builder : EditorWindow
    {
        public enum Standalone
        {
            Windows64,
            Windows,
            Linux64,
            Linux,
            /*OSX64,
            OSX*/
        }

        //StandaloneBuildSubtarget
        [SerializeField] private string m_server_name;
        [SerializeField] private Standalone m_server_platform;
        [SerializeField] private BuildOptions m_server_options;
        [SerializeField] private List<string> m_server_definitions = new List<string>();
        [SerializeField] private string m_server_build_path = "Build/Server";

        [SerializeField] private string m_client_name;
        [SerializeField] private BuildTargetGroup m_client_group;
        [SerializeField] private BuildTarget m_client_target;
        [SerializeField] private BuildOptions m_client_options;
        [SerializeField] private List<string> m_client_definitions = new List<string>();
        [SerializeField] private string m_client_build_path = "Build/Client";

        private Vector2 m_scroll;
        private bool m_build, m_explore, m_server;
        private ReorderableList m_server_defs, m_client_defs;

        /*private string server_path => $"Build/{Application.version}/Server/{m_server_context.Target.ToString().Replace("Standalone", "")}/{m_server_context.Name}";
        private string client_path => $"Build/{Application.version}/Client/{m_client_context.Target.ToString().Replace("Standalone", "")}/{m_client_context.Name}";*/


        [MenuItem("Window/Labyrinth/Builder")]
        public static void Display()
        {
            Builder wnd = GetWindow<Builder>();
            wnd.titleContent = new GUIContent("Builder");
        }

        private void Awake()
        {
        }

        private void OnEnable()
        {
#if UNITY_EDITOR_WIN
            m_server_name = $"{Application.productName}_Server.exe";
            m_server_platform = Standalone.Windows;
#elif UNITY_EDITOR_OSX
            m_server_name = $"{Application.productName}_Server";
            m_server_platform = Standalone.Mac;
#elif UNITY_EDITOR_LINUX
            m_server_name = $"{Application.productName}_Server.x86_64";
            m_server_platform = Standalone.Linux;
#endif
            m_client_name = $"{Application.productName}_Client.(file extension)";


            string json = EditorPrefs.GetString("BuilderWindow", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(json, this);

            m_server_defs = new ReorderableList(m_server_definitions, typeof(string), true, false, true, true);
            m_client_defs = new ReorderableList(m_client_definitions, typeof(string), true, false, true, true);

            m_server_defs.drawHeaderCallback = DrawHeader;
            m_client_defs.drawHeaderCallback = DrawHeader;

            m_server_defs.drawElementCallback = DrawServerItem;
            m_client_defs.drawElementCallback = DrawClientItem;
        }

        private void OnDisable()
        {
            EditorPrefs.SetString("BuilderWindow", JsonUtility.ToJson(this, true));
        }

        private void DrawServerItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_server_definitions[index] == null) m_server_definitions[index] = "";
            m_server_definitions[index] = EditorGUI.TextField(rect, m_server_definitions[index]);
        }

        private void DrawClientItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_client_definitions[index] == null) m_client_definitions[index] = "";
            m_client_definitions[index] = EditorGUI.TextField(rect, m_client_definitions[index]);
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Defines");
        }

        private static BuildPlayerOptions Get(string name, string path, BuildTargetGroup group, BuildTarget target,
#if UNITY_2021_2_OR_NEWER
            int subTarget,
#endif
            BuildOptions options, string[] defines)
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
            player.locationPathName = $"{path}/{name}";
            player.targetGroup = group;
            player.target = target;

#if UNITY_2021_2_OR_NEWER
            player.subtarget = subTarget;
#endif

            player.options = options;
            player.extraScriptingDefines = defines;

            return player;
        }

        private void Update()
        {
            if (m_explore)
            {
                string path = EditorUtility.OpenFolderPanel($"Build", m_server ? m_server_build_path : m_client_build_path, "");
                if (m_server)
                {
                    m_server_build_path = path;
                }
                else
                {
                    m_client_build_path = path;
                }
                m_explore = false;
            }

            /*List<string> definitions = new List<string>();
            PlayerSettings.GetScriptingDefineSymbolsForGroup(m_group, out string[] defines);
            definitions.AddRange(defines);
            foreach (var define in m_server_definitions)
            {
                if (!definitions.Contains(define))
                    definitions.Add(define);
            }
            foreach (var define in m_client_definitions)
            {
                if (!definitions.Contains(define))
                    definitions.Add(define);
            }*/

            if (m_build)
            {
                if (m_server)
                {
                    BuildTarget target;
                    switch(m_server_platform)
                    {
                        case Standalone.Windows64:
                            target = BuildTarget.StandaloneWindows64;
                            break;
                        case Standalone.Windows:
                            target = BuildTarget.StandaloneWindows64;
                            break;
                        case Standalone.Linux:
                            target = BuildTarget.EmbeddedLinux;
                            break;
                        default:
                        case Standalone.Linux64:
                            target = BuildTarget.StandaloneLinux64;
                            break;
                        /*case Standalone.OSX64:
                            break;
                        case Standalone.OSX:
                            break;*/
                    }
                    BuildPipeline.BuildPlayer(Get(m_server_name, m_server_build_path, BuildTargetGroup.Standalone, target,
#if UNITY_2021_2_OR_NEWER
                        (int)StandaloneBuildSubtarget.Server,
#endif

                        m_server_options, m_server_definitions.ToArray()));
                }
                else
                {
                    BuildPipeline.BuildPlayer(Get(m_client_name, m_client_build_path, m_client_group, m_client_target,
#if UNITY_2021_2_OR_NEWER
                        (int)StandaloneBuildSubtarget.Player,
#endif
                        m_client_options, m_client_definitions.ToArray()));
                }
                m_build = false;
            }
        }

        private void OnGUI()
        {
            m_scroll = EditorGUILayout.BeginScrollView(m_scroll);

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

            foreach(var scene in EditorBuildSettings.scenes)
            {
                EditorGUILayout.LabelField($"{scene.path}");
            }

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Server Build");
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            m_server_name = EditorGUILayout.TextField("Name", m_server_name);
            m_server_platform = (Standalone)EditorGUILayout.EnumPopup("Standalone", m_server_platform);
            m_server_options = (BuildOptions)EditorGUILayout.EnumFlagsField("Options", m_server_options);
            m_server_defs.DoLayoutList();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Build Path: {m_server_build_path}");
            if (GUILayout.Button("Change"))
            {
                m_explore = true;
                m_server = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Server"))
            {
                m_build = true;
                m_server = true;
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
            
            m_client_name = EditorGUILayout.TextField("Name", m_client_name);
            m_client_group = (BuildTargetGroup)EditorGUILayout.EnumPopup("Group", m_client_group);
            m_client_target = (BuildTarget)EditorGUILayout.EnumPopup("Target", m_client_target);
            m_client_options = (BuildOptions)EditorGUILayout.EnumFlagsField("Options", m_client_options);
            m_client_defs.DoLayoutList();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Build Path: {m_client_build_path}");
            if (GUILayout.Button("Change"))
            {
                m_explore = true;
                m_server = false;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Client"))
            {
                m_build = true;
                m_server = false;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif