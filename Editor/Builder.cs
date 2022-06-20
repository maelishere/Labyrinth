using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Labyrinth
{
    public class Builder : EditorWindow
    {
        private static string[] m_scenes = new string[0];


        private static string m_server_definition = "SERVER_BUILD";
        private static Context m_server_context;

        private static string m_client_definition = "CLIENT_BUILD";
        private static Context m_client_context;

        private ReorderableList scenes_list;
        private ReorderableList server_def_list;
        private ReorderableList client_def_list;

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
            string server = EditorPrefs.GetString("Builder_Server_Context",
            JsonUtility.ToJson(new Context($"{Application.productName}_Server",
            BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode,
            new string[] { m_server_definition })));

            JsonUtility.FromJsonOverwrite(server, m_server_context);

            string client = EditorPrefs.GetString("Builder_Client_Context",
            JsonUtility.ToJson(new Context($"{Application.productName}_Client",
            BuildTarget.StandaloneWindows64, BuildOptions.Development,
            new string[] { m_client_definition })));

            JsonUtility.FromJsonOverwrite(client, m_client_context);

            scenes_list = new ReorderableList(EditorBuildSettings.scenes, typeof(EditorBuildSettingsScene), true, true, true, true);
            scenes_list.drawElementCallback = DrawSceneItem; // Delegate to draw the elements on the list
            scenes_list.drawHeaderCallback = DrawSceneHeader;

            server_def_list = new ReorderableList(m_server_context.Definitions, typeof(string), true, true, true, true);
            server_def_list.drawElementCallback = DrawSDefItem; // Delegate to draw the elements on the list
            server_def_list.drawHeaderCallback = DrawSDefHeader;

            client_def_list = new ReorderableList(m_client_context.Definitions, typeof(string), true, true, true, true);
            client_def_list.drawElementCallback = DrawCDefItem; // Delegate to draw the elements on the list
            client_def_list.drawHeaderCallback = DrawCDefHeader;
        }

        void OnDisable()
        {
            EditorPrefs.SetString("Builder_Server_Context", JsonUtility.ToJson(m_server_context));
            EditorPrefs.SetString("Builder_Client_Context", JsonUtility.ToJson(m_client_context));
        }

        private static void DrawSceneItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect toggle = new Rect(rect.x, rect.y, rect.width * 0.05f, rect.height * 0.8f);
            EditorBuildSettings.scenes[index].enabled = EditorGUI.Toggle(toggle, EditorBuildSettings.scenes[index].enabled);

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
            EditorGUI.LabelField(label, $"{index}");
        }

        //Draws the header
        private static void DrawSceneHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Scenes");
        }

        private static void DrawSDefItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect element = new Rect(rect.x, rect.y, rect.width, rect.height * 0.8f);
            m_server_context.Definitions[index] = EditorGUI.TextField(element, m_server_context.Definitions[index]);
        }

        //Draws the header
        private static void DrawSDefHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Server Script Definitions");
        }

        private static void DrawCDefItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            Rect element = new Rect(rect.x, rect.y, rect.width, rect.height * 0.8f);
            m_client_context.Definitions[index] = EditorGUI.TextField(element, m_client_context.Definitions[index]);
        }

        //Draws the header
        private static void DrawCDefHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Client Script Definitions");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build Settings");
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            scenes_list?.DoLayoutList();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Server Build");
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            m_server_context.Name = EditorGUILayout.TextField("Name", m_server_context.Name);
            m_server_context.Target = (BuildTarget)EditorGUILayout.EnumPopup("Target", m_server_context.Target);
            m_server_context.Options = (BuildOptions)EditorGUILayout.EnumPopup("Options", m_server_context.Options);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Build Path: {server_path}/");
            EditorGUILayout.Space();

            // server_def_list?.DoLayoutList();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Server"))
            {
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, server_path, m_server_context.Target, m_server_context.Options);
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Client Build");
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            m_client_context.Name = EditorGUILayout.TextField("Name", m_client_context.Name);
            m_client_context.Target = (BuildTarget)EditorGUILayout.EnumPopup("Target", m_client_context.Target);
            m_client_context.Options = (BuildOptions)EditorGUILayout.EnumPopup("Options", m_client_context.Options);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Build Path: {client_path}/");
            EditorGUILayout.Space();

            // client_def_list?.DoLayoutList();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Client"))
            {
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, client_path, m_client_context.Target, m_client_context.Options);
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}