using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Web3
{
    /// <summary>
    /// 质押服务
    /// 处理代币质押、解除质押、领取奖励等功能
    /// </summary>
    public class StakingService
    {
        private Web3Manager web3Manager;

        // 质押池配置
        private decimal baseAPR = 0.15m; // 15% 年利率
        private decimal rewardPerBlock = 0.0001m;

        public StakingService(Web3Manager manager)
        {
            web3Manager = manager;
        }

        /// <summary>
        /// 质押代币
        /// </summary>
        public async Task<bool> Stake(string userAddress, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    Debug.LogError("[StakingService] 质押金额必须大于0");
                    return false;
                }

                // 检查用户余额
                var balance = await web3Manager.GetTokenBalance();
                if (balance < amount)
                {
                    Debug.LogError("[StakingService] 余额不足");
                    return false;
                }

                // 调用智能合约的 stake 函数
                await Task.Delay(2500);

                Debug.Log($"[StakingService] 质押成功: {amount} TRIP");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[StakingService] 质押失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 解除质押
        /// </summary>
        public async Task<bool> Unstake(string userAddress, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    Debug.LogError("[StakingService] 解除质押金额必须大于0");
                    return false;
                }

                var stakedAmount = await GetStakedAmount(userAddress);
                if (stakedAmount < amount)
                {
                    Debug.LogError("[StakingService] 质押金额不足");
                    return false;
                }

                // 调用智能合约的 unstake 函数
                await Task.Delay(2500);

                Debug.Log($"[StakingService] 解除质押成功: {amount} TRIP");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[StakingService] 解除质押失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        public async Task<decimal> ClaimRewards(string userAddress)
        {
            try
            {
                var pendingRewards = await CalculatePendingRewards(userAddress);

                if (pendingRewards <= 0)
                {
                    Debug.LogWarning("[StakingService] 没有可领取的奖励");
                    return 0;
                }

                // 调用智能合约的 claimRewards 函数
                await Task.Delay(2000);

                Debug.Log($"[StakingService] 奖励领取成功: {pendingRewards} TRIP");
                return pendingRewards;
            }
            catch (Exception e)
            {
                Debug.LogError($"[StakingService] 领取奖励失败: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 获取质押金额
        /// </summary>
        public async Task<decimal> GetStakedAmount(string userAddress)
        {
            try
            {
                // 调用智能合约的 getStakedAmount 函数
                await Task.Delay(300);

                // 模拟数据
                return UnityEngine.Random.Range(0, 50000);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StakingService] 获取质押金额失败: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 计算待领取奖励
        /// </summary>
        public async Task<decimal> CalculatePendingRewards(string userAddress)
        {
            try
            {
                var stakedAmount = await GetStakedAmount(userAddress);
                var stakingInfo = await GetStakingInfo(userAddress);

                // 计算奖励 (简化计算)
                decimal blocksPassed = (decimal)(DateTime.Now - stakingInfo.lastClaimTime).TotalSeconds / 12; // 假设12秒一个区块
                decimal rewards = stakedAmount * rewardPerBlock * blocksPassed;

                return rewards;
            }
            catch (Exception e)
            {
                Debug.LogError($"[StakingService] 计算奖励失败: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 获取质押信息
        /// </summary>
        public async Task<StakingInfo> GetStakingInfo(string userAddress)
        {
            await Task.Delay(200);

            return new StakingInfo
            {
                stakedAmount = await GetStakedAmount(userAddress),
                pendingRewards = await CalculatePendingRewards(userAddress),
                totalClaimed = UnityEngine.Random.Range(0, 10000),
                stakingStartTime = DateTime.Now.AddDays(-UnityEngine.Random.Range(1, 365)),
                lastClaimTime = DateTime.Now.AddDays(-UnityEngine.Random.Range(0, 30)),
                apr = baseAPR + (UnityEngine.Random.Range(-0.02f, 0.05f)) // APR 会有波动
            };
        }

        /// <summary>
        /// 获取总质押量
        /// </summary>
        public async Task<decimal> GetTotalStaked()
        {
            await Task.Delay(200);
            return UnityEngine.Random.Range(1000000, 10000000);
        }

        /// <summary>
        /// 获取当前 APR
        /// </summary>
        public async Task<decimal> GetCurrentAPR()
        {
            await Task.Delay(100);

            // 基于总质押量调整 APR
            var totalStaked = await GetTotalStaked();
            decimal aprAdjustment = totalStaked > 5000000 ? -0.05m : 0m;

            return baseAPR + aprAdjustment;
        }

        /// <summary>
        /// 计算复利收益
        /// </summary>
        public decimal CalculateCompoundInterest(decimal principal, decimal apr, int days, int compoundFrequency = 365)
        {
            decimal rate = apr / compoundFrequency;
            decimal periods = (decimal)days / 365 * compoundFrequency;
            return principal * (decimal)Mathf.Pow((float)(1 + rate), (float)periods);
        }

        /// <summary>
        /// 预估收益
        /// </summary>
        public async Task<StakingProjection> ProjectEarnings(decimal amount, int days)
        {
            var apr = await GetCurrentAPR();

            var simpleInterest = amount * apr * ((decimal)days / 365);
            var compoundInterest = CalculateCompoundInterest(amount, apr, days) - amount;

            return new StakingProjection
            {
                stakedAmount = amount,
                durationDays = days,
                estimatedSimpleInterest = simpleInterest,
                estimatedCompoundInterest = compoundInterest,
                apr = apr,
                totalReturn = amount + compoundInterest
            };
        }
    }

    /// <summary>
    /// 质押信息
    /// </summary>
    public class StakingInfo
    {
        public decimal stakedAmount;
        public decimal pendingRewards;
        public decimal totalClaimed;
        public DateTime stakingStartTime;
        public DateTime lastClaimTime;
        public decimal apr;

        public int StakingDays => (int)(DateTime.Now - stakingStartTime).TotalDays;
    }

    /// <summary>
    /// 质押收益预估
    /// </summary>
    public class StakingProjection
    {
        public decimal stakedAmount;
        public int durationDays;
        public decimal estimatedSimpleInterest;
        public decimal estimatedCompoundInterest;
        public decimal apr;
        public decimal totalReturn;
    }
}