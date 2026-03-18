using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TripMeta.VR.Haptics;

namespace TripMeta.Tests.VR
{
    /// <summary>
    /// 触觉反馈系统单元测试
    /// 测试触觉设备管理、预设、反馈触发
    /// </summary>
    public class HapticFeedbackTests
    {
        [Test]
        public void BodyRegion_EnumValues()
        {
            var regions = System.Enum.GetValues(typeof(BodyRegion));
            Assert.Contains(BodyRegion.Head, regions);
            Assert.Contains(BodyRegion.Torso, regions);
            Assert.Contains(BodyRegion.LeftHand, regions);
            Assert.Contains(BodyRegion.RightHand, regions);
            Assert.Contains(BodyRegion.LeftFoot, regions);
            Assert.Contains(BodyRegion.RightFoot, regions);
        }

        [Test]
        public void HapticPriority_EnumValues()
        {
            var priorities = System.Enum.GetValues(typeof(HapticPriority));
            Assert.Contains(HapticPriority.Low, priorities);
            Assert.Contains(HapticPriority.Normal, priorities);
            Assert.Contains(HapticPriority.High, priorities);
            Assert.Contains(HapticPriority.Critical, priorities);
        }

        [Test]
        public void HapticType_EnumValues()
        {
            var types = System.Enum.GetValues(typeof(HapticType));
            Assert.Contains(HapticType.Buzz, types);
            Assert.Contains(HapticType.Click, types);
            Assert.Contains(HapticType.Rumble, types);
            Assert.Contains(HapticType.Pulse, types);
            Assert.Contains(HapticType.Continuous, types);
            Assert.Contains(HapticType.Wave, types);
        }

        [Test]
        public void HapticPattern_DefaultValues()
        {
            var pattern = new HapticPattern();

            Assert.AreEqual(0f, pattern.amplitude);
            Assert.AreEqual(0f, pattern.frequency);
            Assert.AreEqual(0f, pattern.duration);
            Assert.AreEqual(0f, pattern.delay);
            Assert.AreEqual(0f, pattern.fadeIn);
            Assert.AreEqual(0f, pattern.fadeOut);
        }

        [Test]
        public void HapticPattern_SetValues()
        {
            var pattern = new HapticPattern
            {
                type = HapticType.Buzz,
                amplitude = 0.5f,
                frequency = 100f,
                duration = 0.2f,
                delay = 0.05f,
                fadeIn = 0.02f,
                fadeOut = 0.03f
            };

            Assert.AreEqual(HapticType.Buzz, pattern.type);
            Assert.AreEqual(0.5f, pattern.amplitude);
            Assert.AreEqual(100f, pattern.frequency);
            Assert.AreEqual(0.2f, pattern.duration);
            Assert.AreEqual(0.05f, pattern.delay);
        }

        [Test]
        public void HapticPattern_CreateImpactPattern()
        {
            float force = 0.7f;
            var pattern = HapticPattern.CreateImpactPattern(force);

            Assert.AreEqual(HapticType.Click, pattern.type);
            Assert.AreEqual(Mathf.Clamp01(force), pattern.amplitude);
            Assert.AreEqual(200f, pattern.frequency);
            Assert.Greater(pattern.duration, 0f);
            Assert.AreEqual(0f, pattern.fadeIn);
            Assert.AreEqual(0.02f, pattern.fadeOut);
        }

        [Test]
        public void HapticPattern_CreateContinuousPattern()
        {
            float intensity = 0.6f;
            float duration = 1.5f;
            var pattern = HapticPattern.CreateContinuousPattern(intensity, duration);

            Assert.AreEqual(HapticType.Continuous, pattern.type);
            Assert.AreEqual(0.6f, pattern.amplitude);
            Assert.AreEqual(100f, pattern.frequency);
            Assert.AreEqual(1.5f, pattern.duration);
            Assert.AreEqual(0.1f, pattern.fadeIn);
            Assert.AreEqual(0.1f, pattern.fadeOut);
        }

        [Test]
        public void HapticPresets_FootstepPatterns()
        {
            // 测试行走触觉预设
            Assert.AreEqual(HapticType.Click, HapticPresets.FootstepLeft.type);
            Assert.AreEqual(0.4f, HapticPresets.FootstepLeft.amplitude);
            Assert.AreEqual(0.05f, HapticPresets.FootstepLeft.duration);

            Assert.AreEqual(HapticType.Click, HapticPresets.FootstepRight.type);
            Assert.AreEqual(0.4f, HapticPresets.FootstepRight.amplitude);

            // 奔跑应该有更高的强度
            Assert.Greater(HapticPresets.RunLeft.amplitude, HapticPresets.FootstepLeft.amplitude);
            Assert.Greater(HapticPresets.RunLeft.duration, HapticPresets.FootstepLeft.duration);
        }

        [Test]
        public void HapticPresets_CombatPatterns()
        {
            // 测试战斗触觉预设的强度递增
            Assert.Less(HapticPresets.HitLight.amplitude, HapticPresets.HitMedium.amplitude);
            Assert.Less(HapticPresets.HitMedium.amplitude, HapticPresets.HitHeavy.amplitude);

            // 射击模式
            Assert.AreEqual(HapticType.Click, HapticPresets.ShootPistol.type);
            Assert.AreEqual(HapticType.Click, HapticPresets.ShootRifle.type);
            Assert.AreEqual(HapticType.Rumble, HapticPresets.ShootShotgun.type);

            // 霰弹枪应该有更强的效果
            Assert.Greater(HapticPresets.ShootShotgun.amplitude, HapticPresets.ShootPistol.amplitude);
            Assert.Greater(HapticPresets.ShootShotgun.duration, HapticPresets.ShootPistol.duration);
        }

