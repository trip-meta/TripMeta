using UnityEngine;
using System.Collections.Generic;

namespace TripMeta.Presentation
{
    /// <summary>
    /// 空间UI管理器 - 管理VR环境中的3D UI元素
    /// </summary>
    public class SpatialUIManager : MonoBehaviour
    {
        [Header("UI Prefabs")]
        [SerializeField] private GameObject infoPanelPrefab;
        [SerializeField] private GameObject menuPanelPrefab;
        [SerializeField] private GameObject dialogPanelPrefab;

        [Header("UI Settings")]
        [SerializeField] private float defaultDistance = 3f;
        [SerializeField] private Vector3 defaultOffset = new Vector3(0, 0, 0);
        [SerializeField] private bool faceUser = true;

        private List<SpatialPanel> activePanels = new List<SpatialPanel>();

        public static SpatialUIManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 显示信息面板
        /// </summary>
        public SpatialPanel ShowInfoPanel(string title, string content, Vector3? position = null)
        {
            if (infoPanelPrefab == null)
            {
                Debug.LogWarning("[SpatialUIManager] Info panel prefab not assigned");
                return null;
            }

            Vector3 spawnPos = position ?? GetDefaultPanelPosition();
            var panel = CreatePanel(infoPanelPrefab, spawnPos, title, content);
            activePanels.Add(panel);

            Debug.Log($"[SpatialUIManager] Info panel shown: {title}");
            return panel;
        }

        /// <summary>
        /// 显示对话框
        /// </summary>
        public SpatialPanel ShowDialog(string message, System.Action onConfirm = null, System.Action onCancel = null)
        {
            if (dialogPanelPrefab == null)
            {
                Debug.LogWarning("[SpatialUIManager] Dialog prefab not assigned");
                return null;
            }

            Vector3 spawnPos = GetDefaultPanelPosition();
            var panel = CreatePanel(dialogPanelPrefab, spawnPos, "确认", message);

            // TODO: Hook up confirm/cancel buttons
            activePanels.Add(panel);

            return panel;
        }

        /// <summary>
        /// 显示菜单
        /// </summary>
        public SpatialPanel ShowMenu(List<MenuItem> items, Vector3? position = null)
        {
            if (menuPanelPrefab == null)
            {
                Debug.LogWarning("[SpatialUIManager] Menu prefab not assigned");
                return null;
            }

            Vector3 spawnPos = position ?? GetDefaultPanelPosition();
            var panel = CreatePanel(menuPanelPrefab, spawnPos, "Menu", "");
            activePanels.Add(panel);

            // TODO: Populate menu items

            return panel;
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void ClosePanel(SpatialPanel panel)
        {
            if (panel != null && activePanels.Contains(panel))
            {
                activePanels.Remove(panel);
                Destroy(panel.gameObject);
            }
        }

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAllPanels()
        {
            foreach (var panel in activePanels)
            {
                if (panel != null)
                {
                    Destroy(panel.gameObject);
                }
            }
            activePanels.Clear();
        }

        private SpatialPanel CreatePanel(GameObject prefab, Vector3 position, string title, string content)
        {
            var panelObj = Instantiate(prefab, position, Quaternion.identity);
            panelObj.transform.SetParent(transform);

            // 面向用户
            if (faceUser)
            {
                panelObj.transform.LookAt(Camera.main.transform);
                panelObj.transform.Rotate(0, 180, 0); // Flip to face correctly
            }

            var panel = panelObj.GetComponent<SpatialPanel>();
            if (panel != null)
            {
                panel.SetTitle(title);
                panel.SetContent(content);
            }

            return panel;
        }

        private Vector3 GetDefaultPanelPosition()
        {
            if (Camera.main != null)
            {
                return Camera.main.transform.position + Camera.main.transform.forward * defaultDistance + defaultOffset;
            }
            return Vector3.forward * defaultDistance;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    /// <summary>
    /// 空间面板基类
    /// </summary>
    public class SpatialPanel : MonoBehaviour
    {
        [SerializeField] protected UnityEngine.UI.Text titleText;
        [SerializeField] protected UnityEngine.UI.Text contentText;

        public virtual void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }

        public virtual void SetContent(string content)
        {
            if (contentText != null)
                contentText.text = content;
        }

        public virtual void Close()
        {
            SpatialUIManager.Instance?.ClosePanel(this);
        }
    }

    /// <summary>
    /// 菜单项
    /// </summary>
    [System.Serializable]
    public class MenuItem
    {
        public string label;
        public System.Action onClick;
        public string icon;
        public bool isEnabled = true;
    }
}
