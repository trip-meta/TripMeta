using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;

namespace TripMeta.VR.WebXR
{
    /// <summary>
    /// WebXR 输入处理器
    /// 处理手部追踪、手柄输入、触摸输入
    /// </summary>
    public class WebXRInputHandler : MonoBehaviour
    {
        [Header("输入配置")]
        public bool enableHandTracking = true;
        public bool enableGamepadInput = true;
        public bool enableTouchInput = true;

        [Header("手势检测")]
        public float pinchThreshold = 0.02f;
        public float grabThreshold = 0.8f;
        public float gestureCooldown = 0.1f;

        // 手部数据
        private WebXRHandData leftHandData = new WebXRHandData(false, 0);
        private WebXRHandData rightHandData = new WebXRHandData(false, 1);

        // 输入设备
        private List<InputDevice> inputDevices = new List<InputDevice>();
        private Dictionary<string, InputControl> controlCache = new Dictionary<string, InputControl>();

        // 事件
        public event Action<WebXRHandData> OnHandDataReceived;
        public event Action<WebXRHandData> OnHandPoseChanged;
        public event Action<WebXRHandData, float> OnPinchDetected;
        public event Action<WebXRHandData, float> OnGrabDetected;

        // 触摸输入
        private TouchControl[] touchControls;
        private Vector2 lastTouchPosition;
        private float lastTouchTime;

        public void Initialize()
        {
            Debug.Log("[WebXRInputHandler] 初始化输入处理器");

            // 监听输入设备变化
            InputSystem.onDeviceChange += OnDeviceChange;
            RefreshDeviceList();
        }

        void Update()
        {
            // 更新手部追踪数据
            if (enableHandTracking)
            {
                UpdateHandTracking();
            }

            // 更新手柄输入
            if (enableGamepadInput)
            {
                UpdateGamepadInput();
            }

            // 更新触摸输入
            if (enableTouchInput)
            {
                UpdateTouchInput();
            }
        }

        /// <summary>
        /// 刷新设备列表
        /// </summary>
        private void RefreshDeviceList()
        {
            inputDevices.Clear();
            controlCache.Clear();

            var devices = InputSystem.devices;
            foreach (var device in devices)
            {
                if (device is XRController || device is TrackedDevice)
                {
                    inputDevices.Add(device);
                    CacheDeviceControls(device);
                    Debug.Log($"[WebXRInputHandler] 发现输入设备: {device.name}");
                }
            }
        }

        /// <summary>
        /// 缓存设备控制
        /// </summary>
        private void CacheDeviceControls(InputDevice device)
        {
            foreach (var control in device.allControls)
            {
                string key = $"{device.name}.{control.name}";
                controlCache[key] = control;
            }
        }