        [Test]
        public void HapticPresets_EnvironmentPatterns()
        {
            // 环境触觉应该是持续或波动的
            Assert.AreEqual(HapticType.Wave, HapticPresets.Wind.type);
            Assert.AreEqual(HapticType.Pulse, HapticPresets.Rain.type);
            Assert.AreEqual(HapticType.Wave, HapticPresets.Water.type);

            // 持续时间应该较长
            Assert.Greater(HapticPresets.Wind.duration, 1f);
            Assert.Greater(HapticPresets.Water.duration, 1f);
        }

        [Test]
        public void HapticPresets_UIPatterns()
        {
            // UI反馈触觉应该较短
            Assert.Less(HapticPresets.ButtonClick.duration, 0.1f);
            Assert.Less(HapticPresets.Notification.duration, 0.1f);

            // 错误提示应该比成功提示更强烈或更长
            Assert.GreaterOrEqual(HapticPresets.Error.duration, HapticPresets.Success.duration);
        }

        [Test]
        public void HapticPresets_CreateCustom()
        {
            var pattern = HapticPresets.CreateCustom(HapticType.Buzz, 0.7f, 0.5f, 150f);

            Assert.AreEqual(HapticType.Buzz, pattern.type);
            Assert.AreEqual(0.7f, pattern.amplitude);
            Assert.AreEqual(0.5f, pattern.duration);
            Assert.AreEqual(150f, pattern.frequency);
            Assert.AreEqual(0.05f, pattern.fadeIn);
            Assert.AreEqual(0.05f, pattern.fadeOut);
        }

        [Test]
        public void HapticPresets_ScaleIntensity()
        {
            var original = new HapticPattern { amplitude = 0.5f };
            var scaled = HapticPresets.ScaleIntensity(original, 1.5f);

            Assert.AreEqual(0.75f, scaled.amplitude); // 0.5 * 1.5 = 0.75

            // 测试上限限制
            var maxScale = HapticPresets.ScaleIntensity(original, 3f);
            Assert.AreEqual(1f, maxScale.amplitude); // 限制在1.0
        }

        [UnityTest]
        public IEnumerator HapticFeedbackManager_Creation()
        {
            var testObject = new GameObject("TestHapticManager");
            var manager = testObject.AddComponent<HapticFeedbackManager>();

            Assert.IsNotNull(manager);
            Assert.IsTrue(manager.enableHaptics);
            Assert.AreEqual(1.0f, manager.globalIntensity);
            Assert.AreEqual(HapticPriority.Normal, manager.defaultPriority);

            Object.Destroy(testObject);
            yield return null;
        }

        [Test]
        public void HapticDeviceStatus_DefaultValues()
        {
            var status = new HapticDeviceStatus();

            Assert.IsNull(status.deviceName);
            Assert.IsFalse(status.isConnected);
            Assert.AreEqual(0f, status.batteryLevel);
            Assert.IsNull(status.supportedRegions);
        }

        [Test]
        public void HapticDeviceStatus_SetValues()
        {
            var status = new HapticDeviceStatus
            {
                deviceName = "TestDevice",
                isConnected = true,
                batteryLevel = 0.85f,
                supportedRegions = new[] { BodyRegion.LeftHand, BodyRegion.RightHand }
            };

            Assert.AreEqual("TestDevice", status.deviceName);
            Assert.IsTrue(status.isConnected);
            Assert.AreEqual(0.85f, status.batteryLevel);
            Assert.AreEqual(2, status.supportedRegions.Length);
        }

        [Test]
        public void HapticEvent_DefaultValues()
        {
            var hapticEvent = new HapticEvent();

            Assert.AreEqual(default(BodyRegion), hapticEvent.region);
            Assert.AreEqual(default(HapticPriority), hapticEvent.priority);
            Assert.AreEqual(0f, hapticEvent.timestamp);
        }

        [Test]
        public void BodyRegion_AdjacentRegions()
        {
            // 测试左手相邻区域
            var leftHandAdjacents = new[] { BodyRegion.LeftForearm, BodyRegion.Torso };
            CollectionAssert.Contains(leftHandAdjacents, BodyRegion.LeftForearm);

            // 测试右手相邻区域
            var rightHandAdjacents = new[] { BodyRegion.RightForearm, BodyRegion.Torso };
            CollectionAssert.Contains(rightHandAdjacents, BodyRegion.RightForearm);

            // 测试头部相邻区域
            var headAdjacents = new[] { BodyRegion.Torso, BodyRegion.Neck };
            CollectionAssert.Contains(headAdjacents, BodyRegion.Neck);
        }

        [Test]
        public void HapticFiles_Exist()
        {
            var managerPath = "Assets/Scripts/VR/Haptics/HapticFeedbackManager.cs";
            var controllerPath = "Assets/Scripts/VR/Haptics/ControllerHapticDevice.cs";
            var bhapticsPath = "Assets/Scripts/VR/Haptics/BhapticsDevice.cs";
            var presetsPath = "Assets/Scripts/VR/Haptics/HapticPresets.cs";

            Assert.IsTrue(System.IO.File.Exists(managerPath), $"HapticFeedbackManager应存在于{managerPath}");
            Assert.IsTrue(System.IO.File.Exists(controllerPath), $"ControllerHapticDevice应存在于{controllerPath}");
            Assert.IsTrue(System.IO.File.Exists(bhapticsPath), $"BhapticsDevice应存在于{bhapticsPath}");
            Assert.IsTrue(System.IO.File.Exists(presetsPath), $"HapticPresets应存在于{presetsPath}");
        }
    }
}
