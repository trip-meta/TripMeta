using UnityEngine;

namespace TripMeta.VR.Interaction
{
    /// <summary>
    /// VR交互接口 - 定义VR交互对象的标准行为
    /// </summary>
    public interface IVRInteractable
    {
        /// <summary>
        /// 是否支持凝视交互
        /// </summary>
        bool SupportsGazeInteraction { get; }
        
        /// <summary>
        /// 是否可以被抓取
        /// </summary>
        bool IsGrabbable { get; }
        
        /// <summary>
        /// 交互优先级
        /// </summary>
        int InteractionPriority { get; }
        
        /// <summary>
        /// 悬停进入
        /// </summary>
        void OnHoverEnter();
        
        /// <summary>
        /// 悬停更新
        /// </summary>
        /// <param name="hitPoint">碰撞点</param>
        /// <param name="hitNormal">碰撞法线</param>
        void OnHoverUpdate(Vector3 hitPoint, Vector3 hitNormal);
        
        /// <summary>
        /// 悬停退出
        /// </summary>
        void OnHoverExit();
        
        /// <summary>
        /// 选中
        /// </summary>
        void OnSelect();
        
        /// <summary>
        /// 取消选中
        /// </summary>
        void OnDeselect();
        
        /// <summary>
        /// 激活/使用
        /// </summary>
        void OnActivate();
        
        /// <summary>
        /// 抓取
        /// </summary>
        void OnGrab();
        
        /// <summary>
        /// 释放
        /// </summary>
        void OnRelease();
        
        /// <summary>
        /// 凝视进入
        /// </summary>
        void OnGazeEnter();
        
        /// <summary>
        /// 凝视退出
        /// </summary>
        void OnGazeExit();
        
        /// <summary>
        /// 交互更新
        /// </summary>
        void OnInteractionUpdate();
    }
    
    /// <summary>
    /// 手势类型
    /// </summary>
    public enum GestureType
    {
        None,           // 无手势
        Point,          // 指向
        Grab,           // 抓取
        Pinch,          // 捏取
        Wave,           // 挥手
        ThumbsUp,       // 点赞
        Peace,          // 比V
        Fist,           // 握拳
        OpenPalm,       // 张开手掌
        Custom          // 自定义手势
    }
    
    /// <summary>
    /// VR交互事件参数
    /// </summary>
    public class VRInteractionEventArgs
    {
        public IVRInteractable interactable;
        public Vector3 interactionPoint;
        public Vector3 interactionNormal;
        public float interactionDistance;
        public UnityEngine.XR.XRNode controllerNode;
        
        public VRInteractionEventArgs(IVRInteractable interactable, Vector3 point, Vector3 normal, float distance, UnityEngine.XR.XRNode node)
        {
            this.interactable = interactable;
            this.interactionPoint = point;
            this.interactionNormal = normal;
            this.interactionDistance = distance;
            this.controllerNode = node;
        }
    }
}