using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.VR.Interaction
{
    /// <summary>
    /// 手势识别器 - 识别VR手部手势
    /// </summary>
    public class GestureRecognizer : MonoBehaviour
    {
        [Header("手势识别设置")]
        [SerializeField] private bool enableGestureRecognition = true;
        [SerializeField] private float recognitionThreshold = 0.8f;
        [SerializeField] private float gestureTimeout = 2f;
        [SerializeField] private int maxGestureHistory = 10;
        
        [Header("手部追踪")]
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private bool useXRHandTracking = true;
        
        // 手势数据
        private Dictionary<GestureType, GesturePattern> gesturePatterns;
        private List<HandPose> leftHandHistory;
        private List<HandPose> rightHandHistory;
        private GestureType lastRecognizedGesture;
        private float lastGestureTime;
        
        // 手部状态
        private HandState leftHandState;
        private HandState rightHandState;
        
        public event Action<GestureType, HandType> OnGestureRecognized;
        public event Action<HandPose, HandType> OnHandPoseUpdated;
        
        public bool IsInitialized { get; private set; }
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (!enableGestureRecognition || !IsInitialized) return;
            
            UpdateHandTracking();
            ProcessGestureRecognition();
        }
        
        /// <summary>
        /// 初始化手势识别器
        /// </summary>
        public void Initialize()
        {
            try
            {
                Logger.LogInfo("初始化手势识别器...", "GestureRecognizer");
                
                // 初始化手势模式
                InitializeGesturePatterns();
                
                // 初始化手部历史记录
                leftHandHistory = new List<HandPose>();
                rightHandHistory = new List<HandPose>();
                
                // 初始化手部状态
                leftHandState = new HandState();
                rightHandState = new HandState();
                
                // 查找手部Transform
                if (leftHandTransform == null || rightHandTransform == null)
                {
                    FindHandTransforms();
                }
                
                IsInitialized = true;
                Logger.LogInfo("手势识别器初始化完成", "GestureRecognizer");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "手势识别器初始化失败");
            }
        }
        
        /// <summary>
        /// 初始化手势模式
        /// </summary>
        private void InitializeGesturePatterns()
        {
            gesturePatterns = new Dictionary<GestureType, GesturePattern>();
            
            // 指向手势
            gesturePatterns[GestureType.Point] = new GesturePattern
            {
                gestureType = GestureType.Point,
                requiredFingers = new[] { FingerType.Index },
                fingerStates = new Dictionary<FingerType, FingerState>
                {
                    { FingerType.Index, FingerState.Extended },
                    { FingerType.Middle, FingerState.Curled },
                    { FingerType.Ring, FingerState.Curled },
                    { FingerType.Pinky, FingerState.Curled },
                    { FingerType.Thumb, FingerState.Curled }
                }
            };
            
            // 抓取手势
            gesturePatterns[GestureType.Grab] = new GesturePattern
            {
                gestureType = GestureType.Grab,
                requiredFingers = new[] { FingerType.Index, FingerType.Middle, FingerType.Ring, FingerType.Pinky },
                fingerStates = new Dictionary<FingerType, FingerState>
                {
                    { FingerType.Index, FingerState.Curled },
                    { FingerType.Middle, FingerState.Curled },
                    { FingerType.Ring, FingerState.Curled },
                    { FingerType.Pinky, FingerState.Curled },
                    { FingerType.Thumb, FingerState.Extended }
                }
            };
            
            // 捏取手势
            gesturePatterns[GestureType.Pinch] = new GesturePattern
            {
                gestureType = GestureType.Pinch,
                requiredFingers = new[] { FingerType.Thumb, FingerType.Index },
                fingerStates = new Dictionary<FingerType, FingerState>
                {
                    { FingerType.Thumb, FingerState.Extended },
                    { FingerType.Index, FingerState.Extended },
                    { FingerType.Middle, FingerState.Curled },
                    { FingerType.Ring, FingerState.Curled },
                    { FingerType.Pinky, FingerState.Curled }
                },
                proximityThreshold = 0.03f // 拇指和食指需要接近
            };
            
            // 挥手手势
            gesturePatterns[GestureType.Wave] = new GesturePattern
            {
                gestureType = GestureType.Wave,
                requiredFingers = new[] { FingerType.Index, FingerType.Middle, FingerType.Ring, FingerType.Pinky },
                fingerStates = new Dictionary<FingerType, FingerState>
                {
                    { FingerType.Index, FingerState.Extended },
                    { FingerType.Middle, FingerState.Extended },
                    { FingerType.Ring, FingerState.Extended },
                    { FingerType.Pinky, FingerState.Extended },
                    { FingerType.Thumb, FingerState.Extended }
                },
                requiresMotion = true,
                motionThreshold = 0.1f
            };
            
            // 点赞手势
            gesturePatterns[GestureType.ThumbsUp] = new GesturePattern
            {
                gestureType = GestureType.ThumbsUp,
                requiredFingers = new[] { FingerType.Thumb },
                fingerStates = new Dictionary<FingerType, FingerState>
                {
                    { FingerType.Thumb, FingerState.Extended },
                    { FingerType.Index, FingerState.Curled },
                    { FingerType.Middle, FingerState.Curled },
                    { FingerType.Ring, FingerState.Curled },
                    { FingerType.Pinky, FingerState.Curled }
                }
            };
            
            // 张开手掌
            gesturePatterns[GestureType.OpenPalm] = new GesturePattern
            {
                gestureType = GestureType.OpenPalm,
                requiredFingers = new[] { FingerType.Thumb, FingerType.Index, FingerType.Middle, FingerType.Ring, FingerType.Pinky },
                fingerStates = new Dictionary<FingerType, FingerState>
                {
                    { FingerType.Thumb, FingerState.Extended },
                    { FingerType.Index, FingerState.Extended },
                    { FingerType.Middle, FingerState.Extended },
                    { FingerType.Ring, FingerState.Extended },
                    { FingerType.Pinky, FingerState.Extended }
                }
            };
            
            // 握拳
            gesturePatterns[GestureType.Fist] = new GesturePattern
            {
                gestureType = GestureType.Fist,
                requiredFingers = new[] { FingerType.Index, FingerType.Middle, FingerType.Ring, FingerType.Pinky },
                fingerStates = new Dictionary<FingerType, FingerState>
                {
                    { FingerType.Thumb, FingerState.Curled },
                    { FingerType.Index, FingerState.Curled },
                    { FingerType.Middle, FingerState.Curled },
                    { FingerType.Ring, FingerState.Curled },
                    { FingerType.Pinky, FingerState.Curled }
                }
            };
        }
        
        /// <summary>
        /// 查找手部Transform
        /// </summary>
        private void FindHandTransforms()
        {
            // 尝试从XR控制器获取
            var controllers = FindObjectsOfType<XRController>();
            foreach (var controller in controllers)
            {
                if (controller.controllerNode == XRNode.LeftHand)
                    leftHandTransform = controller.transform;
                else if (controller.controllerNode == XRNode.RightHand)
                    rightHandTransform = controller.transform;
            }
        }
        
        /// <summary>
        /// 更新手部追踪
        /// </summary>
        private void UpdateHandTracking()
        {
            // 更新左手
            if (leftHandTransform != null)
            {
                var leftPose = GetHandPose(leftHandTransform, HandType.Left);
                UpdateHandHistory(leftPose, HandType.Left);
                leftHandState.currentPose = leftPose;
                OnHandPoseUpdated?.Invoke(leftPose, HandType.Left);
            }
            
            // 更新右手
            if (rightHandTransform != null)
            {
                var rightPose = GetHandPose(rightHandTransform, HandType.Right);
                UpdateHandHistory(rightPose, HandType.Right);
                rightHandState.currentPose = rightPose;
                OnHandPoseUpdated?.Invoke(rightPose, HandType.Right);
            }
        }
        
        /// <summary>
        /// 获取手部姿态
        /// </summary>
        private HandPose GetHandPose(Transform handTransform, HandType handType)
        {
            var pose = new HandPose
            {
                handType = handType,
                position = handTransform.position,
                rotation = handTransform.rotation,
                timestamp = Time.time
            };
            
            // 如果启用XR手部追踪，获取手指数据
            if (useXRHandTracking)
            {
                pose.fingerPoses = GetFingerPoses(handType);
            }
            else
            {
                // 使用控制器输入模拟手指状态
                pose.fingerPoses = GetSimulatedFingerPoses(handType);
            }
            
            return pose;
        }
        
        /// <summary>
        /// 获取手指姿态（XR手部追踪）
        /// </summary>
        private Dictionary<FingerType, FingerPose> GetFingerPoses(HandType handType)
        {
            var fingerPoses = new Dictionary<FingerType, FingerPose>();
            
            // 这里需要根据具体的XR SDK实现
            // 例如Oculus Hand Tracking、PICO Hand Tracking等
            
            // 示例实现（需要根据实际SDK调整）
            foreach (FingerType fingerType in Enum.GetValues(typeof(FingerType)))
            {
                fingerPoses[fingerType] = new FingerPose
                {
                    fingerType = fingerType,
                    isExtended = UnityEngine.Random.value > 0.5f, // 示例数据
                    flexion = UnityEngine.Random.Range(0f, 1f),
                    confidence = 0.8f
                };
            }
            
            return fingerPoses;
        }
        
        /// <summary>
        /// 获取模拟手指姿态（控制器输入）
        /// </summary>
        private Dictionary<FingerType, FingerPose> GetSimulatedFingerPoses(HandType handType)
        {
            var fingerPoses = new Dictionary<FingerType, FingerPose>();
            
            var node = handType == HandType.Left ? XRNode.LeftHand : XRNode.RightHand;
            var device = InputDevices.GetDeviceAtXRNode(node);
            
            // 获取控制器输入
            device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed);
            device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed);
            device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed);
            
            // 根据按钮状态模拟手指
            fingerPoses[FingerType.Index] = new FingerPose
            {
                fingerType = FingerType.Index,
                isExtended = !triggerPressed,
                flexion = triggerPressed ? 1f : 0f,
                confidence = 0.9f
            };
            
            fingerPoses[FingerType.Middle] = new FingerPose
            {
                fingerType = FingerType.Middle,
                isExtended = !gripPressed,
                flexion = gripPressed ? 1f : 0f,
                confidence = 0.9f
            };
            
            fingerPoses[FingerType.Ring] = new FingerPose
            {
                fingerType = FingerType.Ring,
                isExtended = !gripPressed,
                flexion = gripPressed ? 1f : 0f,
                confidence = 0.9f
            };
            
            fingerPoses[FingerType.Pinky] = new FingerPose
            {
                fingerType = FingerType.Pinky,
                isExtended = !gripPressed,
                flexion = gripPressed ? 1f : 0f,
                confidence = 0.9f
            };
            
            fingerPoses[FingerType.Thumb] = new FingerPose
            {
                fingerType = FingerType.Thumb,
                isExtended = !primaryPressed,
                flexion = primaryPressed ? 1f : 0f,
                confidence = 0.9f
            };
            
            return fingerPoses;
        }
        
        /// <summary>
        /// 更新手部历史记录
        /// </summary>
        private void UpdateHandHistory(HandPose pose, HandType handType)
        {
            var history = handType == HandType.Left ? leftHandHistory : rightHandHistory;
            
            history.Add(pose);
            
            if (history.Count > maxGestureHistory)
            {
                history.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 处理手势识别
        /// </summary>
        private void ProcessGestureRecognition()
        {
            // 检查左手手势
            var leftGesture = RecognizeGesture(leftHandState.currentPose, leftHandHistory);
            if (leftGesture != GestureType.None && leftGesture != lastRecognizedGesture)
            {
                OnGestureRecognized?.Invoke(leftGesture, HandType.Left);
                lastRecognizedGesture = leftGesture;
                lastGestureTime = Time.time;
            }
            
            // 检查右手手势
            var rightGesture = RecognizeGesture(rightHandState.currentPose, rightHandHistory);
            if (rightGesture != GestureType.None && rightGesture != lastRecognizedGesture)
            {
                OnGestureRecognized?.Invoke(rightGesture, HandType.Right);
                lastRecognizedGesture = rightGesture;
                lastGestureTime = Time.time;
            }
            
            // 重置手势超时
            if (Time.time - lastGestureTime > gestureTimeout)
            {
                lastRecognizedGesture = GestureType.None;
            }
        }
        
        /// <summary>
        /// 识别手势
        /// </summary>
        public GestureType RecognizeGesture()
        {
            if (leftHandState.currentPose != null)
            {
                var gesture = RecognizeGesture(leftHandState.currentPose, leftHandHistory);
                if (gesture != GestureType.None) return gesture;
            }
            
            if (rightHandState.currentPose != null)
            {
                var gesture = RecognizeGesture(rightHandState.currentPose, rightHandHistory);
                if (gesture != GestureType.None) return gesture;
            }
            
            return GestureType.None;
        }
        
        /// <summary>
        /// 识别特定手势
        /// </summary>
        private GestureType RecognizeGesture(HandPose currentPose, List<HandPose> history)
        {
            if (currentPose?.fingerPoses == null) return GestureType.None;
            
            float bestScore = 0f;
            GestureType bestGesture = GestureType.None;
            
            foreach (var pattern in gesturePatterns.Values)
            {
                float score = CalculateGestureScore(currentPose, pattern, history);
                
                if (score > bestScore && score >= recognitionThreshold)
                {
                    bestScore = score;
                    bestGesture = pattern.gestureType;
                }
            }
            
            return bestGesture;
        }
        
        /// <summary>
        /// 计算手势匹配分数
        /// </summary>
        private float CalculateGestureScore(HandPose pose, GesturePattern pattern, List<HandPose> history)
        {
            float score = 0f;
            int totalChecks = 0;
            
            // 检查手指状态匹配
            foreach (var fingerState in pattern.fingerStates)
            {
                if (pose.fingerPoses.ContainsKey(fingerState.Key))
                {
                    var fingerPose = pose.fingerPoses[fingerState.Key];
                    bool matches = false;
                    
                    switch (fingerState.Value)
                    {
                        case FingerState.Extended:
                            matches = fingerPose.isExtended && fingerPose.flexion < 0.3f;
                            break;
                        case FingerState.Curled:
                            matches = !fingerPose.isExtended && fingerPose.flexion > 0.7f;
                            break;
                        case FingerState.Neutral:
                            matches = fingerPose.flexion >= 0.3f && fingerPose.flexion <= 0.7f;
                            break;
                    }
                    
                    if (matches)
                    {
                        score += fingerPose.confidence;
                    }
                    
                    totalChecks++;
                }
            }
            
            // 检查手指接近度（用于捏取手势）
            if (pattern.proximityThreshold > 0)
            {
                // 这里需要实现手指间距离计算
                // 简化实现
                score += 0.5f;
                totalChecks++;
            }
            
            // 检查运动要求（用于挥手手势）
            if (pattern.requiresMotion && history.Count >= 3)
            {
                float motion = CalculateMotion(history);
                if (motion > pattern.motionThreshold)
                {
                    score += 1f;
                }
                totalChecks++;
            }
            
            return totalChecks > 0 ? score / totalChecks : 0f;
        }
        
        /// <summary>
        /// 计算运动量
        /// </summary>
        private float CalculateMotion(List<HandPose> history)
        {
            if (history.Count < 2) return 0f;
            
            float totalMotion = 0f;
            for (int i = 1; i < history.Count; i++)
            {
                totalMotion += Vector3.Distance(history[i].position, history[i - 1].position);
            }
            
            return totalMotion / (history.Count - 1);
        }
        
        /// <summary>
        /// 设置识别阈值
        /// </summary>
        public void SetRecognitionThreshold(float threshold)
        {
            recognitionThreshold = Mathf.Clamp01(threshold);
        }
        
        /// <summary>
        /// 启用/禁用手势识别
        /// </summary>
        public void SetGestureRecognitionEnabled(bool enabled)
        {
            enableGestureRecognition = enabled;
        }
        
        /// <summary>
        /// 添加自定义手势模式
        /// </summary>
        public void AddCustomGesturePattern(GesturePattern pattern)
        {
            gesturePatterns[pattern.gestureType] = pattern;
        }
        
        private void OnDestroy()
        {
            gesturePatterns?.Clear();
            leftHandHistory?.Clear();
            rightHandHistory?.Clear();
        }
    }
    
    /// <summary>
    /// 手势模式
    /// </summary>
    [Serializable]
    public class GesturePattern
    {
        public GestureType gestureType;
        public FingerType[] requiredFingers;
        public Dictionary<FingerType, FingerState> fingerStates;
        public float proximityThreshold = 0f;
        public bool requiresMotion = false;
        public float motionThreshold = 0f;
        public float minDuration = 0f;
        public float maxDuration = float.MaxValue;
    }
    
    /// <summary>
    /// 手部姿态
    /// </summary>
    [Serializable]
    public class HandPose
    {
        public HandType handType;
        public Vector3 position;
        public Quaternion rotation;
        public Dictionary<FingerType, FingerPose> fingerPoses;
        public float timestamp;
        public float confidence = 1f;
    }
    
    /// <summary>
    /// 手指姿态
    /// </summary>
    [Serializable]
    public class FingerPose
    {
        public FingerType fingerType;
        public bool isExtended;
        public float flexion; // 0 = 完全伸展, 1 = 完全弯曲
        public Vector3 tipPosition;
        public Quaternion rotation;
        public float confidence = 1f;
    }
    
    /// <summary>
    /// 手部状态
    /// </summary>
    [Serializable]
    public class HandState
    {
        public HandPose currentPose;
        public GestureType currentGesture;
        public float gestureConfidence;
        public bool isTracking;
    }
    
    /// <summary>
    /// 手部类型
    /// </summary>
    public enum HandType
    {
        Left,
        Right
    }
    
    /// <summary>
    /// 手指类型
    /// </summary>
    public enum FingerType
    {
        Thumb,
        Index,
        Middle,
        Ring,
        Pinky
    }
    
    /// <summary>
    /// 手指状态
    /// </summary>
    public enum FingerState
    {
        Extended,   // 伸展
        Curled,     // 弯曲
        Neutral     // 中性
    }
}