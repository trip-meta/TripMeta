using UnityEngine;
using UnityEngine.UI;
using TripMeta.AI;
using TripMeta.Presentation;
using System.Collections.Generic;

namespace TripMeta.Demo
{
    /// <summary>
    /// 交互式演示 - 显示AI导游的实际对话效果
    /// </summary>
    public class InteractiveDemo : MonoBehaviour
    {
        [Header("对话设置")]
        [SerializeField] private bool enableAutoDemo = false;
        [SerializeField] private float autoDemoInterval = 10f;

        [Header("UI组件")]
        [SerializeField] private GameObject chatPanel;
        [SerializeField] private InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private ScrollRect chatScroll;
        [SerializeField] Transform chatContent;

        [Header("对话数据")]
        [SerializeField] private List<ConversationExample> conversationExamples = new List<ConversationExample>();

        private AITourGuide tourGuide;
        private float autoDemoTimer;

        public static InteractiveDemo Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            tourGuide = FindObjectOfType<AITourGuide>();
            InitializeConversationExamples();
            CreateChatUI();

            if (sendButton != null)
            {
                sendButton.onClick.AddListener(OnSendMessage);
            }

            if (inputField != null)
            {
                inputField.onEndEdit.AddListener(OnInputSubmit);
            }

            // 显示欢迎消息
            AddMessage("system", "欢迎来到 TripMeta 演示！你可以向AI导游提问，或等待自动演示。");
        }

        void Update()
        {
            if (enableAutoDemo)
            {
                autoDemoTimer += Time.deltaTime;
                if (autoDemoTimer >= autoDemoInterval)
                {
                    autoDemoTimer = 0;
                    ShowRandomConversation();
                }
            }
        }

        /// <summary>
        /// 初始化对话示例
        /// </summary>
        private void InitializeConversationExamples()
        {
            conversationExamples = new List<ConversationExample>
            {
                new ConversationExample
                {
                    question = "纽约有哪些著名景点？",
                    answer = "纽约有很多著名景点！我推荐几个必去的地方：\n• 自由女神像 - 美国的象征\n• 时代广场 - 世界的十字路口\n• 中央公园 - 城市绿肺\n• 帝国大厦 - 标志性摩天大楼\n• 大都会博物馆 - 世界级艺术收藏\n您对哪个最感兴趣呢？"
                },
                new ConversationExample
                {
                    question = "介绍一下时代广场",
                    answer = "时代广场被称为'世界的十字路口'，是纽约市最繁忙的步行区之一。这里汇集了大量的剧院、餐厅、商店和广告牌。每年有约5000万游客来到这里，尤其是在新年前夜，著名的'落球'仪式就在这里举行。晚上的时候，这里的霓虹灯和巨型广告屏幕非常壮观！"
                },
                new ConversationExample
                {
                    question = "中央公园有多大？",
                    answer = "中央公园占地843英亩（约3.4平方公里），是曼哈顿最大的城市公园。公园长约4公里，宽约0.8公里，比摩纳哥整个国家还要大！里面有湖泊、草坪、树林、动物园和许多雕塑。它是纽约市民休闲娱乐的好去处，也是许多电影的取景地。"
                },
                new ConversationExample
                {
                    question = "推荐一些好吃的地方",
                    answer = "纽约是美食天堂！根据不同口味推荐：\n• 经典披萨: Joe's Pizza, Grimaldi's\n• 贝果: Ess-a-Bagel, Murray's Bagels\n• 熟食店: Katz's Delicatessen\n• 高端餐饮: Le Bernardin, Eleven Madison Park\n• 街头小吃: 尝试一下热狗和椒盐卷饼\n您喜欢哪种类型的食物呢？"
                },
                new ConversationExample
                {
                    question = "在纽约旅游要注意什么？",
                    answer = "旅游小贴士：\n\n🚇 交通：地铁是最便捷的交通方式，建议购买7日无限次地铁卡\n🎫 门票：很多景点建议提前在线购票，可以省去排队时间\n🌤 天气：春秋季节（4-6月，9-11月）最适宜旅游\n⚠️ 安全：晚上尽量在热闹区域活动，注意保管财物\n📱 应用：下载Citymapper和OpenTable会很有帮助\n\n还有什么其他问题吗？"
                }
            };
        }

