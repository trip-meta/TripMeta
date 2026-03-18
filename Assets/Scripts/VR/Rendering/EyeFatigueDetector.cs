using System;
using System.Collections.Generic;
using UnityEngine;

namespace TripMeta.VR.Rendering
{
    /// <summary>
    /// 视觉疲劳检测器
    /// 监测用户眼动数据，检测视觉疲劳并提供缓解建议
    /// </summary>
    public class EyeFatigueDetector : MonoBehaviour
    {
        [Header("疲劳检测配置")]
        public float checkInterval = 5f;
        public int historyWindowSize = 60; // 5分钟的历史数据
        public float blinkRateThreshold = 10f; // 每分钟眨眼次数阈值
        public float fixationDurationThreshold = 3f; // 过长凝视阈值
        public float saccadeVelocityThreshold = 300f; // 异常快速眼动阈值

        [Header("疲劳等级")]
        public float mildFatigueThreshold = 0.3f;
        public float moderateFatigueThreshold = 0.6f;
        public float severeFatigueThreshold = 0.8f;

        // 眼动历史数据
        private Queue<EyeDataSnapshot> eyeDataHistory = new Queue<EyeDataSnapshot>();
        private float lastCheckTime = 0f;
        private float currentFatigueLevel = 0f;

        // 统计
        private int blinkCount = 0;
        private float totalFixationDuration = 0f;
        private float averageSaccadeVelocity = 0f;

        public float CurrentFatigueLevel => currentFatigueLevel;
        public bool IsFatigued => currentFatigueLevel >= mildFatigueThreshold;

        public event Action<float> OnFatigueLevelChanged;
        public event Action<FatigueLevel> OnFatigueDetected;
        public event Action<string> OnRestRecommendation;

        void Start()
        {
            Debug.Log("[EyeFatigueDetector] 视觉疲劳检测器已启动");
        }

        void Update()
        {
            // 从 FoveatedRenderingManager 获取眼动数据
            UpdateEyeData();

            // 定期检查疲劳状态
            if (Time.time - lastCheckTime >= checkInterval)
            {
                AnalyzeFatigue();
                lastCheckTime = Time.time;
            }
        }

        /// <summary>
        /// 更新眼动数据
        /// </summary>
        private void UpdateEyeData()
        {
            if (FoveatedRenderingManager.Instance == null) return;

            var snapshot = new EyeDataSnapshot
            {
                timestamp = Time.time,
                gazePoint = FoveatedRenderingManager.Instance.CurrentGazePoint,
                isGazeStable = false, // 需要从实际眼动追踪获取
                blinkDetected = false, // 需要检测
                saccadeVelocity = 0f   // 需要计算
            };

            eyeDataHistory.Enqueue(snapshot);

            // 保持历史窗口大小
            while (eyeDataHistory.Count > historyWindowSize)
            {
                eyeDataHistory.Dequeue();
            }
        }

        /// <summary>
        /// 分析疲劳状态
        /// </summary>
        private void AnalyzeFatigue()
        {
            if (eyeDataHistory.Count < 10) return;

            // 计算各项指标
            CalculateBlinkRate();
            CalculateFixationPattern();
            CalculateSaccadePattern();

            // 综合评估疲劳等级
            float newFatigueLevel = CalculateFatigueScore();

            // 检测疲劳等级变化
            if (Mathf.Abs(newFatigueLevel - currentFatigueLevel) > 0.1f)
            {
                currentFatigueLevel = newFatigueLevel;
                OnFatigueLevelChanged?.Invoke(currentFatigueLevel);

                // 触发疲劳检测事件
                var fatigueLevel = GetFatigueLevel(currentFatigueLevel);
                if (fatigueLevel != FatigueLevel.None)
                {
                    OnFatigueDetected?.Invoke(fatigueLevel);
                    ProvideRestRecommendation(fatigueLevel);
                }
            }

            // 重置计数器
            blinkCount = 0;
            totalFixationDuration = 0f;
        }

        /// <summary>
        /// 计算眨眼率
        /// </summary>
        private void CalculateBlinkRate()
        {
            // 从历史数据中统计眨眼次数
            // 简化实现：实际应该检测眼睑开合状态
            float timeWindow = checkInterval / 60f; // 转换为分钟
            float blinkRate = blinkCount / timeWindow;

            // 眨眼率异常（过低表示眼睛干涩）
            if (blinkRate < blinkRateThreshold)
            {
                Debug.Log($"[EyeFatigueDetector] 眨眼率偏低: {blinkRate:F1} 次/分钟");
            }
        }

