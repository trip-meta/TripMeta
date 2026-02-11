using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Core.Configuration;
using TripMeta.Core.ErrorHandling;
using TripMeta.Core.DependencyInjection;

namespace TripMeta.AI
{
    /// <summary>
    /// AI服务管理器 - 统一管理所有AI服务
    /// </summary>
    public class AIServiceManager : MonoBehaviour, IAIServiceManager
    {
        [Header("AI服务配置")]
        [SerializeField] private AIServiceConfig config;
        
        private readonly Dictionary<Type, IAIService> services = new Dictionary<Type, IAIService>();
        private bool isInitialized = false;
        
        // AI服务实例
        private IGPTService gptService;
        private IAzureSpeechService speechService;
        private IComputerVisionService visionService;
        private IRecommendationService recommendationService;
        
        public bool IsInitialized => isInitialized;
        public event Action<bool> OnInitializationStatusChanged;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        private async void Start()
        {
            await InitializeAsync();
        }
        
        /// <summary>
        /// 初始化AI服务管理器
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Logger.LogInfo("开始初始化AI服务管理器...", "AI");
                
                // 获取配置
                if (config == null)
                {
                    config = ServiceLocator.Get<AppSettings>()?.aiSettings?.ToAIServiceConfig() ?? 
                             CreateDefaultConfig();
                }
                
                // 初始化各个AI服务
                await InitializeGPTService();
                await InitializeSpeechService();
                await InitializeVisionService();
                await InitializeRecommendationService();
                
                isInitialized = true;
                OnInitializationStatusChanged?.Invoke(true);
                
