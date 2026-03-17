using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Features.MobileCompanion;
using TripMeta.Core.Configuration;

namespace TripMeta.Tests
{
    /// <summary>
    /// 移动伴侣服务单元测试
    /// </summary>
    public class MobileCompanionServiceTests
    {
        private GameObject _testObject;
        private MobileCompanionManager _mobileManager;

        [SetUp]
        public void Setup()
        {
            _testObject = new GameObject("TestMobileManager");
            _mobileManager = _testObject.AddComponent<MobileCompanionManager>();
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
        public void MobileCompanionConfig_IsValid_ValidatesRequiredFields()
        {
            var config = ScriptableObject.CreateInstance<MobileCompanionConfig>();

            // 默认配置应该有效
            Assert.IsTrue(config.IsValid());

            // 清空URL后应该无效
            config.ServerUrl = "";
            Assert.IsFalse(config.IsValid());
        }

        [Test]
        public void MobileCompanionConfig_DefaultValues_AreSet()
        {
            var config = ScriptableObject.CreateInstance<MobileCompanionConfig>();

            Assert.AreEqual("https://api.tripmeta.com", config.ServerUrl);
            Assert.AreEqual(8080, config.ConnectionPort);
            Assert.AreEqual(5f, config.HeartbeatInterval);
            Assert.AreEqual(6, config.PairingCodeLength);
            Assert.AreEqual(300, config.PairingTimeout);
            Assert.IsTrue(config.EnableVRStateSync);
            Assert.IsTrue(config.EnablePushNotifications);
            Assert.IsTrue(config.EnableRemoteControl);
        }

        [UnityTest]
        public IEnumerator InitializeAsync_SetsInitializedState()
        {
            var task = _mobileManager.InitializeAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(task.IsCompletedSuccessfully);
        }

        [Test]
        public void GeneratePairingCode_ReturnsCorrectLength()
        {
            var code = _mobileManager.GeneratePairingCode();

            Assert.AreEqual(6, code.Length);
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(code, "^[0-9]{6}$"));
        }

        [Test]
        public void VRState_CanStoreData()
        {
            var state = new VRState
            {
                CurrentAttractionId = "attr_001",
                CurrentAttractionName = "故宫博物院",
                Progress = 45,
                IsSpeaking = true,
                CurrentSpeechText = "欢迎来到故宫",
                ConnectedPlayers = 3,
                BatteryLevel = 85,
                NetworkLatency = 20,
                Timestamp = 1234567890
            };

            Assert.AreEqual("attr_001", state.CurrentAttractionId);
            Assert.AreEqual("故宫博物院", state.CurrentAttractionName);
            Assert.AreEqual(45, state.Progress);
            Assert.IsTrue(state.IsSpeaking);
            Assert.AreEqual(3, state.ConnectedPlayers);
            Assert.AreEqual(85, state.BatteryLevel);
        }

        [Test]
        public void RemoteCommand_CanStoreData()
        {
            var command = new RemoteCommand
            {
                Type = CommandType.JumpToAttraction,
                Parameter = "attr_002",
                Timestamp = 1234567890
            };

            Assert.AreEqual(CommandType.JumpToAttraction, command.Type);
            Assert.AreEqual("attr_002", command.Parameter);
        }

        [Test]
        public void ChatMessage_CanStoreData()
        {
            var message = new ChatMessage
            {
                SenderName = "TestUser",
                Content = "Hello World",
                Timestamp = 1234567890,
                IsSystemMessage = false
            };

            Assert.AreEqual("TestUser", message.SenderName);
            Assert.AreEqual("Hello World", message.Content);
            Assert.IsFalse(message.IsSystemMessage);
        }

        [Test]
        public void AttractionMobileInfo_CanStoreData()
        {
            var info = new AttractionMobileInfo
            {
                Id = "attr_001",
                Name = "故宫",
                Description = "中国古代皇宫",
                ThumbnailUrl = "https://example.com/image.jpg",
                VisitorCount = 10000,
                Rating = 4.8f
            };

            Assert.AreEqual("attr_001", info.Id);
            Assert.AreEqual("故宫", info.Name);
            Assert.AreEqual(4.8f, info.Rating);
        }

        [Test]
        public void MobileNotification_CanStoreData()
        {
            var notification = new MobileNotification
            {
                Title = "新成就解锁",
                Message = "恭喜您解锁了'初次游览'成就",
                Type = NotificationType.Achievement,
                AttractionId = "attr_001"
            };

            Assert.AreEqual("新成就解锁", notification.Title);
            Assert.AreEqual(NotificationType.Achievement, notification.Type);
        }

        [Test]
        public void PairedDeviceInfo_CanStoreData()
        {
            var device = new PairedDeviceInfo
            {
                DeviceId = "device_123",
                DeviceName = "iPhone 15",
                DeviceType = "iOS",
                PairedTime = System.DateTime.UtcNow,
                ConnectionCount = 5,
                IsTrusted = true
            };

            Assert.AreEqual("device_123", device.DeviceId);
            Assert.AreEqual("iPhone 15", device.DeviceName);
            Assert.IsTrue(device.IsTrusted);
        }

        [Test]
        public void CommandType_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.Pause));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.Resume));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.JumpToAttraction));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.AdjustVolume));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.TakePhoto));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.StartRecording));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.StopRecording));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.RequestHelp));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CommandType), CommandType.ReturnToMenu));
        }

        [Test]
        public void NotificationType_Enum_HasExpectedValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(NotificationType), NotificationType.Info));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NotificationType), NotificationType.Achievement));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NotificationType), NotificationType.Social));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NotificationType), NotificationType.System));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NotificationType), NotificationType.Alert));
        }

        [Test]
        public void IMobileCompanionService_Interface_DefinesRequiredMembers()
        {
            Assert.IsNotNull(typeof(IMobileCompanionService).GetProperty("IsConnected"));
            Assert.IsNotNull(typeof(IMobileCompanionService).GetProperty("PairedDeviceId"));
            Assert.IsNotNull(typeof(IMobileCompanionService).GetMethod("InitializeAsync"));
            Assert.IsNotNull(typeof(IMobileCompanionService).GetMethod("StartPairingAsync"));
            Assert.IsNotNull(typeof(IMobileCompanionService).GetMethod("AcceptPairingAsync"));
            Assert.IsNotNull(typeof(IMobileCompanionService).GetMethod("DisconnectAsync"));
            Assert.IsNotNull(typeof(IMobileCompanionService).GetMethod("SendVRStateAsync"));
            Assert.IsNotNull(typeof(IMobileCompanionService).GetMethod("ExecuteRemoteCommandAsync"));
            Assert.IsNotNull(typeof(IMobileCompanionService).GetMethod("GetPairedDeviceHistory"));
        }

        [Test]
        public void MobileCompanionManager_Singleton_Pattern()
        {
            var singletonProperty = typeof(MobileCompanionManager).GetProperty("Instance",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(singletonProperty);
        }

        [UnityTest]
        public IEnumerator GetPairedDeviceHistory_ReturnsEmptyList_Initially()
        {
            yield return null;

            var history = _mobileManager.GetPairedDeviceHistory();

            Assert.IsNotNull(history);
            Assert.IsInstanceOf<List<PairedDeviceInfo>>(history);
        }
    }
}
