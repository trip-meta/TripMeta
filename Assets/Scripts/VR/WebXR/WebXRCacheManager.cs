using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.VR.WebXR
{
    /// <summary>
    /// WebXR 缓存管理器
    /// 资源缓存、压缩、版本控制
    /// </summary>
    public class WebXRCacheManager : MonoBehaviour
    {
        [Header("缓存配置")]
        public bool enableCaching = true;
        public bool enableCompression = true;
        public int maxCacheSizeMB = 100;
        public int maxCacheEntries = 500;
        public float cacheExpirationDays = 7f;

        [Header("预加载")]
        public string[] preloadAssets;
        public bool preloadOnStart = true;

        // 缓存存储
        private Dictionary<string, CacheEntry> memoryCache = new Dictionary<string, CacheEntry>();
        private long currentCacheSize = 0;
        private readonly object cacheLock = new object();

        public int CachedItemCount => memoryCache.Count;
        public long CurrentCacheSizeBytes => currentCacheSize;

        public void Initialize()
        {
            Debug.Log("[WebXRCacheManager] 初始化缓存管理器");

            if (!enableCaching)
            {
                Debug.Log("[WebXRCacheManager] 缓存已禁用");
                return;
            }

            LoadCacheFromStorage();

            if (preloadOnStart)
            {
                _ = PreloadAssetsAsync();
            }
        }

        /// <summary>
        /// 从存储加载缓存
        /// </summary>
        private void LoadCacheFromStorage()
        {
            try
            {
                string cachePath = GetCacheFilePath();
                if (File.Exists(cachePath))
                {
                    string json = File.ReadAllText(cachePath);
                    var cacheData = JsonUtility.FromJson<CacheData>(json);
                    if (cacheData != null && cacheData.entries != null)                    {
                        foreach (var entry in cacheData.entries)
                        {
                            if (entry.expirationTime > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                            {
                                memoryCache[entry.key] = entry;
                                currentCacheSize += entry.data?.Length ?? 0;
                            }
                        }
                    }
                    Debug.Log($"[WebXRCacheManager] 从存储加载 {memoryCache.Count} 个缓存项");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebXRCacheManager] 加载缓存失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存缓存到存储
        /// </summary>
        private async Task SaveCacheToStorageAsync()
        {
            try
            {
                var cacheData = new CacheData
                {
                    entries = new List<CacheEntry>(memoryCache.Values).ToArray()
                };

                string json = JsonUtility.ToJson(cacheData);
                string cachePath = GetCacheFilePath();

                await File.WriteAllTextAsync(cachePath, json);
                Debug.Log("[WebXRCacheManager] 缓存已保存到存储");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebXRCacheManager] 保存缓存失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取缓存文件路径
        /// </summary>
        private string GetCacheFilePath()
        {
            return Path.Combine(Application.persistentDataPath, "webxr_cache.json");
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        private async Task PreloadAssetsAsync()
        {
            if (preloadAssets == null || preloadAssets.Length == 0) return;

            Debug.Log($"[WebXRCacheManager] 开始预加载 {preloadAssets.Length} 个资源");

            foreach (var assetPath in preloadAssets)
            {
                await CacheAssetAsync(assetPath);
            }

            Debug.Log("[WebXRCacheManager] 资源预加载完成");
        }

        /// <summary>
        /// 缓存资源
        /// </summary>
        public async Task<bool> CacheAssetAsync(string key, byte[] data = null)
        {
            if (!enableCaching) return false;

            lock (cacheLock)
            {
                // 检查是否已缓存
                if (memoryCache.ContainsKey(key))
                {
                    return true;
                }
            }

            try
            {
                // 如果没有提供数据，尝试加载
                if (data == null)
                {
                    data = await LoadAssetDataAsync(key);
                    if (data == null) return false;
                }

                // 压缩数据
                if (enableCompression)
                {
                    data = CompressData(data);
                }

                // 检查缓存大小限制
                if (!CanAddToCache(data.Length))
                {
                    await EvictCacheAsync(data.Length);
                }

                // 添加到缓存
                var entry = new CacheEntry
                {
                    key = key,
                    data = data,
                    isCompressed = enableCompression,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    expirationTime = DateTimeOffset.UtcNow.AddDays(cacheExpirationDays).ToUnixTimeSeconds(),
                    size = data.Length
                };

                lock (cacheLock)
                {
                    memoryCache[key] = entry;
                    currentCacheSize += data.Length;
                }

                Debug.Log($"[WebXRCacheManager] 资源已缓存: {key} ({data.Length} bytes)");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebXRCacheManager] 缓存资源失败 {key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从缓存获取资源
        /// </summary>
        public byte[] GetFromCache(string key)
        {
            if (!enableCaching) return null;

            lock (cacheLock)
            {
                if (memoryCache.TryGetValue(key, out CacheEntry entry))
                {
                    // 检查是否过期
                    if (entry.expirationTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {
                        memoryCache.Remove(key);
                        currentCacheSize -= entry.size;
                        return null;
                    }

                    byte[] data = entry.data;

                    // 解压数据
                    if (entry.isCompressed)
                    {
                        data = DecompressData(data);
                    }

                    // 更新访问时间
                    entry.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    return data;
                }
            }

            return null;
        }

        /// <summary>
        /// 检查缓存中是否存在
        /// </summary>
        public bool IsCached(string key)
        {
            lock (cacheLock)
            {
                return memoryCache.ContainsKey(key);
            }
        }

        /// <summary>
        /// 从缓存移除
        /// </summary>
        public void RemoveFromCache(string key)
        {
            lock (cacheLock)
            {
                if (memoryCache.TryGetValue(key, out CacheEntry entry))
                {
                    memoryCache.Remove(key);
                    currentCacheSize -= entry.size;
                    Debug.Log($"[WebXRCacheManager] 从缓存移除: {key}");
                }
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public async Task ClearCacheAsync()
        {
            lock (cacheLock)
            {
                memoryCache.Clear();
                currentCacheSize = 0;
            }

            string cachePath = GetCacheFilePath();
            if (File.Exists(cachePath))
            {
                File.Delete(cachePath);
            }

            Debug.Log("[WebXRCacheManager] 缓存已清空");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 加载资源数据
        /// </summary>
        private async Task<byte[]> LoadAssetDataAsync(string path)
        {
            try
            {
                using var request = UnityEngine.Networking.UnityWebRequest.Get(path);
                await request.SendWebRequest().AsTask();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.data;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebXRCacheManager] 加载资源失败 {path}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        private byte[] CompressData(byte[] data)
        {
            try
            {
                using var output = new MemoryStream();
                using (var gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        private byte[] DecompressData(byte[] data)
        {
            try
            {
                using var input = new MemoryStream(data);
                using var output = new MemoryStream();
                using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                {
                    gzip.CopyTo(output);
                }
                return output.ToArray();
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// 检查是否可以添加缓存
        /// </summary>
        private bool CanAddToCache(int size)
        {
            long maxSize = maxCacheSizeMB * 1024 * 1024L;
            return currentCacheSize + size <= maxSize && memoryCache.Count < maxCacheEntries;
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        private async Task EvictCacheAsync(long requiredSpace)
        {
            lock (cacheLock)
            {
                // LRU 淘汰策略：移除最旧的条目
                var entries = new List<CacheEntry>(memoryCache.Values);
                entries.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));

                foreach (var entry in entries)
                {
                    if (currentCacheSize + requiredSpace <= maxCacheSizeMB * 1024 * 1024L)
                        break;

                    memoryCache.Remove(entry.key);
                    currentCacheSize -= entry.size;
                }
            }

            await Task.CompletedTask;
        }

        void OnApplicationQuit()
        {
            _ = SaveCacheToStorageAsync();
        }
    }

    #region 数据类型

    [Serializable]
    public class CacheEntry
    {
        public string key;
        public byte[] data;
        public bool isCompressed;
        public long timestamp;
        public long expirationTime;
        public long size;
    }

    [Serializable]
    public class CacheData
    {
        public CacheEntry[] entries;
    }

    #endregion
}
