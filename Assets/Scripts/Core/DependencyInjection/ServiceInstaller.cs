using UnityEngine;
using TripMeta.Core.Configuration;
using TripMeta.Core.ErrorHandling;
using TripMeta.Core.Performance;
using TripMeta.Infrastructure.Network;
using TripMeta.Infrastructure.Cache;
using TripMeta.Infrastructure.Resources;
using TripMeta.Features.TourGuide;
using TripMeta.Features.SceneGeneration;
using TripMeta.Features.Multiplayer;
using TripMeta.Features.AR;
using TripMeta.Features.MobileCompanion;
using TripMeta.AI;
using TripMeta.AI.Services;
using TripMeta.Interaction;
using TripMeta.VR.Platform;
using TripMeta.VR.WebXR;
using TripMeta.VR.Rendering;

namespace TripMeta.Core.DependencyInjection
{
    /// <summary>
    /// 服务安装器 - 负责注册所有服务
    /// </summary>
    public static class ServiceInstaller
    {
        /// <summary>
        /// 安装所有服务
        /// </summary>
        public static void InstallServices(IServiceContainer container, TripMetaConfig config)
        {
            Logger.LogInfo("开始安装服务...", "ServiceInstaller");
            
            // 注册配置
            InstallConfiguration(container, config);
            
            // 注册核心服务
            InstallCoreServices(container);
            
            // 注册基础设施服务
            InstallInfrastructureServices(container);
            
            // 注册AI服务
            InstallAIServices(container);
            
            // 注册功能服务
            InstallFeatureServices(container);
            
            // 注册VR服务
            InstallVRServices(container);
            
            Logger.LogInfo("服务安装完成", "ServiceInstaller");
        }
        
        /// <summary>
        /// 安装配置服务
        /// </summary>
        private static void InstallConfiguration(IServiceContainer container, TripMetaConfig config)
        {
            container.RegisterSingleton<TripMetaConfig>(config);
            
            // 创建并注册AppSettings
            var appSettings = ScriptableObject.CreateInstance<AppSettings>();
            if (config != null)
            {
                // 从TripMetaConfig转换到AppSettings
                appSettings.aiSettings = new AIServiceSettings
                {
                    maxRequestsPerMinute = config.aiConfig?.maxConcurrentRequests ?? 60,
                    requestTimeout = config.aiConfig?.requestTimeout ?? 30f
                };
                
                appSettings.vrSettings = new VRSettings
                {
                    targetFrameRate = config.vrConfig?.targetFrameRate ?? 72f,
                    enableFoveatedRendering = config.vrConfig?.enableFoveatedRendering ?? true,
                    enableDynamicResolution = config.vrConfig?.enableDynamicResolution ?? true
                };
                
                appSettings.performanceSettings = new PerformanceSettings
                {
                    enableProfiling = config.debugConfig?.enablePerformanceMonitoring ?? true,
                    maxDrawCalls = 1000,
                    maxTriangles = 100000
                };
                
                appSettings.networkSettings = new NetworkSettings
                {
                    baseApiUrl = config.networkConfig?.baseApiUrl ?? "",
                    connectionTimeout = (int)(config.networkConfig?.connectionTimeout ?? 30f),
                    maxRetries = config.networkConfig?.maxRetryAttempts ?? 3
                };
            }
            
            container.RegisterSingleton<AppSettings>(appSettings);
            
            Logger.LogInfo("配置服务安装完成", "ServiceInstaller");
        }
        
        /// <summary>
        /// 安装核心服务
        /// </summary>
        private static void InstallCoreServices(IServiceContainer container)
        {
            // 注册错误处理器
            container.RegisterSingleton<IErrorHandler, ErrorHandler>();
            
            // 注册性能监控器
            container.RegisterSingleton<PerformanceMonitor>(Object.FindObjectOfType<PerformanceMonitor>() ?? 
                new GameObject("PerformanceMonitor").AddComponent<PerformanceMonitor>());
            
            Logger.LogInfo("核心服务安装完成", "ServiceInstaller");
        }
        
