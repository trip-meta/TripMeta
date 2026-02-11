using UnityEngine;
using System.Collections;

namespace TripMeta.Demo
{
    /// <summary>
    /// 景点视觉效果 - 添加粒子特效和动画
    /// </summary>
    public class LocationVisualEffects : MonoBehaviour
    {
        [Header("粒子效果")]
        [SerializeField] private bool enableParticles = true;
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private float particleHeight = 2f;

        [Header("光晕效果")]
        [SerializeField] private bool enableGlow = true;
        [SerializeField] private Color glowColor = new Color(0, 1f, 1f, 0.5f);
        [SerializeField] private float glowRange = 3f;

        [Header("浮动动画")]
        [SerializeField] private bool enableFloating = true;
        [SerializeField] private float floatAmplitude = 0.3f;
        [SerializeField] private float floatSpeed = 1f;

        private ParticleSystem particleSystem;
        private Light glowLight;
        private Vector3 startPosition;
        private float floatOffset;

        void Start()
        {
            startPosition = transform.position;
            floatOffset = Random.Range(0f, Mathf.PI * 2f);

            CreateParticles();
            CreateGlowEffect();
        }

        void Update()
        {
            if (enableFloating)
            {
                FloatingAnimation();
            }
        }

        /// <summary>
        /// 创建粒子效果
        /// </summary>
        private void CreateParticles()
        {
            if (!enableParticles) return;

            GameObject particleObj = new GameObject("LocationParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = new Vector3(0, particleHeight, 0);

            particleSystem = particleObj.AddComponent<ParticleSystem>();

            var main = particleSystem.main;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.startSize = 0.2f;
            main.startColor = new Color(0, 1f, 1f, 0.6f);
            main.maxParticles = 100;
            main.emissionRate = 20;
            main.gravityModifier = -0.5f;

            var emission = particleSystem.emission;
            emission.rateOverTime = 20;

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = new Color(0, 1f, 1f);
        }

        /// <summary>
        /// 创建光晕效果
        /// </summary>
        private void CreateGlowEffect()
        {
            if (!enableGlow) return;

            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = new Vector3(0, 1.5f, 0);

            glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = glowColor;
            glowLight.range = glowRange;
            glowLight.intensity = 2f;
            glowLight.shadows = LightShadows.None;
        }

        /// <summary>
        /// 浮动动画
        /// </summary>
        private void FloatingAnimation()
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed + floatOffset) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        /// <summary>
        /// 激活景点效果
        /// </summary>
        public void Activate()
        {
            gameObject.SetActive(true);
            StartCoroutine(ActivationEffect());
        }

        /// <summary>
        /// 激活特效动画
        /// </summary>
        private IEnumerator ActivationEffect()
        {
            float duration = 0.5f;
            float elapsed = 0f;

            Vector3 startScale = Vector3.zero;
            Vector3 targetScale = Vector3.one;

            transform.localScale = startScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// 停用景点效果
        /// </summary>
        public void Deactivate()
        {
            StartCoroutine(DeactivationEffect());
        }

        /// <summary>
        /// 停用特效动画
        /// </summary>
        private IEnumerator DeactivationEffect()
        {
            float duration = 0.3f;
            float elapsed = 0f;

            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                yield return null;
            }

            gameObject.SetActive(false);
        }

        void OnDrawGizmos()
        {
            // 绘制影响范围
            Gizmos.color = new Color(0, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, glowRange);
        }

        void OnDrawGizmosSelected()
        {
            // 选中时显示更多信息
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 3f);
        }
    }

    /// <summary>
    /// 传送特效
    /// </summary>
    public class TeleportEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem teleportParticle;
        [SerializeField] private AudioSource teleportSound;
        [SerializeField] private Light flashLight;
        [SerializeField] private float flashDuration = 0.3f;

        private bool isTeleporting = false;

        public void PlayTeleportEffect(Vector3 position)
        {
            if (isTeleporting) return;

            StartCoroutine(TeleportSequence(position));
        }

        private IEnumerator TeleportSequence(Vector3 targetPosition)
        {
            isTeleporting = true;

            // 开始传送效果
            if (teleportParticle != null)
            {
                teleportParticle.Play();
            }

            // 光照闪烁
            if (flashLight != null)
            {
                flashLight.enabled = true;
                yield return new WaitForSeconds(flashDuration);
                flashLight.enabled = false;
            }

            // 播放声音
            if (teleportSound != null)
            {
                teleportSound.Play();
            }

            isTeleporting = false;
        }
    }

    /// <summary>
    /// 路径指示器
    /// </summary>
    public class PathIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float arrowSpeed = 2f;
        [SerializeField] private int arrowCount = 5;

        private GameObject[] arrows;

        void Start()
        {
            CreatePathArrows();
        }

        private void CreatePathArrows()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            arrows = new GameObject[arrowCount];

            for (int i = 0; i < arrowCount; i++)
            {
                arrows[i] = Instantiate(arrowPrefab, transform);
                arrows[i].transform.position = waypoints[0].position;
                arrows[i].name = "PathArrow_" + i;
            }

            StartCoroutine(AnimateArrows());
        }

        private IEnumerator AnimateArrows()
        {
            float t = 0f;

            while (true)
            {
                t += Time.deltaTime * arrowSpeed;
                float progress = t % 1f;

                // 更新每个箭头的位置
                for (int i = 0; i < arrowCount; i++)
                {
                    float arrowProgress = (progress + i / (float)arrowCount) % 1f;
                    arrows[i].transform.position = GetPointOnPath(arrowProgress);
                    arrows[i].transform.LookAt(GetPointOnPath((arrowProgress + 0.01f) % 1f));
                }

                yield return null;
            }
        }

        private Vector3 GetPointOnPath(float progress)
        {
            // 在路径上插值
            int segment = Mathf.FloorToInt(progress * (waypoints.Length - 1));
            float segmentProgress = (progress * (waypoints.Length - 1)) - segment;

            int nextSegment = (segment + 1) % waypoints.Length;

            return Vector3.Lerp(waypoints[segment].position, waypoints[nextSegment].position, segmentProgress);
        }
    }

    /// <summary>
    /// 文字浮动效果
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float floatHeight = 0.5f;
        [SerializeField] private Color textColor = Color.white;

        private TextMesh textMesh;
        private Vector3 startPosition;

        void Start()
        {
            CreateTextMesh();
            startPosition = transform.position;
        }

        private void CreateTextMesh()
        {
            GameObject textObj = new GameObject("FloatingText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = new Vector3(0, 2.5f, 0);

            textMesh = textObj.AddComponent<TextMesh>();
            textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textMesh.text = name;
            textMesh.fontSize = 24;
            textMesh.color = textColor;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            // 使文字始终面向相机
            textObj.AddComponent<Billboard>();
        }

        void Update()
        {
            if (textMesh != null)
            {
                float newY = startPosition.y + Mathf.Abs(Mathf.Sin(Time.time * floatSpeed)) * floatHeight;
                textMesh.transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }
    }

    /// <summary>
    /// 广告牌组件 - 始终面向相机
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private bool onlyY = true;

        void LateUpdate()
        {
            if (Camera.main == null) return;

            Vector3 targetPosition = Camera.main.transform.position;

            if (onlyY)
            {
                targetPosition.y = transform.position.y;
            }

            transform.LookAt(targetPosition);
        }
    }
}