        /// <summary>
        /// 创建聊天UI
        /// </summary>
        private void CreateChatUI()
        {
            if (chatPanel != null) return;

            // 创建聊天面板
            chatPanel = new GameObject("ChatPanel");
            Canvas canvas = chatPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            chatPanel.transform.position = new Vector3(0, 3, -8);
            chatPanel.transform.localScale = Vector3.one * 0.005f;

            // 添加GraphicRaycaster
            chatPanel.AddComponent<GraphicRaycaster>();

            // 创建背景
            CreatePanelBackground(chatPanel.transform);

            // 创建标题栏
            CreateHeaderBar(chatPanel.transform);

            // 创建聊天内容区域
            CreateChatContentArea(chatPanel.transform);

            // 创建输入区域
            CreateInputArea(chatPanel.transform);
        }

        private void CreatePanelBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);

            Image image = bg.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            RectTransform rect = bg.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        private void CreateHeaderBar(Transform parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent, false);

            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.2f, 0.6f, 1f, 1f);

            RectTransform rect = header.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.9f);
            rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = Vector2.zero;

            // 标题文本
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);

            Text title = titleObj.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.text = "AI导游对话";
            title.fontSize = 28;
            title.color = Color.white;
            title.alignment = TextAnchor.MiddleCenter;

            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.sizeDelta = Vector2.zero;
        }

        private void CreateChatContentArea(Transform parent)
        {
            GameObject scrollObj = new GameObject("ChatScroll");
            scrollObj.transform.SetParent(parent, false);

            chatScroll = scrollObj.AddComponent<ScrollRect>();
            chatScroll.horizontal = false;
            chatScroll.vertical = true;

            RectTransform scrollRect = chatScroll.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.2f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.85f);
            scrollRect.sizeDelta = Vector2.zero;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask viewportMask = viewport.AddComponent<Mask>();

            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0.5f, 1);

            chatScroll.viewport = viewport.transform as RectTransform;
            chatScroll.content = viewport.transform;

            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            chatContent = contentObj.transform;
        }

        private void CreateInputArea(Transform parent)
        {
            GameObject inputArea = new GameObject("InputArea");
            inputArea.transform.SetParent(parent, false);

            RectTransform rect = inputArea.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.05f);
            rect.anchorMax = new Vector2(0.95f, 0.15f);
            rect.sizeDelta = Vector2.zero;

            // 输入框
            GameObject inputFieldObj = new GameObject("InputField");
            inputFieldObj.transform.SetParent(inputArea.transform, false);

            inputField = inputFieldObj.AddComponent<InputField>();
            inputField.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            inputField.characterLimit = 100;
            inputField.placeholder = CreatePlaceholder("请输入问题...");

            Image inputBg = inputFieldObj.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            RectTransform inputRect = inputFieldObj.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0, 0);
            inputRect.anchorMax = new Vector2(0.75f, 1);
            inputRect.sizeDelta = Vector2.zero;
            inputRect.offsetMin = new Vector2(10, 5);
            inputRect.offsetMax = new Vector2(-5, -5);

            // 发送按钮
            GameObject sendButtonObj = new GameObject("SendButton");
            sendButtonObj.transform.SetParent(inputArea.transform, false);

            sendButton = sendButtonObj.AddComponent<Button>();
            Image buttonBg = sendButtonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.2f, 0.6f, 1f, 1f);

            RectTransform buttonRect = sendButtonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.8f, 0);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.sizeDelta = Vector2.zero;
            buttonRect.offsetMin = new Vector2(5, 5);
            buttonRect.offsetMax = new Vector2(-10, -5);

            // 按钮文本
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(sendButtonObj.transform, false);

            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.text = "发送";
            buttonText.fontSize = 20;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;

            RectTransform buttonTextRect = buttonText.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
        }

        private GameObject CreatePlaceholder(string text)
        {
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(inputField.transform);

            Text placeholderText = placeholder.AddComponent<Text>();
            placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholderText.text = text;
            placeholderText.fontSize = 18;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.alignment = TextAnchor.MiddleLeft;

            RectTransform rect = placeholder.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(5, 0);
            rect.offsetMax = new Vector2(0, 0);

            return placeholder;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        private void OnSendMessage()
        {
            if (inputField == null || string.IsNullOrEmpty(inputField.text))
                return;

            string question = inputField.text;
            AddMessage("user", question);
            inputField.text = "";

            // 查找匹配的回答
            string answer = FindAnswer(question);
            AddMessage("guide", answer);
        }

        /// <summary>
        /// 输入提交处理
        /// </summary>
        private void OnInputSubmit(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnSendMessage();
            }
        }

        /// <summary>
        /// 添加消息到聊天界面
        /// </summary>
        public void AddMessage(string sender, string message)
        {
            if (chatContent == null) return;

            GameObject messageObj = new GameObject("Message");
            messageObj.transform.SetParent(chatContent);

            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0, 1);
            messageRect.anchorMax = new Vector2(1, 1);
            messageRect.sizeDelta = new Vector2(0, 60);

            // 消息背景
            Image messageBg = messageObj.AddComponent<Image>();
            if (sender == "user")
            {
                messageBg.color = new Color(0.2f, 0.6f, 1f, 0.3f);
            }
            else if (sender == "system")
            {
                messageBg.color = new Color(1f, 0.8f, 0f, 0.3f);
            }
            else
            {
                messageBg.color = new Color(0.3f, 0.3f, 0.35f, 0.5f);
            }

            // 消息文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(messageObj.transform);

            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = message;
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0);
            textRect.anchorMax = new Vector2(0.95f, 1);
            textRect.offsetMin = new Vector2(0, 5);
            textRect.offsetMax = new Vector2(0, -5);

            LayoutElement layout = messageObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 60;

            ContentSizeFitter fitter = textObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 滚动到底部
            if (chatScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                chatScroll.verticalNormalizedPosition = 0;
            }
        }

        /// <summary>
        /// 查找回答
        /// </summary>
        private string FindAnswer(string question)
        {
            question = question.ToLower();

            // 查找匹配的对话示例
            foreach (var example in conversationExamples)
            {
                if (example.question.ToLower().Contains(question) ||
                    question.Contains(example.question.ToLower()) ||
                    IsSimilarQuestion(question, example.question.ToLower()))
                {
                    return example.answer;
                }
            }

            // 默认回答
            return "感谢您的问题！关于这个话题，我建议您可以：\n\n• 查看相关的旅游景点信息\n• 尝试体验不同的旅游路线\n• 使用推荐功能发现更多有趣的地方\n\n还有什么我可以帮助您的吗？";
        }

        /// <summary>
        /// 判断问题是否相似
        /// </summary>
        private bool IsSimilarQuestion(string input, string target)
        {
            // 简单的关键词匹配
            string[] keywords = target.Split(' ', '，', '？', '?');
            int matchCount = 0;

            foreach (var keyword in keywords)
            {
                if (input.Contains(keyword) || keyword.Contains(input))
                {
                    matchCount++;
                }
            }

            return matchCount >= 2;
        }

        /// <summary>
        /// 显示随机对话
        /// </summary>
        public void ShowRandomConversation()
        {
            if (conversationExamples.Count > 0)
            {
                var example = conversationExamples[Random.Range(0, conversationExamples.Count)];
                AddMessage("user", example.question);
                AddMessage("guide", example.answer);
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    /// <summary>
    /// 对话示例数据
    /// </summary>
    [System.Serializable]
    public class ConversationExample
    {
        public string question;
        [TextArea(3, 8)]
        public string answer;
    }
}