        /// <summary>
        /// 安装基础设施服务
        /// </summary>
        private static void InstallInfrastructureServices(IServiceContainer container)
        {
            // 注册网络服务
            container.RegisterSingleton<INetworkService, NetworkService>();
            
            // 注册缓存服务
            container.RegisterSingleton<ICacheService, CacheService>();
            
            // 注册资源管理器
            container.RegisterSingleton<ResourceManager>(Object.FindObjectOfType<ResourceManager>() ?? 
                new GameObject("ResourceManager").AddComponent<ResourceManager>());
            
            Logger.LogInfo("基础设施服务安装完成", "ServiceInstaller");
        }
        
        /// <summary>
        /// 安装AI服务
        /// </summary>
        private static void InstallAIServices(IServiceContainer container)
        {
            // 注册AI服务管理器
            container.RegisterSingleton<IAIServiceManager, AIServiceManager>();

            // 注册AI导游
            container.RegisterSingleton<IAITourGuide, AITourGuide>();

            // 安装双引擎LLM服务 (Phase 1: AI双引擎架构)
            InstallDualEngineLLMService(container);

            // 安装边缘AI推理服务 (Phase 1: 边缘AI推理)
            InstallEdgeAIServices(container);

            // 注册翻译服务
            InstallTranslationService(container);

            Logger.LogInfo("AI服务安装完成", "ServiceInstaller");
        }

        /// <summary>
        /// 安装边缘AI推理服务 (ONNX Runtime + TensorRT)
        /// </summary>
        private static void InstallEdgeAIServices(IServiceContainer container)
        {
            // 查找或创建 EdgeAIInferenceManager
            var edgeAIManager = Object.FindObjectOfType<EdgeAIInferenceManager>();
            if (edgeAIManager == null)
            {
                var go = new GameObject("EdgeAIInferenceManager");
                edgeAIManager = go.AddComponent<EdgeAIInferenceManager>();
                edgeAIManager.inferenceThreads = 4;
                edgeAIManager.maxConcurrentInferences = 3;
                edgeAIManager.enableTensorRT = true;
                edgeAIManager.enableModelCaching = true;
                edgeAIManager.enablePerformanceMonitoring = true;

                // 配置默认模型
                edgeAIManager.modelConfigs = new System.Collections.Generic.List<EdgeAIModelConfig>
                {
                    new EdgeAIModelConfig
                    {
                        modelId = "intent-recognition",
                        modelName = "Intent Recognition",
                        modelPath = "Models/intent_recognition.onnx",
                        modelType = EdgeAIModelType.IntentRecognition,
                        preloadAtStartup = true,
                        enableQuantization = true,
                        quantizationType = QuantizationType.INT8,
                        inputSize = 224,
                        labels = new string[] { "greeting", "question", "command", "navigation", "general" }
                    },
                    new EdgeAIModelConfig
                    {
                        modelId = "emotion-recognition",
                        modelName = "Emotion Recognition",
                        modelPath = "Models/emotion_recognition.onnx",
                        modelType = EdgeAIModelType.EmotionRecognition,
                        preloadAtStartup = false,
                        enableQuantization = true,
                        quantizationType = QuantizationType.INT8,
                        labels = new string[] { "neutral", "happy", "sad", "angry", "surprised", "fearful" }
                    }
                };

                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 EdgeAIInferenceManager GameObject");
            }

            container.RegisterSingleton<EdgeAIInferenceManager>(edgeAIManager);
            Logger.LogInfo("边缘AI推理服务安装完成 (ONNX Runtime + TensorRT)", "ServiceInstaller");

            // 安装情感计算服务 (Phase 1: 情感计算系统)
            InstallEmotionRecognitionService(container);

            // 安装多模态交互服务 (Phase 1: 多模态交互增强)
            InstallMultimodalInteractionService(container);
        }

