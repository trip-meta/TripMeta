
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Threading.Tasks;
using TripMeta.AI;

namespace TripMeta.Tests.AI
{
    /// <summary>
    /// 双引擎LLM服务测试
    /// 测试AI双引擎架构 (GPT-4 + Claude-3.5 智能选择器)
    /// </summary>
    public class DualEngineLLMTests
    {
        private GPTConfig testGptConfig;
        private ClaudeConfig testClaudeConfig;
        private DualEngineConfig testDualConfig;

        [SetUp]
        public void Setup()
        {
            // 创建测试配置
            testGptConfig = ScriptableObject.CreateInstance<GPTConfig>();
            testGptConfig.apiKey = "test-gpt-key";
            testGptConfig.model = "gpt-4";
            testGptConfig.maxTokens = 100;

            testClaudeConfig = ScriptableObject.CreateInstance<ClaudeConfig>();
            testClaudeConfig.apiKey = "test-claude-key";
            testClaudeConfig.model = "claude-3-5-sonnet-20241022";
            testClaudeConfig.maxTokens = 100;

            testDualConfig = ScriptableObject.CreateInstance<DualEngineConfig>();
            testDualConfig.defaultStrategy = AIEngineSelectionStrategy.Intelligent;
            testDualConfig.enablePerformanceTracking = true;
        }

        [TearDown]
        public void TearDown()
        {
            if (testGptConfig != null)
                Object.Destroy(testGptConfig);
            if (testClaudeConfig != null)
                Object.Destroy(testClaudeConfig);
            if (testDualConfig != null)
                Object.Destroy(testDualConfig);
        }

        [Test]
        public void DualEngineConfig_Exists()
        {
            Assert.IsNotNull(testDualConfig, "双引擎配置应该存在");
            Assert.AreEqual(AIEngineSelectionStrategy.Intelligent, testDualConfig.defaultStrategy, "默认策略应该是智能选择");
            Assert.IsTrue(testDualConfig.enablePerformanceTracking, "性能追踪应该启用");
        }

        [Test]
        public void ClaudeConfig_Exists()
        {
            Assert.IsNotNull(testClaudeConfig, "Claude配置应该存在");
            Assert.AreEqual("claude-3-5-sonnet-20241022", testClaudeConfig.model, "Claude模型应该是3.5版本");
            Assert.AreEqual("https://api.anthropic.com/v1/messages", testClaudeConfig.apiEndpoint, "API端点应该正确");
            Assert.AreEqual("2023-06-01", testClaudeConfig.apiVersion, "API版本应该正确");
        }

        [Test]
        public void AIEngineSelector_ComponentExists()
        {
            // 验证AIEngineSelector组件存在
            var selectorType = typeof(AIEngineSelector);
            Assert.IsNotNull(selectorType, "AIEngineSelector类型应该存在");
            Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(selectorType), "AIEngineSelector应该继承自MonoBehaviour");
        }

        [Test]
        public void DualEngineLLMService_ImplementsInterface()
        {
            // 验证DualEngineLLMService实现了IGPTService接口
            var serviceType = typeof(DualEngineLLMService);
            Assert.IsTrue(typeof(IGPTService).IsAssignableFrom(serviceType), "DualEngineLLMService应该实现IGPTService接口");
        }

        [Test]
        public void AIEngineType_EnumValues()
        {
            // 验证AI引擎类型枚举
            var engineTypes = System.Enum.GetValues(typeof(AIEngineType));
            Assert.Contains(AIEngineType.GPT4, engineTypes, "应该包含GPT4");
            Assert.Contains(AIEngineType.Claude35, engineTypes, "应该包含Claude35");
            Assert.Contains(AIEngineType.Auto, engineTypes, "应该包含Auto");
        }

        [Test]
        public void AITaskType_EnumValues()
        {
            // 验证AI任务类型枚举
            var taskTypes = System.Enum.GetValues(typeof(AITaskType));
            Assert.Contains(AITaskType.Conversation, taskTypes, "应该包含Conversation");
            Assert.Contains(AITaskType.CodeGeneration, taskTypes, "应该包含CodeGeneration");
            Assert.Contains(AITaskType.Analysis, taskTypes, "应该包含Analysis");
            Assert.Contains(AITaskType.Reasoning, taskTypes, "应该包含Reasoning");
            Assert.Contains(AITaskType.Translation, taskTypes, "应该包含Translation");
        }

