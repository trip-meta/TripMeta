using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace TripMeta.Features.MobileCompanion
{
    /// <summary>
    /// 移动伴侣管理器 - 管理移动端配套应用连接
    /// </summary>
    public class MobileCompanionManager : MonoBehaviour, IMobileCompanionService
    {
        [Header("网络设置")]
        [SerializeField] private string serverUrl = "https://api.tripmeta.com";
        [SerializeField] private int connectionPort = 8080;
        [SerializeField] private float heartbeatInterval = 5f;

        [Header("配对设置")]
        [SerializeField] private int pairingCodeLength = 6;
        [SerializeField] private int pairingTimeout = 300; // 5分钟

        private bool _isInitialized;
        private bool _isConnected;
        private string _pairedDeviceId;
        private string _currentPairingCode;
        private float _lastHeartbeatTime;
        private List<PairedDeviceInfo> _pairedDeviceHistory = new List<PairedDeviceInfo>();

        // 事件
        public event Action<bool> OnConnectionStateChanged;
        public event Action<RemoteCommand> OnRemoteCommandReceived;
        public event Action<ChatMessage> OnChatMessageReceived;
        public event Action<PairingRequest> OnPairingRequestReceived;

        // 属性
        public bool IsConnected => _isConnected;
        public string PairedDeviceId => _pairedDeviceId;

        public static MobileCompanionManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_isConnected)
            {
                // 发送心跳包
                if (Time.time - _lastHeartbeatTime > heartbeatInterval)
                {
                    _ = SendHeartbeatAsync();
                    _lastHeartbeatTime = Time.time;
                }
            }
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                Debug.Log("[MobileCompanionManager] 初始化移动伴侣服务...");

                // 加载已配对的设备历史
                LoadPairedDeviceHistory();

                _isInitialized = true;
                Debug.Log("[MobileCompanionManager] 初始化完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 初始化失败: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> StartPairingAsync(string pairingCode)
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            try
            {
                _currentPairingCode = pairingCode;
                Debug.Log($"[MobileCompanionManager] 开始配对模式，配对码: {pairingCode}");

                // 模拟配对过程
                // 实际应该通过WebSocket或HTTP与服务器通信
                await Task.Delay(1000);

                // 模拟接收到配对请求
                var request = new PairingRequest
                {
                    DeviceId = $"device_{Guid.NewGuid().ToString().Substring(0, 8)}",
                    DeviceName = "iPhone 15",
                    PairingCode = pairingCode,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                OnPairingRequestReceived?.Invoke(request);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 启动配对失败: {ex.Message}");
                return false;
            }
        }

        public async Task AcceptPairingAsync(string deviceId)
        {
            try
            {
                Debug.Log($"[MobileCompanionManager] 接受配对: {deviceId}");

                _pairedDeviceId = deviceId;
                _isConnected = true;

                // 添加到配对历史
                var deviceInfo = new PairedDeviceInfo
                {
                    DeviceId = deviceId,
                    DeviceName = "iPhone 15",
                    DeviceType = "iOS",
                    PairedTime = DateTime.UtcNow,
                    LastConnectedTime = DateTime.UtcNow,
                    ConnectionCount = 1,
                    IsTrusted = true
                };

                _pairedDeviceHistory.Add(deviceInfo);
                SavePairedDeviceHistory();

                OnConnectionStateChanged?.Invoke(true);

                // 开始发送VR状态
                _ = StartSendingVRStateAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 接受配对失败: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (!_isConnected) return;

            try
            {
                Debug.Log("[MobileCompanionManager] 断开连接");

                _isConnected = false;
                _pairedDeviceId = null;

                OnConnectionStateChanged?.Invoke(false);

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 断开连接失败: {ex.Message}");
            }
        }

        public async Task SendVRStateAsync(VRState state)
        {
            if (!_isConnected) return;

            try
            {
                var json = JsonConvert.SerializeObject(state);
                Debug.Log($"[MobileCompanionManager] 发送VR状态: {state.CurrentAttractionName}");

                // 实际应该通过WebSocket发送
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 发送VR状态失败: {ex.Message}");
            }
        }

        public async Task SendAttractionInfoAsync(AttractionMobileInfo attractionInfo)
        {
            if (!_isConnected) return;

            try
            {
                var json = JsonConvert.SerializeObject(attractionInfo);
                Debug.Log($"[MobileCompanionManager] 发送景点信息: {attractionInfo.Name}");

                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 发送景点信息失败: {ex.Message}");
            }
        }

        public async Task SendNotificationAsync(MobileNotification notification)
        {
            if (!_isConnected) return;

            try
            {
                Debug.Log($"[MobileCompanionManager] 发送通知: {notification.Title}");

                // 实际应该发送到移动设备
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 发送通知失败: {ex.Message}");
            }
        }

        public async Task ExecuteRemoteCommandAsync(RemoteCommand command)
        {
            Debug.Log($"[MobileCompanionManager] 执行远程命令: {command.Type}");

            try
            {
                switch (command.Type)
                {
                    case CommandType.Pause:
                        // 暂停VR体验
                        Debug.Log("[MobileCompanionManager] 暂停体验");
                        break;

                    case CommandType.Resume:
                        // 继续VR体验
                        Debug.Log("[MobileCompanionManager] 继续体验");
                        break;

                    case CommandType.JumpToAttraction:
                        // 跳转到指定景点
                        Debug.Log($"[MobileCompanionManager] 跳转到景点: {command.Parameter}");
                        break;

                    case CommandType.AdjustVolume:
                        // 调整音量
                        if (float.TryParse(command.Parameter, out float volume))
                        {
                            Debug.Log($"[MobileCompanionManager] 调整音量到: {volume}");
                        }
                        break;

                    case CommandType.TakePhoto:
                        // 拍照
                        Debug.Log("[MobileCompanionManager] 拍照");
                        break;

                    case CommandType.RequestHelp:
                        // 请求帮助
                        Debug.Log("[MobileCompanionManager] 请求帮助");
                        break;

                    case CommandType.ReturnToMenu:
                        // 返回主菜单
                        Debug.Log("[MobileCompanionManager] 返回主菜单");
                        break;
                }

                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 执行命令失败: {ex.Message}");
            }
        }

        public List<PairedDeviceInfo> GetPairedDeviceHistory()
        {
            return new List<PairedDeviceInfo>(_pairedDeviceHistory);
        }

        public async Task RemovePairedDeviceAsync(string deviceId)
        {
            try
            {
                var device = _pairedDeviceHistory.Find(d => d.DeviceId == deviceId);
                if (device != null)
                {
                    _pairedDeviceHistory.Remove(device);
                    SavePairedDeviceHistory();
                    Debug.Log($"[MobileCompanionManager] 移除配对设备: {deviceId}");
                }

                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileCompanionManager] 移除设备失败: {ex.Message}");
            }
        }

        private async Task SendHeartbeatAsync()
        {
            // 发送心跳包保持连接
            await Task.Delay(10);
        }

        private async Task StartSendingVRStateAsync()
        {
            while (_isConnected)
            {
                var state = new VRState
                {
                    CurrentAttractionId = "attr_001",
                    CurrentAttractionName = "故宫博物院",
                    Progress = 45,
                    IsSpeaking = false,
                    CurrentSpeechText = "",
                    ConnectedPlayers = 1,
                    BatteryLevel = 85,
                    NetworkLatency = 20,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                await SendVRStateAsync(state);
                await Task.Delay(2000); // 每2秒发送一次
            }
        }

        private void LoadPairedDeviceHistory()
        {
            // 从本地存储加载配对历史
            var json = PlayerPrefs.GetString("PairedDevices", "[]");
            try
            {
                _pairedDeviceHistory = JsonConvert.DeserializeObject<List<PairedDeviceInfo>>(json) ?? new List<PairedDeviceInfo>();
            }
            catch
            {
                _pairedDeviceHistory = new List<PairedDeviceInfo>();
            }
        }

        private void SavePairedDeviceHistory()
        {
            var json = JsonConvert.SerializeObject(_pairedDeviceHistory);
            PlayerPrefs.SetString("PairedDevices", json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 生成随机配对码
        /// </summary>
        public string GeneratePairingCode()
        {
            var random = new System.Random();
            var code = "";
            for (int i = 0; i < pairingCodeLength; i++)
            {
                code += random.Next(0, 10).ToString();
            }
            return code;
        }

        /// <summary>
        /// 发送聊天消息
        /// </summary>
        public async Task SendChatMessageAsync(string message)
        {
            if (!_isConnected) return;

            var chatMessage = new ChatMessage
            {
                SenderName = "VR User",
                Content = message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                IsSystemMessage = false
            };

            Debug.Log($"[MobileCompanionManager] 发送聊天消息: {message}");
            await Task.Delay(10);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
