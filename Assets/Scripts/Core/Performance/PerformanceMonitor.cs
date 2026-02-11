using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using TripMeta.Core.ErrorHandling;
using TripMeta.Core.DependencyInjection;

namespace TripMeta.Core.Performance
{
    /// <summary>
    /// 性能监控器 - 实时监控Unity和VR性能
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("监控设置")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private float updateInterval = 1.0f;
        [SerializeField] private int maxSampleCount = 60;
        [SerializeField] private bool enableAutoOptimization = true;
        
        [Header("性能阈值")]
        [SerializeField] private float targetFrameRate = 72f;
        [SerializeField] private float criticalFrameRate = 45f;
        [SerializeField] private long maxMemoryUsage = 1024 * 1024 * 1024; // 1GB
        [SerializeField] private int maxDrawCalls = 1000;
        [SerializeField] private int maxTriangles = 100000;
        
        [Header("VR性能设置")]
        [SerializeField] private bool enableVROptimization = true;
        [SerializeField] private float vrTargetFrameTime = 13.89f; // 72fps = 13.89ms
        [SerializeField] private bool enableDynamicResolution = true;
        [SerializeField] private bool enableFoveatedRendering = true;
        
        // 性能数据
        private readonly Queue<float> frameTimeHistory = new Queue<float>();
        private readonly Queue<long> memoryHistory = new Queue<long>();
        private readonly Queue<int> drawCallHistory = new Queue<int>();
        private readonly Queue<int> triangleHistory = new Queue<int>();
        
        // 当前性能指标
        private PerformanceMetrics currentMetrics = new PerformanceMetrics();
        private PerformanceStatus currentStatus = PerformanceStatus.Good;
        
        // 优化器
        private VRPerformanceOptimizer vrOptimizer;
        private RenderingOptimizer renderingOptimizer;
        private MemoryOptimizer memoryOptimizer;
        
        // 事件
        public event Action<PerformanceMetrics> OnMetricsUpdated;
        public event Action<PerformanceStatus> OnStatusChanged;
        public event Action<PerformanceAlert> OnPerformanceAlert;
        public event Action<OptimizationSuggestion> OnOptimizationSuggestion;
        
