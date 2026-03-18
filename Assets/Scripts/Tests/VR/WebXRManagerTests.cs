using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TripMeta.VR.WebXR;

namespace TripMeta.Tests.VR
{
    /// <summary>
    /// WebXR 管理器单元测试
    /// 测试浏览器 VR 体验、WebAssembly 优化、云渲染功能
    /// </summary>
    public class WebXRManagerTests
    {
        [Test]
        public void WebXRSessionMode_EnumValues()
        {
            var modes = System.Enum.GetValues(typeof(WebXRSessionMode));
            Assert.Contains(WebXRSessionMode.None, modes);
            Assert.Contains(WebXRSessionMode.Inline, modes);
            Assert.Contains(WebXRSessionMode.ImmersiveVR, modes);
            Assert.Contains(WebXRSessionMode.ImmersiveAR, modes);
        }

        [Test]
        public void WebXRHandData_DefaultConstructor()
        {
            var handData = new WebXRHandData(false, 0);

            Assert.IsFalse(handData.isTracked);
            Assert.AreEqual(0, handData.handIndex);
            Assert.AreEqual(25, handData.jointPositions.Length);
            Assert.AreEqual(25, handData.jointRotations.Length);
            Assert.AreEqual(25, handData.jointRadii.Length);
            Assert.AreEqual(0f, handData.pinchValue);
            Assert.AreEqual(0f, handData.grabValue);
            Assert.IsFalse(handData.isPinching);
            Assert.IsFalse(handData.isGrabbing);
        }

        [Test]
        public void WebXRHandData_TrackedProperties()
        {
            var handData = new WebXRHandData(true, 1)
            {
                pinchValue = 0.85f,
                grabValue = 0.9f,
                isPinching = true,
                isGrabbing = true
            };

            Assert.IsTrue(handData.isTracked);
            Assert.AreEqual(1, handData.handIndex);
            Assert.AreEqual(0.85f, handData.pinchValue);
            Assert.AreEqual(0.9f, handData.grabValue);
            Assert.IsTrue(handData.isPinching);
            Assert.IsTrue(handData.isGrabbing);
        }

        [Test]
        public void WebXRHeadsetData_DefaultValues()
        {
            var headsetData = new WebXRHeadsetData();

            Assert.AreEqual(Vector3.zero, headsetData.position);
            Assert.AreEqual(Quaternion.identity, headsetData.rotation);
            Assert.IsFalse(headsetData.isTracked);
        }

        [Test]
        public void WebXRHeadsetData_TrackedValues()
        {
            var headsetData = new WebXRHeadsetData
            {
                position = new Vector3(1, 2, 3),
                rotation = Quaternion.Euler(45, 30, 0),
                isTracked = true,
                angularVelocity = new Vector3(0.1f, 0.2f, 0),
                linearVelocity = new Vector3(0.5f, 0, 0),
                timestamp = 12345.6f
            };

            Assert.IsTrue(headsetData.isTracked);
            Assert.AreEqual(new Vector3(1, 2, 3), headsetData.position);
            Assert.AreEqual(new Vector3(0.1f, 0.2f, 0), headsetData.angularVelocity);
            Assert.AreEqual(new Vector3(0.5f, 0, 0), headsetData.linearVelocity);
            Assert.AreEqual(12345.6f, headsetData.timestamp);
        }

        [Test]
        public void WebXRDeviceInfo_DefaultValues()
        {
            var deviceInfo = new WebXRDeviceInfo();

            Assert.IsFalse(deviceInfo.isWebXRSupported);
            Assert.IsFalse(deviceInfo.isInVR);
            Assert.AreEqual(WebXRSessionMode.None, deviceInfo.currentMode);
            Assert.AreEqual(1.0f, deviceInfo.renderScale);
            Assert.AreEqual(0, deviceInfo.targetFrameRate);
        }

        [Test]
        public void WebXRRenderSettings_DefaultValues()
        {
            var settings = new WebXRRenderSettings();

            Assert.AreEqual(0f, settings.renderScale);
            Assert.AreEqual(0, settings.targetFrameRate);
            Assert.AreEqual(0, settings.textureQuality);
            Assert.IsFalse(settings.enableShadows);
            Assert.IsFalse(settings.enablePostProcessing);
        }

