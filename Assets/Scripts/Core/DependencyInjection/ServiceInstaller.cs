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
using TripMeta.VR.Haptics;
using TripMeta.Performance;
using TripMeta.UGC;
using TripMeta.Web3;
using TripMeta.CloudRendering;
using TripMeta.Localization;
using TripMeta.Commerce;
using TripMeta.Analytics;

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

            // 注册性能监控仪表板服务 (Phase 3: 性能监控仪表板)
            InstallPerformanceDashboardService(container);

            // 注册UGC创作工具服务 (Phase 3: UGC创作工具)
            InstallUGCService(container);

            // 注册Web3服务 (Phase 3: Web3集成)
            InstallWeb3Service(container);

            // 注册云渲染服务 (Phase 3: 云渲染流媒体)
            InstallCloudRenderingService(container);

            // 注册多语言本地化服务 (Phase 4: 全球化扩展)
            InstallLocalizationService(container);

            // 注册商业化服务 (Phase 5: 商业化与前沿技术)
            InstallCommerceService(container);

            // 注册分析服务 (Phase 5: 分析与数据平台)
            InstallAnalyticsService(container);

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

            // 安装触觉反馈服务 (Phase 2: 触觉反馈集成)
            InstallHapticFeedbackService(container);

            Logger.LogInfo("VR服务安装完成", "ServiceInstaller");
        }

        /// <summary>
        /// 安装触觉反馈服务 (Phase 2: 触觉反馈集成)
        /// 支持全身触觉反馈设备
        /// </summary>
        private static void InstallHapticFeedbackService(IServiceContainer container)
        {
            // 查找或创建 HapticFeedbackManager
            var hapticManager = Object.FindObjectOfType<HapticFeedbackManager>();
            if (hapticManager == null)
            {
                var go = new GameObject("HapticFeedbackManager");
                hapticManager = go.AddComponent<HapticFeedbackManager>();
                hapticManager.enableHaptics = true;
                hapticManager.globalIntensity = 1.0f;
                hapticManager.defaultPriority = HapticPriority.Normal;
                hapticManager.enableHead = true;
                hapticManager.enableTorso = true;
                hapticManager.enableArms = true;
                hapticManager.enableHands = true;
                hapticManager.enableLegs = true;
                hapticManager.enableFeet = true;
                hapticManager.autoConnect = true;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 HapticFeedbackManager GameObject");
            }

            container.RegisterSingleton<HapticFeedbackManager>(hapticManager);
            Logger.LogInfo("触觉反馈服务安装完成 (支持全身触觉设备)", "ServiceInstaller");
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

        /// <summary>
        /// 安装性能监控仪表板服务 (Phase 3: 性能监控仪表板)
        /// 实时 FPS、延迟、内存监控和分析
        /// </summary>
        private static void InstallPerformanceDashboardService(IServiceContainer container)
        {
            // 查找或创建 PerformanceMonitor
            var performanceMonitor = Object.FindObjectOfType<PerformanceMonitor>();
            if (performanceMonitor == null)
            {
                var go = new GameObject("PerformanceMonitor");
                performanceMonitor = go.AddComponent<PerformanceMonitor>();
                performanceMonitor.enableMonitoring = true;
                performanceMonitor.updateInterval = 1.0f;
                performanceMonitor.maxHistorySize = 300;
                performanceMonitor.trackFPS = true;
                performanceMonitor.targetFPS = 72f;
                performanceMonitor.warningFPS = 60f;
                performanceMonitor.criticalFPS = 45f;
                performanceMonitor.trackLatency = true;
                performanceMonitor.warningLatency = 20f;
                performanceMonitor.criticalLatency = 50f;
                performanceMonitor.trackMemory = true;
                performanceMonitor.warningMemoryMB = 2048;
                performanceMonitor.criticalMemoryMB = 3072;
                performanceMonitor.trackRendering = true;
                performanceMonitor.warningDrawCalls = 2000;
                performanceMonitor.criticalDrawCalls = 3000;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 PerformanceMonitor GameObject");
            }

            container.RegisterSingleton<PerformanceMonitor>(performanceMonitor);

            // 查找或创建 PerformanceDashboard
            var performanceDashboard = Object.FindObjectOfType<PerformanceDashboard>();
            if (performanceDashboard == null)
            {
                // 创建仪表板 Canvas
                var canvasGo = new GameObject("PerformanceDashboardCanvas");
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();

                performanceDashboard = canvasGo.AddComponent<PerformanceDashboard>();
                performanceDashboard.showOnStart = false;
                performanceDashboard.toggleKey = KeyCode.F12;
                performanceDashboard.dashboardCanvas = canvas;

                // 添加图表组件引用（将在UI设置时连接）
                Object.DontDestroyOnLoad(canvasGo);
                Debug.Log("[ServiceInstaller] 创建 PerformanceDashboard Canvas");
            }

            container.RegisterSingleton<PerformanceDashboard>(performanceDashboard);
            Logger.LogInfo("性能监控仪表板服务安装完成 (实时FPS/延迟/内存监控)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装UGC创作工具服务 (Phase 3: UGC创作工具)
        /// 可视化场景编辑器，让用户创建自定义景点
        /// </summary>
        private static void InstallUGCService(IServiceContainer container)
        {
            // 查找或创建 SceneEditorManager
            var sceneEditor = Object.FindObjectOfType<SceneEditorManager>();
            if (sceneEditor == null)
            {
                var go = new GameObject("SceneEditorManager");
                sceneEditor = go.AddComponent<SceneEditorManager>();
                sceneEditor.enableAutoSave = true;
                sceneEditor.autoSaveInterval = 30f;
                sceneEditor.maxUndoSteps = 50;
                sceneEditor.scenesSavePath = "UserScenes/";
                sceneEditor.snapMode = SnapMode.Grid;
                sceneEditor.snapGridSize = 1f;
                sceneEditor.snapAngle = 15f;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 SceneEditorManager GameObject");
            }

            container.RegisterSingleton<SceneEditorManager>(sceneEditor);
            Logger.LogInfo("UGC创作工具服务安装完成 (可视化场景编辑器)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装Web3服务 (Phase 3: Web3集成)
        /// NFT数字资产、虚拟经济系统
        /// </summary>
        private static void InstallWeb3Service(IServiceContainer container)
        {
            // 查找或创建 Web3Manager
            var web3Manager = Object.FindObjectOfType<Web3Manager>();
            if (web3Manager == null)
            {
                var go = new GameObject("Web3Manager");
                web3Manager = go.AddComponent<Web3Manager>();
                web3Manager.defaultNetwork = BlockchainNetwork.Ethereum;
                web3Manager.enableNFT = true;
                web3Manager.enableToken = true;
                web3Manager.enableMarketplace = true;
                web3Manager.enableStaking = true;
                web3Manager.ipfsGateway = "https://ipfs.io/ipfs/";
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 Web3Manager GameObject");
            }

            container.RegisterSingleton<Web3Manager>(web3Manager);
            Logger.LogInfo("Web3服务安装完成 (NFT、代币、市场、质押)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装云渲染服务 (Phase 3: 云渲染流媒体)
        /// 让低端设备也能体验高质量 VR
        /// </summary>
        private static void InstallCloudRenderingService(IServiceContainer container)
        {
            // 查找或创建 CloudRenderingManager
            var cloudManager = Object.FindObjectOfType<CloudRenderingManager>();
            if (cloudManager == null)
            {
                var go = new GameObject("CloudRenderingManager");
                cloudManager = go.AddComponent<CloudRenderingManager>();
                cloudManager.enableCloudRendering = true;
                cloudManager.targetResolutionX = 1920;
                cloudManager.targetResolutionY = 1080;
                cloudManager.targetFrameRate = 60;
                cloudManager.bitrateKbps = 20000;
                cloudManager.enableAdaptiveBitrate = true;
                cloudManager.enableInputPrediction = true;
                cloudManager.signallingServerUrl = "wss://tripmeta-cloud-render.com/signalling";
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 CloudRenderingManager GameObject");
            }

            container.RegisterSingleton<CloudRenderingManager>(cloudManager);
            Logger.LogInfo("云渲染服务安装完成 (WebRTC流媒体，支持低端设备)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装多语言本地化服务 (Phase 4: 全球化扩展)
        /// 支持50+语言，本地化AI导游，文化适配
        /// </summary>
        private static void InstallLocalizationService(IServiceContainer container)
        {
            // 查找或创建 MultilingualGuideManager
            var localizationManager = Object.FindObjectOfType<MultilingualGuideManager>();
            if (localizationManager == null)
            {
                var go = new GameObject("MultilingualGuideManager");
                localizationManager = go.AddComponent<MultilingualGuideManager>();
                localizationManager.autoDetectLanguage = true;
                localizationManager.defaultLanguage = LanguageCode.en_US;
                localizationManager.enableCulturalAdaptation = true;
                localizationManager.enableFormalityAdjustment = true;
                localizationManager.useLocalizedModels = true;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 MultilingualGuideManager GameObject");
            }

            container.RegisterSingleton<MultilingualGuideManager>(localizationManager);

            // 查找或创建 RegionalContentManager
            var regionalManager = Object.FindObjectOfType<RegionalContentManager>();
            if (regionalManager == null)
            {
                var go = new GameObject("RegionalContentManager");
                regionalManager = go.AddComponent<RegionalContentManager>();
                regionalManager.autoDetectRegion = true;
                regionalManager.defaultRegion = RegionType.AsiaPacific;
                regionalManager.enableContentFiltering = true;
                regionalManager.enableCulturalCompliance = true;
                regionalManager.enableRegionalPricing = true;
                regionalManager.enableLocalEvents = true;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 RegionalContentManager GameObject");
            }

            container.RegisterSingleton<RegionalContentManager>(regionalManager);
            Logger.LogInfo("多语言本地化服务安装完成 (50+语言支持，6大区域，文化适配)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装商业化服务 (Phase 5: 商业化与前沿技术)
        /// 订阅管理、支付处理、优惠券、退款
        /// </summary>
        private static void InstallCommerceService(IServiceContainer container)
        {
            // 查找或创建 SubscriptionManager
            var subscriptionManager = Object.FindObjectOfType<SubscriptionManager>();
            if (subscriptionManager == null)
            {
                var go = new GameObject("SubscriptionManager");
                subscriptionManager = go.AddComponent<SubscriptionManager>();
                subscriptionManager.defaultTierId = "basic";
                subscriptionManager.enableFreeTrial = true;
                subscriptionManager.freeTrialDays = 7;
                subscriptionManager.enableCoupons = true;
                subscriptionManager.enableReferralProgram = true;
                subscriptionManager.autoRenewalDefault = true;
                subscriptionManager.supportedCurrencies = new System.Collections.Generic.List<string> { "USD", "EUR", "CNY", "JPY" };
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 SubscriptionManager GameObject");
            }

            container.RegisterSingleton<SubscriptionManager>(subscriptionManager);
            Logger.LogInfo("商业化服务安装完成 (订阅管理、支付网关、优惠券)", "ServiceInstaller");
        }

        /// <summary>
        /// 安装分析服务 (Phase 5: 分析与数据平台)
        /// 用户行为分析、A/B测试、商业智能仪表板
        /// </summary>
        private static void InstallAnalyticsService(IServiceContainer container)
        {
            // 查找或创建 AnalyticsManager
            var analyticsManager = Object.FindObjectOfType<AnalyticsManager>();
            if (analyticsManager == null)
            {
                var go = new GameObject("AnalyticsManager");
                analyticsManager = go.AddComponent<AnalyticsManager>();
                analyticsManager.enableRealTimeAnalytics = true;
                analyticsManager.trackUserSessions = true;
                analyticsManager.trackVRInteractions = true;
                analyticsManager.trackPerformanceMetrics = true;
                analyticsManager.enableABTesting = true;
                analyticsManager.trackConversionFunnel = true;
                analyticsManager.eventBatchInterval = 30f;
                Object.DontDestroyOnLoad(go);
                Debug.Log("[ServiceInstaller] 创建 AnalyticsManager GameObject");
            }

            container.RegisterSingleton<AnalyticsManager>(analyticsManager);
            Logger.LogInfo("分析服务安装完成 (用户行为分析、A/B测试、BI仪表板)", "ServiceInstaller");
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