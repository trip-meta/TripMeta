using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using TripMeta.Core.DependencyInjection;
using TripMeta.Core.ErrorHandling;
using TripMeta.AI.NPC;

namespace TripMeta.AI
{
    /// <summary>
    /// AI服务安装器 - 注册所有AI服务到DI容器
    /// </summary>
    public static class AIServiceInstaller
    {
        /// <summary>
        /// 异步安装AI服务（推荐）- 等待服务初始化完成后再注册
        /// </summary>
        public static async Task InstallServicesAsync(IServiceContainer container)
        {
            try
            {
                Logger.LogInfo("Installing AI services asynchronously...", "AIServiceInstaller");

                // 注册LLM服务 — 使用智谱AI GLM（实现IGPTService接口）
                if (!container.IsRegistered<IGPTService>())
                {
                    var gptConfig = LoadGPTConfig();
                    var glmService = new GLMService(gptConfig);
                    await glmService.InitializeAsync(); // 等待初始化完成
                    container.RegisterSingleton<IGPTService>(glmService);
                    Logger.LogInfo("GLM Service initialized and registered as IGPTService", "AIServiceInstaller");
                }

                // 注册Azure语音服务
                if (!container.IsRegistered<IAzureSpeechService>())
                {
                    var speechConfig = LoadSpeechConfig();
                    var speechService = new AzureSpeechService(speechConfig);
                    await speechService.InitializeAsync();
                    container.RegisterSingleton<IAzureSpeechService>(speechService);
                    Logger.LogInfo("Azure Speech Service initialized and registered", "AIServiceInstaller");
                }

                // 注册计算机视觉服务
                if (!container.IsRegistered<IComputerVisionService>())
                {
                    var visionConfig = LoadVisionConfig();
                    var visionService = new ComputerVisionService(visionConfig);
                    await visionService.InitializeAsync();
                    container.RegisterSingleton<IComputerVisionService>(visionService);
                    Logger.LogInfo("Computer Vision Service initialized and registered", "AIServiceInstaller");
                }

                // 注册推荐服务
                if (!container.IsRegistered<IRecommendationService>())
                {
                    var recommendationConfig = LoadRecommendationConfig();
                    var recommendationService = new RecommendationService(recommendationConfig);
                    await recommendationService.InitializeAsync();
                    container.RegisterSingleton<IRecommendationService>(recommendationService);
                    Logger.LogInfo("Recommendation Service initialized and registered", "AIServiceInstaller");
                }

                Logger.LogInfo("AI services installation completed", "AIServiceInstaller");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to install AI services");
                throw;
            }
        }

        /// <summary>
        /// 安装AI服务（同步版本 - 仅用于初始化顺序不敏感的场景）
        /// 注意：此版本不等待服务初始化完成，使用时需要确保服务在使用前已完成初始化
        /// </summary>
        public static void InstallServices(IServiceContainer container)
        {
            try
            {
                Logger.LogInfo("Installing AI services (sync)...", "AIServiceInstaller");

                // 注册LLM服务 — 使用智谱AI GLM（实现IGPTService接口）
                if (!container.IsRegistered<IGPTService>())
                {
                    var gptConfig = LoadGPTConfig();
                    var glmService = new GLMService(gptConfig);
                    // 异步初始化，不等待
                    _ = InitializeServiceAsync(glmService, container, "IGPTService");
                    container.RegisterSingleton<IGPTService>(glmService);
                    Logger.LogInfo("GLM Service registered as IGPTService (initialization pending)", "AIServiceInstaller");
                }

                // 其他服务...
                Logger.LogInfo("AI services installation completed (initialization pending)", "AIServiceInstaller");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to install AI services");
                throw;
            }
        }

        /// <summary>
        /// 异步初始化服务并记录结果
        /// </summary>
        private static async Task InitializeServiceAsync<T>(IAIService service, IServiceContainer container, string serviceName) where T : class
        {
            try
            {
                await service.InitializeAsync();
                Logger.LogInfo($"{serviceName} initialized successfully", "AIServiceInstaller");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Failed to initialize {serviceName}");
            }
        }
        
        /// <summary>
        /// 安装NPC服务
        /// </summary>
        public static void InstallNPCServices(IServiceContainer container)
        {
            try
            {
                Logger.LogInfo("Installing NPC services...", "AIServiceInstaller");
                
                // NPC对话管理器会在场景中自动创建单例
                // 这里只需要确保DI容器可以解析即可
                
                Logger.LogInfo("NPC services installation completed", "AIServiceInstaller");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Failed to install NPC services");
                throw;
            }
        }
        
        /// <summary>
        /// 加载GPT配置
        /// </summary>
        private static GPTConfig LoadGPTConfig()
        {
            var config = new GPTConfig();

            // 优先从 ScriptableObject 加载
            var appSettings = Resources.Load<AppSettings>("Config/AppSettings");
            if (appSettings != null && appSettings.aiSettings != null)
            {
                config.apiKey = appSettings.aiSettings.openAIApiKey;
                config.model = appSettings.aiSettings.gptModel ?? "glm-4-flash-250414";
                config.maxTokens = appSettings.aiSettings.maxTokens;
                config.temperature = appSettings.aiSettings.temperature;
            }

            // 从 secrets.json 加载 API Key（gitignored，不进版本控制）
            if (string.IsNullOrEmpty(config.apiKey))
            {
                config.apiKey = LoadApiKeyFromSecrets();
            }

            return config;
        }

        private static string LoadApiKeyFromSecrets()
        {
            var secretsPath = Path.Combine(Application.dataPath, "..", "secrets.json");
            if (!File.Exists(secretsPath)) return "";

            try
            {
                var json = File.ReadAllText(secretsPath);
                var secrets = JsonConvert.DeserializeObject<dynamic>(json);
                var key = secrets?.glm_api_key?.ToString() ?? "";
                if (!string.IsNullOrEmpty(key))
                    Logger.LogInfo("API Key loaded from secrets.json", "AIServiceInstaller");
                return key;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to load secrets.json: {ex.Message}", "AIServiceInstaller");
                return "";
            }
        }
        
        /// <summary>
        /// 加载语音配置
        /// </summary>
        private static AzureSpeechConfig LoadSpeechConfig()
        {
            var config = new AzureSpeechConfig();
            
            var appSettings = Resources.Load<AppSettings>("Config/AppSettings");
            if (appSettings != null && appSettings.aiSettings != null)
            {
                config.subscriptionKey = appSettings.aiSettings.azureSpeechKey;
                config.region = appSettings.aiSettings.azureSpeechRegion ?? "eastus";
            }
            
            return config;
        }
        
        /// <summary>
        /// 加载视觉配置
        /// </summary>
        private static ComputerVisionConfig LoadVisionConfig()
        {
            var config = new ComputerVisionConfig();
            
            var appSettings = Resources.Load<AppSettings>("Config/AppSettings");
            if (appSettings != null && appSettings.aiSettings != null)
            {
                config.subscriptionKey = appSettings.aiSettings.azureVisionKey;
                config.endpoint = appSettings.aiSettings.azureVisionEndpoint;
            }
            
            return config;
        }
        
        /// <summary>
        /// 加载推荐配置
        /// </summary>
        private static RecommendationConfig LoadRecommendationConfig()
        {
            return new RecommendationConfig();
        }
    }
}