                Logger.LogInfo("AI服务管理器初始化完成", "AI");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "AI服务管理器初始化失败");
                OnInitializationStatusChanged?.Invoke(false);
                throw;
            }
        }
        
        /// <summary>
        /// 初始化GPT服务
        /// </summary>
        private async Task InitializeGPTService()
        {
            try
            {
                gptService = new GPTService(config.gptConfig);
                await gptService.InitializeAsync();
                RegisterService(gptService);
                
                Logger.LogInfo("GPT服务初始化完成", "AI");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "GPT服务初始化失败");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化语音服务
        /// </summary>
        private async Task InitializeSpeechService()
        {
            try
            {
                speechService = new AzureSpeechService(config.speechConfig);
                await speechService.InitializeAsync();
                RegisterService(speechService);
                
                Logger.LogInfo("Azure语音服务初始化完成", "AI");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Azure语音服务初始化失败");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化计算机视觉服务
        /// </summary>
        private async Task InitializeVisionService()
        {
            try
            {
                visionService = new ComputerVisionService(config.visionConfig);
                await visionService.InitializeAsync();
                RegisterService(visionService);
                
                Logger.LogInfo("计算机视觉服务初始化完成", "AI");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "计算机视觉服务初始化失败");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化推荐服务
        /// </summary>
        private async Task InitializeRecommendationService()
        {
            try
            {
                recommendationService = new RecommendationService(config.recommendationConfig);
                await recommendationService.InitializeAsync();
                RegisterService(recommendationService);
                
                Logger.LogInfo("推荐服务初始化完成", "AI");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "推荐服务初始化失败");
                throw;
            }
        }
        
        /// <summary>
        /// 注册AI服务
        /// </summary>
        private void RegisterService<T>(T service) where T : IAIService
        {
            var serviceType = typeof(T);
            services[serviceType] = service;
            
            // 同时注册到DI容器
            if (ServiceLocator.IsInitialized)
            {
                var container = ServiceLocator.Get<IServiceContainer>();
                container.RegisterSingleton<T>(service);
            }
        }
        
        /// <summary>
        /// 获取AI服务
        /// </summary>
        public T GetService<T>() where T : class, IAIService
        {
            var serviceType = typeof(T);
            
            if (services.TryGetValue(serviceType, out var service))
            {
                return service as T;
            }
            
            // 尝试从具体实现获取
            if (typeof(T) == typeof(IGPTService)) return gptService as T;
            if (typeof(T) == typeof(IAzureSpeechService)) return speechService as T;
            if (typeof(T) == typeof(IComputerVisionService)) return visionService as T;
            if (typeof(T) == typeof(IRecommendationService)) return recommendationService as T;
            
            throw new InvalidOperationException($"AI服务 {typeof(T).Name} 未找到");
        }
        
        /// <summary>
        /// 检查服务是否可用
        /// </summary>
        public bool IsServiceAvailable<T>() where T : class, IAIService
        {
            try
            {
                var service = GetService<T>();
                return service?.IsInitialized ?? false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 获取所有已注册的服务
        /// </summary>
        public IEnumerable<IAIService> GetAllServices()
        {
            return services.Values;
        }
        
        /// <summary>
        /// 重新初始化指定服务
        /// </summary>
        public async Task ReinitializeServiceAsync<T>() where T : class, IAIService
        {
            try
            {
                var service = GetService<T>();
                await service.ReinitializeAsync();
                
                Logger.LogInfo($"服务 {typeof(T).Name} 重新初始化完成", "AI");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"服务 {typeof(T).Name} 重新初始化失败");
                throw;
            }
        }
        
        /// <summary>
        /// 获取服务健康状态
        /// </summary>
        public async Task<Dictionary<string, bool>> GetServicesHealthAsync()
        {
            var healthStatus = new Dictionary<string, bool>();
            
            foreach (var kvp in services)
            {
                try
                {
                    var isHealthy = await kvp.Value.CheckHealthAsync();
                    healthStatus[kvp.Key.Name] = isHealthy;
                }
                catch
                {
                    healthStatus[kvp.Key.Name] = false;
                }
            }
            
            return healthStatus;
        }
        
        /// <summary>
        /// 创建默认配置
        /// </summary>
        private AIServiceConfig CreateDefaultConfig()
        {
            return new AIServiceConfig
            {
                gptConfig = new GPTConfig
                {
                    apiKey = "your-openai-api-key",
                    model = "gpt-4",
                    maxTokens = 2000,
                    temperature = 0.7f,
                    requestTimeout = 30f
                },
                speechConfig = new AzureSpeechConfig
                {
                    subscriptionKey = "your-azure-speech-key",
                    region = "eastus",
                    language = "zh-CN",
                    voiceName = "zh-CN-XiaoxiaoNeural"
                },
                visionConfig = new ComputerVisionConfig
                {
                    subscriptionKey = "your-azure-vision-key",
                    endpoint = "https://your-region.api.cognitive.microsoft.com/",
                    confidenceThreshold = 0.7f
                },
                recommendationConfig = new RecommendationConfig
                {
                    maxRecommendations = 10,
                    similarityThreshold = 0.6f,
                    enableCollaborativeFiltering = true,
                    enableContentBasedFiltering = true
                }
            };
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public async Task DisposeAsync()
        {
            try
            {
                Logger.LogInfo("开始释放AI服务资源...", "AI");
                
                foreach (var service in services.Values)
                {
                    try
                    {
                        await service.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, $"释放服务 {service.GetType().Name} 失败");
                    }
                }
                
                services.Clear();
                isInitialized = false;
                OnInitializationStatusChanged?.Invoke(false);
                
                Logger.LogInfo("AI服务资源释放完成", "AI");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "AI服务资源释放失败");
            }
        }
        
        private void OnDestroy()
        {
            _ = DisposeAsync();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 应用暂停时暂停AI服务
                foreach (var service in services.Values)
                {
                    try
                    {
                        service.Pause();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, $"暂停服务 {service.GetType().Name} 失败");
                    }
                }
            }
            else
            {
                // 应用恢复时恢复AI服务
                foreach (var service in services.Values)
                {
                    try
                    {
                        service.Resume();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, $"恢复服务 {service.GetType().Name} 失败");
                    }
                }
            }
        }
    }
}