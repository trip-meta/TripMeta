using UnityEngine;
using System.Collections.Generic;

namespace TripMeta.UGC
{
    /// <summary>
    /// 选择工具
    /// 允许用户选择场景中的对象
    /// </summary>
    public class SelectionTool : BaseEditorTool
    {
        private Vector2 selectionStart;
        private bool isSelecting;
        private Rect selectionRect;

        public SelectionTool(SceneEditorManager manager) : base(manager) { }

        public override void Update()
        {
            if (!isActive) return;

            HandleSelection();
        }

        /// <summary>
        /// 处理选择
        /// </summary>
        private void HandleSelection()
        {
            if (Input.GetMouseButtonDown(0))
            {
                selectionStart = Input.mousePosition;
                isSelecting = true;

                // 检查是否点击了对象
                if (GetObjectAtMouse(out SceneObject clickedObject))
                {
                    bool additive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    manager.SelectObject(clickedObject, additive);
                }
                else if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    // 点击空白处，取消选择
                    manager.SelectObject(null, false);
                }
            }

            if (Input.GetMouseButton(0) && isSelecting)
            {
                // 更新选择框
                UpdateSelectionRect();
            }

            if (Input.GetMouseButtonUp(0) && isSelecting)
            {
                isSelecting = false;
                // 处理框选
                HandleBoxSelection();
            }

            // 全选
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
            {
                SelectAll();
            }
        }

        /// <summary>
        /// 获取鼠标位置的对象
        /// </summary>
        private bool GetObjectAtMouse(out SceneObject sceneObject)
        {
            sceneObject = null;
            Ray ray = GetMouseRay();

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 从GameObject查找对应的SceneObject
                // 简化实现：通过名称解析
                string name = hit.collider.gameObject.name;
                if (name.StartsWith("SceneObject_"))
                {
                    string objectId = name.Substring("SceneObject_".Length);
                    // 这里应该从manager查找对应的SceneObject
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 更新选择框
        /// </summary>
        private void UpdateSelectionRect()
        {
            Vector2 current = Input.mousePosition;
            selectionRect = new Rect(
                Mathf.Min(selectionStart.x, current.x),
                Mathf.Min(selectionStart.y, current.y),
                Mathf.Abs(current.x - selectionStart.x),
                Mathf.Abs(current.y - selectionStart.y)
            );
        }

        /// <summary>
        /// 处理框选
        /// </summary>
        private void HandleBoxSelection()
        {
            // 这里实现框选逻辑
            // 检查哪些对象在选择框内
            List<SceneObject> selected = new List<SceneObject>();

            // 简化实现：遍历所有对象检查屏幕位置
            if (manager.CurrentScene != null)
            {
                foreach (var obj in manager.CurrentScene.objects)
                {
                    if (obj.Instance != null)
                    {
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(obj.Instance.transform.position);
                        if (selectionRect.Contains(screenPos))
                        {
                            selected.Add(obj);
                        }
                    }
                }
            }

            if (selected.Count > 0)
            {
                bool additive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                if (!additive)
                {
                    manager.SelectObject(null, false);
                }

                foreach (var obj in selected)
                {
                    manager.SelectObject(obj, true);
                }
            }
        }

        /// <summary>
        /// 全选
        /// </summary>
        private void SelectAll()
        {
            if (manager.CurrentScene?.objects == null) return;

            foreach (var obj in manager.CurrentScene.objects)
            {
                manager.SelectObject(obj, true);
            }
        }

        public override void OnDrawGizmos()
        {
            if (isSelecting && selectionRect.size.magnitude > 10f)
            {
                // 绘制选择框
                // 注意：实际实现应该使用OnGUI来绘制UI选择框
            }
        }
    }
}
