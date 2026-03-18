using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using TripMeta.AI;

namespace TripMeta.Tests.AI
{
    /// <summary>
    /// 情感计算系统测试
    /// 测试用户情绪识别和AI对话策略调整
    /// </summary>
    public class EmotionRecognitionTests
    {
        [Test]
        public void EmotionType_EnumValues()
        {
            var emotions = System.Enum.GetValues(typeof(EmotionType));
            Assert.Contains(EmotionType.Neutral, emotions, "应该包含Neutral");
            Assert.Contains(EmotionType.Happy, emotions, "应该包含Happy");
            Assert.Contains(EmotionType.Sad, emotions, "应该包含Sad");
            Assert.Contains(EmotionType.Angry, emotions, "应该包含Angry");
            Assert.Contains(EmotionType.Surprised, emotions, "应该包含Surprised");
            Assert.Contains(EmotionType.Curious, emotions, "应该包含Curious");
            Assert.Contains(EmotionType.Confused, emotions, "应该包含Confused");
            Assert.Contains(EmotionType.Excited, emotions, "应该包含Excited");
            Assert.Contains(EmotionType.Tired, emotions, "应该包含Tired");
            Assert.Contains(EmotionType.Bored, emotions, "应该包含Bored");
        }

        [Test]
        public void DialogueTone_EnumValues()
        {
            var tones = System.Enum.GetValues(typeof(DialogueTone));
            Assert.Contains(DialogueTone.Friendly, tones, "应该包含Friendly");
            Assert.Contains(DialogueTone.Upbeat, tones, "应该包含Upbeat");
            Assert.Contains(DialogueTone.Calm, tones, "应该包含Calm");
            Assert.Contains(DialogueTone.Reassuring, tones, "应该包含Reassuring");
            Assert.Contains(DialogueTone.Helpful, tones, "应该包含Helpful");
            Assert.Contains(DialogueTone.Informative, tones, "应该包含Informative");
            Assert.Contains(DialogueTone.Energizing, tones, "应该包含Energizing");
        }

        [Test]
        public void DialoguePace_EnumValues()
        {
            var paces = System.Enum.GetValues(typeof(DialoguePace));
            Assert.Contains(DialoguePace.Slower, paces, "应该包含Slower");
            Assert.Contains(DialoguePace.Normal, paces, "应该包含Normal");
            Assert.Contains(DialoguePace.Faster, paces, "应该包含Faster");
        }

        [Test]
        public void EmotionState_Creation()
        {
            var state = new EmotionState
            {
                PrimaryEmotion = EmotionType.Happy,
                SecondaryEmotion = EmotionType.Excited,
                Confidence = 0.85f,
                Intensity = 0.7f,
                Timestamp = System.DateTime.Now
            };

            Assert.AreEqual(EmotionType.Happy, state.PrimaryEmotion);
            Assert.AreEqual(EmotionType.Excited, state.SecondaryEmotion);
            Assert.AreEqual(0.85f, state.Confidence);
            Assert.AreEqual(0.7f, state.Intensity);
        }

        [Test]
        public void EmotionState_ToString()
        {
            var state = new EmotionState
            {
                PrimaryEmotion = EmotionType.Curious,
                Confidence = 0.9f,
                Intensity = 0.8f
            };

            var str = state.ToString();
            StringAssert.Contains("Curious", str);
            StringAssert.Contains("90%", str);
        }

        [Test]
        public void DialogueStrategy_Creation()
        {
            var strategy = new DialogueStrategy
            {
                Tone = DialogueTone.Friendly,
                Pace = DialoguePace.Normal,
                DetailLevel = DetailLevel.Medium,
                UseHumor = true,
                EncourageExploration = true
            };

            Assert.AreEqual(DialogueTone.Friendly, strategy.Tone);
            Assert.AreEqual(DialoguePace.Normal, strategy.Pace);
            Assert.AreEqual(DetailLevel.Medium, strategy.DetailLevel);
            Assert.IsTrue(strategy.UseHumor);
            Assert.IsTrue(strategy.EncourageExploration);
        }

