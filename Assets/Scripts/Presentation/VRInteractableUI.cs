using UnityEngine;
using UnityEngine.Events;

namespace TripMeta.Presentation
{
    /// <summary>
    /// VR可交互UI组件 - 使UI元素在VR中可交互
    /// </summary>
    public class VRInteractableUI : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float hoverScaleMultiplier = 1.1f;
        [SerializeField] private float hoverTransitionSpeed = 5f;
        [SerializeField] private bool enableHapticFeedback = true;

        [Header("Events")]
        public UnityEvent OnHoverEnter = new UnityEvent();
        public UnityEvent OnHoverExit = new UnityEvent();
        public UnityEvent OnClick = new UnityEvent();

        private Vector3 originalScale;
        private Vector3 targetScale;
        private bool isHovered = false;
        private Collider interactCollider;

        void Start()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
            interactCollider = GetComponent<Collider>();

            // 确保有碰撞器
            if (interactCollider == null)
            {
                interactCollider = gameObject.AddComponent<BoxCollider>();
            }
        }

        void Update()
        {
            // 平滑过渡缩放
            if (isHovered)
            {
                transform.localScale = Vector3.Lerp(
                    transform.localScale,
                    originalScale * hoverScaleMultiplier,
                    Time.deltaTime * hoverTransitionSpeed
                );
            }
            else
            {
                transform.localScale = Vector3.Lerp(
                    transform.localScale,
                    originalScale,
                    Time.deltaTime * hoverTransitionSpeed
                );
            }
        }

        /// <summary>
        /// 悬停进入 (由VR输入系统调用)
        /// </summary>
        public void OnPointerEnter()
        {
            if (!isHovered)
            {
                isHovered = true;
                OnHoverEnter?.Invoke();
                Debug.Log($"[VRInteractableUI] Hover enter: {gameObject.name}");
            }
        }

        /// <summary>
        /// 悬停退出 (由VR输入系统调用)
        /// </summary>
        public void OnPointerExit()
        {
            if (isHovered)
            {
                isHovered = false;
                OnHoverExit?.Invoke();
                Debug.Log($"[VRInteractableUI] Hover exit: {gameObject.name}");
            }
        }

        /// <summary>
        /// 点击 (由VR输入系统调用)
        /// </summary>
        public void OnPointerClick()
        {
            OnClick?.Invoke();
            Debug.Log($"[VRInteractableUI] Clicked: {gameObject.name}");

            // 触觉反馈 (如果启用)
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback();
            }
        }

        /// <summary>
        /// 触发触觉反馈
        /// </summary>
        private void TriggerHapticFeedback()
        {
            // TODO: 与VRControllerManager集成提供触觉反馈
            Debug.Log("[VRInteractableUI] Haptic feedback triggered");
        }

        void OnDisable()
        {
            isHovered = false;
        }
    }

    /// <summary>
    /// VR按钮组件
    /// </summary>
    public class VRButton : VRInteractableUI
    {
        [Header("Button Settings")]
        [SerializeField] private UnityEngine.UI.Text buttonText;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Renderer buttonRenderer;

        private Material buttonMaterial;

        protected override void Awake()
        {
            base.Awake();

            if (buttonRenderer != null)
            {
                buttonMaterial = buttonRenderer.material;
            }
        }

        public void SetText(string text)
        {
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }

        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            UpdateButtonColor(hoverColor);
        }

        public override void OnPointerExit()
        {
            base.OnPointerExit();
            UpdateButtonColor(normalColor);
        }

        private void UpdateButtonColor(Color color)
        {
            if (buttonMaterial != null)
            {
                buttonMaterial.color = color;
            }
        }
    }
}
