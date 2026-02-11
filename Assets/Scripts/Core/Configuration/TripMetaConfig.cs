using UnityEngine;
using System;

namespace TripMeta.Core.Configuration
{
    /// <summary>
    /// Trip-Meta 主配置文件
    /// </summary>
    [CreateAssetMenu(fileName = "TripMetaConfig", menuName = "TripMeta/Configuration/Main Config")]
    public class TripMetaConfig : ScriptableObject
    {
        [Header("应用信息")]
        public ApplicationInfo applicationInfo;
        
        [Header("VR设置")]
        public VRConfiguration vrConfig;
        
        [Header("AI设置")]
        public AIConfiguration aiConfig;
        
        [Header("性能设置")]
        public PerformanceConfiguration performanceConfig;
        
        [Header("网络设置")]
        public NetworkConfiguration networkConfig;
        
        [Header("调试设置")]
        public DebugConfiguration debugConfig;
        
        /// <summary>
        /// 验证配置
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;
            
            if (vrConfig == null)
            {
                Debug.LogError("[TripMetaConfig] VR配置缺失");
                isValid = false;
            }
            
            if (aiConfig == null)
            {
                Debug.LogError("[TripMetaConfig] AI配置缺失");
                isValid = false;
            }
            
            if (performanceConfig == null)
            {
                Debug.LogError("[TripMetaConfig] 性能配置缺失");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 重置为默认值
        /// </summary>
        [ContextMenu("Reset to Default")]
        public void ResetToDefault()
        {
            applicationInfo = new ApplicationInfo();
            vrConfig = new VRConfiguration();
            aiConfig = new AIConfiguration();
            performanceConfig = new PerformanceConfiguration();
            networkConfig = new NetworkConfiguration();
            debugConfig = new DebugConfiguration();
        }
    }
    
    /// <summary>
    /// 应用信息配置
    /// </summary>
    [Serializable]
    public class ApplicationInfo
    {
        [Header("基本信息")]
        public string appName = "Trip-Meta VR";
        public string version = "1.0.0";
        public string buildNumber = "1";
        
        [Header("开发信息")]
        public string companyName = "";
        public string supportEmail = "";
        public string websiteUrl = "https://github.com/trip-meta/TripMeta";
    }
    
    /// <summary>
    /// VR配置
    /// </summary>
    [Serializable]
    public class VRConfiguration
    {
        [Header("基础设置")]
        public bool enableVROnStart = true;
        public bool enableHandTracking = true;
        public bool enableEyeTracking = false;
        
        [Header("性能设置")]
        [Range(60, 120)]
        public int targetFrameRate = 72;
        public bool enableFoveatedRendering = true;
        public bool enableDynamicResolution = true;
        
        [Header("舒适度设置")]
        public bool enableComfortVignette = true;
        public bool enableSnapTurning = false;
        [Range(15f, 45f)]
        public float snapTurnAngle = 30f;
        
        [Header("交互设置")]
        public bool enableTeleportation = true;
        public bool enableDirectMovement = false;
        [Range(1f, 10f)]
        public float teleportMaxDistance = 10f;
        
        [Header("音频设置")]
        public bool enableSpatialAudio = true;
        [Range(0f, 1f)]
        public float masterVolume = 0.8f;
        [Range(0f, 1f)]
        public float effectsVolume = 0.7f;
        [Range(0f, 1f)]
        public float musicVolume = 0.5f;
    }
    
    /// <summary>
    /// AI配置
    /// </summary>
    [Serializable]
    public class AIConfiguration
    {
        [Header("服务配置")]
        public bool enableAIServices = true;
        public bool useEdgeAI = true;
        public int maxConcurrentRequests = 5;
        [Range(5f, 60f)]
        public float requestTimeout = 30f;
        
        [Header("LLM设置")]
        public LLMConfig llmConfig;
        
        [Header("语音设置")]
        public SpeechConfig speechConfig;
        
        [Header("视觉AI设置")]
        public VisionConfig visionConfig;
        
        [Header("推荐系统")]
        public RecommendationConfig recommendationConfig;
    }
    
