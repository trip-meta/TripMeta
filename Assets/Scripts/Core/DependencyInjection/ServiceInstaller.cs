using UnityEngine;
using TripMeta.Core.Configuration;
using TripMeta.Core.ErrorHandling;
using TripMeta.Core.Performance;
using TripMeta.Infrastructure.Network;
using TripMeta.Infrastructure.Cache;
using TripMeta.Infrastructure.Resources;
using TripMeta.Features.TourGuide;
using TripMeta.Features.SceneGeneration;
using TripMeta.AI;

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
            
            Logger.LogInfo("AI服务安装完成", "ServiceInstaller");
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
            
            Logger.LogInfo("功能服务安装完成", "ServiceInstaller");
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
            
            Logger.LogInfo("VR服务安装完成", "ServiceInstaller");
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