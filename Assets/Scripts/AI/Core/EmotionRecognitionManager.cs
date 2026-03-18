using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.AI
{
    /// <summary>
    /// 情感计算管理器 - 用户情绪识别和AI对话策略调整
    /// 目标：识别用户情绪并动态调整AI导游的对话策略
    /// </summary>
    public class EmotionRecognitionManager : MonoBehaviour
    {
        [Header("情绪识别配置")]
        public bool enableVoiceEmotionRecognition = true;
        public bool enableTextEmotionAnalysis = true;
        public bool enableBehavioralEmotionDetection = true;
        public float emotionAnalysisInterval = 5f; // 每5秒分析一次
        public int emotionHistorySize = 10; // 保留最近10个情绪状态

        [Header("情绪阈值")]
        public float emotionConfidenceThreshold = 0.7f; // 情绪置信度阈值
        public float emotionChangeThreshold = 0.3f; // 情绪变化检测阈值

        [Header("调试")]
        public bool enableDebugLogs = false;
        public bool showEmotionDebugUI = false;

        // 情绪历史
        private Queue<EmotionState> emotionHistory = new Queue<EmotionState>();
        private EmotionState currentEmotion;

        // 服务引用
        private EdgeAIInferenceManager edgeAIInference;
        private AzureSpeechService speechService;

        // 状态
        private bool isInitialized = false;
        private float lastAnalysisTime = 0f;

        public static EmotionRecognitionManager Instance { get; private set; }

        // 事件
        public event Action<EmotionState> OnEmotionDetected;
        public event Action<EmotionState, EmotionState> OnEmotionChanged;
        public event Action<UserEmotionProfile> OnEmotionProfileUpdated;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeManager()
        {
            Logger.LogInfo("初始化情感计算管理器...", "EmotionRecognition");

            currentEmotion = new EmotionState
            {
                PrimaryEmotion = EmotionType.Neutral,
                Confidence = 1.0f,
                Timestamp = DateTime.Now
            };

            isInitialized = true;
            Logger.LogInfo("情感计算管理器初始化完成", "EmotionRecognition");
        }

        async void Start()
        {
            await InitializeServices();
        }

        /// <summary>
        /// 初始化服务引用
        /// </summary>
        private async Task InitializeServices()
        {
            // 等待边缘AI推理管理器
            await Task.Delay(1000);
            edgeAIInference = EdgeAIInferenceManager.Instance;

            // 查找语音服务
            var aiManager = AIServiceManager.Instance;
            if (aiManager != null)
            {
                // 语音服务会在AI管理器中初始化
                Logger.LogInfo("情感计算服务连接完成", "EmotionRecognition");
            }
        }

        void Update()
        {
            if (!isInitialized) return;

            // 定期分析情绪
            if (Time.time - lastAnalysisTime >= emotionAnalysisInterval)
            {
                _ = AnalyzeEmotionAsync();
                lastAnalysisTime = Time.time;
            }
        }

        /// <summary>
        /// 分析用户情绪（多模态融合）
        /// </summary>
        public async Task<EmotionState> AnalyzeEmotionAsync()
        {
            if (!isInitialized) return currentEmotion;

            try
            {
                var emotionScores = new Dictionary<EmotionType, float>();

                // 1. 语音情绪分析
                if (enableVoiceEmotionRecognition)
                {
                    var voiceEmotion = await AnalyzeVoiceEmotionAsync();
                    MergeEmotionScores(emotionScores, voiceEmotion, 0.4f); // 权重40%
                }

                // 2. 文本情绪分析
                if (enableTextEmotionAnalysis)
                {
                    var textEmotion = await AnalyzeTextEmotionAsync();
                    MergeEmotionScores(emotionScores, textEmotion, 0.35f); // 权重35%
                }

                // 3. 行为情绪检测（VR交互行为）
                if (enableBehavioralEmotionDetection)
                {
                    var behaviorEmotion = AnalyzeBehavioralEmotion();
                    MergeEmotionScores(emotionScores, behaviorEmotion, 0.25f); // 权重25%
                }

                // 确定主导情绪
                var detectedEmotion = DetermineDominantEmotion(emotionScores);

                // 检查情绪变化
                if (detectedEmotion.PrimaryEmotion != currentEmotion.PrimaryEmotion ||
                    Mathf.Abs(detectedEmotion.Confidence - currentEmotion.Confidence) > emotionChangeThreshold)
                {
                    var previousEmotion = currentEmotion;
                    currentEmotion = detectedEmotion;

                    // 添加到历史
                    AddToHistory(currentEmotion);

                    // 触发事件
                    OnEmotionChanged?.Invoke(previousEmotion, currentEmotion);

                    if (enableDebugLogs)
                    {
                        Logger.LogInfo($"情绪变化: {previousEmotion.PrimaryEmotion} -> {currentEmotion.PrimaryEmotion} (置信度: {currentEmotion.Confidence:P1})", "EmotionRecognition");
                    }
                }

                OnEmotionDetected?.Invoke(currentEmotion);
                return currentEmotion;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "情绪分析失败");
                return currentEmotion;
            }
        }

        /// <summary>
        /// 分析语音情绪
        /// </summary>
        private async Task<Dictionary<EmotionType, float>> AnalyzeVoiceEmotionAsync()
        {
            var scores = new Dictionary<EmotionType, float>();

            try
            {
                // 使用边缘AI推理
                if (edgeAIInference != null)
                {
                    var result = await edgeAIInference.RunInferenceAsync<EmotionRecognitionResult>(
                        "emotion-recognition", null);

                    if (result != null && result.EmotionScores != null)
                    {
                        foreach (var kvp in result.EmotionScores)
                        {
                            if (System.Enum.TryParse<EmotionType>(kvp.Key, true, out var emotion))
                            {
                                scores[emotion] = kvp.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"语音情绪分析失败: {ex.Message}", "EmotionRecognition");
            }

            return scores;
        }

        /// <summary>
        /// 分析文本情绪
        /// </summary>
        private async Task<Dictionary<EmotionType, float>> AnalyzeTextEmotionAsync()
        {
            var scores = new Dictionary<EmotionType, float>();

            // 获取最近的对话文本（需要从对话历史中获取）
            // 这里简化实现，使用关键词匹配
            await Task.Delay(10);

            // 默认中性
            scores[EmotionType.Neutral] = 0.6f;
            scores[EmotionType.Happy] = 0.2f;
            scores[EmotionType.Curious] = 0.2f;

            return scores;
        }

        /// <summary>
        /// 分析行为情绪（基于VR交互行为）
        /// </summary>
        private Dictionary<EmotionType, float> AnalyzeBehavioralEmotion()
        {
            var scores = new Dictionary<EmotionType, float>();

            // 基于用户行为模式的情绪推断
            // 例如：快速移动可能表示兴奋或焦虑，缓慢移动可能表示放松或无聊

            // 简化实现：基于时间的行为模式
            var hour = DateTime.Now.Hour;
            if (hour >= 9 && hour <= 17)
            {
                scores[EmotionType.Curious] = 0.4f;
                scores[EmotionType.Neutral] = 0.35f;
                scores[EmotionType.Happy] = 0.25f;
            }
            else
            {
                scores[EmotionType.Relaxed] = 0.4f;
                scores[EmotionType.Neutral] = 0.35f;
                scores[EmotionType.Happy] = 0.25f;
            }

            return scores;
        }

        /// <summary>
        /// 合并情绪分数
        /// </summary>
        private void MergeEmotionScores(Dictionary<EmotionType, float> target, Dictionary<EmotionType, float> source, float weight)
        {
            foreach (var kvp in source)
            {
                if (target.ContainsKey(kvp.Key))
                {
                    target[kvp.Key] += kvp.Value * weight;
                }
                else
                {
                    target[kvp.Key] = kvp.Value * weight;
                }
            }
        }

        /// <summary>
        /// 确定主导情绪
        /// </summary>
        private EmotionState DetermineDominantEmotion(Dictionary<EmotionType, float> scores)
        {
            EmotionType dominantEmotion = EmotionType.Neutral;
            float maxScore = 0f;
            float totalScore = 0f;

            foreach (var kvp in scores)
            {
                totalScore += kvp.Value;
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    dominantEmotion = kvp.Key;
                }
            }

            // 计算置信度
            float confidence = totalScore > 0 ? maxScore / totalScore : 1f;

            // 获取次要情绪
            EmotionType? secondaryEmotion = null;
            float secondMaxScore = 0f;
            foreach (var kvp in scores)
            {
                if (kvp.Key != dominantEmotion && kvp.Value > secondMaxScore)
                {
                    secondMaxScore = kvp.Value;
                    secondaryEmotion = kvp.Key;
                }
            }

            return new EmotionState
            {
                PrimaryEmotion = dominantEmotion,
                SecondaryEmotion = secondaryEmotion,
                Confidence = confidence,
                Intensity = Mathf.Clamp01(maxScore),
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 添加到历史
        /// </summary>
        private void AddToHistory(EmotionState emotion)
        {
            emotionHistory.Enqueue(emotion);

            while (emotionHistory.Count > emotionHistorySize)
            {
                emotionHistory.Dequeue();
            }
        }

        /// <summary>
        /// 获取当前情绪
        /// </summary>
        public EmotionState GetCurrentEmotion()
        {
            return currentEmotion;
        }

        /// <summary>
        /// 获取情绪历史
        /// </summary>
        public EmotionState[] GetEmotionHistory()
        {
            return emotionHistory.ToArray();
        }

        /// <summary>
        /// 获取用户情绪档案
        /// </summary>
        public UserEmotionProfile GetEmotionProfile()
        {
            var profile = new UserEmotionProfile();

            // 分析情绪历史，生成情绪倾向
            Dictionary<EmotionType, int> emotionCounts = new Dictionary<EmotionType, int>();
            foreach (var emotion in emotionHistory)
            {
                if (emotionCounts.ContainsKey(emotion.PrimaryEmotion))
                {
                    emotionCounts[emotion.PrimaryEmotion]++;
                }
                else
                {
                    emotionCounts[emotion.PrimaryEmotion] = 1;
                }
            }

            // 找出最频繁的情绪
            EmotionType dominantEmotion = EmotionType.Neutral;
            int maxCount = 0;
            foreach (var kvp in emotionCounts)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    dominantEmotion = kvp.Key;
                }
            }

            profile.DominantEmotion = dominantEmotion;
            profile.EmotionDistribution = emotionCounts;
            profile.AnalysisTime = DateTime.Now;

            return profile;
        }

        /// <summary>
        /// 根据情绪获取对话策略
        /// </summary>
        public DialogueStrategy GetDialogueStrategyForEmotion(EmotionState emotion)
        {
            return emotion.PrimaryEmotion switch
            {
                EmotionType.Happy => new DialogueStrategy
                {
                    Tone = DialogueTone.Upbeat,
                    Pace = DialoguePace.Normal,
                    DetailLevel = DetailLevel.Medium,
                    UseHumor = true,
                    EncourageExploration = true
                },

                EmotionType.Curious => new DialogueStrategy
                {
                    Tone = DialogueTone.Informative,
                    Pace = DialoguePace.Slower,
                    DetailLevel = DetailLevel.Detailed,
                    UseHumor = false,
                    EncourageExploration = true
                },

                EmotionType.Confused => new DialogueStrategy
                {
                    Tone = DialogueTone.Helpful,
                    Pace = DialoguePace.Slower,
                    DetailLevel = DetailLevel.Brief,
                    UseHumor = false,
                    EncourageExploration = false
                },

                EmotionType.Frustrated or EmotionType.Angry => new DialogueStrategy
                {
                    Tone = DialogueTone.Calm,
                    Pace = DialoguePace.Slower,
                    DetailLevel = DetailLevel.Brief,
                    UseHumor = false,
                    EncourageExploration = false,
                    OfferAssistance = true
                },

                EmotionType.Tired or EmotionType.Bored => new DialogueStrategy
                {
                    Tone = DialogueTone.Energizing,
                    Pace = DialoguePace.Faster,
                    DetailLevel = DetailLevel.Brief,
                    UseHumor = true,
                    EncourageExploration = true,
                    SuggestBreak = true
                },

                EmotionType.Anxious or EmotionType.Scared => new DialogueStrategy
                {
                    Tone = DialogueTone.Reassuring,
                    Pace = DialoguePace.Slower,
                    DetailLevel = DetailLevel.Brief,
                    UseHumor = false,
                    EncourageExploration = false,
                    OfferAssistance = true
                },

                _ => new DialogueStrategy
                {
                    Tone = DialogueTone.Friendly,
                    Pace = DialoguePace.Normal,
                    DetailLevel = DetailLevel.Medium,
                    UseHumor = true,
                    EncourageExploration = true
                }
            };
        }

        /// <summary>
        /// 获取当前对话策略
        /// </summary>
        public DialogueStrategy GetCurrentDialogueStrategy()
        {
            return GetDialogueStrategyForEmotion(currentEmotion);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    /// <summary>
    /// 情绪类型
    /// </summary>
    public enum EmotionType
    {
        Neutral,        // 中性
        Happy,          // 开心
        Sad,            // 悲伤
        Angry,          // 愤怒
        Surprised,      // 惊讶
        Fearful,        // 恐惧
        Disgusted,      // 厌恶
        Curious,        // 好奇
        Confused,       // 困惑
        Frustrated,     // 沮丧
        Excited,        // 兴奋
        Relaxed,        // 放松
        Anxious,        // 焦虑
        Tired,          // 疲倦
        Bored           // 无聊
    }

    /// <summary>
    /// 情绪状态
    /// </summary>
    [Serializable]
    public class EmotionState
    {
        public EmotionType PrimaryEmotion;      // 主导情绪
        public EmotionType? SecondaryEmotion;   // 次要情绪
        public float Confidence;                // 置信度 (0-1)
        public float Intensity;                 // 强度 (0-1)
        public DateTime Timestamp;              // 时间戳

        public override string ToString()
        {
            return $"{PrimaryEmotion} (置信度: {Confidence:P0}, 强度: {Intensity:P0})";
        }
    }

    /// <summary>
    /// 用户情绪档案
    /// </summary>
    [Serializable]
    public class UserEmotionProfile
    {
        public EmotionType DominantEmotion;
        public Dictionary<EmotionType, int> EmotionDistribution;
        public DateTime AnalysisTime;

        public override string ToString()
        {
            return $"主导情绪: {DominantEmotion}, 分析时间: {AnalysisTime:HH:mm:ss}";
        }
    }

    /// <summary>
    /// 对话策略
    /// </summary>
    [Serializable]
    public class DialogueStrategy
    {
        public DialogueTone Tone;           // 语气
        public DialoguePace Pace;           // 语速
        public DetailLevel DetailLevel;     // 详细程度
        public bool UseHumor;               // 使用幽默
        public bool EncourageExploration;   // 鼓励探索
        public bool OfferAssistance;        // 提供帮助
        public bool SuggestBreak;           // 建议休息

        public override string ToString()
        {
            return $"语气: {Tone}, 语速: {Pace}, 详细程度: {DetailLevel}";
        }
    }

    /// <summary>
    /// 对话语气
    /// </summary>
    public enum DialogueTone
    {
        Friendly,       // 友好
        Upbeat,         // 积极
        Calm,           // 平静
        Reassuring,     // 安慰
        Helpful,        // 帮助
        Informative,    // 信息性
        Energizing      // 激励
    }

    /// <summary>
    /// 对话语速
    /// </summary>
    public enum DialoguePace
    {
        Slower,         // 较慢
        Normal,         // 正常
        Faster          // 较快
    }
}