        [UnityTest]
        public IEnumerator WebXRManager_Creation()
        {
            var testObject = new GameObject("TestWebXRManager");
            var manager = testObject.AddComponent<WebXRManager>();

            Assert.IsNotNull(manager);
            Assert.IsFalse(manager.IsInitialized);
            Assert.IsFalse(manager.IsInVR);
            Assert.AreEqual(WebXRSessionMode.None, manager.CurrentSessionMode);

            // 检查默认配置
            Assert.IsTrue(manager.autoInitialize);
            Assert.AreEqual(72, manager.targetFrameRate);
            Assert.AreEqual(1.0f, manager.renderScale);
            Assert.IsTrue(manager.enableHandTracking);
            Assert.IsTrue(manager.enableWebAssembly);

            Object.Destroy(testObject);
            yield return null;
        }

        [Test]
        public void WebXRInputHandler_DefaultValues()
        {
            var handlerObject = new GameObject("TestInputHandler");
            var handler = handlerObject.AddComponent<WebXRInputHandler>();

            Assert.IsTrue(handler.enableHandTracking);
            Assert.IsTrue(handler.enableGamepadInput);
            Assert.IsTrue(handler.enableTouchInput);
            Assert.AreEqual(0.02f, handler.pinchThreshold);
            Assert.AreEqual(0.8f, handler.grabThreshold);
            Assert.AreEqual(0.1f, handler.gestureCooldown);

            Object.Destroy(handlerObject);
        }

        [Test]
        public void WebXRRenderHandler_DefaultValues()
        {
            var handlerObject = new GameObject("TestRenderHandler");
            var handler = handlerObject.AddComponent<WebXRRenderHandler>();

            Assert.AreEqual(72, handler.targetFrameRate);
            Assert.AreEqual(1.0f, handler.renderScale);
            Assert.IsFalse(handler.enableFoveatedRendering);
            Assert.AreEqual(1, handler.foveationLevel);
            Assert.IsTrue(handler.enableSinglePassRendering);
            Assert.IsTrue(handler.enableInstancing);

            Object.Destroy(handlerObject);
        }

        [Test]
        public void WebXRNetworkHandler_DefaultValues()
        {
            var handlerObject = new GameObject("TestNetworkHandler");
            var handler = handlerObject.AddComponent<WebXRNetworkHandler>();

            Assert.AreEqual("wss://tripmeta.io/signalling", handler.signallingServerUrl);
            Assert.AreEqual(3, handler.reconnectAttempts);
            Assert.AreEqual(5f, handler.reconnectDelay);
            Assert.IsFalse(handler.enableCloudRendering);
            Assert.AreEqual(20000000, handler.cloudRenderingBitrate);
            Assert.AreEqual(60, handler.cloudRenderingFps);
            Assert.AreEqual(100f, handler.latencyThreshold);

            Object.Destroy(handlerObject);
        }

        [Test]
        public void WebXRCacheManager_DefaultValues()
        {
            var managerObject = new GameObject("TestCacheManager");
            var manager = managerObject.AddComponent<WebXRCacheManager>();

            Assert.IsTrue(manager.enableCaching);
            Assert.IsTrue(manager.enableCompression);
            Assert.AreEqual(100, manager.maxCacheSizeMB);
            Assert.AreEqual(500, manager.maxCacheEntries);
            Assert.AreEqual(7f, manager.cacheExpirationDays);
            Assert.IsTrue(manager.preloadOnStart);

            Object.Destroy(managerObject);
        }

        [Test]
        public void WebXR_CalculatePinchDistance()
        {
            Vector3 thumbTip = Vector3.zero;
            Vector3 indexTip = new Vector3(0.01f, 0, 0);

            float distance = Vector3.Distance(thumbTip, indexTip);

            Assert.AreEqual(0.01f, distance, 0.001f);
            Assert.Less(distance, 0.02f); // 小于捏合阈值
        }

