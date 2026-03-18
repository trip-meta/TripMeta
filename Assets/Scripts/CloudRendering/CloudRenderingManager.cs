using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.WebRTC;

namespace TripMeta.CloudRendering
{
    /// <summary>
    /// 云渲染管理器
    /// 处理云端渲染、视频流接收、输入转发等功能
    /// 让低端设备也能体验高质量 VR
    /// </summary>
    public class CloudRenderingManager : MonoBehaviour
    {
        [Header("服务器配置")]
        public string signallingServerUrl = "wss://tripmeta-cloud-render.com/signalling";
        public string apiKey = "";
        public string region = "asia-east1";

        [Header("渲染配置")]
        public int targetResolutionX = 1920;
        public int targetResolutionY = 1080;
        public int targetFrameRate = 60;
        public int bitrateKbps = 20000; // 20 Mbps
        public int minBitrateKbps = 5000;
        public int maxBitrateKbps = 50000;

        [Header("自适应质量")]
        public bool enableAdaptiveBitrate = true;
        public float bitrateAdjustmentInterval = 5f;
        public float packetLossThreshold = 0.05f;
        public int latencyThresholdMs = 100;

        [Header("功能开关")]
        public bool enableCloudRendering = true;
        public bool useHardwareDecoding = true;
        public bool enableInputPrediction = true;
        public bool enableFrameInterpolation = true;

        // WebRTC 组件
        private RTCPeerConnection peerConnection;
        private RTCDataChannel inputChannel;
        private RTCDataChannel dataChannel;

        // 渲染纹理
        private RenderTexture receiveTexture;
        private Material videoMaterial;

        // 连接状态
        private ConnectionState connectionState = ConnectionState.Disconnected;
        private string currentSessionId;

        // 统计信息
        private StreamingStats stats = new StreamingStats();
        private float lastStatsUpdateTime;

        // 输入队列
        private Queue<InputEvent> inputQueue = new Queue<InputEvent>();
        private float lastInputSendTime;

        public static CloudRenderingManager Instance { get; private set; }

        public bool IsConnected => connectionState == ConnectionState.Connected;
        public ConnectionState CurrentState => connectionState;
        public StreamingStats CurrentStats => stats;
        public RenderTexture ReceiveTexture => receiveTexture;

        // 事件
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnConnectionError;
        public event Action<StreamingStats> OnStatsUpdated;
        public event Action<int, int> OnResolutionChanged;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeWebRTC();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            if (enableCloudRendering)
            {
                CreateReceiveTexture();
            }
        }

