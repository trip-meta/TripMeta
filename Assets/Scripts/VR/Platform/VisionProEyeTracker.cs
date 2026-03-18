using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.VR.Platform
{
    /// <summary>
    /// Vision Pro 眼动追踪器
    /// 高精度眼动数据采集和注视点分析
    /// </summary>
    public class VisionProEyeTracker : MonoBehaviour
    {
        [Header("追踪配置")]
        public float eyeTrackingInterval = 0.016f; // 60Hz
        public int gazeBufferSize = 10;
        public float fixationThreshold = 1.5f; // 凝视判定时间
        public float saccadeThreshold = 100f; // 扫视速度阈值 (度/秒)

        [Header("平滑配置")]
        public bool enableSmoothing = true;
        public float smoothingFactor = 0.3f;
        public bool enablePrediction = true;
        public float predictionTime = 0.05f;

        [Header("数据质量")]
        public float minConfidence = 0.7f;
        public float blinkThreshold = 0.1f;

        // 当前眼动数据
        private VisionProEyeData currentEyeData = new VisionProEyeData(false);
        private Queue<VisionProEyeData> gazeBuffer = new Queue<VisionProEyeData>();
        private List<VisionProEyeData> fixationHistory = new List<VisionProEyeData>();

        // 状态
        private bool isTracking = false;
        private bool isInitialized = false;
        private float currentFixationStartTime = 0f;
        private Vector3 currentFixationPoint = Vector3.zero;
        private bool isFixating = false;

        // 平滑 gaze 点
        private Vector3 smoothedGazePoint;
        private Vector3 gazeVelocity;

        public event Action<VisionProEyeData> OnEyeDataUpdated;
        public event Action<Vector3, float> OnFixationStarted; // point, duration
        public event Action<Vector3, float> OnFixationEnded;   // point, duration
        public event Action OnBlinkDetected;
        public event Action<Vector3> OnSaccadeDetected;        // target point

        public bool IsFixating => isFixating;
        public Vector3 CurrentFixationPoint => currentFixationPoint;
        public float CurrentFixationDuration => isFixating ? Time.time - currentFixationStartTime : 0f;

        public async Task InitializeAsync()
        {
            if (isInitialized) return;

            await Task.Delay(100);

            smoothedGazePoint = Vector3.zero;
            isInitialized = true;

            Debug.Log("[VisionProEyeTracker] 眼动追踪器初始化完成");
        }

        public void StartTracking()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[VisionProEyeTracker] 追踪器未初始化");
                return;
            }

            isTracking = true;
            Debug.Log("[VisionProEyeTracker] 眼动追踪已启动");
        }

        public void StopTracking()
        {
            isTracking = false;
            currentEyeData = new VisionProEyeData(false);
            gazeBuffer.Clear();
            fixationHistory.Clear();
            isFixating = false;
            Debug.Log("[VisionProEyeTracker] 眼动追踪已停止");
        }

        void Update()
        {
            if (!isTracking) return;

            // 更新眼动数据
            UpdateEyeData();

            // 平滑处理
            if (enableSmoothing)
            {
                ApplySmoothing();
            }

            // 预测
            if (enablePrediction)
            {
                ApplyPrediction();
            }

            // 检测凝视
            DetectFixation();

            // 检测眨眼
            DetectBlink();

            // 检测扫视
            DetectSaccade();

            // 更新缓冲区
            UpdateBuffer();

            // 触发事件
            OnEyeDataUpdated?.Invoke(currentEyeData);
        }

        /// <summary>
        /// 更新眼动数据
        /// </summary>
        private void UpdateEyeData()
        {
            #if UNITY_VISIONOS
            UpdateEyeDataFromVisionOS();
            #else
            UpdateEyeDataSimulation();
            #endif
        }

        /// <summary>
        /// 从 VisionOS 获取眼动数据
        /// </summary>
        private void UpdateEyeDataFromVisionOS()
        {
            // 使用 Unity VisionOS XR 插件获取眼动数据
            // 预留接口
        }

        /// <summary>
        /// 模拟眼动数据
        /// </summary>
        private void UpdateEyeDataSimulation()
        {
            if (Camera.main == null) return;

            // 使用头部朝向模拟 gaze 方向
            Vector3 headPos = Camera.main.transform.position;
            Vector3 headForward = Camera.main.transform.forward;

            // 添加一些噪声模拟真实眼动
            Vector2 noise = new Vector2(
                Mathf.PerlinNoise(Time.time * 2f, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, Time.time * 2f) - 0.5f
            ) * 0.05f;

            Vector3 gazeDir = headForward + Camera.main.transform.right * noise.x +
                                              Camera.main.transform.up * noise.y;
            gazeDir.Normalize();

            // 射线检测 gaze 点
            Vector3 gazePoint;
            if (Physics.Raycast(headPos, gazeDir, out RaycastHit hit, 100f))
            {
                gazePoint = hit.point;
            }
            else
            {
                gazePoint = headPos + gazeDir * 10f;
            }

            // 模拟眼 openness
            float openness = 0.8f + Mathf.PerlinNoise(Time.time * 3f, 0f) * 0.2f;

            currentEyeData = new VisionProEyeData(true)
            {
                gazeOrigin = headPos,
                gazeDirection = gazeDir,
                gazePoint = gazePoint,
                leftEyeOpenness = openness,
                rightEyeOpenness = openness,
                timestamp = Time.time
            };
        }

        /// <summary>
        /// 应用平滑
        /// </summary>
        private void ApplySmoothing()
        {
            if (gazeBuffer.Count == 0)
            {
                smoothedGazePoint = currentEyeData.gazePoint;
                return;
            }

            Vector3 avgPoint = Vector3.zero;
            foreach (var data in gazeBuffer)
            {
                avgPoint += data.gazePoint;
            }
            avgPoint /= gazeBuffer.Count;

            smoothedGazePoint = Vector3.Lerp(smoothedGazePoint, avgPoint, smoothingFactor);
            currentEyeData.gazePoint = smoothedGazePoint;
        }

        /// <summary>
        /// 应用预测
        /// </summary>
        private void ApplyPrediction()
        {
            if (gazeBuffer.Count < 2) return;

            var dataArray = gazeBuffer.ToArray();
            Vector3 velocity = (dataArray[dataArray.Length - 1].gazePoint - dataArray[dataArray.Length - 2].gazePoint) /
                               (dataArray[dataArray.Length - 1].timestamp - dataArray[dataArray.Length - 2].timestamp + 0.0001f);

            Vector3 predictedPoint = currentEyeData.gazePoint + velocity * predictionTime;
            currentEyeData.gazePoint = Vector3.Lerp(currentEyeData.gazePoint, predictedPoint, 0.5f);
        }

        /// <summary>
        /// 检测凝视
        /// </summary>
        private void DetectFixation()
        {
            if (gazeBuffer.Count < gazeBufferSize) return;

            // 计算 gaze 点的方差
            Vector3 avgPoint = Vector3.zero;
            foreach (var data in gazeBuffer)
            {
                avgPoint += data.gazePoint;
            }
            avgPoint /= gazeBuffer.Count;

            float variance = 0f;
            foreach (var data in gazeBuffer)
            {
                variance += Vector3.SqrMagnitude(data.gazePoint - avgPoint);
            }
            variance /= gazeBuffer.Count;

            // 方差小表示凝视
            bool isCurrentlyFixating = variance < 0.001f;

            if (isCurrentlyFixating && !isFixating)
            {
                // 开始凝视
                isFixating = true;
                currentFixationStartTime = Time.time;
                currentFixationPoint = avgPoint;
                OnFixationStarted?.Invoke(currentFixationPoint, 0f);
            }
            else if (isCurrentlyFixating && isFixating)
            {
                // 继续凝视
                currentFixationPoint = Vector3.Lerp(currentFixationPoint, avgPoint, 0.1f);

                // 检查是否达到阈值
                float duration = Time.time - currentFixationStartTime;
                if (duration >= fixationThreshold)
                {
                    currentEyeData.fixationDuration = duration;
                }
            }
            else if (!isCurrentlyFixating && isFixating)
            {
                // 结束凝视
                float duration = Time.time - currentFixationStartTime;
                OnFixationEnded?.Invoke(currentFixationPoint, duration);

                fixationHistory.Add(new VisionProEyeData
                {
                    gazePoint = currentFixationPoint,
                    timestamp = duration
                });

                isFixating = false;
                currentEyeData.fixationDuration = 0f;
            }
        }

        /// <summary>
        /// 检测眨眼
        /// </summary>
        private void DetectBlink()
        {
            if (currentEyeData.leftEyeOpenness < blinkThreshold &&
                currentEyeData.rightEyeOpenness < blinkThreshold)
            {
                OnBlinkDetected?.Invoke();
            }
        }

        /// <summary>
        /// 检测扫视
        /// </summary>
        private void DetectSaccade()
        {
            if (gazeBuffer.Count < 2) return;

            var dataArray = gazeBuffer.ToArray();
            Vector3 delta = dataArray[dataArray.Length - 1].gazePoint - dataArray[dataArray.Length - 2].gazePoint;
            float timeDelta = dataArray[dataArray.Length - 1].timestamp - dataArray[dataArray.Length - 2].timestamp;

            if (timeDelta > 0)
            {
                float angularVelocity = delta.magnitude / timeDelta * Mathf.Rad2Deg;

                if (angularVelocity > saccadeThreshold)
                {
                    OnSaccadeDetected?.Invoke(currentEyeData.gazePoint);
                }
            }
        }

        /// <summary>
        /// 更新缓冲区
        /// </summary>
        private void UpdateBuffer()
        {
            gazeBuffer.Enqueue(currentEyeData);

            while (gazeBuffer.Count > gazeBufferSize)
            {
                gazeBuffer.Dequeue();
            }
        }

        /// <summary>
        /// 获取当前眼动数据
        /// </summary>
        public VisionProEyeData GetCurrentEyeData()
        {
            return currentEyeData;
        }

        /// <summary>
        /// 获取凝视历史
        /// </summary>
        public List<VisionProEyeData> GetFixationHistory()
        {
            return new List<VisionProEyeData>(fixationHistory);
        }

        /// <summary>
        /// 清除凝视历史
        /// </summary>
        public void ClearFixationHistory()
        {
            fixationHistory.Clear();
        }
    }
}