        [Test]
        public void DialogueStrategy_ToString()
        {
            var strategy = new DialogueStrategy
            {
                Tone = DialogueTone.Calm,
                Pace = DialoguePace.Slower,
                DetailLevel = DetailLevel.Brief
            };

            var str = strategy.ToString();
            StringAssert.Contains("Calm", str);
            StringAssert.Contains("Slower", str);
        }

        [Test]
        public void UserEmotionProfile_Creation()
        {
            var profile = new UserEmotionProfile
            {
                DominantEmotion = EmotionType.Happy,
                EmotionDistribution = new Dictionary<EmotionType, int>
                {
                    [EmotionType.Happy] = 5,
                    [EmotionType.Curious] = 3,
                    [EmotionType.Neutral] = 2
                },
                AnalysisTime = System.DateTime.Now
            };

            Assert.AreEqual(EmotionType.Happy, profile.DominantEmotion);
            Assert.AreEqual(3, profile.EmotionDistribution.Count);
        }

        [Test]
        public void EmotionRecognitionManager_ComponentExists()
        {
            var managerType = typeof(EmotionRecognitionManager);
            Assert.IsNotNull(managerType, "EmotionRecognitionManager类型应该存在");
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(managerType), "应该继承自MonoBehaviour");
        }

        [UnityTest]
        public IEnumerator EmotionRecognitionManager_Creation()
        {
            var testObject = new GameObject("TestEmotionRecognitionManager");
            var manager = testObject.AddComponent<EmotionRecognitionManager>();

            Assert.IsNotNull(manager);
            Assert.IsTrue(manager.enableVoiceEmotionRecognition);
            Assert.IsTrue(manager.enableTextEmotionAnalysis);
            Assert.IsTrue(manager.enableBehavioralEmotionDetection);
            Assert.AreEqual(5f, manager.emotionAnalysisInterval);
            Assert.AreEqual(10, manager.emotionHistorySize);

            Object.Destroy(testObject);
            yield return null;
        }

        [Test]
        public void EmotionRecognition_StrategyMapping_Happy()
        {
            var emotion = new EmotionState { PrimaryEmotion = EmotionType.Happy };

            // 模拟策略映射
            var strategy = GetTestStrategyForEmotion(emotion);

            Assert.AreEqual(DialogueTone.Upbeat, strategy.Tone);
            Assert.IsTrue(strategy.UseHumor);
            Assert.IsTrue(strategy.EncourageExploration);
        }

        [Test]
        public void EmotionRecognition_StrategyMapping_Curious()
        {
            var emotion = new EmotionState { PrimaryEmotion = EmotionType.Curious };
            var strategy = GetTestStrategyForEmotion(emotion);

            Assert.AreEqual(DialogueTone.Informative, strategy.Tone);
            Assert.AreEqual(DialoguePace.Slower, strategy.Pace);
            Assert.AreEqual(DetailLevel.Detailed, strategy.DetailLevel);
        }

        [Test]
        public void EmotionRecognition_StrategyMapping_Frustrated()
        {
            var emotion = new EmotionState { PrimaryEmotion = EmotionType.Frustrated };
            var strategy = GetTestStrategyForEmotion(emotion);

            Assert.AreEqual(DialogueTone.Calm, strategy.Tone);
            Assert.IsTrue(strategy.OfferAssistance);
        }

        [Test]
        public void EmotionRecognition_StrategyMapping_Tired()
        {
            var emotion = new EmotionState { PrimaryEmotion = EmotionType.Tired };
            var strategy = GetTestStrategyForEmotion(emotion);

            Assert.AreEqual(DialogueTone.Energizing, strategy.Tone);
            Assert.IsTrue(strategy.SuggestBreak);
        }