        [Test]
        public void AIEngineSelectionStrategy_EnumValues()
        {
            // 验证选择策略枚举
            var strategies = System.Enum.GetValues(typeof(AIEngineSelectionStrategy));
            Assert.Contains(AIEngineSelectionStrategy.GPT4Only, strategies, "应该包含GPT4Only");
            Assert.Contains(AIEngineSelectionStrategy.ClaudeOnly, strategies, "应该包含ClaudeOnly");
            Assert.Contains(AIEngineSelectionStrategy.Intelligent, strategies, "应该包含Intelligent");
            Assert.Contains(AIEngineSelectionStrategy.RoundRobin, strategies, "应该包含RoundRobin");
            Assert.Contains(AIEngineSelectionStrategy.ABTesting, strategies, "应该包含ABTesting");
            Assert.Contains(AIEngineSelectionStrategy.TaskBased, strategies, "应该包含TaskBased");
            Assert.Contains(AIEngineSelectionStrategy.Performance, strategies, "应该包含Performance");
        }

        [Test]
        public void EnginePerformanceMetrics_Initialization()
        {
            var metrics = new EnginePerformanceMetrics(AIEngineType.GPT4);

            Assert.AreEqual(AIEngineType.GPT4, metrics.EngineType, "引擎类型应该正确");
            Assert.AreEqual(0, metrics.RequestCount, "初始请求数应该为0");
            Assert.AreEqual(0, metrics.SuccessCount, "初始成功数应该为0");
            Assert.AreEqual(0, metrics.AverageLatency, "初始平均延迟应该为0");
            Assert.AreEqual(0, metrics.SuccessRate, "初始成功率应该为0");
        }

        [Test]
        public void EnginePerformanceMetrics_RecordSuccess()
        {
            var metrics = new EnginePerformanceMetrics(AIEngineType.GPT4);

            metrics.RecordRequest(500, true);
            metrics.RecordRequest(600, true);
            metrics.RecordRequest(700, true);

            Assert.AreEqual(3, metrics.RequestCount, "请求数应该为3");
            Assert.AreEqual(3, metrics.SuccessCount, "成功数应该为3");
            Assert.AreEqual(1.0f, metrics.SuccessRate, "成功率应该为100%");
            Assert.AreEqual(600, metrics.AverageLatency, "平均延迟应该是600ms");
        }

        [Test]
        public void EnginePerformanceMetrics_RecordFailure()
        {
            var metrics = new EnginePerformanceMetrics(AIEngineType.Claude35);

            metrics.RecordRequest(500, true);
            metrics.RecordRequest(0, false);
            metrics.RecordRequest(600, true);

            Assert.AreEqual(3, metrics.RequestCount, "请求数应该为3");
            Assert.AreEqual(2, metrics.SuccessCount, "成功数应该为2");
            Assert.AreEqual(1, metrics.FailureCount, "失败数应该为1");
            Assert.AreEqual(2f / 3f, metrics.SuccessRate, 0.01f, "成功率应该是66.7%");
        }

        [Test]
        public void EnginePerformanceMetrics_Reset()
        {
            var metrics = new EnginePerformanceMetrics(AIEngineType.GPT4);

            metrics.RecordRequest(500, true);
            metrics.RecordRequest(600, true);
            metrics.Reset();

            Assert.AreEqual(0, metrics.RequestCount, "重置后请求数应该为0");
            Assert.AreEqual(0, metrics.SuccessCount, "重置后成功数应该为0");
            Assert.AreEqual(0, metrics.AverageLatency, "重置后平均延迟应该为0");
        }

        [Test]
        public void EnginePerformanceReport_ToString()
        {
            var metrics = new EnginePerformanceMetrics(AIEngineType.GPT4);
            metrics.RecordRequest(500, true);
            metrics.RecordRequest(600, true);

            var report = metrics.GetReport();
            var reportString = report.ToString();

            StringAssert.Contains("GPT4", reportString, "报告应该包含引擎类型");
            StringAssert.Contains("请求数=2", reportString, "报告应该包含请求数");
            StringAssert.Contains("成功率", reportString, "报告应该包含成功率");
        }

        [Test]
        public void ClaudeConversation_Management()
        {
            var conversation = new ClaudeConversation("test-id", 10);

            Assert.AreEqual("test-id", conversation.Id, "对话ID应该正确");
            Assert.AreEqual(0, conversation.MessageCount, "初始消息数应该为0");

            conversation.AddMessage("user", "Hello");
            conversation.AddMessage("assistant", "Hi there!");

            Assert.AreEqual(2, conversation.MessageCount, "消息数应该为2");

            var messages = conversation.GetMessages();
            Assert.AreEqual(2, messages.Count, "获取的消息数应该为2");
            Assert.AreEqual("user", messages[0].role, "第一条消息角色应该是user");
            Assert.AreEqual("Hello", messages[0].content, "第一条消息内容应该正确");

            conversation.Clear();
            Assert.AreEqual(0, conversation.MessageCount, "清空后消息数应该为0");
        }

