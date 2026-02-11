using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using TripMeta.Core.ErrorHandling;
using TripMeta.Core.DependencyInjection;

namespace TripMeta.Tests.Framework
{
    /// <summary>
    /// 测试框架 - 统一的测试基础设施
    /// </summary>
    public class TestFramework : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private bool autoRunTests = false;
        [SerializeField] private bool enablePerformanceTests = true;
        [SerializeField] private bool enableIntegrationTests = true;
        [SerializeField] private float testTimeout = 30f;
        
        private static TestFramework instance;
        private TestRunner testRunner;
        private TestReporter testReporter;
        private List<TestResult> testResults = new List<TestResult>();
        
        public static TestFramework Instance => instance;
        public TestRunner Runner => testRunner;
        public TestReporter Reporter => testReporter;
        
        public event Action<TestResult> OnTestCompleted;
        public event Action<TestSuite> OnTestSuiteCompleted;
        public event Action<TestReport> OnAllTestsCompleted;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTestFramework();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (autoRunTests)
            {
                StartCoroutine(RunAllTestsCoroutine());
            }
        }
        
        /// <summary>
        /// 初始化测试框架
        /// </summary>
        private void InitializeTestFramework()
        {
            try
            {
                testRunner = new TestRunner(this);
                testReporter = new TestReporter();
                
                Logger.LogInfo("测试框架初始化完成", "TestFramework");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "测试框架初始化失败");
            }
        }
        
        /// <summary>
        /// 运行所有测试
        /// </summary>
        public void RunAllTests()
        {
            StartCoroutine(RunAllTestsCoroutine());
        }
        
        /// <summary>
        /// 运行所有测试协程
        /// </summary>
        private IEnumerator RunAllTestsCoroutine()
        {
            Logger.LogInfo("开始运行所有测试...", "TestFramework");
            
            testResults.Clear();
            var startTime = DateTime.Now;
            
            try
            {
                // 运行单元测试
                yield return StartCoroutine(RunUnitTests());
                
                // 运行集成测试
                if (enableIntegrationTests)
                {
                    yield return StartCoroutine(RunIntegrationTests());
                }
                
                // 运行性能测试
                if (enablePerformanceTests)
                {
                    yield return StartCoroutine(RunPerformanceTests());
                }
                
                // 生成测试报告
                var endTime = DateTime.Now;
                var report = GenerateTestReport(startTime, endTime);
                
                OnAllTestsCompleted?.Invoke(report);
                Logger.LogInfo($"所有测试完成，总耗时: {(endTime - startTime).TotalSeconds:F2}秒", "TestFramework");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "测试运行失败");
            }
        }
        
        /// <summary>
        /// 运行单元测试
        /// </summary>
        private IEnumerator RunUnitTests()
        {
            Logger.LogInfo("运行单元测试...", "TestFramework");
            
            var unitTestSuite = new TestSuite("UnitTests", TestType.Unit);
            
            // 发现并运行单元测试
            var testMethods = DiscoverTestMethods(typeof(UnitTestAttribute));
            
            foreach (var method in testMethods)
            {
                yield return StartCoroutine(RunTestMethod(method, unitTestSuite));
            }
            
            OnTestSuiteCompleted?.Invoke(unitTestSuite);
            Logger.LogInfo($"单元测试完成，通过: {unitTestSuite.PassedCount}/{unitTestSuite.TotalCount}", "TestFramework");
        }
        
        /// <summary>
        /// 运行集成测试
        /// </summary>
        private IEnumerator RunIntegrationTests()
        {
            Logger.LogInfo("运行集成测试...", "TestFramework");
            
            var integrationTestSuite = new TestSuite("IntegrationTests", TestType.Integration);
            
            // 发现并运行集成测试
            var testMethods = DiscoverTestMethods(typeof(IntegrationTestAttribute));
            
            foreach (var method in testMethods)
            {
                yield return StartCoroutine(RunTestMethod(method, integrationTestSuite));
            }
            
            OnTestSuiteCompleted?.Invoke(integrationTestSuite);
            Logger.LogInfo($"集成测试完成，通过: {integrationTestSuite.PassedCount}/{integrationTestSuite.TotalCount}", "TestFramework");
        }
        
        /// <summary>
        /// 运行性能测试
        /// </summary>
        private IEnumerator RunPerformanceTests()
        {
            Logger.LogInfo("运行性能测试...", "TestFramework");
            
            var performanceTestSuite = new TestSuite("PerformanceTests", TestType.Performance);
            
            // 发现并运行性能测试
            var testMethods = DiscoverTestMethods(typeof(PerformanceTestAttribute));
            
            foreach (var method in testMethods)
            {
                yield return StartCoroutine(RunPerformanceTestMethod(method, performanceTestSuite));
            }
            
            OnTestSuiteCompleted?.Invoke(performanceTestSuite);
            Logger.LogInfo($"性能测试完成，通过: {performanceTestSuite.PassedCount}/{performanceTestSuite.TotalCount}", "TestFramework");
        }
        
        /// <summary>
        /// 发现测试方法
        /// </summary>
        private List<MethodInfo> DiscoverTestMethods(Type attributeType)
        {
            var testMethods = new List<MethodInfo>();
            
            // 获取所有程序集
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    
                    foreach (var type in types)
                    {
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        
                        foreach (var method in methods)
                        {
                            if (method.GetCustomAttribute(attributeType) != null)
                            {
                                testMethods.Add(method);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, $"发现测试方法失败: {assembly.FullName}");
                }
            }
            
            return testMethods;
        }
        
        /// <summary>
        /// 运行测试方法
        /// </summary>
        private IEnumerator RunTestMethod(MethodInfo method, TestSuite suite)
        {
            var testResult = new TestResult
            {
                testName = $"{method.DeclaringType.Name}.{method.Name}",
                testType = suite.TestType,
                startTime = DateTime.Now
            };
            
            try
            {
                Logger.LogInfo($"运行测试: {testResult.testName}", "TestFramework");
                
                // 创建测试实例
                object testInstance = null;
                if (!method.IsStatic)
                {
                    testInstance = Activator.CreateInstance(method.DeclaringType);
                }
                
                // 运行测试方法
                var result = method.Invoke(testInstance, null);
                
                // 处理异步测试
                if (result is IEnumerator coroutine)
                {
                    yield return StartCoroutine(coroutine);
                }
                else if (result is System.Threading.Tasks.Task task)
                {
                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }
                    
                    if (task.IsFaulted)
                    {
                        throw task.Exception.InnerException;
                    }
                }
                
                testResult.status = TestStatus.Passed;
                testResult.message = "测试通过";
            }
            catch (Exception ex)
            {
                testResult.status = TestStatus.Failed;
                testResult.message = ex.Message;
                testResult.stackTrace = ex.StackTrace;
                
                Logger.LogException(ex, $"测试失败: {testResult.testName}");
            }
            finally
            {
                testResult.endTime = DateTime.Now;
                testResult.duration = testResult.endTime - testResult.startTime;
                
                suite.AddResult(testResult);
                testResults.Add(testResult);
                
                OnTestCompleted?.Invoke(testResult);
            }
        }
        
        /// <summary>
        /// 运行性能测试方法
        /// </summary>
        private IEnumerator RunPerformanceTestMethod(MethodInfo method, TestSuite suite)
        {
            var testResult = new TestResult
            {
                testName = $"{method.DeclaringType.Name}.{method.Name}",
                testType = TestType.Performance,
                startTime = DateTime.Now
            };
            
            try
            {
                Logger.LogInfo($"运行性能测试: {testResult.testName}", "TestFramework");
                
                // 获取性能测试属性
                var perfAttr = method.GetCustomAttribute<PerformanceTestAttribute>();
                var maxDuration = perfAttr?.MaxDurationMs ?? 1000f;
                var iterations = perfAttr?.Iterations ?? 1;
                
                var totalTime = 0f;
                var maxTime = 0f;
                var minTime = float.MaxValue;
                
                // 运行多次迭代
                for (int i = 0; i < iterations; i++)
                {
                    var iterationStart = Time.realtimeSinceStartup;
                    
                    // 创建测试实例并运行
                    object testInstance = null;
                    if (!method.IsStatic)
                    {
                        testInstance = Activator.CreateInstance(method.DeclaringType);
                    }
                    
                    var result = method.Invoke(testInstance, null);
                    
                    if (result is IEnumerator coroutine)
                    {
                        yield return StartCoroutine(coroutine);
                    }
                    
                    var iterationTime = (Time.realtimeSinceStartup - iterationStart) * 1000f;
                    totalTime += iterationTime;
                    maxTime = Mathf.Max(maxTime, iterationTime);
                    minTime = Mathf.Min(minTime, iterationTime);
                }
                
                var avgTime = totalTime / iterations;
                
                // 检查性能要求
                if (avgTime <= maxDuration)
                {
                    testResult.status = TestStatus.Passed;
                    testResult.message = $"性能测试通过 - 平均: {avgTime:F2}ms, 最大: {maxTime:F2}ms, 最小: {minTime:F2}ms";
                }
                else
                {
                    testResult.status = TestStatus.Failed;
                    testResult.message = $"性能测试失败 - 平均: {avgTime:F2}ms > 要求: {maxDuration}ms";
                }
                
                testResult.performanceData = new PerformanceData
                {
                    averageTime = avgTime,
                    maxTime = maxTime,
                    minTime = minTime,
                    iterations = iterations
                };
            }
            catch (Exception ex)
            {
                testResult.status = TestStatus.Failed;
                testResult.message = ex.Message;
                testResult.stackTrace = ex.StackTrace;
                
                Logger.LogException(ex, $"性能测试失败: {testResult.testName}");
            }
            finally
            {
                testResult.endTime = DateTime.Now;
                testResult.duration = testResult.endTime - testResult.startTime;
                
                suite.AddResult(testResult);
                testResults.Add(testResult);
                
                OnTestCompleted?.Invoke(testResult);
            }
        }
        
        /// <summary>
        /// 生成测试报告
        /// </summary>
        private TestReport GenerateTestReport(DateTime startTime, DateTime endTime)
        {
            var report = new TestReport
            {
                startTime = startTime,
                endTime = endTime,
                duration = endTime - startTime,
                totalTests = testResults.Count,
                passedTests = 0,
                failedTests = 0,
                skippedTests = 0,
                testResults = new List<TestResult>(testResults)
            };
            
            foreach (var result in testResults)
            {
                switch (result.status)
                {
                    case TestStatus.Passed:
                        report.passedTests++;
                        break;
                    case TestStatus.Failed:
                        report.failedTests++;
                        break;
                    case TestStatus.Skipped:
                        report.skippedTests++;
                        break;
                }
            }
            
            report.successRate = report.totalTests > 0 ? (float)report.passedTests / report.totalTests : 0f;
            
            // 保存报告
            testReporter.SaveReport(report);
            
            return report;
        }
        
        /// <summary>
        /// 运行特定测试
        /// </summary>
        public void RunTest(string testName)
        {
            StartCoroutine(RunSpecificTest(testName));
        }
        
        /// <summary>
        /// 运行特定测试协程
        /// </summary>
        private IEnumerator RunSpecificTest(string testName)
        {
            var testMethods = DiscoverTestMethods(typeof(TestAttribute));
            
            foreach (var method in testMethods)
            {
                var fullName = $"{method.DeclaringType.Name}.{method.Name}";
                if (fullName == testName)
                {
                    var suite = new TestSuite("SingleTest", TestType.Unit);
                    yield return StartCoroutine(RunTestMethod(method, suite));
                    break;
                }
            }
        }
        
        /// <summary>
        /// 清理测试结果
        /// </summary>
        public void ClearResults()
        {
            testResults.Clear();
            Logger.LogInfo("测试结果已清理", "TestFramework");
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}