    /// <summary>
    /// LLM配置
    /// </summary>
    [Serializable]
    public class LLMConfig
    {
        public string apiKey = "";
        public string modelName = "gpt-4-turbo";
        [Range(0f, 2f)]
        public float temperature = 0.7f;
        [Range(1, 4000)]
        public int maxTokens = 1000;
        public bool enableStreaming = false;
    }
    
    /// <summary>
    /// 语音配置
    /// </summary>
    [Serializable]
    public class SpeechConfig
    {
        public string speechApiKey = "";
        public string speechRegion = "eastus";
        public string defaultVoice = "zh-CN-XiaoxiaoNeural";
        [Range(0.5f, 2f)]
        public float speechSpeed = 1.0f;
        [Range(0f, 1f)]
        public float speechVolume = 0.8f;
        public bool enableRealTimeRecognition = true;
    }
    
    /// <summary>
    /// 视觉AI配置
    /// </summary>
    [Serializable]
    public class VisionConfig
    {
        public string visionApiKey = "";
        public bool enableObjectDetection = true;
        public bool enableTextRecognition = true;
        public bool enableFaceRecognition = false;
        [Range(0.1f, 1f)]
        public float confidenceThreshold = 0.7f;
    }
    
    /// <summary>
    /// 推荐配置
    /// </summary>
    [Serializable]
    public class RecommendationConfig
    {
        public bool enablePersonalization = true;
        public bool enableCollaborativeFiltering = true;
        [Range(1, 20)]
        public int maxRecommendations = 10;
        [Range(0.1f, 1f)]
        public float diversityFactor = 0.3f;
    }
    
    /// <summary>
    /// 性能配置
    /// </summary>
    [Serializable]
    public class PerformanceConfiguration
    {
        [Header("渲染设置")]
        [Range(0.5f, 2f)]
        public float renderScale = 1.0f;
        public bool enableOcclusionCulling = true;
        public bool enableFrustumCulling = true;
        public bool enableLODSystem = true;
        
        [Header("内存管理")]
        [Range(512, 4096)]
        public int maxMemoryUsageMB = 2048;
        public bool enableGarbageCollection = true;
        public bool enableObjectPooling = true;
        
        [Header("网络优化")]
        [Range(1, 10)]
        public int maxConcurrentDownloads = 3;
        [Range(1024, 10240)]
        public int networkBufferSize = 4096;
        public bool enableCompression = true;
        
        [Header("AI性能")]
        [Range(1, 8)]
        public int aiThreadCount = 2;
        public bool enableAIBatching = true;
        [Range(100, 2000)]
        public int aiCacheSize = 500;
    }
    
    /// <summary>
    /// 网络配置
    /// </summary>
    [Serializable]
    public class NetworkConfiguration
    {
        [Header("服务器设置")]
        public string baseApiUrl = "";
        public string cdnUrl = "";
        [Range(5f, 60f)]
        public float connectionTimeout = 30f;
        [Range(1, 5)]
        public int maxRetryAttempts = 3;
        
        [Header("缓存设置")]
        public bool enableCaching = true;
        [Range(1, 24)]
        public int cacheExpirationHours = 6;
        [Range(100, 2000)]
        public int maxCacheSize = 1000;
        
        [Header("安全设置")]
        public bool enableSSL = true;
        public bool validateCertificates = true;
        public string apiVersion = "v1";
    }
    
    /// <summary>
    /// 调试配置
    /// </summary>
    [Serializable]
    public class DebugConfiguration
    {
        [Header("日志设置")]
        public bool enableDebugLogs = true;
        public bool enableVerboseLogs = false;
        public bool logToFile = true;
        public LogLevel minLogLevel = LogLevel.Info;
        
        [Header("性能监控")]
        public bool enablePerformanceMonitoring = true;
        public bool showFPSCounter = true;
        public bool showMemoryUsage = true;
        public bool enableProfiler = false;
        
        [Header("AI调试")]
        public bool enableAIDebugLogs = false;
        public bool showAIResponses = false;
        public bool logAIRequests = false;
        
        [Header("VR调试")]
        public bool enableVRDebugInfo = false;
        public bool showControllerDebug = false;
        public bool logVREvents = false;
    }
    
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }
}