        /// <summary>
        /// 设备变化回调
        /// </summary>
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added)
            {
                if (device is XRController || device is TrackedDevice)
                {
                    inputDevices.Add(device);
                    CacheDeviceControls(device);
                    Debug.Log($"[WebXRInputHandler] 设备添加: {device.name}");
                }
            }
            else if (change == InputDeviceChange.Removed)
            {
                inputDevices.Remove(device);
                Debug.Log($"[WebXRInputHandler] 设备移除: {device.name}");
            }
        }

        /// <summary>
        /// 更新手部追踪
        /// </summary>
        private void UpdateHandTracking()
        {
            // 模拟或获取真实手部数据
            UpdateLeftHand();
            UpdateRightHand();

            // 触发事件
            if (leftHandData.isTracked)
            {
                OnHandDataReceived?.Invoke(leftHandData);
            }
            if (rightHandData.isTracked)
            {
                OnHandDataReceived?.Invoke(rightHandData);
            }
        }

        /// <summary>
        /// 更新左手数据
        /// </summary>
        private void UpdateLeftHand()
        {
            // 从输入设备获取左手数据
            var leftController = GetLeftController();
            if (leftController != null)
            {
                leftHandData.isTracked = true;

                // 获取位置和旋转
                if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 position))
                {
                    leftHandData.jointPositions[0] = position;
                }
                if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotation))
                {
                    leftHandData.jointRotations[0] = rotation;
                }

                // 检测手势
                DetectHandGestures(ref leftHandData);
            }
            else
            {
                // 编辑器模拟模式
                SimulateHandData(ref leftHandData, KeyCode.Q, KeyCode.W);
            }
        }

        /// <summary>
        /// 更新右手数据
        /// </summary>
        private void UpdateRightHand()
        {
            var rightController = GetRightController();
            if (rightController != null)
            {
                rightHandData.isTracked = true;

                if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 position))
                {
                    rightHandData.jointPositions[0] = position;
                }
                if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotation))
                {
                    rightHandData.jointRotations[0] = rotation;
                }

                DetectHandGestures(ref rightHandData);
            }
            else
            {
                SimulateHandData(ref rightHandData, KeyCode.O, KeyCode.P);
            }
        }

        /// <summary>
        /// 获取左手控制器
        /// </summary>
        private InputDevice GetLeftController()
        {
            foreach (var device in inputDevices)
            {
                if (device is XRController controller)
                {
                    var node = controller.GetType().GetProperty("node")?.GetValue(controller);
                    if (node?.ToString() == "LeftHand" || device.name.ToLower().Contains("left"))
                    {
                        return device;
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// 获取右手控制器
        /// </summary>
        private InputDevice GetRightController()
        {
            foreach (var device in inputDevices)
            {
                if (device is XRController controller)
                {
                    var node = controller.GetType().GetProperty("node")?.GetValue(controller);
                    if (node?.ToString() == "RightHand" || device.name.ToLower().Contains("right"))
                    {
                        return device;
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// 模拟手部数据（编辑器模式）
        /// </summary>
        private void SimulateHandData(ref WebXRHandData handData, KeyCode pinchKey, KeyCode grabKey)
        {
            if (Camera.main == null) return;

            // 使用鼠标位置模拟手部
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0.5f + handData.handIndex * 0.1f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            handData.isTracked = true;
            handData.jointPositions[0] = worldPos;
            handData.jointRotations[0] = Quaternion.identity;

            // 模拟捏合和抓取
            bool isPinching = Input.GetKey(pinchKey);
            bool isGrabbing = Input.GetKey(grabKey);

            handData.isPinching = isPinching;
            handData.isGrabbing = isGrabbing;
            handData.pinchValue = isPinching ? 1f : 0f;
            handData.grabValue = isGrabbing ? 1f : 0f;

            // 生成手指关节位置
            GenerateFingerJoints(ref handData);
        }

        /// <summary>
        /// 生成手指关节位置
        /// </summary>
        private void GenerateFingerJoints(ref WebXRHandData handData)
        {
            Vector3 wristPos = handData.jointPositions[0];

            for (int finger = 0; finger < 5; finger++)
            {
                int baseIndex = finger * 4 + 1;
                float fingerSpread = (finger - 2) * 0.03f;

                for (int joint = 0; joint < 4; joint++)
                {
                    int index = baseIndex + joint;
                    if (index >= handData.jointPositions.Length) break;

                    float extension = handData.isGrabbing ? 0.02f : 0.05f + joint * 0.02f;
                    handData.jointPositions[index] = wristPos + new Vector3(
                        fingerSpread * (joint + 1),
                        extension,
                        0.05f * (joint + 1)
                    );
                }
            }
        }

        /// <summary>
        /// 检测手部手势
        /// </summary>
        private void DetectHandGestures(ref WebXRHandData handData)
        {
            // 检测捏合
            float pinchDistance = CalculatePinchDistance(handData);
            handData.pinchValue = Mathf.Clamp01(1f - pinchDistance / pinchThreshold);
            handData.isPinching = handData.pinchValue > 0.5f;

            if (handData.isPinching)
            {
                OnPinchDetected?.Invoke(handData, handData.pinchValue);
            }

            // 检测抓取
            float grabStrength = CalculateGrabStrength(handData);
            handData.grabValue = grabStrength;
            handData.isGrabbing = grabStrength > grabThreshold;

            if (handData.isGrabbing)
            {
                OnGrabDetected?.Invoke(handData, grabStrength);
            }
        }

        /// <summary>
        /// 计算捏合距离
        /// </summary>
        private float CalculatePinchDistance(WebXRHandData handData)
        {
            // 拇指指尖 (index 4) 和食指指尖 (index 8) 之间的距离
            if (handData.jointPositions.Length < 9) return float.MaxValue;

            Vector3 thumbTip = handData.jointPositions[4];
            Vector3 indexTip = handData.jointPositions[8];
            return Vector3.Distance(thumbTip, indexTip);
        }

        /// <summary>
        /// 计算抓取强度
        /// </summary>
        private float CalculateGrabStrength(WebXRHandData handData)
        {
            // 基于所有手指的弯曲程度计算
            float totalBend = 0f;
            int fingerCount = 0;

            for (int finger = 1; finger < 5; finger++)
            {
                int baseIndex = finger * 4 + 1;
                if (baseIndex + 3 >= handData.jointPositions.Length) continue;

                Vector3 mcp = handData.jointPositions[baseIndex];
                Vector3 pip = handData.jointPositions[baseIndex + 1];
                Vector3 dip = handData.jointPositions[baseIndex + 2];
                Vector3 tip = handData.jointPositions[baseIndex + 3];

                float bendAngle = Vector3.Angle(pip - mcp, tip - dip);
                totalBend += bendAngle / 180f;
                fingerCount++;
            }

            return fingerCount > 0 ? totalBend / fingerCount : 0f;
        }

        /// <summary>
        /// 更新手柄输入
        /// </summary>
        private void UpdateGamepadInput()
        {
            // 通过 Unity Input System 处理手柄输入
            foreach (var device in inputDevices)
            {
                if (device is Gamepad gamepad)
                {
                    // 处理游戏手柄输入
                    ProcessGamepadInput(gamepad);
                }
            }
        }

        /// <summary>
        /// 处理游戏手柄输入
        /// </summary>
        private void ProcessGamepadInput(Gamepad gamepad)
        {
            // 按钮状态处理
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                Debug.Log("[WebXRInputHandler] 手柄按钮按下: South");
            }
        }

        /// <summary>
        /// 更新触摸输入
        /// </summary>
        private void UpdateTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPosition = touch.position;
                    lastTouchTime = Time.time;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 delta = touch.position - lastTouchPosition;
                    float duration = Time.time - lastTouchTime;

                    // 检测滑动手势
                    if (delta.magnitude > 50f && duration < 0.5f)
                    {
                        Debug.Log($"[WebXRInputHandler] 触摸滑动: {delta}");
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前手部数据
        /// </summary>
        public WebXRHandData GetCurrentHandData(int handIndex = 0)
        {
            return handIndex == 0 ? leftHandData : rightHandData;
        }

        void OnDestroy()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }
    }
}