        void Update()
        {
            if (!IsConnected) return;

            // 发送输入
            if (enableInputPrediction && Time.time - lastInputSendTime > 0.016f) // 60Hz
            {
                SendPendingInputs();
            }

            // 更新统计
            if (Time.time - lastStatsUpdateTime > bitrateAdjustmentInterval)
            {
                UpdateStreamingStats();
                lastStatsUpdateTime = Time.time;
            }

            // 自适应码率
            if (enableAdaptiveBitrate)
            {
                AdjustBitrate();
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            Disconnect();
            CleanupWebRTC();
        }

        #region 初始化

        /// <summary>
        /// 初始化 WebRTC
        /// </summary>
        private void InitializeWebRTC()
        {
            WebRTC.Initialize(WebRTC.EncoderType.Hardware);
            Debug.Log("[CloudRenderingManager] WebRTC 初始化完成");
        }

        /// <summary>
        /// 清理 WebRTC
        /// </summary>
        private void CleanupWebRTC()
        {
            receiveTexture?.Release();
            WebRTC.Dispose();
        }

        /// <summary>
        /// 创建接收纹理
        /// </summary>
        private void CreateReceiveTexture()
        {
            receiveTexture = new RenderTexture(targetResolutionX, targetResolutionY, 0, RenderTextureFormat.ARGB32);
            receiveTexture.Create();

            videoMaterial = new Material(Shader.Find("Unlit/Texture"));
            videoMaterial.mainTexture = receiveTexture;
        }

        #endregion

        #region 连接管理

        /// <summary>
        /// 连接到云渲染服务器
        /// </summary>
        public async Task<bool> Connect(string preferredRegion = null)
        {
            if (!enableCloudRendering)
            {
                Debug.LogWarning("[CloudRenderingManager] 云渲染未启用");
                return false;
            }

            try
            {
                connectionState = ConnectionState.Connecting;

                // 1. 获取可用的渲染服务器
                var serverInfo = await GetRenderServer(preferredRegion ?? region);
                if (serverInfo == null)
                {
                    throw new Exception("无法获取渲染服务器");
                }

                // 2. 创建 WebRTC 连接
                await CreatePeerConnection(serverInfo);

                // 3. 创建信令连接
                await CreateSignallingConnection(serverInfo);

                // 4. 创建数据通道
                CreateDataChannels();

                // 5. 创建并发送 offer
                await CreateAndSendOffer();

                connectionState = ConnectionState.Connected;
                currentSessionId = serverInfo.sessionId;

                Debug.Log($"[CloudRenderingManager] 云渲染连接成功: {currentSessionId}");
                OnConnected?.Invoke();

                return true;
            }
            catch (Exception e)
            {
                connectionState = ConnectionState.Error;
                Debug.LogError($"[CloudRenderingManager] 连接失败: {e.Message}");
                OnConnectionError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (connectionState == ConnectionState.Disconnected) return;

            connectionState = ConnectionState.Disconnecting;

            // 关闭数据通道
            inputChannel?.Close();
            dataChannel?.Close();

            // 关闭连接
            peerConnection?.Close();

            connectionState = ConnectionState.Disconnected;
            currentSessionId = null;

            Debug.Log("[CloudRenderingManager] 已断开连接");
            OnDisconnected?.Invoke();
        }

        /// <summary>
        /// 获取渲染服务器
        /// </summary>
        private async Task<RenderServerInfo> GetRenderServer(string region)
        {
            // 这里应该调用 API 获取服务器信息
            // 简化实现：返回模拟数据
            await Task.Delay(500);

            return new RenderServerInfo
            {
                sessionId = Guid.NewGuid().ToString("N").Substring(0, 16),
                signallingUrl = signallingServerUrl,
                stunServers = new[] { "stun:stun.l.google.com:19302" },
                turnServers = new string[0],
                region = region,
                gpuType = "NVIDIA RTX 4090",
                availableStreams = 10
            };
        }

        /// <summary>
        /// 创建 WebRTC 连接
        /// </summary>
        private async Task CreatePeerConnection(RenderServerInfo serverInfo)
        {
            var config = new RTCConfiguration
            {
                iceServers = new RTCIceServer[]
                {
                    new RTCIceServer { urls = serverInfo.stunServers }
                }
            };

            peerConnection = new RTCPeerConnection(ref config);

            // 设置视频接收回调
            peerConnection.OnTrack = evt =>
            {
                if (evt.Track is VideoStreamTrack videoTrack)
                {
                    videoTrack.OnVideoReceived += tex =>
                    {
                        Graphics.Blit(tex, receiveTexture);
                    };
                }
            };

            // 监听连接状态
            peerConnection.OnConnectionStateChange = state =>
            {
                Debug.Log($"[CloudRenderingManager] 连接状态: {state}");
                if (state == RTCPeerConnectionState.Connected)
                {
                    connectionState = ConnectionState.Connected;
                }
                else if (state == RTCPeerConnectionState.Disconnected ||
                         state == RTCPeerConnectionState.Failed)
                {
                    connectionState = ConnectionState.Error;
                }
            };

            await Task.Yield();
        }

        /// <summary>
        /// 创建信令连接
        /// </summary>
        private async Task CreateSignallingConnection(RenderServerInfo serverInfo)
        {
            // 这里应该建立 WebSocket 连接
            await Task.Delay(100);
        }

        /// <summary>
        /// 创建数据通道
        /// </summary>
        private void CreateDataChannels()
        {
            // 输入通道
            inputChannel = peerConnection.CreateDataChannel("input", new RTCDataChannelInit
            {
                ordered = false,
                maxRetransmits = 0
            });

            // 数据通道
            dataChannel = peerConnection.CreateDataChannel("data", new RTCDataChannelInit
            {
                ordered = true
            });
        }

        /// <summary>
        /// 创建并发送 offer
        /// </summary>
        private async Task CreateAndSendOffer()
        {
            var offer = peerConnection.CreateOffer();
            await peerConnection.SetLocalDescription(ref offer);

            // 发送 offer 到服务器
            // 等待 answer
            // 设置 remote description

            await Task.Delay(1000);
        }

        #endregion

        #region 输入转发

        /// <summary>
        /// 发送头部追踪数据
        /// </summary>
        public void SendHeadTracking(Vector3 position, Quaternion rotation)
        {
            if (!IsConnected) return;

            var input = new InputEvent
            {
                type = InputType.HeadTracking,
                timestamp = Time.time,
                position = position,
                rotation = rotation
            };

            inputQueue.Enqueue(input);
        }

        /// <summary>
        /// 发送控制器输入
        /// </summary>
        public void SendControllerInput(int controllerIndex, Vector3 position, Quaternion rotation,
            bool trigger, bool grip, Vector2 thumbstick)
        {
            if (!IsConnected) return;

            var input = new InputEvent
            {
                type = InputType.Controller,
                controllerIndex = controllerIndex,
                timestamp = Time.time,
                position = position,
                rotation = rotation,
                trigger = trigger,
                grip = grip,
                thumbstick = thumbstick
            };

            inputQueue.Enqueue(input);
        }

        /// <summary>
        /// 发送手势输入
        /// </summary>
        public void SendHandGesture(int handIndex, HandGesture gesture, Vector3[] jointPositions)
        {
            if (!IsConnected) return;

            var input = new InputEvent
            {
                type = InputType.HandGesture,
                controllerIndex = handIndex,
                timestamp = Time.time,
                gesture = gesture,
                jointPositions = jointPositions
            };

            inputQueue.Enqueue(input);
        }

        /// <summary>
        /// 发送待处理的输入
        /// </summary>
        private void SendPendingInputs()
        {
            if (inputChannel?.ReadyState != RTCDataChannelState.Open) return;

            var inputs = new List<InputEvent>();
            while (inputQueue.Count > 0 && inputs.Count < 10)
            {
                inputs.Add(inputQueue.Dequeue());
            }

            if (inputs.Count > 0)
            {
                var json = JsonUtility.ToJson(new InputBatch { inputs = inputs.ToArray() });
                inputChannel.Send(json);
                lastInputSendTime = Time.time;
            }
        }

        #endregion

        #region 质量自适应

        /// <summary>
        /// 更新流统计
        /// </summary>
        private void UpdateStreamingStats()
        {
            // 从 WebRTC 获取统计信息
            // 简化实现：模拟数据
            stats.frameRate = targetFrameRate;
            stats.bitrateKbps = bitrateKbps;
            stats.packetLoss = UnityEngine.Random.Range(0f, 0.1f);
            stats.latencyMs = UnityEngine.Random.Range(20, 150);
            stats.resolutionX = targetResolutionX;
            stats.resolutionY = targetResolutionY;
            stats.decoderTimeMs = UnityEngine.Random.Range(5f, 20f);

            OnStatsUpdated?.Invoke(stats);
        }

        /// <summary>
        /// 调整码率
        /// </summary>
        private void AdjustBitrate()
        {
            if (stats.packetLoss > packetLossThreshold)
            {
                // 丢包率高，降低码率
                bitrateKbps = Mathf.Max(minBitrateKbps, (int)(bitrateKbps * 0.8f));
                Debug.Log($"[CloudRenderingManager] 降低码率至 {bitrateKbps} kbps");
            }
            else if (stats.latencyMs < latencyThresholdMs && stats.packetLoss < 0.01f)
            {
                // 延迟低且丢包少，可以提高码率
                bitrateKbps = Mathf.Min(maxBitrateKbps, (int)(bitrateKbps * 1.1f));
                Debug.Log($"[CloudRenderingManager] 提高码率至 {bitrateKbps} kbps");
            }

            // 应用新的码率
            ApplyBitrate();
        }

        /// <summary>
        /// 应用码率设置
        /// </summary>
        private void ApplyBitrate()
        {
            // 通过数据通道发送码率调整请求
            dataChannel?.Send($"{{\"type\":\"bitrate\",\"value\":{bitrateKbps}}}");
        }

        /// <summary>
        /// 请求分辨率变更
        /// </summary>
        public void RequestResolution(int width, int height)
        {
            if (!IsConnected) return;

            targetResolutionX = width;
            targetResolutionY = height;

            dataChannel?.Send($"{{\"type\":\"resolution\",\"width\":{width},\"height\":{height}}}");

            // 重新创建接收纹理
            receiveTexture?.Release();
            CreateReceiveTexture();

            OnResolutionChanged?.Invoke(width, height);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取视频材质
        /// </summary>
        public Material GetVideoMaterial()
        {
            return videoMaterial;
        }

        /// <summary>
        /// 检查设备是否支持云渲染
        /// </summary>
        public static bool IsDeviceSupported()
        {
            // 检查网络连接
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return false;
            }

            // 检查硬件解码支持
            return true; // 简化实现
        }

        /// <summary>
        /// 获取推荐的渲染设置
        /// </summary>
        public static RenderingSettings GetRecommendedSettings()
        {
            // 根据设备性能返回推荐设置
            var settings = new RenderingSettings
            {
                resolutionX = 1920,
                resolutionY = 1080,
                frameRate = 60,
                bitrateKbps = 20000
            };

            // 移动设备降低设置
            if (Application.isMobilePlatform)
            {
                settings.resolutionX = 1280;
                settings.resolutionY = 720;
                settings.frameRate = 30;
                settings.bitrateKbps = 8000;
            }

            return settings;
        }

        #endregion
    }

    #region 数据类型

    /// <summary>
    /// 连接状态
    /// </summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
        Error
    }

