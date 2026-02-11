using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Core;

namespace TripMeta.AI
{
    /// <summary>
    /// AI智能导游系统 - 提供个性化、多语言的智能导游服务
    /// </summary>
    public class AITourGuide : MonoBehaviour
    {
        [Header("Guide Configuration")]
        public TourGuidePersonality personality;
        public string[] specialties = { "历史", "艺术", "建筑", "文化" };
        public Language defaultLanguage = Language.Chinese;
        
        [Header("Voice Settings")]
        public VoiceProfile voiceProfile;
        public float speechSpeed = 1.0f;
        public float speechVolume = 0.8f;
        
        [Header("Knowledge Base")]
        public KnowledgeGraphConfig knowledgeConfig;
        public bool enableRealTimeFactCheck = true;
        public bool enablePersonalization = true;
        
        [Header("Interaction")]
        public bool enableVoiceInteraction = true;
        public bool enableGestureRecognition = true;
        public float responseTimeout = 10f;
        
        // 私有变量
        private AIServiceManager aiManager;
        private UserProfile currentUser;
        private LocationContext currentLocation;
        private ConversationHistory conversationHistory;
        private KnowledgeGraph knowledgeGraph;
        
        // 状态管理
        private bool isListening = false;
        private bool isSpeaking = false;
        private bool isProcessingQuery = false;
        
        // 事件
        public event Action<string> OnGuideSpeak;
        public event Action<string> OnUserSpeak;
        public event Action<GuideState> OnGuideStateChanged;
        public event Action<string> OnGuideError;
        
        public static AITourGuide Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGuide();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化AI导游
        /// </summary>
        private void InitializeGuide()
        {
            aiManager = AIServiceManager.Instance;
            conversationHistory = new ConversationHistory();
            knowledgeGraph = new KnowledgeGraph(knowledgeConfig);
            
            Debug.Log("[AITourGuide] AI导游系统初始化完成");
        }
        
        void Start()
        {
            StartCoroutine(InitializeGuideAsync());
        }
        
