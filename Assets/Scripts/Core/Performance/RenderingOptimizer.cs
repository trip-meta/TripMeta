using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.Core.Performance
{
    /// <summary>
    /// 渲染优化器 - 优化渲染性能和质量
    /// </summary>
    public class RenderingOptimizer : MonoBehaviour
    {
        [Header("渲染优化设置")]
        [SerializeField] private bool enableAutoOptimization = true;
        [SerializeField] private bool enableLODOptimization = true;
        [SerializeField] private bool enableCullingOptimization = true;
        [SerializeField] private bool enableBatchingOptimization = true;
        
        [Header("质量设置")]
        [SerializeField] private int minQualityLevel = 0;
        [SerializeField] private int maxQualityLevel = 5;
        [SerializeField] private float qualityAdjustThreshold = 50f; // FPS阈值
        
        [Header("LOD设置")]
        [SerializeField] private float lodBias = 1.0f;
        [SerializeField] private int maxLODLevel = 2;
        [SerializeField] private float lodDistanceMultiplier = 1.0f;
        
        [Header("剔除设置")]
        [SerializeField] private float cullingDistance = 100f;
        [SerializeField] private bool enableFrustumCulling = true;
        [SerializeField] private bool enableOcclusionCulling = true;
        
        [Header("阴影设置")]
        [SerializeField] private ShadowQuality shadowQuality = ShadowQuality.All;
        [SerializeField] private ShadowResolution shadowResolution = ShadowResolution.Medium;
        [SerializeField] private float shadowDistance = 50f;
        [SerializeField] private int shadowCascades = 2;
        
        // 渲染组件引用
        private UniversalRenderPipelineAsset urpAsset;
        private Camera mainCamera;
        private Light mainLight;
        
        // 优化状态
        private int currentQualityLevel;
        private Dictionary<Renderer, LODGroup> lodGroups = new Dictionary<Renderer, LODGroup>();
        private List<Renderer> optimizedRenderers = new List<Renderer>();
        
        // 性能统计
        private int lastDrawCalls;
        private int lastTriangles;
        private float lastFrameTime;
        
        private void Awake()
        {
            InitializeRenderingComponents();
            currentQualityLevel = QualitySettings.GetQualityLevel();
        }
        
        private void Start()
        {
            if (enableAutoOptimization)
            {
                AnalyzeScene();
                SetupOptimizations();
            }
        }
        
        /// <summary>
        /// 初始化渲染组件
        /// </summary>
        private void InitializeRenderingComponents()
        {
            try
            {
                // 获取URP资产
                urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                
                // 获取主摄像机
                mainCamera = Camera.main ?? FindObjectOfType<Camera>();
                
                // 获取主光源
                mainLight = FindObjectOfType<Light>();
                
                Logger.LogInfo("渲染优化器初始化完成", "RenderingOptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "渲染优化器初始化失败");
            }
        }
        
        /// <summary>
        /// 分析场景
        /// </summary>
        private void AnalyzeScene()
        {
            try
            {
                // 收集所有渲染器
                var renderers = FindObjectsOfType<Renderer>();
                optimizedRenderers.Clear();
                
                foreach (var renderer in renderers)
                {
                    if (renderer.gameObject.activeInHierarchy)
                    {
                        optimizedRenderers.Add(renderer);
                        
                        // 检查LOD组
                        var lodGroup = renderer.GetComponent<LODGroup>();
                        if (lodGroup != null)
                        {
                            lodGroups[renderer] = lodGroup;
                        }
                    }
                }
                
                Logger.LogInfo($"场景分析完成，找到 {optimizedRenderers.Count} 个渲染器", "RenderingOptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "场景分析失败");
            }
        }
        
        /// <summary>
        /// 设置优化
        /// </summary>
        private void SetupOptimizations()
        {
            try
            {
                if (enableLODOptimization)
                {
                    SetupLODOptimization();
                }
                
                if (enableCullingOptimization)
                {
                    SetupCullingOptimization();
                }
                
                if (enableBatchingOptimization)
                {
                    SetupBatchingOptimization();
                }
                
                Logger.LogInfo("渲染优化设置完成", "RenderingOptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "渲染优化设置失败");
            }
        }
        
        /// <summary>
        /// 设置LOD优化
        /// </summary>
        private void SetupLODOptimization()
        {
            QualitySettings.lodBias = lodBias;
            QualitySettings.maximumLODLevel = maxLODLevel;
            
            // 为没有LOD的对象创建简单LOD
            foreach (var renderer in optimizedRenderers)
            {
                if (!lodGroups.ContainsKey(renderer) && renderer.GetComponent<MeshRenderer>())
                {
                    CreateSimpleLOD(renderer);
                }
            }
            
            Logger.LogInfo("LOD优化设置完成", "RenderingOptimizer");
        }
        
        /// <summary>
        /// 创建简单LOD
        /// </summary>
        private void CreateSimpleLOD(Renderer renderer)
        {
            try
            {
                var lodGroup = renderer.gameObject.AddComponent<LODGroup>();
                var lods = new LOD[3];
                
                // LOD 0 - 原始质量
                lods[0] = new LOD(0.6f, new Renderer[] { renderer });
                
                // LOD 1 - 中等质量（可以创建简化版本）
                lods[1] = new LOD(0.3f, new Renderer[] { renderer });
                
                // LOD 2 - 低质量（可以创建更简化版本或使用Impostor）
                lods[2] = new LOD(0.1f, new Renderer[] { renderer });
                
                lodGroup.SetLODs(lods);
                lodGroups[renderer] = lodGroup;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"创建LOD失败: {renderer.name}");
            }
        }
        
        /// <summary>
        /// 设置剔除优化
        /// </summary>
        private void SetupCullingOptimization()
        {
            if (mainCamera != null)
            {
                // 设置摄像机剔除距离
                mainCamera.farClipPlane = Mathf.Min(mainCamera.farClipPlane, cullingDistance);
                
                // 启用遮挡剔除
                if (enableOcclusionCulling)
                {
                    mainCamera.useOcclusionCulling = true;
                }
                
                // 设置剔除遮罩（可以根据需要调整）
                // mainCamera.cullingMask = LayerMask.GetMask("Default", "UI");
            }
            
            Logger.LogInfo("剔除优化设置完成", "RenderingOptimizer");
        }
        
        /// <summary>
        /// 设置批处理优化
        /// </summary>
        private void SetupBatchingOptimization()
        {
            // 启用静态批处理
            StaticBatchingUtility.Combine(FindObjectsOfType<GameObject>());
            
            // 启用动态批处理（在Player Settings中设置）
            // 这里可以添加GPU Instancing的设置
            
            Logger.LogInfo("批处理优化设置完成", "RenderingOptimizer");
        }
        
        /// <summary>
        /// 应用自动优化
        /// </summary>
        public void ApplyAutoOptimizations(PerformanceMetrics metrics)
        {
            try
            {
                Logger.LogInfo("应用渲染自动优化...", "RenderingOptimizer");
                
                // 根据帧率调整质量
                if (metrics.currentFPS < qualityAdjustThreshold)
                {
                    ApplyPerformanceOptimizations(metrics);
                }
                else if (metrics.currentFPS > qualityAdjustThreshold * 1.2f)
                {
                    ApplyQualityImprovements(metrics);
                }
                
                // 根据Draw Calls优化
                if (metrics.drawCalls > 1000)
                {
                    OptimizeDrawCalls();
                }
                
                // 根据内存使用优化
                if (metrics.totalMemory > 800 * 1024 * 1024) // 800MB
                {
                    OptimizeMemoryUsage();
                }
                
                Logger.LogInfo("渲染自动优化完成", "RenderingOptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "渲染自动优化失败");
            }
        }
        
        /// <summary>
        /// 应用性能优化
        /// </summary>
        private void ApplyPerformanceOptimizations(PerformanceMetrics metrics)
        {
            // 降低质量等级
            if (currentQualityLevel > minQualityLevel)
            {
                currentQualityLevel--;
                QualitySettings.SetQualityLevel(currentQualityLevel, true);
            }
            
            // 降低阴影质量
            if (shadowQuality > ShadowQuality.Disable)
            {
                shadowQuality = (ShadowQuality)((int)shadowQuality - 1);
                QualitySettings.shadows = shadowQuality;
            }
            
            // 减少阴影距离
            shadowDistance = Mathf.Max(20f, shadowDistance * 0.8f);
            QualitySettings.shadowDistance = shadowDistance;
            
            // 调整LOD偏移
            lodBias = Mathf.Max(0.5f, lodBias * 0.9f);
            QualitySettings.lodBias = lodBias;
            
            // 降低纹理质量
            QualitySettings.masterTextureLimit = Mathf.Min(2, QualitySettings.masterTextureLimit + 1);
            
            Logger.LogInfo("已应用性能优化", "RenderingOptimizer");
        }
        
        /// <summary>
        /// 应用质量改进
        /// </summary>
        private void ApplyQualityImprovements(PerformanceMetrics metrics)
        {
            // 提高质量等级
            if (currentQualityLevel < maxQualityLevel)
            {
                currentQualityLevel++;
                QualitySettings.SetQualityLevel(currentQualityLevel, true);
            }
            
            // 提高阴影质量
            if (shadowQuality < ShadowQuality.All)
            {
                shadowQuality = (ShadowQuality)((int)shadowQuality + 1);
                QualitySettings.shadows = shadowQuality;
            }
            
            // 增加阴影距离
            shadowDistance = Mathf.Min(100f, shadowDistance * 1.1f);
            QualitySettings.shadowDistance = shadowDistance;
            
            // 调整LOD偏移
            lodBias = Mathf.Min(2.0f, lodBias * 1.1f);
            QualitySettings.lodBias = lodBias;
            
            // 提高纹理质量
            QualitySettings.masterTextureLimit = Mathf.Max(0, QualitySettings.masterTextureLimit - 1);
            
            Logger.LogInfo("已应用质量改进", "RenderingOptimizer");
        }
        
        /// <summary>
        /// 优化Draw Calls
        /// </summary>
        private void OptimizeDrawCalls()
        {
            // 启用GPU Instancing
            EnableGPUInstancing();
            
            // 合并材质
            CombineMaterials();
            
            // 启用SRP Batcher
            if (urpAsset != null)
            {
                // urpAsset.useSRPBatcher = true; // 需要URP版本支持
            }
            
            Logger.LogInfo("Draw Calls优化完成", "RenderingOptimizer");
        }
        
        /// <summary>
        /// 启用GPU实例化
        /// </summary>
        private void EnableGPUInstancing()
        {
            foreach (var renderer in optimizedRenderers)
            {
                if (renderer is MeshRenderer meshRenderer)
                {
                    var materials = meshRenderer.materials;
                    foreach (var material in materials)
                    {
                        if (material != null && material.shader != null)
                        {
                            // 检查着色器是否支持GPU Instancing
                            if (material.shader.name.Contains("Standard") || 
                                material.shader.name.Contains("Universal"))
                            {
                                material.enableInstancing = true;
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 合并材质
        /// </summary>
        private void CombineMaterials()
        {
            // 简化实现 - 实际应该根据纹理和着色器进行智能合并
            var materialGroups = new Dictionary<string, List<Renderer>>();
            
            foreach (var renderer in optimizedRenderers)
            {
                if (renderer.material != null)
                {
                    var key = renderer.material.shader.name;
                    if (!materialGroups.ContainsKey(key))
                    {
                        materialGroups[key] = new List<Renderer>();
                    }
                    materialGroups[key].Add(renderer);
                }
            }
            
            // 对于相同着色器的渲染器，尝试使用相同材质
            foreach (var group in materialGroups.Values)
            {
                if (group.Count > 1)
                {
                    var sharedMaterial = group[0].material;
                    for (int i = 1; i < group.Count; i++)
                    {
                        if (group[i].material.mainTexture == sharedMaterial.mainTexture)
                        {
                            group[i].material = sharedMaterial;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 优化内存使用
        /// </summary>
        private void OptimizeMemoryUsage()
        {
            // 降低纹理质量
            QualitySettings.masterTextureLimit = 2;
            
            // 压缩纹理
            CompressTextures();
            
            // 卸载未使用的资源
            Resources.UnloadUnusedAssets();
            
            // 强制垃圾回收
            System.GC.Collect();
            
            Logger.LogInfo("内存使用优化完成", "RenderingOptimizer");
        }
        
        /// <summary>
        /// 压缩纹理
        /// </summary>
        private void CompressTextures()
        {
            var textures = FindObjectsOfType<Texture2D>();
            foreach (var texture in textures)
            {
                // 这里应该在编辑器中设置纹理压缩格式
                // 运行时无法直接修改纹理压缩格式
            }
        }
        
        /// <summary>
        /// 获取渲染统计
        /// </summary>
        public RenderingStats GetRenderingStats()
        {
            return new RenderingStats
            {
                drawCalls = lastDrawCalls,
                triangles = lastTriangles,
                frameTime = lastFrameTime,
                qualityLevel = currentQualityLevel,
                shadowQuality = shadowQuality,
                lodBias = lodBias,
                textureLimit = QualitySettings.masterTextureLimit,
                rendererCount = optimizedRenderers.Count,
                lodGroupCount = lodGroups.Count
            };
        }
        
        /// <summary>
        /// 重置优化设置
        /// </summary>
        public void ResetOptimizations()
        {
            try
            {
                // 恢复默认质量设置
                QualitySettings.SetQualityLevel(maxQualityLevel, true);
                currentQualityLevel = maxQualityLevel;
                
                // 恢复阴影设置
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowDistance = 100f;
                
                // 恢复LOD设置
                QualitySettings.lodBias = 1.0f;
                QualitySettings.maximumLODLevel = 0;
                
                // 恢复纹理设置
                QualitySettings.masterTextureLimit = 0;
                
                Logger.LogInfo("渲染优化设置已重置", "RenderingOptimizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "重置渲染优化设置失败");
            }
        }
        
        private void Update()
        {
            // 更新渲染统计
            lastFrameTime = Time.unscaledDeltaTime * 1000f;
            // lastDrawCalls 和 lastTriangles 需要通过Profiler API获取
        }
    }
    
    /// <summary>
    /// 渲染统计数据
    /// </summary>
    [Serializable]
    public class RenderingStats
    {
        public int drawCalls;
        public int triangles;
        public float frameTime;
        public int qualityLevel;
        public ShadowQuality shadowQuality;
        public float lodBias;
        public int textureLimit;
        public int rendererCount;
        public int lodGroupCount;
    }
}