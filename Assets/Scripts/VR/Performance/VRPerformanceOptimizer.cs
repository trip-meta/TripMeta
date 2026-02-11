using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using TripMeta.Core.DependencyInjection;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.VR.Performance
{
    /// <summary>
    /// VR性能优化器 - 动态优化VR渲染性能
    /// </summary>
    public class VRPerformanceOptimizer : MonoBehaviour, IService
    {
        [Header("性能目标")]
        [SerializeField] private float targetFrameRate = 90f;
        [SerializeField] private float minFrameRate = 72f;
        [SerializeField] private float maxFrameTime = 11.1f; // 90fps = 11.1ms
        
        [Header("渲染优化")]
        [SerializeField] private bool enableDynamicResolution = true;
        [SerializeField] private float minRenderScale = 0.5f;
        [SerializeField] private float maxRenderScale = 1.0f;
        [SerializeField] private bool enableFoveatedRendering = true;
        [SerializeField] private bool enableFixedFoveatedRendering = true;
        
        [Header("LOD优化")]
        [SerializeField] private bool enableDynamicLOD = true;
        [SerializeField] private float lodBias = 1.0f;
        [SerializeField] private float maxLODLevel = 2.0f;
        [SerializeField] private float lodUpdateInterval = 0.1f;
        
        [Header("遮挡剔除")]
        [SerializeField] private bool enableOcclusionCulling = true;
        [SerializeField] private float cullingDistance = 100f;
        [SerializeField] private LayerMask cullingMask = -1;
        
        [Header("阴影优化")]
        [SerializeField] private bool enableDynamicShadows = true;
        [SerializeField] private ShadowQuality shadowQuality = ShadowQuality.Medium;
        [SerializeField] private float shadowDistance = 50f;
        [SerializeField] private int shadowCascades = 2;
        
        [Header("后处理优化")]
        [SerializeField] private bool enablePostProcessing = true;
        [SerializeField] private PostProcessingQuality postProcessingQuality = PostProcessingQuality.Medium;
        
        // 性能监控
        private PerformanceMetrics currentMetrics;
        private Queue<float> frameTimeHistory;
        private float lastOptimizationTime;
        private float optimizationInterval = 1f;
        
        // 渲染组件
        private UniversalRenderPipelineAsset urpAsset;
        private Camera vrCamera;
        private List<LODGroup> lodGroups;
        private List<Renderer> dynamicRenderers;
        
        // 优化状态
        private OptimizationLevel currentOptimizationLevel;
        private bool isOptimizing = false;
        
        public event Action<PerformanceMetrics> OnPerformanceUpdated;
        public event Action<OptimizationLevel> OnOptimizationLevelChanged;
        
        public PerformanceMetrics CurrentMetrics => currentMetrics;
        public OptimizationLevel CurrentOptimizationLevel => currentOptimizationLevel;
        
        private void Awake()
        {
            InitializeOptimizer();
        }
        
        private void Start()
        {
            ServiceLocator.RegisterService<VRPerformanceOptimizer>(this);
            StartCoroutine(PerformanceMonitoringCoroutine());
        }
        
        private void Update()
        {
            UpdatePerformanceMetrics();
            
            if (Time.time - lastOptimizationTime > optimizationInterval)
            {
                OptimizePerformance();
                lastOptimizationTime = Time.time;
            }
        }
        
        /// <summary>
        /// 初始化优化器
        /// </summary>
        private void InitializeOptimizer()
        {
            try
            {
                Logger.LogInfo("初始化VR性能优化器...", "VRPerformanceOptimizer");
                
                // 初始化性能指标
                currentMetrics = new PerformanceMetrics();
                frameTimeHistory = new Queue<float>();
                
                // 获取渲染管线资产
                urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                
                // 查找VR相机
                vrCamera = Camera.main;
                if (vrCamera == null)
                {
                    vrCamera = FindObjectOfType<Camera>();
                }
                
                // 收集LOD组
                lodGroups = new List<LODGroup>(FindObjectsOfType<LODGroup>());
                
                // 收集动态渲染器
                dynamicRenderers = new List<Renderer>();
                var renderers = FindObjectsOfType<Renderer>();
                foreach (var renderer in renderers)
                {
                    if (renderer.gameObject.layer != LayerMask.NameToLayer("UI"))
                    {
                        dynamicRenderers.Add(renderer);
                    }
                }
                
                // 设置初始优化级别
                currentOptimizationLevel = OptimizationLevel.Balanced;
                
                // 应用初始设置
                ApplyOptimizationSettings();
                
                Logger.LogInfo("VR性能优化器初始化完成", "VRPerformanceOptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "VR性能优化器初始化失败");
            }
        }
        
        /// <summary>
        /// 更新性能指标
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            // 更新帧时间
            float frameTime = Time.unscaledDeltaTime * 1000f; // 转换为毫秒
            frameTimeHistory.Enqueue(frameTime);
            
            if (frameTimeHistory.Count > 60) // 保持60帧历史
            {
                frameTimeHistory.Dequeue();
            }
            
            // 计算平均帧时间
            float totalFrameTime = 0f;
            foreach (float time in frameTimeHistory)
            {
                totalFrameTime += time;
            }
            currentMetrics.averageFrameTime = totalFrameTime / frameTimeHistory.Count;
            currentMetrics.currentFrameRate = 1000f / currentMetrics.averageFrameTime;
            
            // 更新GPU指标
            currentMetrics.gpuMemoryUsage = SystemInfo.graphicsMemorySize;
            currentMetrics.renderScale = XRSettings.eyeTextureResolutionScale;
            
            // 更新渲染统计
            currentMetrics.drawCalls = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize("Render.Mesh") / 1024; // 近似值
            currentMetrics.triangles = 0; // 需要通过其他方式获取
            
            OnPerformanceUpdated?.Invoke(currentMetrics);
        }
        
        /// <summary>
        /// 优化性能
        /// </summary>
        private void OptimizePerformance()
        {
            if (isOptimizing) return;
            
            isOptimizing = true;
            
            try
            {
                // 根据当前性能决定优化策略
                if (currentMetrics.currentFrameRate < minFrameRate)
                {
                    // 性能不足，提高优化级别
                    IncreaseOptimization();
                }
                else if (currentMetrics.currentFrameRate > targetFrameRate + 10f)
                {
                    // 性能过剩，降低优化级别以提高质量
                    DecreaseOptimization();
                }
                
                // 应用优化设置
                ApplyOptimizationSettings();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "性能优化失败");
            }
            finally
            {
                isOptimizing = false;
            }
        }
        
        /// <summary>
        /// 提高优化级别
        /// </summary>
        private void IncreaseOptimization()
        {
            switch (currentOptimizationLevel)
            {
                case OptimizationLevel.Quality:
                    currentOptimizationLevel = OptimizationLevel.Balanced;
                    break;
                case OptimizationLevel.Balanced:
                    currentOptimizationLevel = OptimizationLevel.Performance;
                    break;
                case OptimizationLevel.Performance:
                    currentOptimizationLevel = OptimizationLevel.Maximum;
                    break;
            }
            
            Logger.LogInfo($"提高优化级别到: {currentOptimizationLevel}", "VRPerformanceOptimizer");
            OnOptimizationLevelChanged?.Invoke(currentOptimizationLevel);
        }
        
        /// <summary>
        /// 降低优化级别
        /// </summary>
        private void DecreaseOptimization()
        {
            switch (currentOptimizationLevel)
            {
                case OptimizationLevel.Maximum:
                    currentOptimizationLevel = OptimizationLevel.Performance;
                    break;
                case OptimizationLevel.Performance:
                    currentOptimizationLevel = OptimizationLevel.Balanced;
                    break;
                case OptimizationLevel.Balanced:
                    currentOptimizationLevel = OptimizationLevel.Quality;
                    break;
            }
            
            Logger.LogInfo($"降低优化级别到: {currentOptimizationLevel}", "VRPerformanceOptimizer");
            OnOptimizationLevelChanged?.Invoke(currentOptimizationLevel);
        }
        
        /// <summary>
        /// 应用优化设置
        /// </summary>
        private void ApplyOptimizationSettings()
        {
            switch (currentOptimizationLevel)
            {
                case OptimizationLevel.Quality:
                    ApplyQualitySettings();
                    break;
                case OptimizationLevel.Balanced:
                    ApplyBalancedSettings();
                    break;
                case OptimizationLevel.Performance:
                    ApplyPerformanceSettings();
                    break;
                case OptimizationLevel.Maximum:
                    ApplyMaximumOptimizationSettings();
                    break;
            }
        }
        
        /// <summary>
        /// 应用质量设置
        /// </summary>
        private void ApplyQualitySettings()
        {
            // 渲染缩放
            if (enableDynamicResolution)
            {
                XRSettings.eyeTextureResolutionScale = maxRenderScale;
            }
            
            // LOD设置
            QualitySettings.lodBias = 2.0f;
            QualitySettings.maximumLODLevel = 0;
            
            // 阴影设置
            if (urpAsset != null)
            {
                urpAsset.shadowDistance = shadowDistance;
                urpAsset.shadowCascadeCount = 4;
            }
            
            // 后处理
            EnablePostProcessing(true);
        }
        
        /// <summary>
        /// 应用平衡设置
        /// </summary>
        private void ApplyBalancedSettings()
        {
            // 渲染缩放
            if (enableDynamicResolution)
            {
                XRSettings.eyeTextureResolutionScale = 0.8f;
            }
            
            // LOD设置
            QualitySettings.lodBias = 1.5f;
            QualitySettings.maximumLODLevel = 1;
            
            // 阴影设置
            if (urpAsset != null)
            {
                urpAsset.shadowDistance = shadowDistance * 0.8f;
                urpAsset.shadowCascadeCount = shadowCascades;
            }
            
            // 后处理
            EnablePostProcessing(enablePostProcessing);
        }
        
        /// <summary>
        /// 应用性能设置
        /// </summary>
        private void ApplyPerformanceSettings()
        {
            // 渲染缩放
            if (enableDynamicResolution)
            {
                XRSettings.eyeTextureResolutionScale = 0.7f;
            }
            
            // LOD设置
            QualitySettings.lodBias = 1.0f;
            QualitySettings.maximumLODLevel = 2;
            
            // 阴影设置
            if (urpAsset != null)
            {
                urpAsset.shadowDistance = shadowDistance * 0.6f;
                urpAsset.shadowCascadeCount = 2;
            }
            
            // 后处理
            EnablePostProcessing(false);
        }
        
        /// <summary>
        /// 应用最大优化设置
        /// </summary>
        private void ApplyMaximumOptimizationSettings()
        {
            // 渲染缩放
            if (enableDynamicResolution)
            {
                XRSettings.eyeTextureResolutionScale = minRenderScale;
            }
            
            // LOD设置
            QualitySettings.lodBias = 0.5f;
            QualitySettings.maximumLODLevel = 3;
            
            // 阴影设置
            if (urpAsset != null)
            {
                urpAsset.shadowDistance = shadowDistance * 0.4f;
                urpAsset.shadowCascadeCount = 1;
            }
            
            // 禁用后处理
            EnablePostProcessing(false);
            
            // 启用更激进的剔除
            EnableAggressiveCulling(true);
        }
        
        /// <summary>
        /// 启用/禁用后处理
        /// </summary>
        private void EnablePostProcessing(bool enable)
        {
            var postProcessVolumes = FindObjectsOfType<Volume>();
            foreach (var volume in postProcessVolumes)
            {
                volume.enabled = enable;
            }
        }
        
        /// <summary>
        /// 启用激进剔除
        /// </summary>
        private void EnableAggressiveCulling(bool enable)
        {
            if (vrCamera != null)
            {
                vrCamera.farClipPlane = enable ? cullingDistance * 0.5f : cullingDistance;
            }
            
            // 动态禁用远距离渲染器
            foreach (var renderer in dynamicRenderers)
            {
                if (renderer != null)
                {
                    float distance = Vector3.Distance(vrCamera.transform.position, renderer.transform.position);
                    renderer.enabled = !enable || distance < cullingDistance * 0.7f;
                }
            }
        }
        
        /// <summary>
        /// 性能监控协程
        /// </summary>
        private IEnumerator PerformanceMonitoringCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                
                // 检查性能警告
                if (currentMetrics.currentFrameRate < minFrameRate)
                {
                    Logger.LogWarning($"VR帧率过低: {currentMetrics.currentFrameRate:F1}fps", "VRPerformanceOptimizer");
                }
                
                if (currentMetrics.averageFrameTime > maxFrameTime * 1.5f)
                {
                    Logger.LogWarning($"VR帧时间过长: {currentMetrics.averageFrameTime:F1}ms", "VRPerformanceOptimizer");
                }
            }
        }
        
        /// <summary>
        /// 设置目标帧率
        /// </summary>
        public void SetTargetFrameRate(float frameRate)
        {
            targetFrameRate = frameRate;
            maxFrameTime = 1000f / frameRate;
        }
        
        /// <summary>
        /// 强制优化级别
        /// </summary>
        public void ForceOptimizationLevel(OptimizationLevel level)
        {
            currentOptimizationLevel = level;
            ApplyOptimizationSettings();
            OnOptimizationLevelChanged?.Invoke(currentOptimizationLevel);
        }
        
        /// <summary>
        /// 启用/禁用动态分辨率
        /// </summary>
        public void SetDynamicResolutionEnabled(bool enabled)
        {
            enableDynamicResolution = enabled;
        }
        
        /// <summary>
        /// 启用/禁用注视点渲染
        /// </summary>
        public void SetFoveatedRenderingEnabled(bool enabled)
        {
            enableFoveatedRendering = enabled;
            
            // 这里需要根据具体的VR SDK实现注视点渲染
            // 例如Oculus、PICO等都有各自的API
        }
        
        public void Initialize()
        {
            if (urpAsset == null)
            {
                InitializeOptimizer();
            }
        }
        
        public void Cleanup()
        {
            StopAllCoroutines();
            lodGroups?.Clear();
            dynamicRenderers?.Clear();
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
    }
    
    /// <summary>
    /// 性能指标
    /// </summary>
    [Serializable]
    public class PerformanceMetrics
    {
        public float currentFrameRate;
        public float averageFrameTime;
        public float renderScale;
        public int gpuMemoryUsage;
        public int drawCalls;
        public int triangles;
        public float cpuUsage;
        public float gpuUsage;
    }
    
    /// <summary>
    /// 优化级别
    /// </summary>
    public enum OptimizationLevel
    {
        Quality,        // 质量优先
        Balanced,       // 平衡
        Performance,    // 性能优先
        Maximum         // 最大优化
    }
    
    /// <summary>
    /// 后处理质量
    /// </summary>
    public enum PostProcessingQuality
    {
        Disabled,
        Low,
        Medium,
        High
    }
}