using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Analytics
{
    /// <summary>
    /// 分析管理器
    /// 用户行为分析、A/B测试、商业智能仪表板
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        [Header("分析配置")]
        public string analyticsEndpoint = "https://analytics.tripmeta.com/api/v1";
        public string apiKey = "";
        public float eventBatchInterval = 30f;
        public int maxBatchSize = 100;
        public bool enableRealTimeAnalytics = true;

        [Header("功能开关")]
        public bool trackUserSessions = true;
        public bool trackVRInteractions = true;
        public bool trackPerformanceMetrics = true;
        public bool enableABTesting = true;
        public bool trackConversionFunnel = true;
        public bool trackRetentionMetrics = true;

        // 事件队列
        private Queue<AnalyticsEvent> eventQueue = new Queue<AnalyticsEvent>();
        private float lastBatchSendTime;
        private string currentSessionId;
        private DateTime sessionStartTime;

        // A/B测试
        private Dictionary<string, ABTestVariant> activeExperiments = new Dictionary<string, ABTestVariant>();

        // 用户属性
        private UserProperties userProperties = new UserProperties();

        // 实时仪表板数据
        private RealTimeDashboardData dashboardData = new RealTimeDashboardData();

        public static AnalyticsManager Instance { get; private set; }

        public RealTimeDashboardData DashboardData => dashboardData;
        public string CurrentSessionId => currentSessionId;

        // 事件
        public event Action<AnalyticsEvent> OnEventTracked;
        public event Action<RealTimeDashboardData> OnDashboardUpdated;
        public event Action<string, ABTestVariant> OnABTestAssigned;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (Time.time - lastBatchSendTime > eventBatchInterval && eventQueue.Count > 0)
            {
                FlushEvents();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            currentSessionId = Guid.NewGuid().ToString();
            sessionStartTime = DateTime.Now;

            InitializeUserProperties();
            LoadActiveExperiments();

            // 跟踪会话开始
            if (trackUserSessions)
            {
                TrackEvent("session_start", new Dictionary<string, object>
                {
                    { "session_id", currentSessionId },
                    { "platform", Application.platform.ToString() },
                    { "version", Application.version }
                });
            }

            Debug.Log("[AnalyticsManager] 分析管理器初始化完成");
        }

        /// <summary>
        /// 初始化用户属性
        /// </summary>
        private void InitializeUserProperties()
        {
            userProperties = new UserProperties
            {
                userId = SystemInfo.deviceUniqueIdentifier,
                firstSeen = DateTime.Now,
                deviceType = SystemInfo.deviceType.ToString(),
                osVersion = SystemInfo.operatingSystem,
                deviceModel = SystemInfo.deviceModel,
                screenResolution = Screen.currentResolution.ToString(),
                vrHeadset = GetVRHeadsetType()
            };
        }

        /// <summary>
        /// 获取VR头显类型
        /// </summary>
        private string GetVRHeadsetType()
        {
            // 这里应该检测实际连接的VR设备
            return "Unknown";
        }

        /// <summary>
        /// 加载活跃的A/B测试
        /// </summary>
        private void LoadActiveExperiments()
        {
            // 从服务器加载活跃的实验
            // 简化实现：创建默认实验
            if (enableABTesting)
            {
                AssignExperiment("ui_layout_v2", new[] { "control", "variant_a", "variant_b" });
                AssignExperiment("onboarding_flow", new[] { "control", "simplified" });
            }
        }

        #region 事件跟踪

        /// <summary>
        /// 跟踪事件
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            var evt = new AnalyticsEvent
            {
                eventId = Guid.NewGuid().ToString(),
                eventName = eventName,
                timestamp = DateTime.Now,
                sessionId = currentSessionId,
                userId = userProperties.userId,
                parameters = parameters ?? new Dictionary<string, object>()
            };

            eventQueue.Enqueue(evt);
            UpdateDashboardData(evt);
            OnEventTracked?.Invoke(evt);

            // 如果队列过大，立即发送
            if (eventQueue.Count >= maxBatchSize)
            {
                FlushEvents();
            }
        }

        /// <summary>
        /// 跟踪页面浏览
        /// </summary>
        public void TrackPageView(string pageName, Dictionary<string, object> properties = null)
        {
            var parameters = properties ?? new Dictionary<string, object>();
            parameters["page_name"] = pageName;
            TrackEvent("page_view", parameters);
        }

        /// <summary>
        /// 跟踪VR交互
        /// </summary>
        public void TrackVRInteraction(string interactionType, string objectName, float duration, Dictionary<string, object> properties = null)
        {
            if (!trackVRInteractions) return;

            var parameters = properties ?? new Dictionary<string, object>();
            parameters["interaction_type"] = interactionType;
            parameters["object_name"] = objectName;
            parameters["duration"] = duration;
            TrackEvent("vr_interaction", parameters);
        }

        /// <summary>
        /// 跟踪转化事件
        /// </summary>
        public void TrackConversion(string funnelStage, decimal value = 0, Dictionary<string, object> properties = null)
        {
            if (!trackConversionFunnel) return;

            var parameters = properties ?? new Dictionary<string, object>();
            parameters["funnel_stage"] = funnelStage;
            parameters["value"] = value;
            TrackEvent("conversion", parameters);
        }

        /// <summary>
        /// 跟踪错误
        /// </summary>
        public void TrackError(string errorType, string errorMessage, Dictionary<string, object> properties = null)
        {
            var parameters = properties ?? new Dictionary<string, object>();
            parameters["error_type"] = errorType;
            parameters["error_message"] = errorMessage;
            TrackEvent("error", parameters);
        }

        /// <summary>
        /// 刷新事件队列
        /// </summary>
        private async void FlushEvents()
        {
            if (eventQueue.Count == 0) return;

            var eventsToSend = new List<AnalyticsEvent>();
            while (eventQueue.Count > 0 && eventsToSend.Count < maxBatchSize)
            {
                eventsToSend.Add(eventQueue.Dequeue());
            }

            await SendEventsToServer(eventsToSend);
            lastBatchSendTime = Time.time;
        }

        /// <summary>
        /// 发送事件到服务器
        /// </summary>
        private async Task SendEventsToServer(List<AnalyticsEvent> events)
        {
            try
            {
                // 这里应该调用API发送事件
                await Task.Delay(100);
                Debug.Log($"[AnalyticsManager] 发送了 {events.Count} 个事件");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AnalyticsManager] 发送事件失败: {e.Message}");
                // 重新入队
                foreach (var evt in events)
                {
                    eventQueue.Enqueue(evt);
                }
            }
        }

        #endregion

        #region A/B测试

        /// <summary>
        /// 分配实验
        /// </summary>
        private void AssignExperiment(string experimentId, string[] variants)
        {
            // 基于用户ID哈希分配
            int hash = userProperties.userId.GetHashCode();
            int variantIndex = Math.Abs(hash) % variants.Length;

            var variant = new ABTestVariant
            {
                experimentId = experimentId,
                variantName = variants[variantIndex],
                assignedAt = DateTime.Now
            };

            activeExperiments[experimentId] = variant;
            OnABTestAssigned?.Invoke(experimentId, variant);

            // 跟踪分配
            TrackEvent("ab_test_assigned", new Dictionary<string, object>
            {
                { "experiment_id", experimentId },
                { "variant", variant.variantName }
            });
        }

        /// <summary>
        /// 获取实验变体
        /// </summary>
        public string GetExperimentVariant(string experimentId, string defaultVariant = "control")
        {
            if (activeExperiments.TryGetValue(experimentId, out var variant))
            {
                return variant.variantName;
            }
            return defaultVariant;
        }

        /// <summary>
        /// 跟踪实验转化
        /// </summary>
        public void TrackExperimentConversion(string experimentId, string conversionType, Dictionary<string, object> properties = null)
        {
            if (!activeExperiments.TryGetValue(experimentId, out var variant)) return;

            var parameters = properties ?? new Dictionary<string, object>();
            parameters["experiment_id"] = experimentId;
            parameters["variant"] = variant.variantName;
            parameters["conversion_type"] = conversionType;
            TrackEvent("ab_test_conversion", parameters);
        }

        #endregion

        #region 仪表板数据

        /// <summary>
        /// 更新仪表板数据
        /// </summary>
        private void UpdateDashboardData(AnalyticsEvent evt)
        {
            if (!enableRealTimeAnalytics) return;

            dashboardData.totalEvents++;
            dashboardData.lastUpdated = DateTime.Now;

            switch (evt.eventName)
            {
                case "session_start":
                    dashboardData.activeUsers++;
                    break;
                case "session_end":
                    dashboardData.activeUsers--;
                    break;
                case "conversion":
                    dashboardData.conversions++;
                    if (evt.parameters.TryGetValue("value", out var value))
                    {
                        dashboardData.revenue += Convert.ToDecimal(value);
                    }
                    break;
                case "vr_interaction":
                    dashboardData.vrInteractions++;
                    break;
                case "error":
                    dashboardData.errors++;
                    break;
            }

            // 更新每分钟事件数
            UpdateEventsPerMinute();

            OnDashboardUpdated?.Invoke(dashboardData);
        }

        /// <summary>
        /// 更新每分钟事件数
        /// </summary>
        private void UpdateEventsPerMinute()
        {
            // 简化实现
            dashboardData.eventsPerMinute = UnityEngine.Random.Range(50, 200);
        }

        /// <summary>
        /// 获取仪表板数据
        /// </summary>
        public RealTimeDashboardData GetDashboardData()
        {
            return dashboardData;
        }

        #endregion

        #region 留存分析

        /// <summary>
        /// 跟踪留存事件
        /// </summary>
        public void TrackRetentionEvent(RetentionEventType eventType)
        {
            if (!trackRetentionMetrics) return;

            TrackEvent("retention", new Dictionary<string, object>
            {
                { "event_type", eventType.ToString() },
                { "days_since_first_use", (DateTime.Now - userProperties.firstSeen).TotalDays }
            });
        }

        #endregion

        /// <summary>
        /// 获取会话时长
        /// </summary>
        public TimeSpan GetSessionDuration()
        {
            return DateTime.Now - sessionStartTime;
        }

        void OnApplicationQuit()
        {
            // 会话结束
            if (trackUserSessions)
            {
                TrackEvent("session_end", new Dictionary<string, object>
                {
                    { "session_id", currentSessionId },
                    { "duration_seconds", GetSessionDuration().TotalSeconds }
                });
            }

            FlushEvents();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 分析事件
    /// </summary>
    public class AnalyticsEvent
    {
        public string eventId;
        public string eventName;
        public DateTime timestamp;
        public string sessionId;
        public string userId;
        public Dictionary<string, object> parameters;
    }

    /// <summary>
    /// 用户属性
    /// </summary>
    public class UserProperties
    {
        public string userId;
        public DateTime firstSeen;
        public string deviceType;
        public string osVersion;
        public string deviceModel;
        public string screenResolution;
        public string vrHeadset;
    }

    /// <summary>
    /// A/B测试变体
    /// </summary>
    public class ABTestVariant
    {
        public string experimentId;
        public string variantName;
        public DateTime assignedAt;
    }

    /// <summary>
    /// 实时仪表板数据
    /// </summary>
    public class RealTimeDashboardData
    {
        public int activeUsers;
        public int totalEvents;
        public int vrInteractions;
        public int conversions;
        public decimal revenue;
        public int errors;
        public float eventsPerMinute;
        public DateTime lastUpdated;
    }

    /// <summary>
    /// 留存事件类型
    /// </summary>
    public enum RetentionEventType
    {
        Day1,
        Day7,
        Day30,
        Day90
    }

    #endregion
}
