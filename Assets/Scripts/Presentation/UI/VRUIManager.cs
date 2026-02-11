using UnityEngine;
using UnityEngine.UIElements;
using TripMeta.Core.DependencyInjection;

namespace TripMeta.Presentation.UI
{
    /// <summary>
    /// VR用户界面管理器
    /// </summary>
    public class VRUIManager : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private Transform vrCamera;
        
        private VisualElement rootElement;
        
        private void Start()
        {
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not assigned!");
                return;
            }
            
            rootElement = uiDocument.rootVisualElement;
            SetupVRInteractions();
        }
        
        private void SetupVRInteractions()
        {
            // 设置VR交互
            rootElement.RegisterCallback<ClickEvent>(OnUIClick);
            rootElement.RegisterCallback<PointerEnterEvent>(OnUIHover);
        }
        
        private void OnUIClick(ClickEvent evt)
        {
            // 处理VR UI点击
            Debug.Log($"VR UI clicked: {evt.target}");
        }
        
        private void OnUIHover(PointerEnterEvent evt)
        {
            // 处理VR UI悬停
            Debug.Log($"VR UI hovered: {evt.target}");
        }
        
        public void ShowPanel(string panelName)
        {
            var panel = rootElement.Q(panelName);
            panel?.RemoveFromClassList("hidden");
        }
        
        public void HidePanel(string panelName)
        {
            var panel = rootElement.Q(panelName);
            panel?.AddToClassList("hidden");
        }
    }
}