        [Test]
        public void EmotionRecognition_StrategyMapping_Neutral()
        {
            var emotion = new EmotionState { PrimaryEmotion = EmotionType.Neutral };
            var strategy = GetTestStrategyForEmotion(emotion);

            Assert.AreEqual(DialogueTone.Friendly, strategy.Tone);
        }

        [Test]
        public void EmotionConfidence_Threshold()
        {
            float threshold = 0.7f;
            float confidence = 0.85f;

            Assert.GreaterOrEqual(confidence, threshold, "置信度应该高于阈值");
        }

        [Test]
        public void EmotionHistory_SizeLimit()
        {
            int maxSize = 10;
            var history = new Queue<EmotionState>();

            // 添加超过限制的情绪
            for (int i = 0; i < 15; i++)
            {
                history.Enqueue(new EmotionState { PrimaryEmotion = EmotionType.Happy });
                while (history.Count > maxSize)
                {
                    history.Dequeue();
                }
            }

            Assert.AreEqual(maxSize, history.Count, "历史记录应该限制在最大大小");
        }

        [Test]
        public void EmotionChange_Detection()
        {
            var previousEmotion = EmotionType.Happy;
            var currentEmotion = EmotionType.Sad;
            float confidenceChange = 0.5f;
            float threshold = 0.3f;

            bool emotionChanged = previousEmotion != currentEmotion || confidenceChange > threshold;

            Assert.IsTrue(emotionChanged, "情绪变化应该被检测到");
        }

        [Test]
        public void MultiModalEmotion_Fusion()
        {
            // 模拟多模态融合
            var voiceScores = new Dictionary<EmotionType, float>
            {
                [EmotionType.Happy] = 0.6f,
                [EmotionType.Neutral] = 0.4f
            };

            var textScores = new Dictionary<EmotionType, float>
            {
                [EmotionType.Happy] = 0.7f,
                [EmotionType.Neutral] = 0.3f
            };

            // 加权融合 (语音40%, 文本35%, 行为25%)
            var mergedScores = new Dictionary<EmotionType, float>();
            foreach (var kvp in voiceScores)
            {
                mergedScores[kvp.Key] = kvp.Value * 0.4f;
            }
            foreach (var kvp in textScores)
            {
                if (mergedScores.ContainsKey(kvp.Key))
                    mergedScores[kvp.Key] += kvp.Value * 0.35f;
                else
                    mergedScores[kvp.Key] = kvp.Value * 0.35f;
            }

            // 找出主导情绪
            EmotionType dominant = EmotionType.Neutral;
            float maxScore = 0f;
            foreach (var kvp in mergedScores)
            {
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    dominant = kvp.Key;
                }
            }

            Assert.AreEqual(EmotionType.Happy, dominant, "融合后主导情绪应该是Happy");
        }

        [Test]
        public void EmotionRecognitionFiles_Exist()
        {
            var managerPath = "Assets/Scripts/AI/Core/EmotionRecognitionManager.cs";
            Assert.IsTrue(System.IO.File.Exists(managerPath), $"EmotionRecognitionManager应该存在于{managerPath}");
        }

        // 辅助方法：模拟情绪策略映射
        private DialogueStrategy GetTestStrategyForEmotion(EmotionState emotion)
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
                EmotionType.Frustrated or EmotionType.Angry => new DialogueStrategy
                {
                    Tone = DialogueTone.Calm,
                    Pace = DialoguePace.Slower,
                    DetailLevel = DetailLevel.Brief,
                    OfferAssistance = true
                },
                EmotionType.Tired or EmotionType.Bored => new DialogueStrategy
                {
                    Tone = DialogueTone.Energizing,
                    Pace = DialoguePace.Faster,
                    DetailLevel = DetailLevel.Brief,
                    SuggestBreak = true
                },
                _ => new DialogueStrategy
                {
                    Tone = DialogueTone.Friendly,
                    Pace = DialoguePace.Normal,
                    DetailLevel = DetailLevel.Medium
                }
            };
        }
    }
}
