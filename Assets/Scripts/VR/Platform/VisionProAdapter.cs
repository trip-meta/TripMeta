using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace TripMeta.VR.Platform
{
    /// <summary>
    /// Apple Vision Pro 平台适配器
    /// 空间计算API集成、手势交互、混合现实渲染
    /// </summary>
    public class VisionProAdapter : MonoBehaviour, IVRPlatformAdapter
    {
        [Header("Vision Pro 配置")]
        public bool enableHandTracking = true;
        public bool enableEyeTracking = true;
        public bool enableMixedReality = true;
        public bool enableSpatialAudio = true;
        public bool enableFoveatedRendering = true;

        [Header("手势交互")]
        public float handGestureThreshold = 0.8f;
        public float pinchThreshold = 0.02f;
        public float airTapDistance = 0.1f;

        [Header("混合现实")]
        public float passthroughOpacity = 0.5f;
        public LayerMask passthroughLayer = ~0;
        public Material passthroughMaterial;

        [Header("性能优化")]
        public int foveationLevel = 2;
        public float dynamicFoveationRadius = 0.15f;
        public bool enableDynamicResolution = true;

        // Vision Pro 特有功能
        private VisionProHandTracker handTracker;
        private VisionProEyeTracker eyeTracker;
        private VisionProMRController mrController;
        private VisionProSpatialAudio spatialAudio;

        // 状态
        private bool isInitialized = false;
        private bool isRunning = false;

        public VRPlatformType PlatformType => VRPlatformType.VisionPro;
        public bool IsInitialized => isInitialized;
        public bool IsRunning => isRunning;

        // 事件
        public event Action<bool> OnInitializationComplete;
        public event Action<VisionProHandData> OnHandDataUpdated;
        public event Action<VisionProEyeData> OnEyeDataUpdated;
        public event Action<bool> OnMixedRealityModeChanged;

        async void Start()
        {
            await InitializeAsync();
        }

        /// <summary>
        /// 异步初始化 Vision Pro 适配器
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (isInitialized) return true;

            Debug.Log("[VisionProAdapter] 初始化 Apple Vision Pro 适配器...");

            try
            {
                // 检查 Vision Pro 运行时
                if (!CheckVisionProRuntime())
                {
                    Debug.LogWarning("[VisionProAdapter] 未检测到 Vision Pro 运行时，使用模拟模式");
                    SetupSimulationMode();
                }

                // 初始化手势追踪
                if (enableHandTracking)
                {
                    await InitializeHandTracking();
                }

                // 初始化眼动追踪
                if (enableEyeTracking)
                {
                    await InitializeEyeTracking();
                }

                // 初始化混合现实
                if (enableMixedReality)
                {
                    await InitializeMixedReality();
                }

                // 初始化空间音频
                if (enableSpatialAudio)
                {
                    InitializeSpatialAudio();
                }

                // 初始化注视点渲染
                if (enableFoveatedRendering)
                {
                    InitializeFoveatedRendering();
                }

                isInitialized = true;
                OnInitializationComplete?.Invoke(true);

                Debug.Log("[VisionProAdapter] Vision Pro 适配器初始化完成");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VisionProAdapter] 初始化失败: {ex.Message}");
                OnInitializationComplete?.Invoke(false);
                return false;
            }
        }

        /// <summary>
        /// 检查 Vision Pro 运行时
        /// </summary>
        private bool CheckVisionProRuntime()
        {
            // 检查是否是 visionOS 平台
            #if UNITY_VISIONOS
            return true;
            #else
            // 编辑器模拟模式
            return Application.isEditor;
            #endif
        }

        /// <summary>
        /// 设置模拟模式
        /// </summary>
        private void SetupSimulationMode()
        {
            Debug.Log("[VisionProAdapter] 启用 Vision Pro 模拟模式");
            // 在编辑器中模拟 Vision Pro 功能
        }

        /// <summary>
        /// 初始化手势追踪
        /// </summary>
        private async Task InitializeHandTracking()
        {
            handTracker = gameObject.AddComponent<VisionProHandTracker>();
            handTracker.gestureThreshold = handGestureThreshold;
            handTracker.pinchThreshold = pinchThreshold;
            handTracker.OnHandDataUpdated += (data) => OnHandDataUpdated?.Invoke(data);

            await handTracker.InitializeAsync();
            Debug.Log("[VisionProAdapter] 手势追踪初始化完成");
        }

        /// <summary>
        /// 初始化眼动追踪
        /// </summary>
        private async Task InitializeEyeTracking()
        {
            eyeTracker = gameObject.AddComponent<VisionProEyeTracker>();
            eyeTracker.OnEyeDataUpdated += (data) => OnEyeDataUpdated?.Invoke(data);

            await eyeTracker.InitializeAsync();
            Debug.Log("[VisionProAdapter] 眼动追踪初始化完成");
        }

        /// <summary>
        /// 初始化混合现实
        /// </summary>
        private async Task InitializeMixedReality()
        {
            mrController = gameObject.AddComponent<VisionProMRController>();
            mrController.passthroughOpacity = passthroughOpacity;
            mrController.passthroughLayer = passthroughLayer;
            mrController.passthroughMaterial = passthroughMaterial;
            mrController.OnModeChanged += (isMR) => OnMixedRealityModeChanged?.Invoke(isMR);

            await mrController.InitializeAsync();
            Debug.Log("[VisionProAdapter] 混合现实初始化完成");
        }

        /// <summary>
        /// 初始化空间音频
        /// </summary>
        private void InitializeSpatialAudio()
        {
            spatialAudio = gameObject.AddComponent<VisionProSpatialAudio>();
            spatialAudio.Initialize();
            Debug.Log("[VisionProAdapter] 空间音频初始化完成");
        }

        /// <summary>
        /// 初始化注视点渲染
        /// </summary>
        private void InitializeFoveatedRendering()
        {
            #if UNITY_VISIONOS
            // 配置 Vision Pro 注视点渲染
            UnityEngine.XR.VisionOS.VisionOSSettings.instance.foveatedRenderingLevel = foveationLevel;
            #endif

            Debug.Log($"[VisionProAdapter] 注视点渲染初始化完成 (Level: {foveationLevel})");
        }

        /// <summary>
        /// 启动 Vision Pro 功能
        /// </summary>
        public void StartTracking()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[VisionProAdapter] 适配器未初始化");
                return;
            }

            handTracker?.StartTracking();
            eyeTracker?.StartTracking();
            isRunning = true;

            Debug.Log("[VisionProAdapter] 追踪已启动");
        }

        /// <summary>
        /// 停止 Vision Pro 功能
        /// </summary>
        public void StopTracking()
        {
            handTracker?.StopTracking();
            eyeTracker?.StopTracking();
            isRunning = false;

            Debug.Log("[VisionProAdapter] 追踪已停止");
        }

        /// <summary>
        /// 切换混合现实模式
        /// </summary>
        public void SetMixedRealityMode(bool enable)
        {
            mrController?.SetMixedRealityMode(enable);
        }

        /// <summary>
        /// 获取当前手势数据
        /// </summary>
        public VisionProHandData GetCurrentHandData()
        {
            return handTracker?.GetCurrentHandData() ?? new VisionProHandData();
        }

        /// <summary>
        /// 获取当前眼动数据
        /// </summary>
        public VisionProEyeData GetCurrentEyeData()
        {
            return eyeTracker?.GetCurrentEyeData() ?? new VisionProEyeData();
        }

        /// <summary>
        /// 检查是否支持特定手势
        /// </summary>
        public bool IsGestureSupported(VisionProGestureType gestureType)
        {
            return handTracker?.IsGestureSupported(gestureType) ?? false;
        }

        void OnDestroy()
        {
            StopTracking();

            if (handTracker != null) Destroy(handTracker);
            if (eyeTracker != null) Destroy(eyeTracker);
            if (mrController != null) Destroy(mrController);
            if (spatialAudio != null) Destroy(spatialAudio);
        }
    }

    #region 数据类型

    /// <summary>
    /// Vision Pro 手势类型
    /// </summary>
    public enum VisionProGestureType
    {
        None,
        Pinch,              // 捏合
        AirTap,             // 空中点击
        DoublePinch,        // 双指捏合
        Grab,               // 抓取
        Release,            // 释放
        SwipeLeft,          // 左滑
        SwipeRight,         // 右滑
        SwipeUp,            // 上滑
        SwipeDown,          // 下滑
        Rotate,             // 旋转
        Zoom                // 缩放
    }

    /// <summary>
    /// Vision Pro 手势数据
    /// </summary>
    [Serializable]
    public struct VisionProHandData
    {
        public bool isTracked;
        public Vector3 handPosition;
        public Quaternion handRotation;
        public Vector3[] fingerPositions;
        public VisionProGestureType currentGesture;
        public float gestureConfidence;
        public bool isPinching;
        public float pinchStrength;
        public Vector3 palmNormal;
        public float timestamp;

        public VisionProHandData(bool isTracked = false)
        {
            this.isTracked = isTracked;
            this.handPosition = Vector3.zero;
            this.handRotation = Quaternion.identity;
            this.fingerPositions = new Vector3[5];
            this.currentGesture = VisionProGestureType.None;
            this.gestureConfidence = 0f;
            this.isPinching = false;
            this.pinchStrength = 0f;
            this.palmNormal = Vector3.up;
            this.timestamp = Time.time;
        }
    }

    /// <summary>
    /// Vision Pro 眼动数据
    /// </summary>
    [Serializable]
    public struct VisionProEyeData
    {
        public bool isTracked;
        public Vector3 gazeOrigin;
        public Vector3 gazeDirection;
        public Vector3 gazePoint;
        public float leftEyeOpenness;
        public float rightEyeOpenness;
        public Vector2 leftEyePosition;
        public Vector2 rightEyePosition;
        public float fixationDuration;
        public float timestamp;

        public VisionProEyeData(bool isTracked = false)
        {
            this.isTracked = isTracked;
            this.gazeOrigin = Vector3.zero;
            this.gazeDirection = Vector3.forward;
            this.gazePoint = Vector3.zero;
            this.leftEyeOpenness = 1f;
            this.rightEyeOpenness = 1f;
            this.leftEyePosition = Vector2.zero;
            this.rightEyePosition = Vector2.zero;
            this.fixationDuration = 0f;
            this.timestamp = Time.time;
        }
    }

    /// <summary>
    /// VR 平台类型
    /// </summary>
    public enum VRPlatformType
    {
        Generic,
        VisionPro,
        Quest,
        Pico,
        HTC,
        WebXR
    }

    #endregion
}
