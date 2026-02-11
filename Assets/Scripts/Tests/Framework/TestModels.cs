using System;
using System.Collections.Generic;
using UnityEngine;

namespace TripMeta.Tests.Framework
{
    /// <summary>
    /// 测试结果
    /// </summary>
    [Serializable]
    public class TestResult
    {
        public string testName;
        public TestType testType;
        public TestStatus status;
        public string message;
        public string stackTrace;
        public DateTime startTime;
        public DateTime endTime;
        public TimeSpan duration;
        public PerformanceData performanceData;
        public Dictionary<string, object> metadata;
        
        public TestResult()
        {
            metadata = new Dictionary<string, object>();
        }
        
        public bool IsPassed => status == TestStatus.Passed;
        public bool IsFailed => status == TestStatus.Failed;
        public bool IsSkipped => status == TestStatus.Skipped;
    }
    
    /// <summary>
    /// 测试状态
    /// </summary>
    public enum TestStatus
    {
        NotRun,     // 未运行
        Running,    // 运行中
        Passed,     // 通过
        Failed,     // 失败
        Skipped,    // 跳过
        Timeout     // 超时
    }
    
    /// <summary>
    /// 测试类型
    /// </summary>
    public enum TestType
    {
        Unit,           // 单元测试
        Integration,    // 集成测试
        Performance,    // 性能测试
        UI,            // UI测试
        Smoke,         // 冒烟测试
        Regression     // 回归测试
    }
    
    /// <summary>
    /// 测试套件
    /// </summary>
    [Serializable]
    public class TestSuite
    {
        public string name;
        public TestType testType;
        public List<TestResult> results;
        public DateTime startTime;
        public DateTime endTime;
        public TimeSpan duration;
        
        public int TotalCount => results.Count;
        public int PassedCount => results.FindAll(r => r.IsPassed).Count;
        public int FailedCount => results.FindAll(r => r.IsFailed).Count;
        public int SkippedCount => results.FindAll(r => r.IsSkipped).Count;
        public float SuccessRate => TotalCount > 0 ? (float)PassedCount / TotalCount : 0f;
        
        public TestSuite(string name, TestType testType)
        {
            this.name = name;
            this.testType = testType;
            this.results = new List<TestResult>();
            this.startTime = DateTime.Now;
        }
        
        public void AddResult(TestResult result)
        {
            results.Add(result);
            endTime = DateTime.Now;
            duration = endTime - startTime;
        }
    }
    
    /// <summary>
    /// 性能数据
    /// </summary>
    [Serializable]
    public class PerformanceData
    {
        public float averageTime;   // 平均时间(ms)
        public float maxTime;       // 最大时间(ms)
        public float minTime;       // 最小时间(ms)
        public int iterations;      // 迭代次数
        public float memoryUsage;   // 内存使用(MB)
        public int allocations;     // 分配次数
        public float frameRate;     // 帧率
        public Dictionary<string, float> customMetrics;
        
        public PerformanceData()
        {
            customMetrics = new Dictionary<string, float>();
        }
    }
    
    /// <summary>
    /// 测试报告
    /// </summary>
    [Serializable]
    public class TestReport
    {
        public DateTime startTime;
        public DateTime endTime;
        public TimeSpan duration;
        public int totalTests;
        public int passedTests;
        public int failedTests;
        public int skippedTests;
        public float successRate;
        public List<TestResult> testResults;
        public List<TestSuite> testSuites;
        public string environment;
        public string version;
        public Dictionary<string, object> systemInfo;
        
        public TestReport()
        {
            testResults = new List<TestResult>();
            testSuites = new List<TestSuite>();
            systemInfo = new Dictionary<string, object>();
            
            // 收集系统信息
            CollectSystemInfo();
        }
        
        private void CollectSystemInfo()
        {
            systemInfo["Platform"] = Application.platform.ToString();
            systemInfo["UnityVersion"] = Application.unityVersion;
            systemInfo["DeviceModel"] = SystemInfo.deviceModel;
            systemInfo["OperatingSystem"] = SystemInfo.operatingSystem;
            systemInfo["ProcessorType"] = SystemInfo.processorType;
            systemInfo["SystemMemorySize"] = SystemInfo.systemMemorySize;
            systemInfo["GraphicsDeviceName"] = SystemInfo.graphicsDeviceName;
            systemInfo["GraphicsMemorySize"] = SystemInfo.graphicsMemorySize;
        }
        
