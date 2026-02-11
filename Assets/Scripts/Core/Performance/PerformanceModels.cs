using System;
using System.Collections.Generic;
using UnityEngine;

namespace TripMeta.Core.Performance
{
    /// <summary>
    /// 性能指标数据
    /// </summary>
    [Serializable]
    public class PerformanceMetrics
    {
        [Header("帧率指标")]
        public float currentFPS;
        public float averageFPS;
        public float frameTime; // 毫秒
        public float averageFrameTime;
        public float minFPS;
        public float maxFPS;
        
        [Header("内存指标")]
        public long totalMemory; // 字节
        public long usedMemory;
        public long gcMemory;
        public long averageMemoryUsage;
        public int gcCollections;
        
        [Header("渲染指标")]
        public int drawCalls;
        public int triangles;
        public int vertices;
        public int averageDrawCalls;
        public int averageTriangles;
        public int setPassCalls;
        public int batches;
        
        [Header("VR指标")]
        public float vrFrameTime;
        public int vrDroppedFrames;
        public float vrReprojectionRatio;
        public float vrResolutionScale;
        public bool vrFoveatedRenderingEnabled;
        
        [Header("系统指标")]
        public float cpuUsage; // 百分比
        public float gpuUsage; // 百分比
        public float temperature; // 摄氏度
        public float batteryLevel; // 百分比
        public long availableMemory;
        
        [Header("网络指标")]
        public float networkLatency; // 毫秒
        public float networkBandwidth; // Mbps
        public int networkPacketLoss; // 百分比
        
        [Header("AI服务指标")]
        public float aiResponseTime; // 毫秒
        public int aiRequestsPerSecond;
        public float aiErrorRate; // 百分比
        
        public DateTime timestamp;
        
        /// <summary>
        /// 计算性能评分
        /// </summary>
        public float CalculateOverallScore()
        {
            float frameRateScore = Mathf.Clamp01(currentFPS / 72f);
            float memoryScore = Mathf.Clamp01(1f - (float)totalMemory / (1024 * 1024 * 1024)); // 1GB基准
            float renderingScore = Mathf.Clamp01(1f - drawCalls / 1000f); // 1000 draw calls基准
            
            return (frameRateScore + memoryScore + renderingScore) / 3f;
        }
        
        /// <summary>
        /// 获取性能等级
        /// </summary>
        public PerformanceGrade GetPerformanceGrade()
        {
            float score = CalculateOverallScore();
            
            if (score >= 0.9f) return PerformanceGrade.Excellent;
            if (score >= 0.7f) return PerformanceGrade.Good;
            if (score >= 0.5f) return PerformanceGrade.Fair;
            if (score >= 0.3f) return PerformanceGrade.Poor;
            return PerformanceGrade.Critical;
        }
    }
    
    /// <summary>
    /// 性能状态枚举
    /// </summary>
    public enum PerformanceStatus
    {
        Excellent,  // 优秀
        Good,       // 良好
        Fair,       // 一般
        Poor,       // 较差
        Critical    // 严重
    }
    
    /// <summary>
    /// 性能等级枚举
    /// </summary>
    public enum PerformanceGrade
    {
        Excellent,  // S级
        Good,       // A级
        Fair,       // B级
        Poor,       // C级
        Critical    // D级
    }
    
    /// <summary>
    /// 性能警报
    /// </summary>
    [Serializable]
    public class PerformanceAlert
    {
        public AlertType type;
        public AlertSeverity severity;
        public string message;
        public DateTime timestamp;
        public string[] suggestedActions;
        public Dictionary<string, object> metadata;
        
