using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TripMeta.AI
{
    /// <summary>
    /// 对话历史管理器 - 跟踪用户与AI导游的对话记录
    /// </summary>
    public class ConversationHistory
    {
        private readonly List<ConversationExchange> exchanges = new List<ConversationExchange>();
        private readonly int maxHistorySize;
        private LocationContext currentLocation;
        private string locationKnowledge;

        public ConversationHistory(int maxHistorySize = 50)
        {
            this.maxHistorySize = maxHistorySize;
        }

        /// <summary>
        /// 添加对话交换
        /// </summary>
        public void AddExchange(string userInput, string aiResponse)
        {
            var exchange = new ConversationExchange
            {
                timestamp = DateTime.UtcNow,
                userInput = userInput,
                aiResponse = aiResponse
            };

            exchanges.Add(exchange);

            // 保持历史记录大小限制
            while (exchanges.Count > maxHistorySize)
            {
                exchanges.RemoveAt(0);
            }
        }

        /// <summary>
        /// 添加位置上下文
        /// </summary>
        public void AddLocationContext(LocationContext location, string knowledge)
        {
            currentLocation = location;
            locationKnowledge = knowledge;
        }

        /// <summary>
        /// 获取相关上下文
        /// </summary>
        public string GetRelevantContext(UserIntent intent)
        {
            var context = new System.Text.StringBuilder();

            // 添加当前位置信息
            if (currentLocation != null)
            {
                context.AppendLine($"当前位置: {currentLocation.locationName}");
                if (!string.IsNullOrEmpty(locationKnowledge))
                {
                    context.AppendLine($"位置知识: {locationKnowledge}");
                }
            }

            // 添加最近的对话历史
            var recentExchanges = exchanges.Take(Math.Min(3, exchanges.Count));
            if (recentExchanges.Any())
            {
                context.AppendLine("\n最近对话:");
                foreach (var exchange in recentExchanges)
                {
                    context.AppendLine($"用户: {exchange.userInput}");
                    context.AppendLine($"导游: {exchange.aiResponse}");
                }
            }

            return context.ToString();
        }

        /// <summary>
        /// 获取对话历史摘要
        /// </summary>
        public string GetSummary()
        {
            if (exchanges.Count == 0)
                return "暂无对话记录";

            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"对话记录 ({exchanges.Count} 条):");

            foreach (var exchange in exchanges)
            {
                summary.AppendLine($"[{exchange.timestamp:HH:mm}] 用户: {exchange.userInput}");
                summary.AppendLine($"[{exchange.timestamp:HH:mm}] 导游: {exchange.aiResponse.Substring(0, Math.Min(50, exchange.aiResponse.Length))}...");
            }

            return summary.ToString();
        }

        /// <summary>
        /// 清除历史记录
        /// </summary>
        public void Clear()
        {
            exchanges.Clear();
            currentLocation = null;
            locationKnowledge = null;
        }

        /// <summary>
        /// 对话交换记录
        /// </summary>
        [Serializable]
        public class ConversationExchange
        {
            public DateTime timestamp;
            public string userInput;
            public string aiResponse;
            public string intent;
            public float confidence;
        }
    }
}
