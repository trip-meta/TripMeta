using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace TripMeta.VR.Haptics
{
    /// <summary>
    /// 控制器触觉设备实现
    /// 支持 VR 手柄的触觉反馈
    /// </summary>
    public class ControllerHapticDevice : IHapticDevice
    {
        private InputDevice controller;
        private string deviceName;
        private bool isConnected;
        private BodyRegion[] supportedRegions;

        public string DeviceName => deviceName;
        public bool IsConnected => isConnected && controller.isValid;
        public float BatteryLevel { get; private set; } = 1f;
        public BodyRegion[] SupportedRegions => supportedRegions;
        public bool SupportsTextureHaptics => false;
        public bool SupportsTemperature => false;

        public ControllerHapticDevice(InputDevice device)
        {
            controller = device;
            deviceName = device.name;
            isConnected = device.isValid;

            // 根据角色确定支持的身体区域
            supportedRegions = DetermineSupportedRegions(device);
        }

        /// <summary>
        /// 确定控制器支持的身体区域
        /// </summary>
        private BodyRegion[] DetermineSupportedRegions(InputDevice device)
        {
            if (device.role == InputDeviceRole.LeftHanded ||
                device.characteristics.HasFlag(InputDeviceCharacteristics.Left))
            {
                return new[] { BodyRegion.LeftHand };
            }
            else if (device.role == InputDeviceRole.RightHanded ||
                     device.characteristics.HasFlag(InputDeviceCharacteristics.Right))
            {
                return new[] { BodyRegion.RightHand };
            }
            else
            {
                // 未知手 - 可能同时支持
                return new[] { BodyRegion.LeftHand, BodyRegion.RightHand };
            }
        }

        public async Task<bool> ConnectAsync()
        {
            if (controller.isValid)
            {
                isConnected = true;
                await Task.Delay(10);
                return true;
            }
            return false;
        }

        public void Disconnect()
        {
            isConnected = false;
        }

        public async Task TriggerHapticAsync(HapticPattern pattern)
        {
            if (!IsConnected) return;

            try
            {
                // 发送触觉命令到控制器
                if (controller.TryGetHapticCapabilities(out HapticCapabilities capabilities))
                {
                    if (capabilities.supportsImpulse)
                    {
                        uint channel = 0;
                        float amplitude = pattern.amplitude;
                        float duration = pattern.duration;

                        controller.SendHapticImpulse(channel, amplitude, duration);
                    }
                    else if (capabilities.supportsBuffer)
                    {
                        // 使用缓冲区触觉 (如果支持)
                        byte[] buffer = CreateHapticBuffer(pattern);
                        controller.SendHapticBuffer(0, buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ControllerHapticDevice] 触觉触发失败: {ex.Message}");
            }

            await Task.Delay((int)(pattern.duration * 1000));
        }

        /// <summary>
        /// 创建触觉缓冲区
        /// </summary>
        private byte[] CreateHapticBuffer(HapticPattern pattern)
        {
            // 简化的触觉缓冲区创建
            int sampleCount = Mathf.CeilToInt(pattern.duration * 320); // 320Hz 采样率
            byte[] buffer = new byte[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                float amplitude = pattern.amplitude;

                // 应用淡入淡出
                if (t < pattern.fadeIn && pattern.fadeIn > 0)
                {
                    amplitude *= t / pattern.fadeIn;
                }
                else if (t > 1 - pattern.fadeOut && pattern.fadeOut > 0)
                {
                    amplitude *= (1 - t) / pattern.fadeOut;
                }

                buffer[i] = (byte)(amplitude * 255);
            }

            return buffer;
        }

        public Task PlayTextureAsync(HapticTexture texture)
        {
            // 控制器不支持纹理触觉
            return Task.CompletedTask;
        }

        public Task SetTemperatureAsync(float temperature)
        {
            // 控制器不支持温度反馈
            return Task.CompletedTask;
        }

        public void StopAllHaptics()
        {
            if (IsConnected)
            {
                try
                {
                    controller.StopHaptics();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ControllerHapticDevice] 停止触觉失败: {ex.Message}");
                }
            }
        }
    }
}
