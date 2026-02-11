using UnityEngine;
using TripMeta.Core.DependencyInjection;
using TripMeta.Core.Configuration;
using TripMeta.Core.ErrorHandling;
using TripMeta.Infrastructure.Network;
using TripMeta.Infrastructure.Cache;
using TripMeta.Features.TourGuide;

namespace TripMeta.Tests.Integration
{
    /// <summary>
    /// 架构测试 - 验证DI容器和服务定位器
    /// </summary>
    public class ArchitectureTest : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private TripMetaConfig testConfig;
        
        private void Start()
        {
            if (runOnStart)
            {
                RunArchitectureTests();
            }
        }
        
        [ContextMenu("运行架构测试")]
        public void RunArchitectureTests()
        {
            Logger.LogInfo("=== 开始架构测试 ===", "ARCH_TEST");
            
            try
            {
                TestServiceContainer();
                TestServiceLocator();
                TestServiceInstaller();
                TestCompleteArchitecture();
                
                Logger.LogInfo("=== 架构测试全部通过 ===", "ARCH_TEST");
            }
            catch (System.Exception ex)
            {
                Logger.LogException(ex, "架构测试失败");
            }
        }
        
        private void TestServiceContainer()
        {
            Logger.LogInfo("测试服务容器...", "ARCH_TEST");
            
            var container = new ServiceContainer();
            
            // 测试单例注册
            container.RegisterSingleton<ITestService, TestServiceImpl>();
            Assert(container.IsRegistered<ITestService>(), "单例服务注册失败");
            
            // 测试服务解析
            var service1 = container.Resolve<ITestService>();
            var service2 = container.Resolve<ITestService>();
            Assert(service1 != null, "服务解析失败");
            Assert(ReferenceEquals(service1, service2), "单例服务应返回同一实例");
            
            // 测试瞬态服务
            container.RegisterTransient<ITransientTestService, TransientTestServiceImpl>();
            var transient1 = container.Resolve<ITransientTestService>();
            var transient2 = container.Resolve<ITransientTestService>();
            Assert(transient1 != null && transient2 != null, "瞬态服务解析失败");
            Assert(!ReferenceEquals(transient1, transient2), "瞬态服务应返回不同实例");
            
            // 测试工厂注册
            container.RegisterFactory<IFactoryTestService>(c => new FactoryTestServiceImpl("Factory Created"));
            var factoryService = container.Resolve<IFactoryTestService>();
            Assert(factoryService != null, "工厂服务解析失败");
            Assert(factoryService.GetMessage() == "Factory Created", "工厂服务创建失败");
            
            container.Dispose();
            Logger.LogInfo("✓ 服务容器测试通过", "ARCH_TEST");
        }
        
        private void TestServiceLocator()
        {
            Logger.LogInfo("测试服务定位器...", "ARCH_TEST");
            
            var container = new ServiceContainer();
            container.RegisterSingleton<ITestService, TestServiceImpl>();
            
            // 初始化服务定位器
            ServiceLocator.Initialize(container);
            Assert(ServiceLocator.IsInitialized, "服务定位器初始化失败");
            
            // 测试服务获取
            var service = ServiceLocator.Get<ITestService>();
            Assert(service != null, "服务定位器获取服务失败");
            
            // 测试TryGet
            bool success = ServiceLocator.TryGet<ITestService>(out var tryService);
            Assert(success && tryService != null, "TryGet失败");
            
            // 测试未注册服务
            bool notFound = ServiceLocator.TryGet<IUnregisteredService>(out var unregistered);
            Assert(!notFound && unregistered == null, "未注册服务应该返回false");
            
            ServiceLocator.Reset();
            container.Dispose();
            Logger.LogInfo("✓ 服务定位器测试通过", "ARCH_TEST");
        }
        
        private void TestServiceInstaller()
        {
            Logger.LogInfo("测试服务安装器...", "ARCH_TEST");
            
            var container = new ServiceContainer();
            
            // 创建测试配置
            if (testConfig == null)
            {
                testConfig = ScriptableObject.CreateInstance<TripMetaConfig>();
                testConfig.ResetToDefault();
            }
            
            // 安装服务
            ServiceInstaller.InstallServices(container, testConfig);
            
            // 验证核心服务
            Assert(container.IsRegistered<TripMetaConfig>(), "配置服务未注册");
            Assert(container.IsRegistered<AppSettings>(), "应用设置未注册");
            Assert(container.IsRegistered<IErrorHandler>(), "错误处理器未注册");
            
            // 验证基础设施服务
            Assert(container.IsRegistered<INetworkService>(), "网络服务未注册");
            Assert(container.IsRegistered<ICacheService>(), "缓存服务未注册");
            
            // 验证AI服务
            Assert(container.IsRegistered<IAIServiceManager>(), "AI服务管理器未注册");
            
            // 验证功能服务
            Assert(container.IsRegistered<ITourGuideService>(), "导游服务未注册");
            
            container.Dispose();
            Logger.LogInfo("✓ 服务安装器测试通过", "ARCH_TEST");
        }
        
        private void TestCompleteArchitecture()
        {
            Logger.LogInfo("测试完整架构...", "ARCH_TEST");
            
            var container = new ServiceContainer();
            ServiceLocator.Initialize(container);
            
            if (testConfig == null)
            {
                testConfig = ScriptableObject.CreateInstance<TripMetaConfig>();
                testConfig.ResetToDefault();
            }
            
            ServiceInstaller.InstallServices(container, testConfig);
            
            // 测试依赖注入链
            var tourGuideService = ServiceLocator.Get<ITourGuideService>();
            Assert(tourGuideService != null, "导游服务获取失败");
            
            var networkService = ServiceLocator.Get<INetworkService>();
            Assert(networkService != null, "网络服务获取失败");
            
            var cacheService = ServiceLocator.Get<ICacheService>();
            Assert(cacheService != null, "缓存服务获取失败");
            
            // 测试配置访问
            var config = ServiceLocator.Get<TripMetaConfig>();
            Assert(config != null, "配置获取失败");
            
            var appSettings = ServiceLocator.Get<AppSettings>();
            Assert(appSettings != null, "应用设置获取失败");
            
            ServiceLocator.Reset();
            container.Dispose();
            Logger.LogInfo("✓ 完整架构测试通过", "ARCH_TEST");
        }
        
        private void Assert(bool condition, string message)
        {
            if (!condition)
            {
                string error = $"架构测试断言失败: {message}";
                Logger.LogError(error, "ARCH_TEST");
                throw new System.Exception(error);
            }
        }
    }
    
    // 测试接口和实现
    public interface ITestService
    {
        string GetMessage();
    }
    
    public class TestServiceImpl : ITestService
    {
        public string GetMessage() => "Test Service";
    }
    
    public interface ITransientTestService
    {
        int GetId();
    }
    
    public class TransientTestServiceImpl : ITransientTestService
    {
        private static int counter = 0;
        private readonly int id;
        
        public TransientTestServiceImpl()
        {
            id = ++counter;
        }
        
        public int GetId() => id;
    }
    
    public interface IFactoryTestService
    {
        string GetMessage();
    }
    
    public class FactoryTestServiceImpl : IFactoryTestService
    {
        private readonly string message;
        
        public FactoryTestServiceImpl(string message)
        {
            this.message = message;
        }
        
        public string GetMessage() => message;
    }
    
    public interface IUnregisteredService { }
}