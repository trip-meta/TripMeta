using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Commerce
{
    /// <summary>
    /// 优惠券服务
    /// 处理优惠券验证和应用
    /// </summary>
    public class CouponService
    {
        /// <summary>
        /// 验证优惠券
        /// </summary>
        public async Task<CouponValidationResult> ValidateCoupon(string couponCode, string tierId)
        {
            await Task.Delay(300);

            // 模拟优惠券验证
            if (couponCode.ToUpper() == "WELCOME50")
            {
                return new CouponValidationResult
                {
                    valid = true,
                    discountAmount = 5.00m,
                    discountPercentage = 50,
                    message = "50% off your first month!"
                };
            }
            else if (couponCode.ToUpper() == "ANNUAL20")
            {
                return new CouponValidationResult
                {
                    valid = true,
                    discountAmount = 0,
                    discountPercentage = 20,
                    message = "20% off annual subscription!"
                };
            }

            return new CouponValidationResult
            {
                valid = false,
                message = "Invalid or expired coupon code"
            };
        }
    }

    /// <summary>
    /// 优惠券验证结果
    /// </summary>
    public class CouponValidationResult
    {
        public bool valid;
        public decimal discountAmount;
        public int discountPercentage;
        public string message;
    }
}
