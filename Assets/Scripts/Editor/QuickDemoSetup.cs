using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TripMeta.Demo;

namespace TripMeta.Editor
{
    /// <summary>
    /// 快速演示场景设置工具
    /// </summary>
    public class QuickDemoSetup : EditorWindow
    {
        [MenuItem("TripMeta/Quick Demo Setup")]
        public static void ShowWindow()
        {
            GetWindow<QuickDemoSetup>("Quick Demo Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("TripMeta 快速演示场景设置", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "这个工具将帮助你快速创建一个可运行的演示场景，\n" +
                "包含所有必要的组件和视觉效果。",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("创建完整演示场景", GUILayout.Height(40)))
            {
                CreateDemoScene();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "演示场景将包含:\n" +
                "• DemoController - 演示控制器\n" +
                "• 可视化景点标记\n" +
                "• 旅游路径线\n" +
                "• 完整的UI界面\n" +
                "• 粒子和光效",
                MessageType.None);
        }

        public static void CreateDemoScene()
        {
            // 创建新场景
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // 设置基础场景
            SetupBasicScene();

            // 创建演示控制器
            CreateDemoController();

            // 创建可视化效果
            CreateVisualEffects();

            // 保存场景
            string scenePath = "Assets/Scenes/DemoScene.unity";
            bool success = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);

            if (success)
            {
                // 打开场景
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                EditorUtility.DisplayDialog("成功", "演示场景创建成功！\n\n点击Play即可查看演示效果。", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "场景保存失败！", "确定");
            }
        }

        public static void SetupBasicScene()
        {
            // 主相机
            GameObject mainCamera = new GameObject("Main Camera");
            Camera camera = mainCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.1f, 0.15f, 0.2f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;
            mainCamera.tag = "MainCamera";
            mainCamera.transform.position = new Vector3(0, 5, -15);
            mainCamera.transform.rotation = Quaternion.Euler(15f, 0, 0);

            // 方向光
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.5f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0);

            // 地面
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(20, 1, 20);
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            groundRenderer.material.color = new Color(0.2f, 0.3f, 0.35f);

            // 创建平台区域
            CreatePlatform();

            // 创建装饰元素
            CreateDecorations();
        }

        private static void CreatePlatform()
        {
            // 主平台
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            platform.name = "MainPlatform";
            platform.transform.position = new Vector3(0, 0.1f, 5);
            platform.transform.localScale = new Vector3(8, 0.2f, 8);
            Renderer platformRenderer = platform.GetComponent<Renderer>();
            platformRenderer.material.color = new Color(0.15f, 0.2f, 0.3f);

            // 边缘装饰
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 8f, 0.5f, Mathf.Sin(angle) * 8f + 5f);

                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.position = pos;
                pillar.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
                Renderer pillarRenderer = pillar.GetComponent<Renderer>();
                pillarRenderer.material.color = new Color(0.3f, 0.4f, 0.5f);
            }
        }

        private static void CreateDecorations()
        {
            // 创建简单的建筑模型
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-10, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(-10, 0, 10),
                new Vector3(10, 0, 10)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                CreateBuilding(positions[i], i);
            }
        }

        private static void CreateBuilding(Vector3 position, int index)
        {
            GameObject building = new GameObject("Building_" + index);
            building.transform.position = position;

            // 主体
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(building.transform);
            body.transform.localPosition = new Vector3(0, 3f, 0);
            body.transform.localScale = new Vector3(5f, 6f, 5f);
            Renderer bodyRenderer = body.GetComponent<Renderer>();
            bodyRenderer.material.color = new Color(0.5f, 0.5f, 0.55f);

            // 屋顶
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(building.transform);
            roof.transform.localPosition = new Vector3(0, 6.5f, 0);
            roof.transform.localScale = new Vector3(5.5f, 1f, 5.5f);
            Renderer roofRenderer = roof.GetComponent<Renderer>();
            roofRenderer.material.color = new Color(0.4f, 0.3f, 0.3f);
        }

        public static void CreateDemoController()
        {
            GameObject demoControllerObj = new GameObject("--- Managers ---");

            // 添加DemoController
            GameObject dcObj = new GameObject("DemoController");
            dcObj.transform.SetParent(demoControllerObj.transform);
            DemoController dc = dcObj.AddComponent<DemoController>();
            dc.autoStartDemo = true;
        }

        public static void CreateVisualEffects()
        {
            // 创建一个简单的粒子预制体
            CreateParticlePrefab();

            // 创建路径指示器
            CreatePathIndicator();
        }

        private static void CreateParticlePrefab()
        {
            GameObject particlePrefab = new GameObject("ParticlePrefab");
            particlePrefab.hideFlags = HideFlags.HideInHierarchy;

            ParticleSystem ps = particlePrefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 0.5f;
            main.startSize = 0.3f;
            main.startColor = new Color(0, 1f, 1f, 0.5f);
            main.maxParticles = 50;
            main.gravityModifier = -0.2f;

            var emission = ps.emission;
            emission.rateOverTime = 10;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            // 保存为预制体
            #if UNITY_2018_3_OR_NEWER
            PrefabUtility.SaveAsPrefabAsset(particlePrefab, "Assets/Prefabs/ParticlePrefab.prefab");
            #endif
        }

        private static void CreatePathIndicator()
        {
            // 创建箭头预制体
            GameObject arrowPrefab = GameObject.CreatePrimitive(PrimitiveType.Cone);
            arrowPrefab.name = "PathArrowPrefab";
            arrowPrefab.transform.rotation = Quaternion.Euler(0, 0, 90);
            arrowPrefab.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            Renderer arrowRenderer = arrowPrefab.GetComponent<Renderer>();
            arrowRenderer.material.color = new Color(1f, 0.8f, 0f);

            #if UNITY_2018_3_OR_NEWER
            PrefabUtility.SaveAsPrefabAsset(arrowPrefab, "Assets/Prefabs/PathArrowPrefab.prefab");
            #endif

            DestroyImmediate(arrowPrefab);
        }
    }
}
