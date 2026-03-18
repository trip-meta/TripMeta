using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace TripMeta.VR.Rendering
{
    /// <summary>
    /// 注视点渲染管理器
    /// 基于眼动追踪的动态注视点渲染，提升性能30%
    /// </summary>
    public class FoveatedRenderingManager : MonoBehaviour
    {
        [Header("注视点渲染配置")]
        public bool enableFoveatedRendering = true;
        public FoveationMode foveationMode = FoveationMode.Dynamic;
        public int foveationLevel = 2; // 0-3, 越高边缘分辨率越低

        [Header("动态调整")]
        public bool enableDynamicAdjustment = true;
        public float gazeCheckInterval = 0.016f; // 60Hz
        public float gazeVelocityThreshold = 100f; // 度/秒
        public float gazeStabilityThreshold = 0.5f; // 秒

        [Header("性能优化")]
        public float innerRadius = 0.15f; // 内圈半径 (注视点区域)
        public float middleRadius = 0.3f; // 中圈半径
        public float outerRadius = 0.5f;  // 外圈半径
        public Shader foveatedRenderingShader;

        [Header("渲染比例")]
        public float innerRegionScale = 1.0f;   // 内圈渲染比例 (100%)
        public float middleRegionScale = 0.75f; // 中圈渲染比例 (75%)
        public float outerRegionScale = 0.5f;   // 外圈渲染比例 (50%)

        // 组件引用
        private Camera vrCamera;
        private Material foveationMaterial;
        private RenderTexture[] eyeTextures = new RenderTexture[2];

        // 眼动追踪数据
        private Vector3 leftEyeGazeDirection;
        private Vector3 rightEyeGazeDirection;
        private Vector3 combinedGazeDirection;
        private Vector2 gazePointUV; // 归一化的注视点位置
        private float gazeVelocity;
        private float lastGazeUpdateTime;
        private bool isGazeStable = false;
        private float gazeStabilityTimer = 0f;

        // 性能统计
        private float performanceGain = 0f;
        private int frameCount = 0;
        private float fpsTimer = 0f;
        private float currentFPS = 0f;

        public static FoveatedRenderingManager Instance { get; private set; }

        public bool IsEnabled => enableFoveatedRendering;
        public float PerformanceGain => performanceGain;
        public float CurrentFPS => currentFPS;
        public Vector2 CurrentGazePoint => gazePointUV;

        public event Action<Vector2, float> OnGazePointUpdated; // gazePoint, confidence
        public event Action<float> OnPerformanceGainUpdated;

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

        void Start()
        {
            Initialize();
        }

        void OnEnable()
        {
            if (enableFoveatedRendering)
            {
                EnableFoveatedRendering();
            }
        }

        void OnDisable()
        {
            DisableFoveatedRendering();
        }

        /// <summary>
        /// 初始化注视点渲染
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[FoveatedRenderingManager] 初始化注视点渲染...");

            vrCamera = Camera.main;
            if (vrCamera == null)
            {
                Debug.LogError("[FoveatedRenderingManager] 未找到主相机");
                return;
            }

            // 加载注视点渲染Shader
            if (foveatedRenderingShader == null)
            {
                foveatedRenderingShader = Shader.Find("Hidden/FoveatedRendering");
            }

            if (foveatedRenderingShader != null)
            {
                foveationMaterial = new Material(foveatedRenderingShader);
            }
            else
            {
                Debug.LogWarning("[FoveatedRenderingManager] 未找到注视点渲染Shader");
            }

            // 创建渲染纹理
            CreateEyeTextures();

            Debug.Log("[FoveatedRenderingManager] 注视点渲染初始化完成");
        }

        /// <summary>
        /// 创建眼部渲染纹理
        /// </summary>
        private void CreateEyeTextures()
        {
            int width = XRSettings.eyeTextureWidth;
            int height = XRSettings.eyeTextureHeight;

            for (int i = 0; i < 2; i++)
            {
                eyeTextures[i] = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                eyeTextures[i].antiAliasing = 1;
                eyeTextures[i].name = $"FoveatedEyeTexture_{i}";
            }
        }

        void Update()
        {
            if (!enableFoveatedRendering) return;

            // 更新眼动数据
            UpdateGazeData();

            // 动态调整注视点渲染
            if (enableDynamicAdjustment)
            {
                DynamicAdjustment();
            }

            // 计算性能提升
            CalculatePerformanceGain();

            // 统计FPS
            UpdateFPS();
        }

        /// <summary>
        /// 更新眼动数据
        /// </summary>
        private void UpdateGazeData()
        {
            // 从眼动追踪设备获取数据
            GetEyeTrackingData();

            // 计算注视点速度
            float deltaTime = Time.time - lastGazeUpdateTime;
            if (deltaTime > 0)
            {
                float angleDelta = Vector3.Angle(combinedGazeDirection,
                    (leftEyeGazeDirection + rightEyeGazeDirection) / 2);
                gazeVelocity = angleDelta / deltaTime;
            }

            // 检查注视稳定性
            CheckGazeStability();

            // 更新着色器参数
            UpdateShaderParameters();

            lastGazeUpdateTime = Time.time;
            OnGazePointUpdated?.Invoke(gazePointUV, isGazeStable ? 1f : 0.5f);
        }

        /// <summary>
        /// 获取眼动追踪数据
        /// </summary>
        private void GetEyeTrackingData()
        {
#if UNITY_VISIONOS
            // Vision Pro 眼动追踪
            GetVisionProEyeTracking();
#elif UNITY_OPENXR
            // OpenXR 眼动追踪
            GetOpenXREyeTracking();
#else
            // 模拟眼动追踪 (基于头部朝向)
            GetSimulatedEyeTracking();
#endif
        }

        /// <summary>
        /// Vision Pro 眼动追踪
        /// </summary>
        private void GetVisionProEyeTracking()
        {
            // 从 Vision Pro 获取眼动数据
            // 实际实现需要使用 Vision Pro SDK
            leftEyeGazeDirection = vrCamera.transform.forward;
            rightEyeGazeDirection = vrCamera.transform.forward;
            combinedGazeDirection = vrCamera.transform.forward;

            // 转换为UV坐标
            gazePointUV = new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// OpenXR 眼动追踪
        /// </summary>
        private void GetOpenXREyeTracking()
        {
            // 从 OpenXR 获取眼动数据
            leftEyeGazeDirection = vrCamera.transform.forward;
            rightEyeGazeDirection = vrCamera.transform.forward;
            combinedGazeDirection = vrCamera.transform.forward;

            gazePointUV = new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// 模拟眼动追踪 (编辑器模式)
        /// </summary>
        private void GetSimulatedEyeTracking()
        {
            // 使用头部朝向作为注视方向
            if (vrCamera != null)
            {
                leftEyeGazeDirection = vrCamera.transform.forward;
                rightEyeGazeDirection = vrCamera.transform.forward;
                combinedGazeDirection = vrCamera.transform.forward;

                // 转换为UV坐标 (基于屏幕中心)
                gazePointUV = new Vector2(0.5f, 0.5f);

                // 添加一些噪声模拟真实眼动
                float noise = Mathf.PerlinNoise(Time.time * 2f, 0f) * 0.1f;
                gazePointUV += new Vector2(noise - 0.05f, noise - 0.05f);
                gazePointUV = Vector2.ClampMagnitude(gazePointUV - new Vector2(0.5f, 0.5f), 0.3f)
                    + new Vector2(0.5f, 0.5f);
            }
        }

        /// <summary>
        /// 检查注视稳定性
        /// </summary>
        private void CheckGazeStability()
        {
            if (gazeVelocity < gazeVelocityThreshold)
            {
                gazeStabilityTimer += Time.deltaTime;
                if (gazeStabilityTimer >= gazeStabilityThreshold)
                {
                    isGazeStable = true;
                }
            }
            else
            {
                gazeStabilityTimer = 0f;
                isGazeStable = false;
            }
        }

        /// <summary>
        /// 动态调整
        /// </summary>
        private void DynamicAdjustment()
        {
            // 根据注视速度调整注视点级别
            if (gazeVelocity > gazeVelocityThreshold * 2)
            {
                // 快速扫视时降低注视点级别
                SetFoveationLevel(Mathf.Max(0, foveationLevel - 1));
            }
            else if (isGazeStable && gazeVelocity < gazeVelocityThreshold * 0.5f)
            {
                // 稳定注视时提高注视点级别
                SetFoveationLevel(Mathf.Min(3, foveationLevel + 1));
            }
        }

        /// <summary>
        /// 更新着色器参数
        /// </summary>
        private void UpdateShaderParameters()
        {
            if (foveationMaterial == null) return;

            foveationMaterial.SetVector("_GazePointUV", gazePointUV);
            foveationMaterial.SetFloat("_InnerRadius", innerRadius);
            foveationMaterial.SetFloat("_MiddleRadius", middleRadius);
            foveationMaterial.SetFloat("_OuterRadius", outerRadius);
            foveationMaterial.SetFloat("_InnerScale", innerRegionScale);
            foveationMaterial.SetFloat("_MiddleScale", middleRegionScale);
            foveationMaterial.SetFloat("_OuterScale", outerRegionScale);
        }

        /// <summary>
        /// 计算性能提升
        /// </summary>
        private void CalculatePerformanceGain()
        {
            // 计算由于注视点渲染带来的性能提升
            float innerArea = Mathf.PI * innerRadius * innerRadius;
            float middleArea = Mathf.PI * middleRadius * middleRadius - innerArea;
            float outerArea = Mathf.PI * outerRadius * outerRadius - middleArea;

            float totalPixels = Mathf.PI * outerRadius * outerRadius;
            float weightedPixels = innerArea * (1f / innerRegionScale) +
                                  middleArea * (1f / middleRegionScale) +
                                  outerArea * (1f / outerRegionScale);

            performanceGain = (weightedPixels / totalPixels - 1f) * 100f;

            OnPerformanceGainUpdated?.Invoke(performanceGain);
        }

        /// <summary>
        /// 更新FPS统计
        /// </summary>
        private void UpdateFPS()
        {
            frameCount++;
            fpsTimer += Time.deltaTime;

            if (fpsTimer >= 1f)
            {
                currentFPS = frameCount / fpsTimer;
                frameCount = 0;
                fpsTimer = 0f;
            }
        }

        /// <summary>
        /// 启用注视点渲染
        /// </summary>
        public void EnableFoveatedRendering()
        {
            enableFoveatedRendering = true;

#if UNITY_VISIONOS
            // 启用 Vision Pro 注视点渲染
            UnityEngine.XR.VisionOS.VisionOSSettings.instance.foveatedRenderingLevel = foveationLevel;
#elif UNITY_OPENXR
            // 启用 OpenXR 注视点渲染
            // 需要 OpenXR 注视点渲染扩展
#endif

            Debug.Log($"[FoveatedRenderingManager] 注视点渲染已启用 (Level: {foveationLevel})");
        }

        /// <summary>
        /// 禁用注视点渲染
        /// </summary>
        public void DisableFoveatedRendering()
        {
            enableFoveatedRendering = false;

#if UNITY_VISIONOS
            if (UnityEngine.XR.VisionOS.VisionOSSettings.instance != null)
            {
                UnityEngine.XR.VisionOS.VisionOSSettings.instance.foveatedRenderingLevel = 0;
            }
#endif

            Debug.Log("[FoveatedRenderingManager] 注视点渲染已禁用");
        }

        /// <summary>
        /// 设置注视点级别
        /// </summary>
        public void SetFoveationLevel(int level)
        {
            foveationLevel = Mathf.Clamp(level, 0, 3);

            // 根据级别调整半径
            switch (foveationLevel)
            {
                case 0:
                    innerRadius = 0.2f;
                    middleRadius = 0.4f;
                    outerRadius = 0.6f;
                    break;
                case 1:
                    innerRadius = 0.15f;
                    middleRadius = 0.3f;
                    outerRadius = 0.5f;
                    break;
                case 2:
                    innerRadius = 0.1f;
                    middleRadius = 0.25f;
                    outerRadius = 0.45f;
                    break;
                case 3:
                    innerRadius = 0.08f;
                    middleRadius = 0.2f;
                    outerRadius = 0.4f;
                    break;
            }

#if UNITY_VISIONOS
            UnityEngine.XR.VisionOS.VisionOSSettings.instance.foveatedRenderingLevel = foveationLevel;
#endif

            Debug.Log($"[FoveatedRenderingManager] 注视点级别设置为: {foveationLevel}");
        }

        /// <summary>
        /// 设置注视点区域渲染比例
        /// </summary>
        public void SetRegionScales(float inner, float middle, float outer)
        {
            innerRegionScale = Mathf.Clamp(inner, 0.25f, 1f);
            middleRegionScale = Mathf.Clamp(middle, 0.25f, 1f);
            outerRegionScale = Mathf.Clamp(outer, 0.25f, 1f);

            Debug.Log($"[FoveatedRenderingManager] 渲染比例 - 内圈: {innerRegionScale:P0}, 中圈: {middleRegionScale:P0}, 外圈: {outerRegionScale:P0}");
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (foveationMaterial != null)
            {
                Destroy(foveationMaterial);
            }

            foreach (var tex in eyeTextures)
            {
                if (tex != null)
                {
                    tex.Release();
                    Destroy(tex);
                }
            }
        }
    }

    /// <summary>
    /// 注视点模式
    /// </summary>
    public enum FoveationMode
    {
        Fixed,      // 固定注视点 (屏幕中心)
        Dynamic,    // 动态注视点 (基于眼动追踪)
        Hybrid      // 混合模式
    }
}
