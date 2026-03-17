using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace TripMeta.Features.AR
{
    /// <summary>
    /// AR管理器 - 管理景点增强现实体验
    /// </summary>
    public class ARManager : MonoBehaviour, IARService
    {
        [Header("AR组件")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARCameraManager cameraManager;

        [Header("AR预制体")]
        [SerializeField] private GameObject arCardPrefab;
        [SerializeField] private GameObject navigationArrowPrefab;
        [SerializeField] private GameObject infoPanelPrefab;

        [Header("设置")]
        [SerializeField] private float maxRaycastDistance = 10f;
        [SerializeField] private LayerMask arLayerMask;

        private bool _isInitialized;
        private bool _isScanning;
        private List<GameObject> _activeOverlays = new List<GameObject>();
        private Dictionary<string, AROverlayInfo> _overlayDatabase = new Dictionary<string, AROverlayInfo>();

        // 事件
        public event Action OnARReady;
        public event Action<AttractionRecognitionResult> OnAttractionRecognized;
        public event Action<AROverlayInfo> OnOverlayClicked;

        // 属性
        public bool IsInitialized => _isInitialized;
        public bool IsScanning => _isScanning;

        public static ARManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                Debug.Log("[ARManager] 初始化AR服务...");

                // 检查AR支持
                if (!IsARSupported())
                {
                    Debug.LogWarning("[ARManager] 设备不支持AR功能");
                    return;
                }

                // 查找或创建AR组件
                EnsureARComponents();

                // 初始化AR会话
                if (arSession != null)
                {
                    arSession.stateChanged += OnARSessionStateChanged;
                }

                _isInitialized = true;
                Debug.Log("[ARManager] AR服务初始化完成");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ARManager] 初始化失败: {ex.Message}");
                throw;
            }
        }

        private bool IsARSupported()
        {
            // 检查是否支持AR
            return ARSession.state != ARSessionState.Unsupported;
        }

        private void EnsureARComponents()
        {
            if (arSession == null)
            {
                arSession = FindObjectOfType<ARSession>();
                if (arSession == null)
                {
                    var sessionGO = new GameObject("ARSession");
                    arSession = sessionGO.AddComponent<ARSession>();
                }
            }

            if (xrOrigin == null)
            {
                xrOrigin = FindObjectOfType<XROrigin>();
                if (xrOrigin == null)
                {
                    var originGO = new GameObject("XROrigin");
                    xrOrigin = originGO.AddComponent<XROrigin>();
                }
            }

            if (raycastManager == null)
            {
                raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
                if (raycastManager == null)
                {
                    raycastManager = xrOrigin.gameObject.AddComponent<ARRaycastManager>();
                }
            }

            if (cameraManager == null)
            {
                cameraManager = Camera.main?.GetComponent<ARCameraManager>();
            }
        }

        private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            Debug.Log($"[ARManager] AR会话状态: {args.state}");

            if (args.state == ARSessionState.SessionTracking)
            {
                OnARReady?.Invoke();
            }
        }

        public async Task StartARExperienceAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }

            if (arSession != null)
            {
                arSession.enabled = true;
                Debug.Log("[ARManager] AR体验已启动");
            }
        }

        public async Task StopARExperienceAsync()
        {
            if (arSession != null)
            {
                arSession.enabled = false;
            }

            ClearAllOverlays();
            Debug.Log("[ARManager] AR体验已停止");

            await Task.Delay(100);
        }

        public async Task<AttractionRecognitionResult> ScanAttractionAsync(Texture2D cameraTexture)
        {
            if (!_isInitialized || cameraTexture == null)
            {
                return new AttractionRecognitionResult
                {
                    IsRecognized = false,
                    ErrorMessage = "AR未初始化或图像无效"
                };
            }

            _isScanning = true;

            try
            {
                Debug.Log("[ARManager] 开始扫描景点...");

                // 模拟图像识别过程
                // 实际应该调用 Azure Computer Vision API
                await Task.Delay(1500); // 模拟处理时间

                // 模拟识别结果
                var result = new AttractionRecognitionResult
                {
                    IsRecognized = true,
                    AttractionId = "attraction_001",
                    AttractionName = "模拟景点",
                    Confidence = 0.85f,
                    Position = new Vector3(0, 0, 3), // 前方3米
                    BoundingBox = new Rect(100, 100, 200, 200),
                    Landmarks = new List<LandmarkInfo>
                    {
                        new LandmarkInfo
                        {
                            Name = "主入口",
                            Type = LandmarkType.Gate,
                            Position = new Vector3(0, 0, 3),
                            Confidence = 0.9f
                        }
                    }
                };

                OnAttractionRecognized?.Invoke(result);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ARManager] 扫描失败: {ex.Message}");
                return new AttractionRecognitionResult
                {
                    IsRecognized = false,
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                _isScanning = false;
            }
        }

        public async Task<List<AROverlayInfo>> GetAttractionOverlaysAsync(string attractionId)
        {
            // 返回景点的AR叠加信息
            var overlays = new List<AROverlayInfo>();

            // 模拟数据
            overlays.Add(new AROverlayInfo
            {
                Id = $"{attractionId}_info",
                Title = "景点介绍",
                Description = "这是一个著名的旅游景点，拥有悠久的历史...",
                Type = OverlayType.InfoCard,
                AttractionId = attractionId,
                WorldPosition = new Vector3(0, 1.5f, 3)
            });

            overlays.Add(new AROverlayInfo
            {
                Id = $"{attractionId}_history",
                Title = "历史背景",
                Description = "建于18世纪，是当时最重要的建筑之一...",
                Type = OverlayType.HistoricalPhoto,
                AttractionId = attractionId,
                WorldPosition = new Vector3(-1, 1.5f, 3)
            });

            overlays.Add(new AROverlayInfo
            {
                Id = $"{attractionId}_navigation",
                Title = "导航",
                Description = "距离下一个景点 50米",
                Type = OverlayType.NavigationArrow,
                AttractionId = attractionId,
                WorldPosition = new Vector3(0, 0.5f, 3)
            });

            await Task.Delay(100);
            return overlays;
        }

        public GameObject PlaceARCard(Vector3 position, AROverlayInfo info)
        {
            if (arCardPrefab == null)
            {
                Debug.LogError("[ARManager] AR卡片预制体未设置");
                return null;
            }

            // 在指定位置创建AR卡片
            var arCard = Instantiate(arCardPrefab, position, Quaternion.identity);
            arCard.name = $"ARCard_{info.Id}";

            // 设置AR卡片内容
            var cardComponent = arCard.GetComponent<ARCardController>();
            if (cardComponent != null)
            {
                cardComponent.SetInfo(info);
                cardComponent.OnClicked += () => OnOverlayClicked?.Invoke(info);
            }

            // 让AR卡片面向相机
            if (Camera.main != null)
            {
                arCard.transform.LookAt(Camera.main.transform);
                arCard.transform.rotation = Quaternion.Euler(0, arCard.transform.rotation.eulerAngles.y + 180, 0);
            }

            _activeOverlays.Add(arCard);
            Debug.Log($"[ARManager] 放置AR卡片: {info.Title} 在 {position}");

            return arCard;
        }

        public void ClearAllOverlays()
        {
            foreach (var overlay in _activeOverlays)
            {
                if (overlay != null)
                {
                    Destroy(overlay);
                }
            }
            _activeOverlays.Clear();

            Debug.Log("[ARManager] 所有AR叠加已清除");
        }

        public void SetARVisibility(bool visible)
        {
            foreach (var overlay in _activeOverlays)
            {
                if (overlay != null)
                {
                    overlay.SetActive(visible);
                }
            }

            Debug.Log($"[ARManager] AR可见性: {visible}");
        }

        public List<ARCapability> GetSupportedCapabilities()
        {
            var capabilities = new List<ARCapability>();

            // 检查支持的AR功能
            if (cameraManager != null)
            {
                capabilities.Add(ARCapability.ImageRecognition);
            }

            if (raycastManager != null)
            {
                capabilities.Add(ARCapability.PlaneDetection);
            }

            return capabilities;
        }

        /// <summary>
        /// 射线检测放置AR对象
        /// </summary>
        public bool RaycastPlacement(Vector2 screenPosition, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;

            if (raycastManager == null) return false;

            var hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                worldPosition = hits[0].pose.position;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 放置导航箭头
        /// </summary>
        public GameObject PlaceNavigationArrow(Vector3 targetPosition, string label)
        {
            if (navigationArrowPrefab == null) return null;

            var arrow = Instantiate(navigationArrowPrefab);
            arrow.name = $"NavArrow_{label}";

            // 设置箭头指向目标
            var arrowComponent = arrow.GetComponent<ARNavigationArrow>();
            if (arrowComponent != null)
            {
                arrowComponent.SetTarget(targetPosition, label);
            }

            _activeOverlays.Add(arrow);
            return arrow;
        }

        private void OnDestroy()
        {
            if (arSession != null)
            {
                arSession.stateChanged -= OnARSessionStateChanged;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