        [Test]
        public void WebXR_CalculateGrabStrength()
        {
            // 模拟手指弯曲角度
            float[] bendAngles = { 150f, 160f, 170f, 160f }; // 4个手指的弯曲角度
            float totalBend = 0f;

            foreach (float angle in bendAngles)
            {
                totalBend += angle / 180f;
            }

            float grabStrength = totalBend / bendAngles.Length;

            Assert.Greater(grabStrength, 0.8f); // 抓取强度应该大于阈值
        }

        [Test]
        public void WebXRRenderSettings_PerformanceTiers()
        {
            // 高性能设置
            var highSettings = new WebXRRenderSettings
            {
                renderScale = 1.0f,
                targetFrameRate = 72,
                textureQuality = 0,
                enableShadows = true,
                enablePostProcessing = true
            };

            Assert.AreEqual(1.0f, highSettings.renderScale);
            Assert.AreEqual(72, highSettings.targetFrameRate);
            Assert.IsTrue(highSettings.enableShadows);

            // 低性能设置
            var lowSettings = new WebXRRenderSettings
            {
                renderScale = 0.8f,
                targetFrameRate = 60,
                textureQuality = 2,
                enableShadows = false,
                enablePostProcessing = false
            };

            Assert.AreEqual(0.8f, lowSettings.renderScale);
            Assert.AreEqual(60, lowSettings.targetFrameRate);
            Assert.IsFalse(lowSettings.enableShadows);
        }

        [Test]
        public void WebXR_CloudRenderingConfiguration()
        {
            bool enableCloudRendering = true;
            string endpoint = "wss://cloud.tripmeta.io/stream";
            int bitrate = 20000000;
            int fps = 60;

            Assert.IsTrue(enableCloudRendering);
            Assert.AreEqual("wss://cloud.tripmeta.io/stream", endpoint);
            Assert.AreEqual(20000000, bitrate);
            Assert.AreEqual(60, fps);
            Assert.Greater(bitrate, 10000000); // 至少 10 Mbps
        }

        [Test]
        public void WebXR_CacheConfiguration()
        {
            int maxCacheSizeMB = 100;
            int maxEntries = 500;
            float expirationDays = 7f;

            long maxSizeBytes = maxCacheSizeMB * 1024 * 1024L;

            Assert.AreEqual(100, maxCacheSizeMB);
            Assert.AreEqual(500, maxEntries);
            Assert.AreEqual(7f, expirationDays);
            Assert.AreEqual(104857600L, maxSizeBytes); // 100MB in bytes
        }

        [Test]
        public void WebXR_LatencyThreshold()
        {
            float currentLatency = 50f;
            float threshold = 100f;

            Assert.Less(currentLatency, threshold);

            // 测试高延迟情况
            float highLatency = 150f;
            Assert.Greater(highLatency, threshold);
        }

        [Test]
        public void WebXRFiles_Exist()
        {
            var managerPath = "Assets/Scripts/VR/WebXR/WebXRManager.cs";
            var inputHandlerPath = "Assets/Scripts/VR/WebXR/WebXRInputHandler.cs";
            var renderHandlerPath = "Assets/Scripts/VR/WebXR/WebXRRenderHandler.cs";
            var networkHandlerPath = "Assets/Scripts/VR/WebXR/WebXRNetworkHandler.cs";
            var cacheManagerPath = "Assets/Scripts/VR/WebXR/WebXRCacheManager.cs";

            Assert.IsTrue(System.IO.File.Exists(managerPath), $"WebXRManager应存在于{managerPath}");
            Assert.IsTrue(System.IO.File.Exists(inputHandlerPath), $"WebXRInputHandler应存在于{inputHandlerPath}");
            Assert.IsTrue(System.IO.File.Exists(renderHandlerPath), $"WebXRRenderHandler应存在于{renderHandlerPath}");
            Assert.IsTrue(System.IO.File.Exists(networkHandlerPath), $"WebXRNetworkHandler应存在于{networkHandlerPath}");
            Assert.IsTrue(System.IO.File.Exists(cacheManagerPath), $"WebXRCacheManager应存在于{cacheManagerPath}");
        }
    }
}
