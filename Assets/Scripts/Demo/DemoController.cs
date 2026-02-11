using UnityEngine;
using UnityEngine.UI;
using TripMeta.AI;
using TripMeta.Presentation;
using System.Collections.Generic;

namespace TripMeta.Demo
{
    /// <summary>
    /// 演示控制器 - 展示TripMeta的实际功能效果
    /// </summary>
    public class DemoController : MonoBehaviour
    {
        [Header("演示设置")]
        [SerializeField] private bool autoStartDemo = true;
        [SerializeField] private float demoStepDelay = 3f;

        [Header("UI引用")]
        [SerializeField] private Canvas demoCanvas;
        [SerializeField] private Text statusText;
        [SerializeField] private Text guideText;
        [SerializeField] private Button[] actionButtons;

        [Header("演示内容")]
        [SerializeField] private GameObject[] tourPoints;
        [SerializeField] private LineRenderer tourPath;

        private AITourGuide tourGuide;
        private TourLocationManager locationManager;
        private TourUIManager tourUI;
        private int currentDemoStep = 0;

        // 演示数据
        private readonly string[] demoConversations = new string[]
        {
            "欢迎来到纽约！我是你的AI导游小美。",
            "纽约是美国最大的城市，拥有丰富的历史。",
            "时代广场被称为'世界的十字路口'。",
            "中央公园是城市的绿肺，占地843英亩。",
            "自由女神像是美国的象征，高93米。"
        };

        private readonly string[] demoLocations = new string[]
        {
            "时代广场 - 商业娱乐中心",
            "中央公园 - 城市绿肺",
            "自由女神像 - 美国象征",
            "帝国大厦 - 地标建筑",
            "大都会博物馆 - 世界艺术宝库"
        };

        public static DemoController Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            InitializeDemo();
            if (autoStartDemo)
            {
                StartCoroutine(RunDemoSequence());
            }
        }

        /// <summary>
        /// 初始化演示
        /// </summary>
        private void InitializeDemo()
        {
            // 查找必要组件
            tourGuide = FindObjectOfType<AITourGuide>();
            locationManager = FindObjectOfType<TourLocationManager>();
            tourUI = FindObjectOfType<TourUIManager>();

            // 创建演示UI
            if (demoCanvas == null)
            {
                CreateDemoUI();
            }

            // 创建演示场景元素
            CreateDemoSceneElements();

            UpdateStatus("演示系统已就绪");
        }

        /// <summary>
        /// 创建演示UI
        /// </summary>
        private void CreateDemoUI()
        {
            GameObject canvasObj = new GameObject("DemoCanvas");
            demoCanvas = canvasObj.AddComponent<Canvas>();
            demoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 状态栏
            GameObject statusPanel = CreateUIPanel(canvasObj.transform, new Vector2(0, 400), new Vector2(800, 60));
            statusText = CreateUIText(statusPanel.transform, "系统初始化中...", 24, TextAnchor.MiddleCenter);

            // 导游对话区域
            GameObject guidePanel = CreateUIPanel(canvasObj.transform, new Vector2(0, -200), new Vector2(700, 150));
            guidePanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            guideText = CreateUIText(guidePanel.transform, "导游对话将在这里显示...", 20, TextAnchor.UpperLeft);

            // 操作按钮
            actionButtons = new Button[4];
            string[] buttonLabels = { "开始导游", "下一个景点", "AI对话", "重置演示" };
            for (int i = 0; i < buttonLabels.Length; i++)
            {
                actionButtons[i] = CreateUIButton(canvasObj.transform, buttonLabels[i], new Vector2(-300 + i * 200, -350), i);
            }

            // 版本信息
            GameObject versionPanel = CreateUIPanel(canvasObj.transform, new Vector2(-450, 420), new Vector2(200, 40));
            var versionText = CreateUIText(versionPanel.transform, "TripMeta v1.0.0 | Alpha", 14, TextAnchor.MiddleLeft);
            versionText.color = Color.gray;
        }

        /// <summary>
        /// 创建UI面板
        /// </summary>
        private GameObject CreateUIPanel(Transform parent, Vector2 position, Vector2 size)
        {
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(parent, false);

            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            return panel;
        }

