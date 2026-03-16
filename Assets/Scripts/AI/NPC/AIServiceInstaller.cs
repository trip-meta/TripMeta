using System;
using UnityEngine;
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
        /// 安装AI服务
        /// </summary>
        public static void InstallServices(IServiceContainer container)
        {
            try
            {
                Logger.LogInfo("Installing AI services...", "AIServiceInstaller");
                
                // 注册GPT服务
                if (!container.IsRegistered<IGPTService>())
                {
                    var gptConfig = LoadGPTConfig();
                    var gptService = new GPTService(gptConfig);
                    container.RegisterSingleton<IGPTService>(gptService);
                    Logger.LogInfo("GPT Service registered", "AIServiceInstaller");
                }
                
                // 注册Azure语音服务
                if (!container.IsRegistered<IAzureSpeechService>())
                {
                    var speechConfig = LoadSpeechConfig();
                    var speechService = new AzureSpeechService(speechConfig);
                    container.RegisterSingleton<IAzureSpeechService>(speechService);
                    Logger.LogInfo("Azure Speech Service registered", "AIServiceInstaller");
                }
                
                // 注册计算机视觉服务
                if (!container.IsRegistered<IComputerVisionService>())
                {
                    var visionConfig = LoadVisionConfig();
                    var visionService = new ComputerVisionService(visionConfig);
                    container.RegisterSingleton<IComputerVisionService>(visionService);
                    Logger.LogInfo("Computer Vision Service registered", "AIServiceInstaller");
                }
                
                // 注册推荐服务
                if (!container.IsRegistered<IRecommendationService>())
                {
                    var recommendationConfig = LoadRecommendationConfig();
                    var recommendationService = new RecommendationService(recommendationConfig);
                    container.RegisterSingleton<IRecommendationService>(recommendationService);
                    Logger.LogInfo("Recommendation Service registered", "AIServiceInstaller");
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
            
            // 从配置文件加载（如果有）
            var appSettings = Resources.Load<AppSettings>("Config/AppSettings");
            if (appSettings != null && appSettings.aiSettings != null)
            {
                config.apiKey = appSettings.aiSettings.openAIApiKey;
                config.model = appSettings.aiSettings.gptModel ?? "gpt-4o";
                config.maxTokens = appSettings.aiSettings.maxTokens;
                config.temperature = appSettings.aiSettings.temperature;
            }
            
            return config;
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
