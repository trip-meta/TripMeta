using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TripMeta.Features.AR
{
    /// <summary>
    /// AR卡片控制器 - 管理AR信息卡片的显示和交互
    /// </summary>
    public class ARCardController : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button expandButton;

        [Header("动画")]
        [SerializeField] private Animator animator;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float lookAtSpeed = 5f;

        [Header("视觉效果")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float hoverScale = 1.1f;

        private AROverlayInfo _info;
        private bool _isExpanded;
        private Camera _mainCamera;
        private Collider _collider;

        // 事件
        public event Action OnClicked;
        public event Action OnClosed;

        private void Start()
        {
            _mainCamera = Camera.main;
            _collider = GetComponent<Collider>();

            // 设置按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (expandButton != null)
            {
                expandButton.onClick.AddListener(OnExpandClicked);
            }

            // 初始动画
            FadeIn();
        }

        private void Update()
        {
            // 让AR卡片始终面向相机
            FaceCamera();
        }

        /// <summary>
        /// 设置AR卡片信息
        /// </summary>
        public void SetInfo(AROverlayInfo info)
        {
            _info = info;

            if (titleText != null)
            {
                titleText.text = info.Title;
            }

            if (descriptionText != null)
            {
                descriptionText.text = info.Description;
                descriptionText.gameObject.SetActive(false); // 默认折叠
            }

            // 根据类型设置图标
            SetIconByType(info.Type);
        }

        private void SetIconByType(OverlayType type)
        {
            if (iconImage == null) return;

            // 根据类型设置不同的图标颜色
            switch (type)
            {
                case OverlayType.InfoCard:
                    backgroundImage.color = new Color(0.2f, 0.6f, 1f, 0.8f);
                    break;
                case OverlayType.HistoricalPhoto:
                    backgroundImage.color = new Color(0.8f, 0.6f, 0.2f, 0.8f);
                    break;
                case OverlayType.AudioGuide:
                    backgroundImage.color = new Color(0.2f, 0.8f, 0.4f, 0.8f);
                    break;
                case OverlayType.NavigationArrow:
                    backgroundImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                    break;
                case OverlayType.FunFact:
                    backgroundImage.color = new Color(0.8f, 0.2f, 0.8f, 0.8f);
                    break;
            }
        }

        private void FaceCamera()
        {
            if (_mainCamera == null) return;

            // 平滑转向相机
            Vector3 directionToCamera = _mainCamera.transform.position - transform.position;
            directionToCamera.y = 0; // 只在水平方向旋转

            if (directionToCamera != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookAtSpeed);
            }
        }

        private void FadeIn()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                StartCoroutine(FadeInCoroutine());
            }

            // 缩放动画
            transform.localScale = Vector3.zero;
            LeanTween.scale(gameObject, Vector3.one, fadeInDuration)
                .setEaseOutBack();
        }

        private System.Collections.IEnumerator FadeInCoroutine()
        {
            float elapsed = 0;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / fadeInDuration;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }

        private void OnCloseClicked()
        {
            OnClosed?.Invoke();
            CloseCard();
        }

        private void OnExpandClicked()
        {
            _isExpanded = !_isExpanded;

            if (descriptionText != null)
            {
                descriptionText.gameObject.SetActive(_isExpanded);
            }

            // 播放展开/折叠动画
            if (animator != null)
            {
                animator.SetBool("IsExpanded", _isExpanded);
            }
        }

        /// <summary>
        /// 关闭卡片
        /// </summary>
        public void CloseCard()
        {
            LeanTween.scale(gameObject, Vector3.zero, 0.3f)
                .setEaseInBack()
                .setOnComplete(() => Destroy(gameObject));
        }

        /// <summary>
        /// 处理点击事件（用于3D点击检测）
        /// </summary>
        public void OnPointerClick()
        {
            OnClicked?.Invoke();

            // 点击反馈动画
            LeanTween.scale(gameObject, Vector3.one * 0.9f, 0.1f)
                .setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, Vector3.one, 0.1f);
                });
        }

        /// <summary>
        /// 处理悬停进入
        /// </summary>
        public void OnPointerEnter()
        {
            LeanTween.scale(gameObject, Vector3.one * hoverScale, 0.2f);
        }

        /// <summary>
        /// 处理悬停离开
        /// </summary>
        public void OnPointerExit()
        {
            LeanTween.scale(gameObject, Vector3.one, 0.2f);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }

            if (expandButton != null)
            {
                expandButton.onClick.RemoveListener(OnExpandClicked);
            }
        }
    }
}
