using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Features.Multiplayer;
using TripMeta.Core.Configuration;

namespace TripMeta.Tests
{
    /// <summary>
    /// 多人游戏服务单元测试
    /// </summary>
    public class MultiplayerServiceTests
    {
        private GameObject _testObject;
        private MultiplayerManager _multiplayerManager;

        [SetUp]
        public void Setup()
        {
            _testObject = new GameObject("TestMultiplayerManager");
            _multiplayerManager = _testObject.AddComponent<MultiplayerManager>();
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
        public void MultiplayerConfig_IsValid_ValidatesRequiredFields()
        {
            var config = ScriptableObject.CreateInstance<MultiplayerConfig>();

            // 默认配置应该有效
            Assert.IsTrue(config.IsValid());

            // 清空地址后应该无效
            config.DefaultServerAddress = "";
            Assert.IsFalse(config.IsValid());
        }

        [Test]
        public void MultiplayerConfig_DefaultValues_AreSet()
        {
            var config = ScriptableObject.CreateInstance<MultiplayerConfig>();

            Assert.AreEqual("127.0.0.1", config.DefaultServerAddress);
            Assert.AreEqual(7777, config.DefaultPort);
            Assert.AreEqual(8, config.MaxConnections);
            Assert.AreEqual(20, config.SyncRate);
            Assert.IsTrue(config.EnableVoiceChat);
            Assert.IsTrue(config.SyncTourGuideState);
        }

        [UnityTest]
        public IEnumerator InitializeAsync_SetsInitializedState()
        {
            var task = _multiplayerManager.InitializeAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            // 初始化应该成功完成
            Assert.IsTrue(task.IsCompletedSuccessfully);
        }

        [Test]
        public void PlayerInfo_Struct_CanStoreData()
        {
            var playerInfo = new PlayerInfo
            {
                ClientId = 123,
                PlayerName = "TestPlayer",
                IsHost = true,
                Position = new Vector3(1, 2, 3),
                Rotation = Quaternion.Euler(0, 90, 0),
                Status = PlayerStatus.Connected
            };

            Assert.AreEqual(123ul, playerInfo.ClientId);
            Assert.AreEqual("TestPlayer", playerInfo.PlayerName);
            Assert.IsTrue(playerInfo.IsHost);
            Assert.AreEqual(new Vector3(1, 2, 3), playerInfo.Position);
            Assert.AreEqual(PlayerStatus.Connected, playerInfo.Status);
        }

        [Test]
        public void TourGuideSyncState_NetworkSerializable()
        {
            var state = new TourGuideSyncState
            {
                CurrentAttractionIndex = 5,
                CurrentGuideText = "欢迎来到故宫",
                GuideProgress = 0.75f,
                IsSpeaking = true,
                GuidePosition = new Vector3(10, 0, 20)
            };

            Assert.AreEqual(5, state.CurrentAttractionIndex);
            Assert.AreEqual("欢迎来到故宫", state.CurrentGuideText);
            Assert.AreEqual(0.75f, state.GuideProgress);
            Assert.IsTrue(state.IsSpeaking);
            Assert.AreEqual(new Vector3(10, 0, 20), state.GuidePosition);
        }

        [Test]
        public void IMultiplayerService_Interface_DefinesRequiredMembers()
        {
            // 验证接口定义了必要的成员
            Assert.IsNotNull(typeof(IMultiplayerService).GetProperty("IsConnected"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetProperty("IsHost"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetProperty("ConnectedClientCount"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetProperty("LocalClientId"));

            Assert.IsNotNull(typeof(IMultiplayerService).GetMethod("InitializeAsync"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetMethod("CreateRoomAsync"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetMethod("JoinRoomAsync"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetMethod("LeaveRoomAsync"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetMethod("GetConnectedPlayers"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetMethod("SendVoiceChatAsync"));
            Assert.IsNotNull(typeof(IMultiplayerService).GetMethod("SyncTourGuideStateAsync"));
        }

        [Test]
        public void PlayerStatus_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerStatus), PlayerStatus.Connected));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerStatus), PlayerStatus.InTour));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerStatus), PlayerStatus.Speaking));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerStatus), PlayerStatus.AFK));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerStatus), PlayerStatus.Disconnected));
        }

        [Test]
        public void VoiceQuality_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceQuality), VoiceQuality.Low));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceQuality), VoiceQuality.Medium));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceQuality), VoiceQuality.High));
            Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceQuality), VoiceQuality.Ultra));
        }

        [UnityTest]
        public IEnumerator GetConnectedPlayers_ReturnsEmptyList_WhenNotConnected()
        {
            yield return null;

            var players = _multiplayerManager.GetConnectedPlayers();

            Assert.IsNotNull(players);
            Assert.IsEmpty(players);
        }

        [Test]
        public void NetworkVRPlayer_HasRequiredComponents()
        {
            var go = new GameObject("TestVRPlayer");
            var networkPlayer = go.AddComponent<NetworkVRPlayer>();

            Assert.IsNotNull(networkPlayer);
            Assert.IsNotNull(typeof(NetworkVRPlayer).GetField("headTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            Assert.IsNotNull(typeof(NetworkVRPlayer).GetField("leftHandTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            Assert.IsNotNull(typeof(NetworkVRPlayer).GetField("rightHandTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void MultiplayerManager_Singleton_Pattern()
        {
            // 验证 MultiplayerManager 使用了单例模式
            var singletonProperty = typeof(MultiplayerManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(singletonProperty);
        }
    }
}