        /// <summary>
        /// 生成摘要
        /// </summary>
        public string GenerateSummary()
        {
            var summary = $"测试报告摘要\n";
            summary += $"执行时间: {startTime:yyyy-MM-dd HH:mm:ss} - {endTime:yyyy-MM-dd HH:mm:ss}\n";
            summary += $"总耗时: {duration.TotalMinutes:F1} 分钟\n";
            summary += $"总测试数: {totalTests}\n";
            summary += $"通过: {passedTests} ({successRate:P1})\n";
            summary += $"失败: {failedTests}\n";
            summary += $"跳过: {skippedTests}\n";
            summary += $"平台: {systemInfo["Platform"]}\n";
            summary += $"Unity版本: {systemInfo["UnityVersion"]}";
            
            return summary;
        }
    }
    
    /// <summary>
    /// 测试运行器
    /// </summary>
    public class TestRunner
    {
        private TestFramework framework;
        private Queue<TestResult> testQueue;
        private bool isRunning;
        
        public bool IsRunning => isRunning;
        
        public TestRunner(TestFramework framework)
        {
            this.framework = framework;
            this.testQueue = new Queue<TestResult>();
        }
        
        public void QueueTest(TestResult test)
        {
            testQueue.Enqueue(test);
        }
        
        public void Start()
        {
            isRunning = true;
        }
        
        public void Stop()
        {
            isRunning = false;
            testQueue.Clear();
        }
    }
    
    /// <summary>
    /// 测试报告器
    /// </summary>
    public class TestReporter
    {
        private string reportDirectory;
        
        public TestReporter()
        {
            reportDirectory = System.IO.Path.Combine(Application.persistentDataPath, "TestReports");
            
            if (!System.IO.Directory.Exists(reportDirectory))
            {
                System.IO.Directory.CreateDirectory(reportDirectory);
            }
        }
        
        /// <summary>
        /// 保存报告
        /// </summary>
        public void SaveReport(TestReport report)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"TestReport_{timestamp}.json";
                var filePath = System.IO.Path.Combine(reportDirectory, fileName);
                
                var json = JsonUtility.ToJson(report, true);
                System.IO.File.WriteAllText(filePath, json);
                
                Debug.Log($"测试报告已保存: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// 生成HTML报告
        /// </summary>
        public void GenerateHTMLReport(TestReport report)
        {
            try
            {
                var html = GenerateHTMLContent(report);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"TestReport_{timestamp}.html";
                var filePath = System.IO.Path.Combine(reportDirectory, fileName);
                
                System.IO.File.WriteAllText(filePath, html);
                
                Debug.Log($"HTML测试报告已生成: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        private string GenerateHTMLContent(TestReport report)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>TripMeta 测试报告</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #f0f0f0; padding: 20px; border-radius: 5px; }}
        .summary {{ margin: 20px 0; }}
        .test-result {{ margin: 10px 0; padding: 10px; border-left: 4px solid #ccc; }}
        .passed {{ border-left-color: #4CAF50; }}
        .failed {{ border-left-color: #f44336; }}
        .skipped {{ border-left-color: #ff9800; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>TripMeta 测试报告</h1>
        <p>生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
    </div>
    
    <div class='summary'>
        <h2>测试摘要</h2>
        <p>总测试数: {report.totalTests}</p>
        <p>通过: {report.passedTests} ({report.successRate:P1})</p>
        <p>失败: {report.failedTests}</p>
        <p>跳过: {report.skippedTests}</p>
        <p>执行时间: {report.duration.TotalMinutes:F1} 分钟</p>
    </div>
    
    <div class='details'>
        <h2>测试详情</h2>
        <table>
            <tr>
                <th>测试名称</th>
                <th>类型</th>
                <th>状态</th>
                <th>耗时</th>
                <th>消息</th>
            </tr>";
            
            foreach (var result in report.testResults)
            {
                var statusClass = result.status.ToString().ToLower();
                html += $@"
            <tr class='{statusClass}'>
                <td>{result.testName}</td>
                <td>{result.testType}</td>
                <td>{result.status}</td>
                <td>{result.duration.TotalMilliseconds:F0}ms</td>
                <td>{result.message}</td>
            </tr>";
            }
            
            html += @"
        </table>
    </div>
</body>
</html>";
            
            return html;
        }
    }
    
    // 测试属性
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class UnitTestAttribute : TestAttribute { }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class IntegrationTestAttribute : TestAttribute { }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class PerformanceTestAttribute : TestAttribute
    {
        public float MaxDurationMs { get; set; } = 1000f;
        public int Iterations { get; set; } = 1;
        
        public PerformanceTestAttribute(float maxDurationMs = 1000f, int iterations = 1)
        {
            MaxDurationMs = maxDurationMs;
            Iterations = iterations;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class UITestAttribute : TestAttribute { }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class SmokeTestAttribute : TestAttribute { }
}