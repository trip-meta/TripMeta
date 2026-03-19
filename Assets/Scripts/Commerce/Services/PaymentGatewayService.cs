using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Commerce
{
    /// <summary>
    /// 支付网关服务
    /// 处理各种支付方式的集成
    /// </summary>
    public class PaymentGatewayService
    {
        /// <summary>
        /// 处理支付
        /// </summary>
        public async Task<PaymentResult> ProcessPayment(decimal amount, string currency, string paymentMethodId, string description)
        {
            // 模拟支付处理
            await Task.Delay(1500);

            // 模拟成功率 95%
            if (Random.value > 0.05f)
            {
                return new PaymentResult
                {
                    success = true,
                    transactionId = "txn_" + System.Guid.NewGuid().ToString("N").Substring(0, 16),
                    amount = amount,
                    currency = currency,
                    timestamp = System.DateTime.Now
                };
            }
            else
            {
                return new PaymentResult
                {
                    success = false,
                    error = "Payment declined by bank"
                };
            }
        }

        /// <summary>
        /// 添加支付方式
        /// </summary>
        public async Task<PaymentMethodResult> AddPaymentMethod(PaymentMethodType type, string token)
        {
            await Task.Delay(800);

            return new PaymentMethodResult
            {
                success = true,
                paymentMethod = new PaymentMethod
                {
                    methodId = "pm_" + System.Guid.NewGuid().ToString("N").Substring(0, 16),
                    type = type,
                    lastFour = "4242",
                    expiryMonth = "12",
                    expiryYear = "2025",
                    isDefault = false
                }
            };
        }
    }
}