        /// <summary>
        /// 创建UI文本
        /// </summary>
        private Text CreateUIText(Transform parent, string content, int fontSize, TextAnchor alignment)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);

            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            return text;
        }

        /// <summary>
        /// 创建UI按钮
        /// </summary>
        private Button CreateUIButton(Transform parent, string label, Vector2 position, int index)
        {
            GameObject buttonObj = new GameObject("Button_" + index);
            buttonObj.transform.SetParent(parent, false);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 1f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(150, 50);

            // 按钮文本
            Text buttonText = CreateUIText(buttonObj.transform, label, 18, TextAnchor.MiddleCenter);

            // 添加点击事件
            int buttonIndex = index;
            button.onClick.AddListener(() => OnButtonClick(buttonIndex));

            return button;
        }

        /// <summary>
        /// 创建演示场景元素
        /// </summary>
        private void CreateDemoSceneElements()
        {
            // 创建地面
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10, 1, 10);
            ground.transform.position = new Vector3(0, 0, 0);

            // 添加材质颜色
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            groundRenderer.material.color = new Color(0.3f, 0.5f, 0.3f);

            // 创建演示标记点
            tourPoints = new GameObject[5];
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0, 1, 10),
                new Vector3(10, 1, 5),
                new Vector3(5, 1, -10),
                new Vector3(-10, 1, -5),
                new Vector3(-5, 1, 10)
            };

            for (int i = 0; i < tourPoints.Length; i++)
            {
                tourPoints[i] = CreateTourPoint(positions[i], i);
            }

            // 创建路径线
            CreateTourPath(positions);
        }

        /// <summary>
        /// 创建旅游景点标记
        /// </summary>
        private GameObject CreateTourPoint(Vector3 position, int index)
        {
            GameObject point = new GameObject("TourPoint_" + index);
            point.transform.position = position;

            // 柱子
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.transform.SetParent(point.transform);
            pillar.transform.localPosition = new Vector3(0, 0.5f, 0);
            pillar.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
            pillar.GetComponent<Renderer>().material.color = Color.yellow;

            // 顶部球体
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(point.transform);
            sphere.transform.localPosition = new Vector3(0, 1.2f, 0);
            sphere.transform.localScale = Vector3.one * 0.5f;
            sphere.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);

            // 添加旋转动画
            point.AddComponent<Rotator>();

            return point;
        }

        /// <summary>
        /// 创建旅游路径
        /// </summary>
        private void CreateTourPath(Vector3[] positions)
        {
            GameObject pathObj = new GameObject("TourPath");
            tourPath = pathObj.AddComponent<LineRenderer>();

            tourPath.positionCount = positions.Length + 1;
            tourPath.startWidth = 0.2f;
            tourPath.endWidth = 0.2f;
            tourPath.loop = true;
            tourPath.useWorldSpace = false;

            // 设置路径点
            for (int i = 0; i <= positions.Length; i++)
            {
                tourPath.SetPosition(i, positions[i % positions.Length]);
            }

            tourPath.material = new Material(Shader.Find("Sprites/Default"));
            tourPath.startColor = new Color(0, 1f, 1f, 0.5f);
            tourPath.endColor = new Color(1f, 0, 1f, 0.5f);
        }

        /// <summary>
        /// 运行演示序列
        /// </summary>
        private System.Collections.IEnumerator RunDemoSequence()
        {
            UpdateStatus("初始化系统...");
            yield return new WaitForSeconds(1f);

            UpdateStatus("加载AI导游...");
            yield return new WaitForSeconds(1f);

            // 模拟导游对话
            for (int i = 0; i < demoConversations.Length; i++)
            {
                ShowGuideText(demoConversations[i]);
                UpdateStatus($"演示进度: {i + 1}/{demoConversations.Length}");
                yield return new WaitForSeconds(demoStepDelay);
            }

            UpdateStatus("演示完成！请点击按钮体验功能。");
        }

        /// <summary>
        /// 按钮点击处理
        /// </summary>
        private void OnButtonClick(int index)
        {
            switch (index)
            {
                case 0: // 开始导游
                    StartTourGuide();
                    break;
                case 1: // 下一个景点
                    NextLocation();
                    break;
                case 2: // AI对话
                    ShowAIConversation();
                    break;
                case 3: // 重置演示
                    ResetDemo();
                    break;
            }
        }

        /// <summary>
        /// 开始导游
        /// </summary>
        public void StartTourGuide()
        {
            UpdateStatus("启动AI导游...");
            ShowGuideText("你好！我是你的AI导游小美。让我们一起探索纽约的精彩景点吧！");

            if (locationManager != null)
            {
                locationManager.SetCurrentLocation("newyork");
            }

            // 高亮所有景点
            StartCoroutine(HighlightTourPoints());
        }

        /// <summary>
        /// 下一个景点
        /// </summary>
        public void NextLocation()
        {
            currentDemoStep = (currentDemoStep + 1) % demoLocations.Length;
            string location = demoLocations[currentDemoStep];

            UpdateStatus($"前往: {location.Split('-')[0].Trim()}");
            ShowGuideText($"现在我们来到{location}。这是一个非常值得参观的地方！");

            // 移动相机到对应位置
            if (tourPoints != null && tourPoints.Length > currentDemoStep)
            {
                Vector3 targetPos = tourPoints[currentDemoStep].transform.position;
                StartCoroutine(MoveCameraTo(targetPos + new Vector3(0, 3, -5)));
            }
        }

        /// <summary>
        /// AI对话演示
        /// </summary>
        public void ShowAIConversation()
        {
            UpdateStatus("AI对话演示");
            string[] questions = new string[]
            {
                "纽约有多少人口？",
                "时代广场有什么特色？",
                "推荐一个值得去的地方"
            };

            string randomQuestion = questions[Random.Range(0, questions.Length)];
            ShowGuideText($"游客问: {randomQuestion}\n\n导游回答: {GetMockAnswer(randomQuestion)}");
        }

        /// <summary>
        /// 获取模拟回答
        /// </summary>
        private string GetMockAnswer(string question)
        {
            if (question.Contains("人口"))
                return "纽约市拥有超过800万人口，是美国人口最多的城市。";
            else if (question.Contains("时代广场"))
                return "时代广场被称为'世界的十字路口'，每年吸引约5000万游客，是世界的娱乐和商业中心。";
            else
                return "我强烈推荐中央公园，它是纽约的绿洲，你可以在这里散步、野餐或乘坐马车游览。";
        }

        /// <summary>
        /// 重置演示
        /// </summary>
        public void ResetDemo()
        {
            currentDemoStep = 0;
            UpdateStatus("演示已重置");
            ShowGuideText("演示已重置，请选择功能体验。");

            // 重置相机位置
            Camera.main.transform.position = new Vector3(0, 3, -10);
            Camera.main.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatus(string status)
        {
            if (statusText != null)
            {
                statusText.text = $"状态: {status}";
            }
            Debug.Log($"[DemoController] {status}");
        }

        /// <summary>
        /// 显示导游文本
        /// </summary>
        private void ShowGuideText(string text)
        {
            if (guideText != null)
            {
                guideText.text = text;
            }
        }

        /// <summary>
        /// 高亮景点
        /// </summary>
        private System.Collections.IEnumerator HighlightTourPoints()
        {
            for (int i = 0; i < tourPoints.Length; i++)
            {
                if (tourPoints[i] != null)
                {
                    Renderer renderer = tourPoints[i].GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.cyan;
                        yield return new WaitForSeconds(0.3f);
                        renderer.material.color = new Color(1f, 0.5f, 0f);
                    }
                }
            }
        }

        /// <summary>
        /// 移动相机
        /// </summary>
        private System.Collections.IEnumerator MoveCameraTo(Vector3 targetPosition)
        {
            float duration = 1f;
            Vector3 startPosition = Camera.main.transform.position;
            Quaternion startRotation = Camera.main.transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(tourPoints[currentDemoStep].transform.position - targetPosition);

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                Camera.main.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                yield return null;
            }
        }
    }

    /// <summary>
    /// 旋转组件 - 用于景点标记
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        void Update()
        {
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
        }
    }
}
