using UnityEngine;

namespace TripMeta.UGC
{
    /// <summary>
    /// 绘制工具
    /// 允许用户在场景中绘制装饰元素
    /// </summary>
    public class PaintTool : BaseEditorTool
    {
        private string selectedPrefabId;
        private float paintDensity = 1f;
        private float paintRadius = 5f;
        private bool isPainting;
        private Vector3 lastPaintPosition;

        public PaintTool(SceneEditorManager manager) : base(manager) { }

        public override void Update()
        {
            if (!isActive) return;

            HandleInput();

            if (isPainting)
            {
                PaintObjects();
            }
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPainting = true;
                lastPaintPosition = Vector3.zero;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isPainting = false;
            }

            // 调整笔刷大小
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                paintRadius = Mathf.Clamp(paintRadius + scroll * 2f, 0.5f, 20f);
            }
        }

        /// <summary>
        /// 绘制对象
        /// </summary>
        private void PaintObjects()
        {
            if (string.IsNullOrEmpty(selectedPrefabId)) return;

            Ray ray = GetMouseRay();
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            // 检查与上次绘制的距离
            if (Vector3.Distance(hit.point, lastPaintPosition) < 1f / paintDensity)
            {
                return;
            }

            lastPaintPosition = hit.point;

            // 在笔刷范围内随机位置放置对象
            int objectCount = Mathf.CeilToInt(paintDensity);

            for (int i = 0; i < objectCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * paintRadius;
                Vector3 position = hit.point + new Vector3(randomOffset.x, 0, randomOffset.y);

                // 射线检测找到准确的地表位置
                if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out RaycastHit groundHit))
                {
                    position = groundHit.point;

                    // 随机旋转
                    Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                    // 随机缩放
                    float scaleVariation = Random.Range(0.8f, 1.2f);
                    Vector3 scale = Vector3.one * scaleVariation;

                    manager.AddObject(selectedPrefabId, position, rotation, scale);
                }
            }
        }

        /// <summary>
        /// 设置绘制的预制体
        /// </summary>
        public void SetPaintPrefab(string prefabId)
        {
            selectedPrefabId = prefabId;
        }

        /// <summary>
        /// 设置绘制密度
        /// </summary>
        public void SetPaintDensity(float density)
        {
            paintDensity = Mathf.Clamp(density, 0.1f, 10f);
        }

        /// <summary>
        /// 设置笔刷半径
        /// </summary>
        public void SetPaintRadius(float radius)
        {
            paintRadius = Mathf.Clamp(radius, 0.5f, 20f);
        }

        public override void OnDrawGizmos()
        {
            if (!isActive) return;

            Ray ray = GetMouseRay();
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(hit.point, paintRadius);

                // 绘制密度指示
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                int previewCount = Mathf.Min((int)paintDensity, 10);
                for (int i = 0; i < previewCount; i++)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * paintRadius;
                    Vector3 pos = hit.point + new Vector3(randomOffset.x, 0, randomOffset.y);
                    Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
                }
            }
        }
    }
}
