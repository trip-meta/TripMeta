using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace TripMeta.VR.Haptics
{
    /// <summary>
    /// 触觉反馈管理器
    /// 支持全身触觉反馈设备，提供沉浸式触觉体验
    /// </summary>
    public class HapticFeedbackManager : MonoBehaviour
    {
        [Header("触觉配置")]
        public bool enableHaptics = true;
        public float globalIntensity = 1.0f;
        public float globalDuration = 0.1f;
        public HapticPriority defaultPriority = HapticPriority.Normal;

        [Header("身体区域")]
        public bool enableHead = true;
        public bool enableTorso = true;
        public bool enableArms = true;
        public bool enableHands = true;
        public bool enableLegs = true;
        public bool enableFeet = true;

        [Header("设备连接")]
        public bool autoConnect = true;
        public float connectionTimeout = 5f;
        public int maxReconnectAttempts = 3;

        [Header("高级设置")]
        public bool enableSpatialHaptics = true;
        public bool enableTextureHaptics = true;
        public bool enableTemperatureHaptics = false;
        public int hapticChannelCount = 16;

        // 设备管理
        private List<IHapticDevice> connectedDevices = new List<IHapticDevice>();
        private Dictionary<BodyRegion, List<IHapticDevice>> regionDevices = new Dictionary<BodyRegion, List<IHapticDevice>>();

        // 触觉队列
        private Queue<HapticEvent> hapticQueue = new Queue<HapticEvent>();
        private bool isProcessing = false;

        // 状态
        private bool isInitialized = false;
        private bool isConnected = false;

        public static HapticFeedbackManager Instance { get; private set; }

        public bool IsInitialized => isInitialized;
        public bool IsConnected => isConnected;
        public int ConnectedDeviceCount => connectedDevices.Count;

        public event Action<bool> OnConnectionStateChanged;
        public event Action<BodyRegion, HapticPattern> OnHapticTriggered;
        public event Action<string> OnError;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        async void Start()
        {
            if (autoConnect)
            {
                await InitializeAsync();
            }
        }

        /// <summary>
        /// 异步初始化触觉系统
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (isInitialized) return true;

            Debug.Log("[HapticFeedbackManager] 初始化触觉反馈系统...");

            try
            {
                InitializeRegionDevices();

                if (enableHaptics)
                {
                    await DiscoverAndConnectDevicesAsync();
                }

                isInitialized = true;
                Debug.Log($"[HapticFeedbackManager] 触觉系统初始化完成，连接设备数: {connectedDevices.Count}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HapticFeedbackManager] 初始化失败: {ex.Message}");
                OnError?.Invoke(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 初始化身体区域设备映射
        /// </summary>
        private void InitializeRegionDevices()
        {
            foreach (BodyRegion region in Enum.GetValues(typeof(BodyRegion)))
            {
                regionDevices[region] = new List<IHapticDevice>();
            }
        }

        /// <summary>
        /// 发现并连接设备
        /// </summary>
        private async Task DiscoverAndConnectDevicesAsync()
        {
            // 检查控制器触觉
            var controllers = FindControllers();
            foreach (var controller in controllers)
            {
                var device = new ControllerHapticDevice(controller);
                await ConnectDeviceAsync(device);
            }

            // 检查专用触觉设备 (bHaptics, TactSuit等)
            await CheckForDedicatedHapticDevicesAsync();

            isConnected = connectedDevices.Count > 0;
            OnConnectionStateChanged?.Invoke(isConnected);
        }

        /// <summary>
        /// 查找控制器
        /// </summary>
        private List<InputDevice> FindControllers()
        {
            var controllers = new List<InputDevice>();
            var devices = new List<InputDevice>();
            InputDevices.GetDevices(devices);

            foreach (var device in devices)
            {
                if (device.role == InputDeviceRole.LeftHanded ||
                    device.role == InputDeviceRole.RightHanded ||
                    device.name.ToLower().Contains("controller"))
                {
                    controllers.Add(device);
                    Debug.Log($"[HapticFeedbackManager] 发现控制器: {device.name}");
                }
            }

            return controllers;
        }

        /// <summary>
        /// 检查专用触觉设备
        /// </summary>
        private async Task CheckForDedicatedHapticDevicesAsync()
        {
            // bHaptics TactSuit
            var bhapticsDevice = await CheckForBhapticsDeviceAsync();
            if (bhapticsDevice != null)
            {
                await ConnectDeviceAsync(bhapticsDevice);
            }

            // 其他触觉设备...
            await Task.Delay(100);
        }

        /// <summary>
        /// 检查bHaptics设备
        /// </summary>
        private async Task<IHapticDevice> CheckForBhapticsDeviceAsync()
        {
            // bHaptics SDK集成
            await Task.Delay(50);
            return null; // 实际实现需要调用bHaptics SDK
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        private async Task ConnectDeviceAsync(IHapticDevice device)
        {
            if (await device.ConnectAsync())
            {
                connectedDevices.Add(device);

                // 注册到对应身体区域
                foreach (var region in device.SupportedRegions)
                {
                    if (regionDevices.ContainsKey(region))
                    {
                        regionDevices[region].Add(device);
                    }
                }

                Debug.Log($"[HapticFeedbackManager] 设备已连接: {device.DeviceName}");
            }
        }

        /// <summary>
        /// 触发触觉反馈
        /// </summary>
        public void TriggerHaptic(BodyRegion region, HapticPattern pattern, HapticPriority priority = HapticPriority.Normal)
        {
            if (!enableHaptics || !isConnected) return;

            var hapticEvent = new HapticEvent
            {
                region = region,
                pattern = pattern,
                priority = priority,
                timestamp = Time.time
            };

            if (priority == HapticPriority.Critical)
            {
                // 高优先级立即执行
                _ = ExecuteHapticAsync(hapticEvent);
            }
            else
            {
                hapticQueue.Enqueue(hapticEvent);
                if (!isProcessing)
                {
                    _ = ProcessHapticQueueAsync();
                }
            }

            OnHapticTriggered?.Invoke(region, pattern);
        }

        /// <summary>
        /// 触发手部触觉 (快捷方法)
        /// </summary>
        public void TriggerHandHaptic(bool isLeftHand, float amplitude, float duration)
        {
            var region = isLeftHand ? BodyRegion.LeftHand : BodyRegion.RightHand;
            var pattern = new HapticPattern
            {
                type = HapticType.Buzz,
                amplitude = amplitude * globalIntensity,
                duration = duration,
                frequency = 100f
            };

            TriggerHaptic(region, pattern, defaultPriority);
        }

        /// <summary>
        /// 触发撞击触觉
        /// </summary>
        public void TriggerImpact(Vector3 impactPoint, float impactForce, BodyRegion hitRegion)
        {
            var pattern = HapticPattern.CreateImpactPattern(impactForce);
            TriggerHaptic(hitRegion, pattern, HapticPriority.High);

            if (enableSpatialHaptics)
            {
                // 传播到相邻区域
                PropagateHapticToAdjacentRegions(hitRegion, impactForce * 0.5f);
            }
        }

        /// <summary>
        /// 传播触觉到相邻区域
        /// </summary>
        private void PropagateHapticToAdjacentRegions(BodyRegion sourceRegion, float intensity)
        {
            var adjacentRegions = GetAdjacentRegions(sourceRegion);
            foreach (var region in adjacentRegions)
            {
                var pattern = new HapticPattern
                {
                    type = HapticType.Rumble,
                    amplitude = intensity * globalIntensity,
                    duration = 0.05f,
                    frequency = 50f,
                    delay = 0.02f
                };

                TriggerHaptic(region, pattern, HapticPriority.Low);
            }
        }

        /// <summary>
        /// 获取相邻身体区域
        /// </summary>
        private BodyRegion[] GetAdjacentRegions(BodyRegion region)
        {
            return region switch
            {
                BodyRegion.LeftHand => new[] { BodyRegion.LeftForearm, BodyRegion.Torso },
                BodyRegion.RightHand => new[] { BodyRegion.RightForearm, BodyRegion.Torso },
                BodyRegion.LeftFoot => new[] { BodyRegion.LeftCalf, BodyRegion.Torso },
                BodyRegion.RightFoot => new[] { BodyRegion.RightCalf, BodyRegion.Torso },
                BodyRegion.Head => new[] { BodyRegion.Torso, BodyRegion.Neck },
                _ => Array.Empty<BodyRegion>()
            };
        }

        /// <summary>
        /// 处理触觉队列
        /// </summary>
        private async Task ProcessHapticQueueAsync()
        {
            isProcessing = true;

            while (hapticQueue.Count > 0)
            {
                var hapticEvent = hapticQueue.Dequeue();
                await ExecuteHapticAsync(hapticEvent);
                await Task.Delay(10); // 防止过载
            }

            isProcessing = false;
        }

        /// <summary>
        /// 执行触觉事件
        /// </summary>
        private async Task ExecuteHapticAsync(HapticEvent hapticEvent)
        {
            if (!regionDevices.TryGetValue(hapticEvent.region, out var devices))
                return;

            foreach (var device in devices)
            {
                if (device.IsConnected)
                {
                    await device.TriggerHapticAsync(hapticEvent.pattern);
                }
            }
        }

        /// <summary>
        /// 播放触觉纹理 (纹理触觉)
        /// </summary>
        public async Task PlayHapticTexture(BodyRegion region, HapticTexture texture)
        {
            if (!enableTextureHaptics || !enableHaptics) return;

            if (regionDevices.TryGetValue(region, out var devices))
            {
                foreach (var device in devices)
                {
                    if (device.SupportsTextureHaptics)
                    {
                        await device.PlayTextureAsync(texture);
                    }
                }
            }
        }

        /// <summary>
        /// 设置温度反馈
        /// </summary>
        public async Task SetTemperatureAsync(BodyRegion region, float temperature)
        {
            if (!enableTemperatureHaptics || !enableHaptics) return;

            if (regionDevices.TryGetValue(region, out var devices))
            {
                foreach (var device in devices)
                {
                    if (device.SupportsTemperature)
                    {
                        await device.SetTemperatureAsync(temperature);
                    }
                }
            }
        }

        /// <summary>
        /// 停止所有触觉
        /// </summary>
        public void StopAllHaptics()
        {
            hapticQueue.Clear();

            foreach (var device in connectedDevices)
            {
                device.StopAllHaptics();
            }

            Debug.Log("[HapticFeedbackManager] 所有触觉已停止");
        }

        /// <summary>
        /// 获取设备状态报告
        /// </summary>
        public HapticDeviceStatus[] GetDeviceStatusReport()
        {
            var statuses = new List<HapticDeviceStatus>();

            foreach (var device in connectedDevices)
            {
                statuses.Add(new HapticDeviceStatus
                {
                    deviceName = device.DeviceName,
                    isConnected = device.IsConnected,
                    batteryLevel = device.BatteryLevel,
                    supportedRegions = device.SupportedRegions
                });
            }

            return statuses.ToArray();
        }

        void OnDestroy()
        {
            StopAllHaptics();

            foreach (var device in connectedDevices)
            {
                device.Disconnect();
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 身体区域
    /// </summary>
    public enum BodyRegion
    {
        Head,
        Neck,
        Torso,
        LeftShoulder,
        RightShoulder,
        LeftUpperArm,
        RightUpperArm,
        LeftForearm,
        RightForearm,
        LeftHand,
        RightHand,
        LeftHip,
        RightHip,
        LeftThigh,
        RightThigh,
        LeftCalf,
        RightCalf,
        LeftFoot,
        RightFoot
    }

    /// <summary>
    /// 触觉优先级
    /// </summary>
    public enum HapticPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// 触觉类型
    /// </summary>
    public enum HapticType
    {
        Buzz,       // 嗡嗡声
        Click,      // 点击
        Rumble,     // 隆隆声
        Pulse,      // 脉冲
        Continuous, // 持续
        Wave        // 波形
    }

    /// <summary>
    /// 触觉模式
    /// </summary>
    [Serializable]
    public struct HapticPattern
    {
        public HapticType type;
        public float amplitude;     // 0-1
        public float frequency;     // Hz
        public float duration;      // seconds
        public float delay;         // seconds before start
        public float fadeIn;        // seconds
        public float fadeOut;       // seconds

        /// <summary>
        /// 创建撞击触觉模式
        /// </summary>
        public static HapticPattern CreateImpactPattern(float force)
        {
            return new HapticPattern
            {
                type = HapticType.Click,
                amplitude = Mathf.Clamp01(force),
                frequency = 200f,
                duration = 0.05f + force * 0.1f,
                fadeIn = 0f,
                fadeOut = 0.02f
            };
        }

        /// <summary>
        /// 创建持续触觉模式
        /// </summary>
        public static HapticPattern CreateContinuousPattern(float intensity, float duration)
        {
            return new HapticPattern
            {
                type = HapticType.Continuous,
                amplitude = Mathf.Clamp01(intensity),
                frequency = 100f,
                duration = duration,
                fadeIn = 0.1f,
                fadeOut = 0.1f
            };
        }
    }

    /// <summary>
    /// 触觉纹理
    /// </summary>
    public class HapticTexture
    {
        public float[] amplitudeData;
        public float[] frequencyData;
        public float duration;
        public int sampleRate;
    }

    /// <summary>
    /// 触觉事件
    /// </summary>
    public struct HapticEvent
    {
        public BodyRegion region;
        public HapticPattern pattern;
        public HapticPriority priority;
        public float timestamp;
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    [Serializable]
    public struct HapticDeviceStatus
    {
        public string deviceName;
        public bool isConnected;
        public float batteryLevel;
        public BodyRegion[] supportedRegions;
    }

    /// <summary>
    /// 触觉设备接口
    /// </summary>
    public interface IHapticDevice
    {
        string DeviceName { get; }
        bool IsConnected { get; }
        float BatteryLevel { get; }
        BodyRegion[] SupportedRegions { get; }
        bool SupportsTextureHaptics { get; }
        bool SupportsTemperature { get; }

        Task<bool> ConnectAsync();
        void Disconnect();
        Task TriggerHapticAsync(HapticPattern pattern);
        Task PlayTextureAsync(HapticTexture texture);
        Task SetTemperatureAsync(float temperature);
        void StopAllHaptics();
    }

    #endregion
}
