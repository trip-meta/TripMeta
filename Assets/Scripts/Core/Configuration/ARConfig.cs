using UnityEngine;

namespace TripMeta.Core.Configuration
{
    /// <summary>
    /// AR配置
    /// </summary>
    [CreateAssetMenu(fileName = "ARConfig", menuName = "TripMeta/Config/AR Config")]
    public class ARConfig : ScriptableObject
    {
        [Header("Azure Computer Vision")]
        [Tooltip("Azure Vision API 密钥")]
        public string VisionApiKey = "";

        [Tooltip("Azure Vision API 端点")]
        public string VisionEndpoint = "https://<your-region>.api.cognitive.microsoft.com/vision/v3.2";

        [Header("AR设置")]
        [Tooltip("启用AR功能")]
        public bool EnableAR = true;

        [Tooltip("自动开始AR")]
        public bool AutoStartAR = false;

        [Tooltip("扫描间隔（秒）")]
        [Range(1f, 10f)]
        public float ScanInterval = 3f;

        [Tooltip("识别置信度阈值")]
        [Range(0.5f, 1f)]
        public float RecognitionThreshold = 0.7f;

        [Tooltip("最大识别距离（米）")]
        [Range(5f, 100f)]
        public float MaxRecognitionDistance = 50f;

        [Header("AR卡片设置")]
        [Tooltip("AR卡片预制体")]
        public GameObject ARCardPrefab;

        [Tooltip("导航箭头预制体")]
        public GameObject NavigationArrowPrefab;

        [Tooltip("信息面板预制体")]
        public GameObject InfoPanelPrefab;

        [Tooltip("卡片显示距离")]
        [Range(1f, 10f)]
        public float CardDisplayDistance = 3f;

        [Tooltip("卡片高度偏移")]
        [Range(0.5f, 3f)]
        public float CardHeightOffset = 1.5f;

        [Header("视觉效果")]
        [Tooltip("启用卡片动画")]
        public bool EnableCardAnimations = true;

        [Tooltip("卡片淡入时间（秒）")]
        [Range(0.1f, 2f)]
        public float CardFadeInDuration = 0.5f;

        [Tooltip("卡片始终面向相机")]
        public bool CardsFaceCamera = true;

        [Tooltip("导航箭头浮动高度")]
        [Range(0f, 1f)]
        public float ArrowBobbingHeight = 0.1f;

        [Header("调试")]
        [Tooltip("显示调试信息")]
        public bool ShowDebugInfo = false;

        [Tooltip("模拟AR模式（无需真实AR设备）")]
        public bool SimulateARMode = false;

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(VisionApiKey) &&
                   !string.IsNullOrEmpty(VisionEndpoint) &&
                   EnableAR;
        }
    }
}
