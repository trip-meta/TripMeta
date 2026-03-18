using UnityEngine;

namespace TripMeta.UGC
{
    /// <summary>
    /// 旋转工具
    /// 允许用户旋转选中的对象
    /// </summary>
    public class RotateTool : BaseEditorTool
    {
        private Quaternion dragStartRotation;
        private Vector3 dragStartMousePosition;
        private bool isDragging;
        private Vector3 rotationCenter;

        public RotateTool(SceneEditorManager manager) : base(manager) { }

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
            rotationCenter = GetSelectionCenter();
            dragStartMousePosition = Input.mousePosition;

            // 记录初始旋转
            if (manager.SelectedObjects.Count == 1)
            {
                dragStartRotation = manager.SelectedObjects[0].Instance.transform.rotation;
            }
        }

        /// <summary>
        /// 更新拖拽
        /// </summary>
        private void UpdateDrag()
        {
            if (!isDragging) return;

            Vector2 mouseDelta = (Vector2)Input.mousePosition - (Vector2)dragStartMousePosition;
            float sensitivity = 0.5f;

            // 计算旋转
            float rotationX = -mouseDelta.y * sensitivity;
            float rotationY = mouseDelta.x * sensitivity;

            Quaternion deltaRotation = Quaternion.Euler(rotationX, rotationY, 0);

            foreach (var obj in manager.SelectedObjects)
            {
                if (obj.Instance != null && !obj.IsLocked)
                {
                    if (manager.SelectedObjects.Count == 1)
                    {
                        // 单个对象：直接旋转
                        obj.Instance.transform.rotation = dragStartRotation * deltaRotation;
                    }
                    else
                    {
                        // 多个对象：围绕中心旋转
                        Vector3 offset = obj.Instance.transform.position - rotationCenter;
                        offset = deltaRotation * offset;
                        obj.Instance.transform.position = rotationCenter + offset;
                        obj.Instance.transform.rotation = obj.Instance.transform.rotation * deltaRotation;
                    }

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

            // 应用捕捉
            if (manager.snapMode != SnapMode.None)
            {
                foreach (var obj in manager.SelectedObjects)
                {
                    if (obj.Instance != null && !obj.IsLocked)
                    {
                        obj.Instance.transform.rotation = manager.GetSnappedRotation(obj.Instance.transform.rotation);
                        obj.UpdateTransform();
                    }
                }
            }

            manager.RecordHistory();
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

        public override void OnDrawGizmos()
        {
            if (manager.SelectedObjects.Count == 0) return;

            Vector3 center = GetSelectionCenter();

            // 绘制旋转环
            Gizmos.color = Color.red;
            DrawGizmoCircle(center, Vector3.right, 2f);

            Gizmos.color = Color.green;
            DrawGizmoCircle(center, Vector3.up, 2f);

            Gizmos.color = Color.blue;
            DrawGizmoCircle(center, Vector3.forward, 2f);
        }

        /// <summary>
        /// 绘制Gizmo圆环
        /// </summary>
        private void DrawGizmoCircle(Vector3 center, Vector3 normal, float radius)
        {
            int segments = 32;
            Vector3 prevPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                Vector3 point = center + GetCirclePoint(normal, angle) * radius;

                if (i > 0)
                {
                    Gizmos.DrawLine(prevPoint, point);
                }

                prevPoint = point;
            }
        }

        /// <summary>
        /// 获取圆上的点
        /// </summary>
        private Vector3 GetCirclePoint(Vector3 normal, float angle)
        {
            if (normal == Vector3.up)
            {
                return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            }
            else if (normal == Vector3.right)
            {
                return new Vector3(0, Mathf.Cos(angle), Mathf.Sin(angle));
            }
            else
            {
                return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            }
        }
    }
}