        /// <summary>
        /// 计算凝视模式
        /// </summary>
        private void CalculateFixationPattern()
        {
            // 分析凝视时间和频率
            var dataArray = eyeDataHistory.ToArray();
            float totalDuration = 0f;
            int fixationCount = 0;

            for (int i = 1; i < dataArray.Length; i++)
            {
                float deltaTime = dataArray[i].timestamp - dataArray[i - 1].timestamp;
                if (dataArray[i].isGazeStable)
                {
                    totalDuration += deltaTime;
                }
                else if (totalDuration > 0)
                {
                    fixationCount++;
                    totalFixationDuration += totalDuration;
                    totalDuration = 0f;
                }
            }

            // 检测过长凝视
            float avgFixationDuration = fixationCount > 0 ? totalFixationDuration / fixationCount : 0f;
            if (avgFixationDuration > fixationDurationThreshold)
            {
                Debug.Log($"[EyeFatigueDetector] 平均凝视时间过长: {avgFixationDuration:F2}秒");
            }
        }

        /// <summary>
        /// 计算扫视模式
        /// </summary>
        private void CalculateSaccadePattern()
        {
            // 分析扫视速度和频率
            var dataArray = eyeDataHistory.ToArray();
            float totalVelocity = 0f;
            int saccadeCount = 0;

            for (int i = 1; i < dataArray.Length; i++)
            {
                if (dataArray[i].saccadeVelocity > 0)
                {
                    totalVelocity += dataArray[i].saccadeVelocity;
                    saccadeCount++;

                    // 检测异常快速眼动
                    if (dataArray[i].saccadeVelocity > saccadeVelocityThreshold)
                    {
                        Debug.Log("[EyeFatigueDetector] 检测到异常快速眼动");
                    }
                }
            }

            averageSaccadeVelocity = saccadeCount > 0 ? totalVelocity / saccadeCount : 0f;
        }

        /// <summary>
        /// 计算疲劳分数
        /// </summary>
        private float CalculateFatigueScore()
        {
            float score = 0f;

            // 基于各项指标计算疲劳分数
            // 眨眼率权重: 30%
            // 凝视模式权重: 40%
            // 扫视模式权重: 30%

            // 眨眼率评分 (眨眼率越低，疲劳度越高)
            float blinkScore = Mathf.Clamp01((blinkRateThreshold - (blinkCount / (checkInterval / 60f))) / blinkRateThreshold);
            score += blinkScore * 0.3f;

            // 凝视时长评分 (凝视时间越长，疲劳度越高)
            float fixationScore = Mathf.Clamp01(totalFixationDuration / (fixationDurationThreshold * 60f));
            score += fixationScore * 0.4f;

            // 扫视速度评分 (速度异常，疲劳度越高)
            float saccadeScore = Mathf.Clamp01(averageSaccadeVelocity / saccadeVelocityThreshold);
            score += saccadeScore * 0.3f;

            return Mathf.Clamp01(score);
        }

        /// <summary>
        /// 获取疲劳等级
        /// </summary>
        private FatigueLevel GetFatigueLevel(float score)
        {
            if (score >= severeFatigueThreshold) return FatigueLevel.Severe;
            if (score >= moderateFatigueThreshold) return FatigueLevel.Moderate;
            if (score >= mildFatigueThreshold) return FatigueLevel.Mild;
            return FatigueLevel.None;
        }

        /// <summary>
        /// 提供休息建议
        /// </summary>
        private void ProvideRestRecommendation(FatigueLevel level)
        {
            string recommendation = level switch
            {
                FatigueLevel.Mild => "建议眨眼几次并调整视线焦点",
                FatigueLevel.Moderate => "建议休息片刻，闭眼10-20秒",
                FatigueLevel.Severe => "建议立即停止VR体验，休息至少15分钟",
                _ => ""
            };

            if (!string.IsNullOrEmpty(recommendation))
            {
                OnRestRecommendation?.Invoke(recommendation);
                Debug.Log($"[EyeFatigueDetector] 疲劳建议: {recommendation}");
            }
        }

        /// <summary>
        /// 重置疲劳检测
        /// </summary>
        public void ResetDetection()
        {
            eyeDataHistory.Clear();
            currentFatigueLevel = 0f;
            blinkCount = 0;
            totalFixationDuration = 0f;
            Debug.Log("[EyeFatigueDetector] 疲劳检测已重置");
        }
    }

    /// <summary>
    /// 眼动数据快照
    /// </summary>
    public struct EyeDataSnapshot
    {
        public float timestamp;
        public Vector2 gazePoint;
        public bool isGazeStable;
        public bool blinkDetected;
        public float saccadeVelocity;
    }

    /// <summary>
    /// 疲劳等级
    /// </summary>
    public enum FatigueLevel
    {
        None,       // 无疲劳
        Mild,       // 轻度疲劳
        Moderate,   // 中度疲劳
        Severe      // 严重疲劳
    }
}
