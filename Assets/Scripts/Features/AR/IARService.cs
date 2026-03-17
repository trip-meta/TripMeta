using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Features.AR
{
    /// <summary>
    /// AR服务接口 - 提供景点增强现实叠加功能
    /// </summary>
    public interface IARService
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 是否正在扫描
        /// </summary>
        bool IsScanning { get; }

        /// <summary>
        /// AR就绪事件
        /// </summary>
        event Action OnARReady;

        /// <summary>
        /// 景点识别事件
        /// </summary>
        event Action<AttractionRecognitionResult> OnAttractionRecognized;

        /// <summary>
        /// AR信息叠加点击事件
        /// </summary>
        event Action<AROverlayInfo> OnOverlayClicked;

        /// <summary>
        /// 初始化AR服务
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 开始AR体验
        /// </summary>
        Task StartARExperienceAsync();

        /// <summary>
        /// 停止AR体验
        /// </summary>
        Task StopARExperienceAsync();

        /// <summary>
        /// 扫描当前景点
        /// </summary>
        /// <param name="cameraTexture">相机图像</param>
        /// <returns>识别结果</returns>
        Task<AttractionRecognitionResult> ScanAttractionAsync(Texture2D cameraTexture);

        /// <summary>
        /// 获取景点信息叠加
        /// </summary>
        /// <param name="attractionId">景点ID</param>
        /// <returns>AR叠加信息</returns>
        Task<List<AROverlayInfo>> GetAttractionOverlaysAsync(string attractionId);

        /// <summary>
        /// 在指定位置放置AR信息卡片
        /// </summary>
        /// <param name="position">世界坐标位置</param>
        /// <param name="info">信息内容</param>
        /// <returns>创建的AR对象</returns>
        GameObject PlaceARCard(Vector3 position, AROverlayInfo info);

        /// <summary>
        /// 移除所有AR叠加
        /// </summary>
        void ClearAllOverlays();

        /// <summary>
        /// 设置AR可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        void SetARVisibility(bool visible);

        /// <summary>
        /// 获取支持的AR功能
        /// </summary>
        /// <returns>AR功能列表</returns>
        List<ARCapability> GetSupportedCapabilities();
    }

    /// <summary>
    /// 景点识别结果
    /// </summary>
    public class AttractionRecognitionResult
    {
        /// <summary>
        /// 是否识别成功
        /// </summary>
        public bool IsRecognized { get; set; }

        /// <summary>
        /// 景点ID
        /// </summary>
        public string AttractionId { get; set; }

        /// <summary>
        /// 景点名称
        /// </summary>
        public string AttractionName { get; set; }

        /// <summary>
        /// 置信度 (0-1)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// 景点位置（相对于相机）
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// 景点边界框
        /// </summary>
        public Rect BoundingBox { get; set; }

        /// <summary>
        /// 识别到的地标信息
        /// </summary>
        public List<LandmarkInfo> Landmarks { get; set; } = new List<LandmarkInfo>();

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 地标信息
    /// </summary>
    public class LandmarkInfo
    {
        /// <summary>
        /// 地标名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 地标类型
        /// </summary>
        public LandmarkType Type { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// 置信度
        /// </summary>
        public float Confidence { get; set; }
    }

    /// <summary>
    /// 地标类型
    /// </summary>
    public enum LandmarkType
    {
        Building,
        Sculpture,
        Gate,
        Tower,
        Bridge,
        Monument,
        NaturalFeature,
        Other
    }

    /// <summary>
    /// AR叠加信息
    /// </summary>
    public class AROverlayInfo
    {
        /// <summary>
        /// 叠加ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 描述内容
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 图片URL
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 音频URL
        /// </summary>
        public string AudioUrl { get; set; }

        /// <summary>
        /// 叠加类型
        /// </summary>
        public OverlayType Type { get; set; }

        /// <summary>
        /// 世界坐标位置
        /// </summary>
        public Vector3 WorldPosition { get; set; }

        /// <summary>
        /// 屏幕坐标位置（用于2D UI）
        /// </summary>
        public Vector2 ScreenPosition { get; set; }

        /// <summary>
        /// 关联的景点ID
        /// </summary>
        public string AttractionId { get; set; }

        /// <summary>
        /// 额外数据
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// 叠加类型
    /// </summary>
    public enum OverlayType
    {
        InfoCard,
        HistoricalPhoto,
        AudioGuide,
        NavigationArrow,
        DistanceMarker,
        FunFact,
        InteractiveElement
    }

    /// <summary>
    /// AR功能
    /// </summary>
    public enum ARCapability
    {
        ImageRecognition,
        ObjectDetection,
        PlaneDetection,
        SpatialMapping,
        FaceTracking,
        HandTracking
    }
}
