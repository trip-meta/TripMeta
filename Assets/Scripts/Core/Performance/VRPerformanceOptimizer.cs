using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.Core.Performance
{
    /// <summary>
    /// VR性能优化器 - 专门针对VR应用的性能优化
    /// </summary>
    public class VRPerformanceOptimizer : MonoBehaviour
    {
        [Header("VR优化设置")]
        [SerializeField] private bool enableDynamicResolution = true;
        [SerializeField] private bool enableFoveatedRendering = true;
        [SerializeField] private bool enableAdaptiveQuality = true;
        [SerializeField] private bool enableOcclusionCulling = true;
        
        [Header("分辨率设置")]
        [SerializeField] private float minResolutionScale = 0.5f;
        [SerializeField] private float maxResolutionScale = 1.0f;
        [SerializeField] private float resolutionAdjustStep = 0.1f;
        [SerializeField] private float targetFrameTime = 13.89f; // 72fps
        
        [Header("质量设置")]
        [SerializeField] private int minQualityLevel = 0;
        [SerializeField] private int maxQualityLevel = 5;
        [SerializeField] private float qualityAdjustThreshold = 2.0f; // ms
        
        [Header("注视点渲染")]
        [SerializeField] private bool enableEyeTracking = false;
        [SerializeField] private float foveationStrength = 1.0f;
        [SerializeField] private float peripheralQuality = 0.5f;
        
        // VR性能数据
        private float currentFrameTime;
        private int droppedFrames;
        private float reprojectionRatio;
        private float currentResolutionScale = 1.0f;
        private int currentQualityLevel;
        
        // 优化状态
        private bool isOptimizing = false;
        private Coroutine optimizationCoroutine;
        
        // VR组件引用
        private Camera vrCamera;
        private UniversalRenderPipelineAsset urpAsset;
        private XRDisplaySubsystem xrDisplay;
        
        public float GetCurrentFrameTime() => currentFrameTime;
        public int GetDroppedFrames() => droppedFrames;
        public float GetReprojectionRatio() => reprojectionRatio;
        public float GetCurrentResolutionScale() => currentResolutionScale;
        
        private void Awake()
        {
            InitializeVRComponents();
            currentQualityLevel = QualitySettings.GetQualityLevel();
        }
        
        private void Start()
        {
            if (enableAdaptiveQuality)
            {
                StartOptimization();
            }
        }
        
        /// <summary>
        /// 初始化VR组件
        /// </summary>
        private void InitializeVRComponents()
        {
            try
            {
                // 获取VR摄像机
                vrCamera = Camera.main;
                if (vrCamera == null)
                {
                    vrCamera = FindObjectOfType<Camera>();
                }
                
                // 获取URP资产
                urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                
                // 获取XR显示子系统
                var xrDisplaySubsystems = new System.Collections.Generic.List<XRDisplaySubsystem>();
                SubsystemManager.GetInstances(xrDisplaySubsystems);
                if (xrDisplaySubsystems.Count > 0)
                {
                    xrDisplay = xrDisplaySubsystems[0];
                }
                
                Logger.LogInfo("VR性能优化器初始化完成", "VROptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "VR性能优化器初始化失败");
            }
        }
        
        /// <summary>
        /// 开始优化
        /// </summary>
        public void StartOptimization()
        {
            if (!isOptimizing)
            {
                isOptimizing = true;
                optimizationCoroutine = StartCoroutine(OptimizationLoop());
                Logger.LogInfo("VR性能优化已启动", "VROptimizer");
            }
        }
        
        /// <summary>
        /// 停止优化
        /// </summary>
        public void StopOptimization()
        {
            if (isOptimizing)
            {
                isOptimizing = false;
                if (optimizationCoroutine != null)
                {
                    StopCoroutine(optimizationCoroutine);
                    optimizationCoroutine = null;
                }
                Logger.LogInfo("VR性能优化已停止", "VROptimizer");
            }
        }
        
        /// <summary>
        /// 优化循环
        /// </summary>
        private IEnumerator OptimizationLoop()
        {
            while (isOptimizing)
            {
                try
                {
                    UpdateVRMetrics();
                    
                    if (enableAdaptiveQuality)
                    {
                        AdaptiveQualityAdjustment();
                    }
                    
                    if (enableDynamicResolution)
                    {
                        DynamicResolutionAdjustment();
                    }
                    
                    if (enableFoveatedRendering)
                    {
                        UpdateFoveatedRendering();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "VR优化循环错误");
                }
                
                yield return new WaitForSeconds(0.5f); // 每0.5秒检查一次
            }
        }
        
        /// <summary>
        /// 更新VR性能指标
        /// </summary>
        private void UpdateVRMetrics()
        {
            // 获取当前帧时间
            currentFrameTime = Time.unscaledDeltaTime * 1000f;
            
            // 获取丢帧数据（简化实现）
            if (currentFrameTime > targetFrameTime * 1.5f)
            {
                droppedFrames++;
            }
            
            // 计算重投影率（简化实现）
            reprojectionRatio = Mathf.Clamp01((currentFrameTime - targetFrameTime) / targetFrameTime);
            
            // 重置丢帧计数器（每秒）
            if (Time.frameCount % (int)Application.targetFrameRate == 0)
            {
                droppedFrames = 0;
            }
        }
        
        /// <summary>
        /// 自适应质量调整
        /// </summary>
        private void AdaptiveQualityAdjustment()
        {
            if (currentFrameTime > targetFrameTime + qualityAdjustThreshold)
            {
                // 性能不足，降低质量
                if (currentQualityLevel > minQualityLevel)
                {
                    currentQualityLevel--;
                    QualitySettings.SetQualityLevel(currentQualityLevel, true);
                    Logger.LogInfo($"降低质量等级到: {currentQualityLevel}", "VROptimizer");
                }
            }
            else if (currentFrameTime < targetFrameTime - qualityAdjustThreshold)
            {
                // 性能充足，提高质量
                if (currentQualityLevel < maxQualityLevel)
                {
                    currentQualityLevel++;
                    QualitySettings.SetQualityLevel(currentQualityLevel, true);
                    Logger.LogInfo($"提高质量等级到: {currentQualityLevel}", "VROptimizer");
                }
            }
        }
        
        /// <summary>
        /// 动态分辨率调整
        /// </summary>
        private void DynamicResolutionAdjustment()
        {
            if (xrDisplay == null) return;
            
            float targetScale = currentResolutionScale;
            
            if (currentFrameTime > targetFrameTime * 1.2f)
            {
                // 性能不足，降低分辨率
                targetScale = Mathf.Max(minResolutionScale, currentResolutionScale - resolutionAdjustStep);
            }
            else if (currentFrameTime < targetFrameTime * 0.8f)
            {
                // 性能充足，提高分辨率
                targetScale = Mathf.Min(maxResolutionScale, currentResolutionScale + resolutionAdjustStep);
            }
            
            if (Mathf.Abs(targetScale - currentResolutionScale) > 0.01f)
            {
                currentResolutionScale = targetScale;
                XRSettings.eyeTextureResolutionScale = currentResolutionScale;
                Logger.LogInfo($"调整VR分辨率缩放到: {currentResolutionScale:F2}", "VROptimizer");
            }
        }
        
        /// <summary>
        /// 更新注视点渲染
        /// </summary>
        private void UpdateFoveatedRendering()
        {
            if (!enableEyeTracking) return;
            
            try
            {
                // 简化的注视点渲染实现
                // 实际应该使用眼动追踪数据来调整渲染质量
                
                if (urpAsset != null)
                {
                    // 根据性能调整外围渲染质量
                    float qualityMultiplier = currentFrameTime > targetFrameTime ? peripheralQuality : 1.0f;
                    
                    // 这里应该设置URP的注视点渲染参数
                    // urpAsset.foveatedRenderingMode = FoveatedRenderingMode.Enabled;
                    // urpAsset.foveatedRenderingStrength = foveationStrength * qualityMultiplier;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "注视点渲染更新失败");
            }
        }
        
        /// <summary>
        /// 应用自动优化
        /// </summary>
        public void ApplyAutoOptimizations(PerformanceMetrics metrics)
        {
            try
            {
                Logger.LogInfo("应用VR自动优化...", "VROptimizer");
                
                // 根据性能指标应用优化
                if (metrics.currentFPS < 60f)
                {
                    // 激进优化
                    ApplyAggressiveOptimizations();
                }
                else if (metrics.currentFPS < 72f)
                {
                    // 温和优化
                    ApplyModerateOptimizations();
                }
                
                // 内存优化
                if (metrics.totalMemory > 800 * 1024 * 1024) // 800MB
                {
                    ApplyMemoryOptimizations();
                }
                
                Logger.LogInfo("VR自动优化完成", "VROptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "VR自动优化失败");
            }
        }
        
        /// <summary>
        /// 应用激进优化
        /// </summary>
        private void ApplyAggressiveOptimizations()
        {
            // 大幅降低分辨率
            currentResolutionScale = Mathf.Max(minResolutionScale, 0.6f);
            XRSettings.eyeTextureResolutionScale = currentResolutionScale;
            
            // 降低质量等级
            currentQualityLevel = Mathf.Max(minQualityLevel, currentQualityLevel - 2);
            QualitySettings.SetQualityLevel(currentQualityLevel, true);
            
            // 启用更强的注视点渲染
            if (enableFoveatedRendering)
            {
                foveationStrength = 2.0f;
                peripheralQuality = 0.3f;
            }
            
            // 禁用一些视觉效果
            DisableExpensiveEffects();
            
            Logger.LogInfo("已应用激进VR优化", "VROptimizer");
        }
        
        /// <summary>
        /// 应用温和优化
        /// </summary>
        private void ApplyModerateOptimizations()
        {
            // 适度降低分辨率
            currentResolutionScale = Mathf.Max(minResolutionScale, 0.8f);
            XRSettings.eyeTextureResolutionScale = currentResolutionScale;
            
            // 降低质量等级
            if (currentQualityLevel > minQualityLevel)
            {
                currentQualityLevel--;
                QualitySettings.SetQualityLevel(currentQualityLevel, true);
            }
            
            // 调整注视点渲染
            if (enableFoveatedRendering)
            {
                foveationStrength = 1.5f;
                peripheralQuality = 0.5f;
            }
            
            Logger.LogInfo("已应用温和VR优化", "VROptimizer");
        }
        
        /// <summary>
        /// 应用内存优化
        /// </summary>
        private void ApplyMemoryOptimizations()
        {
            // 强制垃圾回收
            System.GC.Collect();
            
            // 卸载未使用的资源
            Resources.UnloadUnusedAssets();
            
            // 降低纹理质量
            QualitySettings.masterTextureLimit = 1;
            
            Logger.LogInfo("已应用VR内存优化", "VROptimizer");
        }
        
        /// <summary>
        /// 禁用昂贵的视觉效果
        /// </summary>
        private void DisableExpensiveEffects()
        {
            // 禁用阴影
            QualitySettings.shadows = ShadowQuality.Disable;
            
            // 降低抗锯齿
            QualitySettings.antiAliasing = 0;
            
            // 禁用实时反射
            QualitySettings.realtimeReflectionProbes = false;
            
            // 降低粒子系统质量
            QualitySettings.particleRaycastBudget = 16;
            
            Logger.LogInfo("已禁用昂贵的视觉效果", "VROptimizer");
        }
        
        /// <summary>
        /// 恢复默认设置
        /// </summary>
        public void RestoreDefaultSettings()
        {
            try
            {
                currentResolutionScale = 1.0f;
                XRSettings.eyeTextureResolutionScale = currentResolutionScale;
                
                QualitySettings.SetQualityLevel(maxQualityLevel, true);
                currentQualityLevel = maxQualityLevel;
                
                foveationStrength = 1.0f;
                peripheralQuality = 0.5f;
                
                // 恢复视觉效果
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.antiAliasing = 4;
                QualitySettings.realtimeReflectionProbes = true;
                QualitySettings.masterTextureLimit = 0;
                
                Logger.LogInfo("已恢复VR默认设置", "VROptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "恢复VR默认设置失败");
            }
        }
        
        /// <summary>
        /// 获取优化建议
        /// </summary>
        public OptimizationSuggestion[] GetOptimizationSuggestions(PerformanceMetrics metrics)
        {
            var suggestions = new System.Collections.Generic.List<OptimizationSuggestion>();
            
            if (metrics.currentFPS < targetFrameTime)
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    type = OptimizationType.VRResolution,
                    priority = OptimizationPriority.High,
                    description = "降低VR渲染分辨率以提高帧率",
                    expectedImprovement = "提高10-20%性能",
                    action = () => ApplyModerateOptimizations()
                });
            }
            
            if (metrics.vrReprojectionRatio > 0.1f)
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    type = OptimizationType.FoveatedRendering,
                    priority = OptimizationPriority.Medium,
                    description = "启用或增强注视点渲染",
                    expectedImprovement = "减少15-25%GPU负载",
                    action = () => { enableFoveatedRendering = true; foveationStrength = 2.0f; }
                });
            }
            
            return suggestions.ToArray();
        }
        
        private void OnDestroy()
        {
            StopOptimization();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                StopOptimization();
            }
            else if (enableAdaptiveQuality)
            {
                StartOptimization();
            }
        }
    }
}