        /// <summary>
        /// 异步初始化导游
        /// </summary>
        private async System.Collections.IEnumerator InitializeGuideAsync()
        {
            yield return new WaitUntil(() => aiManager != null && aiManager.isInitialized);
            
            try
            {
                await LoadKnowledgeGraph();
                await InitializePersonality();
                
                SetGuideState(GuideState.Ready);
                
                // 欢迎语
                await SpeakWelcomeMessage();
            }
            catch (Exception e)
            {
                Debug.LogError($"[AITourGuide] 导游初始化失败: {e.Message}");
                OnGuideError?.Invoke($"导游初始化失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 加载知识图谱
        /// </summary>
        private async Task LoadKnowledgeGraph()
        {
            Debug.Log("[AITourGuide] 加载知识图谱...");
            await knowledgeGraph.LoadAsync();
            Debug.Log("[AITourGuide] 知识图谱加载完成");
        }
        
        /// <summary>
        /// 初始化导游个性
        /// </summary>
        private async Task InitializePersonality()
        {
            var personalityPrompt = GeneratePersonalityPrompt();
            
            var request = new LLMRequest
            {
                prompt = personalityPrompt,
                maxTokens = 100,
                temperature = 0.7f
            };
            
            var response = await aiManager.SendAIRequestAsync<LLMResponse>(AIServiceType.LLM, request);
            
            if (response.success)
            {
                personality.description = response.generatedText;
                Debug.Log($"[AITourGuide] 导游个性初始化: {personality.description}");
            }
        }
        
        /// <summary>
        /// 生成个性化提示词
        /// </summary>
        private string GeneratePersonalityPrompt()
        {
            return $@"你是一位专业的VR旅游导游，具有以下特征：
- 姓名: {personality.name}
- 专长: {string.Join(", ", specialties)}
- 性格: {personality.traits}
- 语言风格: {personality.speakingStyle}

请用一段话描述你的导游风格和服务理念。";
        }
        
        /// <summary>
        /// 设置用户档案
        /// </summary>
        public void SetUserProfile(UserProfile user)
        {
            currentUser = user;
            
            // 根据用户偏好调整导游风格
            AdaptToUserPreferences();
            
            Debug.Log($"[AITourGuide] 用户档案已设置: {user.name}");
        }
        
        /// <summary>
        /// 设置当前位置
        /// </summary>
        public void SetCurrentLocation(LocationContext location)
        {
            currentLocation = location;
            
            // 加载位置相关知识
            _ = LoadLocationKnowledge(location);
            
            Debug.Log($"[AITourGuide] 当前位置: {location.locationName}");
        }
        
        /// <summary>
        /// 适应用户偏好
        /// </summary>
        private void AdaptToUserPreferences()
        {
            if (currentUser == null) return;
            
            // 调整语速
            speechSpeed = currentUser.preferredSpeechSpeed;
            
            // 调整语言
            if (currentUser.preferredLanguage != Language.Auto)
            {
                defaultLanguage = currentUser.preferredLanguage;
            }
            
            // 调整内容深度
            personality.detailLevel = currentUser.preferredDetailLevel;
        }
        
        /// <summary>
        /// 加载位置知识
        /// </summary>
        private async Task LoadLocationKnowledge(LocationContext location)
        {
            try
            {
                var locationKnowledge = await knowledgeGraph.GetLocationKnowledgeAsync(location.locationId);
                
                // 缓存位置知识以提高响应速度
                conversationHistory.AddLocationContext(location, locationKnowledge);
                
                Debug.Log($"[AITourGuide] 位置知识加载完成: {location.locationName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AITourGuide] 位置知识加载失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 处理用户查询
        /// </summary>
        public async Task<string> ProcessUserQuery(string userInput)
        {
            if (isProcessingQuery)
            {
                return "请稍等，我正在处理您的问题...";
            }
            
            isProcessingQuery = true;
            SetGuideState(GuideState.Thinking);
            
            try
            {
                OnUserSpeak?.Invoke(userInput);
                
                // 1. 理解用户意图
                var intent = await AnalyzeUserIntent(userInput);
                
                // 2. 获取相关知识
                var knowledge = await GetRelevantKnowledge(intent);
                
                // 3. 生成个性化回答
                var response = await GeneratePersonalizedResponse(intent, knowledge);
                
                // 4. 事实核查 (如果启用)
                if (enableRealTimeFactCheck)
                {
                    response = await FactCheckResponse(response);
                }
                
                // 5. 记录对话历史
                conversationHistory.AddExchange(userInput, response);
                
                SetGuideState(GuideState.Speaking);
                OnGuideSpeak?.Invoke(response);
                
                return response;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AITourGuide] 查询处理失败: {e.Message}");
                OnGuideError?.Invoke($"抱歉，我遇到了一些问题: {e.Message}");
                return "抱歉，我暂时无法回答您的问题，请稍后再试。";
            }
            finally
            {
                isProcessingQuery = false;
                SetGuideState(GuideState.Ready);
            }
        }
        
        /// <summary>
        /// 分析用户意图
        /// </summary>
        private async Task<UserIntent> AnalyzeUserIntent(string userInput)
        {
            var intentPrompt = $@"分析以下用户输入的意图类型：
用户输入: ""{userInput}""
当前位置: {currentLocation?.locationName ?? "未知"}

请识别意图类型：
1. 询问历史 (HISTORY)
2. 询问文化 (CULTURE) 
3. 寻求推荐 (RECOMMENDATION)
4. 导航帮助 (NAVIGATION)
5. 一般对话 (GENERAL)

返回JSON格式: {{""intent"": ""类型"", ""confidence"": 0.95, ""entities"": [""实体1"", ""实体2""]}}";
            
            var request = new LLMRequest
            {
                prompt = intentPrompt,
                maxTokens = 200,
                temperature = 0.3f
            };
            
            var response = await aiManager.SendAIRequestAsync<LLMResponse>(AIServiceType.LLM, request);
            
            if (response.success)
            {
                try
                {
                    var intent = JsonUtility.FromJson<UserIntent>(response.generatedText);
                    return intent;
                }
                catch
                {
                    // 解析失败，返回默认意图
                    return new UserIntent { intent = "GENERAL", confidence = 0.5f };
                }
            }
            
            return new UserIntent { intent = "GENERAL", confidence = 0.5f };
        }
        
        /// <summary>
        /// 获取相关知识
        /// </summary>
        private async Task<string> GetRelevantKnowledge(UserIntent intent)
        {
            var knowledge = "";
            
            // 从知识图谱获取相关信息
            if (currentLocation != null)
            {
                knowledge += await knowledgeGraph.QueryKnowledgeAsync(
                    currentLocation.locationId, 
                    intent.intent, 
                    intent.entities
                );
            }
            
            // 从对话历史获取上下文
            var contextualInfo = conversationHistory.GetRelevantContext(intent);
            if (!string.IsNullOrEmpty(contextualInfo))
            {
                knowledge += "\n\n上下文信息:\n" + contextualInfo;
            }
            
            return knowledge;
        }
        
        /// <summary>
        /// 生成个性化回答
        /// </summary>
        private async Task<string> GeneratePersonalizedResponse(UserIntent intent, string knowledge)
        {
            var responsePrompt = GenerateResponsePrompt(intent, knowledge);
            
            var request = new LLMRequest
            {
                prompt = responsePrompt,
                maxTokens = 500,
                temperature = 0.7f
            };
            
            var response = await aiManager.SendAIRequestAsync<LLMResponse>(AIServiceType.LLM, request);
            
            if (response.success)
            {
                return response.generatedText;
            }
            
            return "抱歉，我暂时无法为您提供详细信息。";
        }
        
        /// <summary>
        /// 生成回答提示词
        /// </summary>
        private string GenerateResponsePrompt(UserIntent intent, string knowledge)
        {
            var userInfo = currentUser != null ? 
                $"用户偏好: {currentUser.interests}, 详细程度: {currentUser.preferredDetailLevel}" : 
                "用户信息未知";
            
            return $@"你是{personality.name}，一位{string.Join("、", specialties)}专家导游。

个性特征: {personality.traits}
说话风格: {personality.speakingStyle}
当前位置: {currentLocation?.locationName ?? "未知"}
{userInfo}

用户意图: {intent.intent}
相关知识: {knowledge}

请根据你的个性和专业知识，为用户提供有帮助、有趣且个性化的回答。
回答要求:
1. 符合你的导游个性
2. 内容准确且有用
3. 语言生动有趣
4. 适合VR环境体验

回答:";
        }
        
        /// <summary>
        /// 事实核查
        /// </summary>
        private async Task<string> FactCheckResponse(string response)
        {
            // 这里可以集成事实核查API
            // 暂时返回原始回答
            return response;
        }
        
        /// <summary>
        /// 语音合成并播放
        /// </summary>
        public async Task SpeakAsync(string text)
        {
            if (isSpeaking) return;
            
            isSpeaking = true;
            SetGuideState(GuideState.Speaking);
            
            try
            {
                var speechRequest = new SpeechRequest
                {
                    text = text,
                    voiceProfile = voiceProfile,
                    speed = speechSpeed,
                    volume = speechVolume,
                    language = defaultLanguage
                };
                
                var speechResponse = await aiManager.SendAIRequestAsync<SpeechResponse>(
                    AIServiceType.Speech, speechRequest);
                
                if (speechResponse.success)
                {
                    // 播放音频
                    await PlayAudioClip(speechResponse.audioClip);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AITourGuide] 语音合成失败: {e.Message}");
            }
            finally
            {
                isSpeaking = false;
                SetGuideState(GuideState.Ready);
            }
        }
        
        /// <summary>
        /// 播放音频剪辑
        /// </summary>
        private async Task PlayAudioClip(AudioClip clip)
        {
            if (clip == null) return;
            
            var audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.clip = clip;
            audioSource.Play();
            
            // 等待播放完成
            while (audioSource.isPlaying)
            {
                await Task.Delay(100);
            }
        }
        
        /// <summary>
        /// 说欢迎语
        /// </summary>
        private async Task SpeakWelcomeMessage()
        {
            var welcomeMessage = GenerateWelcomeMessage();
            await SpeakAsync(welcomeMessage);
        }
        
        /// <summary>
        /// 生成欢迎语
        /// </summary>
        private string GenerateWelcomeMessage()
        {
            var userName = currentUser?.name ?? "朋友";
            var locationName = currentLocation?.locationName ?? "这个美丽的地方";
            
            return $"你好，{userName}！我是{personality.name}，很高兴为您介绍{locationName}。" +
                   $"我专长于{string.Join("、", specialties)}，有什么想了解的尽管问我！";
        }
        
        /// <summary>
        /// 设置导游状态
        /// </summary>
        private void SetGuideState(GuideState state)
        {
            OnGuideStateChanged?.Invoke(state);
        }
        
        /// <summary>
        /// 开始语音监听
        /// </summary>
        public async Task StartListening()
        {
            if (!enableVoiceInteraction || isListening) return;
            
            isListening = true;
            SetGuideState(GuideState.Listening);
            
            try
            {
                var listenRequest = new VoiceRecognitionRequest
                {
                    language = defaultLanguage,
                    timeout = responseTimeout
                };
                
                var listenResponse = await aiManager.SendAIRequestAsync<VoiceRecognitionResponse>(
                    AIServiceType.Speech, listenRequest);
                
                if (listenResponse.success && !string.IsNullOrEmpty(listenResponse.recognizedText))
                {
                    var response = await ProcessUserQuery(listenResponse.recognizedText);
                    await SpeakAsync(response);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AITourGuide] 语音监听失败: {e.Message}");
            }
            finally
            {
                isListening = false;
                SetGuideState(GuideState.Ready);
            }
        }
        
        /// <summary>
        /// 停止语音监听
        /// </summary>
        public void StopListening()
        {
            isListening = false;
            SetGuideState(GuideState.Ready);
        }
        
        /// <summary>
        /// 获取推荐内容
        /// </summary>
        public async Task<List<Recommendation>> GetRecommendations()
        {
            if (currentLocation == null || currentUser == null)
            {
                return new List<Recommendation>();
            }
            
            var recommendationRequest = new RecommendationRequest
            {
                userId = currentUser.userId,
                locationId = currentLocation.locationId,
                userPreferences = currentUser.interests,
                currentContext = currentLocation
            };
            
            var response = await aiManager.SendAIRequestAsync<RecommendationResponse>(
                AIServiceType.Recommendation, recommendationRequest);
            
            return response.success ? response.recommendations : new List<Recommendation>();
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
    /// 导游状态
    /// </summary>
    public enum GuideState
    {
        Initializing,
        Ready,
        Listening,
        Thinking,
        Speaking,
        Error
    }
    
    /// <summary>
    /// 导游个性配置
    /// </summary>
    [Serializable]
    public class TourGuidePersonality
    {
        public string name = "小美";
        public string description;
        public string traits = "热情、专业、幽默";
        public string speakingStyle = "生动有趣、通俗易懂";
        public DetailLevel detailLevel = DetailLevel.Medium;
    }
    
    /// <summary>
    /// 用户意图
    /// </summary>
    [Serializable]
    public class UserIntent
    {
        public string intent;
        public float confidence;
        public string[] entities;
    }
}