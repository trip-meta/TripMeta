using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace TripMeta.Features.Multiplayer
{
    /// <summary>
    /// 多人游戏服务接口
    /// </summary>
    public interface IMultiplayerService
    {
        /// <summary>
        /// 是否已连接到服务器
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 是否是主机
        /// </summary>
        bool IsHost { get; }

        /// <summary>
        /// 当前连接的客户端数量
        /// </summary>
        int ConnectedClientCount { get; }

        /// <summary>
        /// 当前玩家ID
        /// </summary>
        ulong LocalClientId { get; }

        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        event Action<bool> OnConnectionStatusChanged;

        /// <summary>
        /// 玩家加入事件
        /// </summary>
        event Action<ulong> OnPlayerJoined;

        /// <summary>
        /// 玩家离开事件
        /// </summary>
        event Action<ulong> OnPlayerLeft;

        /// <summary>
        /// 语音聊天数据接收事件
        /// </summary>
        event Action<ulong, byte[]> OnVoiceChatReceived;

        /// <summary>
        /// 初始化多人服务
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 创建房间（作为主机）
        /// </summary>
        /// <param name="roomName">房间名称</param>
        /// <param name="maxPlayers">最大玩家数</param>
        /// <returns>是否成功创建</returns>
        Task<bool> CreateRoomAsync(string roomName, int maxPlayers = 8);

        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="roomCode">房间代码</param>
        /// <returns>是否成功加入</returns>
        Task<bool> JoinRoomAsync(string roomCode);

        /// <summary>
        /// 离开当前房间
        /// </summary>
        Task LeaveRoomAsync();

        /// <summary>
        /// 断开连接
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// 获取房间中的玩家列表
        /// </summary>
        /// <returns>玩家信息列表</returns>
        List<PlayerInfo> GetConnectedPlayers();

        /// <summary>
        /// 发送语音聊天数据
        /// </summary>
        /// <param name="audioData">音频数据</param>
        Task SendVoiceChatAsync(byte[] audioData);

        /// <summary>
        /// 同步导游状态
        /// </summary>
        /// <param name="tourState">导游状态</param>
        Task SyncTourGuideStateAsync(TourGuideSyncState tourState);

        /// <summary>
        /// 获取网络延迟（毫秒）
        /// </summary>
        /// <returns>延迟时间</returns>
        int GetNetworkLatency();
    }

    /// <summary>
    /// 玩家信息
    /// </summary>
    public struct PlayerInfo
    {
        public ulong ClientId;
        public string PlayerName;
        public bool IsHost;
        public Vector3 Position;
        public Quaternion Rotation;
        public PlayerStatus Status;
    }

    /// <summary>
    /// 玩家状态
    /// </summary>
    public enum PlayerStatus
    {
        Connected,
        InTour,
        Speaking,
        AFK,
        Disconnected
    }

    /// <summary>
    /// 导游同步状态
    /// </summary>
    public struct TourGuideSyncState : INetworkSerializable
    {
        public int CurrentAttractionIndex;
        public string CurrentGuideText;
        public float GuideProgress;
        public bool IsSpeaking;
        public Vector3 GuidePosition;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CurrentAttractionIndex);
            serializer.SerializeValue(ref CurrentGuideText);
            serializer.SerializeValue(ref GuideProgress);
            serializer.SerializeValue(ref IsSpeaking);
            serializer.SerializeValue(ref GuidePosition);
        }
    }
}
