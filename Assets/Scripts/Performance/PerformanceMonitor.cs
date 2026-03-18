using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace TripMeta.Performance
{
    /// <summary>
    /// 性能监控管理器
    /// 实时 FPS、延迟、内存监控和分析
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("监控配置")]
        public bool enableMonitoring = true;
        public float updateInterval = 1.0f;
        public int maxHistorySize = 300; // 5分钟历史 (1秒间隔)

        [Header("FPS 监控")]
        public bool trackFPS = true;
        public float targetFPS = 72f;
        public float warningFPS = 60f;
        public float criticalFPS = 45f;

        [Header("延迟监控")]
        public bool trackLatency = true;
        public float warningLatency = 20f; // ms
        public float criticalLatency = 50f; // ms

        [Header("内存监控")]
        public bool trackMemory = true;
        public long warningMemoryMB = 2048;
        public long criticalMemoryMB = 3072;

        [Header("渲染监控")]
        public bool trackRendering = true;
        public int warningDrawCalls = 2000;
        public int criticalDrawCalls = 3000;

        // 性能数据
        private PerformanceData currentData = new PerformanceData();
        private Queue<PerformanceData> dataHistory = new Queue<PerformanceData>();
        private Dictionary<string, float> customMetrics = new Dictionary<string, float>();

        // 统计
        private float fpsAccumulator = 0f;
        private int fpsFrameCount = 0;
        private float lastUpdateTime = 0f;

        public static PerformanceMonitor Instance { get; private set; }

        public PerformanceData CurrentData => currentData;
        public PerformanceData[] DataHistory => dataHistory.ToArray();
        public bool IsMonitoring => enableMonitoring;

        // 事件
        public event Action<PerformanceData> OnDataUpdated;
        public event Action<string, PerformanceAlertLevel> OnAlertTriggered;
        public event Action<PerformanceReport> OnReportGenerated;

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
            if (enableMonitoring)
            {
                StartCoroutine(MonitoringCoroutine());
            }
        }

        void Update()
        {
            if (!enableMonitoring) return;

            // 累计 FPS 数据
            if (trackFPS)
            {
                fpsAccumulator += 1f / Time.unscaledDeltaTime;
                fpsFrameCount++;
            }

            // 计算帧延迟
            if (trackLatency)
            {
                currentData.frameTime = Time.unscaledDeltaTime * 1000f;
            }
        }

        /// <summary>
        /// 监控协程
        /// </summary>
        private IEnumerator MonitoringCoroutine()
        {
            while (enableMonitoring)
            {
                yield return new WaitForSeconds(updateInterval);
                CollectPerformanceData();
            }
        }

        /// <summary>
        /// 收集性能数据
        /// </summary>
        private void CollectPerformanceData()
        {
            var data = new PerformanceData
            {
                timestamp = Time.time,
                frameCount = Time.frameCount
            };

            // FPS
            if (trackFPS && fpsFrameCount > 0)
            {
                data.fps = fpsAccumulator / fpsFrameCount;
                data.targetFPS = targetFPS;
                fpsAccumulator = 0f;
                fpsFrameCount = 0;
            }

            // 内存
            if (trackMemory)
            {
                data.totalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024;
                data.allocatedMemoryMB = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024;
                data.reservedMemoryMB = Profiler.GetTotalReservedMemoryLong() / 1024 / 1024;
                data.monoMemoryMB = Profiler.GetMonoUsedSizeLong() / 1024 / 1024;
            }

            // 渲染统计
            if (trackRendering)
            {
                data.drawCalls = UnityStats.drawCalls;
                data.triangles = UnityStats.triangles;
                data.vertices = UnityStats.vertices;
                data.shadowCasters = UnityStats.shadowCasters;
            }

            // 延迟
            if (trackLatency)
            {
                data.frameTime = currentData.frameTime;
                data.avgFrameTime = dataHistory.Count > 0
                    ? dataHistory.Average(d => d.frameTime)
                    : data.frameTime;
            }

            // 自定义指标
            data.customMetrics = new Dictionary<string, float>(customMetrics);

            // 检查警报
            CheckAlerts(data);

            // 更新当前数据
            currentData = data;

            // 添加到历史
            dataHistory.Enqueue(data);
            while (dataHistory.Count > maxHistorySize)
            {
                dataHistory.Dequeue();
            }

            OnDataUpdated?.Invoke(data);
        }

        /// <summary>
        /// 检查警报条件
        /// </summary>
        private void CheckAlerts(PerformanceData data)
        {
            // FPS 警报
            if (trackFPS)
            {
                if (data.fps < criticalFPS)
                {
                    TriggerAlert("FPS", PerformanceAlertLevel.Critical, $"FPS 过低: {data.fps:F1}");
                }
                else if (data.fps < warningFPS)
                {
                    TriggerAlert("FPS", PerformanceAlertLevel.Warning, $"FPS 警告: {data.fps:F1}");
                }
            }

            // 内存警报
            if (trackMemory)
            {
                if (data.totalMemoryMB > criticalMemoryMB)
                {
                    TriggerAlert("Memory", PerformanceAlertLevel.Critical, $"内存过高: {data.totalMemoryMB}MB");
                }
                else if (data.totalMemoryMB > warningMemoryMB)
                {
                    TriggerAlert("Memory", PerformanceAlertLevel.Warning, $"内存警告: {data.totalMemoryMB}MB");
                }
            }

            // 延迟警报
            if (trackLatency)
            {
                if (data.frameTime > criticalLatency)
                {
                    TriggerAlert("Latency", PerformanceAlertLevel.Critical, $"延迟过高: {data.frameTime:F1}ms");
                }
                else if (data.frameTime > warningLatency)
                {
                    TriggerAlert("Latency", PerformanceAlertLevel.Warning, $"延迟警告: {data.frameTime:F1}ms");
                }
            }

            // DrawCall 警报
            if (trackRendering)
            {
                if (data.drawCalls > criticalDrawCalls)
                {
                    TriggerAlert("DrawCalls", PerformanceAlertLevel.Critical, $"DrawCall 过高: {data.drawCalls}");
                }
                else if (data.drawCalls > warningDrawCalls)
                {
                    TriggerAlert("DrawCalls", PerformanceAlertLevel.Warning, $"DrawCall 警告: {data.drawCalls}");
                }
            }
        }

        /// <summary>
        /// 触发警报
        /// </summary>
        private void TriggerAlert(string metric, PerformanceAlertLevel level, string message)
        {
            Debug.LogWarning($"[PerformanceMonitor] [{level}] {message}");
            OnAlertTriggered?.Invoke(metric, level);
        }

        /// <summary>
        /// 生成性能报告
        /// </summary>
        public PerformanceReport GenerateReport(TimeSpan duration)
        {
            var dataArray = dataHistory.ToArray();
            if (dataArray.Length == 0) return null;

            var report = new PerformanceReport
            {
                generatedAt = DateTime.Now,
                duration = duration,
                sampleCount = dataArray.Length
            };

            // FPS 统计
            if (trackFPS)
            {
                report.avgFPS = dataArray.Average(d => d.fps);
                report.minFPS = dataArray.Min(d => d.fps);
                report.maxFPS = dataArray.Max(d => d.fps);
                report.fpsBelow60 = dataArray.Count(d => d.fps < 60) / (float)dataArray.Length * 100f;
                report.fpsBelow45 = dataArray.Count(d => d.fps < 45) / (float)dataArray.Length * 100f;
            }

            // 内存统计
            if (trackMemory)
            {
                report.avgMemoryMB = dataArray.Average(d => d.totalMemoryMB);
                report.maxMemoryMB = dataArray.Max(d => d.totalMemoryMB);
                report.memoryIncreaseMB = dataArray.Last().totalMemoryMB - dataArray.First().totalMemoryMB;
            }

            // 延迟统计
            if (trackLatency)
            {
                report.avgFrameTime = dataArray.Average(d => d.frameTime);
                report.maxFrameTime = dataArray.Max(d => d.frameTime);
                report.avgLatency = report.avgFrameTime;
            }

            // 渲染统计
            if (trackRendering)
            {
                report.avgDrawCalls = (int)dataArray.Average(d => d.drawCalls);
                report.maxDrawCalls = dataArray.Max(d => d.drawCalls);
                report.avgTriangles = (int)dataArray.Average(d => d.triangles);
            }

            // 计算性能评分
            report.performanceScore = CalculatePerformanceScore(report);

            OnReportGenerated?.Invoke(report);
            return report;
        }

        /// <summary>
        /// 计算性能评分
        /// </summary>
        private float CalculatePerformanceScore(PerformanceReport report)
        {
            float score = 100f;

            // FPS 评分 (权重 40%)
            if (trackFPS)
            {
                float fpsScore = Mathf.Clamp01(report.avgFPS / targetFPS) * 40f;
                score = fpsScore;
            }

            // 内存评分 (权重 20%)
            if (trackMemory)
            {
                float memoryRatio = report.avgMemoryMB / warningMemoryMB;
                score += Mathf.Clamp01(1f - memoryRatio) * 20f;
            }

            // 延迟评分 (权重 20%)
            if (trackLatency)
            {
                float latencyScore = Mathf.Clamp01(1f - report.avgLatency / warningLatency) * 20f;
                score += latencyScore;
            }

            // 渲染评分 (权重 20%)
            if (trackRendering)
            {
                float drawCallRatio = report.avgDrawCalls / (float)warningDrawCalls;
                score += Mathf.Clamp01(1f - drawCallRatio) * 20f;
            }

            return Mathf.Clamp(score, 0f, 100f);
        }

        /// <summary>
        /// 注册自定义指标
        /// </summary>
        public void RegisterCustomMetric(string name, float value)
        {
            customMetrics[name] = value;
        }

        /// <summary>
        /// 获取历史数据平均值
        /// </summary>
        public PerformanceData GetAverageData(int sampleCount = 60)
        {
            var dataArray = dataHistory.ToArray();
            if (dataArray.Length == 0) return new PerformanceData();

            var recentData = dataArray.Skip(Math.Max(0, dataArray.Length - sampleCount)).ToArray();

            return new PerformanceData
            {
                fps = recentData.Average(d => d.fps),
                frameTime = recentData.Average(d => d.frameTime),
                totalMemoryMB = (long)recentData.Average(d => d.totalMemoryMB),
                drawCalls = (int)recentData.Average(d => d.drawCalls),
                triangles = (int)recentData.Average(d => d.triangles)
            };
        }

        /// <summary>
        /// 清除历史数据
        /// </summary>
        public void ClearHistory()
        {
            dataHistory.Clear();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 性能数据
    /// </summary>
    [Serializable]
    public struct PerformanceData
    {
        public float timestamp;
        public int frameCount;

        // FPS
        public float fps;
        public float targetFPS;

        // 延迟
        public float frameTime; // ms
        public float avgFrameTime; // ms

        // 内存 (MB)
        public long totalMemoryMB;
        public long allocatedMemoryMB;
        public long reservedMemoryMB;
        public long monoMemoryMB;

        // 渲染
        public int drawCalls;
        public int triangles;
        public int vertices;
        public int shadowCasters;

        // 自定义指标
        public Dictionary<string, float> customMetrics;
    }

    /// <summary>
    /// 性能报告
    /// </summary>
    [Serializable]
    public struct PerformanceReport
    {
        public DateTime generatedAt;
        public TimeSpan duration;
        public int sampleCount;
        public float performanceScore;

        // FPS
        public float avgFPS;
        public float minFPS;
        public float maxFPS;
        public float fpsBelow60;
        public float fpsBelow45;

        // 内存
        public double avgMemoryMB;
        public long maxMemoryMB;
        public long memoryIncreaseMB;

        // 延迟
        public double avgFrameTime;
        public float maxFrameTime;
        public double avgLatency;

        // 渲染
        public int avgDrawCalls;
        public int maxDrawCalls;
        public int avgTriangles;
    }

    /// <summary>
    /// 警报级别
    /// </summary>
    public enum PerformanceAlertLevel
    {
        Info,
        Warning,
        Critical
    }

    #endregion
}
