using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.AI
{
    /// <summary>
    /// AI引擎类型
    /// </summary>
    public enum AIEngineType
    {
        GPT4,
        Claude35,
        Auto
    }

    /// <summary>
    /// AI任务类型 - 用于智能选择引擎
    /// </summary>
    public enum AITaskType
    {
        Conversation,       // 一般对话
        CodeGeneration,     // 代码生成
        Analysis,           // 分析任务
        CreativeWriting,    // 创意写作
        Translation,        // 翻译任务
        Reasoning,          // 复杂推理
        Summarization,      // 内容总结
        QuestionAnswering,  // 问答任务
        IntentRecognition   // 意图识别
    }

    /// <summary>
    /// AI引擎选择器 - 智能选择最优AI引擎
    /// 根据任务类型、引擎可用性、性能指标自动选择GPT-4或Claude-3.5
    /// </summary>
    public class AIEngineSelector : MonoBehaviour
    {
        [Header("引擎配置")]
        public GPTConfig gptConfig;
        public ClaudeConfig claudeConfig;
        public AIEngineSelectionStrategy selectionStrategy = AIEngineSelectionStrategy.Intelligent;

        [Header("性能监控")]
        public bool enablePerformanceTracking = true;
        public int performanceWindowSize = 100;

        [Header("A/B测试")]
        public bool enableABTesting = false;
        public float abTestSplitRatio = 0.5f;

        // 引擎实例
        private GPTService gptService;
        private ClaudeService claudeService;

        // 性能追踪
        private Dictionary<AIEngineType, EnginePerformanceMetrics> performanceMetrics;
        private Dictionary<AITaskType, Dictionary<AIEngineType, float>> taskSuccessRates;

        // 状态
        private bool isInitialized = false;
        private System.Random random = new System.Random();

        // 事件
        public event Action<AIEngineType, AIEngineType> OnEngineSwitched;
        public event Action<AITaskType, AIEngineType, float> OnEngineSelected;

        public static AIEngineSelector Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSelector();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化选择器
        /// </summary>
        private void InitializeSelector()
        {
            performanceMetrics = new Dictionary<AIEngineType, EnginePerformanceMetrics>
            {
                { AIEngineType.GPT4, new EnginePerformanceMetrics(AIEngineType.GPT4) },
                { AIEngineType.Claude35, new EnginePerformanceMetrics(AIEngineType.Claude35) }
            };

            taskSuccessRates = new Dictionary<AITaskType, Dictionary<AIEngineType, float>>();
            foreach (AITaskType taskType in Enum.GetValues(typeof(AITaskType)))
            {
                taskSuccessRates[taskType] = new Dictionary<AIEngineType, float>
                {
                    { AIEngineType.GPT4, 0.9f },
                    { AIEngineType.Claude35, 0.9f }
                };
            }

            Logger.LogInfo("AI引擎选择器初始化完成", "AIEngineSelector");
        }

        async void Start()
        {
            await InitializeEngines();
        }

        /// <summary>
        /// 初始化AI引擎
        /// </summary>
        private async Task InitializeEngines()
        {
            try
            {
                Logger.LogInfo("开始初始化AI引擎...", "AIEngineSelector");

                // 初始化GPT-4
                if (gptConfig != null && !string.IsNullOrEmpty(gptConfig.apiKey))
                {
                    gptService = new GPTService(gptConfig);
                    await gptService.InitializeAsync();
                    Logger.LogInfo("GPT-4引擎初始化成功", "AIEngineSelector");
                }

                // 初始化Claude-3.5
                if (claudeConfig != null && !string.IsNullOrEmpty(claudeConfig.apiKey))
                {
                    claudeService = new ClaudeService(claudeConfig);
                    await claudeService.InitializeAsync();
                    Logger.LogInfo("Claude-3.5引擎初始化成功", "AIEngineSelector");
                }

                isInitialized = true;

                // 启动健康检查
                _ = StartHealthMonitoring();

                Logger.LogInfo("所有AI引擎初始化完成", "AIEngineSelector");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "AI引擎初始化失败");
            }
        }

        /// <summary>
        /// 选择最优引擎并发送请求
        /// </summary>
        public async Task<string> SendRequestAsync(string message, AITaskType taskType = AITaskType.Conversation, AIEngineType preferredEngine = AIEngineType.Auto)
        {
            if (!isInitialized)
                throw new InvalidOperationException("AI引擎选择器尚未初始化");

            var engine = await SelectOptimalEngine(taskType, preferredEngine);
            var startTime = DateTime.Now;

            try
            {
                string response;

                if (engine == AIEngineType.GPT4 && gptService != null && gptService.IsInitialized)
                {
                    response = await gptService.SendChatAsync(message);
                }
                else if (engine == AIEngineType.Claude35 && claudeService != null && claudeService.IsInitialized)
                {
                    response = await claudeService.SendChatAsync(message);
                }
                else
                {
                    // 回退到可用引擎
                    if (gptService != null && gptService.IsInitialized)
                    {
                        engine = AIEngineType.GPT4;
                        response = await gptService.SendChatAsync(message);
                    }
                    else if (claudeService != null && claudeService.IsInitialized)
                    {
                        engine = AIEngineType.Claude35;
                        response = await claudeService.SendChatAsync(message);
                    }
                    else
                    {
                        throw new InvalidOperationException("没有可用的AI引擎");
                    }
                }

                // 记录性能指标
                if (enablePerformanceTracking)
                {
                    var latency = (float)(DateTime.Now - startTime).TotalMilliseconds;
                    RecordPerformanceMetrics(engine, latency, true);
                }

                OnEngineSelected?.Invoke(taskType, engine, 1.0f);

                return response;
            }
            catch (Exception ex)
            {
                // 记录失败
                if (enablePerformanceTracking)
                {
                    RecordPerformanceMetrics(engine, 0, false);
                }

                Logger.LogException(ex, $"AI请求失败 (引擎: {engine})");
                throw;
            }
        }

        /// <summary>
        /// 选择最优引擎
        /// </summary>
        public async Task<AIEngineType> SelectOptimalEngine(AITaskType taskType = AITaskType.Conversation, AIEngineType preferredEngine = AIEngineType.Auto)
        {
            // 如果指定了具体引擎
            if (preferredEngine != AIEngineType.Auto)
            {
                if (IsEngineAvailable(preferredEngine))
                    return preferredEngine;

                // 如果指定引擎不可用，回退到自动选择
                Logger.LogWarning($"首选引擎 {preferredEngine} 不可用，使用自动选择", "AIEngineSelector");
            }

            // 根据策略选择引擎
            return selectionStrategy switch
            {
                AIEngineSelectionStrategy.GPT4Only => GetAvailableEngine(AIEngineType.GPT4),
                AIEngineSelectionStrategy.ClaudeOnly => GetAvailableEngine(AIEngineType.Claude35),
                AIEngineSelectionStrategy.RoundRobin => SelectRoundRobin(),
                AIEngineSelectionStrategy.ABTesting => SelectABTest(),
                AIEngineSelectionStrategy.TaskBased => SelectBasedOnTask(taskType),
                AIEngineSelectionStrategy.Performance => SelectBasedOnPerformance(),
                AIEngineSelectionStrategy.Intelligent => await SelectIntelligent(taskType),
                _ => SelectBasedOnTask(taskType)
            };
        }

        /// <summary>
        /// 智能选择引擎
        /// 综合考虑任务类型、历史性能、引擎可用性
        /// </summary>
        private async Task<AIEngineType> SelectIntelligent(AITaskType taskType)
        {
            var gptScore = CalculateEngineScore(AIEngineType.GPT4, taskType);
            var claudeScore = CalculateEngineScore(AIEngineType.Claude35, taskType);

            // 根据任务类型调整分数
            switch (taskType)
            {
                case AITaskType.CodeGeneration:
                    // GPT-4在代码生成方面通常更强
                    gptScore *= 1.1f;
                    break;

                case AITaskType.Reasoning:
                    // Claude-3.5在复杂推理方面表现优秀
                    claudeScore *= 1.1f;
                    break;

                case AITaskType.CreativeWriting:
                    // Claude-3.5在创意写作方面通常更好
                    claudeScore *= 1.15f;
                    break;

                case AITaskType.Analysis:
                    // GPT-4在分析任务上更稳定
                    gptScore *= 1.05f;
                    break;

                case AITaskType.Translation:
                    // GPT-4多语言支持更好
                    gptScore *= 1.1f;
                    break;
            }

            // 检查引擎健康状态
            var gptHealth = await CheckEngineHealth(AIEngineType.GPT4);
            var claudeHealth = await CheckEngineHealth(AIEngineType.Claude35);

            if (!gptHealth && !claudeHealth)
            {
                throw new InvalidOperationException("所有AI引擎都不可用");
            }

            if (!gptHealth)
            {
                Logger.LogWarning("GPT-4引擎不健康，切换到Claude-3.5", "AIEngineSelector");
                return AIEngineType.Claude35;
            }

            if (!claudeHealth)
            {
                Logger.LogWarning("Claude-3.5引擎不健康，切换到GPT-4", "AIEngineSelector");
                return AIEngineType.GPT4;
            }

            // 选择分数更高的引擎
            var selectedEngine = gptScore >= claudeScore ? AIEngineType.GPT4 : AIEngineType.Claude35;

            Logger.LogInfo($"智能选择引擎: {selectedEngine} (GPT-4分数: {gptScore:F2}, Claude-3.5分数: {claudeScore:F2}, 任务: {taskType})", "AIEngineSelector");

            return selectedEngine;
        }

        /// <summary>
        /// 计算引擎分数
        /// </summary>
        private float CalculateEngineScore(AIEngineType engine, AITaskType taskType)
        {
            var metrics = performanceMetrics[engine];
            var successRate = taskSuccessRates[taskType][engine];

            // 基础分数
            float score = successRate * 100;

            // 根据平均延迟调整
            if (metrics.AverageLatency > 0)
            {
                // 延迟越低分数越高，假设最佳延迟是500ms
                var latencyFactor = Math.Max(0, 1 - (metrics.AverageLatency / 2000));
                score *= (0.7f + 0.3f * latencyFactor);
            }

            // 根据成功率调整
            score *= (0.5f + 0.5f * metrics.SuccessRate);

            return score;
        }

        /// <summary>
        /// 基于任务类型选择
        /// </summary>
        private AIEngineType SelectBasedOnTask(AITaskType taskType)
        {
            return taskType switch
            {
                AITaskType.CodeGeneration => GetAvailableEngine(AIEngineType.GPT4),
                AITaskType.CreativeWriting => GetAvailableEngine(AIEngineType.Claude35),
                AITaskType.Reasoning => GetAvailableEngine(AIEngineType.Claude35),
                AITaskType.Translation => GetAvailableEngine(AIEngineType.GPT4),
                AITaskType.Analysis => GetAvailableEngine(AIEngineType.GPT4),
                AITaskType.IntentRecognition => GetAvailableEngine(AIEngineType.GPT4),
                _ => SelectRoundRobin()
            };
        }

        /// <summary>
        /// 基于性能选择
        /// </summary>
        private AIEngineType SelectBasedOnPerformance()
        {
            var gptMetrics = performanceMetrics[AIEngineType.GPT4];
            var claudeMetrics = performanceMetrics[AIEngineType.Claude35];

            if (gptMetrics.AverageLatency == 0 && claudeMetrics.AverageLatency == 0)
            {
                return SelectRoundRobin();
            }

            // 选择平均延迟更低的引擎
            if (gptMetrics.AverageLatency == 0) return AIEngineType.Claude35;
            if (claudeMetrics.AverageLatency == 0) return AIEngineType.GPT4;

            return gptMetrics.AverageLatency <= claudeMetrics.AverageLatency
                ? AIEngineType.GPT4
                : AIEngineType.Claude35;
        }

        /// <summary>
        /// 轮询选择
        /// </summary>
        private AIEngineType SelectRoundRobin()
        {
            // 简单的轮询：基于请求计数
            var totalRequests = performanceMetrics[AIEngineType.GPT4].RequestCount +
                               performanceMetrics[AIEngineType.Claude35].RequestCount;

            var engine = totalRequests % 2 == 0 ? AIEngineType.GPT4 : AIEngineType.Claude35;
            return GetAvailableEngine(engine);
        }

        /// <summary>
        /// A/B测试选择
        /// </summary>
        private AIEngineType SelectABTest()
        {
            var randomValue = random.NextDouble();
            var engine = randomValue < abTestSplitRatio ? AIEngineType.GPT4 : AIEngineType.Claude35;
            return GetAvailableEngine(engine);
        }

        /// <summary>
        /// 获取可用引擎
        /// </summary>
        private AIEngineType GetAvailableEngine(AIEngineType preferred)
        {
            if (IsEngineAvailable(preferred))
                return preferred;

            // 回退到另一个引擎
            var fallback = preferred == AIEngineType.GPT4 ? AIEngineType.Claude35 : AIEngineType.GPT4;

            if (IsEngineAvailable(fallback))
            {
                OnEngineSwitched?.Invoke(preferred, fallback);
                Logger.LogWarning($"引擎 {preferred} 不可用，回退到 {fallback}", "AIEngineSelector");
                return fallback;
            }

            throw new InvalidOperationException("没有可用的AI引擎");
        }

        /// <summary>
        /// 检查引擎是否可用
        /// </summary>
        private bool IsEngineAvailable(AIEngineType engine)
        {
            return engine switch
            {
                AIEngineType.GPT4 => gptService != null && gptService.IsInitialized,
                AIEngineType.Claude35 => claudeService != null && claudeService.IsInitialized,
                _ => false
            };
        }

        /// <summary>
        /// 检查引擎健康状态
        /// </summary>
        private async Task<bool> CheckEngineHealth(AIEngineType engine)
        {
            try
            {
                return engine switch
                {
                    AIEngineType.GPT4 => gptService != null && await gptService.CheckHealthAsync(),
                    AIEngineType.Claude35 => claudeService != null && await claudeService.CheckHealthAsync(),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 记录性能指标
        /// </summary>
        private void RecordPerformanceMetrics(AIEngineType engine, float latency, bool success)
        {
            if (!performanceMetrics.ContainsKey(engine))
                return;

            var metrics = performanceMetrics[engine];
            metrics.RecordRequest(latency, success);
        }

        /// <summary>
        /// 获取引擎性能指标
        /// </summary>
        public EnginePerformanceMetrics GetPerformanceMetrics(AIEngineType engine)
        {
            return performanceMetrics.TryGetValue(engine, out var metrics) ? metrics : null;
        }

        /// <summary>
        /// 获取所有引擎性能报告
        /// </summary>
        public Dictionary<AIEngineType, EnginePerformanceReport> GetAllPerformanceReports()
        {
            var reports = new Dictionary<AIEngineType, EnginePerformanceReport>();

            foreach (var kvp in performanceMetrics)
            {
                reports[kvp.Key] = kvp.Value.GetReport();
            }

            return reports;
        }

        /// <summary>
        /// 启动健康监控
        /// </summary>
        private async Task StartHealthMonitoring()
        {
            while (true)
            {
                await Task.Delay(60000); // 每分钟检查一次

                try
                {
                    var gptHealth = await CheckEngineHealth(AIEngineType.GPT4);
                    var claudeHealth = await CheckEngineHealth(AIEngineType.Claude35);

                    Logger.LogInfo($"AI引擎健康检查 - GPT-4: {(gptHealth ? "健康" : "异常")}, Claude-3.5: {(claudeHealth ? "健康" : "异常")}", "AIEngineSelector");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "AI引擎健康检查失败");
                }
            }
        }

        /// <summary>
        /// 设置选择策略
        /// </summary>
        public void SetSelectionStrategy(AIEngineSelectionStrategy strategy)
        {
            selectionStrategy = strategy;
            Logger.LogInfo($"AI引擎选择策略已更改为: {strategy}", "AIEngineSelector");
        }

        /// <summary>
        /// 重置性能统计
        /// </summary>
        public void ResetPerformanceStats()
        {
            foreach (var metrics in performanceMetrics.Values)
            {
                metrics.Reset();
            }

            Logger.LogInfo("性能统计已重置", "AIEngineSelector");
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            gptService?.DisposeAsync();
            claudeService?.DisposeAsync();
        }
    }

    /// <summary>
    /// AI引擎选择策略
    /// </summary>
    public enum AIEngineSelectionStrategy
    {
        GPT4Only,       // 仅使用GPT-4
        ClaudeOnly,     // 仅使用Claude-3.5
        RoundRobin,     // 轮询
        ABTesting,      // A/B测试
        TaskBased,      // 基于任务类型
        Performance,    // 基于性能
        Intelligent     // 智能选择（综合考虑）
    }

    /// <summary>
    /// 引擎性能指标
    /// </summary>
    public class EnginePerformanceMetrics
    {
        public AIEngineType EngineType { get; private set; }
        public int RequestCount { get; private set; }
        public int SuccessCount { get; private set; }
        public int FailureCount { get; private set; }
        public float TotalLatency { get; private set; }
        public float AverageLatency => RequestCount > 0 ? TotalLatency / RequestCount : 0;
        public float SuccessRate => RequestCount > 0 ? (float)SuccessCount / RequestCount : 0;
        public float FailureRate => RequestCount > 0 ? (float)FailureCount / RequestCount : 0;

        private Queue<float> recentLatencies = new Queue<float>();
        private const int MaxRecentLatencies = 100;

        public EnginePerformanceMetrics(AIEngineType engineType)
        {
            EngineType = engineType;
        }

        public void RecordRequest(float latency, bool success)
        {
            RequestCount++;

            if (success)
            {
                SuccessCount++;
                TotalLatency += latency;

                recentLatencies.Enqueue(latency);
                if (recentLatencies.Count > MaxRecentLatencies)
                {
                    recentLatencies.Dequeue();
                }
            }
            else
            {
                FailureCount++;
            }
        }

        public float GetRecentAverageLatency()
        {
            if (recentLatencies.Count == 0) return 0;

            float sum = 0;
            foreach (var latency in recentLatencies)
            {
                sum += latency;
            }
            return sum / recentLatencies.Count;
        }

        public EnginePerformanceReport GetReport()
        {
            return new EnginePerformanceReport
            {
                EngineType = EngineType,
                RequestCount = RequestCount,
                SuccessCount = SuccessCount,
                FailureCount = FailureCount,
                AverageLatency = AverageLatency,
                RecentAverageLatency = GetRecentAverageLatency(),
                SuccessRate = SuccessRate,
                FailureRate = FailureRate
            };
        }

        public void Reset()
        {
            RequestCount = 0;
            SuccessCount = 0;
            FailureCount = 0;
            TotalLatency = 0;
            recentLatencies.Clear();
        }
    }

    /// <summary>
    /// 引擎性能报告
    /// </summary>
    [Serializable]
    public class EnginePerformanceReport
    {
        public AIEngineType EngineType;
        public int RequestCount;
        public int SuccessCount;
        public int FailureCount;
        public float AverageLatency;
        public float RecentAverageLatency;
        public float SuccessRate;
        public float FailureRate;

        public override string ToString()
        {
            return $"{EngineType}: 请求数={RequestCount}, 成功率={SuccessRate:P1}, 平均延迟={AverageLatency:F0}ms";
        }
    }
}
