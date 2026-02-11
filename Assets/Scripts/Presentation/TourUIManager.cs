using UnityEngine;
using UnityEngine.UI;
using TripMeta.AI;
using System;

namespace TripMeta.Presentation
{
    /// <summary>
    /// 旅游UI管理器 - 管理AI导游相关的UI
    /// </summary>
    public class TourUIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Text guideText;
        [SerializeField] private Text locationTitle;
        [SerializeField] private Image guideAvatar;
        [SerializeField] private GameObject listeningIndicator;
        [SerializeField] private GameObject thinkingIndicator;

        [Header("VR Settings")]
        [SerializeField] private bool useSpatialUI = true;
        [SerializeField] private float uiDistance = 2.5f;

        private AITourGuide tourGuide;
        private Coroutine currentDisplayCoroutine;

        public static TourUIManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // 查找AITourGuide
            tourGuide = FindObjectOfType<AITourGuide>();

            if (tourGuide != null)
            {
                // 订阅事件
                tourGuide.OnGuideSpeak += OnGuideSpeak;
                tourGuide.OnGuideStateChanged += OnGuideStateChanged;
            }

            SetupCanvas();
        }

        /// <summary>
        /// 设置画布
        /// </summary>
        private void SetupCanvas()
        {
            if (canvas == null)
            {
                // 创建画布
                GameObject canvasObj = new GameObject("TourUI Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                // 设置为VR空间UI
                canvasObj.transform.SetParent(transform);
                canvasObj.transform.localPosition = Vector3.forward * uiDistance;
                canvasObj.transform.localRotation = Quaternion.identity;
                canvasObj.transform.localScale = Vector3.one * 0.01f;

                // 添加必要的UI元素
                CreateUIElements(canvasObj);
            }
        }

        /// <summary>
        /// 创建UI元素
        /// </summary>
        private void CreateUIElements(GameObject canvasObj)
        {
            // 背景面板
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObj.transform, false);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            // 导游头像 (占位符)
            GameObject avatarObj = new GameObject("GuideAvatar");
            avatarObj.transform.SetParent(panel.transform, false);

            guideAvatar = avatarObj.AddComponent<Image>();
            RectTransform avatarRect = guideAvatar.GetComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0, 0.5f);
            avatarRect.anchorMax = new Vector2(0, 0.5f);
            avatarRect.pivot = new Vector2(0, 0.5f);
            avatarRect.anchoredPosition = new Vector2(50, 0);
            avatarRect.sizeDelta = new Vector2(100, 100);

            // 位置标题
            GameObject titleObj = new GameObject("LocationTitle");
            titleObj.transform.SetParent(panel.transform, false);

            locationTitle = titleObj.AddComponent<Text>();
            locationTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            locationTitle.text = "欢迎来到 TripMeta";
            locationTitle.fontSize = 36;
            locationTitle.color = Color.white;
            locationTitle.alignment = TextAnchor.MiddleLeft;

            RectTransform titleRect = locationTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 0.9f);
            titleRect.offsetMin = new Vector2(170, 0);
            titleRect.offsetMax = new Vector2(-20, 0);

            // 导游文本
            GameObject textObj = new GameObject("GuideText");
            textObj.transform.SetParent(panel.transform, false);

            guideText = textObj.AddComponent<Text>();
            guideText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            guideText.text = "你好！我是你的AI导游。";
            guideText.fontSize = 24;
            guideText.color = Color.white;
            guideText.alignment = TextAnchor.UpperLeft;

            RectTransform textRect = guideText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.1f);
            textRect.anchorMax = new Vector2(1, 0.75f);
            textRect.offsetMin = new Vector2(170, 0);
            textRect.offsetMax = new Vector2(-20, 0);

            // 监听指示器
            listeningIndicator = CreateIndicator(panel.transform, "Listening...", Color.yellow, new Vector2(0, 0.05f));
            thinkingIndicator = CreateIndicator(panel.transform, "Thinking...", Color.cyan, new Vector2(0, 0.05f));

            listeningIndicator.SetActive(false);
            thinkingIndicator.SetActive(false);
        }

        /// <summary>
        /// 创建指示器
        /// </summary>
        private GameObject CreateIndicator(Transform parent, string text, Color color, Vector2 anchoredPos)
        {
            GameObject indicatorObj = new GameObject(text.Replace("...", "") + "Indicator");
            indicatorObj.transform.SetParent(parent, false);

            Text indicatorText = indicatorObj.AddComponent<Text>();
            indicatorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            indicatorText.text = text;
            indicatorText.fontSize = 20;
            indicatorText.color = color;
            indicatorText.alignment = TextAnchor.MiddleCenter;

            RectTransform rect = indicatorText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(200, 30);

            return indicatorObj;
        }

        /// <summary>
        /// 显示导游说话
        /// </summary>
        public void DisplayGuideSpeech(string text, float duration = 5f)
        {
            if (guideText != null)
            {
                guideText.text = text;
            }

            // TODO: 实现文本显示动画和自动隐藏
        }

        /// <summary>
        /// 更新位置信息
        /// </summary>
        public void UpdateLocationInfo(LocationContext location)
        {
            if (locationTitle != null && location != null)
            {
                locationTitle.text = location.locationName;
            }
        }

        /// <summary>
        /// 显示正在监听
        /// </summary>
        public void ShowListening(bool show)
        {
            if (listeningIndicator != null)
            {
                listeningIndicator.SetActive(show);
            }
        }

        /// <summary>
        /// 显示正在思考
        /// </summary>
        public void ShowThinking(bool show)
        {
            if (thinkingIndicator != null)
            {
                thinkingIndicator.SetActive(show);
            }
        }

        // 事件处理
        private void OnGuideSpeak(string text)
        {
            DisplayGuideSpeech(text);
        }

        private void OnGuideStateChanged(GuideState state)
        {
            switch (state)
            {
                case GuideState.Listening:
                    ShowListening(true);
                    ShowThinking(false);
                    break;
                case GuideState.Thinking:
                    ShowListening(false);
                    ShowThinking(true);
                    break;
                case GuideState.Speaking:
                    ShowListening(false);
                    ShowThinking(false);
                    break;
                default:
                    ShowListening(false);
                    ShowThinking(false);
                    break;
            }
        }

        void OnDestroy()
        {
            if (tourGuide != null)
            {
                tourGuide.OnGuideSpeak -= OnGuideSpeak;
                tourGuide.OnGuideStateChanged -= OnGuideStateChanged;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
