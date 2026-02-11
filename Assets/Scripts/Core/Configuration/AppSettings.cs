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
        public string openAIApiKey;
        public string azureSpeechKey;
        public string azureSpeechRegion;
        public string googleVisionApiKey;
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