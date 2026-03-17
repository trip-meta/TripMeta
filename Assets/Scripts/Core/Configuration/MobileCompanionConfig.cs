using UnityEngine;

namespace TripMeta.Core.Configuration
{
    /// <summary>
    /// 移动伴侣配置
    /// </summary>
    [CreateAssetMenu(fileName = "MobileCompanionConfig", menuName = "TripMeta/Config/Mobile Companion Config")]
    public class MobileCompanionConfig : ScriptableObject
    {
        [Header("服务器设置")]
        [Tooltip("服务器URL")]
        public string ServerUrl = "https://api.tripmeta.com";

        [Tooltip("连接端口")]
        [Range(1000, 65535)]
        public int ConnectionPort = 8080;

        [Tooltip("心跳间隔（秒）")]
        [Range(1f, 30f)]
        public float HeartbeatInterval = 5f;

        [Header("配对设置")]
        [Tooltip("配对码长度")]
        [Range(4, 8)]
        public int PairingCodeLength = 6;

        [Tooltip("配对超时时间（秒）")]
        [Range(60, 600)]
        public int PairingTimeout = 300;

        [Tooltip("自动接受可信设备")]
        public bool AutoAcceptTrustedDevices = true;

        [Header("VR状态同步")]
        [Tooltip("启用VR状态同步")]
        public bool EnableVRStateSync = true;

        [Tooltip("状态同步间隔（秒）")]
        [Range(1f, 10f)]
        public float StateSyncInterval = 2f;

        [Tooltip("同步电池状态")]
        public bool SyncBatteryLevel = true;

        [Tooltip("同步网络延迟")]
        public bool SyncNetworkLatency = true;

        [Header("通知设置")]
        [Tooltip("启用推送通知")]
        public bool EnablePushNotifications = true;

        [Tooltip("通知保持时间（秒）")]
        [Range(5, 60)]
        public int NotificationDuration = 10;

        [Tooltip("通知声音")]
        public bool NotificationSound = true;

        [Tooltip("通知震动")]
        public bool NotificationVibration = true;

        [Header("功能开关")]
        [Tooltip("启用远程控制")]
        public bool EnableRemoteControl = true;

        [Tooltip("启用聊天功能")]
        public bool EnableChat = true;

        [Tooltip("启用景点信息同步")]
        public bool EnableAttractionSync = true;

        [Tooltip("启用拍照功能")]
        public bool EnablePhotoCapture = true;

        [Tooltip("启用录音功能")]
        public bool EnableVoiceRecording = true;

        [Header("调试")]
        [Tooltip("模拟模式（无需真实服务器）")]
        public bool SimulateMode = false;

        [Tooltip("显示调试日志")]
        public bool ShowDebugLogs = false;

        /// <summary>
        /// 验证配置
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ServerUrl) &&
                   ConnectionPort > 0 &&
                   ConnectionPort <= 65535;
        }
    }
}
