using UnityEngine;

namespace TripMeta.Demo
{
    /// <summary>
    /// 运行时演示助手 - 可附加到任何GameObject快速启用演示功能
    /// </summary>
    public class RuntimeDemoHelper : MonoBehaviour
    {
        [Header("自动启用功能")]
        [Tooltip("启用时自动创建并添加演示组件")]
        [SerializeField] private bool enableOnStart = true;

        [Header("功能选择")]
        [SerializeField] private bool enableDemoController = true;
        [SerializeField] private bool enableInteractiveDemo = true;
        [SerializeField] private bool enableVisualEffects = true;

        [Header("调试信息")]
        [SerializeField] private bool showDebugKeys = true;
        [SerializeField] private bool autoInitialize = true;

        private static bool isInitialized = false;

        void Start()
        {
            if (enableOnStart && !isInitialized)
            {
                InitializeDemo();
            }
        }

        /// <summary>
        /// 初始化演示系统
        /// </summary>
        [ContextMenu("Initialize Demo")]
        public void InitializeDemo()
        {
            if (isInitialized) return;

            Debug.Log("[RuntimeDemoHelper] 初始化演示系统...");

            // 查找或创建演示控制器
            if (enableDemoController)
            {
                SetupDemoController();
            }

            // 查找或创建交互式演示
            if (enableInteractiveDemo)
            {
                SetupInteractiveDemo();
            }

            // 创建可视化效果
            if (enableVisualEffects)
            {
                SetupVisualEffects();
            }

            isInitialized = true;
            Debug.Log("[RuntimeDemoHelper] 演示系统初始化完成！");
            Debug.Log("[RuntimeDemoHelper] 可用快捷键:");
            Debug.Log("  [1] 开始导游");
            Debug.Log("  [2] 下一个景点");
            Debug.Log("  [3] AI对话");
            Debug.Log("  [4] 重置演示");
        }

        private void SetupDemoController()
        {
            var demoController = FindObjectOfType<DemoController>();
            if (demoController == null)
            {
                GameObject dcObj = new GameObject("DemoController");
                demoController = dcObj.AddComponent<DemoController>();
                Debug.Log("[RuntimeDemoHelper] 已创建DemoController");
            }
        }

        private void SetupInteractiveDemo()
        {
            var interactiveDemo = FindObjectOfType<InteractiveDemo>();
            if (interactiveDemo == null)
            {
                GameObject idObj = new GameObject("InteractiveDemo");
                interactiveDemo = idObj.AddComponent<InteractiveDemo>();
                Debug.Log("[RuntimeDemoHelper] 已创建InteractiveDemo");
            }
        }

        private void SetupVisualEffects()
        {
            // 为现有GameObject添加视觉效果
            var effects = GetComponent<LocationVisualEffects>();
            if (effects == null)
            {
                effects = gameObject.AddComponent<LocationVisualEffects>();
                Debug.Log("[RuntimeDemoHelper] 已添加LocationVisualEffects");
            }

            // 添加浮动文字
            var floatingText = GetComponent<FloatingText>();
            if (floatingText == null)
            {
                floatingText = gameObject.AddComponent<FloatingText>();
                Debug.Log("[RuntimeDemoHelper] 已添加FloatingText");
            }
        }

        void Update()
        {
            if (!showDebugKeys || !isInitialized) return;

            // 快捷键处理
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var demo = FindObjectOfType<DemoController>();
                if (demo != null) demo.StartTourGuide();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                var demo = FindObjectOfType<DemoController>();
                if (demo != null) demo.NextLocation();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                var demo = FindObjectOfType<InteractiveDemo>();
                if (demo != null) demo.ShowRandomConversation();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                var demo = FindObjectOfType<DemoController>();
                if (demo != null) demo.ResetDemo();
            }
        }

        /// <summary>
        /// 创建快速演示场景
        /// </summary>
        [ContextMenu("Create Quick Demo")]
        public void CreateQuickDemo()
        {
            Debug.Log("[RuntimeDemoHelper] 创建快速演示场景...");

            // 清除现有场景内容
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                if (obj.name != "Main Camera")
                {
                    DestroyImmediate(obj);
                }
            }

            // 创建基础环境
            CreateBasicEnvironment();

            // 创建演示标记点
            CreateDemoMarkers();

            // 添加演示系统
            InitializeDemo();

            Debug.Log("[RuntimeDemoHelper] 快速演示场景创建完成！");
        }

        private void CreateBasicEnvironment()
        {
            // 地面
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10, 1, 10);
            ground.GetComponent<Renderer>().material.color = new Color(0.2f, 0.3f, 0.35f);

            // 灯光
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        private void CreateDemoMarkers()
        {
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0, 0, 5),
                new Vector3(5, 0, 0),
                new Vector3(0, 0, -5),
                new Vector3(-5, 0, 0)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject marker = new GameObject("Marker_" + i);
                marker.transform.position = positions[i];

                // 柱子
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.SetParent(marker.transform);
                pillar.transform.localPosition = new Vector3(0, 0.5f, 0);
                pillar.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
                pillar.GetComponent<Renderer>().material.color = Color.yellow;

                // 球体
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(marker.transform);
                sphere.transform.localPosition = new Vector3(0, 1.2f, 0);
                sphere.transform.localScale = Vector3.one * 0.4f;
                sphere.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);

                // 添加效果
                marker.AddComponent<LocationVisualEffects>();
                marker.AddComponent<FloatingText>();
            }
        }
    }
}
