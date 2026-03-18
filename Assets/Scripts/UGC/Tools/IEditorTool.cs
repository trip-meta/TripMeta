using UnityEngine;

namespace TripMeta.UGC
{
    /// <summary>
    /// 编辑器工具接口
    /// </summary>
    public interface IEditorTool
    {
        void Activate();
        void Deactivate();
        void Update();
        void OnMouseDown(Vector3 position);
        void OnMouseDrag(Vector3 position);
        void OnMouseUp(Vector3 position);
        void OnDrawGizmos();
    }

    /// <summary>
    /// 基础编辑工具
    /// </summary>
    public abstract class BaseEditorTool : IEditorTool
    {
        protected SceneEditorManager manager;
        protected bool isActive;

        public BaseEditorTool(SceneEditorManager manager)
        {
            this.manager = manager;
        }

        public virtual void Activate()
        {
            isActive = true;
        }

        public virtual void Deactivate()
        {
            isActive = false;
        }

        public virtual void Update() { }

        public virtual void OnMouseDown(Vector3 position) { }

        public virtual void OnMouseDrag(Vector3 position) { }

        public virtual void OnMouseUp(Vector3 position) { }

        public virtual void OnDrawGizmos() { }

        protected Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        protected bool GetMouseWorldPosition(out Vector3 position)
        {
            position = Vector3.zero;
            Ray ray = GetMouseRay();

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                position = manager.GetSnappedPosition(hit.point);
                return true;
            }

            return false;
        }
    }
}
