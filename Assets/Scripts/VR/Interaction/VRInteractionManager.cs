using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TripMeta.Core.DependencyInjection;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.VR.Interaction
{
    /// <summary>
    /// VR交互管理器 - 统一管理VR交互系统
    /// </summary>
    public class VRInteractionManager : MonoBehaviour, IService
    {
        [Header("VR设备配置")]
        [SerializeField] private XRRig xrRig;
        [SerializeField] private Camera vrCamera;
        [SerializeField] private Transform leftController;
        [SerializeField] private Transform rightController;
        
        [Header("交互设置")]
        [SerializeField] private LayerMask interactableLayer = -1;
        [SerializeField] private float raycastDistance = 10f;
        [SerializeField] private float hapticIntensity = 0.5f;
        [SerializeField] private float hapticDuration = 0.1f;
        
        [Header("手势识别")]
        [SerializeField] private bool enableHandTracking = true;
        [SerializeField] private HandTrackingManager handTrackingManager;
        [SerializeField] private GestureRecognizer gestureRecognizer;
        
        [Header("UI交互")]
        [SerializeField] private VRUIManager vrUIManager;
        [SerializeField] private float uiInteractionDistance = 5f;
        [SerializeField] private bool enableGazeInteraction = true;
        
        // 交互状态
        private Dictionary<XRNode, VRControllerState> controllerStates;
        private List<IVRInteractable> currentInteractables;
        private IVRInteractable selectedInteractable;
        private bool isInitialized = false;
        
        // 事件
        public event Action<IVRInteractable> OnInteractableHovered;
        public event Action<IVRInteractable> OnInteractableSelected;
        public event Action<IVRInteractable> OnInteractableActivated;
        public event Action<GestureType> OnGestureRecognized;
        
        public bool IsInitialized => isInitialized;
        public XRRig XRRig => xrRig;
        public Camera VRCamera => vrCamera;
        
        private void Awake()
        {
            InitializeVRInteraction();
        }
        
        private void Start()
        {
            ServiceLocator.RegisterService<VRInteractionManager>(this);
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            UpdateControllerStates();
            ProcessInteractions();
            UpdateHandTracking();
            ProcessGestures();
        }
        
        /// <summary>
        /// 初始化VR交互系统
        /// </summary>
        private void InitializeVRInteraction()
        {
            try
            {
                Logger.LogInfo("初始化VR交互系统...", "VRInteractionManager");
                
                // 初始化控制器状态
                controllerStates = new Dictionary<XRNode, VRControllerState>
                {
                    { XRNode.LeftHand, new VRControllerState() },
                    { XRNode.RightHand, new VRControllerState() }
                };
                
                currentInteractables = new List<IVRInteractable>();
                
                // 设置XR设备
                SetupXRDevice();
                
                // 初始化手势识别
                if (enableHandTracking && gestureRecognizer == null)
                {
                    gestureRecognizer = gameObject.AddComponent<GestureRecognizer>();
                    gestureRecognizer.Initialize();
                }
                
                // 初始化UI管理器
                if (vrUIManager == null)
                {
                    vrUIManager = FindObjectOfType<VRUIManager>();
                    if (vrUIManager == null)
                    {
                        var uiManagerGO = new GameObject("VRUIManager");
                        vrUIManager = uiManagerGO.AddComponent<VRUIManager>();
                    }
                }
                
                isInitialized = true;
                Logger.LogInfo("VR交互系统初始化完成", "VRInteractionManager");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "VR交互系统初始化失败");
            }
        }
        
        /// <summary>
        /// 设置XR设备
        /// </summary>
        private void SetupXRDevice()
        {
            // 自动查找XR Rig
            if (xrRig == null)
            {
                xrRig = FindObjectOfType<XRRig>();
            }
            
            if (xrRig != null)
            {
                vrCamera = xrRig.cameraGameObject.GetComponent<Camera>();
                
                // 查找控制器
                var controllers = xrRig.GetComponentsInChildren<XRController>();
                foreach (var controller in controllers)
                {
                    if (controller.controllerNode == XRNode.LeftHand)
                        leftController = controller.transform;
                    else if (controller.controllerNode == XRNode.RightHand)
                        rightController = controller.transform;
                }
            }
            
            // 配置XR设置
            XRSettings.eyeTextureResolutionScale = 1.0f;
            XRSettings.renderViewportScale = 1.0f;
        }
        
        /// <summary>
        /// 更新控制器状态
        /// </summary>
        private void UpdateControllerStates()
        {
            foreach (var kvp in controllerStates)
            {
                var node = kvp.Key;
                var state = kvp.Value;
                
                // 更新位置和旋转
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out state.position);
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out state.rotation);
                
                // 更新按钮状态
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.triggerButton, out state.triggerPressed);
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.gripButton, out state.gripPressed);
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.primaryButton, out state.primaryPressed);
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.secondaryButton, out state.secondaryPressed);
                
                // 更新触摸板/摇杆
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.primary2DAxis, out state.touchpadAxis);
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out state.touchpadTouched);
                InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out state.touchpadPressed);
                
                // 检测按钮按下事件
                CheckButtonEvents(node, state);
            }
        }
        
        /// <summary>
        /// 检测按钮事件
        /// </summary>
        private void CheckButtonEvents(XRNode node, VRControllerState state)
        {
            // 触发器按下
            if (state.triggerPressed && !state.wasTriggerPressed)
            {
                OnTriggerPressed(node);
                TriggerHapticFeedback(node, hapticIntensity, hapticDuration);
            }
            else if (!state.triggerPressed && state.wasTriggerPressed)
            {
                OnTriggerReleased(node);
            }
            
            // 抓握按钮
            if (state.gripPressed && !state.wasGripPressed)
            {
                OnGripPressed(node);
                TriggerHapticFeedback(node, hapticIntensity * 0.8f, hapticDuration);
            }
            else if (!state.gripPressed && state.wasGripPressed)
            {
                OnGripReleased(node);
            }
            
            // 更新上一帧状态
            state.wasTriggerPressed = state.triggerPressed;
            state.wasGripPressed = state.gripPressed;
            state.wasPrimaryPressed = state.primaryPressed;
            state.wasSecondaryPressed = state.secondaryPressed;
        }
        
        /// <summary>
        /// 处理交互
        /// </summary>
        private void ProcessInteractions()
        {
            // 射线检测交互
            ProcessRaycastInteractions();
            
            // 凝视交互
            if (enableGazeInteraction)
            {
                ProcessGazeInteraction();
            }
            
            // 处理选中的交互对象
            if (selectedInteractable != null)
            {
                selectedInteractable.OnInteractionUpdate();
            }
        }
        
        /// <summary>
        /// 处理射线检测交互
        /// </summary>
        private void ProcessRaycastInteractions()
        {
            foreach (var kvp in controllerStates)
            {
                var node = kvp.Key;
                var state = kvp.Value;
                
                Transform controllerTransform = node == XRNode.LeftHand ? leftController : rightController;
                if (controllerTransform == null) continue;
                
                // 发射射线
                var ray = new Ray(controllerTransform.position, controllerTransform.forward);
                
                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayer))
                {
                    var interactable = hit.collider.GetComponent<IVRInteractable>();
                    if (interactable != null)
                    {
                        // 悬停效果
                        if (state.hoveredInteractable != interactable)
                        {
                            if (state.hoveredInteractable != null)
                            {
                                state.hoveredInteractable.OnHoverExit();
                            }
                            
                            state.hoveredInteractable = interactable;
                            interactable.OnHoverEnter();
                            OnInteractableHovered?.Invoke(interactable);
                        }
                        
                        // 更新悬停位置
                        interactable.OnHoverUpdate(hit.point, hit.normal);
                    }
                }
                else
                {
                    // 清除悬停状态
                    if (state.hoveredInteractable != null)
                    {
                        state.hoveredInteractable.OnHoverExit();
                        state.hoveredInteractable = null;
                    }
                }
                
                // 绘制射线（调试用）
                Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 0.1f);
            }
        }
        
        /// <summary>
        /// 处理凝视交互
        /// </summary>
        private void ProcessGazeInteraction()
        {
            if (vrCamera == null) return;
            
            var ray = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, uiInteractionDistance, interactableLayer))
            {
                var interactable = hit.collider.GetComponent<IVRInteractable>();
                if (interactable != null && interactable.SupportsGazeInteraction)
                {
                    interactable.OnGazeEnter();
                    
                    // 凝视选择逻辑
                    // 这里可以添加凝视计时器等逻辑
                }
            }
        }
        
        /// <summary>
        /// 更新手部追踪
        /// </summary>
        private void UpdateHandTracking()
        {
            if (!enableHandTracking || handTrackingManager == null) return;
            
            handTrackingManager.UpdateHandTracking();
        }
        
        /// <summary>
        /// 处理手势
        /// </summary>
        private void ProcessGestures()
        {
            if (gestureRecognizer == null) return;
            
            var recognizedGesture = gestureRecognizer.RecognizeGesture();
            if (recognizedGesture != GestureType.None)
            {
                OnGestureRecognized?.Invoke(recognizedGesture);
                ProcessGestureAction(recognizedGesture);
            }
        }
        
        /// <summary>
        /// 处理手势动作
        /// </summary>
        private void ProcessGestureAction(GestureType gesture)
        {
            switch (gesture)
            {
                case GestureType.Point:
                    // 指向手势 - 可以用于选择
                    break;
                    
                case GestureType.Grab:
                    // 抓取手势
                    if (selectedInteractable != null)
                    {
                        selectedInteractable.OnGrab();
                    }
                    break;
                    
                case GestureType.Pinch:
                    // 捏取手势 - 精确操作
                    break;
                    
                case GestureType.Wave:
                    // 挥手手势 - 可以用于菜单召唤
                    vrUIManager?.ToggleMainMenu();
                    break;
                    
                case GestureType.ThumbsUp:
                    // 点赞手势 - 确认操作
                    break;
            }
        }
        
        /// <summary>
        /// 触发器按下事件
        /// </summary>
        private void OnTriggerPressed(XRNode node)
        {
            var state = controllerStates[node];
            
            if (state.hoveredInteractable != null)
            {
                selectedInteractable = state.hoveredInteractable;
                selectedInteractable.OnSelect();
                OnInteractableSelected?.Invoke(selectedInteractable);
            }
        }
        
        /// <summary>
        /// 触发器释放事件
        /// </summary>
        private void OnTriggerReleased(XRNode node)
        {
            if (selectedInteractable != null)
            {
                selectedInteractable.OnActivate();
                OnInteractableActivated?.Invoke(selectedInteractable);
                
                selectedInteractable.OnDeselect();
                selectedInteractable = null;
            }
        }
        
        /// <summary>
        /// 抓握按下事件
        /// </summary>
        private void OnGripPressed(XRNode node)
        {
            var state = controllerStates[node];
            
            if (state.hoveredInteractable != null)
            {
                state.hoveredInteractable.OnGrab();
            }
        }
        
        /// <summary>
        /// 抓握释放事件
        /// </summary>
        private void OnGripReleased(XRNode node)
        {
            var state = controllerStates[node];
            
            if (state.hoveredInteractable != null)
            {
                state.hoveredInteractable.OnRelease();
            }
        }
        
        /// <summary>
        /// 触发震动反馈
        /// </summary>
        public void TriggerHapticFeedback(XRNode node, float intensity, float duration)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid)
            {
                device.SendHapticImpulse(0, intensity, duration);
            }
        }
        
        /// <summary>
        /// 注册交互对象
        /// </summary>
        public void RegisterInteractable(IVRInteractable interactable)
        {
            if (!currentInteractables.Contains(interactable))
            {
                currentInteractables.Add(interactable);
            }
        }
        
        /// <summary>
        /// 注销交互对象
        /// </summary>
        public void UnregisterInteractable(IVRInteractable interactable)
        {
            currentInteractables.Remove(interactable);
            
            if (selectedInteractable == interactable)
            {
                selectedInteractable = null;
            }
        }
        
        /// <summary>
        /// 获取控制器位置
        /// </summary>
        public Vector3 GetControllerPosition(XRNode node)
        {
            return controllerStates.ContainsKey(node) ? controllerStates[node].position : Vector3.zero;
        }
        
        /// <summary>
        /// 获取控制器旋转
        /// </summary>
        public Quaternion GetControllerRotation(XRNode node)
        {
            return controllerStates.ContainsKey(node) ? controllerStates[node].rotation : Quaternion.identity;
        }
        
        /// <summary>
        /// 设置交互距离
        /// </summary>
        public void SetInteractionDistance(float distance)
        {
            raycastDistance = distance;
        }
        
        /// <summary>
        /// 启用/禁用手势识别
        /// </summary>
        public void SetHandTrackingEnabled(bool enabled)
        {
            enableHandTracking = enabled;
            
            if (gestureRecognizer != null)
            {
                gestureRecognizer.enabled = enabled;
            }
        }
        
        public void Initialize()
        {
            if (!isInitialized)
            {
                InitializeVRInteraction();
            }
        }
        
        public void Cleanup()
        {
            currentInteractables.Clear();
            selectedInteractable = null;
            
            if (gestureRecognizer != null)
            {
                Destroy(gestureRecognizer);
            }
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
    }
    
    /// <summary>
    /// VR控制器状态
    /// </summary>
    [Serializable]
    public class VRControllerState
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool triggerPressed;
        public bool gripPressed;
        public bool primaryPressed;
        public bool secondaryPressed;
        public Vector2 touchpadAxis;
        public bool touchpadTouched;
        public bool touchpadPressed;
        
        // 上一帧状态
        public bool wasTriggerPressed;
        public bool wasGripPressed;
        public bool wasPrimaryPressed;
        public bool wasSecondaryPressed;
        
        // 当前悬停的交互对象
        public IVRInteractable hoveredInteractable;
    }
}