using UnityEngine;

namespace TripMeta.Core.Configuration
{
    /// <summary>
    /// 多人游戏配置
    /// </summary>
    [CreateAssetMenu(fileName = "MultiplayerConfig", menuName = "TripMeta/Config/Multiplayer Config")]
    public class MultiplayerConfig : ScriptableObject
    {
        [Header("网络设置")]
        [Tooltip("默认服务器地址")]
        public string DefaultServerAddress = "127.0.0.1";

        [Tooltip("默认端口")]
        [Range(1000, 65535)]
        public int DefaultPort = 7777;

        [Tooltip("最大连接数")]
        [Range(2, 16)]
        public int MaxConnections = 8;

        [Tooltip("连接超时时间（秒）")]
        [Range(5, 60)]
        public int ConnectionTimeout = 30;

        [Header("VR玩家同步")]
        [Tooltip("同步频率(Hz)")]
        [Range(10, 60)]
        public int SyncRate = 20;

        [Tooltip("位置同步阈值")]
        [Range(0.001f, 0.1f)]
        public float PositionThreshold = 0.01f;

        [Tooltip("旋转同步阈值（度）")]
        [Range(0.1f, 10f)]
        public float RotationThreshold = 1f;

        [Tooltip("插值速度")]
        [Range(0.1f, 1f)]
        public float InterpolationSpeed = 0.3f;

        [Header("语音聊天")]
        [Tooltip("启用语音聊天")]
        public bool EnableVoiceChat = true;

        [Tooltip("语音质量（采样率）")]
        public VoiceQuality VoiceQuality = VoiceQuality.Medium;

        [Tooltip("语音压缩")]
        public bool CompressVoice = true;

        [Header("导游同步")]
        [Tooltip("启用导游状态同步")]
        public bool SyncTourGuideState = true;

        [Tooltip("同步间隔（秒）")]
        [Range(0.1f, 5f)]
        public float TourSyncInterval = 1f;

        /// <summary>
        /// 验证配置
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(DefaultServerAddress) &&
                   DefaultPort > 0 &&
                   MaxConnections >= 2;
        }
    }

    /// <summary>
    /// 语音质量
    /// </summary>
    public enum VoiceQuality
    {
        Low,      // 8kHz
        Medium,   // 16kHz
        High,     // 22kHz
        Ultra     // 44kHz
    }
}
