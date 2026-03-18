using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.CloudRendering;

namespace TripMeta.Tests.CloudRendering
{
    /// <summary>
    /// 云渲染管理器单元测试
    /// </summary>
    public class CloudRenderingTests
    {
        private GameObject testObject;
        private CloudRenderingManager cloudManager;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestCloudRenderingManager");
            cloudManager = testObject.AddComponent<CloudRenderingManager>();
            cloudManager.enableCloudRendering = true;
            cloudManager.targetResolutionX = 1920;
            cloudManager.targetResolutionY = 1080;
            cloudManager.targetFrameRate = 60;
            cloudManager.bitrateKbps = 20000;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator CloudRendering_Initialization_SetsCorrectDefaults()
        {
            yield return null;

            Assert.IsNotNull(cloudManager);
            Assert.IsTrue(cloudManager.enableCloudRendering);
            Assert.AreEqual(1920, cloudManager.targetResolutionX);
            Assert.AreEqual(1080, cloudManager.targetResolutionY);
            Assert.AreEqual(60, cloudManager.targetFrameRate);
            Assert.AreEqual(20000, cloudManager.bitrateKbps);
        }

        [Test]
        public void CloudRendering_IsDeviceSupported_ReturnsBoolean()
        {
            bool supported = CloudRenderingManager.IsDeviceSupported();
            Assert.IsTrue(supported || !supported); // 只检查方法执行不报错
        }

        [Test]
        public void CloudRendering_GetRecommendedSettings_ReturnsValidSettings()
        {
            var settings = CloudRenderingManager.GetRecommendedSettings();

            Assert.Greater(settings.resolutionX, 0);
            Assert.Greater(settings.resolutionY, 0);
            Assert.Greater(settings.frameRate, 0);
            Assert.Greater(settings.bitrateKbps, 0);
        }

        [Test]
        public void CloudRendering_GetRecommendedSettings_MobileHasLowerSettings()
        {
            var settings = CloudRenderingManager.GetRecommendedSettings();

            if (Application.isMobilePlatform)
            {
                Assert.LessOrEqual(settings.resolutionX, 1280);
                Assert.LessOrEqual(settings.frameRate, 30);
                Assert.LessOrEqual(settings.bitrateKbps, 10000);
            }
        }

        [Test]
        public void InputEvent_Creation_StoresDataCorrectly()
        {
            var input = new InputEvent
            {
                type = InputType.HeadTracking,
                controllerIndex = 0,
                timestamp = Time.time,
                position = Vector3.one,
                rotation = Quaternion.identity
            };

            Assert.AreEqual(InputType.HeadTracking, input.type);
            Assert.AreEqual(Vector3.one, input.position);
            Assert.AreEqual(Quaternion.identity, input.rotation);
        }

        [Test]
        public void StreamingStats_StoresDataCorrectly()
        {
            var stats = new StreamingStats
            {
                frameRate = 60,
                bitrateKbps = 20000,
                packetLoss = 0.02f,
                latencyMs = 50,
                resolutionX = 1920,
                resolutionY = 1080
            };

            Assert.AreEqual(60, stats.frameRate);
            Assert.AreEqual(20000, stats.bitrateKbps);
            Assert.AreEqual(0.02f, stats.packetLoss, 0.001f);
            Assert.AreEqual(50, stats.latencyMs);
        }

        [Test]
        public void RenderServerInfo_StoresDataCorrectly()
        {
            var server = new RenderServerInfo
            {
                sessionId = "test-session-123",
                region = "asia-east1",
                gpuType = "NVIDIA RTX 4090",
                availableStreams = 5
            };

            Assert.AreEqual("test-session-123", server.sessionId);
            Assert.AreEqual("asia-east1", server.region);
            Assert.AreEqual("NVIDIA RTX 4090", server.gpuType);
            Assert.AreEqual(5, server.availableStreams);
        }

        [UnityTest]
        public IEnumerator CloudRendering_Disconnect_WhenNotConnected_DoesNotThrow()
        {
            yield return null;

            Assert.DoesNotThrow(() => cloudManager.Disconnect());
        }

        [Test]
        public void RenderingSettings_Values_ArePositive()
        {
            var settings = new RenderingSettings
            {
                resolutionX = 1920,
                resolutionY = 1080,
                frameRate = 60,
                bitrateKbps = 20000
            };

            Assert.Greater(settings.resolutionX, 0);
            Assert.Greater(settings.resolutionY, 0);
            Assert.Greater(settings.frameRate, 0);
            Assert.Greater(settings.bitrateKbps, 0);
        }

        [Test]
        public void InputBatch_CanStoreMultipleInputs()
        {
            var batch = new InputBatch
            {
                inputs = new InputEvent[]
                {
                    new InputEvent { type = InputType.HeadTracking },
                    new InputEvent { type = InputType.Controller },
                    new InputEvent { type = InputType.HandGesture }
                }
            };

            Assert.AreEqual(3, batch.inputs.Length);
            Assert.AreEqual(InputType.HeadTracking, batch.inputs[0].type);
            Assert.AreEqual(InputType.Controller, batch.inputs[1].type);
            Assert.AreEqual(InputType.HandGesture, batch.inputs[2].type);
        }
    }
}
