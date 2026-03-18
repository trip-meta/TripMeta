using UnityEngine;

namespace TripMeta.UGC
{
    /// <summary>
    /// 缩放工具
    /// 允许用户缩放选中的对象
    /// </summary>
    public class ScaleTool : BaseEditorTool
    {
        private Vector3 dragStartScale;
        private Vector3 dragStartMousePosition;
        private bool isDragging;
        private float initialDistance;

        public ScaleTool(SceneEditorManager manager) : base(manager) { }

        public override void Update()
        {
            if (!isActive) return;

            if (Input.GetMouseButtonDown(0))
            {
                StartDrag();
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                UpdateDrag();
            }

            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                EndDrag();
            }
        }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        private void StartDrag()
        {
            if (manager.SelectedObjects.Count == 0) return;

            isDragging = true;
            dragStartMousePosition = Input.mousePosition;

            // 记录初始缩放
            if (manager.SelectedObjects.Count == 1)
            {
                dragStartScale = manager.SelectedObjects[0].Instance.transform.localScale;
            }
            else
            {
                dragStartScale = Vector3.one;
            }

            initialDistance = ((Vector2)Input.mousePosition - (Vector2)dragStartMousePosition).magnitude;
        }

        /// <summary>
        /// 更新拖拽
        /// </summary>
        private void UpdateDrag()
        {
            if (!isDragging) return;

            Vector2 mouseDelta = (Vector2)Input.mousePosition - (Vector2)dragStartMousePosition;
            float currentDistance = mouseDelta.magnitude;

            // 计算缩放因子
            float scaleFactor = 1f;
            if (initialDistance > 0)
            {
                scaleFactor = currentDistance / 100f + 1f;
            }
            else
            {
                scaleFactor = 1f + mouseDelta.y * 0.01f;
            }

            // 限制最小缩放
            scaleFactor = Mathf.Max(0.1f, scaleFactor);

            Vector3 newScale = dragStartScale * scaleFactor;

            foreach (var obj in manager.SelectedObjects)
            {
                if (obj.Instance != null && !obj.IsLocked)
                {
                    obj.Instance.transform.localScale = newScale;
                    obj.UpdateTransform();
                }
            }
        }

        /// <summary>
        /// 结束拖拽
        /// </summary>
        private void EndDrag()
        {
            isDragging = false;
            manager.RecordHistory();
        }

        public override void OnDrawGizmos()
        {
            if (manager.SelectedObjects.Count == 0) return;

            Vector3 center = GetSelectionCenter();

            // 绘制缩放轴
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, center + Vector3.right * 2f);
            Gizmos.DrawCube(center + Vector3.right * 2f, Vector3.one * 0.2f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + Vector3.up * 2f);
            Gizmos.DrawCube(center + Vector3.up * 2f, Vector3.one * 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, center + Vector3.forward * 2f);
            Gizmos.DrawCube(center + Vector3.forward * 2f, Vector3.one * 0.2f);

            // 绘制中心缩放盒
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, Vector3.one * 0.5f);
        }

        /// <summary>
        /// 获取选择对象的中心
        /// </summary>
        private Vector3 GetSelectionCenter()
        {
            if (manager.SelectedObjects.Count == 0) return Vector3.zero;

            Vector3 center = Vector3.zero;
            int count = 0;

            foreach (var obj in manager.SelectedObjects)
            {
                if (obj.Instance != null)
                {
                    center += obj.Instance.transform.position;
                    count++;
                }
            }

            return count > 0 ? center / count : Vector3.zero;
        }
    }
}
