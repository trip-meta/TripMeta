using UnityEngine;

namespace TripMeta.UGC
{
    /// <summary>
    /// 移动工具
    /// 允许用户移动选中的对象
    /// </summary>
    public class MoveTool : BaseEditorTool
    {
        private Vector3 dragStartPosition;
        private Vector3 dragStartMousePosition;
        private bool isDragging;
        private TransformAxis activeAxis;

        public MoveTool(SceneEditorManager manager) : base(manager) { }

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

            // 检查是否点击了变换轴
            activeAxis = GetAxisUnderMouse();
            if (activeAxis != TransformAxis.None)
            {
                isDragging = true;
                dragStartPosition = GetSelectionCenter();
                dragStartMousePosition = Input.mousePosition;
            }
        }

        /// <summary>
        /// 更新拖拽
        /// </summary>
        private void UpdateDrag()
        {
            if (!isDragging) return;

            Vector3 delta = CalculateMoveDelta();

            foreach (var obj in manager.SelectedObjects)
            {
                if (obj.Instance != null && !obj.IsLocked)
                {
                    obj.Instance.transform.position += delta;
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
            activeAxis = TransformAxis.None;
            manager.RecordHistory();
        }

        /// <summary>
        /// 获取鼠标下的变换轴
        /// </summary>
        private TransformAxis GetAxisUnderMouse()
        {
            // 简化实现：使用键盘快捷键选择轴
            if (Input.GetKey(KeyCode.X)) return TransformAxis.X;
            if (Input.GetKey(KeyCode.Y)) return TransformAxis.Y;
            if (Input.GetKey(KeyCode.Z)) return TransformAxis.Z;

            return TransformAxis.All;
        }

        /// <summary>
        /// 计算移动增量
        /// </summary>
        private Vector3 CalculateMoveDelta()
        {
            Vector2 mouseDelta = (Vector2)Input.mousePosition - (Vector2)dragStartMousePosition;
            float sensitivity = 0.01f;

            Vector3 worldDelta = Vector3.zero;

            switch (activeAxis)
            {
                case TransformAxis.X:
                    worldDelta = Camera.main.transform.right * mouseDelta.x * sensitivity;
                    break;
                case TransformAxis.Y:
                    worldDelta = Vector3.up * mouseDelta.y * sensitivity;
                    break;
                case TransformAxis.Z:
                    worldDelta = Camera.main.transform.forward * mouseDelta.x * sensitivity;
                    break;
                case TransformAxis.All:
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Plane plane = new Plane(Vector3.up, dragStartPosition);
                    if (plane.Raycast(ray, out float distance))
                    {
                        worldDelta = ray.GetPoint(distance) - dragStartPosition;
                        dragStartPosition = ray.GetPoint(distance);
                    }
                    break;
            }

            return manager.GetSnappedPosition(worldDelta) - worldDelta + worldDelta;
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

            // 绘制移动轴
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, center + Vector3.right * 2f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + Vector3.up * 2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, center + Vector3.forward * 2f);
        }
    }

    public enum TransformAxis
    {
        None,
        X,
        Y,
        Z,
        All
    }
}
