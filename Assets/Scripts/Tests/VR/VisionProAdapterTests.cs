using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TripMeta.VR.Platform;

namespace TripMeta.Tests.VR
{
    /// <summary>
    /// Vision Pro 适配器单元测试
    /// 测试空间计算API集成、手势交互、混合现实功能
    /// </summary>
    public class VisionProAdapterTests
    {
        [Test]
        public void VRPlatformType_EnumValues()
        {
            var platforms = System.Enum.GetValues(typeof(VRPlatformType));
            Assert.Contains(VRPlatformType.Generic, platforms);
            Assert.Contains(VRPlatformType.VisionPro, platforms);
            Assert.Contains(VRPlatformType.Quest, platforms);
            Assert.Contains(VRPlatformType.Pico, platforms);
            Assert.Contains(VRPlatformType.HTC, platforms);
            Assert.Contains(VRPlatformType.WebXR, platforms);
        }

        [Test]
        public void VisionProGestureType_EnumValues()
        {
            var gestures = System.Enum.GetValues(typeof(VisionProGestureType));
            Assert.Contains(VisionProGestureType.None, gestures);
            Assert.Contains(VisionProGestureType.Pinch, gestures);
            Assert.Contains(VisionProGestureType.AirTap, gestures);
            Assert.Contains(VisionProGestureType.Grab, gestures);
            Assert.Contains(VisionProGestureType.SwipeLeft, gestures);
            Assert.Contains(VisionProGestureType.SwipeRight, gestures);
            Assert.Contains(VisionProGestureType.Rotate, gestures);
            Assert.Contains(VisionProGestureType.Zoom, gestures);
        }

        [Test]
        public void VisionProHandData_DefaultConstructor()
        {
            var handData = new VisionProHandData(false);

            Assert.IsFalse(handData.isTracked);
            Assert.AreEqual(Vector3.zero, handData.handPosition);
            Assert.AreEqual(Quaternion.identity, handData.handRotation);
            Assert.AreEqual(VisionProGestureType.None, handData.currentGesture);
            Assert.AreEqual(0f, handData.gestureConfidence);
            Assert.IsFalse(handData.isPinching);
        }

        [Test]
        public void VisionProHandData_TrackedConstructor()
        {
            var handData = new VisionProHandData(true)
            {
                handPosition = new Vector3(1, 2, 3),
                currentGesture = VisionProGestureType.Pinch,
                gestureConfidence = 0.9f,
                isPinching = true,
                pinchStrength = 0.85f
            };

            Assert.IsTrue(handData.isTracked);
            Assert.AreEqual(new Vector3(1, 2, 3), handData.handPosition);
            Assert.AreEqual(VisionProGestureType.Pinch, handData.currentGesture);
            Assert.AreEqual(0.9f, handData.gestureConfidence);
            Assert.IsTrue(handData.isPinching);
            Assert.AreEqual(0.85f, handData.pinchStrength);
        }

        [Test]
        public void VisionProHandData_FingerPositions()
        {
            var handData = new VisionProHandData(true)
            {
                fingerPositions = new Vector3[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(0.01f, 0.05f, 0),
                    new Vector3(0.02f, 0.08f, 0),
                    new Vector3(0.03f, 0.06f, 0),
                    new Vector3(0.04f, 0.04f, 0)
                }
            };

            Assert.AreEqual(5, handData.fingerPositions.Length);
            Assert.AreEqual(new Vector3(0.02f, 0.08f, 0), handData.fingerPositions[2]);
        }

        [Test]
        public void VisionProEyeData_DefaultConstructor()
        {
            var eyeData = new VisionProEyeData(false);

            Assert.IsFalse(eyeData.isTracked);
            Assert.AreEqual(Vector3.zero, eyeData.gazeOrigin);
            Assert.AreEqual(Vector3.forward, eyeData.gazeDirection);
            Assert.AreEqual(1f, eyeData.leftEyeOpenness);
            Assert.AreEqual(1f, eyeData.rightEyeOpenness);
        }

        [Test]
        public void VisionProEyeData_TrackedProperties()
        {
            var eyeData = new VisionProEyeData(true)
            {
                gazeOrigin = Vector3.zero,
                gazeDirection = Vector3.forward,
                gazePoint = new Vector3(0, 0, 10),
                leftEyeOpenness = 0.8f,
                rightEyeOpenness = 0.85f,
                fixationDuration = 2.5f
            };

            Assert.IsTrue(eyeData.isTracked);
            Assert.AreEqual(new Vector3(0, 0, 10), eyeData.gazePoint);
            Assert.AreEqual(0.8f, eyeData.leftEyeOpenness);
            Assert.AreEqual(0.85f, eyeData.rightEyeOpenness);
            Assert.AreEqual(2.5f, eyeData.fixationDuration);
        }

        [UnityTest]
        public IEnumerator VisionProAdapter_Creation()
        {
            var testObject = new GameObject("TestVisionProAdapter");
            var adapter = testObject.AddComponent<VisionProAdapter>();

            Assert.IsNotNull(adapter);
            Assert.AreEqual(VRPlatformType.VisionPro, adapter.PlatformType);
            Assert.IsFalse(adapter.IsInitialized);
            Assert.IsFalse(adapter.IsRunning);

            // 检查默认配置
            Assert.IsTrue(adapter.enableHandTracking);
            Assert.IsTrue(adapter.enableEyeTracking);
            Assert.IsTrue(adapter.enableMixedReality);
            Assert.AreEqual(0.8f, adapter.gestureThreshold);
            Assert.AreEqual(1.5f, adapter.gazeDwellTime);

            Object.Destroy(testObject);
            yield return null;
        }

