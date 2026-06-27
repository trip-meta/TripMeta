using UnityEngine;

namespace TripMeta.Core.Configuration
{
    /// <summary>
    /// 应用程序设置
    /// </summary>
    [CreateAssetMenu(fileName = "AppSettings", menuName = "TripMeta/App Settings")]
    public class AppSettings : ScriptableObject
    {
        [Header("AI Services")]
        public AIServiceSettings aiSettings;
        
        [Header("VR Settings")]
        public VRSettings vrSettings;
        
        [Header("Performance")]
        public PerformanceSettings performanceSettings;
        
        [Header("Network")]
        public NetworkSettings networkSettings;
    }

    [System.Serializable]
    public class AIServiceSettings
    {
        [Header("Ark LLM")]
        public string arkApiKey;
        public string arkBaseUrl = TripMeta.AI.GPTConfig.DefaultArkBaseUrl;
        public string arkChatModel = TripMeta.AI.GPTConfig.DefaultArkModel;

        [Header("Legacy / Compatible")]
        public string openAIApiKey;
        public int maxTokens = 2048;
        public float temperature = 0.7f;

        [Header("Azure Speech")]
        public string azureSpeechKey;
        public string azureSpeechRegion;

        [Header("Azure Vision")]
        public string azureVisionKey;
        public string azureVisionEndpoint;
        public string googleVisionApiKey;

        [Header("Limits")]
        public int maxRequestsPerMinute = 60;
        public float requestTimeout = 30f;
    }

    [System.Serializable]
    public class VRSettings
    {
        public float targetFrameRate = 72f;
        public bool enableFoveatedRendering = true;
        public bool enableDynamicResolution = true;
        public float renderScale = 1.0f;
        public int msaaLevel = 4;
    }

    [System.Serializable]
    public class PerformanceSettings
    {
        public bool enableProfiling = true;
        public int maxDrawCalls = 1000;
        public int maxTriangles = 100000;
        public float lodBias = 1.0f;
        public bool enableOcclusion = true;
    }

    [System.Serializable]
    public class NetworkSettings
    {
        public string baseApiUrl;
        public int connectionTimeout = 10;
        public int maxRetries = 3;
        public bool enableCaching = true;
        public int cacheExpirationMinutes = 30;
    }
}
