using UnityEngine;
using Unity.XR.PXR;

namespace TripMeta.Interaction
{
    /// <summary>
    /// VR控制器管理器 - 处理PICO控制器输入和交互
    /// </summary>
    public class VRControllerManager : MonoBehaviour
    {
        [Header("控制器设置")]
        [SerializeField] private bool enableControllerTracking = true;
        [SerializeField] private bool enableHandTracking = false;
        
        [Header("输入设置")]
        [SerializeField] private float triggerThreshold = 0.5f;
        [SerializeField] private float gripThreshold = 0.5f;
        
        // 控制器状态
        private bool leftControllerConnected = false;
        private bool rightControllerConnected = false;
        
        // 输入状态
        private bool leftTriggerPressed = false;
        private bool rightTriggerPressed = false;
        private bool leftGripPressed = false;
        private bool rightGripPressed = false;
        
        // 事件
        public System.Action<bool> OnLeftTriggerChanged;
        public System.Action<bool> OnRightTriggerChanged;
        public System.Action<bool> OnLeftGripChanged;
        public System.Action<bool> OnRightGripChanged;
        
        void Start()
        {
            InitializeControllers();
        }
        
        void Update()
        {
            if (enableControllerTracking)
            {
                UpdateControllerStatus();
                UpdateControllerInput();
            }
        }
        
        /// <summary>
        /// 初始化控制器
        /// </summary>
        private void InitializeControllers()
        {
            try
            {
                // 检查PICO SDK是否可用
                if (PXR_Plugin.System.UPxr_GetAPIVersion() > 0)
                {
                    Debug.Log("PICO SDK initialized successfully");
                    
                    // 启用控制器追踪
                    if (enableControllerTracking)
                    {
                        PXR_Input.SetControllerMainInputFeature(PXR_Input.ControllerDevice.LeftController);
                        Debug.Log("Controller tracking enabled");
                    }
                    
                    // 启用手部追踪（如果支持）
                    if (enableHandTracking)
                    {
                        // 注意：手部追踪需要在PICO设备设置中启用
                        Debug.Log("Hand tracking requested (需要在设备中启用)");
                    }
                }
                else
                {
                    Debug.LogWarning("PICO SDK not available - 运行在编辑器模式");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize controllers: {e.Message}");
            }
        }
        
        /// <summary>
        /// 更新控制器连接状态
        /// </summary>
        private void UpdateControllerStatus()
        {
            try
            {
                // 检查左控制器
                bool leftConnected = PXR_Input.IsControllerConnected(PXR_Input.Controller.LeftController);
                if (leftConnected != leftControllerConnected)
                {
                    leftControllerConnected = leftConnected;
                    Debug.Log($"Left controller {(leftConnected ? "connected" : "disconnected")}");
                }
                
                // 检查右控制器
                bool rightConnected = PXR_Input.IsControllerConnected(PXR_Input.Controller.RightController);
                if (rightConnected != rightControllerConnected)
                {
                    rightControllerConnected = rightConnected;
                    Debug.Log($"Right controller {(rightConnected ? "connected" : "disconnected")}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating controller status: {e.Message}");
            }
        }
        
        /// <summary>
        /// 更新控制器输入
        /// </summary>
        private void UpdateControllerInput()
        {
            try
            {
                // 左控制器输入
                if (leftControllerConnected)
                {
                    // 扳机键
                    float leftTriggerValue = PXR_Input.GetControllerTriggerValue(PXR_Input.Controller.LeftController);
                    bool leftTriggerCurrentlyPressed = leftTriggerValue > triggerThreshold;
                    if (leftTriggerCurrentlyPressed != leftTriggerPressed)
                    {
                        leftTriggerPressed = leftTriggerCurrentlyPressed;
                        OnLeftTriggerChanged?.Invoke(leftTriggerPressed);
                    }
                    
                    // 握持键
                    float leftGripValue = PXR_Input.GetControllerGripValue(PXR_Input.Controller.LeftController);
                    bool leftGripCurrentlyPressed = leftGripValue > gripThreshold;
                    if (leftGripCurrentlyPressed != leftGripPressed)
                    {
                        leftGripPressed = leftGripCurrentlyPressed;
                        OnLeftGripChanged?.Invoke(leftGripPressed);
                    }
                }
                
                // 右控制器输入
                if (rightControllerConnected)
                {
                    // 扳机键
                    float rightTriggerValue = PXR_Input.GetControllerTriggerValue(PXR_Input.Controller.RightController);
                    bool rightTriggerCurrentlyPressed = rightTriggerValue > triggerThreshold;
                    if (rightTriggerCurrentlyPressed != rightTriggerPressed)
                    {
                        rightTriggerPressed = rightTriggerCurrentlyPressed;
                        OnRightTriggerChanged?.Invoke(rightTriggerPressed);
                    }
                    
                    // 握持键
                    float rightGripValue = PXR_Input.GetControllerGripValue(PXR_Input.Controller.RightController);
                    bool rightGripCurrentlyPressed = rightGripValue > gripThreshold;
                    if (rightGripCurrentlyPressed != rightGripPressed)
                    {
                        rightGripPressed = rightGripCurrentlyPressed;
                        OnRightGripChanged?.Invoke(rightGripPressed);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating controller input: {e.Message}");
            }
        }
        
        /// <summary>
        /// 获取控制器位置
        /// </summary>
        public Vector3 GetControllerPosition(PXR_Input.Controller controller)
        {
            try
            {
                return PXR_Input.GetControllerPosition(controller);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting controller position: {e.Message}");
                return Vector3.zero;
            }
        }
        
        /// <summary>
        /// 获取控制器旋转
        /// </summary>
        public Quaternion GetControllerRotation(PXR_Input.Controller controller)
        {
            try
            {
                return PXR_Input.GetControllerRotation(controller);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting controller rotation: {e.Message}");
                return Quaternion.identity;
            }
        }
        
        /// <summary>
        /// 控制器震动
        /// </summary>
        public void VibrateController(PXR_Input.Controller controller, float strength = 0.5f, int time = 100)
        {
            try
            {
                PXR_Input.SendHapticImpulse(controller, strength, time);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error vibrating controller: {e.Message}");
            }
        }
        
        /// <summary>
        /// 检查按钮是否按下
        /// </summary>
        public bool IsButtonPressed(PXR_Input.Controller controller, PXR_Input.ControllerButton button)
        {
            try
            {
                return PXR_Input.GetControllerButtonDown(button, controller);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking button press: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取摇杆输入
        /// </summary>
        public Vector2 GetJoystickInput(PXR_Input.Controller controller)
        {
            try
            {
                return PXR_Input.GetControllerJoyStickValue(controller);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting joystick input: {e.Message}");
                return Vector2.zero;
            }
        }
        
        // 公共属性
        public bool LeftControllerConnected => leftControllerConnected;
        public bool RightControllerConnected => rightControllerConnected;
        public bool LeftTriggerPressed => leftTriggerPressed;
        public bool RightTriggerPressed => rightTriggerPressed;
        public bool LeftGripPressed => leftGripPressed;
        public bool RightGripPressed => rightGripPressed;
    }
}