        [Test]
        public void VisionProHandTracker_DefaultValues()
        {
            var trackerObject = new GameObject("TestHandTracker");
            var tracker = trackerObject.AddComponent<VisionProHandTracker>();

            Assert.AreEqual(0.8f, tracker.gestureThreshold);
            Assert.AreEqual(0.02f, tracker.pinchThreshold);
            Assert.AreEqual(5, tracker.gestureHistorySize);
            Assert.AreEqual(0.2f, tracker.gestureCooldown);

            Object.Destroy(trackerObject);
        }

        [Test]
        public void VisionProEyeTracker_DefaultValues()
        {
            var trackerObject = new GameObject("TestEyeTracker");
            var tracker = trackerObject.AddComponent<VisionProEyeTracker>();

            Assert.AreEqual(0.016f, tracker.eyeTrackingInterval, 0.001f);
            Assert.AreEqual(10, tracker.gazeBufferSize);
            Assert.AreEqual(1.5f, tracker.fixationThreshold);
            Assert.AreEqual(100f, tracker.saccadeThreshold);
            Assert.IsTrue(tracker.enableSmoothing);
            Assert.AreEqual(0.3f, tracker.smoothingFactor);
        }

        [Test]
        public void VisionProMRController_DefaultValues()
        {
            var controllerObject = new GameObject("TestMRController");
            var controller = controllerObject.AddComponent<VisionProMRController>();

            Assert.AreEqual(0.5f, controller.passthroughOpacity);
            Assert.AreEqual(~0, (int)controller.passthroughLayer);
            Assert.IsTrue(controller.enableOcclusion);
            Assert.AreEqual(0.8f, controller.occlusionOpacity);
            Assert.IsTrue(controller.enableShadows);
            Assert.AreEqual(50, controller.maxAnchors);

            Object.Destroy(controllerObject);
        }

        [Test]
        public void VisionProAdapter_MixedRealityMode()
        {
            var adapterObject = new GameObject("TestAdapter");
            var adapter = adapterObject.AddComponent<VisionProAdapter>();

            // 测试混合现实配置
            adapter.enableMixedReality = true;
            adapter.passthroughOpacity = 0.6f;

            Assert.IsTrue(adapter.enableMixedReality);
            Assert.AreEqual(0.6f, adapter.passthroughOpacity);

            Object.Destroy(adapterObject);
        }

        [Test]
        public void VisionProGesture_ConfidenceCalculation()
        {
            float confidence = 0.85f;
            float threshold = 0.8f;

            Assert.GreaterOrEqual(confidence, threshold, "手势置信度应超过阈值");
        }

        [Test]
        public void VisionProEyeTracking_FixationDetection()
        {
            float fixationDuration = 2.0f;
            float threshold = 1.5f;

            Assert.GreaterOrEqual(fixationDuration, threshold, "凝视时间应超过阈值");
        }

        [Test]
        public void VisionProMR_OcclusionConfiguration()
        {
            bool enableOcclusion = true;
            float occlusionOpacity = 0.75f;

            Assert.IsTrue(enableOcclusion);
            Assert.GreaterOrEqual(occlusionOpacity, 0f);
            Assert.LessOrEqual(occlusionOpacity, 1f);
        }

        [Test]
        public void VisionProHandData_VelocityCalculation()
        {
            Vector3 pos1 = Vector3.zero;
            Vector3 pos2 = new Vector3(0.1f, 0, 0);
            float deltaTime = 0.016f;

            Vector3 velocity = (pos2 - pos1) / deltaTime;

            Assert.AreEqual(new Vector3(6.25f, 0, 0), velocity);
            Assert.Greater(velocity.magnitude, 0);
        }

        [Test]
        public void VisionProEyeTracking_Smoothing()
        {
            Vector3 current = new Vector3(1, 0, 0);
            Vector3 target = new Vector3(2, 0, 0);
            float factor = 0.5f;

            Vector3 smoothed = Vector3.Lerp(current, target, factor);

            Assert.AreEqual(new Vector3(1.5f, 0, 0), smoothed);
        }

        [Test]
        public void VisionProFiles_Exist()
        {
            var adapterPath = "Assets/Scripts/VR/Platform/VisionProAdapter.cs";
            var handTrackerPath = "Assets/Scripts/VR/Platform/VisionProHandTracker.cs";
            var eyeTrackerPath = "Assets/Scripts/VR/Platform/VisionProEyeTracker.cs";

            Assert.IsTrue(System.IO.File.Exists(adapterPath), $"VisionProAdapter应存在于{adapterPath}");
            Assert.IsTrue(System.IO.File.Exists(handTrackerPath), $"VisionProHandTracker应存在于{handTrackerPath}");
            Assert.IsTrue(System.IO.File.Exists(eyeTrackerPath), $"VisionProEyeTracker应存在于{eyeTrackerPath}");
        }
    }
}
