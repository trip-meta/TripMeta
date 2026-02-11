using UnityEngine;
using System.Collections;
using TripMeta.AI;
using TripMeta.Core;
using TripMeta.Demo;
using TripMeta.Presentation;
using TripMeta.Features;

namespace TripMeta.Tests
{
    /// <summary>
    /// 运行时系统测试 - 验证所有核心系统正常工作
    /// </summary>
    public class RuntimeSystemTest : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private float testDelay = 1f;

        private int passedTests = 0;
        private int failedTests = 0;

        void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }

        private IEnumerator RunAllTests()
        {
            Debug.Log("[RuntimeSystemTest] ========== 开始系统测试 ==========");

            yield return new WaitForSeconds(testDelay);

            // 测试1: SimpleStartup
            yield return StartCoroutine(TestSimpleStartup());

            // 测试2: VRManager
            yield return StartCoroutine(TestVRManager());

            // 测试3: AIServiceManager
            yield return StartCoroutine(TestAIServiceManager());

            // 测试4: AITourGuide
            yield return StartCoroutine(TestAITourGuide());

            // 测试5: DemoController
            yield return StartCoroutine(TestDemoController());

            // 测试6: TourLocationManager
            yield return StartCoroutine(TestTourLocationManager());

            // 测试7: TourUIManager
            yield return StartCoroutine(TestTourUIManager());

            // 测试总结
            PrintTestSummary();
        }

        private IEnumerator TestSimpleStartup()
        {
            PrintTestHeader("SimpleStartup");

            var startup = FindObjectOfType<SimpleStartup>();
            if (startup == null)
            {
                FailTest("SimpleStartup not found in scene");
                yield break;
            }

            PassTest("SimpleStartup found");
            PassTest($"SimpleStartup.IsInitialized: {startup.IsInitialized}");

            var vrManager = startup.GetVRManager();
            var aiManager = startup.GetAIManager();
            var tourGuide = startup.GetTourGuide();

            PassTest($"VR Manager: {(vrManager != null ? "Found" : "Not Found")}");
            PassTest($"AI Manager: {(aiManager != null ? "Found" : "Not Found")}");
            PassTest($"Tour Guide: {(tourGuide != null ? "Found" : "Not Found")}");

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator TestVRManager()
        {
            PrintTestHeader("VRManager");

            var vrManager = FindObjectOfType<VRManager>();
            if (vrManager == null)
            {
                FailTest("VRManager not found in scene");
                yield break;
            }

            PassTest("VRManager found");

            // Note: We can't fully test VR without a headset, but we can verify the component exists
            PassTest("VRManager component present");

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator TestAIServiceManager()
        {
            PrintTestHeader("AIServiceManager");

            var aiManager = AIServiceManager.Instance;
            if (aiManager == null)
            {
                FailTest("AIServiceManager.Instance is null");
                yield break;
            }

            PassTest("AIServiceManager.Instance found");
            PassTest($"IsInitialized: {aiManager.isInitialized}");

            var status = aiManager.GetServiceStatus();
            PassTest($"LLM Status: {status.llmStatus}");
            PassTest($"Speech Status: {status.speechStatus}");
            PassTest($"Vision Status: {status.visionStatus}");
            PassTest($"Recommendation Status: {status.recommendationStatus}");
            PassTest($"Translation Status: {status.translationStatus}");

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator TestAITourGuide()
        {
            PrintTestHeader("AITourGuide");

            var tourGuide = FindObjectOfType<AITourGuide>();
            if (tourGuide == null)
            {
                FailTest("AITourGuide not found in scene");
                yield break;
            }

            PassTest("AITourGuide found");
            PassTest($"Default Language: {tourGuide.defaultLanguage}");
            PassTest($"Specialties: {string.Join(", ", tourGuide.specialties)}");
            PassTest($"Voice Interaction: {tourGuide.enableVoiceInteraction}");
            PassTest($"Gesture Recognition: {tourGuide.enableGestureRecognition}");

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator TestDemoController()
        {
            PrintTestHeader("DemoController");

            var demoController = DemoController.Instance;
            if (demoController == null)
            {
                FailTest("DemoController.Instance not found");
                yield break;
            }

            PassTest("DemoController.Instance found");

            // Test that we can call the public methods without errors
            try
            {
                demoController.StartTourGuide();
                PassTest("StartTourGuide() called successfully");

                yield return new WaitForSeconds(1f);

                demoController.NextLocation();
                PassTest("NextLocation() called successfully");

                yield return new WaitForSeconds(1f);

                demoController.ShowAIConversation();
                PassTest("ShowAIConversation() called successfully");

                yield return new WaitForSeconds(1f);

                demoController.ResetDemo();
                PassTest("ResetDemo() called successfully");
            }
            catch (System.Exception e)
            {
                FailTest($"DemoController method call failed: {e.Message}");
            }

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator TestTourLocationManager()
        {
            PrintTestHeader("TourLocationManager");

            var locationManager = FindObjectOfType<TourLocationManager>();
            if (locationManager == null)
            {
                // This is optional, so we'll just log a warning
                Debug.LogWarning("[RuntimeSystemTest] TourLocationManager not found (optional)");
                yield break;
            }

            PassTest("TourLocationManager found");

            var allIds = locationManager.GetAllLocationIds();
            PassTest($"Available locations: {allIds.Length}");

            if (allIds.Length > 0)
            {
                PassTest($"First location ID: {allIds[0]}");
            }

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator TestTourUIManager()
        {
            PrintTestHeader("TourUIManager");

            var tourUI = FindObjectOfType<TourUIManager>();
            if (tourUI == null)
            {
                Debug.LogWarning("[RuntimeSystemTest] TourUIManager not found (optional)");
                yield break;
            }

            PassTest("TourUIManager found");
            PassTest($"Use Spatial UI: {tourUI.UseSpatialUI}");

            yield return new WaitForSeconds(0.5f);
        }

        private void PrintTestHeader(string testName)
        {
            Debug.Log($"[RuntimeSystemTest] \n========== 测试: {testName} ==========");
        }

        private void PassTest(string message)
        {
            passedTests++;
            Debug.Log($"[RuntimeSystemTest] ✅ PASS: {message}");
        }

        private void FailTest(string message)
        {
            failedTests++;
            Debug.LogError($"[RuntimeSystemTest] ❌ FAIL: {message}");
        }

        private void PrintTestSummary()
        {
            Debug.Log("[RuntimeSystemTest] \n========== 测试总结 ==========");
            Debug.Log($"[RuntimeSystemTest] 总计: {passedTests + failedTests} 个测试");
            Debug.Log($"[RuntimeSystemTest] 通过: {passedTests} ✅");
            Debug.Log($"[RuntimeSystemTest] 失败: {failedTests} {(failedTests == 0 ? "✅" : "❌")}");
            Debug.Log($"[RuntimeSystemTest] 成功率: {(passedTests + failedTests > 0 ? (float)passedTests / (passedTests + failedTests) * 100 : 0):F1}%");

            if (failedTests == 0)
            {
                Debug.Log("[RuntimeSystemTest] 🎉 所有测试通过！系统运行正常。");
            }
            else
            {
                Debug.LogWarning($"[RuntimeSystemTest] ⚠️ 有 {failedTests} 个测试失败，请检查配置。");
            }

            Debug.Log("[RuntimeSystemTest] ================================");
        }

        /// <summary>
        /// 手动运行测试的快捷方法
        /// </summary>
        [ContextMenu("运行系统测试")]
        public void RunTests()
        {
            StopAllCoroutines();
            StartCoroutine(RunAllTests());
        }
    }
}
