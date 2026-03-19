using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Localization;

namespace TripMeta.Tests.Localization
{
    /// <summary>
    /// 区域化内容管理器单元测试
    /// </summary>
    public class RegionalContentManagerTests
    {
        private GameObject testObject;
        private RegionalContentManager manager;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestRegionalManager");
            manager = testObject.AddComponent<RegionalContentManager>();
            manager.autoDetectRegion = false;
            manager.defaultRegion = RegionType.AsiaPacific;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator RegionalManager_Initialization_HasContentLibraries()
        {
            yield return null;

            Assert.IsNotNull(manager);
            Assert.Greater(manager.ContentLibraries.Count, 0);
            Debug.Log($"内容库数量: {manager.ContentLibraries.Count}");
        }

        [UnityTest]
        public IEnumerator RegionalManager_SetRegion_ChangesRegion()
        {
            yield return null;

            bool regionChanged = false;
            RegionType changedRegion = RegionType.AsiaPacific;
            manager.OnRegionChanged += (region) =>
            {
                regionChanged = true;
                changedRegion = region;
            };

            manager.SetRegion(RegionType.Europe);

            Assert.IsTrue(regionChanged);
            Assert.AreEqual(RegionType.Europe, changedRegion);
            Assert.AreEqual(RegionType.Europe, manager.CurrentRegion);
        }

        [Test]
        public void RegionalContentLibrary_HasRequiredData()
        {
            var library = new RegionalContentLibrary
            {
                region = RegionType.AsiaPacific,
                name = "Asia Pacific",
                cultures = new[] { "chinese", "japanese" },
                languages = new[] { LanguageCode.zh_CN, LanguageCode.ja_JP },
                popularDestinations = new[] { "great_wall", "mount_fuji" },
                pricingMultiplier = 0.8f
            };

            Assert.AreEqual(RegionType.AsiaPacific, library.region);
            Assert.AreEqual("Asia Pacific", library.name);
            Assert.AreEqual(2, library.cultures.Length);
            Assert.AreEqual(2, library.languages.Length);
            Assert.AreEqual(0.8f, library.pricingMultiplier);
        }

        [Test]
        public void RegionalEvent_HasDateRange()
        {
            var evt = new RegionalEvent
            {
                name = "Cherry Blossom Season",
                startMonth = 3,
                endMonth = 4,
                type = EventType.Seasonal
            };

            Assert.AreEqual("Cherry Blossom Season", evt.name);
            Assert.AreEqual(3, evt.startMonth);
            Assert.AreEqual(4, evt.endMonth);
            Assert.AreEqual(EventType.Seasonal, evt.type);
        }

        [UnityTest]
        public IEnumerator RegionalManager_Supports6Regions()
        {
            yield return null;

            Assert.GreaterOrEqual(manager.ContentLibraries.Count, 6);
            Debug.Log($"支持 {manager.ContentLibraries.Count} 个区域");
        }

        [Test]
        public void LocalizedExperience_HasPricingData()
        {
            var experience = new LocalizedExperience
            {
                experienceId = "exp_001",
                region = RegionType.Europe,
                basePrice = 100m,
                localPrice = 110m,
                localCurrency = "EUR",
                complianceStatus = ComplianceStatus.Approved
            };

            Assert.AreEqual("exp_001", experience.experienceId);
            Assert.AreEqual(110m, experience.localPrice);
            Assert.AreEqual("EUR", experience.localCurrency);
            Assert.AreEqual(ComplianceStatus.Approved, experience.complianceStatus);
        }

        [Test]
        public void ComplianceReport_IdentifiesViolations()
        {
            var report = new ComplianceReport
            {
                experienceId = "exp_001",
                isCompliant = false,
                violations = new[] { "Inappropriate content", "Missing disclaimer" },
                recommendations = new[] { "Review content", "Add disclaimer" }
            };

            Assert.IsFalse(report.isCompliant);
            Assert.AreEqual(2, report.violations.Length);
            Assert.AreEqual(2, report.recommendations.Length);
        }

        [Test]
        public void RegionalPartner_HasCommissionRate()
        {
            var partner = new RegionalPartner
            {
                partnerId = "partner_001",
                name = "Tokyo Tours",
                type = PartnerType.TourOperator,
                commissionRate = 0.15f
            };

            Assert.AreEqual("Tokyo Tours", partner.name);
            Assert.AreEqual(PartnerType.TourOperator, partner.type);
            Assert.AreEqual(0.15f, partner.commissionRate);
        }

        [UnityTest]
        public IEnumerator RegionalManager_GetCurrentSeasonalEvents_ReturnsEvents()
        {
            yield return null;

            var events = manager.GetCurrentSeasonalEvents();
            Assert.IsNotNull(events);
        }

        [Test]
        public void RegionalConfig_HasPaymentMethods()
        {
            var config = new RegionalConfig
            {
                pricingTier = PricingTier.Standard,
                supportedPaymentMethods = new[] { "credit_card", "paypal", "crypto" },
                requireAgeVerification = true
            };

            Assert.AreEqual(PricingTier.Standard, config.pricingTier);
            Assert.AreEqual(3, config.supportedPaymentMethods.Length);
            Assert.IsTrue(config.requireAgeVerification);
        }

        [UnityTest]
        public IEnumerator ComplianceService_CheckExperience_ReturnsReport()
        {
            var service = new ComplianceService();
            var task = service.CheckExperience("exp_001", RegionType.AsiaPacific, new[] { "no_gambling" });

            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsNotNull(task.Result);
            Assert.AreEqual("exp_001", task.Result.experienceId);
            Assert.IsNotNull(task.Result.violations);
        }
    }
}
