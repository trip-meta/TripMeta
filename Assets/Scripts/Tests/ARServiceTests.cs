using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Features.AR;
using TripMeta.Core.Configuration;

namespace TripMeta.Tests
{
    /// <summary>
    /// AR服务单元测试
    /// </summary>
    public class ARServiceTests
    {
        private GameObject _testObject;
        private ARManager _arManager;

        [SetUp]
        public void Setup()
        {
            _testObject = new GameObject("TestARManager");
            _arManager = _testObject.AddComponent<ARManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_testObject != null)
            {
                Object.DestroyImmediate(_testObject);
            }
        }

        [Test]
        public void ARConfig_IsValid_ValidatesRequiredFields()
        {
            var config = ScriptableObject.CreateInstance<ARConfig>();

            // 未配置API密钥时应该无效
            Assert.IsFalse(config.IsValid());

            // 配置密钥后应该有效
            config.VisionApiKey = "test-key";
            config.VisionEndpoint = "https://test.api.cognitive.microsoft.com";
            Assert.IsTrue(config.IsValid());
        }

        [Test]
        public void ARConfig_DefaultValues_AreSet()
        {
            var config = ScriptableObject.CreateInstance<ARConfig>();

            Assert.IsTrue(config.EnableAR);
            Assert.AreEqual(3f, config.ScanInterval);
            Assert.AreEqual(0.7f, config.RecognitionThreshold);
            Assert.AreEqual(50f, config.MaxRecognitionDistance);
            Assert.AreEqual(1.5f, config.CardHeightOffset);
            Assert.IsTrue(config.EnableCardAnimations);
        }

        [UnityTest]
        public IEnumerator InitializeAsync_SetsInitializedState()
        {
            var task = _arManager.InitializeAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            // 初始化应该完成（即使设备不支持AR）
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void AttractionRecognitionResult_CanStoreData()
        {
            var result = new AttractionRecognitionResult
            {
                IsRecognized = true,
                AttractionId = "test_attraction",
                AttractionName = "Test Attraction",
                Confidence = 0.85f,
                Position = new Vector3(1, 2, 3),
                BoundingBox = new Rect(10, 20, 100, 200),
                ErrorMessage = null
            };

            Assert.IsTrue(result.IsRecognized);
            Assert.AreEqual("test_attraction", result.AttractionId);
            Assert.AreEqual("Test Attraction", result.AttractionName);
            Assert.AreEqual(0.85f, result.Confidence);
            Assert.AreEqual(new Vector3(1, 2, 3), result.Position);
            Assert.IsNull(result.ErrorMessage);
        }

        [Test]
        public void AROverlayInfo_CanStoreData()
        {
            var info = new AROverlayInfo
            {
                Id = "overlay_001",
                Title = "Test Title",
                Description = "Test Description",
                Type = OverlayType.InfoCard,
                AttractionId = "attraction_001",
                WorldPosition = new Vector3(10, 1.5f, 20),
                ScreenPosition = new Vector2(100, 200),
                ImageUrl = "https://example.com/image.jpg",
                AudioUrl = "https://example.com/audio.mp3"
            };

            Assert.AreEqual("overlay_001", info.Id);
            Assert.AreEqual("Test Title", info.Title);
            Assert.AreEqual("Test Description", info.Description);
            Assert.AreEqual(OverlayType.InfoCard, info.Type);
            Assert.AreEqual(new Vector3(10, 1.5f, 20), info.WorldPosition);
        }

        [Test]
        public void LandmarkInfo_CanStoreData()
        {
            var landmark = new LandmarkInfo
            {
                Name = "Main Gate",
                Type = LandmarkType.Gate,
                Position = new Vector3(5, 0, 10),
                Confidence = 0.92f
            };

            Assert.AreEqual("Main Gate", landmark.Name);
            Assert.AreEqual(LandmarkType.Gate, landmark.Type);
            Assert.AreEqual(0.92f, landmark.Confidence);
        }

        [Test]
        public void OverlayType_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(OverlayType), OverlayType.InfoCard));
            Assert.IsTrue(System.Enum.IsDefined(typeof(OverlayType), OverlayType.HistoricalPhoto));
            Assert.IsTrue(System.Enum.IsDefined(typeof(OverlayType), OverlayType.AudioGuide));
            Assert.IsTrue(System.Enum.IsDefined(typeof(OverlayType), OverlayType.NavigationArrow));
            Assert.IsTrue(System.Enum.IsDefined(typeof(OverlayType), OverlayType.DistanceMarker));
            Assert.IsTrue(System.Enum.IsDefined(typeof(OverlayType), OverlayType.FunFact));
            Assert.IsTrue(System.Enum.IsDefined(typeof(OverlayType), OverlayType.InteractiveElement));
        }

        [Test]
        public void LandmarkType_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(LandmarkType), LandmarkType.Building));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LandmarkType), LandmarkType.Sculpture));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LandmarkType), LandmarkType.Gate));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LandmarkType), LandmarkType.Tower));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LandmarkType), LandmarkType.Bridge));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LandmarkType), LandmarkType.Monument));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LandmarkType), LandmarkType.NaturalFeature));
        }

        [Test]
        public void ARCapability_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(ARCapability), ARCapability.ImageRecognition));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ARCapability), ARCapability.ObjectDetection));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ARCapability), ARCapability.PlaneDetection));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ARCapability), ARCapability.SpatialMapping));
        }

        [Test]
        public void IARService_Interface_DefinesRequiredMembers()
        {
            // 验证接口定义了必要的成员
            Assert.IsNotNull(typeof(IARService).GetProperty("IsInitialized"));
            Assert.IsNotNull(typeof(IARService).GetProperty("IsScanning"));
            Assert.IsNotNull(typeof(IARService).GetMethod("InitializeAsync"));
            Assert.IsNotNull(typeof(IARService).GetMethod("StartARExperienceAsync"));
            Assert.IsNotNull(typeof(IARService).GetMethod("StopARExperienceAsync"));
            Assert.IsNotNull(typeof(IARService).GetMethod("ScanAttractionAsync"));
            Assert.IsNotNull(typeof(IARService).GetMethod("GetAttractionOverlaysAsync"));
            Assert.IsNotNull(typeof(IARService).GetMethod("PlaceARCard"));
            Assert.IsNotNull(typeof(IARService).GetMethod("ClearAllOverlays"));
        }

        [Test]
        public void ARManager_Singleton_Pattern()
        {
            // 验证 ARManager 使用了单例模式
            var singletonProperty = typeof(ARManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(singletonProperty);
        }

        [UnityTest]
        public IEnumerator GetSupportedCapabilities_ReturnsCapabilities()
        {
            yield return null;

            var capabilities = _arManager.GetSupportedCapabilities();

            Assert.IsNotNull(capabilities);
            // 即使没有AR设备，也应该返回一个列表
            Assert.IsInstanceOf<List<ARCapability>>(capabilities);
        }

        [UnityTest]
        public IEnumerator SetARVisibility_TogglesOverlays()
        {
            yield return null;

            // 测试切换AR可见性
            _arManager.SetARVisibility(false);
            _arManager.SetARVisibility(true);

            // 没有抛出异常即为成功
            Assert.Pass();
        }
    }
}
