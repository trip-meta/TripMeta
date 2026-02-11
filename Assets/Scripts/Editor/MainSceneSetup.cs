using UnityEngine;
using UnityEditor;
using TripMeta.Core;
using TripMeta.AI;

namespace TripMeta.Editor
{
    /// <summary>
    /// 主场景设置工具 - 自动化设置MainScene
    /// </summary>
    public class MainSceneSetup : EditorWindow
    {
        [MenuItem("TripMeta/Setup Main Scene")]
        public static void ShowWindow()
        {
            GetWindow<MainSceneSetup>("TripMeta Main Scene Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("TripMeta Main Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool will help you set up the main scene for TripMeta VR application.\n\n" +
                "Follow these steps:",
                MessageType.Info);

            GUILayout.Label("Step 1: XR Setup", EditorStyles.boldLabel);
            if (GUILayout.Button("Open XR Management"))
            {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings/XR Plug-in Management");
            }
            GUILayout.Label("→ Enable PICO plugin in XR Plug-in Management");
            GUILayout.Space(5);

            GUILayout.Label("Step 2: Create MainScene", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Main Scene"))
            {
                CreateMainScene();
            }
            GUILayout.Space(5);

            GUILayout.Label("Step 3: Manual Setup Instructions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "After creating the scene:\n" +
                "1. Add XR Rig from XR Interaction Toolkit\n" +
                "2. Add VRControllerManager to scene\n" +
                "3. Add VRManager to scene\n" +
                "4. Add ApplicationBootstrap to scene\n" +
                "5. Add AITourGuide to scene\n" +
                "6. Create basic environment (ground, lighting)",
                MessageType.None);
        }

        private static void CreateMainScene()
        {
            string scenePath = "Assets/Scenes/MainScene.unity";
            string sceneFolder = "Assets/Scenes";

            // Create Scenes folder if it doesn't exist
            if (!System.IO.Directory.Exists(sceneFolder))
            {
                System.IO.Directory.CreateDirectory(sceneFolder);
                AssetDatabase.Refresh();
            }

            // Create new scene
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // Setup basic scene objects
            SetupSceneHierarchy();

            // Save the scene
            bool success = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            if (success)
            {
                Debug.Log($"[MainSceneSetup] MainScene created at: {scenePath}");
                EditorUtility.DisplayDialog("Success", $"MainScene created successfully!\n\nLocation: {scenePath}", "OK");
            }
            else
            {
                Debug.LogError("[MainSceneSetup] Failed to save MainScene");
            }
        }

        private static void SetupSceneHierarchy()
        {
            // Create Main Camera
            GameObject mainCamera = new GameObject("Main Camera");
            Camera camera = mainCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.1921569f, 0.3019608f, 0.4745098f, 0f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;
            mainCamera.tag = "MainCamera";

            // Create Directional Light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.9568627f, 0.8392157f, 1f);
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;

            // Create Managers GameObject
            GameObject managers = new GameObject("--- Managers ---");

            // Add ApplicationBootstrap
            GameObject bootstrapObj = new GameObject("ApplicationBootstrap");
            bootstrapObj.transform.SetParent(managers.transform);
            bootstrapObj.AddComponent<ApplicationBootstrap>();

            // Add VRManager
            GameObject vrManagerObj = new GameObject("VRManager");
            vrManagerObj.transform.SetParent(managers.transform);
            vrManagerObj.AddComponent<VRManager>();

            // Add AIServiceManager
            GameObject aiManagerObj = new GameObject("AIServiceManager");
            aiManagerObj.transform.SetParent(managers.transform);
            aiManagerObj.AddComponent<AIServiceManager>();

            // Add AITourGuide
            GameObject tourGuideObj = new GameObject("AITourGuide");
            tourGuideObj.transform.SetParent(managers.transform);
            tourGuideObj.AddComponent<AITourGuide>();

            // Create Environment GameObject
            GameObject environment = new GameObject("--- Environment ---");

            // Create Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(environment.transform);
            ground.transform.localScale = new Vector3(10f, 1f, 10f);

            Debug.Log("[MainSceneSetup] Scene hierarchy created");
        }
    }
}
