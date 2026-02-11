using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.AI
{
    /// <summary>
    /// 知识图谱 - 存储和检索旅游相关的知识信息
    /// </summary>
    public class KnowledgeGraph
    {
        private readonly Dictionary<string, LocationKnowledge> knowledgeBase = new Dictionary<string, LocationKnowledge>();
        private readonly KnowledgeGraphConfig config;
        private readonly Dictionary<string, DateTime> cacheTimestamps = new Dictionary<string, DateTime>();

        public KnowledgeGraph(KnowledgeGraphConfig config)
        {
            this.config = config ?? new KnowledgeGraphConfig();
            InitializeDefaultKnowledge();
        }

        /// <summary>
        /// 初始化默认知识
        /// </summary>
        private void InitializeDefaultKnowledge()
        {
            // 添加一些默认的位置知识
            knowledgeBase["newyork"] = new LocationKnowledge
            {
                locationId = "newyork",
                name = "纽约",
                description = "纽约是美国最大的城市，位于纽约州南部，是美国经济、金融、媒体、政治、教育、娱乐和时尚中心。",
                categories = new[] { "城市", "历史", "文化", "建筑" },
                facts = new List<string>
                {
                    "纽约市由5个行政区组成：曼哈顿、布鲁克林、皇后区、布朗克斯和斯塔滕岛。",
                    "纽约拥有超过800万人口，是美国人口最多的城市。",
                    "时代广场被称为'世界的十字路口'，每年吸引约5000万游客。",
                    "中央公园是曼哈顿最大的公园，占地843英亩。"
                },
                pointsOfInterest = new List<PointOfInterest>
                {
                    new PointOfInterest { name = "自由女神像", description = "美国的象征，位于自由岛上。" },
                    new PointOfInterest { name = "帝国大厦", description = "纽约的地标性摩天大楼，曾是世界最高建筑。" },
                    new PointOfInterest { name = "时代广场", description = "世界的娱乐和商业中心。" },
                    new PointOfInterest { name = "中央公园", description = "曼哈顿的绿肺，占地广阔的城市公园。" }
                }
            };

            knowledgeBase["downtown"] = new LocationKnowledge
            {
                locationId = "downtown",
                name = "市中心商业区",
                description = "繁华的市中心，商业和金融活动的集中地。",
                categories = new[] { "商业", "金融", "购物" },
                facts = new List<string>
                {
                    "市中心是城市经济发展的核心区域。",
                    "这里汇集了众多知名企业和金融机构。"
                },
                pointsOfInterest = new List<PointOfInterest>()
            };
        }

        /// <summary>
        /// 异步加载知识图谱
        /// </summary>
        public async Task LoadAsync()
        {
            // 模拟异步加载
            await Task.Delay(100);
            Debug.Log("[KnowledgeGraph] 知识图谱加载完成");
        }

        /// <summary>
        /// 获取位置知识
        /// </summary>
        public async Task<LocationKnowledge> GetLocationKnowledgeAsync(string locationId)
        {
            // 检查缓存
            if (config.enableLocalCache && knowledgeBase.TryGetValue(locationId, out var knowledge))
            {
                if (cacheTimestamps.TryGetValue(locationId, out var timestamp))
                {
                    var age = DateTime.UtcNow - timestamp;
                    if (age.TotalHours < config.cacheExpirationHours)
                    {
                        return knowledge;
                    }
                }
            }

            // 如果没有缓存或缓存过期，返回默认知识
            if (knowledgeBase.ContainsKey(locationId))
            {
                return knowledgeBase[locationId];
            }

            // 返回未知位置的基本信息
            return new LocationKnowledge
            {
                locationId = locationId,
                name = "未知位置",
                description = "暂无此位置的详细信息。",
                categories = new string[0],
                facts = new List<string>(),
                pointsOfInterest = new List<PointOfInterest>()
            };
        }

        /// <summary>
        /// 查询知识
        /// </summary>
        public async Task<string> QueryKnowledgeAsync(string locationId, string intent, string[] entities)
        {
            var knowledge = await GetLocationKnowledgeAsync(locationId);
            var result = new System.Text.StringBuilder();

            result.AppendLine(knowledge.description);
            result.AppendLine();

            // 根据意图返回相关知识
            switch (intent.ToLower())
            {
                case "history":
                case "历史":
                    result.AppendLine("历史信息:");
                    foreach (var fact in knowledge.facts)
                    {
                        result.AppendLine($"• {fact}");
                    }
                    break;

                case "recommendation":
                case "推荐":
                    result.AppendLine("推荐景点:");
                    foreach (var poi in knowledge.pointsOfInterest)
                    {
                        result.AppendLine($"• {poi.name}: {poi.description}");
                    }
                    break;

                default:
                    result.AppendLine(knowledge.description);
                    if (knowledge.facts.Count > 0)
                    {
                        result.AppendLine($"\n有趣的事实: {knowledge.facts[0]}");
                    }
                    break;
            }

            return result.ToString();
        }

        /// <summary>
        /// 添加位置知识
        /// </summary>
        public void AddLocationKnowledge(LocationKnowledge knowledge)
        {
            knowledgeBase[knowledge.locationId] = knowledge;
            cacheTimestamps[knowledge.locationId] = DateTime.UtcNow;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            cacheTimestamps.Clear();
        }
    }

    /// <summary>
    /// 位置知识
    /// </summary>
    [Serializable]
    public class LocationKnowledge
    {
        public string locationId;
        public string name;
        public string description;
        public string[] categories;
        public List<string> facts = new List<string>();
        public List<PointOfInterest> pointsOfInterest = new List<PointOfInterest>();
    }

    /// <summary>
    /// 兴趣点
    /// </summary>
    [Serializable]
    public class PointOfInterest
    {
        public string name;
        public string description;
        public string category;
        public Vector3 position;
    }
}
