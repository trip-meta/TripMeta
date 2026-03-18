using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.VR.Haptics
{
    /// <summary>
    /// bHaptics TactSuit 设备支持
    /// 全身触觉反馈背心/手套
    /// </summary>
    public class BhapticsDevice : IHapticDevice
    {
        public string DeviceName => "bHaptics TactSuit";
        public bool IsConnected { get; private set; }
        public float BatteryLevel { get; private set; } = 1f;
        public BodyRegion[] SupportedRegions => supportedRegions;
        public bool SupportsTextureHaptics => true;
        public bool SupportsTemperature => false;

        private BodyRegion[] supportedRegions = new[]
        {
            BodyRegion.Head,
            BodyRegion.Torso,
            BodyRegion.LeftHand,
            BodyRegion.RightHand,
            BodyRegion.LeftForearm,
            BodyRegion.RightForearm,
            BodyRegion.LeftFoot,
            BodyRegion.RightFoot
        };

        // bHaptics 设备位置映射
        private static readonly string[] PositionMappings = new[]
        {
            "Head",
            "Vest_Front",
            "Vest_Back",
            "Glove_Left",
            "Glove_Right",
            "Forearm_Left",
            "Forearm_Right",
            "Foot_Left",
            "Foot_Right"
        };

        public async Task<bool> ConnectAsync()
        {
            Debug.Log("[BhapticsDevice] 连接到 bHaptics TactSuit...");

            try
            {
                // 实际实现需要调用 bHaptics SDK
                // bHapticsLib.bHapticsManager.Initialize();
                await Task.Delay(200);

                IsConnected = true;
                Debug.Log("[BhapticsDevice] bHaptics TactSuit 已连接");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BhapticsDevice] 连接失败: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                // bHapticsLib.bHapticsManager.Destroy();
                IsConnected = false;
                Debug.Log("[BhapticsDevice] bHaptics TactSuit 已断开");
            }
        }

        public async Task TriggerHapticAsync(HapticPattern pattern)
        {
            if (!IsConnected) return;

            try
            {
                // 播放触觉效果
                string effectName = ConvertPatternToEffectName(pattern);
                // bHapticsLib.bHapticsManager.PlayRegistered(effectName);

                await Task.Delay((int)(pattern.duration * 1000));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BhapticsDevice] 触觉触发失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将触觉模式转换为 bHaptics 效果名称
        /// </summary>
        private string ConvertPatternToEffectName(HapticPattern pattern)
        {
            return pattern.type switch
            {
                HapticType.Buzz => "Buzz",
                HapticType.Click => "Click",
                HapticType.Rumble => "Rumble",
                HapticType.Pulse => "Pulse",
                _ => "Default"
            };
        }

        public async Task PlayTextureAsync(HapticTexture texture)
        {
            if (!IsConnected) return;

            try
            {
                // 将纹理数据转换为 bHaptics 格式
                foreach (var region in SupportedRegions)
                {
                    string position = ConvertRegionToPosition(region);
                    if (!string.IsNullOrEmpty(position))
                    {
                        // 播放纹理触觉
                        // bHapticsLib.bHapticsManager.PlayRegistered(position);
                    }
                }

                await Task.Delay((int)(texture.duration * 1000));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BhapticsDevice] 纹理播放失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 将身体区域转换为 bHaptics 位置标识
        /// </summary>
        private string ConvertRegionToPosition(BodyRegion region)
        {
            return region switch
            {
                BodyRegion.Head => "Head",
                BodyRegion.Torso => "Vest_Front",
                BodyRegion.LeftHand => "Glove_Left",
                BodyRegion.RightHand => "Glove_Right",
                BodyRegion.LeftForearm => "Arm_Left",
                BodyRegion.RightForearm => "Arm_Right",
                BodyRegion.LeftFoot => "Foot_Left",
                BodyRegion.RightFoot => "Foot_Right",
                _ => null
            };
        }

        public Task SetTemperatureAsync(float temperature)
        {
            // bHaptics 不支持温度控制
            return Task.CompletedTask;
        }

        public void StopAllHaptics()
        {
            if (IsConnected)
            {
                // bHapticsLib.bHapticsManager.StopAll();
                Debug.Log("[BhapticsDevice] 所有触觉已停止");
            }
        }

        /// <summary>
        /// 注册自定义触觉效果
        /// </summary>
        public void RegisterHapticEffect(string name, byte[] data)
        {
            if (IsConnected)
            {
                // bHapticsLib.bHapticsManager.RegisterFeedback(name, data);
                Debug.Log($"[BhapticsDevice] 注册触觉效果: {name}");
            }
        }

        /// <summary>
        /// 获取设备电池状态
        /// </summary>
        public float GetBatteryLevel(string position)
        {
            // 实际实现需要调用 SDK
            // return bHapticsLib.bHapticsManager.GetBattery(position);
            return 1f;
        }
    }
}
