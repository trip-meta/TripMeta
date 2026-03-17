using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace TripMeta.Features.Multiplayer
{
    /// <summary>
    /// 多人游戏管理器 - 管理多人VR会话
    /// </summary>
    public class MultiplayerManager : NetworkBehaviour, IMultiplayerService
    {
        [Header("网络配置")]
        [SerializeField] private int maxConnections = 8;
        [SerializeField] private ushort port = 7777;
        [SerializeField] private string defaultAddress = "127.0.0.1";

        [Header("VR玩家预制体")]
        [SerializeField] private GameObject vrPlayerPrefab;

        private NetworkManager _networkManager;
        private Dictionary<ulong, NetworkVRPlayer> _connectedPlayers = new Dictionary<ulong, NetworkVRPlayer>();
        private bool _isInitialized;

        // 事件
        public event Action<bool> OnConnectionStatusChanged;
        public event Action<ulong> OnPlayerJoined;
        public event Action<ulong> OnPlayerLeft;
        public event Action<ulong, byte[]> OnVoiceChatReceived;

        // 属性
        public bool IsConnected => _networkManager != null && _networkManager.IsConnectedClient;
        public bool IsHost => _networkManager != null && _networkManager.IsHost;
        public int ConnectedClientCount => _networkManager != null ? _networkManager.ConnectedClients.Count : 0;
        public ulong LocalClientId => _networkManager != null ? _networkManager.LocalClient.ClientId : 0;

        public static MultiplayerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _networkManager = GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                _networkManager = gameObject.AddComponent<NetworkManager>();
            }

            // 配置Unity Transport
            var transport = GetComponent<UnityTransport>();
            if (transport == null)
            {
                transport = gameObject.AddComponent<UnityTransport>();
            }
            transport.ConnectionData.Port = port;
            _networkManager.NetworkConfig.NetworkTransport = transport;

            // 注册网络回调
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                Debug.Log("[MultiplayerManager] 初始化多人游戏管理器...");

                // 确保NetworkManager配置正确
                if (_networkManager.NetworkConfig.PlayerPrefab == null && vrPlayerPrefab != null)
                {
                    _networkManager.NetworkConfig.PlayerPrefab = vrPlayerPrefab;
                }

                _isInitialized = true;
                Debug.Log("[MultiplayerManager] 初始化完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerManager] 初始化失败: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateRoomAsync(string roomName, int maxPlayers = 8)
        {
            try
            {
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                maxConnections = maxPlayers;

                // 配置为Host模式
                var transport = _networkManager.GetComponent<UnityTransport>();
                transport.SetConnectionData(defaultAddress, port);

                var success = _networkManager.StartHost();

                if (success)
                {
                    Debug.Log($"[MultiplayerManager] 房间创建成功: {roomName}, 主机ID: {LocalClientId}");
                    OnConnectionStatusChanged?.Invoke(true);
                    return true;
                }
                else
                {
                    Debug.LogError("[MultiplayerManager] 房间创建失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerManager] 创建房间失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> JoinRoomAsync(string roomCode)
        {
            try
            {
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                // 解析房间代码获取IP和端口
                // 这里简化处理，实际应该通过房间代码获取连接信息
                var address = defaultAddress;
                var connectPort = port;

                // 配置为Client模式
                var transport = _networkManager.GetComponent<UnityTransport>();
                transport.SetConnectionData(address, connectPort);

                var success = _networkManager.StartClient();

                if (success)
                {
                    Debug.Log($"[MultiplayerManager] 正在加入房间: {roomCode}");
                    // 等待连接完成
                    await Task.Delay(2000);
                    return IsConnected;
                }
                else
                {
                    Debug.LogError("[MultiplayerManager] 加入房间失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerManager] 加入房间失败: {ex.Message}");
                return false;
            }
        }

        public async Task LeaveRoomAsync()
        {
            try
            {
                if (IsHost)
                {
                    // 主机断开会关闭整个房间
                    _networkManager.Shutdown();
                    Debug.Log("[MultiplayerManager] 房间已关闭");
                }
                else if (IsConnected)
                {
                    _networkManager.Shutdown();
                    Debug.Log("[MultiplayerManager] 已离开房间");
                }

                OnConnectionStatusChanged?.Invoke(false);
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerManager] 离开房间失败: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            await LeaveRoomAsync();
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[MultiplayerManager] 客户端连接: {clientId}");

            // 如果是本地客户端连接
            if (clientId == LocalClientId)
            {
                OnConnectionStatusChanged?.Invoke(true);
            }

            OnPlayerJoined?.Invoke(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[MultiplayerManager] 客户端断开: {clientId}");

            if (_connectedPlayers.ContainsKey(clientId))
            {
                _connectedPlayers.Remove(clientId);
            }

            // 如果是本地客户端断开
            if (clientId == LocalClientId)
            {
                OnConnectionStatusChanged?.Invoke(false);
            }

            OnPlayerLeft?.Invoke(clientId);
        }

        public List<PlayerInfo> GetConnectedPlayers()
        {
            var players = new List<PlayerInfo>();

            if (_networkManager == null || _networkManager.ConnectedClients == null)
                return players;

            foreach (var client in _networkManager.ConnectedClients)
            {
                var playerInfo = new PlayerInfo
                {
                    ClientId = client.Key,
                    IsHost = client.Key == _networkManager.LocalClientId && IsHost,
                    Status = PlayerStatus.Connected
                };

                // 获取玩家位置和旋转
                if (_connectedPlayers.TryGetValue(client.Key, out var networkPlayer))
                {
                    playerInfo.Position = networkPlayer.transform.position;
                    playerInfo.Rotation = networkPlayer.transform.rotation;
                    playerInfo.PlayerName = networkPlayer.PlayerName;
                }

                players.Add(playerInfo);
            }

            return players;
        }

        public async Task SendVoiceChatAsync(byte[] audioData)
        {
            if (!IsConnected) return;

            try
            {
                // 通过RPC发送语音数据到所有客户端
                SendVoiceChatServerRpc(audioData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerManager] 发送语音数据失败: {ex.Message}");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendVoiceChatServerRpc(byte[] audioData, ServerRpcParams rpcParams = default)
        {
            // 服务器转发到所有客户端
            ReceiveVoiceChatClientRpc(audioData, rpcParams.Receive.SenderClientId);
        }

        [ClientRpc]
        private void ReceiveVoiceChatClientRpc(byte[] audioData, ulong senderId)
        {
            // 不播放自己的语音
            if (senderId != LocalClientId)
            {
                OnVoiceChatReceived?.Invoke(senderId, audioData);
            }
        }

        public async Task SyncTourGuideStateAsync(TourGuideSyncState tourState)
        {
            if (!IsConnected) return;

            try
            {
                // 只有主机可以同步导游状态
                if (IsHost)
                {
                    SyncTourGuideStateClientRpc(tourState);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiplayerManager] 同步导游状态失败: {ex.Message}");
            }
        }

        [ClientRpc]
        private void SyncTourGuideStateClientRpc(TourGuideSyncState tourState)
        {
            // 客户端接收导游状态同步
            Debug.Log($"[MultiplayerManager] 接收到导游状态同步: 景点索引 {tourState.CurrentAttractionIndex}");
            // 这里应该触发事件让TourGuideService处理
        }

        public int GetNetworkLatency()
        {
            if (!IsConnected || _networkManager == null)
                return -1;

            // 获取网络往返时间(RTT)
            if (_networkManager.NetworkConfig.NetworkTransport is UnityTransport transport)
            {
                // Unity Transport 2.0+ 支持获取RTT
                return (int)(transport.GetCurrentRtt(0) * 1000); // 转换为毫秒
            }

            return 0;
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= OnClientConnected;
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
