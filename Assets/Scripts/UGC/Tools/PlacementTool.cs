using UnityEngine;

namespace TripMeta.UGC
{
    /// <summary>
    /// 对象放置工具
    /// 允许用户在场景中放置预制体对象
    /// </summary>
    public class PlacementTool : BaseEditorTool
    {
        private string selectedPrefabId;
        private GameObject previewInstance;
        private Material previewMaterial;
        private bool isValidPlacement;

        public string SelectedPrefabId
        {
            get => selectedPrefabId;
            set
            {
                selectedPrefabId = value;
                UpdatePreview();
            }
        }

        public PlacementTool(SceneEditorManager manager) : base(manager) { }

        public override void Activate()
        {
            base.Activate();
            CreatePreviewMaterial();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            DestroyPreview();
        }

        public override void Update()
        {
            if (!isActive || string.IsNullOrEmpty(selectedPrefabId)) return;

            UpdatePreviewPosition();

            if (Input.GetMouseButtonDown(0) && isValidPlacement)
            {
                PlaceObject();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RotatePreview();
            }
        }

        /// <summary>
        /// 创建预览材质
        /// </summary>
        private void CreatePreviewMaterial()
        {
            previewMaterial = new Material(Shader.Find("Standard"));
            previewMaterial.SetFloat("_Mode", 3); // Transparent
            previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            previewMaterial.SetInt("_ZWrite", 0);
            previewMaterial.DisableKeyword("_ALPHATEST_ON");
            previewMaterial.EnableKeyword("_ALPHABLEND_ON");
            previewMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            previewMaterial.renderQueue = 3000;
        }

        /// <summary>
        /// 更新预览
        /// </summary>
        private void UpdatePreview()
        {
            DestroyPreview();

            if (string.IsNullOrEmpty(selectedPrefabId)) return;

            // 从资源管理器加载预制体
            var prefab = Resources.Load<GameObject>($"Prefabs/{selectedPrefabId}");
            if (prefab != null)
            {
                previewInstance = Object.Instantiate(prefab);
                SetPreviewMaterial(previewInstance);
            }
        }

        /// <summary>
        /// 设置预览材质
        /// </summary>
        private void SetPreviewMaterial(GameObject obj)
        {
            if (previewMaterial == null) return;

            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = previewMaterial;
            }
        }

        /// <summary>
        /// 更新预览位置
        /// </summary>
        private void UpdatePreviewPosition()
        {
            if (previewInstance == null) return;

            if (GetMouseWorldPosition(out Vector3 position))
            {
                previewInstance.transform.position = position;
                isValidPlacement = true;

                // 更新材质颜色表示有效性
                previewMaterial.color = new Color(0, 1, 0, 0.5f);
            }
            else
            {
                isValidPlacement = false;
                previewMaterial.color = new Color(1, 0, 0, 0.5f);
            }
        }

        /// <summary>
        /// 旋转预览
        /// </summary>
        private void RotatePreview()
        {
            if (previewInstance == null) return;

            previewInstance.transform.Rotate(Vector3.up, 15f);
        }

        /// <summary>
        /// 放置对象
        /// </summary>
        private void PlaceObject()
        {
            if (previewInstance == null) return;

            Vector3 position = previewInstance.transform.position;
            Quaternion rotation = previewInstance.transform.rotation;
            Vector3 scale = previewInstance.transform.localScale;

            manager.AddObject(selectedPrefabId, position, rotation, scale);
        }

        /// <summary>
        /// 销毁预览
        /// </summary>
        private void DestroyPreview()
        {
            if (previewInstance != null)
            {
                Object.Destroy(previewInstance);
                previewInstance = null;
            }
        }
    }
}