        /// <summary>
        /// 安装情感计算服务 (Phase 1: 情感计算系统)
        /// </summary>
        private static void InstallEmotionRecognitionService(IServiceContainer container)
        {
            // 查找或创建 EmotionRecognitionManager
            var emotionManager = Object.FindObjectOfType<EmotionRecognitionManager>();
            if (emotionManager == null)
            {
                var go = new GameObject("EmotionRecognitionManager");
                emotionManager = go.AddComponent<EmotionRecognitionManager>();
                emotionManager.enableVoiceEmotionRecognition = true;
                emotionManager.enableTextEmotionAnalysis = true;
                emotionManager.enableBehavioralEmotionDetection = true;
                emotionManager.emotionAnalysisInterval = 5f;
                emotionManager.emotionHistorySize = 10;
                emotionManager.emotionConfidenceThreshold = 0.7f;
                emotionManager.enableDebugLogs = false;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 EmotionRecognitionManager GameObject");
            }

            container.RegisterSingleton<EmotionRecognitionManager>(emotionManager);
            Logger.LogInfo("情感计算服务安装完成", "ServiceInstaller");
        }

        /// <summary>
        /// 安装多模态交互服务 (Phase 1: 多模态交互增强)
        /// 手势识别、视线追踪、语音合成
        /// </summary>
        private static void InstallMultimodalInteractionService(IServiceContainer container)
        {
            // 查找或创建 MultimodalInteractionManager
            var multimodalManager = Object.FindObjectOfType<MultimodalInteractionManager>();
            if (multimodalManager == null)
            {
                var go = new GameObject("MultimodalInteractionManager");
                multimodalManager = go.AddComponent<MultimodalInteractionManager>();
                multimodalManager.enableGestureRecognition = true;
                multimodalManager.enableEyeTracking = true;
                multimodalManager.enableVoiceSynthesis = true;
                multimodalManager.enableMultimodalFusion = true;
                multimodalManager.gestureConfidenceThreshold = 0.8f;
                multimodalManager.gazeDwellTime = 1.5f;
                multimodalManager.speechSpeed = 1.0f;
                multimodalManager.speechVolume = 0.8f;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 MultimodalInteractionManager GameObject");
            }

            container.RegisterSingleton<MultimodalInteractionManager>(multimodalManager);
            Logger.LogInfo("多模态交互服务安装完成 (手势识别、视线追踪、语音合成)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装双引擎LLM服务 (GPT-4 + Claude-3.5)
        /// </summary>
        private static void InstallDualEngineLLMService(IServiceContainer container)
        {
            // 加载GPT配置
            GPTConfig gptConfig = null;
            if (Resources.Load<GPTConfig>("Config/GPTConfig") != null)
            {
                gptConfig = Resources.Load<GPTConfig>("Config/GPTConfig");
            }
            else
            {
                gptConfig = ScriptableObject.CreateInstance<GPTConfig>();
                gptConfig.apiKey = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
                gptConfig.model = "gpt-4";
                Debug.LogWarning("[ServiceInstaller] 未找到 GPTConfig，使用环境变量或默认配置。");
            }

            // 加载Claude配置
            ClaudeConfig claudeConfig = null;
            if (Resources.Load<ClaudeConfig>("Config/ClaudeConfig") != null)
            {
                claudeConfig = Resources.Load<ClaudeConfig>("Config/ClaudeConfig");
            }
            else
            {
                claudeConfig = ScriptableObject.CreateInstance<ClaudeConfig>();
                claudeConfig.apiKey = System.Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";
                claudeConfig.model = "claude-3-5-sonnet-20241022";
                Debug.LogWarning("[ServiceInstaller] 未找到 ClaudeConfig，使用环境变量或默认配置。");
            }

            // 加载双引擎配置
            DualEngineConfig dualConfig = null;
            if (Resources.Load<DualEngineConfig>("Config/DualEngineConfig") != null)
            {
                dualConfig = Resources.Load<DualEngineConfig>("Config/DualEngineConfig");
            }
            else
            {
                dualConfig = ScriptableObject.CreateInstance<DualEngineConfig>();
                dualConfig.defaultStrategy = AIEngineSelectionStrategy.Intelligent;
                dualConfig.enablePerformanceTracking = true;
            }

            container.RegisterSingleton<GPTConfig>(gptConfig);
            container.RegisterSingleton<ClaudeConfig>(claudeConfig);
            container.RegisterSingleton<DualEngineConfig>(dualConfig);

            // 注册AI引擎选择器 (MonoBehaviour)
            var selectorObject = new GameObject("AIEngineSelector");
            var engineSelector = selectorObject.AddComponent<AIEngineSelector>();
            engineSelector.gptConfig = gptConfig;
            engineSelector.claudeConfig = claudeConfig;
            engineSelector.selectionStrategy = dualConfig.defaultStrategy;
            Object.DontDestroyOnLoad(selectorObject);
            container.RegisterSingleton<AIEngineSelector>(engineSelector);

            // 注册双引擎LLM服务
            var dualEngineService = new DualEngineLLMService(gptConfig, claudeConfig, dualConfig);
            container.RegisterSingleton<IGPTService>(dualEngineService);
            container.RegisterSingleton<DualEngineLLMService>(dualEngineService);

            Logger.LogInfo("双引擎LLM服务安装完成 (GPT-4 + Claude-3.5)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装翻译服务
        /// </summary>
        private static void InstallTranslationService(IServiceContainer container)
        {
            // 加载翻译配置
            TranslationConfig translationConfig = null;
            if (Resources.Load<TranslationConfig>("Config/TranslationConfig") != null)
            {
                translationConfig = Resources.Load<TranslationConfig>("Config/TranslationConfig");
            }
            else
            {
                // 创建默认配置
                translationConfig = ScriptableObject.CreateInstance<TranslationConfig>();
                translationConfig.SubscriptionKey = System.Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_KEY") ?? "";
                translationConfig.Region = System.Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_REGION") ?? "eastasia";
                Debug.LogWarning("[ServiceInstaller] 未找到 TranslationConfig，使用默认配置。请创建配置资源。");
            }

            container.RegisterSingleton<TranslationConfig>(translationConfig);

            // 注册翻译服务
            if (!string.IsNullOrEmpty(translationConfig.SubscriptionKey))
            {
                var translationService = new TranslationService(
                    translationConfig.SubscriptionKey,
                    translationConfig.Region,
                    translationConfig.Endpoint
                );
                translationService.SetTranslationOptions(translationConfig.ToTranslationOptions());
                container.RegisterSingleton<ITranslationService>(translationService);
                Logger.LogInfo("翻译服务安装完成", "ServiceInstaller");
            }
            else
            {
                // 如果没有配置密钥，注册一个模拟服务
                var mockService = new MockTranslationService();
                container.RegisterSingleton<ITranslationService>(mockService);
                Logger.LogWarning("翻译服务使用模拟实现，请配置 Azure Translator 密钥以启用真实翻译功能", "ServiceInstaller");
            }
        }
        
        /// <summary>
        /// 安装功能服务
        /// </summary>
        private static void InstallFeatureServices(IServiceContainer container)
        {
            // 注册导游服务
            container.RegisterSingleton<ITourGuideService, TourGuideService>();

            // 注册场景生成服务
            container.RegisterSingleton<ISceneGenerationService, SceneGenerationService>();

            // 注册多人游戏服务
            InstallMultiplayerService(container);

            // 注册AR服务
            InstallARService(container);

            // 注册移动伴侣服务
            InstallMobileCompanionService(container);

            Logger.LogInfo("功能服务安装完成", "ServiceInstaller");
        }

        /// <summary>
        /// 安装多人游戏服务
        /// </summary>
        private static void InstallMultiplayerService(IServiceContainer container)
        {
            // 加载多人游戏配置
            MultiplayerConfig multiplayerConfig = null;
            if (Resources.Load<MultiplayerConfig>("Config/MultiplayerConfig") != null)
            {
                multiplayerConfig = Resources.Load<MultiplayerConfig>("Config/MultiplayerConfig");
            }
            else
            {
                // 创建默认配置
                multiplayerConfig = ScriptableObject.CreateInstance<MultiplayerConfig>();
                Debug.LogWarning("[ServiceInstaller] 未找到 MultiplayerConfig，使用默认配置。请创建配置资源。");
            }

            container.RegisterSingleton<MultiplayerConfig>(multiplayerConfig);

            // 查找或创建 MultiplayerManager
            var multiplayerManager = Object.FindObjectOfType<MultiplayerManager>();
            if (multiplayerManager == null)
            {
                var go = new GameObject("MultiplayerManager");
                multiplayerManager = go.AddComponent<MultiplayerManager>();
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 MultiplayerManager GameObject");
            }

            container.RegisterSingleton<IMultiplayerService>(multiplayerManager);
            container.RegisterSingleton<MultiplayerManager>(multiplayerManager);

            Logger.LogInfo("多人游戏服务安装完成", "ServiceInstaller");
        }

        /// <summary>
        /// 安装AR服务
        /// </summary>
        private static void InstallARService(IServiceContainer container)
        {
            // 加载AR配置
            ARConfig arConfig = null;
            if (Resources.Load<ARConfig>("Config/ARConfig") != null)
            {
                arConfig = Resources.Load<ARConfig>("Config/ARConfig");
            }
            else
            {
                // 创建默认配置
                arConfig = ScriptableObject.CreateInstance<ARConfig>();
                arConfig.VisionApiKey = System.Environment.GetEnvironmentVariable("AZURE_VISION_KEY") ?? "";
                Debug.LogWarning("[ServiceInstaller] 未找到 ARConfig，使用默认配置。请创建配置资源。");
            }

            container.RegisterSingleton<ARConfig>(arConfig);

            // 查找或创建 ARManager
            var arManager = Object.FindObjectOfType<ARManager>();
            if (arManager == null)
            {
                var go = new GameObject("ARManager");
                arManager = go.AddComponent<ARManager>();
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 ARManager GameObject");
            }

            container.RegisterSingleton<IARService>(arManager);
            container.RegisterSingleton<ARManager>(arManager);

            Logger.LogInfo("AR服务安装完成", "ServiceInstaller");
        }

        /// <summary>
        /// 安装移动伴侣服务
        /// </summary>
        private static void InstallMobileCompanionService(IServiceContainer container)
        {
            // 加载移动伴侣配置
            MobileCompanionConfig companionConfig = null;
            if (Resources.Load<MobileCompanionConfig>("Config/MobileCompanionConfig") != null)
            {
                companionConfig = Resources.Load<MobileCompanionConfig>("Config/MobileCompanionConfig");
            }
            else
            {
                // 创建默认配置
                companionConfig = ScriptableObject.CreateInstance<MobileCompanionConfig>();
                Debug.LogWarning("[ServiceInstaller] 未找到 MobileCompanionConfig，使用默认配置。请创建配置资源。");
            }

            container.RegisterSingleton<MobileCompanionConfig>(companionConfig);

            // 查找或创建 MobileCompanionManager
            var companionManager = Object.FindObjectOfType<MobileCompanionManager>();
            if (companionManager == null)
            {
                var go = new GameObject("MobileCompanionManager");
                companionManager = go.AddComponent<MobileCompanionManager>();
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 MobileCompanionManager GameObject");
            }

            container.RegisterSingleton<IMobileCompanionService>(companionManager);
            container.RegisterSingleton<MobileCompanionManager>(companionManager);

            Logger.LogInfo("移动伴侣服务安装完成", "ServiceInstaller");
        }

        /// <summary>
        /// 安装VR服务
        /// </summary>
        private static void InstallVRServices(IServiceContainer container)
        {
            // 注册VR管理器
            var vrManager = Object.FindObjectOfType<VRManager>();
            if (vrManager != null)
            {
                container.RegisterSingleton<VRManager>(vrManager);
            }

            // 注册VR性能优化器
            container.RegisterSingleton<VRPerformanceOptimizer>(Object.FindObjectOfType<VRPerformanceOptimizer>() ??
                new GameObject("VRPerformanceOptimizer").AddComponent<VRPerformanceOptimizer>());

            // 安装 Vision Pro 适配器 (Phase 2: Apple Vision Pro 适配)
            InstallVisionProAdapter(container);

            // 安装 WebXR 服务 (Phase 2: WebXR 跨平台)
            InstallWebXRService(container);

            // 安装注视点渲染服务 (Phase 2: 注视点渲染)
            InstallFoveatedRenderingService(container);

            Logger.LogInfo("VR服务安装完成", "ServiceInstaller");
        }

        /// <summary>
        /// 安装 Vision Pro 适配器 (Phase 2: Apple Vision Pro 适配)
        /// 空间计算API集成、手势交互、混合现实渲染
        /// </summary>
        private static void InstallVisionProAdapter(IServiceContainer container)
        {
            // 查找或创建 VisionProAdapter
            var visionProAdapter = Object.FindObjectOfType<VisionProAdapter>();
            if (visionProAdapter == null)
            {
                var go = new GameObject("VisionProAdapter");
                visionProAdapter = go.AddComponent<VisionProAdapter>();
                visionProAdapter.enableHandTracking = true;
                visionProAdapter.enableEyeTracking = true;
                visionProAdapter.enableMixedReality = true;
                visionProAdapter.enableSpatialAudio = true;
                visionProAdapter.enableFoveatedRendering = true;
                visionProAdapter.gestureThreshold = 0.8f;
                visionProAdapter.pinchThreshold = 0.02f;
                visionProAdapter.passthroughOpacity = 0.5f;
                visionProAdapter.foveationLevel = 2;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 VisionProAdapter GameObject");
            }

            container.RegisterSingleton<IVRPlatformAdapter>(visionProAdapter);
            container.RegisterSingleton<VisionProAdapter>(visionProAdapter);
            Logger.LogInfo("Vision Pro 适配器安装完成 (空间计算API、手势交互、混合现实)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装 WebXR 服务 (Phase 2: WebXR 跨平台)
        /// 浏览器 VR 体验、WebAssembly 优化、云渲染
        /// </summary>
        private static void InstallWebXRService(IServiceContainer container)
        {
            // 查找或创建 WebXRManager
            var webXRManager = Object.FindObjectOfType<WebXRManager>();
            if (webXRManager == null)
            {
                var go = new GameObject("WebXRManager");
                webXRManager = go.AddComponent<WebXRManager>();
                webXRManager.autoInitialize = true;
                webXRManager.enableWebAssembly = true;
                webXRManager.enableCompression = true;
                webXRManager.enableCaching = true;
                webXRManager.targetFrameRate = 72;
                webXRManager.renderScale = 1.0f;
                webXRManager.enableHandTracking = true;
                webXRManager.enableGamepadInput = true;
                webXRManager.signallingServerUrl = "wss://tripmeta.io/signalling";
                webXRManager.enableCloudRendering = false;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 WebXRManager GameObject");
            }

            container.RegisterSingleton<WebXRManager>(webXRManager);
            Logger.LogInfo("WebXR 服务安装完成 (浏览器VR、WebAssembly、云渲染)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装注视点渲染服务 (Phase 2: 注视点渲染)
        /// 基于眼动追踪的动态注视点渲染，提升性能30%
        /// </summary>
        private static void InstallFoveatedRenderingService(IServiceContainer container)
        {
            // 查找或创建 FoveatedRenderingManager
            var foveatedManager = Object.FindObjectOfType<FoveatedRenderingManager>();
            if (foveatedManager == null)
            {
                var go = new GameObject("FoveatedRenderingManager");
                foveatedManager = go.AddComponent<FoveatedRenderingManager>();
                foveatedManager.enableFoveatedRendering = true;
                foveatedManager.foveationMode = FoveationMode.Dynamic;
                foveatedManager.foveationLevel = 2;
                foveatedManager.enableDynamicAdjustment = true;
                foveatedManager.gazeCheckInterval = 0.016f;
                foveatedManager.innerRadius = 0.15f;
                foveatedManager.middleRadius = 0.3f;
                foveatedManager.outerRadius = 0.5f;
                foveatedManager.innerRegionScale = 1.0f;
                foveatedManager.middleRegionScale = 0.75f;
                foveatedManager.outerRegionScale = 0.5f;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 FoveatedRenderingManager GameObject");
            }

            container.RegisterSingleton<FoveatedRenderingManager>(foveatedManager);

            // 查找或创建 EyeFatigueDetector
            var fatigueDetector = Object.FindObjectOfType<EyeFatigueDetector>();
            if (fatigueDetector == null)
            {
                var go = new GameObject("EyeFatigueDetector");
                fatigueDetector = go.AddComponent<EyeFatigueDetector>();
                fatigueDetector.checkInterval = 5f;
                fatigueDetector.historyWindowSize = 60;
                fatigueDetector.blinkRateThreshold = 10f;
                fatigueDetector.mildFatigueThreshold = 0.3f;
                fatigueDetector.moderateFatigueThreshold = 0.6f;
                fatigueDetector.severeFatigueThreshold = 0.8f;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 EyeFatigueDetector GameObject");
            }

            container.RegisterSingleton<EyeFatigueDetector>(fatigueDetector);
            Logger.LogInfo("注视点渲染服务安装完成 (性能提升30%，视觉疲劳监测)", "ServiceInstaller");
        }
    }

    // 临时实现类，用于服务注册
    public class NetworkService : INetworkService
    {
        public bool IsConnected => true;
        public event System.Action<bool> OnConnectionStatusChanged;
        
        public async System.Threading.Tasks.Task<T> GetAsync<T>(string endpoint)
        {
            await System.Threading.Tasks.Task.Delay(100);
            return default(T);
        }
        
        public async System.Threading.Tasks.Task<T> PostAsync<T>(string endpoint, object data)
        {
            await System.Threading.Tasks.Task.Delay(100);
            return default(T);
        }
        
        public async System.Threading.Tasks.Task<T> PutAsync<T>(string endpoint, object data)
        {
            await System.Threading.Tasks.Task.Delay(100);
            return default(T);
        }
        
        public async System.Threading.Tasks.Task DeleteAsync(string endpoint)
        {
            await System.Threading.Tasks.Task.Delay(100);
        }
    }
    
    public class CacheService : ICacheService
    {
        private readonly System.Collections.Generic.Dictionary<string, object> cache = 
            new System.Collections.Generic.Dictionary<string, object>();
        
        public async System.Threading.Tasks.Task<T> GetAsync<T>(string key)
        {
            await System.Threading.Tasks.Task.Yield();
            return cache.TryGetValue(key, out var value) ? (T)value : default(T);
        }
        
        public async System.Threading.Tasks.Task SetAsync<T>(string key, T value, System.TimeSpan? expiration = null)
        {
            await System.Threading.Tasks.Task.Yield();
            cache[key] = value;
        }
        
        public async System.Threading.Tasks.Task RemoveAsync(string key)
        {
            await System.Threading.Tasks.Task.Yield();
            cache.Remove(key);
        }
        
        public async System.Threading.Tasks.Task ClearAsync()
        {
            await System.Threading.Tasks.Task.Yield();
            cache.Clear();
        }
        
        public async System.Threading.Tasks.Task<bool> ExistsAsync(string key)
        {
            await System.Threading.Tasks.Task.Yield();
            return cache.ContainsKey(key);
        }
    }
    
    public class SceneGenerationService : ISceneGenerationService
    {
        public async System.Threading.Tasks.Task<GameObject> GenerateSceneAsync(SceneGenerationRequest request)
        {
            await System.Threading.Tasks.Task.Delay(1000);
            return new GameObject($"GeneratedScene_{request.description}");
        }
        
        public async System.Threading.Tasks.Task<Texture2D> GenerateTextureAsync(string description)
        {
            await System.Threading.Tasks.Task.Delay(500);
            return new Texture2D(512, 512);
        }
        
        public async System.Threading.Tasks.Task<Mesh> GenerateMeshAsync(string description)
        {
            await System.Threading.Tasks.Task.Delay(800);
            return new Mesh();
        }
        
        public async System.Threading.Tasks.Task OptimizeSceneAsync(GameObject scene)
        {
            await System.Threading.Tasks.Task.Delay(300);
        }
    }
}