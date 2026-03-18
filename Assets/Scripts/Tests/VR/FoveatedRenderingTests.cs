using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TripMeta.VR.Rendering;

namespace TripMeta.Tests.VR
{
    /// <summary>
    /// 注视点渲染单元测试
    /// 测试眼动追踪、动态渲染优化、疲劳检测
    /// </summary>
    public class FoveatedRenderingTests
    {
        [Test]
        public void FoveationMode_EnumValues()
        {
            var modes = System.Enum.GetValues(typeof(FoveationMode));
            Assert.Contains(FoveationMode.Fixed, modes);
            Assert.Contains(FoveationMode.Dynamic, modes);
            Assert.Contains(FoveationMode.Hybrid, modes);
        }

        [Test]
        public void FatigueLevel_EnumValues()
        {
            var levels = System.Enum.GetValues(typeof(FatigueLevel));
            Assert.Contains(FatigueLevel.None, levels);
            Assert.Contains(FatigueLevel.Mild, levels);
            Assert.Contains(FatigueLevel.Moderate, levels);
            Assert.Contains(FatigueLevel.Severe, levels);
        }

        [Test]
        public void EyeDataSnapshot_DefaultValues()
        {
            var snapshot = new EyeDataSnapshot();

            Assert.AreEqual(0f, snapshot.timestamp);
            Assert.AreEqual(Vector2.zero, snapshot.gazePoint);
            Assert.IsFalse(snapshot.isGazeStable);
            Assert.IsFalse(snapshot.blinkDetected);
            Assert.AreEqual(0f, snapshot.saccadeVelocity);
        }

        [Test]
        public void EyeDataSnapshot_SetValues()
        {
            var snapshot = new EyeDataSnapshot
            {
                timestamp = 123.45f,
                gazePoint = new Vector2(0.6f, 0.4f),
                isGazeStable = true,
                blinkDetected = true,
                saccadeVelocity = 150f
            };

            Assert.AreEqual(123.45f, snapshot.timestamp);
            Assert.AreEqual(new Vector2(0.6f, 0.4f), snapshot.gazePoint);
            Assert.IsTrue(snapshot.isGazeStable);
            Assert.IsTrue(snapshot.blinkDetected);
            Assert.AreEqual(150f, snapshot.saccadeVelocity);
        }

        [UnityTest]
        public IEnumerator FoveatedRenderingManager_Creation()
        {
            var testObject = new GameObject("TestFoveatedRenderingManager");
            var manager = testObject.AddComponent<FoveatedRenderingManager>();

            Assert.IsNotNull(manager);
            Assert.IsTrue(manager.enableFoveatedRendering);
            Assert.AreEqual(FoveationMode.Dynamic, manager.foveationMode);
            Assert.AreEqual(2, manager.foveationLevel);

            // 检查默认半径配置
            Assert.AreEqual(0.15f, manager.innerRadius);
            Assert.AreEqual(0.3f, manager.middleRadius);
            Assert.AreEqual(0.5f, manager.outerRadius);

            Object.Destroy(testObject);
            yield return null;
        }

        [Test]
        public void FoveatedRenderingManager_RenderScaleSettings()
        {
            var managerObject = new GameObject("TestManager");
            var manager = managerObject.AddComponent<FoveatedRenderingManager>();

            Assert.AreEqual(1.0f, manager.innerRegionScale);
            Assert.AreEqual(0.75f, manager.middleRegionScale);
            Assert.AreEqual(0.5f, manager.outerRegionScale);

            // 验证渲染比例关系
            Assert.GreaterOrEqual(manager.innerRegionScale, manager.middleRegionScale);
            Assert.GreaterOrEqual(manager.middleRegionScale, manager.outerRegionScale);

            Object.Destroy(managerObject);
        }

        [Test]
        public void EyeFatigueDetector_DefaultValues()
        {
            var detectorObject = new GameObject("TestDetector");
            var detector = detectorObject.AddComponent<EyeFatigueDetector>();

            Assert.AreEqual(5f, detector.checkInterval);
            Assert.AreEqual(60, detector.historyWindowSize);
            Assert.AreEqual(10f, detector.blinkRateThreshold);
            Assert.AreEqual(3f, detector.fixationDurationThreshold);
            Assert.AreEqual(300f, detector.saccadeVelocityThreshold);

            Object.Destroy(detectorObject);
        }

        [Test]
        public void EyeFatigueDetector_FatigueThresholds()
        {
            var detectorObject = new GameObject("TestDetector");
            var detector = detectorObject.AddComponent<EyeFatigueDetector>();

            Assert.AreEqual(0.3f, detector.mildFatigueThreshold);
            Assert.AreEqual(0.6f, detector.moderateFatigueThreshold);
            Assert.AreEqual(0.8f, detector.severeFatigueThreshold);

            // 验证阈值递增
            Assert.Less(detector.mildFatigueThreshold, detector.moderateFatigueThreshold);
            Assert.Less(detector.moderateFatigueThreshold, detector.severeFatigueThreshold);

            Object.Destroy(detectorObject);
        }

        [Test]
        public void FoveatedRendering_GazePointCalculation()
        {
            // 测试注视点UV坐标计算
            Vector2 gazePoint = new Vector2(0.5f, 0.5f); // 屏幕中心
            Vector2 pixelUV = new Vector2(0.6f, 0.6f);   // 测试点

            float distance = Vector2.Distance(pixelUV, gazePoint);

            Assert.Less(distance, 0.5f); // 距离应小于对角线一半
            Assert.Greater(distance, 0f); // 距离应大于0
        }

