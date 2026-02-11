using System;
using System.Collections.Generic;

namespace TripMeta.Core.DependencyInjection
{
    /// <summary>
    /// 服务容器接口 - 提供依赖注入功能
    /// </summary>
    public interface IServiceContainer
    {
        /// <summary>
        /// 注册单例服务
        /// </summary>
        void RegisterSingleton<TInterface, TImplementation>() 
            where TImplementation : class, TInterface;
        
        /// <summary>
        /// 注册单例服务（使用实例）
        /// </summary>
        void RegisterSingleton<TInterface>(TInterface instance) 
            where TInterface : class;
        
        /// <summary>
        /// 注册瞬态服务
        /// </summary>
        void RegisterTransient<TInterface, TImplementation>() 
            where TImplementation : class, TInterface;
        
        /// <summary>
        /// 注册作用域服务
        /// </summary>
        void RegisterScoped<TInterface, TImplementation>() 
            where TImplementation : class, TInterface;
        
        /// <summary>
        /// 注册工厂方法
        /// </summary>
        void RegisterFactory<TInterface>(Func<IServiceContainer, TInterface> factory) 
            where TInterface : class;
        
        /// <summary>
        /// 解析服务
        /// </summary>
        T Resolve<T>() where T : class;
        
        /// <summary>
        /// 解析服务（按类型）
        /// </summary>
        object Resolve(Type serviceType);
        
        /// <summary>
        /// 尝试解析服务
        /// </summary>
        bool TryResolve<T>(out T service) where T : class;
        
        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        bool IsRegistered<T>() where T : class;
        
        /// <summary>
        /// 检查服务是否已注册（按类型）
        /// </summary>
        bool IsRegistered(Type serviceType);
        
        /// <summary>
        /// 获取所有已注册的服务类型
        /// </summary>
        IEnumerable<Type> GetRegisteredServices();
        
        /// <summary>
        /// 创建子容器
        /// </summary>
        IServiceContainer CreateChildContainer();
        
        /// <summary>
        /// 释放资源
        /// </summary>
        void Dispose();
    }
    
    /// <summary>
    /// 服务生命周期
    /// </summary>
    public enum ServiceLifetime
    {
        Singleton,  // 单例
        Transient,  // 瞬态
        Scoped      // 作用域
    }
    
    /// <summary>
    /// 服务描述符
    /// </summary>
    public class ServiceDescriptor
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public Func<IServiceContainer, object> Factory { get; set; }
        public object Instance { get; set; }
    }
}