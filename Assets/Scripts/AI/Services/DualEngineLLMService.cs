using System;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.AI
{
    /// <summary>
    /// 双引擎LLM服务 - 统一的LLM服务接口
    /// 内部使用AIEngineSelector智能选择GPT-4或Claude-3.5
    /// 实现IGPTService接口，可无缝替换现有的GPTService
    /// </summary>
    public class DualEngineLLMService : IGPTService
    {
        private readonly GPTConfig gptConfig;
        private readonly ClaudeConfig claudeConfig;
        private readonly DualEngineConfig dualConfig;

        private AIEngineSelector engineSelector;
        private bool isInitialized = false;

        public bool IsInitialized => isInitialized;

        public event Action<string, string> OnResponseReceived;
        public event Action<string> OnError;

        public DualEngineLLMService(GPTConfig gptConfig, ClaudeConfig claudeConfig, DualEngineConfig dualConfig = null)
        {
            this.gptConfig = gptConfig;
            this.claudeConfig = claudeConfig;
            this.dualConfig = dualConfig ?? new DualEngineConfig();
        }

        /// <summary>
        /// 初始化双引擎服务
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Logger.LogInfo("初始化双引擎LLM服务...", "DualEngineLLM");

                // 创建引擎选择器
                var selectorObject = new GameObject("AIEngineSelector");
                engineSelector = selectorObject.AddComponent<AIEngineSelector>();

                // 配置引擎
                engineSelector.gptConfig = gptConfig;
                engineSelector.claudeConfig = claudeConfig;
                engineSelector.selectionStrategy = dualConfig.defaultStrategy;
                engineSelector.enablePerformanceTracking = dualConfig.enablePerformanceTracking;
                engineSelector.enableABTesting = dualConfig.enableABTesting;
                engineSelector.abTestSplitRatio = dualConfig.abTestSplitRatio;

                // 等待初始化完成
                await Task.Delay(2000);

                isInitialized = engineSelector != null;

                Logger.LogInfo("双引擎LLM服务初始化完成", "DualEngineLLM");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "双引擎LLM服务初始化失败");
                throw;
            }
        }

        /// <summary>
        /// 发送聊天请求 - 自动选择最优引擎
        /// </summary>
        public async Task<string> SendChatAsync(string message, string conversationId = null)
        {
            if (!isInitialized)
                throw new InvalidOperationException("双引擎LLM服务未初始化");

            try
            {
                // 分析消息类型以选择最佳引擎
                var taskType = AnalyzeTaskType(message);

                // 使用引擎选择器发送请求
                var response = await engineSelector.SendRequestAsync(message, taskType);

                OnResponseReceived?.Invoke(message, response);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "双引擎LLM聊天请求失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 生成内容 - 使用适合内容生成的引擎
        /// </summary>
        public async Task<string> GenerateContentAsync(string prompt, GPTGenerationOptions options = null)
        {
            if (!isInitialized)
                throw new InvalidOperationException("双引擎LLM服务未初始化");

            try
            {
                // 内容生成任务通常使用Claude-3.5效果更好
                var taskType = AITaskType.CreativeWriting;

                if (options?.systemPrompt?.ToLower().Contains("代码") == true ||
                    options?.systemPrompt?.ToLower().Contains("code") == true)
                {
                    taskType = AITaskType.CodeGeneration;
                }
                else if (options?.systemPrompt?.ToLower().Contains("分析") == true ||
                         options?.systemPrompt?.ToLower().Contains("analyze") == true)
                {
                    taskType = AITaskType.Analysis;
                }

                var response = await engineSelector.SendRequestAsync(prompt, taskType);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "双引擎LLM内容生成失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 流式聊天 - 使用选定的引擎
        /// </summary>
        public async Task SendStreamChatAsync(string message, Action<string> onPartialResponse, string conversationId = null)
        {
            if (!isInitialized)
                throw new InvalidOperationException("双引擎LLM服务未初始化");

            try
            {
                // 流式响应使用非流式API模拟，因为需要特殊处理
                var response = await SendChatAsync(message, conversationId);

                // 模拟流式输出
                var words = response.Split(' ');
                var currentText = "";

                foreach (var word in words)
                {
                    currentText += word + " ";
                    onPartialResponse?.Invoke(currentText.Trim());
                    await Task.Delay(30);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "双引擎LLM流式聊天失败");
                OnError?.Invoke(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 获取对话历史 - 返回空（双引擎服务不直接管理对话）
        /// </summary>
        public GPTConversation GetConversation(string conversationId)
        {
            // 双引擎服务不直接管理对话历史
            // 对话管理由上层应用负责
            return null;
        }

        /// <summary>
        /// 清除对话历史 - 空实现
        /// </summary>
        public void ClearConversation(string conversationId = null)
        {
            // 双引擎服务不直接管理对话历史
        }

        /// <summary>
        /// 分析任务类型
        /// </summary>
        private AITaskType AnalyzeTaskType(string message)
        {
            var lowerMessage = message.ToLower();

            // 代码相关
            if (lowerMessage.Contains("代码") || lowerMessage.Contains("code") ||
                lowerMessage.Contains("编程") || lowerMessage.Contains("programming") ||
                lowerMessage.Contains("函数") || lowerMessage.Contains("function"))
            {
                return AITaskType.CodeGeneration;
            }

            // 翻译相关
            if (lowerMessage.Contains("翻译") || lowerMessage.Contains("translate") ||
                lowerMessage.Contains("用英文") || lowerMessage.Contains("用中文"))
            {
                return AITaskType.Translation;
            }

            // 分析相关
            if (lowerMessage.Contains("分析") || lowerMessage.Contains("analyze") ||
                lowerMessage.Contains("比较") || lowerMessage.Contains("compare") ||
                lowerMessage.Contains("评估") || lowerMessage.Contains("evaluate"))
            {
                return AITaskType.Analysis;
            }

            // 推理相关
            if (lowerMessage.Contains("为什么") || lowerMessage.Contains("为什么") ||
                lowerMessage.Contains("如何") || lowerMessage.Contains("how") ||
                lowerMessage.Contains("解释") || lowerMessage.Contains("explain") ||
                lowerMessage.Contains("推理") || lowerMessage.Contains("reason"))
            {
                return AITaskType.Reasoning;
            }

            // 总结相关
            if (lowerMessage.Contains("总结") || lowerMessage.Contains("summarize") ||
                lowerMessage.Contains("概括") || lowerMessage.Contains("summary"))
            {
                return AITaskType.Summarization;
            }

            // 意图识别（短句通常是意图）
            if (message.Length < 20)
            {
                return AITaskType.IntentRecognition;
            }

            // 默认对话类型
            return AITaskType.Conversation;
        }

        /// <summary>
        /// 检查健康状态
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            if (!isInitialized || engineSelector == null)
                return false;

            try
            {
                var reports = engineSelector.GetAllPerformanceReports();
                return reports.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 重新初始化
        /// </summary>
        public async Task ReinitializeAsync()
        {
            isInitialized = false;
            await InitializeAsync();
        }

        /// <summary>
        /// 暂停服务
        /// </summary>
        public void Pause()
        {
            Logger.LogInfo("双引擎LLM服务已暂停", "DualEngineLLM");
        }

        /// <summary>
        /// 恢复服务
        /// </summary>
        public void Resume()
        {
            Logger.LogInfo("双引擎LLM服务已恢复", "DualEngineLLM");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public async Task DisposeAsync()
        {
            try
            {
                if (engineSelector != null)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(engineSelector.gameObject);
                    }
                    engineSelector = null;
                }

                isInitialized = false;
                Logger.LogInfo("双引擎LLM服务资源已释放", "DualEngineLLM");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "双引擎LLM服务资源释放失败");
            }
        }

        /// <summary>
        /// 获取性能报告
        /// </summary>
        public string GetPerformanceReport()
        {
            if (engineSelector == null)
                return "引擎选择器未初始化";

            var reports = engineSelector.GetAllPerformanceReports();
            var reportText = "=== 双引擎LLM性能报告 ===\n\n";

            foreach (var report in reports.Values)
            {
                reportText += report.ToString() + "\n";
            }

            return reportText;
        }

        /// <summary>
        /// 设置选择策略
        /// </summary>
        public void SetSelectionStrategy(AIEngineSelectionStrategy strategy)
        {
            if (engineSelector != null)
            {
                engineSelector.SetSelectionStrategy(strategy);
            }
        }

        /// <summary>
        /// 使用特定引擎发送请求（覆盖自动选择）
        /// </summary>
        public async Task<string> SendRequestWithEngine(string message, AIEngineType engineType)
        {
            if (!isInitialized)
                throw new InvalidOperationException("双引擎LLM服务未初始化");

            var taskType = AnalyzeTaskType(message);
            return await engineSelector.SendRequestAsync(message, taskType, engineType);
        }
    }

    /// <summary>
    /// 双引擎配置
    /// </summary>
    [Serializable]
    public class DualEngineConfig
    {
        [Header("选择策略")]
        public AIEngineSelectionStrategy defaultStrategy = AIEngineSelectionStrategy.Intelligent;

        [Header("性能监控")]
        public bool enablePerformanceTracking = true;

        [Header("A/B测试")]
        public bool enableABTesting = false;
        [Range(0f, 1f)]
        public float abTestSplitRatio = 0.5f;

        [Header("回退配置")]
        public bool enableFallback = true;
        public int maxRetries = 2;
        public float retryDelay = 1f;
    }
}
