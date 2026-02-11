using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TripMeta.Core.ErrorHandling;

namespace TripMeta.Core.DependencyInjection
{
    /// <summary>
    /// 现代化依赖注入容器 - 完整实现
    /// </summary>
    public class ServiceContainer : MonoBehaviour, IServiceContainer
    {
        private Dictionary<Type, ServiceDescriptor> services = new Dictionary<Type, ServiceDescriptor>();
        private Dictionary<Type, object> singletonInstances = new Dictionary<Type, object>();
        private Dictionary<Type, object> scopedInstances = new Dictionary<Type, object>();
        private IServiceContainer parentContainer;
        private bool disposed = false;
        
        public ServiceContainer(IServiceContainer parent = null)
        {
            parentContainer = parent;
        }
        
        public void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class
        {
            ThrowIfDisposed();
            var serviceType = typeof(TInterface);
            services[serviceType] = new ServiceDescriptor
            {
                ServiceType = serviceType,
                Instance = instance,
                Lifetime = ServiceLifetime.Singleton
            };
            singletonInstances[serviceType] = instance;
            Logger.LogInfo($"注册单例实例: {serviceType.Name}", "DI");
        }
        
        public void RegisterSingleton<TInterface, TImplementation>() 
            where TInterface : class 
            where TImplementation : class, TInterface
        {
            ThrowIfDisposed();
            var serviceType = typeof(TInterface);
            var implementationType = typeof(TImplementation);
            services[serviceType] = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = ServiceLifetime.Singleton
            };
            Logger.LogInfo($"注册单例服务: {serviceType.Name} -> {implementationType.Name}", "DI");
        }
        
        public void RegisterTransient<TInterface, TImplementation>() 
            where TInterface : class 
            where TImplementation : class, TInterface
        {
            ThrowIfDisposed();
            var serviceType = typeof(TInterface);
            var implementationType = typeof(TImplementation);
            services[serviceType] = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = ServiceLifetime.Transient
            };
            Logger.LogInfo($"注册瞬态服务: {serviceType.Name} -> {implementationType.Name}", "DI");
        }
        
        public void RegisterScoped<TInterface, TImplementation>() 
            where TInterface : class 
            where TImplementation : class, TInterface
        {
            ThrowIfDisposed();
            var serviceType = typeof(TInterface);
            var implementationType = typeof(TImplementation);
            services[serviceType] = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = ServiceLifetime.Scoped
            };
            Logger.LogInfo($"注册作用域服务: {serviceType.Name} -> {implementationType.Name}", "DI");
        }
        
        public void RegisterFactory<TInterface>(Func<IServiceContainer, TInterface> factory) 
            where TInterface : class
        {
            ThrowIfDisposed();
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            
            var serviceType = typeof(TInterface);
            services[serviceType] = new ServiceDescriptor
            {
                ServiceType = serviceType,
                Factory = container => factory(container),
                Lifetime = ServiceLifetime.Transient
            };
            Logger.LogInfo($"注册工厂方法: {serviceType.Name}", "DI");
        }
        
        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }
        
        public object Resolve(Type serviceType)
        {
            ThrowIfDisposed();
            
            // 检查本容器
            if (services.TryGetValue(serviceType, out var descriptor))
            {
                return CreateInstance(descriptor);
            }
            
            // 检查父容器
            if (parentContainer != null && parentContainer.IsRegistered(serviceType))
            {
                return parentContainer.Resolve(serviceType);
            }
            
            // 尝试创建具体类型
            if (!serviceType.IsInterface && !serviceType.IsAbstract)
            {
                return CreateInstance(serviceType);
            }
            
            throw new InvalidOperationException($"服务 {serviceType.Name} 未注册");
        }
        
        public bool TryResolve<T>(out T service) where T : class
        {
            try
            {
                service = Resolve<T>();
                return service != null;
            }
            catch
            {
                service = null;
                return false;
            }
        }
        
        private object CreateInstance(ServiceDescriptor descriptor)
        {
            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    if (singletonInstances.TryGetValue(descriptor.ServiceType, out var singletonInstance))
                        return singletonInstance;
                    
                    if (descriptor.Instance != null)
                        return descriptor.Instance;
                    
                    var newSingleton = CreateNewInstance(descriptor);
                    singletonInstances[descriptor.ServiceType] = newSingleton;
                    return newSingleton;
                
                case ServiceLifetime.Scoped:
                    if (scopedInstances.TryGetValue(descriptor.ServiceType, out var scopedInstance))
                        return scopedInstance;
                    
                    var newScoped = CreateNewInstance(descriptor);
                    scopedInstances[descriptor.ServiceType] = newScoped;
                    return newScoped;
                
                case ServiceLifetime.Transient:
                    return CreateNewInstance(descriptor);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private object CreateNewInstance(ServiceDescriptor descriptor)
        {
            if (descriptor.Instance != null)
                return descriptor.Instance;
            
            if (descriptor.Factory != null)
                return descriptor.Factory(this);
            
            return CreateInstance(descriptor.ImplementationType);
        }
        
        private object CreateInstance(Type type)
        {
            try
            {
                var constructors = type.GetConstructors();
                if (constructors.Length == 0)
                    throw new InvalidOperationException($"类型 {type.Name} 没有公共构造函数");
                
                // 选择参数最多的构造函数（贪婪注入）
                var constructor = constructors[0];
                foreach (var ctor in constructors)
                {
                    if (ctor.GetParameters().Length > constructor.GetParameters().Length)
                        constructor = ctor;
                }
                
                var parameters = constructor.GetParameters();
                var args = new object[parameters.Length];
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    
                    if (IsRegistered(paramType))
                    {
                        args[i] = Resolve(paramType);
                    }
                    else if (parameters[i].HasDefaultValue)
                    {
                        args[i] = parameters[i].DefaultValue;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"无法解析类型 {type.Name} 的构造函数参数 {paramType.Name}");
                    }
                }
                
                return Activator.CreateInstance(type, args);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"创建实例失败: {type.Name}");
                throw;
            }
        }
        
        public bool IsRegistered<T>() where T : class
        {
            return IsRegistered(typeof(T));
        }
        
        public bool IsRegistered(Type serviceType)
        {
            ThrowIfDisposed();
            
            if (services.ContainsKey(serviceType))
                return true;
            
            return parentContainer?.IsRegistered(serviceType) ?? false;
        }
        
        public IEnumerable<Type> GetRegisteredServices()
        {
            ThrowIfDisposed();
            
            var allServices = new HashSet<Type>(services.Keys);
            
            if (parentContainer != null)
            {
                foreach (var service in parentContainer.GetRegisteredServices())
                {
                    allServices.Add(service);
                }
            }
            
            return allServices;
        }
        
        public IServiceContainer CreateChildContainer()
        {
            ThrowIfDisposed();
            return new ServiceContainer(this);
        }
        
        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ServiceContainer));
        }
        
        public void Dispose()
        {
            if (disposed) return;
            
            // 释放作用域服务
            foreach (var instance in scopedInstances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, "释放作用域服务失败");
                    }
                }
            }
            scopedInstances.Clear();
            
            // 释放单例服务
            foreach (var instance in singletonInstances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, "释放单例服务失败");
                    }
                }
            }
            singletonInstances.Clear();
            
            services.Clear();
            disposed = true;
            
            Logger.LogInfo("服务容器已释放", "DI");
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
    }
}