using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace TripMeta.UGC
{
    /// <summary>
    /// 场景编辑器管理器
    /// UGC创作工具的核心管理器，提供可视化场景编辑功能
    /// </summary>
    public class SceneEditorManager : MonoBehaviour
    {
        [Header("编辑器配置")]
        public bool enableAutoSave = true;
        public float autoSaveInterval = 30f;
        public int maxUndoSteps = 50;
        public string scenesSavePath = "UserScenes/";

        [Header("编辑模式")]
        public EditMode currentEditMode = EditMode.Select;
        public SnapMode snapMode = SnapMode.Grid;
        public float snapGridSize = 1f;
        public float snapAngle = 15f;

        [Header("预览设置")]
        public bool enableRealtimePreview = true;
        public Material previewMaterial;
        public LayerMask placementLayer = ~0;

        // 当前编辑的场景
        private EditableScene currentScene;
        private List<EditableScene> sceneHistory = new List<EditableScene>();
        private int currentHistoryIndex = -1;
        private float lastAutoSaveTime;

        // 选中的对象
        private List<SceneObject> selectedObjects = new List<SceneObject>();
        private SceneObject hoveredObject;

        // 工具状态
        private Dictionary<ToolType, IEditorTool> tools = new Dictionary<ToolType, IEditorTool>();
        private IEditorTool currentTool;

        // 事件
        public event Action<EditableScene> OnSceneLoaded;
        public event Action OnSceneModified;
        public event Action<List<SceneObject>> OnSelectionChanged;
        public event Action<EditMode> OnEditModeChanged;
        public event Action<string> OnAutoSave;

        public static SceneEditorManager Instance { get; private set; }

        public EditableScene CurrentScene => currentScene;
        public bool HasUnsavedChanges { get; private set; }
        public IReadOnlyList<SceneObject> SelectedObjects => selectedObjects;
        public SceneObject HoveredObject => hoveredObject;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            HandleInput();

            if (enableAutoSave && Time.time - lastAutoSaveTime > autoSaveInterval)
            {
                AutoSave();
            }

            currentTool?.Update();
        }

        /// <summary>
        /// 初始化编辑工具
        /// </summary>
        private void InitializeTools()
        {
            tools[ToolType.Select] = new SelectionTool(this);
            tools[ToolType.Move] = new MoveTool(this);
            tools[ToolType.Rotate] = new RotateTool(this);
            tools[ToolType.Scale] = new ScaleTool(this);
            tools[ToolType.Place] = new PlacementTool(this);
            tools[ToolType.Terrain] = new TerrainEditTool(this);
            tools[ToolType.Paint] = new PaintTool(this);

            SetTool(ToolType.Select);
        }

        /// <summary>
        /// 创建新场景
        /// </summary>
        public EditableScene CreateNewScene(string sceneName, Vector3 sceneSize)
        {
            var scene = new EditableScene
            {
                sceneId = Guid.NewGuid().ToString(),
                name = sceneName,
                size = sceneSize,
                createdAt = DateTime.Now,
                modifiedAt = DateTime.Now,
                objects = new List<SceneObject>(),
                terrainData = new TerrainData(),
                lightingSettings = new LightingSettings(),
                postProcessSettings = new PostProcessSettings()
            };

            LoadScene(scene);
            return scene;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene(EditableScene scene)
        {
            ClearCurrentScene();
            currentScene = scene;
            InstantiateSceneObjects(scene);
            ClearHistory();
            HasUnsavedChanges = false;
            OnSceneLoaded?.Invoke(scene);
        }

        /// <summary>
        /// 实例化场景对象
        /// </summary>
        private void InstantiateSceneObjects(EditableScene scene)
        {
            foreach (var obj in scene.objects)
            {
                obj.InstantiateInWorld();
            }

            // 应用地形
            if (scene.terrainData != null)
            {
                ApplyTerrainData(scene.terrainData);
            }
        }

        /// <summary>
        /// 应用地形数据
        /// </summary>
        private void ApplyTerrainData(TerrainData terrainData)
        {
            var terrain = FindObjectOfType<Terrain>();
            if (terrain != null && terrainData.heightmap != null)
            {
                terrain.terrainData.SetHeights(0, 0, terrainData.heightmap);
                terrain.terrainData.size = terrainData.size;
            }
        }

        /// <summary>
        /// 清空当前场景
        /// </summary>
        public void ClearCurrentScene()
        {
            if (currentScene != null)
            {
                foreach (var obj in currentScene.objects)
                {
                    obj.DestroyInstance();
                }
            }

            selectedObjects.Clear();
            currentScene = null;
        }

        /// <summary>
        /// 添加对象到场景
        /// </summary>
        public SceneObject AddObject(string prefabId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (currentScene == null) return null;

            var sceneObject = new SceneObject
            {
                objectId = Guid.NewGuid().ToString(),
                prefabId = prefabId,
                position = position,
                rotation = rotation,
                scale = scale,
                isVisible = true,
                isLocked = false
            };

            sceneObject.InstantiateInWorld();
            currentScene.objects.Add(sceneObject);
            currentScene.modifiedAt = DateTime.Now;

            RecordHistory();
            HasUnsavedChanges = true;
            OnSceneModified?.Invoke();

            return sceneObject;
        }

        /// <summary>
        /// 删除选中的对象
        /// </summary>
        public void DeleteSelectedObjects()
        {
            if (selectedObjects.Count == 0) return;

            foreach (var obj in selectedObjects)
            {
                currentScene?.objects.Remove(obj);
                obj.DestroyInstance();
            }

            selectedObjects.Clear();
            OnSelectionChanged?.Invoke(selectedObjects);

            RecordHistory();
            HasUnsavedChanges = true;
            OnSceneModified?.Invoke();
        }

        /// <summary>
        /// 选择对象
        /// </summary>
        public void SelectObject(SceneObject obj, bool additive = false)
        {
            if (!additive)
            {
                foreach (var selected in selectedObjects)
                {
                    selected.SetSelected(false);
                }
                selectedObjects.Clear();
            }

            if (obj != null && !selectedObjects.Contains(obj))
            {
                selectedObjects.Add(obj);
                obj.SetSelected(true);
            }

            OnSelectionChanged?.Invoke(selectedObjects);
        }

        /// <summary>
        /// 设置编辑工具
        /// </summary>
        public void SetTool(ToolType toolType)
        {
            currentTool?.Deactivate();
            currentTool = tools.GetValueOrDefault(toolType);
            currentTool?.Activate();
        }

        /// <summary>
        /// 设置编辑模式
        /// </summary>
        public void SetEditMode(EditMode mode)
        {
            currentEditMode = mode;
            OnEditModeChanged?.Invoke(mode);
        }

        /// <summary>
        /// 保存场景
        /// </summary>
        public bool SaveScene(string path = null)
        {
            if (currentScene == null) return false;

            path ??= $"{scenesSavePath}{currentScene.sceneId}.json";

            try
            {
                var json = JsonUtility.ToJson(currentScene, true);
                System.IO.File.WriteAllText(path, json);
                currentScene.modifiedAt = DateTime.Now;
                HasUnsavedChanges = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneEditorManager] 保存场景失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 自动保存
        /// </summary>
        private void AutoSave()
        {
            if (!HasUnsavedChanges || currentScene == null) return;

            var autoSavePath = $"{scenesSavePath}autosave/{currentScene.sceneId}.json";
            if (SaveScene(autoSavePath))
            {
                lastAutoSaveTime = Time.time;
                OnAutoSave?.Invoke(autoSavePath);
            }
        }

        /// <summary>
        /// 撤销
        /// </summary>
        public void Undo()
        {
            if (currentHistoryIndex <= 0) return;

            currentHistoryIndex--;
            RestoreFromHistory();
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void Redo()
        {
            if (currentHistoryIndex >= sceneHistory.Count - 1) return;

            currentHistoryIndex++;
            RestoreFromHistory();
        }

        /// <summary>
        /// 记录历史
        /// </summary>
        private void RecordHistory()
        {
            // 删除当前位置之后的历史
            while (sceneHistory.Count > currentHistoryIndex + 1)
            {
                sceneHistory.RemoveAt(sceneHistory.Count - 1);
            }

            // 添加新历史
            var snapshot = currentScene?.Clone();
            if (snapshot != null)
            {
                sceneHistory.Add(snapshot);

                // 限制历史数量
                while (sceneHistory.Count > maxUndoSteps)
                {
                    sceneHistory.RemoveAt(0);
                }

                currentHistoryIndex = sceneHistory.Count - 1;
            }
        }

        /// <summary>
        /// 从历史恢复
        /// </summary>
        private void RestoreFromHistory()
        {
            if (currentHistoryIndex < 0 || currentHistoryIndex >= sceneHistory.Count) return;

            var snapshot = sceneHistory[currentHistoryIndex];
            LoadScene(snapshot.Clone());
        }

        /// <summary>
        /// 清空历史
        /// </summary>
        private void ClearHistory()
        {
            sceneHistory.Clear();
            currentHistoryIndex = -1;
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            // 快捷键
            if (Input.GetKeyDown(KeyCode.Q)) SetTool(ToolType.Select);
            if (Input.GetKeyDown(KeyCode.W)) SetTool(ToolType.Move);
            if (Input.GetKeyDown(KeyCode.E)) SetTool(ToolType.Rotate);
            if (Input.GetKeyDown(KeyCode.R)) SetTool(ToolType.Scale);
            if (Input.GetKeyDown(KeyCode.T)) SetTool(ToolType.Place);

            // 撤销/重做
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.Z)) Undo();
                if (Input.GetKeyDown(KeyCode.Y)) Redo();
                if (Input.GetKeyDown(KeyCode.S)) SaveScene();
            }

            // 删除
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelectedObjects();
            }
        }

        /// <summary>
        /// 获取捕捉位置
        /// </summary>
        public Vector3 GetSnappedPosition(Vector3 position)
        {
            if (snapMode == SnapMode.None) return position;

            return new Vector3(
                Mathf.Round(position.x / snapGridSize) * snapGridSize,
                Mathf.Round(position.y / snapGridSize) * snapGridSize,
                Mathf.Round(position.z / snapGridSize) * snapGridSize
            );
        }

        /// <summary>
        /// 获取捕捉旋转
        /// </summary>
        public Quaternion GetSnappedRotation(Quaternion rotation)
        {
            if (snapMode == SnapMode.None) return rotation;

            var euler = rotation.eulerAngles;
            euler.x = Mathf.Round(euler.x / snapAngle) * snapAngle;
            euler.y = Mathf.Round(euler.y / snapAngle) * snapAngle;
            euler.z = Mathf.Round(euler.z / snapAngle) * snapAngle;
            return Quaternion.Euler(euler);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 可编辑场景
    /// </summary>
    [Serializable]
    public class EditableScene
    {
        public string sceneId;
        public string name;
        public string description;
        public Vector3 size;
        public DateTime createdAt;
        public DateTime modifiedAt;
        public List<SceneObject> objects;
        public TerrainData terrainData;
        public LightingSettings lightingSettings;
        public PostProcessSettings postProcessSettings;
        public string thumbnailPath;
        public List<string> tags;
        public string authorId;
        public bool isPublished;

        public EditableScene Clone()
        {
            return new EditableScene
            {
                sceneId = sceneId,
                name = name,
                description = description,
                size = size,
                createdAt = createdAt,
                modifiedAt = modifiedAt,
                objects = objects?.Select(o => o.Clone()).ToList(),
                terrainData = terrainData?.Clone(),
                lightingSettings = lightingSettings,
                postProcessSettings = postProcessSettings,
                thumbnailPath = thumbnailPath,
                tags = tags?.ToList(),
                authorId = authorId,
                isPublished = isPublished
            };
        }
    }

    /// <summary>
    /// 场景对象
    /// </summary>
    [Serializable]
    public class SceneObject
    {
        public string objectId;
        public string prefabId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public bool isVisible;
        public bool isLocked;
        public Dictionary<string, string> customProperties;

        [NonSerialized]
        private GameObject instance;

        [NonSerialized]
        private bool isSelected;

        public GameObject Instance => instance;
        public bool IsSelected => isSelected;

        public void InstantiateInWorld()
        {
            // 这里会从资源管理器加载预制体并实例化
            // 简化实现：创建空物体
            instance = new GameObject($"SceneObject_{objectId}");
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;
        }

        public void DestroyInstance()
        {
            if (instance != null)
            {
                Destroy(instance);
                instance = null;
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            // 可以在这里添加选中视觉效果
        }

        public void UpdateTransform()
        {
            if (instance != null)
            {
                position = instance.transform.position;
                rotation = instance.transform.rotation;
                scale = instance.transform.localScale;
            }
        }

        public SceneObject Clone()
        {
            return new SceneObject
            {
                objectId = objectId,
                prefabId = prefabId,
                position = position,
                rotation = rotation,
                scale = scale,
                isVisible = isVisible,
                isLocked = isLocked,
                customProperties = customProperties?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }
    }

    /// <summary>
    /// 地形数据
    /// </summary>
    [Serializable]
    public class TerrainData
    {
        public Vector3 size;
        public float[,] heightmap;
        public Texture2D splatmap;
        public List<Texture2D> layerTextures;

        public TerrainData Clone()
        {
            var clone = new TerrainData
            {
                size = size,
                splatmap = splatmap,
                layerTextures = layerTextures?.ToList()
            };

            if (heightmap != null)
            {
                clone.heightmap = (float[,])heightmap.Clone();
            }

            return clone;
        }
    }

    /// <summary>
    /// 光照设置
    /// </summary>
    [Serializable]
    public class LightingSettings
    {
        public float ambientIntensity = 1f;
        public Color ambientColor = Color.gray;
        public bool enableShadows = true;
        public float shadowDistance = 100f;
    }

    /// <summary>
    /// 后处理设置
    /// </summary>
    [Serializable]
    public class PostProcessSettings
    {
        public bool enableBloom = true;
        public float bloomIntensity = 0.5f;
        public bool enableToneMapping = true;
        public bool enableAmbientOcclusion = true;
        public float aoIntensity = 0.5f;
    }

    /// <summary>
    /// 编辑模式
    /// </summary>
    public enum EditMode
    {
        Select,
        Object,
        Terrain,
        Lighting,
        Audio
    }

    /// <summary>
    /// 工具类型
    /// </summary>
    public enum ToolType
    {
        Select,
        Move,
        Rotate,
        Scale,
        Place,
        Terrain,
        Paint
    }

    /// <summary>
    /// 捕捉模式
    /// </summary>
    public enum SnapMode
    {
        None,
        Grid,
        Surface
    }

    #endregion
}
