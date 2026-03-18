using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TripMeta.Performance
{
    /// <summary>
    /// 性能监控仪表板 UI
    /// 实时显示 FPS、延迟、内存、渲染统计
    /// </summary>
    public class PerformanceDashboard : MonoBehaviour
    {
        [Header("面板配置")]
        public bool showOnStart = false;
        public KeyCode toggleKey = KeyCode.F12;
        public Canvas dashboardCanvas;

        [Header("FPS 显示")]
        public Text fpsText;
        public Image fpsBar;
        public Color fpsGoodColor = Color.green;
        public Color fpsWarningColor = Color.yellow;
        public Color fpsCriticalColor = Color.red;

        [Header("延迟显示")]
        public Text latencyText;
        public Image latencyBar;
        public Color latencyGoodColor = Color.green;
        public Color latencyWarningColor = Color.yellow;
        public Color latencyCriticalColor = Color.red;

        [Header("内存显示")]
        public Text memoryText;
        public Image memoryBar;
        public Slider memorySlider;

        [Header("渲染显示")]
        public Text drawCallsText;
        public Text trianglesText;
        public Text verticesText;

        [Header("图表")]
        public PerformanceGraph fpsGraph;
        public PerformanceGraph memoryGraph;
        public PerformanceGraph latencyGraph;

        [Header("警报")]
        public GameObject alertPanel;
        public Text alertText;
        public float alertDuration = 3f;

        private PerformanceMonitor monitor;
        private Queue<string> alertQueue = new Queue<string>();
        private bool isShowingAlert = false;

        void Start()
        {
            monitor = PerformanceMonitor.Instance;
            if (monitor == null)
            {
                Debug.LogError("[PerformanceDashboard] PerformanceMonitor 未找到");
                enabled = false;
                return;
            }

            monitor.OnDataUpdated += OnPerformanceDataUpdated;
            monitor.OnAlertTriggered += OnPerformanceAlert;

            if (dashboardCanvas != null)
            {
                dashboardCanvas.enabled = showOnStart;
            }

            if (alertPanel != null)
            {
                alertPanel.SetActive(false);
            }
        }

        void Update()
        {
            // 切换显示
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleDashboard();
            }

            // 处理警报队列
            ProcessAlertQueue();
        }

        /// <summary>
        /// 切换仪表板显示
        /// </summary>
        public void ToggleDashboard()
        {
            if (dashboardCanvas != null)
            {
                dashboardCanvas.enabled = !dashboardCanvas.enabled;
            }
        }

        /// <summary>
        /// 性能数据更新回调
        /// </summary>
        private void OnPerformanceDataUpdated(PerformanceData data)
        {
            if (!dashboardCanvas.enabled) return;

            UpdateFPSDisplay(data);
            UpdateLatencyDisplay(data);
            UpdateMemoryDisplay(data);
            UpdateRenderingDisplay(data);
            UpdateGraphs(data);
        }

        /// <summary>
        /// 更新 FPS 显示
        /// </summary>
        private void UpdateFPSDisplay(PerformanceData data)
        {
            if (fpsText != null)
            {
                fpsText.text = $"{data.fps:F1} FPS";
            }

            if (fpsBar != null)
            {
                float ratio = Mathf.Clamp01(data.fps / data.targetFPS);
                fpsBar.fillAmount = ratio;

                // 根据 FPS 设置颜色
                if (data.fps >= 60)
                    fpsBar.color = fpsGoodColor;
                else if (data.fps >= 45)
                    fpsBar.color = fpsWarningColor;
                else
                    fpsBar.color = fpsCriticalColor;
            }
        }

        /// <summary>
        /// 更新延迟显示
        /// </summary>
        private void UpdateLatencyDisplay(PerformanceData data)
        {
            if (latencyText != null)
            {
                latencyText.text = $"{data.frameTime:F1} ms";
            }

            if (latencyBar != null)
            {
                float ratio = Mathf.Clamp01(data.frameTime / 50f);
                latencyBar.fillAmount = ratio;

                if (data.frameTime < 16.6f)
                    latencyBar.color = latencyGoodColor;
                else if (data.frameTime < 33.3f)
                    latencyBar.color = latencyWarningColor;
                else
                    latencyBar.color = latencyCriticalColor;
            }
        }

        /// <summary>
        /// 更新内存显示
        /// </summary>
        private void UpdateMemoryDisplay(PerformanceData data)
        {
            if (memoryText != null)
            {
                memoryText.text = $"{data.totalMemoryMB} MB";
            }

            if (memoryBar != null)
            {
                float ratio = Mathf.Clamp01(data.totalMemoryMB / 3072f);
                memoryBar.fillAmount = ratio;
            }

            if (memorySlider != null)
            {
                memorySlider.value = data.totalMemoryMB;
            }
        }

        /// <summary>
        /// 更新渲染统计
        /// </summary>
        private void UpdateRenderingDisplay(PerformanceData data)
        {
            if (drawCallsText != null)
            {
                drawCallsText.text = $"Draw Calls: {data.drawCalls}";
            }

            if (trianglesText != null)
            {
                trianglesText.text = $"Triangles: {data.triangles / 1000}K";
            }

            if (verticesText != null)
            {
                verticesText.text = $"Vertices: {data.vertices / 1000}K";
            }
        }

        /// <summary>
        /// 更新图表
        /// </summary>
        private void UpdateGraphs(PerformanceData data)
        {
            if (fpsGraph != null)
            {
                fpsGraph.AddDataPoint(data.fps);
            }

            if (memoryGraph != null)
            {
                memoryGraph.AddDataPoint(data.totalMemoryMB);
            }

            if (latencyGraph != null)
            {
                latencyGraph.AddDataPoint(data.frameTime);
            }
        }

        /// <summary>
        /// 性能警报回调
        /// </summary>
        private void OnPerformanceAlert(string metric, PerformanceAlertLevel level)
        {
            string message = $"[{level}] {metric} 警报";
            alertQueue.Enqueue(message);
        }

        /// <summary>
        /// 处理警报队列
        /// </summary>
        private void ProcessAlertQueue()
        {
            if (isShowingAlert || alertQueue.Count == 0) return;

            string message = alertQueue.Dequeue();
            ShowAlert(message);
        }

        /// <summary>
        /// 显示警报
        /// </summary>
        private void ShowAlert(string message)
        {
            if (alertPanel == null || alertText == null) return;

            isShowingAlert = true;
            alertText.text = message;
            alertPanel.SetActive(true);

            Invoke(nameof(HideAlert), alertDuration);
        }

        /// <summary>
        /// 隐藏警报
        /// </summary>
        private void HideAlert()
        {
            if (alertPanel != null)
            {
                alertPanel.SetActive(false);
            }
            isShowingAlert = false;
        }

        /// <summary>
        /// 生成并显示报告
        /// </summary>
        public void ShowPerformanceReport()
        {
            if (monitor == null) return;

            var report = monitor.GenerateReport(TimeSpan.FromMinutes(5));
            if (report.HasValue)
            {
                Debug.Log($"[PerformanceDashboard] 性能报告:" +
                    $"\n评分: {report.Value.performanceScore:F1}/100" +
                    $"\n平均FPS: {report.Value.avgFPS:F1}" +
                    $"\n平均内存: {report.Value.avgMemoryMB:F0}MB" +
                    $"\n平均延迟: {report.Value.avgLatency:F1}ms" +
                    $"\n平均DrawCalls: {report.Value.avgDrawCalls}");
            }
        }

        void OnDestroy()
        {
            if (monitor != null)
            {
                monitor.OnDataUpdated -= OnPerformanceDataUpdated;
                monitor.OnAlertTriggered -= OnPerformanceAlert;
            }
        }
    }
}
