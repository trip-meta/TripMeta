using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Analytics;

namespace TripMeta.Tests.Analytics
{
    /// <summary>
    /// 分析管理器单元测试
    /// </summary>
    public class AnalyticsManagerTests
    {
        private GameObject testObject;
        private AnalyticsManager manager;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestAnalyticsManager");
            manager = testObject.AddComponent<AnalyticsManager>();
            manager.enableRealTimeAnalytics = true;
            manager.trackUserSessions = true;
            manager.trackVRInteractions = true;
            manager.enableABTesting = true;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator AnalyticsManager_Initialization_HasSessionId()
        {
            yield return null;

            Assert.IsNotNull(manager);
            Assert.IsNotNull(manager.CurrentSessionId);
            Assert.IsNotEmpty(manager.CurrentSessionId);
        }

        [UnityTest]
        public IEnumerator AnalyticsManager_TrackEvent_AddsToQueue()
        {
            yield return null;

            bool eventTracked = false;
            manager.OnEventTracked += (evt) => eventTracked = true;

            manager.TrackEvent("test_event", new Dictionary<string, object> { { "key", "value" } });

            Assert.IsTrue(eventTracked);
        }

        [UnityTest]
        public IEnumerator AnalyticsManager_TrackPageView_TracksCorrectly()
        {
            yield return null;

            AnalyticsEvent capturedEvent = null;
            manager.OnEventTracked += (evt) => capturedEvent = evt;

            manager.TrackPageView("home_page", new Dictionary<string, object> { { "referrer", "google" } });

            Assert.IsNotNull(capturedEvent);
            Assert.AreEqual("page_view", capturedEvent.eventName);
            Assert.AreEqual("home_page", capturedEvent.parameters["page_name"]);
        }

        [UnityTest]
        public IEnumerator AnalyticsManager_TrackVRInteraction_TracksVRData()
        {
            yield return null;

            AnalyticsEvent capturedEvent = null;
            manager.OnEventTracked += (evt) => capturedEvent = evt;

            manager.TrackVRInteraction("grab", "ancient_vase", 5.2f);

            Assert.IsNotNull(capturedEvent);
            Assert.AreEqual("vr_interaction", capturedEvent.eventName);
            Assert.AreEqual("grab", capturedEvent.parameters["interaction_type"]);
            Assert.AreEqual(5.2f, capturedEvent.parameters["duration"]);
        }

        [UnityTest]
        public IEnumerator AnalyticsManager_ABTest_AssignsVariant()
        {
            yield return null;

            string assignedExperiment = null;
            ABTestVariant assignedVariant = null;
            manager.OnABTestAssigned += (exp, variant) =>
            {
                assignedExperiment = exp;
                assignedVariant = variant;
            };

            yield return new WaitForSeconds(0.5f);

            // A/B测试应该在初始化时自动分配
            Assert.IsNotNull(assignedVariant);
            Assert.IsNotNull(assignedVariant.variantName);
        }

        [UnityTest]
        public IEnumerator AnalyticsManager_GetExperimentVariant_ReturnsVariant()
        {
            yield return null;

            var variant = manager.GetExperimentVariant("ui_layout_v2", "control");
            Assert.IsNotNull(variant);
            Assert.IsTrue(variant == "control" || variant == "variant_a" || variant == "variant_b");
        }

        [UnityTest]
        public IEnumerator AnalyticsManager_TrackConversion_TracksRevenue()
        {
            yield return null;

            AnalyticsEvent capturedEvent = null;
            manager.OnEventTracked += (evt) => capturedEvent = evt;

            manager.TrackConversion("purchase", 19.99m, new Dictionary<string, object> { { "product", "premium" } });

            Assert.IsNotNull(capturedEvent);
            Assert.AreEqual("conversion", capturedEvent.eventName);
            Assert.AreEqual(19.99m, capturedEvent.parameters["value"]);
        }

        [UnityTest]
        public IEnumerator AnalyticsManager_DashboardData_UpdatesInRealTime()
        {
            yield return null;

            var initialData = manager.GetDashboardData();
            initialData.totalEvents = 0;

            manager.TrackEvent("test_event_1");
            manager.TrackEvent("test_event_2");

            var updatedData = manager.GetDashboardData();
            Assert.Greater(updatedData.totalEvents, initialData.totalEvents);
        }

        [Test]
        public void UserProperties_HasDeviceInfo()
        {
            var props = new UserProperties
            {
                userId = "user_123",
                deviceType = "Desktop",
                osVersion = "Windows 11",
                vrHeadset = "Meta Quest 3"
            };

            Assert.AreEqual("user_123", props.userId);
            Assert.AreEqual("Desktop", props.deviceType);
            Assert.AreEqual("Meta Quest 3", props.vrHeadset);
        }

        [Test]
        public void RealTimeDashboardData_TracksMetrics()
        {
            var data = new RealTimeDashboardData
            {
                activeUsers = 150,
                totalEvents = 5000,
                conversions = 45,
                revenue = 1250.50m,
                errors = 3
            };

            Assert.AreEqual(150, data.activeUsers);
            Assert.AreEqual(5000, data.totalEvents);
            Assert.AreEqual(45, data.conversions);
            Assert.AreEqual(1250.50m, data.revenue);
        }

        [UnityTest]
        public IEnumerator DashboardService_GetRetentionReport_ReturnsReport()
        {
            var service = new DashboardService();
            var task = service.GetRetentionReport(30);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsNotNull(task.Result);
            Assert.AreEqual(30, task.Result.period);
            Assert.IsNotNull(task.Result.cohorts);
        }

        [UnityTest]
        public IEnumerator DashboardService_GetFunnelReport_ReturnsFunnel()
        {
            var service = new DashboardService();
            var task = service.GetFunnelReport("signup");

            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsNotNull(task.Result);
            Assert.AreEqual("signup", task.Result.funnelName);
            Assert.IsNotNull(task.Result.steps);
            Assert.Greater(task.Result.steps.Count, 0);
        }

        [UnityTest]
        public IEnumerator DashboardService_GetRevenueReport_ReturnsRevenue()
        {
            var service = new DashboardService();
            var task = service.GetRevenueReport(12);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsNotNull(task.Result);
            Assert.Greater(task.Result.totalRevenue, 0);
            Assert.Greater(task.Result.mrr, 0);
        }

        [Test]
        public void FunnelStep_HasConversionData()
        {
            var step = new FunnelStep
            {
                stepName = "Checkout",
                users = 500,
                conversionRate = 25.5f
            };

            Assert.AreEqual("Checkout", step.stepName);
            Assert.AreEqual(500, step.users);
            Assert.AreEqual(25.5f, step.conversionRate);
        }

        [Test]
        public void ABTestVariant_HasAssignmentData()
        {
            var variant = new ABTestVariant
            {
                experimentId = "exp_001",
                variantName = "variant_a",
                assignedAt = System.DateTime.Now
            };

            Assert.AreEqual("exp_001", variant.experimentId);
            Assert.AreEqual("variant_a", variant.variantName);
        }
    }
}