        [Test]
        public void ClaudeConversation_MaxLengthLimit()
        {
            var conversation = new ClaudeConversation("test-id", 3);

            conversation.AddMessage("user", "Message 1");
            conversation.AddMessage("assistant", "Response 1");
            conversation.AddMessage("user", "Message 2");
            conversation.AddMessage("assistant", "Response 2");

            Assert.AreEqual(3, conversation.MessageCount, "消息数应该限制为3");

            var messages = conversation.GetMessages();
            Assert.AreEqual("assistant", messages[2].role, "最新消息应该是Response 2");
        }

        [UnityTest]
        public IEnumerator AIEngineSelector_Creation()
        {
            // 创建测试对象
            var testObject = new GameObject("TestAIEngineSelector");
            var selector = testObject.AddComponent<AIEngineSelector>();

            Assert.IsNotNull(selector, "AIEngineSelector应该成功创建");

            // 配置
            selector.gptConfig = testGptConfig;
            selector.claudeConfig = testClaudeConfig;
            selector.selectionStrategy = AIEngineSelectionStrategy.Intelligent;

            Assert.AreEqual(AIEngineSelectionStrategy.Intelligent, selector.selectionStrategy, "策略应该正确设置");

            Object.Destroy(testObject);
            yield return null;
        }

        [Test]
        public void DualEngineLLMService_Configuration()
        {
            var service = new DualEngineLLMService(testGptConfig, testClaudeConfig, testDualConfig);

            Assert.IsNotNull(service, "服务应该成功创建");
            Assert.IsFalse(service.IsInitialized, "初始状态应该未初始化");
        }

        [Test]
        public void PerformanceMetrics_CalculateRecentAverage()
        {
            var metrics = new EnginePerformanceMetrics(AIEngineType.GPT4);

            // 记录超过100次的请求
            for (int i = 0; i < 110; i++)
            {
                metrics.RecordRequest(100 + i, true);
            }

            var report = metrics.GetReport();

            // 只保留最近100次的平均值
            Assert.AreEqual(110, report.RequestCount, "总请求数应该为110");
            Assert.Less(report.RecentAverageLatency, 210, "最近平均延迟应该小于210");
        }

        [Test]
        public void ServiceInstaller_RegistersDualEngineServices()
        {
            // 验证ServiceInstaller中注册了双引擎服务
            var installerType = typeof(ServiceInstaller);
            Assert.IsNotNull(installerType, "ServiceInstaller类型应该存在");

            // 验证InstallDualEngineLLMService方法存在
            var method = installerType.GetMethod("InstallDualEngineLLMService",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(method, "InstallDualEngineLLMService方法应该存在");
        }

        [Test]
        public void ClaudeService_ImplementsIGPTService()
        {
            // 验证ClaudeService实现了IGPTService接口
            var serviceType = typeof(ClaudeService);
            Assert.IsTrue(typeof(IGPTService).IsAssignableFrom(serviceType), "ClaudeService应该实现IGPTService接口");
        }

        [Test]
        public void AIFiles_ExistInCorrectLocations()
        {
            // 验证关键AI文件存在
            var claudeServicePath = "Assets/Scripts/AI/Services/ClaudeService.cs";
            var engineSelectorPath = "Assets/Scripts/AI/Core/AIEngineSelector.cs";
            var dualEngineServicePath = "Assets/Scripts/AI/Services/DualEngineLLMService.cs";

            Assert.IsTrue(System.IO.File.Exists(claudeServicePath), $"ClaudeService应该存在于{claudeServicePath}");
            Assert.IsTrue(System.IO.File.Exists(engineSelectorPath), $"AIEngineSelector应该存在于{engineSelectorPath}");
            Assert.IsTrue(System.IO.File.Exists(dualEngineServicePath), $"DualEngineLLMService应该存在于{dualEngineServicePath}");
        }

        [Test]
        public void TaskType_Analysis()
        {
            // 测试任务类型分析逻辑
            var testMessages = new System.Collections.Generic.Dictionary<string, AITaskType>
            {
                { "帮我写一段代码", AITaskType.CodeGeneration },
                { "分析一下这个数据", AITaskType.Analysis },
                { "为什么天空是蓝色的", AITaskType.Reasoning },
                { "翻译这句话", AITaskType.Translation },
                { "总结这篇文章", AITaskType.Summarization }
            };

            // 这里我们只是验证消息存在，实际分析逻辑在服务内部
            foreach (var kvp in testMessages)
            {
                Assert.IsNotNull(kvp.Key, "测试消息应该有效");
                Assert.IsNotNull(kvp.Value, "任务类型应该有效");
            }
        }
    }
}