        public PerformanceAlert()
        {
            metadata = new Dictionary<string, object>();
            timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// 警报类型
    /// </summary>
    public enum AlertType
    {
        FrameRate,          // 帧率
        Memory,             // 内存
        VRPerformance,      // VR性能
        Temperature,        // 温度
        Battery,            // 电池
        Network,            // 网络
        AIService,          // AI服务
        Rendering           // 渲染
    }
    
    /// <summary>
    /// 警报严重程度
    /// </summary>
    public enum AlertSeverity
    {
        Low,        // 低
        Medium,     // 中
        High,       // 高
        Critical    // 严重
    }
    
    /// <summary>
    /// 优化建议
    /// </summary>
    [Serializable]
    public class OptimizationSuggestion
    {
        public OptimizationType type;
        public OptimizationPriority priority;
        public string description;
        public string expectedImprovement;
        public float estimatedImpact; // 0-1之间
        public Action action;
        public Dictionary<string, object> parameters;
        
        public OptimizationSuggestion()
        {
            parameters = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// 优化类型
    /// </summary>
    public enum OptimizationType
    {
        QualitySettings,        // 质量设置
        VRResolution,          // VR分辨率
        FoveatedRendering,     // 注视点渲染
        MemoryManagement,      // 内存管理
        RenderingOptimization, // 渲染优化
        AIOptimization,        // AI优化
        NetworkOptimization,   // 网络优化
        AssetOptimization      // 资源优化
    }
    
    /// <summary>
    /// 优化优先级
    /// </summary>
    public enum OptimizationPriority
    {
        Low,        // 低
        Medium,     // 中
        High,       // 高
        Critical    // 严重
    }
    
    /// <summary>
    /// 性能报告
    /// </summary>
    [Serializable]
    public class PerformanceReport
    {
        public DateTime startTime;
        public DateTime endTime;
        public TimeSpan duration;
        public PerformanceMetrics averageMetrics;
        public PerformanceMetrics peakMetrics;
        public PerformanceMetrics minMetrics;
        public PerformanceStatus status;
        public List<PerformanceAlert> alerts;
        public List<OptimizationSuggestion> optimizations;
        public Dictionary<string, float> trends; // 性能趋势
        
        public PerformanceReport()
        {
            alerts = new List<PerformanceAlert>();
            optimizations = new List<OptimizationSuggestion>();
            trends = new Dictionary<string, float>();
        }
        
        /// <summary>
        /// 生成报告摘要
        /// </summary>
        public string GenerateSummary()
        {
            var summary = $"性能报告 ({startTime:yyyy-MM-dd HH:mm} - {endTime:yyyy-MM-dd HH:mm})\n";
            summary += $"持续时间: {duration.TotalMinutes:F1} 分钟\n";
            summary += $"平均帧率: {averageMetrics.averageFPS:F1} FPS\n";
            summary += $"平均内存使用: {averageMetrics.totalMemory / (1024 * 1024):F1} MB\n";
            summary += $"性能状态: {status}\n";
            summary += $"警报数量: {alerts.Count}\n";
            summary += $"优化建议: {optimizations.Count}";
            
            return summary;
        }
    }
    
    /// <summary>
    /// 性能配置
    /// </summary>
    [Serializable]
    public class PerformanceConfig
    {
        [Header("监控设置")]
        public bool enableMonitoring = true;
        public float updateInterval = 1.0f;
        public int maxSampleCount = 60;
        public bool enableAutoOptimization = true;
        
        [Header("性能目标")]
        public float targetFrameRate = 72f;
        public float criticalFrameRate = 45f;
        public long maxMemoryUsage = 1024 * 1024 * 1024; // 1GB
        public int maxDrawCalls = 1000;
        public int maxTriangles = 100000;
        
        [Header("VR设置")]
        public bool enableVROptimization = true;
        public float vrTargetFrameTime = 13.89f; // 72fps
        public bool enableDynamicResolution = true;
        public bool enableFoveatedRendering = true;
        
        [Header("警报设置")]
        public bool enableAlerts = true;
        public AlertSeverity minAlertSeverity = AlertSeverity.Medium;
        public float alertCooldown = 5.0f; // 秒
        
        [Header("优化设置")]
        public bool enableAggressiveOptimization = false;
        public float optimizationThreshold = 0.5f; // 性能评分阈值
        public int maxOptimizationSteps = 5;
    }
    
    /// <summary>
    /// 性能基准测试结果
    /// </summary>
    [Serializable]
    public class BenchmarkResult
    {
        public string testName;
        public DateTime testTime;
        public TimeSpan duration;
        public PerformanceMetrics results;
        public float score;
        public PerformanceGrade grade;
        public string deviceInfo;
        public Dictionary<string, object> additionalData;
        
        public BenchmarkResult()
        {
            additionalData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// 性能趋势数据
    /// </summary>
    [Serializable]
    public class PerformanceTrend
    {
        public string metricName;
        public List<float> values;
        public List<DateTime> timestamps;
        public float trend; // 正值表示上升趋势，负值表示下降趋势
        public float correlation; // 与其他指标的相关性
        
        public PerformanceTrend()
        {
            values = new List<float>();
            timestamps = new List<DateTime>();
        }
        
        /// <summary>
        /// 添加数据点
        /// </summary>
        public void AddDataPoint(float value, DateTime timestamp)
        {
            values.Add(value);
            timestamps.Add(timestamp);
            
            // 保持数据点数量限制
            while (values.Count > 100)
            {
                values.RemoveAt(0);
                timestamps.RemoveAt(0);
            }
            
            CalculateTrend();
        }
        
        /// <summary>
        /// 计算趋势
        /// </summary>
        private void CalculateTrend()
        {
            if (values.Count < 2) return;
            
            // 简单线性回归计算趋势
            float sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            int n = values.Count;
            
            for (int i = 0; i < n; i++)
            {
                float x = i;
                float y = values[i];
                
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }
            
            trend = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        }
    }
}