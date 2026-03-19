using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Commerce
{
    /// <summary>
    /// 订阅管理器
    /// 管理多层级订阅、支付处理、优惠券、退款
    /// </summary>
    public class SubscriptionManager : MonoBehaviour
    {
        [Header("订阅配置")]
        public List<SubscriptionTier> subscriptionTiers = new List<SubscriptionTier>();
        public string defaultTierId = "basic";
        public bool enableFreeTrial = true;
        public int freeTrialDays = 7;

        [Header("支付配置")]
        public string stripeApiKey = "";
        public string paypalClientId = "";
        public bool enableCryptoPayments = true;
        public List<string> supportedCurrencies = new List<string> { "USD", "EUR", "CNY", "JPY" };

        [Header("功能开关")]
        public bool enableCoupons = true;
        public bool enableReferralProgram = true;
        public bool enableFamilyPlans = true;
        public bool autoRenewalDefault = true;

        // 当前用户订阅
        private UserSubscription currentSubscription;
        private List<PaymentMethod> savedPaymentMethods = new List<PaymentMethod>();

        // 服务
        private PaymentGatewayService paymentService;
        private CouponService couponService;
        private RefundService refundService;

        public static SubscriptionManager Instance { get; private set; }

        public UserSubscription CurrentSubscription => currentSubscription;
        public IReadOnlyList<SubscriptionTier> AvailableTiers => subscriptionTiers;
        public bool IsSubscribed => currentSubscription?.status == SubscriptionStatus.Active;

        // 事件
        public event Action<UserSubscription> OnSubscriptionChanged;
        public event Action<PaymentResult> OnPaymentProcessed;
        public event Action<RefundResult> OnRefundProcessed;
        public event Action<string> OnBillingError;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            InitializeTiers();
            InitializeServices();
            LoadUserSubscription();

            Debug.Log("[SubscriptionManager] 订阅管理器初始化完成");
        }

        /// <summary>
        /// 初始化订阅层级
        /// </summary>
        private void InitializeTiers()
        {
            subscriptionTiers = new List<SubscriptionTier>
            {
                new SubscriptionTier
                {
                    tierId = "free",
                    name = "Free",
                    description = "Basic VR tourism experience",
                    monthlyPrice = 0,
                    yearlyPrice = 0,
                    features = new[] { "3 free tours/month", "Basic AI guide", "Standard quality" },
                    limitations = new[] { "No multiplayer", "No cloud rendering", "Ads included" },
                    maxUsers = 1,
                    isPopular = false
                },
                new SubscriptionTier
                {
                    tierId = "basic",
                    name = "Basic",
                    description = "Enhanced VR tourism with AI guide",
                    monthlyPrice = 9.99m,
                    yearlyPrice = 99.99m,
                    features = new[] { "Unlimited tours", "AI guide - Basic", "HD quality", "Multiplayer (4 people)", "Cloud rendering" },
                    limitations = new[] { "No Web3 features", "Standard support" },
                    maxUsers = 1,
                    isPopular = false
                },
                new SubscriptionTier
                {
                    tierId = "premium",
                    name = "Premium",
                    description = "Full-featured VR tourism experience",
                    monthlyPrice = 19.99m,
                    yearlyPrice = 199.99m,
                    features = new[] { "Unlimited tours", "AI guide - Premium", "4K quality", "Multiplayer (10 people)", "Cloud rendering", "Web3 wallet", "NFT collection", "Priority support" },
                    limitations = new[] { },
                    maxUsers = 1,
                    isPopular = true
                },
                new SubscriptionTier
                {
                    tierId = "family",
                    name = "Family",
                    description = "Share with up to 5 family members",
                    monthlyPrice = 29.99m,
                    yearlyPrice = 299.99m,
                    features = new[] { "All Premium features", "5 family members", "Parental controls", "Shared NFT collection", "Family cloud storage" },
                    limitations = new[] { },
                    maxUsers = 5,
                    isPopular = false
                },
                new SubscriptionTier
                {
                    tierId = "enterprise",
                    name = "Enterprise",
                    description = "For businesses and educational institutions",
                    monthlyPrice = 99.99m,
                    yearlyPrice = 999.99m,
                    features = new[] { "All features", "Unlimited users", "Custom branding", "API access", "Dedicated support", "SLA guarantee", "Analytics dashboard" },
                    limitations = new[] { },
                    maxUsers = -1, // Unlimited
                    isPopular = false
                }
            };
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        private void InitializeServices()
        {
            paymentService = new PaymentGatewayService();
            couponService = new CouponService();
            refundService = new RefundService();
        }

        /// <summary>
        /// 加载用户订阅
        /// </summary>
        private void LoadUserSubscription()
        {
            // 从本地存储或服务器加载
            currentSubscription = new UserSubscription
            {
                subscriptionId = "sub_default",
                tierId = "free",
                status = SubscriptionStatus.Active,
                startDate = DateTime.Now,
                endDate = DateTime.Now.AddYears(1),
                autoRenew = autoRenewalDefault,
                paymentMethod = null
            };
        }

        #region 订阅管理

        /// <summary>
        /// 订阅指定层级
        /// </summary>
        public async Task<SubscriptionResult> Subscribe(string tierId, string paymentMethodId, string couponCode = null)
        {
            try
            {
                var tier = subscriptionTiers.FirstOrDefault(t => t.tierId == tierId);
                if (tier == null)
                {
                    return new SubscriptionResult { success = false, error = "Invalid subscription tier" };
                }

                // 验证优惠券
                decimal discount = 0;
                if (enableCoupons && !string.IsNullOrEmpty(couponCode))
                {
                    var couponResult = await couponService.ValidateCoupon(couponCode, tierId);
                    if (couponResult.valid)
                    {
                        discount = couponResult.discountAmount;
                    }
                }

                // 计算价格
                decimal finalPrice = tier.monthlyPrice - discount;
                if (finalPrice < 0) finalPrice = 0;

                // 处理支付
                var paymentResult = await paymentService.ProcessPayment(
                    finalPrice,
                    "USD",
                    paymentMethodId,
                    $"Subscription to {tier.name}"
                );

                if (!paymentResult.success)
                {
                    OnBillingError?.Invoke(paymentResult.error);
                    return new SubscriptionResult { success = false, error = paymentResult.error };
                }

                // 创建订阅
                currentSubscription = new UserSubscription
                {
                    subscriptionId = Guid.NewGuid().ToString(),
                    tierId = tierId,
                    status = SubscriptionStatus.Active,
                    startDate = DateTime.Now,
                    endDate = DateTime.Now.AddMonths(1),
                    autoRenew = autoRenewalDefault,
                    paymentMethod = paymentMethodId,
                    lastPaymentDate = DateTime.Now,
                    nextBillingDate = DateTime.Now.AddMonths(1),
                    couponUsed = couponCode
                };

                OnSubscriptionChanged?.Invoke(currentSubscription);
                OnPaymentProcessed?.Invoke(paymentResult);

                Debug.Log($"[SubscriptionManager] 订阅成功: {tier.name}");
                return new SubscriptionResult { success = true, subscription = currentSubscription };
            }
            catch (Exception e)
            {
                Debug.LogError($"[SubscriptionManager] 订阅失败: {e.Message}");
                return new SubscriptionResult { success = false, error = e.Message };
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public async Task<bool> CancelSubscription(bool immediate = false)
        {
            if (currentSubscription == null || currentSubscription.status != SubscriptionStatus.Active)
            {
                return false;
            }

            if (immediate)
            {
                currentSubscription.status = SubscriptionStatus.Cancelled;
                currentSubscription.endDate = DateTime.Now;
            }
            else
            {
                currentSubscription.autoRenew = false;
                currentSubscription.status = SubscriptionStatus.PendingCancellation;
            }

            OnSubscriptionChanged?.Invoke(currentSubscription);
            Debug.Log("[SubscriptionManager] 订阅已取消");
            return true;
        }

        /// <summary>
        /// 升级订阅
        /// </summary>
        public async Task<SubscriptionResult> UpgradeSubscription(string newTierId)
        {
            if (currentSubscription == null)
            {
                return new SubscriptionResult { success = false, error = "No active subscription" };
            }

            var currentTier = subscriptionTiers.FirstOrDefault(t => t.tierId == currentSubscription.tierId);
            var newTier = subscriptionTiers.FirstOrDefault(t => t.tierId == newTierId);

            if (newTier == null || currentTier == null)
            {
                return new SubscriptionResult { success = false, error = "Invalid tier" };
            }

            // 计算差价
            decimal proratedAmount = CalculateProratedUpgrade(currentTier, newTier);

            // 支付差价
            if (proratedAmount > 0)
            {
                var paymentResult = await paymentService.ProcessPayment(
                    proratedAmount,
                    "USD",
                    currentSubscription.paymentMethod,
                    $"Upgrade to {newTier.name}"
                );

                if (!paymentResult.success)
                {
                    return new SubscriptionResult { success = false, error = paymentResult.error };
                }
            }

            // 更新订阅
            currentSubscription.tierId = newTierId;
            currentSubscription.lastPaymentDate = DateTime.Now;

            OnSubscriptionChanged?.Invoke(currentSubscription);
            Debug.Log($"[SubscriptionManager] 订阅已升级至: {newTier.name}");

            return new SubscriptionResult { success = true, subscription = currentSubscription };
        }

        /// <summary>
        /// 计算升级差价
        /// </summary>
        private decimal CalculateProratedUpgrade(SubscriptionTier currentTier, SubscriptionTier newTier)
        {
            if (currentSubscription == null) return newTier.monthlyPrice;

            var daysRemaining = (currentSubscription.endDate - DateTime.Now).Days;
            var daysInPeriod = 30;

            decimal currentValue = currentTier.monthlyPrice * daysRemaining / daysInPeriod;
            decimal newValue = newTier.monthlyPrice * daysRemaining / daysInPeriod;

            return Math.Max(0, newValue - currentValue);
        }

        #endregion

        #region 支付管理

        /// <summary>
        /// 添加支付方式
        /// </summary>
        public async Task<PaymentMethodResult> AddPaymentMethod(PaymentMethodType type, string token)
        {
            var result = await paymentService.AddPaymentMethod(type, token);

            if (result.success)
            {
                savedPaymentMethods.Add(result.paymentMethod);
            }

            return result;
        }

        /// <summary>
        /// 获取保存的支付方式
        /// </summary>
        public IReadOnlyList<PaymentMethod> GetSavedPaymentMethods()
        {
            return savedPaymentMethods;
        }

        /// <summary>
        /// 处理单次购买
        /// </summary>
        public async Task<PaymentResult> ProcessOneTimePurchase(string itemId, decimal amount, string currency, string paymentMethodId)
        {
            var result = await paymentService.ProcessPayment(amount, currency, paymentMethodId, $"Purchase: {itemId}");
            OnPaymentProcessed?.Invoke(result);
            return result;
        }

        #endregion

        #region 退款管理

        /// <summary>
        /// 申请退款
        /// </summary>
        public async Task<RefundResult> RequestRefund(string transactionId, decimal amount, string reason)
        {
            var result = await refundService.ProcessRefund(transactionId, amount, reason);
            OnRefundProcessed?.Invoke(result);
            return result;
        }

        /// <summary>
        /// 获取退款资格
        /// </summary>
        public RefundEligibility CheckRefundEligibility(string transactionId)
        {
            return refundService.CheckEligibility(transactionId);
        }

        #endregion

        #region 实用方法

        /// <summary>
        /// 获取当前层级的功能列表
        /// </summary>
        public string[] GetCurrentFeatures()
        {
            var tier = subscriptionTiers.FirstOrDefault(t => t.tierId == currentSubscription?.tierId);
            return tier?.features ?? new string[0];
        }

        /// <summary>
        /// 检查功能是否可用
        /// </summary>
        public bool IsFeatureAvailable(string featureName)
        {
            var tier = subscriptionTiers.FirstOrDefault(t => t.tierId == currentSubscription?.tierId);
            return tier?.features?.Contains(featureName) ?? false;
        }

        /// <summary>
        /// 获取推荐层级
        /// </summary>
        public SubscriptionTier GetRecommendedTier()
        {
            return subscriptionTiers.FirstOrDefault(t => t.isPopular) ?? subscriptionTiers[1];
        }

        #endregion

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    #region 数据类型

    /// <summary>
    /// 订阅层级
    /// </summary>
    [Serializable]
    public class SubscriptionTier
    {
        public string tierId;
        public string name;
        public string description;
        public decimal monthlyPrice;
        public decimal yearlyPrice;
        public string[] features;
        public string[] limitations;
        public int maxUsers;
        public bool isPopular;
    }

    /// <summary>
    /// 用户订阅
    /// </summary>
    [Serializable]
    public class UserSubscription
    {
        public string subscriptionId;
        public string tierId;
        public SubscriptionStatus status;
        public DateTime startDate;
        public DateTime endDate;
        public bool autoRenew;
        public string paymentMethod;
        public DateTime lastPaymentDate;
        public DateTime nextBillingDate;
        public string couponUsed;
    }

    /// <summary>
    /// 订阅状态
    /// </summary>
    public enum SubscriptionStatus
    {
        Active,
        Pending,
        PendingCancellation,
        Cancelled,
        Expired,
        Suspended
    }

    /// <summary>
    /// 支付方式
    /// </summary>
    [Serializable]
    public class PaymentMethod
    {
        public string methodId;
        public PaymentMethodType type;
        public string lastFour;
        public string expiryMonth;
        public string expiryYear;
        public bool isDefault;
    }

    /// <summary>
    /// 支付方式类型
    /// </summary>
    public enum PaymentMethodType
    {
        CreditCard,
        PayPal,
        Crypto,
        ApplePay,
        GooglePay
    }

    /// <summary>
    /// 订阅结果
    /// </summary>
    public class SubscriptionResult
    {
        public bool success;
        public string error;
        public UserSubscription subscription;
    }

    /// <summary>
    /// 支付结果
    /// </summary>
    public class PaymentResult
    {
        public bool success;
        public string error;
        public string transactionId;
        public decimal amount;
        public string currency;
        public DateTime timestamp;
    }

    /// <summary>
    /// 支付方式结果
    /// </summary>
    public class PaymentMethodResult
    {
        public bool success;
        public string error;
        public PaymentMethod paymentMethod;
    }

    /// <summary>
    /// 退款结果
    /// </summary>
    public class RefundResult
    {
        public bool success;
        public string error;
        public string refundId;
        public decimal amount;
        public DateTime timestamp;
    }

    /// <summary>
    /// 退款资格
    /// </summary>
    public class RefundEligibility
    {
        public bool isEligible;
        public decimal maxRefundAmount;
        public string[] conditions;
        public DateTime eligibleUntil;
    }

    #endregion
}
