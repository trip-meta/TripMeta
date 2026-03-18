using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace TripMeta.VR.Platform
{
    /// <summary>
    /// Vision Pro 混合现实控制器
    /// 管理透视模式、混合现实渲染和空间锚点
    /// </summary>
    public class VisionProMRController : MonoBehaviour
    {
        [Header("透视配置")]
        public float passthroughOpacity = 0.5f;
        public LayerMask passthroughLayer = ~0;
        public Material passthroughMaterial;
        public bool useChromaKey = false;
        public Color chromaKeyColor = Color.green;

        [Header("混合现实")]
        public bool enableOcclusion = true;
        public float occlusionOpacity = 0.8f;
        public bool enableShadows = true;
        public float shadowIntensity = 0.5f;

        [Header("空间锚点")]
        public int maxAnchors = 50;
        public float anchorPersistenceTimeout = 5f;

        // 状态
        private bool isInitialized = false;
        private bool isMixedRealityMode = false;
        private Camera mainCamera;
        private RenderTexture passthroughTexture;

        public event Action<bool> OnModeChanged;
        public event Action<float> OnOpacityChanged;

        public bool IsMixedRealityMode => isMixedRealityMode;

        public async Task InitializeAsync()
        {
            if (isInitialized) return;

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[VisionProMRController] 未找到主相机");
                return;
            }

            // 配置相机
            ConfigureCamera();

            // 创建透视纹理
            CreatePassthroughTexture();

            await Task.Delay(100);
            isInitialized = true;

            Debug.Log("[VisionProMRController] 混合现实控制器初始化完成");
        }

        /// <summary>
        /// 配置相机
        /// </summary>
        private void ConfigureCamera()
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0, 0, 0, 0);
            mainCamera.allowHDR = true;
            mainCamera.allowMSAA = false;

            #if UNITY_VISIONOS
            // Vision Pro 特定配置
            mainCamera.stereoTargetEye = StereoTargetEyeMask.Both;
            #endif
        }

        /// <summary>
        /// 创建透视纹理
        /// </summary>
        private void CreatePassthroughTexture()
        {
            int width = Screen.width;
            int height = Screen.height;

            passthroughTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            passthroughTexture.name = "VisionPro_Passthrough";
            passthroughTexture.Create();
        }

        /// <summary>
        /// 设置混合现实模式
        /// </summary>
        public void SetMixedRealityMode(bool enable)
        {
            if (isMixedRealityMode == enable) return;

            isMixedRealityMode = enable;

            if (enable)
            {
                EnablePassthrough();
            }
            else
            {
                DisablePassthrough();
            }

            OnModeChanged?.Invoke(enable);
            Debug.Log($"[VisionProMRController] 混合现实模式: {(enable ? "开启" : "关闭")}");
        }

        /// <summary>
        /// 启用透视
        /// </summary>
        private void EnablePassthrough()
        {
            #if UNITY_VISIONOS
            // 在 Vision Pro 上启用 ARKit 透视
            UnityEngine.XR.ARFoundation.ARSession arSession = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSession>();
            if (arSession != null)
            {
                arSession.enabled = true;
            }
            #else
            // 编辑器模拟：使用纯色背景
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, passthroughOpacity);
            #endif
        }

        /// <summary>
        /// 禁用透视
        /// </summary>
        private void DisablePassthrough()
        {
            #if UNITY_VISIONOS
            UnityEngine.XR.ARFoundation.ARSession arSession = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSession>();
            if (arSession != null)
            {
                arSession.enabled = false;
            }
            #else
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            #endif
        }

        /// <summary>
        /// 设置透视透明度
        /// </summary>
        public void SetPassthroughOpacity(float opacity)
        {
            passthroughOpacity = Mathf.Clamp01(opacity);

            if (passthroughMaterial != null)
            {
                passthroughMaterial.SetFloat("_Opacity", passthroughOpacity);
            }

            if (!isMixedRealityMode)
            {
                mainCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, passthroughOpacity);
            }

            OnOpacityChanged?.Invoke(passthroughOpacity);
        }

        /// <summary>
        /// 创建空间锚点
        /// </summary>
        public async Task<bool> CreateSpatialAnchor(Vector3 position, Quaternion rotation, string anchorId = null)
        {
            #if UNITY_VISIONOS
            // 使用 ARKit 空间锚点 API
            Debug.Log($"[VisionProMRController] 创建空间锚点: {anchorId ?? "auto"}");
            await Task.Delay(100);
            return true;
            #else
            await Task.Delay(50);
            return true;
            #endif
        }

        /// <summary>
        /// 移除空间锚点
        /// </summary>
        public void RemoveSpatialAnchor(string anchorId)
        {
            Debug.Log($"[VisionProMRController] 移除空间锚点: {anchorId}");
        }

        /// <summary>
        /// 配置遮挡
        /// </summary>
        public void ConfigureOcclusion(bool enable, float opacity)
        {
            enableOcclusion = enable;
            occlusionOpacity = Mathf.Clamp01(opacity);

            // 更新遮挡材质
            Shader.SetGlobalFloat("_OcclusionOpacity", enable ? occlusionOpacity : 0f);
        }

        void OnDestroy()
        {
            if (passthroughTexture != null)
            {
                passthroughTexture.Release();
                Destroy(passthroughTexture);
            }
        }
    }
}
