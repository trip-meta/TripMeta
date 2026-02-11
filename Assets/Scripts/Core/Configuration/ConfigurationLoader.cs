using UnityEngine;

namespace TripMeta.Core.Configuration
{
    /// <summary>
    /// 配置加载器 - 负责加载运行时配置
    /// </summary>
    public static class ConfigurationLoader
    {
        private static TripMetaConfig _cachedConfig;
        private static AppSettings _cachedAppSettings;

        /// <summary>
        /// 加载主配置
        /// </summary>
        public static TripMetaConfig LoadTripMetaConfig()
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            // 尝试从Resources加载
            _cachedConfig = Resources.Load<TripMetaConfig>("Config/TripMetaConfig");

            if (_cachedConfig == null)
            {
                Debug.LogWarning("[ConfigurationLoader] TripMetaConfig not found, creating default instance");
                _cachedConfig = ScriptableObject.CreateInstance<TripMetaConfig>();
                _cachedConfig.ResetToDefault();
            }

            return _cachedConfig;
        }

        /// <summary>
        /// 加载应用设置
        /// </summary>
        public static AppSettings LoadAppSettings()
        {
            if (_cachedAppSettings != null)
                return _cachedAppSettings;

            _cachedAppSettings = Resources.Load<AppSettings>("Config/AppSettings");

            if (_cachedAppSettings == null)
            {
                Debug.LogWarning("[ConfigurationLoader] AppSettings not found, creating default instance");
                _cachedAppSettings = ScriptableObject.CreateInstance<AppSettings>();
            }

            return _cachedAppSettings;
        }

        /// <summary>
        /// 清除缓存的配置
        /// </summary>
        public static void ClearCache()
        {
            _cachedConfig = null;
            _cachedAppSettings = null;
        }
    }
}