        public PerformanceMetrics CurrentMetrics => currentMetrics;
        public PerformanceStatus CurrentStatus => currentStatus;
        public bool IsMonitoring => enableMonitoring;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeOptimizers();
        }
        
        private void Start()
        {
            if (enableMonitoring)
            {
                StartMonitoring();
            }
        }
        
        /// <summary>
        /// 初始化优化器
        /// </summary>
        private void InitializeOptimizers()
        {
            try
            {
                // 创建VR优化器
                if (enableVROptimization)
                {
                    vrOptimizer = GetComponent<VRPerformanceOptimizer>() ?? 
                                 gameObject.AddComponent<VRPerformanceOptimizer>();
                }
                
                // 创建渲染优化器
                renderingOptimizer = GetComponent<RenderingOptimizer>() ?? 
                                   gameObject.AddComponent<RenderingOptimizer>();
                
                // 创建内存优化器
                memoryOptimizer = GetComponent<MemoryOptimizer>() ?? 
                                gameObject.AddComponent<MemoryOptimizer>();
                
                Logger.LogInfo("性能优化器初始化完成", "Performance");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "性能优化器初始化失败");
            }
        }
        
        /// <summary>
        /// 开始性能监控
        /// </summary>
        public void StartMonitoring()
        {
            if (enableMonitoring)
            {
                StartCoroutine(MonitoringCoroutine());
                Logger.LogInfo("性能监控已启动", "Performance");
            }
        }
        
        /// <summary>
        /// 停止性能监控
        /// </summary>
        public void StopMonitoring()
        {
            enableMonitoring = false;
            StopAllCoroutines();
            Logger.LogInfo("性能监控已停止", "Performance");
        }
        
        /// <summary>
        /// 监控协程
        /// </summary>
        private IEnumerator MonitoringCoroutine()
        {
            while (enableMonitoring)
            {
                try
                {
                    UpdatePerformanceMetrics();
                    AnalyzePerformance();
                    
                    if (enableAutoOptimization)
                    {
                        ApplyAutoOptimizations();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "性能监控更新失败");
                }
                
                yield return new WaitForSeconds(updateInterval);
            }
        }
        
        /// <summary>
        /// 更新性能指标
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            // 帧率和帧时间
            float frameTime = Time.unscaledDeltaTime * 1000f; // 转换为毫秒
            float fps = 1f / Time.unscaledDeltaTime;
            
            AddToHistory(frameTimeHistory, frameTime);
            
            // 内存使用
            long totalMemory = Profiler.GetTotalAllocatedMemory(Profiler.Area.All);
            long usedMemory = Profiler.GetTotalReservedMemory(Profiler.Area.All);
            long gcMemory = GC.GetTotalMemory(false);
            
            AddToHistory(memoryHistory, totalMemory);
            
            // 渲染统计
            int drawCalls = UnityEngine.Rendering.FrameDebugger.enabled ? 
                           UnityEngine.Rendering.FrameDebugger.count : 0;
            int triangles = 0; // 需要通过Profiler API获取
            
            AddToHistory(drawCallHistory, drawCalls);
            AddToHistory(triangleHistory, triangles);
            
            // 更新当前指标
            currentMetrics = new PerformanceMetrics
            {
                // 帧率指标
                currentFPS = fps,
                averageFPS = CalculateAverage(frameTimeHistory, (ft) => 1000f / ft),
                frameTime = frameTime,
                averageFrameTime = CalculateAverage(frameTimeHistory),
                
                // 内存指标
                totalMemory = totalMemory,
                usedMemory = usedMemory,
                gcMemory = gcMemory,
                averageMemoryUsage = CalculateAverage(memoryHistory),
                
                // 渲染指标
                drawCalls = drawCalls,
                triangles = triangles,
                averageDrawCalls = (int)CalculateAverage(drawCallHistory, (dc) => dc),
                averageTriangles = (int)CalculateAverage(triangleHistory, (t) => t),
                
                // VR指标
                vrFrameTime = vrOptimizer?.GetCurrentFrameTime() ?? frameTime,
                vrDroppedFrames = vrOptimizer?.GetDroppedFrames() ?? 0,
                vrReprojectionRatio = vrOptimizer?.GetReprojectionRatio() ?? 0f,
                
                // 系统指标
                cpuUsage = GetCPUUsage(),
                gpuUsage = GetGPUUsage(),
                temperature = GetDeviceTemperature(),
                batteryLevel = GetBatteryLevel(),
                
                timestamp = DateTime.Now
            };
            
            OnMetricsUpdated?.Invoke(currentMetrics);
        }
        
        /// <summary>
        /// 分析性能状态
        /// </summary>
        private void AnalyzePerformance()
        {
            var previousStatus = currentStatus;
            
            // 评估整体性能状态
            var frameRateScore = EvaluateFrameRate();
            var memoryScore = EvaluateMemoryUsage();
            var renderingScore = EvaluateRenderingPerformance();
            var vrScore = EvaluateVRPerformance();
            
            var overallScore = (frameRateScore + memoryScore + renderingScore + vrScore) / 4f;
            
            if (overallScore >= 0.8f)
                currentStatus = PerformanceStatus.Excellent;
            else if (overallScore >= 0.6f)
                currentStatus = PerformanceStatus.Good;
            else if (overallScore >= 0.4f)
                currentStatus = PerformanceStatus.Fair;
            else if (overallScore >= 0.2f)
                currentStatus = PerformanceStatus.Poor;
            else
                currentStatus = PerformanceStatus.Critical;
            
            // 状态变化通知
            if (currentStatus != previousStatus)
            {
                OnStatusChanged?.Invoke(currentStatus);
                Logger.LogInfo($"性能状态变化: {previousStatus} -> {currentStatus}", "Performance");
            }
            
            // 生成性能警报
            GeneratePerformanceAlerts();
        }
        
        /// <summary>
        /// 评估帧率性能
        /// </summary>
        private float EvaluateFrameRate()
        {
            var avgFPS = currentMetrics.averageFPS;
            
            if (avgFPS >= targetFrameRate)
                return 1.0f;
            else if (avgFPS >= criticalFrameRate)
                return (avgFPS - criticalFrameRate) / (targetFrameRate - criticalFrameRate);
            else
                return 0.0f;
        }
        
        /// <summary>
        /// 评估内存使用
        /// </summary>
        private float EvaluateMemoryUsage()
        {
            var memoryRatio = (float)currentMetrics.totalMemory / maxMemoryUsage;
            
            if (memoryRatio <= 0.6f)
                return 1.0f;
            else if (memoryRatio <= 0.8f)
                return 1.0f - (memoryRatio - 0.6f) / 0.2f * 0.5f;
            else if (memoryRatio <= 1.0f)
                return 0.5f - (memoryRatio - 0.8f) / 0.2f * 0.5f;
            else
                return 0.0f;
        }
        
        /// <summary>
        /// 评估渲染性能
        /// </summary>
        private float EvaluateRenderingPerformance()
        {
            var drawCallRatio = (float)currentMetrics.drawCalls / maxDrawCalls;
            var triangleRatio = (float)currentMetrics.triangles / maxTriangles;
            
            var drawCallScore = drawCallRatio <= 0.8f ? 1.0f : 1.0f - (drawCallRatio - 0.8f) / 0.2f;
            var triangleScore = triangleRatio <= 0.8f ? 1.0f : 1.0f - (triangleRatio - 0.8f) / 0.2f;
            
            return (drawCallScore + triangleScore) / 2f;
        }
        
        /// <summary>
        /// 评估VR性能
        /// </summary>
        private float EvaluateVRPerformance()
        {
            if (!enableVROptimization || vrOptimizer == null)
                return 1.0f;
            
            var frameTimeRatio = currentMetrics.vrFrameTime / vrTargetFrameTime;
            var reprojectionPenalty = currentMetrics.vrReprojectionRatio * 0.5f;
            
            var score = frameTimeRatio <= 1.0f ? 1.0f : 1.0f / frameTimeRatio;
            score -= reprojectionPenalty;
            
            return Mathf.Clamp01(score);
        }
        
        /// <summary>
        /// 生成性能警报
        /// </summary>
        private void GeneratePerformanceAlerts()
        {
            // 帧率警报
            if (currentMetrics.currentFPS < criticalFrameRate)
            {
                var alert = new PerformanceAlert
                {
                    type = AlertType.FrameRate,
                    severity = AlertSeverity.Critical,
                    message = $"帧率过低: {currentMetrics.currentFPS:F1} FPS (目标: {targetFrameRate} FPS)",
                    timestamp = DateTime.Now,
                    suggestedActions = new[] { "降低渲染质量", "减少Draw Calls", "启用动态分辨率" }
                };
                OnPerformanceAlert?.Invoke(alert);
            }
            
            // 内存警报
            var memoryRatio = (float)currentMetrics.totalMemory / maxMemoryUsage;
            if (memoryRatio > 0.9f)
            {
                var alert = new PerformanceAlert
                {
                    type = AlertType.Memory,
                    severity = AlertSeverity.High,
                    message = $"内存使用过高: {memoryRatio:P1} (限制: {maxMemoryUsage / (1024 * 1024)} MB)",
                    timestamp = DateTime.Now,
                    suggestedActions = new[] { "执行垃圾回收", "卸载未使用资源", "优化纹理压缩" }
                };
                OnPerformanceAlert?.Invoke(alert);
            }
            
            // VR性能警报
            if (enableVROptimization && currentMetrics.vrReprojectionRatio > 0.1f)
            {
                var alert = new PerformanceAlert
                {
                    type = AlertType.VRPerformance,
                    severity = AlertSeverity.Medium,
                    message = $"VR重投影率过高: {currentMetrics.vrReprojectionRatio:P1}",
                    timestamp = DateTime.Now,
                    suggestedActions = new[] { "启用注视点渲染", "降低渲染分辨率", "优化着色器复杂度" }
                };
                OnPerformanceAlert?.Invoke(alert);
            }
        }
        
        /// <summary>
        /// 应用自动优化
        /// </summary>
        private void ApplyAutoOptimizations()
        {
            if (currentStatus == PerformanceStatus.Poor || currentStatus == PerformanceStatus.Critical)
            {
                // VR优化
                if (enableVROptimization && vrOptimizer != null)
                {
                    vrOptimizer.ApplyAutoOptimizations(currentMetrics);
                }
                
                // 渲染优化
                if (renderingOptimizer != null)
                {
                    renderingOptimizer.ApplyAutoOptimizations(currentMetrics);
                }
                
                // 内存优化
                if (memoryOptimizer != null)
                {
                    memoryOptimizer.ApplyAutoOptimizations(currentMetrics);
                }
                
                Logger.LogInfo("已应用自动性能优化", "Performance");
            }
        }
        
        /// <summary>
        /// 添加到历史记录
        /// </summary>
        private void AddToHistory<T>(Queue<T> history, T value)
        {
            history.Enqueue(value);
            while (history.Count > maxSampleCount)
            {
                history.Dequeue();
            }
        }
        
        /// <summary>
        /// 计算平均值
        /// </summary>
        private float CalculateAverage<T>(Queue<T> history, Func<T, float> selector = null)
        {
            if (history.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (var item in history)
            {
                sum += selector?.Invoke(item) ?? Convert.ToSingle(item);
            }
            
            return sum / history.Count;
        }
        
        /// <summary>
        /// 获取CPU使用率
        /// </summary>
        private float GetCPUUsage()
        {
            // 简化实现，实际需要平台特定的API
            return UnityEngine.Random.Range(20f, 80f);
        }
        
        /// <summary>
        /// 获取GPU使用率
        /// </summary>
        private float GetGPUUsage()
        {
            // 简化实现，实际需要平台特定的API
            return UnityEngine.Random.Range(30f, 90f);
        }
        
        /// <summary>
        /// 获取设备温度
        /// </summary>
        private float GetDeviceTemperature()
        {
            // 简化实现，实际需要平台特定的API
            return UnityEngine.Random.Range(35f, 65f);
        }
        
        /// <summary>
        /// 获取电池电量
        /// </summary>
        private float GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }
        
        /// <summary>
        /// 获取性能报告
        /// </summary>
        public PerformanceReport GenerateReport(TimeSpan duration)
        {
            return new PerformanceReport
            {
                startTime = DateTime.Now - duration,
                endTime = DateTime.Now,
                averageMetrics = currentMetrics,
                status = currentStatus,
                alerts = new List<PerformanceAlert>(),
                optimizations = new List<OptimizationSuggestion>()
            };
        }
        
        /// <summary>
        /// 重置性能数据
        /// </summary>
        public void ResetMetrics()
        {
            frameTimeHistory.Clear();
            memoryHistory.Clear();
            drawCallHistory.Clear();
            triangleHistory.Clear();
            
            Logger.LogInfo("性能数据已重置", "Performance");
        }
        
        private void OnDestroy()
        {
            StopMonitoring();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                StopMonitoring();
            }
            else if (enableMonitoring)
            {
                StartMonitoring();
            }
        }
    }
}