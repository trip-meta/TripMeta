using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TripMeta.SDK
{
    /// <summary>
    /// 插件管理器
    /// 管理第三方插件的加载、初始化和生命周期
    /// </summary>
    public class PluginManager : MonoBehaviour
    {
        [Header("插件配置")]
        public string pluginsDirectory = "Plugins/";
        public bool enableHotReload = true;
        public bool sandboxPlugins = true;
        public int maxPluginMemoryMB = 512;

        [Header("安全设置")]
        public bool verifyPluginSignatures = true;
        public List<string> trustedPublishers = new List<string>();
        public List<string> blockedPlugins = new List<string>();

        // 已加载的插件
        private Dictionary<string, LoadedPlugin> loadedPlugins = new Dictionary<string, LoadedPlugin>();
        private List<PluginAPI> pluginAPIs = new List<PluginAPI>();

        // 事件
        public event Action<LoadedPlugin> OnPluginLoaded;
        public event Action<LoadedPlugin> OnPluginUnloaded;
        public event Action<string, string> OnPluginError;

        public static PluginManager Instance { get; private set; }

        public IReadOnlyDictionary<string, LoadedPlugin> LoadedPlugins => loadedPlugins;
        public int ActivePluginCount => loadedPlugins.Count(p => p.Value.isActive);

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化插件系统
        /// </summary>
        private void Initialize()
        {
            EnsurePluginsDirectory();
            LoadAllPlugins();
            Debug.Log($"[PluginManager] 插件管理器初始化完成，已加载 {loadedPlugins.Count} 个插件");
        }

        /// <summary>
        /// 确保插件目录存在
        /// </summary>
        private void EnsurePluginsDirectory()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, pluginsDirectory);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        /// <summary>
        /// 加载所有插件
        /// </summary>
        private void LoadAllPlugins()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, pluginsDirectory);
            if (!Directory.Exists(fullPath)) return;

            // 查找所有插件清单文件
            string[] manifestFiles = Directory.GetFiles(fullPath, "*.json", SearchOption.AllDirectories);

            foreach (var manifestPath in manifestFiles)
            {
                try
                {
                    LoadPluginFromManifest(manifestPath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PluginManager] 加载插件失败 {manifestPath}: {e.Message}");
                    OnPluginError?.Invoke(manifestPath, e.Message);
                }
            }
        }

        /// <summary>
        /// 从清单文件加载插件
        /// </summary>
        private void LoadPluginFromManifest(string manifestPath)
        {
            string json = File.ReadAllText(manifestPath);
            var manifest = JsonUtility.FromJson<PluginManifest>(json);

            if (manifest == null || string.IsNullOrEmpty(manifest.pluginId))
            {
                throw new Exception("Invalid plugin manifest");
            }

            // 检查是否在黑名单中
            if (blockedPlugins.Contains(manifest.pluginId))
            {
                Debug.LogWarning($"[PluginManager] 插件 {manifest.pluginId} 在黑名单中，跳过加载");
                return;
            }

            // 验证签名
            if (verifyPluginSignatures && !VerifyPluginSignature(manifest))
            {
                Debug.LogWarning($"[PluginManager] 插件 {manifest.pluginId} 签名验证失败");
                return;
            }

            string pluginDirectory = Path.GetDirectoryName(manifestPath);

            var loadedPlugin = new LoadedPlugin
            {
                manifest = manifest,
                directoryPath = pluginDirectory,
                loadTime = DateTime.Now,
                isActive = false
            };

            // 加载插件程序集
            if (!string.IsNullOrEmpty(manifest.assemblyFile))
            {
                string assemblyPath = Path.Combine(pluginDirectory, manifest.assemblyFile);
                if (File.Exists(assemblyPath))
                {
                    loadedPlugin.assembly = Assembly.LoadFrom(assemblyPath);
                }
            }

            loadedPlugins[manifest.pluginId] = loadedPlugin;

            // 如果插件标记为自动启动，则激活它
            if (manifest.autoStart)
            {
                ActivatePlugin(manifest.pluginId);
            }

            OnPluginLoaded?.Invoke(loadedPlugin);
            Debug.Log($"[PluginManager] 插件已加载: {manifest.name} v{manifest.version}");
        }

        /// <summary>
        /// 验证插件签名
        /// </summary>
        private bool VerifyPluginSignature(PluginManifest manifest)
        {
            // 简化实现：检查发布者是否在信任列表中
            if (trustedPublishers.Count == 0) return true;
            return trustedPublishers.Contains(manifest.publisher);
        }

        /// <summary>
        /// 激活插件
        /// </summary>
        public bool ActivatePlugin(string pluginId)
        {
            if (!loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                Debug.LogError($"[PluginManager] 插件未找到: {pluginId}");
                return false;
            }

            if (plugin.isActive) return true;

            try
            {
                // 创建插件实例
                if (!string.IsNullOrEmpty(plugin.manifest.entryClass))
                {
                    Type entryType = plugin.assembly?.GetType(plugin.manifest.entryClass);
                    if (entryType != null)
                    {
                        plugin.instance = Activator.CreateInstance(entryType) as ITripMetaPlugin;
                        plugin.instance?.OnEnable();
                    }
                }

                plugin.isActive = true;
                Debug.Log($"[PluginManager] 插件已激活: {plugin.manifest.name}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PluginManager] 激活插件失败 {pluginId}: {e.Message}");
                OnPluginError?.Invoke(pluginId, e.Message);
                return false;
            }
        }

        /// <summary>
        /// 停用插件
        /// </summary>
        public bool DeactivatePlugin(string pluginId)
        {
            if (!loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                return false;
            }

            if (!plugin.isActive) return true;

            try
            {
                plugin.instance?.OnDisable();
                plugin.isActive = false;
                Debug.Log($"[PluginManager] 插件已停用: {plugin.manifest.name}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PluginManager] 停用插件失败 {pluginId}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        public bool UnloadPlugin(string pluginId)
        {
            if (!loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                return false;
            }

            DeactivatePlugin(pluginId);
            loadedPlugins.Remove(pluginId);
            OnPluginUnloaded?.Invoke(plugin);

            Debug.Log($"[PluginManager] 插件已卸载: {plugin.manifest.name}");
            return true;
        }

        /// <summary>
        /// 注册插件API
        /// </summary>
        public void RegisterAPI(PluginAPI api)
        {
            if (!pluginAPIs.Any(a => a.apiName == api.apiName))
            {
                pluginAPIs.Add(api);
                Debug.Log($"[PluginManager] API已注册: {api.apiName}");
            }
        }

        /// <summary>
        /// 获取插件API
        /// </summary>
        public PluginAPI GetAPI(string apiName)
        {
            return pluginAPIs.FirstOrDefault(a => a.apiName == apiName);
        }

        /// <summary>
        /// 获取所有可用的API
        /// </summary>
        public IReadOnlyList<PluginAPI> GetAllAPIs()
        {
            return pluginAPIs;
        }

        /// <summary>
        /// 安装插件（从文件）
        /// </summary>
        public async Task<bool> InstallPlugin(string sourcePath)
        {
            try
            {
                string pluginId = Path.GetFileNameWithoutExtension(sourcePath);
                string targetPath = Path.Combine(Application.persistentDataPath, pluginsDirectory, pluginId);

                // 创建插件目录
                Directory.CreateDirectory(targetPath);

                // 复制文件
                if (Directory.Exists(sourcePath))
                {
                    CopyDirectory(sourcePath, targetPath);
                }
                else
                {
                    File.Copy(sourcePath, Path.Combine(targetPath, Path.GetFileName(sourcePath)), true);
                }

                // 加载插件
                string manifestPath = Path.Combine(targetPath, "manifest.json");
                if (File.Exists(manifestPath))
                {
                    LoadPluginFromManifest(manifestPath);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PluginManager] 安装插件失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 复制目录
        /// </summary>
        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(dir, Path.Combine(targetDir, Path.GetFileName(dir)));
            }
        }

        void OnDestroy()
        {
            // 停用所有插件
            foreach (var plugin in loadedPlugins.Values)
            {
                if (plugin.isActive)
                {
                    plugin.instance?.OnDisable();
                }
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 插件清单
    /// </summary>
    [Serializable]
    public class PluginManifest
    {
        public string pluginId;
        public string name;
        public string version;
        public string description;
        public string author;
        public string publisher;
        public string entryClass;
        public string assemblyFile;
        public bool autoStart;
        public string[] dependencies;
        public string[] permissions;
        public string[] supportedVersions;
    }

    /// <summary>
    /// 已加载的插件
    /// </summary>
    public class LoadedPlugin
    {
        public PluginManifest manifest;
        public string directoryPath;
        public Assembly assembly;
        public ITripMetaPlugin instance;
        public DateTime loadTime;
        public bool isActive;
    }

    /// <summary>
    /// 插件API定义
    /// </summary>
    public class PluginAPI
    {
        public string apiName;
        public string version;
        public string description;
        public Delegate method;
        public Type[] parameterTypes;
        public Type returnType;
    }

    /// <summary>
    /// 插件接口
    /// </summary>
    public interface ITripMetaPlugin
    {
        void OnEnable();
        void OnDisable();
        void OnUpdate();
    }

    #endregion
}
