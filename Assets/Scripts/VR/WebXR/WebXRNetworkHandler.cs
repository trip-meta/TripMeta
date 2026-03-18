using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TripMeta.VR.WebXR
{
    /// <summary>
    /// WebXR 网络处理器
    /// 处理云渲染、信令服务器连接、数据同步
    /// </summary>
    public class WebXRNetworkHandler : MonoBehaviour
    {
        [Header("网络配置")]
        public string signallingServerUrl = "wss://tripmeta.io/signalling";
        public int reconnectAttempts = 3;
        public float reconnectDelay = 5f;
        public int connectionTimeout = 10;

        [Header("云渲染")]
        public bool enableCloudRendering = false;
        public string cloudRenderingEndpoint = "";
        public int cloudRenderingBitrate = 20000000;
        public int cloudRenderingFps = 60;
        public float latencyThreshold = 100f;

        // 连接状态
        private bool isConnected = false;
        private bool isConnecting = false;
        private int currentReconnectAttempt = 0;

        // 事件
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        public event Action<float> OnLatencyUpdated;

        public bool IsConnected => isConnected;
        public float CurrentLatency { get; private set; } = 0f;

        public void Initialize()
        {
            Debug.Log("[WebXRNetworkHandler] 初始化网络处理器");

            if (enableCloudRendering)
            {
                _ = ConnectToCloudRenderingAsync();
            }
            else
            {
                _ = ConnectToSignallingServerAsync();
            }
        }

        /// <summary>
        /// 连接到信令服务器
        /// </summary>
        private async Task ConnectToSignallingServerAsync()
        {
            if (isConnecting || isConnected) return;

            isConnecting = true;
            Debug.Log($"[WebXRNetworkHandler] 连接到信令服务器: {signallingServerUrl}");

            try
            {
                // WebSocket 连接逻辑
                #if UNITY_WEBGL && !UNITY_EDITOR
                // 在 WebGL 中使用 JavaScript WebSocket
                await ConnectWebSocketJS(signallingServerUrl);
                #else
                // 编辑器模拟
                await Task.Delay(100);
                isConnected = true;
                #endif

                if (isConnected)
                {
                    currentReconnectAttempt = 0;
                    OnConnected?.Invoke();
                    Debug.Log("[WebXRNetworkHandler] 信令服务器连接成功");

                    // 启动延迟监测
                    StartCoroutine(LatencyMonitorCoroutine());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebXRNetworkHandler] 连接失败: {ex.Message}");
                OnError?.Invoke(ex.Message);
                await HandleReconnect();
            }
            finally
            {
                isConnecting = false;
            }
        }

        /// <summary>
        /// 连接到云渲染服务
        /// </summary>
        private async Task ConnectToCloudRenderingAsync()
        {
            if (string.IsNullOrEmpty(cloudRenderingEndpoint))
            {
                Debug.LogError("[WebXRNetworkHandler] 云渲染端点未配置");
                return;
            }

            Debug.Log($"[WebXRNetworkHandler] 连接到云渲染服务: {cloudRenderingEndpoint}");

            try
            {
                // 云渲染连接逻辑
                await Task.Delay(200);
                isConnected = true;
                OnConnected?.Invoke();
                Debug.Log("[WebXRNetworkHandler] 云渲染服务连接成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebXRNetworkHandler] 云渲染连接失败: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// 处理重连
        /// </summary>
        private async Task HandleReconnect()
        {
            if (currentReconnectAttempt >= reconnectAttempts)
            {
                Debug.LogError("[WebXRNetworkHandler] 重连次数耗尽");
                return;
            }

            currentReconnectAttempt++;
            Debug.Log($"[WebXRNetworkHandler] 尝试重连 {currentReconnectAttempt}/{reconnectAttempts}");

            await Task.Delay((int)(reconnectDelay * 1000));
            await ConnectToSignallingServerAsync();
        }

        /// <summary>
        /// 延迟监测协程
        /// </summary>
        private IEnumerator LatencyMonitorCoroutine()
        {
            while (isConnected)
            {
                yield return new WaitForSeconds(1f);

                _ = MeasureLatencyAsync();
            }
        }

        /// <summary>
        /// 测量延迟
        /// </summary>
        private async Task MeasureLatencyAsync()
        {
            float startTime = Time.realtimeSinceStartup;

            // 发送 ping 请求
            #if UNITY_WEBGL && !UNITY_EDITOR
            await PingServerJS();
            #else
            await Task.Delay(20);
            #endif

            CurrentLatency = (Time.realtimeSinceStartup - startTime) * 1000f;
            OnLatencyUpdated?.Invoke(CurrentLatency);

            // 检查延迟是否超过阈值
            if (CurrentLatency > latencyThreshold)
            {
                Debug.LogWarning($"[WebXRNetworkHandler] 延迟过高: {CurrentLatency:F1}ms");
            }
        }

        /// <summary>
        /// 发送数据到服务器
        /// </summary>
        public async Task SendDataAsync(byte[] data)
        {
            if (!isConnected)
            {
                Debug.LogWarning("[WebXRNetworkHandler] 未连接，无法发送数据");
                return;
            }

            #if UNITY_WEBGL && !UNITY_EDITOR
            await SendDataJS(data);
            #else
            await Task.Delay(10);
            #endif
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!isConnected) return;

            Debug.Log("[WebXRNetworkHandler] 断开连接");

            #if UNITY_WEBGL && !UNITY_EDITOR
            await DisconnectJS();
            #else
            await Task.Delay(50);
            #endif

            isConnected = false;
            OnDisconnected?.Invoke();
        }

        #region JavaScript 互操作 (WebGL)

        #if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern Task ConnectWebSocketJS(string url);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern Task PingServerJS();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern Task SendDataJS(byte[] data);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern Task DisconnectJS();
        #endif

        #endregion

        void OnDestroy()
        {
            _ = DisconnectAsync();
        }
    }
}
