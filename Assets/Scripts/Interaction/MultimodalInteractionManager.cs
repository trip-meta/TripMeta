using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.Interaction
{
    /// <summary>
    /// 多模态交互管理器 - 手势识别、视线追踪、语音合成优化
    /// 目标：提供更自然、直观的VR交互体验
    /// </summary>
    public class MultimodalInteractionManager : MonoBehaviour
    {
        [Header("手势识别")]
        public bool enableGestureRecognition = true;
        public float gestureRecognitionInterval = 0.1f; // 100ms
        public float gestureConfidenceThreshold = 0.8f;
        public XRNode dominantHand = XRNode.RightHand;

        [Header("视线追踪")]
        public bool enableEyeTracking = true;
        public float eyeTrackingInterval = 0.05f; // 50ms
        public float gazeDwellTime = 1.5f; // 凝视触发时间
        public float gazeRadius = 2f; // 凝视区域半径

        [Header("语音合成")]
        public bool enableVoiceSynthesis = true;
        public float speechSpeed = 1.0f;
        public float speechVolume = 0.8f;
        public string defaultVoice = "zh-CN-XiaoxiaoNeural";

        [Header("多模态融合")]
        public bool enableMultimodalFusion = true;
        public float fusionConfidenceThreshold = 0.7f;

        [Header("性能优化")]
        public int maxConcurrentGestures = 5;
        public int gazeBufferSize = 10;
        public bool useObjectPooling = true;

        // 手势追踪
        private Dictionary<GestureType, GestureData> gestureDatabase = new Dictionary<GestureType, GestureData>();
        private Queue<RecognizedGesture> gestureHistory = new Queue<RecognizedGesture>();
        private GestureType currentGesture = GestureType.None;

        // 视线追踪
        private Queue<Vector3> gazePositionBuffer = new Queue<Vector3>();
        private Vector3 smoothedGazePosition;
        private float currentGazeDwellTime = 0f;
        private GameObject currentGazeTarget;

        // 语音合成
        private Queue<SpeechRequest> speechQueue = new Queue<SpeechRequest>();
        private bool isSpeaking = false;

        // 服务引用
        private AIServiceManager aiServiceManager;
        private AzureSpeechService speechService;

        // 状态
        private bool isInitialized = false;

        public static MultimodalInteractionManager Instance { get; private set; }

        // 事件
        public event Action<RecognizedGesture> OnGestureRecognized;
        public event Action<Vector3, GameObject> OnGazeChanged;
        public event Action<GameObject> OnGazeDwell;
        public event Action<string> OnSpeechStarted;
        public event Action<string> OnSpeechCompleted;
        public event Action<MultimodalInput> OnMultimodalInput;

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
            Logger.LogInfo("初始化多模态交互管理器...", "MultimodalInteraction");

            InitializeGestureDatabase();

            isInitialized = true;
            Logger.LogInfo("多模态交互管理器初始化完成", "MultimodalInteraction");
        }

        async void Start()
        {
            await InitializeServices();
        }

        /// <summary>
        /// 初始化手势数据库
        /// </summary>
        private void InitializeGestureDatabase()
        {
            gestureDatabase[GestureType.Point] = new GestureData
            {
                gestureType = GestureType.Point,
                description = "食指指向",
                confidenceThreshold = 0.8f,
                handPose = HandPose.Open,
                fingerExtension = new bool[] { false, true, false, false, false }
            };

            gestureDatabase[GestureType.Grab] = new GestureData
            {
                gestureType = GestureType.Grab,
                description = "握拳抓取",
                confidenceThreshold = 0.85f,
                handPose = HandPose.Fist,
                fingerExtension = new bool[] { false, false, false, false, false }
            };

            gestureDatabase[GestureType.OpenPalm] = new GestureData
            {
                gestureType = GestureType.OpenPalm,
                description = "张开手掌",
                confidenceThreshold = 0.8f,
                handPose = HandPose.Open,
                fingerExtension = new bool[] { true, true, true, true, true }
            };

            gestureDatabase[GestureType.ThumbsUp] = new GestureData
            {
                gestureType = GestureType.ThumbsUp,
                description = "竖起大拇指",
                confidenceThreshold = 0.85f,
                handPose = HandPose.Open,
                fingerExtension = new bool[] { true, false, false, false, false }
            };

            gestureDatabase[GestureType.Pinch] = new GestureData
            {
                gestureType = GestureType.Pinch,
                description = "拇指食指捏合",
                confidenceThreshold = 0.8f,
                handPose = HandPose.Pinch,
                fingerExtension = new bool[] { true, true, false, false, false }
            };

            Logger.LogInfo($"手势数据库初始化完成: {gestureDatabase.Count} 种手势", "MultimodalInteraction");
        }

        /// <summary>
        /// 初始化服务引用
        /// </summary>
        private async Task InitializeServices()
        {
            await Task.Delay(1000);
            aiServiceManager = AIServiceManager.Instance;
            Logger.LogInfo("多模态交互服务连接完成", "MultimodalInteraction");
        }

        void Update()
        {
            if (!isInitialized) return;

            // 手势识别
            if (enableGestureRecognition)
            {
                ProcessGestureRecognition();
            }

            // 视线追踪
            if (enableEyeTracking)
            {
                ProcessEyeTracking();
            }

            // 语音合成队列处理
            if (enableVoiceSynthesis && speechQueue.Count > 0 && !isSpeaking)
            {
                _ = ProcessSpeechQueue();
            }
        }

        #region 手势识别

        /// <summary>
        /// 处理手势识别
        /// </summary>
        private void ProcessGestureRecognition()
        {
            // 获取手部数据
            var handData = GetHandTrackingData();
            if (handData == null) return;

            // 识别手势
            var recognizedGesture = RecognizeGesture(handData);

            if (recognizedGesture != null && recognizedGesture.confidence >= gestureConfidenceThreshold)
            {
                if (recognizedGesture.gestureType != currentGesture)
                {
                    currentGesture = recognizedGesture.gestureType;

                    // 添加到历史
                    AddGestureToHistory(recognizedGesture);

                    // 触发事件
                    OnGestureRecognized?.Invoke(recognizedGesture);

                    // 多模态融合
                    if (enableMultimodalFusion)
                    {
                        ProcessMultimodalInput(new MultimodalInput
                        {
                            gesture = recognizedGesture,
                            gazePosition = smoothedGazePosition,
                            gazeTarget = currentGazeTarget,
                            timestamp = DateTime.Now
                        });
                    }

                    Logger.LogInfo($"手势识别: {recognizedGesture.gestureType} (置信度: {recognizedGesture.confidence:P1})", "MultimodalInteraction");
                }
            }
        }

        /// <summary>
        /// 获取手部追踪数据
        /// </summary>
        private HandTrackingData GetHandTrackingData()
        {
            // 从XR Interaction Toolkit获取手部数据
            // 简化实现，实际需要集成手部追踪SDK
            return new HandTrackingData
            {
                handPosition = GetHandPosition(dominantHand),
                handRotation = GetHandRotation(dominantHand),
                fingerPositions = GetFingerPositions(dominantHand),
                isTracking = true
            };
        }

        private Vector3 GetHandPosition(XRNode handNode)
        {
            InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
            return position;
        }

        private Quaternion GetHandRotation(XRNode handNode)
        {
            InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);
            return rotation;
        }

        private Vector3[] GetFingerPositions(XRNode handNode)
        {
            // 简化实现，实际需要获取每个手指的位置
            return new Vector3[5];
        }

        /// <summary>
        /// 识别手势
        /// </summary>
        private RecognizedGesture RecognizeGesture(HandTrackingData handData)
        {
            GestureType bestMatch = GestureType.None;
            float bestConfidence = 0f;

            foreach (var gesture in gestureDatabase.Values)
            {
                float confidence = CalculateGestureConfidence(handData, gesture);
                if (confidence > bestConfidence && confidence >= gesture.confidenceThreshold)
                {
                    bestConfidence = confidence;
                    bestMatch = gesture.gestureType;
                }
            }

            if (bestMatch != GestureType.None)
            {
                return new RecognizedGesture
                {
                    gestureType = bestMatch,
                    confidence = bestConfidence,
                    handPosition = handData.handPosition,
                    handRotation = handData.handRotation,
                    timestamp = DateTime.Now
                };
            }

            return null;
        }

        /// <summary>
        /// 计算手势置信度
        /// </summary>
        private float CalculateGestureConfidence(HandTrackingData handData, GestureData gesture)
        {
            // 简化实现，实际需要复杂的手势匹配算法
            float confidence = 0.5f;

            // 基于手指伸展程度计算置信度
            if (handData.fingerPositions != null && handData.fingerPositions.Length >= 5)
            {
                int matchCount = 0;
                for (int i = 0; i < 5; i++)
                {
                    bool isExtended = Vector3.Distance(handData.handPosition, handData.fingerPositions[i]) > 0.05f;
                    if (isExtended == gesture.fingerExtension[i])
                    {
                        matchCount++;
                    }
                }
                confidence = (float)matchCount / 5f;
            }

            return confidence;
        }

        /// <summary>
        /// 添加手势到历史
        /// </summary>
        private void AddGestureToHistory(RecognizedGesture gesture)
        {
            gestureHistory.Enqueue(gesture);

            while (gestureHistory.Count > maxConcurrentGestures)
            {
                gestureHistory.Dequeue();
            }
        }

        #endregion

        #region 视线追踪

        /// <summary>
        /// 处理视线追踪
        /// </summary>
        private void ProcessEyeTracking()
        {
            // 获取凝视点
            Vector3 gazePosition = GetGazePosition();

            // 平滑处理
            gazePositionBuffer.Enqueue(gazePosition);
            if (gazePositionBuffer.Count > gazeBufferSize)
            {
                gazePositionBuffer.Dequeue();
            }

            smoothedGazePosition = CalculateSmoothedGazePosition();

            // 射线检测
            GameObject gazeTarget = GetGazeTarget(smoothedGazePosition);

            if (gazeTarget != currentGazeTarget)
            {
                currentGazeDwellTime = 0f;
                currentGazeTarget = gazeTarget;
                OnGazeChanged?.Invoke(smoothedGazePosition, gazeTarget);
            }
            else if (gazeTarget != null)
            {
                currentGazeDwellTime += Time.deltaTime;

                if (currentGazeDwellTime >= gazeDwellTime)
                {
                    OnGazeDwell?.Invoke(gazeTarget);
                    currentGazeDwellTime = 0f;

                    Logger.LogInfo($"凝视触发: {gazeTarget.name}", "MultimodalInteraction");
                }
            }
        }

        /// <summary>
        /// 获取凝视位置
        /// </summary>
        private Vector3 GetGazePosition()
        {
            // 从头显中心发射射线
            if (Camera.main != null)
            {
                Ray gazeRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                if (Physics.Raycast(gazeRay, out RaycastHit hit, 100f))
                {
                    return hit.point;
                }
                return gazeRay.GetPoint(10f);
            }
            return Vector3.zero;
        }

        /// <summary>
        /// 计算平滑凝视位置
        /// </summary>
        private Vector3 CalculateSmoothedGazePosition()
        {
            Vector3 sum = Vector3.zero;
            foreach (var pos in gazePositionBuffer)
            {
                sum += pos;
            }
            return sum / gazePositionBuffer.Count;
        }

        /// <summary>
        /// 获取凝视目标
        /// </summary>
        private GameObject GetGazeTarget(Vector3 gazePosition)
        {
            Collider[] hits = Physics.OverlapSphere(gazePosition, gazeRadius);
            GameObject nearestTarget = null;
            float nearestDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Interactable"))
                {
                    float distance = Vector3.Distance(gazePosition, hit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestTarget = hit.gameObject;
                    }
                }
            }

            return nearestTarget;
        }

        #endregion

        #region 语音合成

        /// <summary>
        /// 请求语音合成
        /// </summary>
        public void RequestSpeech(string text, SpeechPriority priority = SpeechPriority.Normal)
        {
            if (!enableVoiceSynthesis) return;

            speechQueue.Enqueue(new SpeechRequest
            {
                text = text,
                priority = priority,
                voice = defaultVoice,
                speed = speechSpeed,
                volume = speechVolume,
                timestamp = DateTime.Now
            });

            Logger.LogInfo($"语音请求已排队: {text.Substring(0, Math.Min(30, text.Length))}...", "MultimodalInteraction");
        }

        /// <summary>
        /// 处理语音队列
        /// </summary>
        private async Task ProcessSpeechQueue()
        {
            if (speechQueue.Count == 0 || isSpeaking) return;

            isSpeaking = true;
            var request = speechQueue.Dequeue();

            try
            {
                OnSpeechStarted?.Invoke(request.text);

                // 调用Azure语音服务
                if (speechService != null)
                {
                    await speechService.SynthesizeSpeechAsync(request.text, request.voice);
                }
                else
                {
                    // 模拟语音合成
                    float speechDuration = request.text.Length * 0.1f / speechSpeed;
                    await Task.Delay((int)(speechDuration * 1000));
                }

                OnSpeechCompleted?.Invoke(request.text);

                Logger.LogInfo("语音合成完成", "MultimodalInteraction");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "语音合成失败");
            }
            finally
            {
                isSpeaking = false;
            }
        }

        /// <summary>
        /// 停止语音
        /// </summary>
        public void StopSpeech()
        {
            speechQueue.Clear();
            isSpeaking = false;
            Logger.LogInfo("语音已停止", "MultimodalInteraction");
        }

        #endregion

        #region 多模态融合

        /// <summary>
        /// 处理多模态输入
        /// </summary>
        private void ProcessMultimodalInput(MultimodalInput input)
        {
            // 计算融合置信度
            float fusionConfidence = CalculateFusionConfidence(input);

            if (fusionConfidence >= fusionConfidenceThreshold)
            {
                OnMultimodalInput?.Invoke(input);

                Logger.LogInfo($"多模态输入: 手势={input.gesture?.gestureType}, 凝视={input.gazeTarget?.name}, 置信度={fusionConfidence:P1}", "MultimodalInteraction");
            }
        }

        /// <summary>
        /// 计算融合置信度
        /// </summary>
        private float CalculateFusionConfidence(MultimodalInput input)
        {
            float confidence = 0f;
            int factorCount = 0;

            if (input.gesture != null)
            {
                confidence += input.gesture.confidence * 0.4f; // 手势权重40%
                factorCount++;
            }

            if (input.gazeTarget != null)
            {
                confidence += 0.3f; // 凝视权重30%
                factorCount++;
            }

            if (factorCount > 0)
            {
                confidence /= factorCount;
            }

            return confidence;
        }

        #endregion

        /// <summary>
        /// 获取当前手势
        /// </summary>
        public GestureType GetCurrentGesture()
        {
            return currentGesture;
        }

        /// <summary>
        /// 获取凝视位置
        /// </summary>
        public Vector3 GetCurrentGazePosition()
        {
            return smoothedGazePosition;
        }

        /// <summary>
        /// 获取凝视目标
        /// </summary>
        public GameObject GetCurrentGazeTarget()
        {
            return currentGazeTarget;
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
    /// 手势类型
    /// </summary>
    public enum GestureType
    {
        None,
        Point,          // 指向
        Grab,           // 抓取
        OpenPalm,       // 张开手掌
        ThumbsUp,       // 竖起大拇指
        Pinch,          // 捏合
        Wave,           // 挥手
        SwipeLeft,      // 向左滑动
        SwipeRight,     // 向右滑动
        Circle,         // 画圈
        Heart           // 心形
    }

    /// <summary>
    /// 手部姿态
    /// </summary>
    public enum HandPose
    {
        Open,
        Fist,
        Pinch,
        Point
    }

    /// <summary>
    /// 语音优先级
    /// </summary>
    public enum SpeechPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// 手势数据
    /// </summary>
    [Serializable]
    public class GestureData
    {
        public GestureType gestureType;
        public string description;
        public float confidenceThreshold;
        public HandPose handPose;
        public bool[] fingerExtension; // 5根手指的伸展状态
    }

    /// <summary>
    /// 识别到的手势
    /// </summary>
    public class RecognizedGesture
    {
        public GestureType gestureType;
        public float confidence;
        public Vector3 handPosition;
        public Quaternion handRotation;
        public DateTime timestamp;

        public override string ToString()
        {
            return $"{gestureType} (置信度: {confidence:P1})";
        }
    }

    /// <summary>
    /// 手部追踪数据
    /// </summary>
    public class HandTrackingData
    {
        public Vector3 handPosition;
        public Quaternion handRotation;
        public Vector3[] fingerPositions;
        public bool isTracking;
    }

    /// <summary>
    /// 语音请求
    /// </summary>
    public class SpeechRequest
    {
        public string text;
        public SpeechPriority priority;
        public string voice;
        public float speed;
        public float volume;
        public DateTime timestamp;
    }

    /// <summary>
    /// 多模态输入
    /// </summary>
    public class MultimodalInput
    {
        public RecognizedGesture gesture;
        public Vector3 gazePosition;
        public GameObject gazeTarget;
        public DateTime timestamp;
    }

    #endregion
}
