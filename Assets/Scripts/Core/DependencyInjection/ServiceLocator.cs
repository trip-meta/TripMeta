using UnityEngine;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.Core.DependencyInjection
{
    /// <summary>
    /// 服务定位器 - 提供全局服务访问
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceContainer _container;
        private static bool _isInitialized = false;
        
        /// <summary>
        /// 初始化服务定位器
        /// </summary>
        public static void Initialize(IServiceContainer container)
        {
            if (_isInitialized)
            {
                Logger.LogWarning("服务定位器已经初始化", "ServiceLocator");
                return;
            }
            
            _container = container ?? throw new System.ArgumentNullException(nameof(container));
            _isInitialized = true;
            
            Logger.LogInfo("服务定位器初始化完成", "ServiceLocator");
        }
        
        /// <summary>
        /// 获取服务
        /// </summary>
        public static T Get<T>() where T : class
        {
            ThrowIfNotInitialized();
            
            try
            {
                return _container.Resolve<T>();
            }
            catch (System.Exception ex)
            {
                Logger.LogException(ex, $"获取服务失败: {typeof(T).Name}");
                throw;
            }
        }
        
        /// <summary>
        /// 尝试获取服务
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            service = null;
            
            if (!_isInitialized)
            {
                Logger.LogWarning("服务定位器未初始化", "ServiceLocator");
                return false;
            }
            
            return _container.TryResolve(out service);
        }
        
        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            if (!_isInitialized) return false;
            return _container.IsRegistered<T>();
        }
        
        /// <summary>
        /// 重置服务定位器
        /// </summary>
        public static void Reset()
        {
            _container?.Dispose();
            _container = null;
            _isInitialized = false;
            
            Logger.LogInfo("服务定位器已重置", "ServiceLocator");
        }
        
        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public static bool IsInitialized => _isInitialized;
        
        private static void ThrowIfNotInitialized()
        {
            if (!_isInitialized)
            {
                throw new System.InvalidOperationException("服务定位器未初始化，请先调用 Initialize 方法");
            }
        }
    }
}