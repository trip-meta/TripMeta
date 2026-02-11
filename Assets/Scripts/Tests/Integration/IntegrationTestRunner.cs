using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TripMeta.Core.Bootstrap;
using TripMeta.Core.DependencyInjection;
using TripMeta.Core.ErrorHandling;
using TripMeta.AI;

namespace TripMeta.Tests.Integration
{
    /// <summary>
    /// 集成测试运行器
    /// </summary>
    public class IntegrationTestRunner : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool showDetailedResults = true;
        [SerializeField] private float testTimeout = 30f;
        
        private List<IIntegrationTest> tests;
        private TestResults testResults;
        
        public static IntegrationTestRunner Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTests();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            if (runTestsOnStart)
            {
                _ = RunAllTestsAsync();
            }
        }
        
        /// <summary>
        /// 初始化测试
        /// </summary>
        private void InitializeTests()
        {
            tests = new List<IIntegrationTest>
            {
                new ServiceContainerTest(),
                new ErrorHandlerTest(),
                new ApplicationBootstrapTest(),
                new VRSystemTest(),
                new AIServicesTest()
            };
            
            testResults = new TestResults();
            
            Debug.Log($"[IntegrationTestRunner] 初始化了 {tests.Count} 个集成测试");
        }
        
        /// <summary>
        /// 运行所有测试
        /// </summary>
        public async Task<TestResults> RunAllTestsAsync()
        {
            Debug.Log("[IntegrationTestRunner] 开始运行集成测试...");
            
            testResults.Reset();
            testResults.StartTime = DateTime.UtcNow;
            
            foreach (var test in tests)
            {
                await RunSingleTestAsync(test);
            }
            
            testResults.EndTime = DateTime.UtcNow;
            testResults.Duration = testResults.EndTime - testResults.StartTime;
            
            LogTestResults();
            
            return testResults;
        }
        
        /// <summary>
        /// 运行单个测试
        /// </summary>
        private async Task RunSingleTestAsync(IIntegrationTest test)
        {
            var testResult = new TestResult
            {
                TestName = test.GetType().Name,
                StartTime = DateTime.UtcNow
            };
            
            try
            {
                Debug.Log($"[IntegrationTestRunner] 运行测试: {testResult.TestName}");
                
                // 设置超时
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(testTimeout));
                var testTask = test.RunTestAsync();
                
                var completedTask = await Task.WhenAny(testTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"测试 {testResult.TestName} 超时");
                }
                
                testResult.Success = await testTask;
                testResult.Message = testResult.Success ? "测试通过" : "测试失败";
                
                if (testResult.Success)
                {
                    testResults.PassedTests++;
                    Debug.Log($"[IntegrationTestRunner] ✓ {testResult.TestName} 通过");
                }
                else
                {
                    testResults.FailedTests++;
                    Debug.LogError($"[IntegrationTestRunner] ✗ {testResult.TestName} 失败");
                }
            }
            catch (Exception ex)
            {
                testResult.Success = false;
                testResult.Message = ex.Message;
                testResult.Exception = ex;
                testResults.FailedTests++;
                
                Debug.LogError($"[IntegrationTestRunner] ✗ {testResult.TestName} 异常: {ex.Message}");
            }
            finally
            {
                testResult.EndTime = DateTime.UtcNow;
                testResult.Duration = testResult.EndTime - testResult.StartTime;
                testResults.TestResults.Add(testResult);
                testResults.TotalTests++;
            }
        }
        
        /// <summary>
        /// 记录测试结果
        /// </summary>
        private void LogTestResults()
        {
            Debug.Log("=== 集成测试结果 ===");
            Debug.Log($"总测试数: {testResults.TotalTests}");
            Debug.Log($"通过: {testResults.PassedTests}");
            Debug.Log($"失败: {testResults.FailedTests}");
            Debug.Log($"成功率: {testResults.SuccessRate:P1}");
            Debug.Log($"总耗时: {testResults.Duration.TotalSeconds:F2}秒");
            
            if (showDetailedResults)
            {
                Debug.Log("\n=== 详细结果 ===");
                foreach (var result in testResults.TestResults)
                {
                    var status = result.Success ? "✓" : "✗";
                    Debug.Log($"{status} {result.TestName}: {result.Message} ({result.Duration.TotalMilliseconds:F0}ms)");
                    
                    if (!result.Success && result.Exception != null)
                    {
                        Debug.LogError($"  异常: {result.Exception.Message}");
                    }
                }
            }
            
            Debug.Log("==================");
        }
        
        /// <summary>
        /// 手动运行测试
        /// </summary>
        [ContextMenu("Run Integration Tests")]
        public void ManualRunTests()
        {
            _ = RunAllTestsAsync();
        }
    }
    
    /// <summary>
    /// 集成测试接口
    /// </summary>
    public interface IIntegrationTest
    {
        Task<bool> RunTestAsync();
    }
    
    /// <summary>
    /// 测试结果
    /// </summary>
    public class TestResult
    {
        public string TestName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
    
    /// <summary>
    /// 测试结果集合
    /// </summary>
    public class TestResults
    {
        public List<TestResult> TestResults { get; set; } = new List<TestResult>();
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        
        public float SuccessRate => TotalTests > 0 ? (float)PassedTests / TotalTests : 0f;
        
        public void Reset()
        {
            TestResults.Clear();
            TotalTests = 0;
            PassedTests = 0;
            FailedTests = 0;
        }
    }
    
    // 具体测试实现
    
    /// <summary>
    /// 服务容器测试
    /// </summary>
    public class ServiceContainerTest : IIntegrationTest
    {
        public async Task<bool> RunTestAsync()
        {
            try
            {
                var container = new ServiceContainer();
                
                // 测试单例注册和解析
                container.RegisterSingleton<ITestService, TestService>();
                var service1 = container.Resolve<ITestService>();
                var service2 = container.Resolve<ITestService>();
                
                if (service1 == null || service2 == null || service1 != service2)
                    return false;
                
                // 测试瞬态注册和解析
                container.RegisterTransient<ITestService, TestService>();
                var service3 = container.Resolve<ITestService>();
                var service4 = container.Resolve<ITestService>();
                
                if (service3 == null || service4 == null || service3 == service4)
                    return false;
                
                container.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// 错误处理器测试
    /// </summary>
    public class ErrorHandlerTest : IIntegrationTest
    {
        public async Task<bool> RunTestAsync()
        {
            try
            {
                var errorHandler = new ErrorHandler();
                
                // 测试错误处理
                errorHandler.HandleError("测试错误", "测试上下文");
                errorHandler.HandleWarning("测试警告", "测试上下文");
                errorHandler.HandleInfo("测试信息", "测试上下文");
                
                // 测试异常处理
                var testException = new InvalidOperationException("测试异常");
                errorHandler.HandleException(testException, "测试上下文");
                
                // 检查统计信息
                var stats = errorHandler.GetErrorStatistics();
                if (stats.TotalErrors < 4) return false;
                
                errorHandler.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// 应用启动器测试
    /// </summary>
    public class ApplicationBootstrapTest : IIntegrationTest
    {
        public async Task<bool> RunTestAsync()
        {
            try
            {
                var bootstrap = ApplicationBootstrap.Instance;
                if (bootstrap == null) return false;
                
                // 检查是否已初始化
                if (!bootstrap.IsInitialized)
                {
                    // 等待初始化完成
                    await Task.Delay(5000);
                }
                
                return bootstrap.IsInitialized;
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// VR系统测试
    /// </summary>
    public class VRSystemTest : IIntegrationTest
    {
        public async Task<bool> RunTestAsync()
        {
            try
            {
                var bootstrap = ApplicationBootstrap.Instance;
                if (bootstrap == null || !bootstrap.IsInitialized) return false;
                
                var vrManager = bootstrap.GetService<VRManager>();
                return vrManager != null;
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// AI服务测试
    /// </summary>
    public class AIServicesTest : IIntegrationTest
    {
        public async Task<bool> RunTestAsync()
        {
            try
            {
                var bootstrap = ApplicationBootstrap.Instance;
                if (bootstrap == null || !bootstrap.IsInitialized) return false;
                
                var aiManager = bootstrap.GetService<IAIServiceManager>();
                if (aiManager == null) return false;
                
                // 测试AI服务是否可用
                return aiManager.IsInitialized;
            }
            catch
            {
                return false;
            }
        }
    }
    
    // 测试用接口和类
    public interface ITestService { }
    public class TestService : ITestService { }
}