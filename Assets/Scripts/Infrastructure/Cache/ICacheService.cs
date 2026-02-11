using System;
using System.Threading.Tasks;

namespace TripMeta.Infrastructure.Cache
{
    /// <summary>
    /// 缓存服务接口
    /// </summary>
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task ClearAsync();
        Task<bool> ExistsAsync(string key);
    }

    public enum CacheType
    {
        Memory,
        Disk,
        Hybrid
    }
}