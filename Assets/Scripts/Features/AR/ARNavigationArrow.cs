using UnityEngine;
using TMPro;

namespace TripMeta.Features.AR
{
    /// <summary>
    /// AR导航箭头 - 指向目标位置的3D箭头
    /// </summary>
    public class ARNavigationArrow : MonoBehaviour
    {
        [Header("视觉组件")]
        [SerializeField] private Transform arrowMesh;
        [SerializeField] private TextMeshPro distanceText;
        [SerializeField] private TextMeshPro labelText;
        [SerializeField] private LineRenderer pathLine;

        [Header("动画")]
        [SerializeField] private float bobbingSpeed = 2f;
        [SerializeField] private float bobbingHeight = 0.1f;
        [SerializeField] private float rotationSpeed = 50f;

        [Header("设置")]
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private float hideDistance = 2f; // 距离目标多近时隐藏

        private Vector3 _targetPosition;
        private string _label;
        private Transform _playerTransform;
        private float _lastUpdateTime;
        private Vector3 _initialPosition;

        private void Start()
        {
            _playerTransform = Camera.main?.transform;
            _initialPosition = transform.position;
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            // 浮动动画
            FloatAnimation();

            // 更新方向和距离
            if (Time.time - _lastUpdateTime > updateInterval)
            {
                UpdateDirection();
                UpdateDistance();
                _lastUpdateTime = Time.time;
            }

            // 检查是否到达目标
            CheckArrival();
        }

        /// <summary>
        /// 设置目标位置
        /// </summary>
        public void SetTarget(Vector3 targetPosition, string label)
        {
            _targetPosition = targetPosition;
            _label = label;

            if (labelText != null)
            {
                labelText.text = label;
            }

            UpdateDirection();
        }

        private void FloatAnimation()
        {
            // 上下浮动
            float yOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
            transform.position = _initialPosition + Vector3.up * yOffset;

            // 旋转动画
            if (arrowMesh != null)
            {
                arrowMesh.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }

        private void UpdateDirection()
        {
            if (_playerTransform == null) return;

            // 计算指向目标的方向
            Vector3 directionToTarget = _targetPosition - _playerTransform.position;
            directionToTarget.y = 0; // 保持水平

            if (directionToTarget.magnitude > 0.1f)
            {
                // 设置箭头旋转
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            // 更新路径线
            if (pathLine != null)
            {
                pathLine.SetPosition(0, transform.position);
                pathLine.SetPosition(1, _targetPosition);
            }
        }

        private void UpdateDistance()
        {
            if (_playerTransform == null) return;

            float distance = Vector3.Distance(_playerTransform.position, _targetPosition);

            if (distanceText != null)
            {
                distanceText.text = $"{distance:F0}m";
            }

            // 根据距离调整透明度
            float alpha = Mathf.Clamp01(distance / 10f);
            SetAlpha(alpha);
        }

        private void CheckArrival()
        {
            if (_playerTransform == null) return;

            float distance = Vector3.Distance(_playerTransform.position, _targetPosition);

            // 如果足够近，隐藏箭头
            if (distance < hideDistance)
            {
                gameObject.SetActive(false);
            }
            else if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        private void SetAlpha(float alpha)
        {
            if (arrowMesh != null)
            {
                var renderers = arrowMesh.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    foreach (var material in renderer.materials)
                    {
                        Color color = material.color;
                        color.a = alpha;
                        material.color = color;
                    }
                }
            }

            if (distanceText != null)
            {
                distanceText.alpha = alpha;
            }

            if (labelText != null)
            {
                labelText.alpha = alpha;
            }
        }

        /// <summary>
        /// 更新箭头位置（跟随玩家）
        /// </summary>
        public void UpdatePosition(Vector3 newPosition)
        {
            _initialPosition = newPosition;
        }
    }
}
