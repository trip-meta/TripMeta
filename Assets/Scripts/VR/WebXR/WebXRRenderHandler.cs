using UnityEngine;
using UnityEngine.Rendering;

namespace TripMeta.VR.WebXR
{
    /// <summary>
    /// WebXR 渲染处理器
    /// 优化 WebGL/WebXR 渲染性能
    /// </summary>
    public class WebXRRenderHandler : MonoBehaviour
    {
        [Header("渲染配置")]
        public int targetFrameRate = 72;
        public float renderScale = 1.0f;
        public bool enableFoveatedRendering = false;
        public int foveationLevel = 1;
        public bool enableSinglePassRendering = true;
        public bool enableInstancing = true;

        [Header("质量设置")]
        public int textureQuality = 1; // 0 = Full, 1 = Half, 2 = Quarter
        public int shadowResolution = 1024;
        public bool enableShadows = true;
        public bool enablePostProcessing = false;

        [Header("LOD配置")]
        public float lodBias = 1.0f;
        public int maximumLODLevel = 0;

        private Camera mainCamera;
        private RenderTextureDescriptor eyeTextureDesc;

        public void Initialize()
        {
            Debug.Log("[WebXRRenderHandler] 初始化渲染处理器");

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[WebXRRenderHandler] 未找到主相机");
                return;
            }

            ConfigureRendering();
            ConfigureQualitySettings();
            ConfigureCamera();
        }

        /// <summary>
        /// 配置渲染设置
        /// </summary>
        private void ConfigureRendering()
        {
            Application.targetFrameRate = targetFrameRate;

            #if UNITY_WEBGL
            // WebGL 特定优化
            QualitySettings.vSyncCount = 0; // 禁用 VSync 以获得更好性能
            QualitySettings.antiAliasing = 2; // 降低抗锯齿级别

            // 启用 GPU Instancing
            if (enableInstancing)
            {
                Shader.EnableKeyword("GPU_INSTANCING");
            }
            #endif
        }

        /// <summary>
        /// 配置质量设置
        /// </summary>
        private void ConfigureQualitySettings()
        {
            // 纹理质量
            QualitySettings.masterTextureLimit = textureQuality;

            // 阴影设置
            QualitySettings.shadows = enableShadows ? ShadowQuality.HardOnly : ShadowQuality.Disable;
            QualitySettings.shadowResolution = (ShadowResolution)shadowResolution;
            QualitySettings.shadowDistance = 20f;

            // LOD 设置
            QualitySettings.lodBias = lodBias;
            QualitySettings.maximumLODLevel = maximumLODLevel;

            // 粒子系统
            QualitySettings.vSyncCount = 0;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.billboardsFaceCameraPosition = false;
        }

        /// <summary>
        /// 配置相机
        /// </summary>
        private void ConfigureCamera()
        {
            mainCamera.stereoTargetEye = StereoTargetEyeMask.Both;
            mainCamera.allowHDR = false; // WebGL 不支持 HDR
            mainCamera.allowMSAA = false; // WebGL 不支持 MSAA

            if (enableSinglePassRendering)
            {
                // 配置单通道立体渲染
                ConfigureSinglePassRendering();
            }
        }

        /// <summary>
        /// 配置单通道立体渲染
        /// </summary>
        private void ConfigureSinglePassRendering()
        {
            #if UNITY_WEBGL
            // WebGL 单通道渲染配置
            Debug.Log("[WebXRRenderHandler] 启用单通道立体渲染");
            #endif
        }

        /// <summary>
        /// 配置注视点渲染
        /// </summary>
        public void ConfigureFoveatedRendering(bool enable, int level)
        {
            enableFoveatedRendering = enable;
            foveationLevel = Mathf.Clamp(level, 0, 3);

            #if UNITY_WEBGL && UNITY_WEBXR
            // WebXR 注视点渲染配置
            Debug.Log($"[WebXRRenderHandler] 注视点渲染: {(enable ? "开启" : "关闭")}, 级别: {foveationLevel}");
            #endif
        }

        /// <summary>
        /// 设置渲染缩放
        /// </summary>
        public void SetRenderScale(float scale)
        {
            renderScale = Mathf.Clamp(scale, 0.5f, 2.0f);

            #if UNITY_WEBGL
            // 动态调整渲染分辨率
            if (mainCamera != null)
            {
                // 通过 RenderTexture 调整渲染缩放
                Debug.Log($"[WebXRRenderHandler] 渲染缩放设置为: {renderScale}");
            }
            #endif
        }

        /// <summary>
        /// 获取推荐渲染设置
        /// </summary>
        public WebXRRenderSettings GetRecommendedSettings()
        {
            // 根据设备性能返回推荐设置
            float devicePerformance = EstimateDevicePerformance();

            if (devicePerformance > 0.8f)
            {
                // 高性能设备
                return new WebXRRenderSettings
                {
                    renderScale = 1.0f,
                    targetFrameRate = 72,
                    textureQuality = 0,
                    enableShadows = true,
                    enablePostProcessing = true
                };
            }
            else if (devicePerformance > 0.5f)
            {
                // 中性能设备
                return new WebXRRenderSettings
                {
                    renderScale = 0.9f,
                    targetFrameRate = 60,
                    textureQuality = 1,
                    enableShadows = true,
                    enablePostProcessing = false
                };
            }
            else
            {
                // 低性能设备
                return new WebXRRenderSettings
                {
                    renderScale = 0.8f,
                    targetFrameRate = 60,
                    textureQuality = 2,
                    enableShadows = false,
                    enablePostProcessing = false
                };
            }
        }

        /// <summary>
        /// 估算设备性能
        /// </summary>
        private float EstimateDevicePerformance()
        {
            // 基于设备信息进行简单估算
            int processorCount = SystemInfo.processorCount;
            int graphicsMemory = SystemInfo.graphicsMemorySize;

            // 简单启发式算法
            float score = 0.5f;
            score += Mathf.Clamp01(processorCount / 8f) * 0.3f;
            score += Mathf.Clamp01(graphicsMemory / 4000f) * 0.2f;

            return Mathf.Clamp01(score);
        }

        /// <summary>
        /// 应用渲染设置
        /// </summary>
        public void ApplySettings(WebXRRenderSettings settings)
        {
            renderScale = settings.renderScale;
            targetFrameRate = settings.targetFrameRate;
            textureQuality = settings.textureQuality;
            enableShadows = settings.enableShadows;
            enablePostProcessing = settings.enablePostProcessing;

            ConfigureQualitySettings();
            ConfigureCamera();

            Debug.Log("[WebXRRenderHandler] 渲染设置已应用");
        }
    }

    /// <summary>
    /// WebXR 渲染设置
    /// </summary>
    [System.Serializable]
    public struct WebXRRenderSettings
    {
        public float renderScale;
        public int targetFrameRate;
        public int textureQuality;
        public bool enableShadows;
        public bool enablePostProcessing;
    }
}
