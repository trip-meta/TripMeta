using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.Commerce;

namespace TripMeta.Tests.Commerce
{
    /// <summary>
    /// 订阅管理器单元测试
    /// </summary>
    public class SubscriptionManagerTests
    {
        private GameObject testObject;
        private SubscriptionManager manager;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestSubscriptionManager");
            manager = testObject.AddComponent<SubscriptionManager>();
            manager.defaultTierId = "basic";
            manager.enableFreeTrial = true;
            manager.freeTrialDays = 7;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator SubscriptionManager_Initialization_HasTiers()
        {
            yield return null;

            Assert.IsNotNull(manager);
            Assert.Greater(manager.AvailableTiers.Count, 0);
            Debug.Log($"订阅层级数量: {manager.AvailableTiers.Count}");
        }

        [UnityTest]
        public IEnumerator SubscriptionManager_HasAllRequiredTiers()
        {
            yield return null;

            var tierIds = manager.AvailableTiers.Select(t => t.tierId).ToList();
            Assert.Contains("free", tierIds);
            Assert.Contains("basic", tierIds);
            Assert.Contains("premium", tierIds);
            Assert.Contains("family", tierIds);
            Assert.Contains("enterprise", tierIds);
        }

        [Test]
        public void SubscriptionTier_HasPricing()
        {
            var tier = new SubscriptionTier
            {
                tierId = "premium",
                name = "Premium",
                monthlyPrice = 19.99m,
                yearlyPrice = 199.99m
            };

            Assert.AreEqual(19.99m, tier.monthlyPrice);
            Assert.AreEqual(199.99m, tier.yearlyPrice);
        }

        [Test]
        public void UserSubscription_HasStatus()
        {
            var subscription = new UserSubscription
            {
                subscriptionId = "sub_123",
                tierId = "premium",
                status = SubscriptionStatus.Active,
                startDate = System.DateTime.Now,
                endDate = System.DateTime.Now.AddMonths(1),
                autoRenew = true
            };

            Assert.AreEqual("sub_123", subscription.subscriptionId);
            Assert.AreEqual(SubscriptionStatus.Active, subscription.status);
            Assert.IsTrue(subscription.autoRenew);
        }

        [UnityTest]
        public IEnumerator SubscriptionManager_GetRecommendedTier_ReturnsPopularTier()
        {
            yield return null;

            var recommended = manager.GetRecommendedTier();
            Assert.IsNotNull(recommended);
            Assert.IsTrue(recommended.isPopular);
        }

        [Test]
        public void PaymentMethod_StoresCardInfo()
        {
            var method = new PaymentMethod
            {
                methodId = "pm_123",
                type = PaymentMethodType.CreditCard,
                lastFour = "4242",
                expiryMonth = "12",
                expiryYear = "2025",
                isDefault = true
            };

            Assert.AreEqual("4242", method.lastFour);
            Assert.AreEqual(PaymentMethodType.CreditCard, method.type);
            Assert.IsTrue(method.isDefault);
        }

        [Test]
        public void PaymentResult_StoresTransactionData()
        {
            var result = new PaymentResult
            {
                success = true,
                transactionId = "txn_123",
                amount = 19.99m,
                currency = "USD"
            };

            Assert.IsTrue(result.success);
            Assert.AreEqual("txn_123", result.transactionId);
            Assert.AreEqual(19.99m, result.amount);
        }

        [Test]
        public void RefundEligibility_ChecksConditions()
        {
            var eligibility = new RefundEligibility
            {
                isEligible = true,
                maxRefundAmount = 99.99m,
                conditions = new[] { "Within 30 days", "No usage" },
                eligibleUntil = System.DateTime.Now.AddDays(30)
            };

            Assert.IsTrue(eligibility.isEligible);
            Assert.AreEqual(99.99m, eligibility.maxRefundAmount);
            Assert.AreEqual(2, eligibility.conditions.Length);
        }

        [UnityTest]
        public IEnumerator CouponService_ValidateCoupon_ReturnsResult()
        {
            var service = new CouponService();
            var task = service.ValidateCoupon("WELCOME50", "premium");

            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsNotNull(task.Result);
            Assert.IsTrue(task.Result.valid);
            Assert.AreEqual(50, task.Result.discountPercentage);
        }

        [UnityTest]
        public IEnumerator RefundService_CheckEligibility_ReturnsEligibility()
        {
            var service = new RefundService();
            var eligibility = service.CheckEligibility("txn_123");

            Assert.IsNotNull(eligibility);
            Assert.IsTrue(eligibility.isEligible);
            yield return null;
        }

        [Test]
        public void SubscriptionManager_SupportsMultipleCurrencies()
        {
            Assert.Contains("USD", manager.supportedCurrencies);
            Assert.Contains("EUR", manager.supportedCurrencies);
        }
    }
}
