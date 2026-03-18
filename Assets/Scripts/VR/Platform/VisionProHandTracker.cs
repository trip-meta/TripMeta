using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.VR.Platform
{
    /// <summary>
    /// Vision Pro 手势追踪器
    /// 处理手部骨骼追踪和手势识别
    /// </summary>
    public class VisionProHandTracker : MonoBehaviour
    {
        [Header("追踪配置")]
        public float gestureThreshold = 0.8f;
        public float pinchThreshold = 0.02f;
        public int gestureHistorySize = 5;
        public float gestureCooldown = 0.2f;

        [Header("手势灵敏度")]
        public float swipeThreshold = 0.05f;
        public float swipeVelocityThreshold = 0.3f;
        public float rotationThreshold = 15f;
        public float zoomThreshold = 0.05f;

        // 当前手部数据
        private VisionProHandData currentHandData = new VisionProHandData(false);
        private Queue<VisionProHandData> handDataHistory = new Queue<VisionProHandData>();
        private Dictionary<VisionProGestureType, float> gestureConfidences = new Dictionary<VisionProGestureType, float>();

        // 手势检测状态
        private VisionProGestureType lastGesture = VisionProGestureType.None;
        private float lastGestureTime = 0f;
        private bool isTracking = false;
        private bool isInitialized = false;

        // 滑动检测
        private Vector3 lastHandPosition;
        private Vector3 handVelocity;
        private float velocitySampleTime;

        public event Action<VisionProHandData> OnHandDataUpdated;
        public event Action<VisionProGestureType, float> OnGestureDetected;
        public event Action<float> OnPinchStrengthChanged;

        public async Task InitializeAsync()
        {
            if (isInitialized) return;

            // 初始化手势置信度字典
            foreach (VisionProGestureType gesture in Enum.GetValues(typeof(VisionProGestureType)))
            {
                gestureConfidences[gesture] = 0f;
            }

            await Task.Delay(100); // 模拟初始化延迟
            isInitialized = true;

            Debug.Log("[VisionProHandTracker] 手势追踪器初始化完成");
        }

        public void StartTracking()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[VisionProHandTracker] 追踪器未初始化");
                return;
            }

            isTracking = true;
            lastHandPosition = Vector3.zero;
            Debug.Log("[VisionProHandTracker] 手势追踪已启动");
        }

        public void StopTracking()
        {
            isTracking = false;
            currentHandData = new VisionProHandData(false);
            handDataHistory.Clear();
            Debug.Log("[VisionProHandTracker] 手势追踪已停止");
        }

        void Update()
        {
            if (!isTracking) return;

            // 模拟或获取真实手部数据
            UpdateHandData();

            // 检测手势
            DetectGestures();

            // 更新历史
            UpdateHistory();

            // 触发事件
            OnHandDataUpdated?.Invoke(currentHandData);
        }

        /// <summary>
        /// 更新手部数据
        /// </summary>
        private void UpdateHandData()
        {
            #if UNITY_VISIONOS
            // 在真实 Vision Pro 设备上获取手部追踪数据
            UpdateHandDataFromVisionOS();
            #else
            // 编辑器模拟模式
            UpdateHandDataSimulation();
            #endif
        }

        /// <summary>
        /// 从 VisionOS 获取手部数据
        /// </summary>
        private void UpdateHandDataFromVisionOS()
        {
            // 实际实现需要使用 Unity VisionOS XR 插件
            // 这里预留接口
        }

        /// <summary>
        /// 模拟手部数据（编辑器模式）
        /// </summary>
        private void UpdateHandDataSimulation()
        {
            // 模拟手部追踪
            if (Application.isEditor)
            {
                // 使用鼠标位置模拟手部
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 0.5f;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

                // 计算速度
                float deltaTime = Time.time - velocitySampleTime;
                if (deltaTime > 0.016f)
                {
                    handVelocity = (worldPos - lastHandPosition) / deltaTime;
                    lastHandPosition = worldPos;
                    velocitySampleTime = Time.time;
                }

                // 检测捏合（鼠标左键）
                bool isPinching = Input.GetMouseButton(0);
                float pinchStrength = isPinching ? 1f : 0f;

                // 更新手部数据
                currentHandData = new VisionProHandData(true)
                {
                    handPosition = worldPos,
                    handRotation = Quaternion.identity,
                    fingerPositions = GenerateFingerPositions(worldPos, isPinching),
                    isPinching = isPinching,
                    pinchStrength = pinchStrength,
                    palmNormal = Vector3.up,
                    timestamp = Time.time
                };
            }
        }

        /// <summary>
        /// 生成模拟手指位置
        /// </summary>
        private Vector3[] GenerateFingerPositions(Vector3 handPos, bool isPinching)
        {
            Vector3[] fingers = new Vector3[5];
            float spread = isPinching ? 0.01f : 0.03f;

            for (int i = 0; i < 5; i++)
            {
                float offset = (i - 2) * spread;
                fingers[i] = handPos + new Vector3(offset, 0.05f, 0);
            }

            return fingers;
        }

        /// <summary>
        /// 检测手势
        /// </summary>
        private void DetectGestures()
        {
            if (!currentHandData.isTracked) return;

            // 检测捏合
            DetectPinchGesture();

            // 检测滑动
            DetectSwipeGesture();

            // 检测旋转和缩放（需要双手，简化实现）
            DetectRotationAndZoom();

            // 选择最佳手势
            VisionProGestureType bestGesture = SelectBestGesture();

            // 检查冷却时间
            if (Time.time - lastGestureTime < gestureCooldown)
            {
                return;
            }

            // 触发手势事件
            if (bestGesture != VisionProGestureType.None && bestGesture != lastGesture)
            {
                float confidence = gestureConfidences[bestGesture];
                if (confidence >= gestureThreshold)
                {
                    lastGesture = bestGesture;
                    lastGestureTime = Time.time;
                    currentHandData.currentGesture = bestGesture;
                    currentHandData.gestureConfidence = confidence;

                    OnGestureDetected?.Invoke(bestGesture, confidence);
                }
            }
            else if (bestGesture == VisionProGestureType.None)
            {
                lastGesture = VisionProGestureType.None;
                currentHandData.currentGesture = VisionProGestureType.None;
                currentHandData.gestureConfidence = 0f;
            }

            // 触发捏合强度变化
            OnPinchStrengthChanged?.Invoke(currentHandData.pinchStrength);
        }

        /// <summary>
        /// 检测捏合手势
        /// </summary>
        private void DetectPinchGesture()
        {
            if (currentHandData.isPinching)
            {
                gestureConfidences[VisionProGestureType.Pinch] = currentHandData.pinchStrength;

                // 检测双击
                if (Time.time - lastGestureTime < 0.3f && lastGesture == VisionProGestureType.Pinch)
                {
                    gestureConfidences[VisionProGestureType.DoublePinch] = 0.9f;
                }

                // 检测抓取
                if (currentHandData.pinchStrength > 0.8f)
                {
                    gestureConfidences[VisionProGestureType.Grab] = currentHandData.pinchStrength;
                }
            }
            else
            {
                gestureConfidences[VisionProGestureType.Pinch] = 0f;
                gestureConfidences[VisionProGestureType.DoublePinch] = 0f;

                // 检测释放
                if (lastGesture == VisionProGestureType.Grab || lastGesture == VisionProGestureType.Pinch)
                {
                    gestureConfidences[VisionProGestureType.Release] = 0.8f;
                }
            }
        }

        /// <summary>
        /// 检测滑动手势
        /// </summary>
        private void DetectSwipeGesture()
        {
            if (handVelocity.magnitude < swipeVelocityThreshold)
            {
                gestureConfidences[VisionProGestureType.SwipeLeft] = 0f;
                gestureConfidences[VisionProGestureType.SwipeRight] = 0f;
                gestureConfidences[VisionProGestureType.SwipeUp] = 0f;
                gestureConfidences[VisionProGestureType.SwipeDown] = 0f;
                return;
            }

            Vector3 velocityNormalized = handVelocity.normalized;

            // 水平滑动
            if (Mathf.Abs(velocityNormalized.x) > Mathf.Abs(velocityNormalized.y))
            {
                if (velocityNormalized.x < -swipeThreshold)
                {
                    gestureConfidences[VisionProGestureType.SwipeLeft] = Mathf.Min(1f, handVelocity.magnitude);
                }
                else if (velocityNormalized.x > swipeThreshold)
                {
                    gestureConfidences[VisionProGestureType.SwipeRight] = Mathf.Min(1f, handVelocity.magnitude);
                }
            }
            // 垂直滑动
            else
            {
                if (velocityNormalized.y > swipeThreshold)
                {
                    gestureConfidences[VisionProGestureType.SwipeUp] = Mathf.Min(1f, handVelocity.magnitude);
                }
                else if (velocityNormalized.y < -swipeThreshold)
                {
                    gestureConfidences[VisionProGestureType.SwipeDown] = Mathf.Min(1f, handVelocity.magnitude);
                }
            }
        }

        /// <summary>
        /// 检测旋转和缩放手势
        /// </summary>
        private void DetectRotationAndZoom()
        {
            // 简化实现：基于手部旋转速度
            // 实际实现需要双手追踪数据
            gestureConfidences[VisionProGestureType.Rotate] = 0f;
            gestureConfidences[VisionProGestureType.Zoom] = 0f;
        }

        /// <summary>
        /// 选择最佳手势
        /// </summary>
        private VisionProGestureType SelectBestGesture()
        {
            VisionProGestureType bestGesture = VisionProGestureType.None;
            float bestConfidence = 0f;

            foreach (var kvp in gestureConfidences)
            {
                if (kvp.Value > bestConfidence)
                {
                    bestConfidence = kvp.Value;
                    bestGesture = kvp.Key;
                }
            }

            return bestGesture;
        }

        /// <summary>
        /// 更新历史数据
        /// </summary>
        private void UpdateHistory()
        {
            handDataHistory.Enqueue(currentHandData);

            while (handDataHistory.Count > gestureHistorySize)
            {
                handDataHistory.Dequeue();
            }
        }

        /// <summary>
        /// 获取当前手部数据
        /// </summary>
        public VisionProHandData GetCurrentHandData()
        {
            return currentHandData;
        }

        /// <summary>
        /// 检查是否支持特定手势
        /// </summary>
        public bool IsGestureSupported(VisionProGestureType gestureType)
        {
            return gestureConfidences.ContainsKey(gestureType);
        }

        /// <summary>
        /// 获取手势历史
        /// </summary>
        public VisionProHandData[] GetHandDataHistory()
        {
            return handDataHistory.ToArray();
        }
    }
}
