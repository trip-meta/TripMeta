using UnityEngine;
using TripMeta.Core.DependencyInjection;
using TripMeta.Core.Configuration;
using TripMeta.Core.ErrorHandling;
using TripMeta.Interaction;

namespace TripMeta.Core
{
    /// <summary>
    /// 游戏主管理器 - 负责初始化和协调各个系统
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("系统设置")]
        [SerializeField] private bool initializeVR = true;
        [SerializeField] private bool initializeAI = false; // 暂时关闭AI，先让基础系统运行
        [SerializeField] private bool enableDebugMode = true;
        
        // 系统组件
        private IServiceContainer serviceContainer;
        private VRManager vrManager;
        private VRControllerManager controllerManager;
        private TripMetaConfig config;
        
        // 初始化状态
        private bool isInitialized = false;
        
        void Awake()
        {
            // 确保GameManager是单例
            if (FindObjectsOfType<GameManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
        }
        
        void Start()
        {
            if (isInitialized)
            {
                StartSystems();
            }
        }
        
        /// <summary>
        /// 初始化所有系统
        /// </summary>
        private void InitializeSystems()
        {
            try
            {
                Debug.Log("=== TripMeta 系统初始化开始 ===");
                
                // 1. 初始化配置系统
                InitializeConfiguration();
                
                // 2. 初始化依赖注入容器
                InitializeDependencyInjection();
                
                // 3. 初始化错误处理
                InitializeErrorHandling();
                
                // 4. 初始化VR系统
                if (initializeVR)
                {
                    InitializeVRSystems();
                }
                
                // 5. 初始化AI系统（暂时跳过）
                if (initializeAI)
                {
                    // InitializeAISystems(); // 暂时注释掉
                    Debug.Log("AI系统初始化已跳过（开发阶段）");
                }
                
                isInitialized = true;
                Debug.Log("=== TripMeta 系统初始化完成 ===");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"系统初始化失败: {e.Message}");
                ErrorHandler.HandleError(e, "GameManager.InitializeSystems");
            }
        }
        
        /// <summary>
        /// 初始化配置系统
        /// </summary>
        private void InitializeConfiguration()
        {
            try
            {
                // 查找或创建配置对象
                config = FindObjectOfType<TripMetaConfig>();
                if (config == null)
                {
                    var configGO = new GameObject("TripMetaConfig");
                    config = configGO.AddComponent<TripMetaConfig>();
                    DontDestroyOnLoad(configGO);
                }
                
                Debug.Log("配置系统初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"配置系统初始化失败: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化依赖注入
        /// </summary>
        private void InitializeDependencyInjection()
        {
            try
            {
                serviceContainer = new ServiceContainer();
                
                // 注册核心服务
                serviceContainer.RegisterSingleton<TripMetaConfig>(config);
                
                Debug.Log("依赖注入系统初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"依赖注入初始化失败: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化错误处理
        /// </summary>
        private void InitializeErrorHandling()
        {
            try
            {
                // 设置全局错误处理
                Application.logMessageReceived += OnLogMessageReceived;
                
                Debug.Log("错误处理系统初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"错误处理初始化失败: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 初始化VR系统
        /// </summary>
        private void InitializeVRSystems()
        {
            try
            {
                // 查找或创建VR管理器
                vrManager = FindObjectOfType<VRManager>();
                if (vrManager == null)
                {
                    var vrGO = new GameObject("VRManager");
                    vrManager = vrGO.AddComponent<VRManager>();
                    DontDestroyOnLoad(vrGO);
                }
                
                // 查找或创建控制器管理器
                controllerManager = FindObjectOfType<VRControllerManager>();
                if (controllerManager == null)
                {
                    var controllerGO = new GameObject("VRControllerManager");
                    controllerManager = controllerGO.AddComponent<VRControllerManager>();
                    DontDestroyOnLoad(controllerGO);
                }
                
                // 注册VR服务
                serviceContainer.RegisterSingleton<VRManager>(vrManager);
                serviceContainer.RegisterSingleton<VRControllerManager>(controllerManager);
                
                Debug.Log("VR系统初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"VR系统初始化失败: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 启动所有系统
        /// </summary>
        private void StartSystems()
        {
            try
            {
                Debug.Log("=== TripMeta 系统启动开始 ===");
                
                // 启动VR系统
                if (vrManager != null)
                {
                    // VRManager会在其Start方法中自动初始化
                    Debug.Log("VR系统启动完成");
                }
                
                // 设置控制器事件监听
                if (controllerManager != null)
                {
                    SetupControllerEvents();
                    Debug.Log("控制器事件监听设置完成");
                }
                
                Debug.Log("=== TripMeta 系统启动完成 ===");
                
                // 显示系统状态
                if (enableDebugMode)
                {
                    ShowSystemStatus();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"系统启动失败: {e.Message}");
                ErrorHandler.HandleError(e, "GameManager.StartSystems");
            }
        }
        
        /// <summary>
        /// 设置控制器事件监听
        /// </summary>
        private void SetupControllerEvents()
        {
            if (controllerManager != null)
            {
                controllerManager.OnLeftTriggerChanged += OnLeftTriggerChanged;
                controllerManager.OnRightTriggerChanged += OnRightTriggerChanged;
                controllerManager.OnLeftGripChanged += OnLeftGripChanged;
                controllerManager.OnRightGripChanged += OnRightGripChanged;
            }
        }
        
        /// <summary>
        /// 显示系统状态
        /// </summary>
        private void ShowSystemStatus()
        {
            Debug.Log("=== 系统状态 ===");
            Debug.Log($"VR系统: {(vrManager != null ? "已启动" : "未启动")}");
            Debug.Log($"控制器系统: {(controllerManager != null ? "已启动" : "未启动")}");
            Debug.Log($"配置系统: {(config != null ? "已加载" : "未加载")}");
            Debug.Log($"依赖注入: {(serviceContainer != null ? "已初始化" : "未初始化")}");
            Debug.Log("================");
        }
        
        // 控制器事件处理
        private void OnLeftTriggerChanged(bool pressed)
        {
            if (enableDebugMode)
                Debug.Log($"左控制器扳机: {(pressed ? "按下" : "释放")}");
        }
        
        private void OnRightTriggerChanged(bool pressed)
        {
            if (enableDebugMode)
                Debug.Log($"右控制器扳机: {(pressed ? "按下" : "释放")}");
        }
        
        private void OnLeftGripChanged(bool pressed)
        {
            if (enableDebugMode)
                Debug.Log($"左控制器握持: {(pressed ? "按下" : "释放")}");
        }
        
        private void OnRightGripChanged(bool pressed)
        {
            if (enableDebugMode)
                Debug.Log($"右控制器握持: {(pressed ? "按下" : "释放")}");
        }
        
        /// <summary>
        /// 日志消息处理
        /// </summary>
        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                ErrorHandler.HandleError(new System.Exception(logString), "Unity.LogSystem", stackTrace);
            }
        }
        
        void OnDestroy()
        {
            // 清理事件监听
            if (controllerManager != null)
            {
                controllerManager.OnLeftTriggerChanged -= OnLeftTriggerChanged;
                controllerManager.OnRightTriggerChanged -= OnRightTriggerChanged;
                controllerManager.OnLeftGripChanged -= OnLeftGripChanged;
                controllerManager.OnRightGripChanged -= OnRightGripChanged;
            }
            
            Application.logMessageReceived -= OnLogMessageReceived;
        }
        
        // 公共属性
        public bool IsInitialized => isInitialized;
        public VRManager VRManager => vrManager;
        public VRControllerManager ControllerManager => controllerManager;
        public TripMetaConfig Config => config;
        public IServiceContainer ServiceContainer => serviceContainer;
    }
}