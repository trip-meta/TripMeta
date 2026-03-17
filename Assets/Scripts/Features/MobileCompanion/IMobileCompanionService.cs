using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Features.MobileCompanion
{
    /// <summary>
    /// 移动伴侣服务接口 - 提供移动端配套应用功能
    /// </summary>
    public interface IMobileCompanionService
    {
        /// <summary>
        /// 是否已连接到移动应用
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 已配对的设备ID
        /// </summary>
        string PairedDeviceId { get; }

        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        event Action<bool> OnConnectionStateChanged;

        /// <summary>
        /// 接收到远程命令事件
        /// </summary>
        event Action<RemoteCommand> OnRemoteCommandReceived;

        /// <summary>
        /// 接收到聊天消息事件
        /// </summary>
        event Action<ChatMessage> OnChatMessageReceived;

        /// <summary>
        /// 设备配对请求事件
        /// </summary>
        event Action<PairingRequest> OnPairingRequestReceived;

        /// <summary>
        /// 初始化服务
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 开始配对模式
        /// </summary>
        /// <param name="pairingCode">配对码</param>
        /// <returns>是否成功启动配对</returns>
        Task<bool> StartPairingAsync(string pairingCode);

        /// <summary>
        /// 接受配对请求
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        Task AcceptPairingAsync(string deviceId);

        /// <summary>
        /// 断开与移动应用的连接
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// 发送当前VR状态到移动应用
        /// </summary>
        /// <param name="state">VR状态</param>
        Task SendVRStateAsync(VRState state);

        /// <summary>
        /// 发送景点信息到移动应用
        /// </summary>
        /// <param name="attractionInfo">景点信息</param>
        Task SendAttractionInfoAsync(AttractionMobileInfo attractionInfo);

        /// <summary>
        /// 发送通知到移动应用
        /// </summary>
        /// <param name="notification">通知内容</param>
        Task SendNotificationAsync(MobileNotification notification);

        /// <summary>
        /// 执行远程命令
        /// </summary>
        /// <param name="command">命令</param>
        Task ExecuteRemoteCommandAsync(RemoteCommand command);

        /// <summary>
        /// 获取配对历史
        /// </summary>
        /// <returns>配对设备列表</returns>
        List<PairedDeviceInfo> GetPairedDeviceHistory();

        /// <summary>
        /// 移除配对设备
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        Task RemovePairedDeviceAsync(string deviceId);
    }

    /// <summary>
    /// VR状态
    /// </summary>
    public struct VRState
    {
        /// <summary>
        /// 当前景点ID
        /// </summary>
        public string CurrentAttractionId;

        /// <summary>
        /// 当前景点名称
        /// </summary>
        public string CurrentAttractionName;

        /// <summary>
        /// 体验进度 (0-100)
        /// </summary>
        public int Progress;

        /// <summary>
        /// 是否正在播放语音
        /// </summary>
        public bool IsSpeaking;

        /// <summary>
        /// 当前语音文本
        /// </summary>
        public string CurrentSpeechText;

        /// <summary>
        /// 连接的多人玩家数
        /// </summary>
        public int ConnectedPlayers;

        /// <summary>
        /// 电池电量 (0-100)
        /// </summary>
        public int BatteryLevel;

        /// <summary>
        /// 网络延迟 (ms)
        /// </summary>
        public int NetworkLatency;

        /// <summary>
        /// 更新时间戳
        /// </summary>
        public long Timestamp;
    }

    /// <summary>
    /// 景点移动端信息
    /// </summary>
    public class AttractionMobileInfo
    {
        /// <summary>
        /// 景点ID
        /// </summary>
        public string Id;

        /// <summary>
        /// 景点名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 描述
        /// </summary>
        public string Description;

        /// <summary>
        /// 缩略图URL
        /// </summary>
        public string ThumbnailUrl;

        /// <summary>
        /// 历史照片列表
        /// </summary>
        public List<string> HistoricalPhotos;

        /// <summary>
        /// 音频讲解URL
        /// </summary>
        public string AudioGuideUrl;

        /// <summary>
        /// 趣味事实
        /// </summary>
        public List<string> FunFacts;

        /// <summary>
        /// 访问人数
        /// </summary>
        public int VisitorCount;

        /// <summary>
        /// 评分 (0-5)
        /// </summary>
        public float Rating;
    }

    /// <summary>
    /// 远程命令
    /// </summary>
    public struct RemoteCommand
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        public CommandType Type;

        /// <summary>
        /// 命令参数
        /// </summary>
        public string Parameter;

        /// <summary>
        /// 发送时间戳
        /// </summary>
        public long Timestamp;
    }

    /// <summary>
    /// 命令类型
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// 暂停体验
        /// </summary>
        Pause,

        /// <summary>
        /// 继续体验
        /// </summary>
        Resume,

        /// <summary>
        /// 跳转到指定景点
        /// </summary>
        JumpToAttraction,

        /// <summary>
        /// 调整音量
        /// </summary>
        AdjustVolume,

        /// <summary>
        /// 拍照
        /// </summary>
        TakePhoto,

        /// <summary>
        /// 开始录音
        /// </summary>
        StartRecording,

        /// <summary>
        /// 停止录音
        /// </summary>
        StopRecording,

        /// <summary>
        /// 请求帮助
        /// </summary>
        RequestHelp,

        /// <summary>
        /// 返回主菜单
        /// </summary>
        ReturnToMenu
    }

    /// <summary>
    /// 聊天消息
    /// </summary>
    public struct ChatMessage
    {
        /// <summary>
        /// 发送者名称
        /// </summary>
        public string SenderName;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content;

        /// <summary>
        /// 发送时间戳
        /// </summary>
        public long Timestamp;

        /// <summary>
        /// 是否是系统消息
        /// </summary>
        public bool IsSystemMessage;
    }

    /// <summary>
    /// 移动通知
    /// </summary>
    public class MobileNotification
    {
        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title;

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Message;

        /// <summary>
        /// 通知类型
        /// </summary>
        public NotificationType Type;

        /// <summary>
        /// 关联的景点ID
        /// </summary>
        public string AttractionId;

        /// <summary>
        /// 图片URL
        /// </summary>
        public string ImageUrl;

        /// <summary>
        /// 操作按钮文本
        /// </summary>
        public string ActionButtonText;
    }

    /// <summary>
    /// 通知类型
    /// </summary>
    public enum NotificationType
    {
        Info,
        Achievement,
        Social,
        System,
        Alert
    }

    /// <summary>
    /// 配对请求
    /// </summary>
    public struct PairingRequest
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId;

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName;

        /// <summary>
        /// 配对码
        /// </summary>
        public string PairingCode;

        /// <summary>
        /// 请求时间
        /// </summary>
        public long Timestamp;
    }

    /// <summary>
    /// 已配对设备信息
    /// </summary>
    public class PairedDeviceInfo
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId;

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName;

        /// <summary>
        /// 设备类型 (iOS/Android)
        /// </summary>
        public string DeviceType;

        /// <summary>
        /// 配对时间
        /// </summary>
        public DateTime PairedTime;

        /// <summary>
        /// 最后连接时间
        /// </summary>
        public DateTime LastConnectedTime;

        /// <summary>
        /// 连接次数
        /// </summary>
        public int ConnectionCount;

        /// <summary>
        /// 是否已信任
        /// </summary>
        public bool IsTrusted;
    }
}
