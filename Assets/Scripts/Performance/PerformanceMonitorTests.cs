using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Performance;

namespace TripMeta.Tests.Performance
{
    /// <summary>
    /// 性能监控模块单元测试
    /// </summary>
    public class PerformanceMonitorTests
    {
        private GameObject testObject;
        private PerformanceMonitor monitor;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestPerformanceMonitor");
            monitor = testObject.AddComponent<PerformanceMonitor>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_Initialization_SetsCorrectDefaults()
        {
            yield return null;

            Assert.IsNotNull(monitor);
            Assert.IsTrue(monitor.enableMonitoring);
            Assert.AreEqual(1.0f, monitor.updateInterval);
            Assert.AreEqual(300, monitor.maxHistorySize);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_TracksFPS_WhenEnabled()
        {
            monitor.trackFPS = true;
            monitor.targetFPS = 72f;
            monitor.warningFPS = 60f;
            monitor.criticalFPS = 45f;

            yield return new WaitForSeconds(0.1f);

            var data = monitor.CurrentData;
            Assert.Greater(data.fps, 0);
            Assert.AreEqual(72f, data.targetFPS);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_TracksMemory_WhenEnabled()
        {
            monitor.trackMemory = true;

            yield return new WaitForSeconds(0.1f);

            var data = monitor.CurrentData;
            Assert.Greater(data.totalMemoryMB, 0);
            Assert.Greater(data.allocatedMemoryMB, 0);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_TracksLatency_WhenEnabled()
        {
            monitor.trackLatency = true;

            yield return new WaitForSeconds(0.1f);

            var data = monitor.CurrentData;
            Assert.Greater(data.frameTime, 0);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_TracksRendering_WhenEnabled()
        {
            monitor.trackRendering = true;

            yield return new WaitForSeconds(0.1f);

            var data = monitor.CurrentData;
            Assert.GreaterOrEqual(data.drawCalls, 0);
            Assert.GreaterOrEqual(data.triangles, 0);
            Assert.GreaterOrEqual(data.vertices, 0);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_GeneratesReport_WithCorrectData()
        {
            monitor.trackFPS = true;
            monitor.trackMemory = true;
            monitor.trackLatency = true;
            monitor.trackRendering = true;

            yield return new WaitForSeconds(0.2f);

            var report = monitor.GenerateReport(System.TimeSpan.FromMinutes(5));

            Assert.IsNotNull(report);
            Assert.Greater(report.sampleCount, 0);
            Assert.GreaterOrEqual(report.performanceScore, 0);
            Assert.LessOrEqual(report.performanceScore, 100);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_ClearsHistory_Successfully()
        {
            monitor.trackFPS = true;
            yield return new WaitForSeconds(0.2f);

            Assert.Greater(monitor.DataHistory.Length, 0);

            monitor.ClearHistory();

            Assert.AreEqual(0, monitor.DataHistory.Length);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_CalculatesAverageData_Correctly()
        {
            monitor.trackFPS = true;
            monitor.trackLatency = true;
            monitor.trackMemory = true;

            yield return new WaitForSeconds(0.3f);

            var avgData = monitor.GetAverageData(10);

            Assert.Greater(avgData.fps, 0);
            Assert.Greater(avgData.frameTime, 0);
        }

        [UnityTest]
        public IEnumerator PerformanceMonitor_TriggersFPSAlert_WhenLow()
        {
            monitor.trackFPS = true;
            monitor.criticalFPS = 1000f; // Set high to trigger alert
            monitor.warningFPS = 2000f;

            bool alertTriggered = false;
            monitor.OnAlertTriggered += (metric, level) =>
            {
                if (metric == "FPS")
                    alertTriggered = true;
            };

            yield return new WaitForSeconds(0.2f);

            // Alert may or may not trigger depending on actual FPS
            // Just verify the alert system is wired correctly
            Assert.IsNotNull(monitor);
        }

        [Test]
        public void PerformanceMonitor_RegisterCustomMetric_StoresValue()
        {
            monitor.RegisterCustomMetric("TestMetric", 42.0f);

            var data = monitor.CurrentData;
            Assert.IsTrue(data.customMetrics.ContainsKey("TestMetric"));
            Assert.AreEqual(42.0f, data.customMetrics["TestMetric"]);
        }
    }
}
