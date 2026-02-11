using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.Infrastructure.Resources
{
    /// <summary>
    /// 统一资源管理器
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        private Dictionary<string, AsyncOperationHandle> loadedAssets = new Dictionary<string, AsyncOperationHandle>();
        private Dictionary<Type, Queue<GameObject>> objectPools = new Dictionary<Type, Queue<GameObject>>();
        
        public async Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            try
            {
                if (loadedAssets.ContainsKey(address))
                {
                    var existingHandle = loadedAssets[address];
                    if (existingHandle.IsValid() && existingHandle.IsDone)
                    {
                        return existingHandle.Result as T;
                    }
                }
                
                Logger.LogInfo($"Loading asset: {address}", "RESOURCE");
                
                var handle = Addressables.LoadAssetAsync<T>(address);
                loadedAssets[address] = handle;
                
                var result = await handle.Task;
                
                Logger.LogInfo($"Asset loaded successfully: {address}", "RESOURCE");
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Failed to load asset: {address}");
                return null;
            }
        }
        
        public async Task<GameObject> InstantiateAsync(string address, Transform parent = null)
        {
            try
            {
                Logger.LogInfo($"Instantiating object: {address}", "RESOURCE");
                
                var handle = Addressables.InstantiateAsync(address, parent);
                var result = await handle.Task;
                
                // 存储句柄以便后续释放
                string instanceKey = $"{address}_{result.GetInstanceID()}";
                loadedAssets[instanceKey] = handle;
                
                Logger.LogInfo($"Object instantiated successfully: {address}", "RESOURCE");
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Failed to instantiate object: {address}");
                return null;
            }
        }
        
        public void ReleaseAsset(string address)
        {
            if (loadedAssets.TryGetValue(address, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                    Logger.LogInfo($"Asset released: {address}", "RESOURCE");
                }
                loadedAssets.Remove(address);
            }
        }
        
        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null) return;
            
            string instanceKey = null;
            foreach (var kvp in loadedAssets)
            {
                if (kvp.Key.EndsWith($"_{instance.GetInstanceID()}"))
                {
                    instanceKey = kvp.Key;
                    break;
                }
            }
            
            if (!string.IsNullOrEmpty(instanceKey))
            {
                var handle = loadedAssets[instanceKey];
                if (handle.IsValid())
                {
                    Addressables.ReleaseInstance(instance);
                    Logger.LogInfo($"Instance released: {instanceKey}", "RESOURCE");
                }
                loadedAssets.Remove(instanceKey);
            }
            else
            {
                // 如果不是通过Addressables创建的，直接销毁
                Destroy(instance);
            }
        }
        
        public T GetPooledObject<T>() where T : Component
        {
            var type = typeof(T);
            if (objectPools.TryGetValue(type, out var pool) && pool.Count > 0)
            {
                var pooledObject = pool.Dequeue();
                pooledObject.SetActive(true);
                return pooledObject.GetComponent<T>();
            }
            
            return null;
        }
        
        public void ReturnToPool<T>(T component) where T : Component
        {
            if (component == null) return;
            
            var type = typeof(T);
            if (!objectPools.ContainsKey(type))
            {
                objectPools[type] = new Queue<GameObject>();
            }
            
            component.gameObject.SetActive(false);
            objectPools[type].Enqueue(component.gameObject);
        }
        
        public void PrewarmPool<T>(T prefab, int count) where T : Component
        {
            var type = typeof(T);
            if (!objectPools.ContainsKey(type))
            {
                objectPools[type] = new Queue<GameObject>();
            }
            
            for (int i = 0; i < count; i++)
            {
                var instance = Instantiate(prefab.gameObject);
                instance.SetActive(false);
                objectPools[type].Enqueue(instance);
            }
            
            Logger.LogInfo($"Pool prewarmed for {type.Name}: {count} objects", "RESOURCE");
        }
        
        private void OnDestroy()
        {
            // 清理所有加载的资源
            foreach (var kvp in loadedAssets)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }
            loadedAssets.Clear();
            
            // 清理对象池
            foreach (var pool in objectPools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            objectPools.Clear();
            
            Logger.LogInfo("ResourceManager cleaned up", "RESOURCE");
        }
    }
}