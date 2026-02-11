using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace TripMeta.AI
{
    /// <summary>
    /// AI服务管理器 - 统一管理所有AI服务
    /// </summary>
    public class AIServiceManager : MonoBehaviour
    {
        [Header("AI Configuration")]
        public AIConfig aiConfig;
        public bool enableEdgeAI = true;
        public int maxConcurrentRequests = 5;
        public float requestTimeout = 30f;
        
        [Header("Service Status")]
        public bool isInitialized = false;
        public AIServiceStatus serviceStatus;
        
        // AI服务实例
        private Dictionary<AIServiceType, IAIService> services;
        private Queue<AIRequest> requestQueue;
        private int activeRequests = 0;
        
        // 事件
        public event Action<AIServiceType, bool> OnServiceStatusChanged;
        public event Action<AIResponse> OnAIResponseReceived;
        public event Action<string> OnAIError;
        
        public static AIServiceManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化AI服务管理器
        /// </summary>
        private void InitializeManager()
        {
            services = new Dictionary<AIServiceType, IAIService>();
            requestQueue = new Queue<AIRequest>();
            serviceStatus = new AIServiceStatus();
            
            Debug.Log("[AIServiceManager] AI服务管理器初始化完成");
        }
        
        async void Start()
        {
            await InitializeAIServices();
        }
        
        /// <summary>
        /// 初始化所有AI服务
        /// </summary>
        private async Task InitializeAIServices()
        {
            try
            {
                Debug.Log("[AIServiceManager] 开始初始化AI服务...");
                
                // 初始化各种AI服务
                await InitializeLLMService();
                await InitializeSpeechService();
                await InitializeVisionService();
                await InitializeRecommendationService();
                await InitializeTranslationService();
                
                // 预热模型
                await PrewarmModels();
                
                isInitialized = true;
                Debug.Log("[AIServiceManager] 所有AI服务初始化完成");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIServiceManager] AI服务初始化失败: {e.Message}");
                OnAIError?.Invoke($"AI服务初始化失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 初始化大语言模型服务
        /// </summary>
        private async Task InitializeLLMService()
        {
            try
            {
                var gptConfig = aiConfig?.openAIConfig ?? new GPTConfig();
                var llmService = new OpenAIService(gptConfig);
                await llmService.InitializeAsync();

                services[AIServiceType.LLM] = llmService;
                serviceStatus.llmStatus = true;

                OnServiceStatusChanged?.Invoke(AIServiceType.LLM, true);
                Debug.Log("[AIServiceManager] LLM服务初始化成功");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIServiceManager] LLM服务初始化失败: {e.Message}");
                serviceStatus.llmStatus = false;
            }
        }

        /// <summary>
        /// 初始化语音服务
        /// </summary>
        private async Task InitializeSpeechService()
        {
            try
            {
                var speechConfig = aiConfig?.azureSpeechConfig ?? new AzureSpeechConfig();
                var speechService = new AzureSpeechService(speechConfig);
                await speechService.InitializeAsync();

                services[AIServiceType.Speech] = speechService;
                serviceStatus.speechStatus = true;

                OnServiceStatusChanged?.Invoke(AIServiceType.Speech, true);
                Debug.Log("[AIServiceManager] 语音服务初始化成功");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIServiceManager] 语音服务初始化失败: {e.Message}");
                serviceStatus.speechStatus = false;
            }
        }

        /// <summary>
        /// 初始化计算机视觉服务
        /// </summary>
        private async Task InitializeVisionService()
        {
            try
            {
                var visionConfig = aiConfig?.visionConfig ?? new ComputerVisionConfig();
                var visionService = new ComputerVisionService(visionConfig);
                await visionService.InitializeAsync();

                services[AIServiceType.Vision] = visionService;
                serviceStatus.visionStatus = true;

                OnServiceStatusChanged?.Invoke(AIServiceType.Vision, true);
                Debug.Log("[AIServiceManager] 视觉服务初始化成功");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIServiceManager] 视觉服务初始化失败: {e.Message}");
                serviceStatus.visionStatus = false;
            }
        }

        /// <summary>
        /// 初始化推荐服务
        /// </summary>
        private async Task InitializeRecommendationService()
        {
            try
            {
                var recConfig = aiConfig?.recommendationConfig ?? new RecommendationConfig();
                var recommendationService = new RecommendationService(recConfig);
                await recommendationService.InitializeAsync();

                services[AIServiceType.Recommendation] = recommendationService;
                serviceStatus.recommendationStatus = true;

                OnServiceStatusChanged?.Invoke(AIServiceType.Recommendation, true);
                Debug.Log("[AIServiceManager] 推荐服务初始化成功");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIServiceManager] 推荐服务初始化失败: {e.Message}");
                serviceStatus.recommendationStatus = false;
            }
        }

        /// <summary>
        /// 初始化翻译服务
        /// </summary>
        private async Task InitializeTranslationService()
        {
            try
            {
                var transConfig = aiConfig?.translationConfig ?? new TranslationConfig();
                var translationService = new TranslationService(transConfig);
                await translationService.InitializeAsync();

                services[AIServiceType.Translation] = translationService;
                serviceStatus.translationStatus = true;

                OnServiceStatusChanged?.Invoke(AIServiceType.Translation, true);
                Debug.Log("[AIServiceManager] 翻译服务初始化成功");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIServiceManager] 翻译服务初始化失败: {e.Message}");
                serviceStatus.translationStatus = false;
            }
        }
        
        /// <summary>
        /// 预热AI模型
        /// </summary>
        private async Task PrewarmModels()
        {
            Debug.Log("[AIServiceManager] 开始预热AI模型...");
            
            var prewarmTasks = new List<Task>();
            
            foreach (var service in services.Values)
            {
                if (service != null)
                {
                    prewarmTasks.Add(service.PrewarmAsync());
                }
            }
            
            await Task.WhenAll(prewarmTasks);
            Debug.Log("[AIServiceManager] AI模型预热完成");
        }
        
        /// <summary>
        /// 发送AI请求
        /// </summary>
        public async Task<T> SendAIRequestAsync<T>(AIServiceType serviceType, AIRequest request) where T : AIResponse
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("AI服务尚未初始化");
            }
            
            if (!services.ContainsKey(serviceType) || services[serviceType] == null)
            {
                throw new InvalidOperationException($"AI服务 {serviceType} 不可用");
            }
            
            // 检查并发请求限制
            if (activeRequests >= maxConcurrentRequests)
            {
                requestQueue.Enqueue(request);
                await WaitForQueueProcessing();
            }
            
            activeRequests++;
            
            try
            {
                var service = services[serviceType];
                var response = await service.ProcessRequestAsync<T>(request);
                
                OnAIResponseReceived?.Invoke(response);
                return response;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIServiceManager] AI请求处理失败: {e.Message}");
                OnAIError?.Invoke($"AI请求失败: {e.Message}");
                throw;
            }
            finally
            {
                activeRequests--;
                ProcessQueue();
            }
        }
        
        /// <summary>
        /// 等待队列处理
        /// </summary>
        private async Task WaitForQueueProcessing()
        {
            while (activeRequests >= maxConcurrentRequests)
            {
                await Task.Delay(100);
            }
        }
        
        /// <summary>
        /// 处理请求队列
        /// </summary>
        private void ProcessQueue()
        {
            if (requestQueue.Count > 0 && activeRequests < maxConcurrentRequests)
            {
                var nextRequest = requestQueue.Dequeue();
                // 异步处理下一个请求
                _ = Task.Run(async () => await ProcessQueuedRequest(nextRequest));
            }
        }
        
        /// <summary>
        /// 处理队列中的请求
        /// </summary>
        private async Task ProcessQueuedRequest(AIRequest request)
        {
            try
            {
                await SendAIRequestAsync<AIResponse>(request.ServiceType, request);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AIServiceManager] 队列请求处理失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 获取服务状态
        /// </summary>
        public bool IsServiceAvailable(AIServiceType serviceType)
        {
            return services.ContainsKey(serviceType) && 
                   services[serviceType] != null && 
                   services[serviceType].IsAvailable;
        }
        
        /// <summary>
        /// 获取所有服务状态
        /// </summary>
        public AIServiceStatus GetServiceStatus()
        {
            return serviceStatus;
        }
        
        /// <summary>
        /// 重启服务
        /// </summary>
        public async Task RestartService(AIServiceType serviceType)
        {
            Debug.Log($"[AIServiceManager] 重启服务: {serviceType}");
            
            if (services.ContainsKey(serviceType))
            {
                var service = services[serviceType];
                if (service != null)
                {
                    await service.ShutdownAsync();
                }
                services.Remove(serviceType);
            }
            
            // 重新初始化服务
            switch (serviceType)
            {
                case AIServiceType.LLM:
                    await InitializeLLMService();
                    break;
                case AIServiceType.Speech:
                    await InitializeSpeechService();
                    break;
                case AIServiceType.Vision:
                    await InitializeVisionService();
                    break;
                case AIServiceType.Recommendation:
                    await InitializeRecommendationService();
                    break;
                case AIServiceType.Translation:
                    await InitializeTranslationService();
                    break;
            }
        }
        
        void OnDestroy()
        {
            if (Instance == this)
            {
                ShutdownAllServices();
                Instance = null;
            }
        }
        
        /// <summary>
        /// 关闭所有服务
        /// </summary>
        private async void ShutdownAllServices()
        {
            Debug.Log("[AIServiceManager] 关闭所有AI服务...");
            
            var shutdownTasks = new List<Task>();
            
            foreach (var service in services.Values)
            {
                if (service != null)
                {
                    shutdownTasks.Add(service.ShutdownAsync());
                }
            }
            
            await Task.WhenAll(shutdownTasks);
            services.Clear();
            
            Debug.Log("[AIServiceManager] 所有AI服务已关闭");
        }
    }
    
    /// <summary>
    /// AI服务类型
    /// </summary>
    public enum AIServiceType
    {
        LLM,            // 大语言模型
        Speech,         // 语音服务
        Vision,         // 计算机视觉
        Recommendation, // 推荐系统
        Translation,    // 翻译服务
        SceneGeneration // 场景生成
    }
    
    /// <summary>
    /// AI服务状态
    /// </summary>
    [Serializable]
    public class AIServiceStatus
    {
        public bool llmStatus = false;
        public bool speechStatus = false;
        public bool visionStatus = false;
        public bool recommendationStatus = false;
        public bool translationStatus = false;
        public bool sceneGenerationStatus = false;
        
        public bool AllServicesReady => 
            llmStatus && speechStatus && visionStatus && 
            recommendationStatus && translationStatus;
    }
    
    /// <summary>
    /// AI配置
    /// </summary>
    [Serializable]
    public class AIConfig
    {
        public GPTConfig openAIConfig;
        public AzureSpeechConfig azureSpeechConfig;
        public ComputerVisionConfig visionConfig;
        public RecommendationConfig recommendationConfig;
        public TranslationConfig translationConfig;
    }
    
    /// <summary>
    /// AI服务接口
    /// </summary>
    public interface IAIService
    {
        bool IsAvailable { get; }
        Task InitializeAsync();
        Task<T> ProcessRequestAsync<T>(AIRequest request) where T : AIResponse;
        Task PrewarmAsync();
        Task ShutdownAsync();
    }
    
    /// <summary>
    /// AI请求基类
    /// </summary>
    [Serializable]
    public abstract class AIRequest
    {
        public string requestId = Guid.NewGuid().ToString();
        public DateTime timestamp = DateTime.UtcNow;
        public AIServiceType ServiceType { get; protected set; }
        public float priority = 1.0f;
    }
    
    /// <summary>
    /// AI响应基类
    /// </summary>
    [Serializable]
    public abstract class AIResponse
    {
        public string requestId;
        public DateTime timestamp = DateTime.UtcNow;
        public bool success;
        public string errorMessage;
        public float processingTime;
    }
}