using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;

namespace TripMeta.Core
{
    /// <summary>
    /// VR管理器 - 负责VR系统的初始化和管理
    /// </summary>
    public class VRManager : MonoBehaviour
    {
        [Header("VR Settings")]
        public bool enableVROnStart = true;
        public bool enableHandTracking = true;
        public bool enableEyeTracking = false;
        
        [Header("Performance Settings")]
        public int targetFrameRate = 72;
        public bool enableFoveatedRendering = true;
        
        [Header("Debug")]
        public bool showDebugInfo = false;
        
        private bool isVRInitialized = false;
        private XRDisplaySubsystem displaySubsystem;
        
        public static VRManager Instance { get; private set; }
        
        void Awake()
        {
            // 单例模式
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            if (enableVROnStart)
            {
                InitializeVR();
            }
        }
        
        /// <summary>
        /// 初始化VR系统
        /// </summary>
        public void InitializeVR()
        {
            Debug.Log("[VRManager] 开始初始化VR系统...");
            
            try
            {
                // 设置目标帧率
                Application.targetFrameRate = targetFrameRate;
                
                // 初始化PICO SDK
                InitializePICOSDK();
                
                // 配置性能设置
                ConfigurePerformanceSettings();
                
                // 启用手部追踪
                if (enableHandTracking)
                {
                    EnableHandTracking();
                }
                
                // 启用眼球追踪
                if (enableEyeTracking)
                {
                    EnableEyeTracking();
                }
                
                isVRInitialized = true;
                Debug.Log("[VRManager] VR系统初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VRManager] VR初始化失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 初始化PICO SDK
        /// </summary>
        private void InitializePICOSDK()
        {
            // 检查VR设备是否可用
            if (!XRSettings.enabled)
            {
                Debug.LogWarning("[VRManager] XR未启用，尝试启动VR模式");
                XRSettings.enabled = true;
            }
            
            // 获取显示子系统
            displaySubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRDisplaySubsystem>();
            
            if (displaySubsystem != null)
            {
                Debug.Log($"[VRManager] VR显示系统已加载: {displaySubsystem.SubsystemDescriptor.id}");
            }
            else
            {
                Debug.LogWarning("[VRManager] 未找到VR显示系统");
            }
        }
        
        /// <summary>
        /// 配置性能设置
        /// </summary>
        private void ConfigurePerformanceSettings()
        {
            // 启用注视点渲染
            if (enableFoveatedRendering)
            {
                try
                {
                    PXR_Plugin.System.UPxr_EnableFoveation(true);
                    Debug.Log("[VRManager] 注视点渲染已启用");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[VRManager] 注视点渲染启用失败: {e.Message}");
                }
            }
            
            // 设置渲染质量
            QualitySettings.vSyncCount = 0; // 禁用垂直同步，让VR系统控制
            QualitySettings.antiAliasing = 2; // 2x抗锯齿
        }
        
        /// <summary>
        /// 启用手部追踪
        /// </summary>
        private void EnableHandTracking()
        {
            try
            {
                PXR_Plugin.HandTracking.UPxr_StartHandTracking();
                Debug.Log("[VRManager] 手部追踪已启用");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VRManager] 手部追踪启用失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 启用眼球追踪
        /// </summary>
        private void EnableEyeTracking()
        {
            try
            {
                PXR_Plugin.EyeTracking.UPxr_StartEyeTracking();
                Debug.Log("[VRManager] 眼球追踪已启用");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VRManager] 眼球追踪启用失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 获取VR设备信息
        /// </summary>
        public VRDeviceInfo GetVRDeviceInfo()
        {
            var deviceInfo = new VRDeviceInfo();
            
            if (isVRInitialized)
            {
                deviceInfo.deviceName = XRSettings.loadedDeviceName;
                deviceInfo.isPresent = XRDevice.isPresent;
                deviceInfo.refreshRate = XRDevice.refreshRate;
                deviceInfo.eyeTextureWidth = XRSettings.eyeTextureWidth;
                deviceInfo.eyeTextureHeight = XRSettings.eyeTextureHeight;
            }
            
            return deviceInfo;
        }
        
        /// <summary>
        /// 重新居中VR视图
        /// </summary>
        public void RecenterView()
        {
            try
            {
                InputTracking.Recenter();
                Debug.Log("[VRManager] VR视图已重新居中");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VRManager] 重新居中失败: {e.Message}");
            }
        }
        
        void Update()
        {
            if (showDebugInfo && isVRInitialized)
            {
                // 显示调试信息
                DisplayDebugInfo();
            }
        }
        
        /// <summary>
        /// 显示调试信息
        /// </summary>
        private void DisplayDebugInfo()
        {
            var deviceInfo = GetVRDeviceInfo();
            
            // 这里可以添加UI显示调试信息
            // 或者使用Debug.Log定期输出状态
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("[VRManager] 应用暂停");
            }
            else
            {
                Debug.Log("[VRManager] 应用恢复");
            }
        }
        
        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    /// <summary>
    /// VR设备信息结构
    /// </summary>
    [System.Serializable]
    public struct VRDeviceInfo
    {
        public string deviceName;
        public bool isPresent;
        public float refreshRate;
        public int eyeTextureWidth;
        public int eyeTextureHeight;
    }
}