    /// <summary>
    /// 渲染服务器信息
    /// </summary>
    public class RenderServerInfo
    {
        public string sessionId;
        public string signallingUrl;
        public string[] stunServers;
        public string[] turnServers;
        public string region;
        public string gpuType;
        public int availableStreams;
    }

    /// <summary>
    /// 流统计
    /// </summary>
    public class StreamingStats
    {
        public int frameRate;
        public int bitrateKbps;
        public float packetLoss;
        public int latencyMs;
        public int resolutionX;
        public int resolutionY;
        public float decoderTimeMs;
    }

    /// <summary>
    /// 输入事件
    /// </summary>
    public class InputEvent
    {
        public InputType type;
        public int controllerIndex;
        public float timestamp;
        public Vector3 position;
        public Quaternion rotation;
        public bool trigger;
        public bool grip;
        public Vector2 thumbstick;
        public HandGesture gesture;
        public Vector3[] jointPositions;
    }

    /// <summary>
    /// 输入批量
    /// </summary>
    public class InputBatch
    {
        public InputEvent[] inputs;
    }

    /// <summary>
    /// 输入类型
    /// </summary>
    public enum InputType
    {
        HeadTracking,
        Controller,
        HandGesture,
        Keyboard,
        Mouse
    }

    /// <summary>
    /// 手势类型
    /// </summary>
    public enum HandGesture
    {
        None,
        Open,
        Fist,
        Point,
        Pinch,
        ThumbsUp
    }

    /// <summary>
    /// 渲染设置
    /// </summary>
    public class RenderingSettings
    {
        public int resolutionX;
        public int resolutionY;
        public int frameRate;
        public int bitrateKbps;
    }

    #endregion
}
