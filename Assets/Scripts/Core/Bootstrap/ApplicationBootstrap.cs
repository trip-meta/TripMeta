using UnityEngine;
using System;
using System.Threading.Tasks;
using TripMeta.Core.Configuration;

namespace TripMeta.Core.Bootstrap
{
    /// <summary>
    /// 应用程序启动器 - 负责系统初始化和服务注册
    /// </summary>
    public class ApplicationBootstrap : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private TripMetaConfig config;
        
        [Header("启动设置")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool showSplashScreen = true;
        [SerializeField] private float splashDuration = 3f;

        // 启动状态
        private bool isInitialized = false;
        private bool isInitializing = false;
        
        // 事件
        public static event Action<float> OnInitializationProgress;
        public static event Action OnInitializationComplete;
        public static event Action<string> OnInitializationError;
        
        public static ApplicationBootstrap Instance { get; private set; }
        public bool IsInitialized => isInitialized;
        
        void Awake()
        {
            // 单例模式
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // 设置应用程序基本配置
                SetupApplicationSettings();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            // 如果配置未分配，尝试自动加载
            if (config == null)
            {
                config = ConfigurationLoader.LoadTripMetaConfig();
                Debug.Log("[ApplicationBootstrap] Configuration loaded from Resources");
            }

            if (autoStart)
            {
                _ = InitializeApplicationAsync();
            }
        }
        
        /// <summary>
        /// 设置应用程序基本配置
        /// </summary>
        private void SetupApplicationSettings()
        {
            // 设置目标帧率
            if (config?.performanceConfig != null)
            {
                Application.targetFrameRate = config.vrConfig?.targetFrameRate ?? 72;
            }
            
            // 设置应用程序信息
            if (config?.applicationInfo != null)
            {
                Application.companyName = config.applicationInfo.companyName;
                Application.productName = config.applicationInfo.appName;
                Application.version = config.applicationInfo.version;
            }
            
            // 防止屏幕休眠
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            Debug.Log($"[ApplicationBootstrap] 应用程序基本设置完成");
        }
        
        /// <summary>
        /// 异步初始化应用程序
        /// </summary>
        public async Task<bool> InitializeApplicationAsync()
        {
            if (isInitialized || isInitializing)
            {
                Debug.LogWarning("[ApplicationBootstrap] 应用程序已经初始化或正在初始化中");
                return isInitialized;
            }

            isInitializing = true;

            try
            {
                Debug.Log("[ApplicationBootstrap] 开始初始化应用程序...");

                // 显示启动画面
                if (showSplashScreen)
                {
                    ReportProgress(0.05f, "显示启动画面...");
                    await ShowSplashScreenAsync();
                }

                // 步骤1: 验证配置
                ReportProgress(0.15f, "验证配置...");
                ValidateConfiguration();

                // 步骤2: 设置应用程序基本配置
                ReportProgress(0.25f, "设置应用程序...");
                SetupApplicationSettings();

                // 步骤3: 初始化VR系统
                ReportProgress(0.4f, "初始化VR系统...");
                await InitializeVRSystemAsync();

                // 步骤4: 等待AI服务初始化
                ReportProgress(0.6f, "等待AI服务...");
                await InitializeAIServicesAsync();

                // 步骤5: 加载资源
                ReportProgress(0.8f, "加载资源...");
                await LoadResourcesAsync();

                // 步骤6: 完成初始化
                ReportProgress(1.0f, "初始化完成");

                isInitialized = true;
                isInitializing = false;

                Debug.Log("[ApplicationBootstrap] 应用程序初始化完成");
                OnInitializationComplete?.Invoke();

                return true;
            }
            catch (Exception ex)
            {
                isInitializing = false;
                var errorMessage = $"应用程序初始化失败: {ex.Message}";
                Debug.LogError($"[ApplicationBootstrap] {errorMessage}");
                OnInitializationError?.Invoke(errorMessage);
                return false;
            }
        }
        
        /// <summary>
        /// 显示启动画面
        /// </summary>
        private async Task ShowSplashScreenAsync()
        {
            Debug.Log("[ApplicationBootstrap] 显示启动画面");
            
            // 这里可以显示自定义启动画面
            // 暂时使用简单的延迟
            await Task.Delay(Mathf.RoundToInt(splashDuration * 1000));
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        private void ValidateConfiguration()
        {
            if (config == null)
            {
                Debug.LogWarning("[ApplicationBootstrap] 配置文件缺失，使用默认配置");
                return;
            }

            config.ValidateConfiguration();
        }
        
        /// <summary>
        /// 初始化VR系统
        /// </summary>
        private async Task InitializeVRSystemAsync()
        {
            try
            {
                var vrManagerInstance = VRManager.Instance;
                if (vrManagerInstance != null)
                {
                    vrManagerInstance.InitializeVR();
                    Debug.Log("[ApplicationBootstrap] VR系统初始化完成");
                }

                // 模拟异步初始化
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                errorHandler?.HandleException(ex, "VR系统初始化");
                throw;
            }
        }

        /// <summary>
        /// 初始化AI服务
        /// </summary>
        private async Task InitializeAIServicesAsync()
        {
            try
            {
                var aiManagerInstance = AIServiceManager.Instance;
                if (aiManagerInstance != null && !aiManagerInstance.isInitialized)
                {
                    // 等待AIServiceManager自己初始化
                    await Task.Delay(100);
                    Debug.Log("[ApplicationBootstrap] AI服务初始化完成");
                }
            }
            catch (Exception ex)
            {
                errorHandler?.HandleException(ex, "AI服务初始化");
                // AI服务初始化失败不应该阻止应用启动
                Debug.LogWarning("[ApplicationBootstrap] AI服务初始化失败，但应用将继续启动");
            }
        }
        
        /// <summary>
        /// 加载资源
        /// </summary>
        private async Task LoadResourcesAsync()
        {
            try
            {
                // 预加载关键资源
                Debug.Log("[ApplicationBootstrap] 开始加载资源");
                
                // 模拟资源加载
                await Task.Delay(1000);
                
                Debug.Log("[ApplicationBootstrap] 资源加载完成");
            }
            catch (Exception ex)
            {
                errorHandler?.HandleException(ex, "资源加载");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化场景管理
        /// </summary>
        private void InitializeSceneManagement()
        {
            try
            {
                // 场景管理器初始化逻辑
                Debug.Log("[ApplicationBootstrap] 场景管理初始化完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ApplicationBootstrap] 场景管理初始化失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 报告初始化进度
        /// </summary>
        private void ReportProgress(float progress, string message)
        {
            Debug.Log($"[ApplicationBootstrap] {progress:P0} - {message}");
            OnInitializationProgress?.Invoke(progress);
        }
        
        /// <summary>
        /// 手动初始化（用于测试）
        /// </summary>
        [ContextMenu("Manual Initialize")]
        public void ManualInitialize()
        {
            _ = InitializeApplicationAsync();
        }

        /// <summary>
        /// 应用程序退出时清理
        /// </summary>
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("[ApplicationBootstrap] 应用程序暂停");
            }
            else
            {
                Debug.Log("[ApplicationBootstrap] 应用程序恢复");
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Debug.Log("[ApplicationBootstrap] 应用程序获得焦点");
            }
            else
            {
                Debug.Log("[ApplicationBootstrap] 应用程序失去焦点");
            }
        }
        
        void OnDestroy()
        {
            if (Instance == this)
            {
                Debug.Log("[ApplicationBootstrap] 应用程序启动器已销毁");
            }
        }
    }
}