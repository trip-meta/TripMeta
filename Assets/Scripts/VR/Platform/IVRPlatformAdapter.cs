using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.VR.Platform
{
    /// <summary>
    /// VR 平台适配器接口
    /// 定义跨平台 VR 功能的统一接口
    /// </summary>
    public interface IVRPlatformAdapter
    {
        /// <summary>
        /// 平台类型
        /// </summary>
        VRPlatformType PlatformType { get; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 异步初始化
        /// </summary>
        Task<bool> InitializeAsync();

        /// <summary>
        /// 启动追踪
        /// </summary>
        void StartTracking();

        /// <summary>
        /// 停止追踪
        /// </summary>
        void StopTracking();

        /// <summary>
        /// 初始化完成事件
        /// </summary>
        event Action<bool> OnInitializationComplete;
    }
}
