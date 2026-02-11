using UnityEngine;
using System.Collections.Generic;
using TripMeta.AI;

namespace TripMeta.Features
{
    /// <summary>
    /// 旅游位置数据 - 定义一个旅游地点
    /// </summary>
    [CreateAssetMenu(fileName = "TourLocation", menuName = "TripMeta/Tour Location")]
    public class TourLocation : ScriptableObject
    {
        [Header("Basic Info")]
        public string locationId;
        public string locationName;
        [TextArea(3, 6)]
        public string description;
        public Sprite icon;

        [Header("Categories")]
        public LocationType locationType;
        public string[] tags;

        [Header("Position")]
        public Vector3 spawnPosition;
        public Quaternion spawnRotation = Quaternion.identity;

        [Header("Points of Interest")]
        public List<PointOfInterestData> pointsOfInterest = new List<PointOfInterestData>();

        [Header("Tour Guide Settings")]
        [TextArea(3, 6)]
        public string introductionMessage;
        public TourGuidePersonality recommendedGuidePersonality;

        /// <summary>
        /// 创建位置上下文
        /// </summary>
        public LocationContext CreateContext()
        {
            return new LocationContext
            {
                locationId = locationId,
                locationName = locationName,
                description = description,
                position = spawnPosition,
                tags = tags,
                locationType = locationType
            };
        }
    }

    /// <summary>
    /// 兴趣点数据
    /// </summary>
    [System.Serializable]
    public class PointOfInterestData
    {
        public string poiId;
        public string name;
        [TextArea(2, 4)]
        public string description;
        public Vector3 position;
        public string category;

        [Header("Tour Info")]
        [TextArea(2, 4)]
        public string tourGuideInfo;
        public AudioClip audioNarration;
        public Sprite image;
    }

    /// <summary>
    /// 纽约位置数据
    /// </summary>
    [CreateAssetMenu(fileName = "NewYorkLocation", menuName = "TripMeta/Preset Locations/New York")]
    public class NewYorkLocation : TourLocation
    {
        void OnEnable()
        {
            if (string.IsNullOrEmpty(locationId))
            {
                locationId = "newyork";
                locationName = "纽约";
                description = "纽约是美国最大的城市，拥有丰富的历史和文化。这里是全球金融、媒体和文化的中心。";
                locationType = LocationType.City;
                tags = new string[] { "城市", "历史", "文化", "金融", "旅游" };
                spawnPosition = new Vector3(0, 0, 0);

                introductionMessage = "欢迎来到纽约！我是您的AI导游小美。纽约是美国最大的城市，这里有著名的自由女神像、帝国大厦、时代广场等景点。让我们一起探索这个精彩的城市吧！";

                // 创建主要兴趣点
                pointsOfInterest = new List<PointOfInterestData>
                {
                    new PointOfInterestData
                    {
                        poiId = "statue_of_liberty",
                        name = "自由女神像",
                        description = "美国的象征，位于自由岛上",
                        position = new Vector3(100, 0, 50),
                        category = "Landmark",
                        tourGuideInfo = "自由女神像是法国人民送给美国的礼物，于1886年落成。她手持火炬和独立宣言，象征着自由和民主。雕像高93米，是纽约港的标志性景观。"
                    },
                    new PointOfInterestData
                    {
                        poiId = "empire_state_building",
                        name = "帝国大厦",
                        description = "纽约的地标性摩天大楼",
                        position = new Vector3(0, 0, 100),
                        category = "Architecture",
                        tourGuideInfo = "帝国大厦建于1931年，曾是世界最高建筑。它高381米，共有102层。这座装饰艺术风格的建筑是纽约最著名的地标之一，每年吸引数百万游客。"
                    },
                    new PointOfInterestData
                    {
                        poiId = "times_square",
                        name = "时代广场",
                        description = "世界的十字路口",
                        position = new Vector3(-100, 0, 50),
                        category = "Entertainment",
                        tourGuideInfo = "时代广场被称为'世界的十字路口'，这里是纽约市最繁忙的步行区之一。这里汇集了大量的剧院、餐厅、商店和广告牌，是纽约夜生活的中心。每年除夕的落球仪式也在这里举行。"
                    },
                    new PointOfInterestData
                    {
                        poiId = "central_park",
                        name = "中央公园",
                        description = "曼哈顿的绿肺",
                        position = new Vector3(0, 0, -100),
                        category = "Nature",
                        tourGuideInfo = "中央公园是曼哈顿最大的城市公园，占地843英亩。这里有湖泊、草坪、树林、动物园和许多雕塑。它是纽约市民休闲娱乐的好去处，也是电影的经典取景地。"
                    }
                };
            }
        }
    }

    /// <summary>
    /// 旅游位置管理器
    /// </summary>
    public class TourLocationManager : MonoBehaviour
    {
        [Header("Available Locations")]
        [SerializeField] private List<TourLocation> availableLocations = new List<TourLocation>();

        [Header("Current Tour")]
        [SerializeField] private TourLocation currentLocation;
        [SerializeField] private int currentPOIIndex = 0;

        private Dictionary<string, TourLocation> locationMap = new Dictionary<string, TourLocation>();

        public static TourLocationManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLocations();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeLocations()
        {
            foreach (var location in availableLocations)
            {
                if (location != null && !string.IsNullOrEmpty(location.locationId))
                {
                    locationMap[location.locationId] = location;
                }
            }

            Debug.Log($"[TourLocationManager] Initialized with {locationMap.Count} locations");
        }

        /// <summary>
        /// 设置当前位置
        /// </summary>
        public void SetCurrentLocation(string locationId)
        {
            if (locationMap.TryGetValue(locationId, out var location))
            {
                currentLocation = location;
                currentPOIIndex = 0;

                // 通知AI导游
                var tourGuide = FindObjectOfType<AITourGuide>();
                if (tourGuide != null)
                {
                    tourGuide.SetCurrentLocation(location.CreateContext());
                }

                // 通知UI
                var tourUI = FindObjectOfType<TourUIManager>();
                if (tourUI != null)
                {
                    tourUI.UpdateLocationInfo(location.CreateContext());
                }

                Debug.Log($"[TourLocationManager] Current location set to: {location.locationName}");
            }
            else
            {
                Debug.LogWarning($"[TourLocationManager] Location not found: {locationId}");
            }
        }

        /// <summary>
        /// 获取当前位置
        /// </summary>
        public TourLocation GetCurrentLocation()
        {
            return currentLocation;
        }

        /// <summary>
        /// 获取兴趣点
        /// </summary>
        public PointOfInterestData GetCurrentPOI()
        {
            if (currentLocation != null &&
                currentPOIIndex >= 0 &&
                currentPOIIndex < currentLocation.pointsOfInterest.Count)
            {
                return currentLocation.pointsOfInterest[currentPOIIndex];
            }
            return null;
        }

        /// <summary>
        /// 移动到下一个兴趣点
        /// </summary>
        public PointOfInterestData NextPOI()
        {
            if (currentLocation != null)
            {
                currentPOIIndex = (currentPOIIndex + 1) % currentLocation.pointsOfInterest.Count;
                return GetCurrentPOI();
            }
            return null;
        }

        /// <summary>
        /// 获取所有位置ID
        /// </summary>
        public string[] GetAllLocationIds()
        {
            var ids = new string[locationMap.Count];
            locationMap.Keys.CopyTo(ids, 0);
            return ids;
        }

        /// <summary>
        /// 通过ID获取位置
        /// </summary>
        public TourLocation GetLocationById(string locationId)
        {
            locationMap.TryGetValue(locationId, out var location);
            return location;
        }
    }
}
