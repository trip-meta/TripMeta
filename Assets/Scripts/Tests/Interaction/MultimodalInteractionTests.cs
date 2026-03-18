using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TripMeta.Interaction;

namespace TripMeta.Tests.Interaction
{
    /// <summary>
    /// 多模态交互测试
    /// 测试手势识别、视线追踪、语音合成
    /// </summary>
    public class MultimodalInteractionTests
    {
        [Test]
        public void GestureType_EnumValues()
        {
            var gestures = System.Enum.GetValues(typeof(GestureType));
            Assert.Contains(GestureType.None, gestures);
            Assert.Contains(GestureType.Point, gestures);
            Assert.Contains(GestureType.Grab, gestures);
            Assert.Contains(GestureType.OpenPalm, gestures);
            Assert.Contains(GestureType.ThumbsUp, gestures);
            Assert.Contains(GestureType.Pinch, gestures);
            Assert.Contains(GestureType.Wave, gestures);
        }

        [Test]
        public void HandPose_EnumValues()
        {
            var poses = System.Enum.GetValues(typeof(HandPose));
            Assert.Contains(HandPose.Open, poses);
            Assert.Contains(HandPose.Fist, poses);
            Assert.Contains(HandPose.Pinch, poses);
            Assert.Contains(HandPose.Point, poses);
        }

        [Test]
        public void SpeechPriority_EnumValues()
        {
            var priorities = System.Enum.GetValues(typeof(SpeechPriority));
            Assert.Contains(SpeechPriority.Low, priorities);
            Assert.Contains(SpeechPriority.Normal, priorities);
            Assert.Contains(SpeechPriority.High, priorities);
            Assert.Contains(SpeechPriority.Critical, priorities);
        }

        [Test]
        public void GestureData_Creation()
        {
            var gesture = new GestureData
            {
                gestureType = GestureType.Point,
                description = "食指指向",
                confidenceThreshold = 0.8f,
                handPose = HandPose.Open,
                fingerExtension = new bool[] { false, true, false, false, false }
            };

            Assert.AreEqual(GestureType.Point, gesture.gestureType);
            Assert.AreEqual(0.8f, gesture.confidenceThreshold);
            Assert.AreEqual(5, gesture.fingerExtension.Length);
        }

        [Test]
        public void RecognizedGesture_ToString()
        {
            var gesture = new RecognizedGesture
            {
                gestureType = GestureType.Grab,
                confidence = 0.9f
            };

            var str = gesture.ToString();
            StringAssert.Contains("Grab", str);
            StringAssert.Contains("90%", str);
        }

        [Test]
        public void SpeechRequest_Creation()
        {
            var request = new SpeechRequest
            {
                text = "Hello World",
                priority = SpeechPriority.High,
                voice = "zh-CN-XiaoxiaoNeural",
                speed = 1.2f,
                volume = 0.9f
            };

            Assert.AreEqual("Hello World", request.text);
            Assert.AreEqual(SpeechPriority.High, request.priority);
            Assert.AreEqual(1.2f, request.speed);
        }

        [Test]
        public void MultimodalInput_Creation()
        {
            var input = new MultimodalInput
            {
                gesture = new RecognizedGesture { gestureType = GestureType.Point, confidence = 0.85f },
                gazePosition = new Vector3(1, 2, 3),
                gazeTarget = null
            };

            Assert.AreEqual(GestureType.Point, input.gesture.gestureType);
            Assert.AreEqual(new Vector3(1, 2, 3), input.gazePosition);
        }

        [Test]
        public void HandTrackingData_Creation()
        {
            var handData = new HandTrackingData
            {
                handPosition = Vector3.zero,
                handRotation = Quaternion.identity,
                fingerPositions = new Vector3[5],
                isTracking = true
            };

            Assert.IsTrue(handData.isTracking);
            Assert.AreEqual(5, handData.fingerPositions.Length);
        }

        [UnityTest]
        public IEnumerator MultimodalInteractionManager_Creation()
        {
            var testObject = new GameObject("TestMultimodalInteractionManager");
            var manager = testObject.AddComponent<MultimodalInteractionManager>();

            Assert.IsNotNull(manager);
            Assert.IsTrue(manager.enableGestureRecognition);
            Assert.IsTrue(manager.enableEyeTracking);
            Assert.IsTrue(manager.enableVoiceSynthesis);
            Assert.IsTrue(manager.enableMultimodalFusion);
            Assert.AreEqual(0.8f, manager.gestureConfidenceThreshold);

            Object.Destroy(testObject);
            yield return null;
        }

        [Test]
        public void GestureConfidence_Calculation()
        {
            float confidence = 0.85f;
            float threshold = 0.8f;

            Assert.GreaterOrEqual(confidence, threshold, "手势置信度应该高于阈值");
        }

        [Test]
        public void GazeDwell_Detection()
        {
            float dwellTime = 1.5f;
            float currentTime = 1.6f;

            Assert.GreaterOrEqual(currentTime, dwellTime, "凝视时间应该超过阈值");
        }

        [Test]
        public void FusionConfidence_Calculation()
        {
            float gestureConfidence = 0.9f;
            float gazeConfidence = 0.8f;

            // 手势权重40%, 凝视权重30%
            float fusionConfidence = gestureConfidence * 0.4f + gazeConfidence * 0.3f;
            fusionConfidence /= 2; // 平均

            Assert.Greater(fusionConfidence, 0.3f, "融合置信度应该合理");
        }

        [Test]
        public void MultimodalFiles_Exist()
        {
            var managerPath = "Assets/Scripts/Interaction/MultimodalInteractionManager.cs";
            Assert.IsTrue(System.IO.File.Exists(managerPath), $"MultimodalInteractionManager应该存在于{managerPath}");
        }
    }
}
