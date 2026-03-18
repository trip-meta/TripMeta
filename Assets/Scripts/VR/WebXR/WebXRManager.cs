using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace TripMeta.VR.WebXR
{
    /// <summary>
    /// WebXR 管理器 - 浏览器 VR 体验支持
    /// 无需安装应用即可在浏览器中体验 VR
    /// </summary>
    public class WebXRManager : MonoBehaviour
    {
        [Header("WebXR 配置")]
        public bool autoInitialize = true;
        public bool autoEnterVR = false;
        public bool enableWebAssembly = true;
        public bool enableCompression = true;
        public bool enableCaching = true;

        [Header("渲染配置")]
        public int targetFrameRate = 72;
        public float renderScale = 1.0f;
        public bool enableFoveatedRendering = false;
        public int foveationLevel = 1;

        [Header("输入配置")]
        public bool enableHandTracking = true;
        public bool enableGamepadInput = true;
        public bool enableTouchInput = true;
        public bool enableMouseKeyboardFallback = true;

        [Header("网络配置")]
        public string signallingServerUrl = "wss://tripmeta.io/signalling";
        public int reconnectAttempts = 3;
        public float reconnectDelay = 5f;

        [Header("云渲染")]
        public bool enableCloudRendering = false;
        public string cloudRenderingEndpoint = "";
        public int cloudRenderingBitrate = 20000000; // 20 Mbps
        public int cloudRenderingFps = 60;

        // 状态
        private bool isInitialized = false;
        private bool isInVR = false;
        private bool isSessionSupported = false;
        private WebXRSessionMode currentSessionMode = WebXRSessionMode.None;

        // 组件引用
        private WebXRInputHandler inputHandler;
        private WebXRRenderHandler renderHandler;
        private WebXRNetworkHandler networkHandler;
        private WebXRCacheManager cacheManager;

        // 事件
        public UnityEvent OnInitialized;
        public UnityEvent OnVREntered;
        public UnityEvent OnVRExited;
        public UnityEvent<WebXRHandData> OnHandDataReceived;
        public UnityEvent<WebXRHeadsetData> OnHeadsetDataReceived;
        public UnityEvent<string> OnError;

        public static WebXRManager Instance { get; private set; }

        public bool IsInitialized => isInitialized;
        public bool IsInVR => isInVR;
        public WebXRSessionMode CurrentSessionMode => currentSessionMode;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        async void Start()
        {
            if (autoInitialize)
            {
                await InitializeAsync();
            }
        }

        /// <summary>
        /// 异步初始化 WebXR
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (isInitialized) return true;

            Debug.Log("[WebXRManager] 初始化 WebXR...");

            try
            {
                // 检查 WebXR 支持
                isSessionSupported = await CheckWebXRSupport();
                if (!isSessionSupported)
                {
                    Debug.LogWarning("[WebXRManager] 浏览器不支持 WebXR");
                    SetupFallbackMode();
                }

                // 初始化输入处理
                InitializeInputHandler();

                // 初始化渲染处理
                InitializeRenderHandler();

                // 初始化网络处理
                InitializeNetworkHandler();

                // 初始化缓存管理
                InitializeCacheManager();

                // 配置 WebAssembly
                if (enableWebAssembly)
                {
                    ConfigureWebAssembly();
                }

                isInitialized = true;
                OnInitialized?.Invoke();

                Debug.Log("[WebXRManager] WebXR 初始化完成");

                // 自动进入 VR
                if (autoEnterVR && isSessionSupported)
                {
                    await EnterVRAsync(WebXRSessionMode.ImmersiveVR);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebXRManager] 初始化失败: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 检查 WebXR 支持
        /// </summary>
        private async Task<bool> CheckWebXRSupport()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            // 在真实 WebGL 构建中检查浏览器支持
            return await CheckWebXRSupportJS();
            #else
            // 编辑器或其他平台模拟支持
            await Task.Delay(50);
            return Application.platform == RuntimePlatform.WebGLPlayer ||
                   Application.isEditor;
            #endif
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern Task<bool> CheckWebXRSupportJS();
        #endif

        /// <summary>
        /// 设置回退模式
        /// </summary>
        private void SetupFallbackMode()
        {
            if (enableMouseKeyboardFallback)
            {
                Debug.Log("[WebXRManager] 启用鼠标键盘回退模式");
                // 设置鼠标键盘控制
            }
        }

        /// <summary>
        /// 初始化输入处理
        /// </summary>
        private void InitializeInputHandler()
        {
            inputHandler = gameObject.AddComponent<WebXRInputHandler>();
            inputHandler.enableHandTracking = enableHandTracking;
            inputHandler.enableGamepadInput = enableGamepadInput;
            inputHandler.enableTouchInput = enableTouchInput;
            inputHandler.OnHandDataReceived += (data) => OnHandDataReceived?.Invoke(data);
            inputHandler.Initialize();
        }

        /// <summary>
        /// 初始化渲染处理
        /// </summary>
        private void InitializeRenderHandler()
        {
            renderHandler = gameObject.AddComponent<WebXRRenderHandler>();
            renderHandler.targetFrameRate = targetFrameRate;
            renderHandler.renderScale = renderScale;
            renderHandler.enableFoveatedRendering = enableFoveatedRendering;
            renderHandler.foveationLevel = foveationLevel;
            renderHandler.Initialize();
        }

        /// <summary>
        /// 初始化网络处理
        /// </summary>
        private void InitializeNetworkHandler()
        {
            networkHandler = gameObject.AddComponent<WebXRNetworkHandler>();
            networkHandler.signallingServerUrl = signallingServerUrl;
            networkHandler.reconnectAttempts = reconnectAttempts;
            networkHandler.reconnectDelay = reconnectDelay;
            networkHandler.Initialize();
        }

        /// <summary>
        /// 初始化缓存管理
        /// </summary>
        private void InitializeCacheManager()
        {
            cacheManager = gameObject.AddComponent<WebXRCacheManager>();
            cacheManager.enableCaching = enableCaching;
            cacheManager.enableCompression = enableCompression;
            cacheManager.Initialize();
        }

        /// <summary>
        /// 配置 WebAssembly
        /// </summary>
        private void ConfigureWebAssembly()
        {
            Debug.Log("[WebXRManager] 配置 WebAssembly 优化");
            // WebAssembly 内存和性能优化配置
            #if UNITY_WEBGL
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
            #endif
        }

        /// <summary>
        /// 异步进入 VR
        /// </summary>
        public async Task<bool> EnterVRAsync(WebXRSessionMode mode)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[WebXRManager] WebXR 未初始化");
                return false;
            }

            if (isInVR)
            {
                Debug.Log("[WebXRManager] 已经在 VR 模式中");
                return true;
            }

            Debug.Log($"[WebXRManager] 请求进入 VR 模式: {mode}");

            #if UNITY_WEBGL && !UNITY_EDITOR
            bool success = await EnterVRJS(mode.ToString().ToLower());
            #else
            await Task.Delay(100);
            bool success = true;
            #endif

            if (success)
            {
                isInVR = true;
                currentSessionMode = mode;
                OnVREntered?.Invoke();
                Debug.Log("[WebXRManager] 成功进入 VR 模式");
            }
            else
            {
                OnError?.Invoke("进入 VR 失败");
            }

            return success;
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern Task<bool> EnterVRJS(string mode);
        #endif

        /// <summary>
        /// 退出 VR
        /// </summary>
        public async Task ExitVRAsync()
        {
            if (!isInVR) return;

            Debug.Log("[WebXRManager] 退出 VR 模式");

            #if UNITY_WEBGL && !UNITY_EDITOR
            await ExitVRJS();
            #else
            await Task.Delay(50);
            #endif

            isInVR = false;
            currentSessionMode = WebXRSessionMode.None;
            OnVRExited?.Invoke();
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern Task ExitVRJS();
        #endif

        /// <summary>
        /// 切换云渲染模式
        /// </summary>
        public void SetCloudRendering(bool enable)
        {
            enableCloudRendering = enable;
            if (networkHandler != null)
            {
                networkHandler.enableCloudRendering = enable;
            }
            Debug.Log($"[WebXRManager] 云渲染模式: {(enable ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 获取当前输入数据
        /// </summary>
        public WebXRHandData GetCurrentHandData()
        {
            return inputHandler?.GetCurrentHandData() ?? new WebXRHandData();
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        public WebXRDeviceInfo GetDeviceInfo()
        {
            return new WebXRDeviceInfo
            {
                isWebXRSupported = isSessionSupported,
                isInVR = isInVR,
                currentMode = currentSessionMode,
                userAgent = Application.platform.ToString(),
                renderScale = renderScale,
                targetFrameRate = targetFrameRate
            };
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// WebXR 会话模式
    /// </summary>
    public enum WebXRSessionMode
    {
        None,
        Inline,
        ImmersiveVR,
        ImmersiveAR
    }

    /// <summary>
    /// WebXR 手部数据
    /// </summary>
    [Serializable]
    public struct WebXRHandData
    {
        public bool isTracked;
        public int handIndex; // 0 = left, 1 = right
        public Vector3[] jointPositions;
        public Quaternion[] jointRotations;
        public float[] jointRadii;
        public float pinchValue;
        public float grabValue;
        public bool isPinching;
        public bool isGrabbing;

        public WebXRHandData(bool isTracked = false, int handIndex = 0)
        {
            this.isTracked = isTracked;
            this.handIndex = handIndex;
            this.jointPositions = new Vector3[25]; // WebXR 标准 25 个关节
            this.jointRotations = new Quaternion[25];
            this.jointRadii = new float[25];
            this.pinchValue = 0f;
            this.grabValue = 0f;
            this.isPinching = false;
            this.isGrabbing = false;
        }
    }

    /// <summary>
    /// WebXR 头显数据
    /// </summary>
    [Serializable]
    public struct WebXRHeadsetData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 angularVelocity;
        public Vector3 linearVelocity;
        public bool isTracked;
        public float timestamp;
    }

    /// <summary>
    /// WebXR 设备信息
    /// </summary>
    [Serializable]
    public struct WebXRDeviceInfo
    {
        public bool isWebXRSupported;
        public bool isInVR;
        public WebXRSessionMode currentMode;
        public string userAgent;
        public float renderScale;
        public int targetFrameRate;
    }

    #endregion
}