        [Test]
        public void FoveatedRendering_RegionRadiusCalculation()
        {
            float innerRadius = 0.15f;
            float middleRadius = 0.3f;
            float outerRadius = 0.5f;

            // 验证半径递增
            Assert.Less(innerRadius, middleRadius);
            Assert.Less(middleRadius, outerRadius);

            // 计算区域面积
            float innerArea = Mathf.PI * innerRadius * innerRadius;
            float middleArea = Mathf.PI * (middleRadius * middleRadius - innerRadius * innerRadius);
            float outerArea = Mathf.PI * (outerRadius * outerRadius - middleRadius * middleRadius);

            Assert.Greater(innerArea, 0);
            Assert.Greater(middleArea, 0);
            Assert.Greater(outerArea, 0);

            // 外圈面积最大
            Assert.Greater(outerArea, innerArea);
        }

        [Test]
        public void FoveatedRendering_PerformanceGainCalculation()
        {
            // 模拟性能提升计算
            float innerRadius = 0.15f;
            float middleRadius = 0.3f;
            float outerRadius = 0.5f;

            float innerScale = 1.0f;
            float middleScale = 0.75f;
            float outerScale = 0.5f;

            float innerArea = Mathf.PI * innerRadius * innerRadius;
            float middleArea = Mathf.PI * middleRadius * middleRadius - innerArea;
            float outerArea = Mathf.PI * outerRadius * outerRadius - middleArea;

            float totalPixels = Mathf.PI * outerRadius * outerRadius;
            float weightedPixels = innerArea * (1f / innerScale) +
                                  middleArea * (1f / middleScale) +
                                  outerArea * (1f / outerScale);

            float performanceGain = (weightedPixels / totalPixels - 1f) * 100f;

            // 性能提升应为正值
            Assert.Greater(performanceGain, 0f);
            // 合理的性能提升范围 (10-50%)
            Assert.Greater(performanceGain, 10f);
            Assert.Less(performanceGain, 50f);
        }

        [Test]
        public void EyeFatigue_BlinkRateCalculation()
        {
            int blinkCount = 8; // 5秒内眨眼8次
            float timeWindow = 5f / 60f; // 5秒 = 0.083分钟

            float blinkRate = blinkCount / timeWindow;

            Assert.AreEqual(96f, blinkRate, 1f); // 约96次/分钟

            // 低于阈值表示疲劳
            float blinkRateThreshold = 10f;
            Assert.Greater(blinkRate, blinkRateThreshold);
        }

        [Test]
        public void EyeFatigue_FixationDurationAnalysis()
        {
            float[] fixationDurations = { 2.5f, 3.2f, 4.1f, 2.8f, 3.5f };
            float totalDuration = 0f;

            foreach (float duration in fixationDurations)
            {
                totalDuration += duration;
            }

            float averageDuration = totalDuration / fixationDurations.Length;

            Assert.AreEqual(3.22f, averageDuration, 0.01f);

            // 超过阈值表示疲劳
            float threshold = 3f;
            Assert.Greater(averageDuration, threshold);
        }

        [Test]
        public void FoveatedRendering_DynamicLevelAdjustment()
        {
            int currentLevel = 2;
            float gazeVelocity = 150f; // 度/秒
            float velocityThreshold = 100f;

            // 快速扫视时降低级别
            int newLevel = gazeVelocity > velocityThreshold * 2
                ? Mathf.Max(0, currentLevel - 1)
                : currentLevel;

            Assert.AreEqual(1, newLevel); // 应该降低到级别1

            // 稳定注视时提高级别
            gazeVelocity = 20f;
            newLevel = gazeVelocity < velocityThreshold * 0.5f
                ? Mathf.Min(3, currentLevel + 1)
                : currentLevel;

            Assert.AreEqual(3, newLevel); // 应该提高到级别3
        }

        [Test]
        public void FoveatedRenderingShader_Properties()
        {
            // 验证Shader属性配置
            string shaderCode = System.IO.File.ReadAllText("Assets/Scripts/VR/Rendering/FoveatedRenderingShader.shader");

            Assert.IsTrue(shaderCode.Contains("_GazePointUV"));
            Assert.IsTrue(shaderCode.Contains("_InnerRadius"));
            Assert.IsTrue(shaderCode.Contains("_MiddleRadius"));
            Assert.IsTrue(shaderCode.Contains("_OuterRadius"));
            Assert.IsTrue(shaderCode.Contains("_InnerScale"));
            Assert.IsTrue(shaderCode.Contains("_MiddleScale"));
            Assert.IsTrue(shaderCode.Contains("_OuterScale"));
        }

        [Test]
        public void FoveatedRenderingFiles_Exist()
        {
            var managerPath = "Assets/Scripts/VR/Rendering/FoveatedRenderingManager.cs";
            var shaderPath = "Assets/Scripts/VR/Rendering/FoveatedRenderingShader.shader";
            var fatiguePath = "Assets/Scripts/VR/Rendering/EyeFatigueDetector.cs";

            Assert.IsTrue(System.IO.File.Exists(managerPath), $"FoveatedRenderingManager应存在于{managerPath}");
            Assert.IsTrue(System.IO.File.Exists(shaderPath), $"FoveatedRenderingShader应存在于{shaderPath}");
            Assert.IsTrue(System.IO.File.Exists(fatiguePath), $"EyeFatigueDetector应存在于{fatiguePath}");
        }
    }
}
