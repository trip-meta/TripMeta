using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Web3
{
    /// <summary>
    /// 代币服务
    /// 处理平台代币的转账、余额查询等功能
    /// </summary>
    public class TokenService
    {
        private Web3Manager web3Manager;
        private string tokenSymbol = "TRIP";
        private int tokenDecimals = 18;

        public TokenService(Web3Manager manager)
        {
            web3Manager = manager;
        }

        /// <summary>
        /// 获取代币余额
        /// </summary>
        public async Task<decimal> GetBalance(string address)
        {
            try
            {
                // 调用智能合约的 balanceOf 函数
                await Task.Delay(300);

                // 模拟余额
                return UnityEngine.Random.Range(100, 10000);
            }
            catch (Exception e)
            {
                Debug.LogError($"[TokenService] 获取余额失败: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 转账代币
        /// </summary>
        public async Task<bool> Transfer(string from, string to, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    Debug.LogError("[TokenService] 转账金额必须大于0");
                    return false;
                }

                // 检查余额
                var balance = await GetBalance(from);
                if (balance < amount)
                {
                    Debug.LogError("[TokenService] 余额不足");
                    return false;
                }

                // 调用智能合约的 transfer 函数
                await Task.Delay(2000);

                Debug.Log($"[TokenService] 转账成功: {amount} {tokenSymbol} 从 {from} 到 {to}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[TokenService] 转账失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 批准代币使用额度
        /// </summary>
        public async Task<bool> Approve(string owner, string spender, decimal amount)
        {
            try
            {
                await Task.Delay(1500);
                Debug.Log($"[TokenService] 批准成功: {spender} 可使用 {amount} {tokenSymbol}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[TokenService] 批准失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 查询授权额度
        /// </summary>
        public async Task<decimal> Allowance(string owner, string spender)
        {
            await Task.Delay(300);
            return UnityEngine.Random.Range(0, 1000);
        }

        /// <summary>
        /// 获取代币信息
        /// </summary>
        public TokenInfo GetTokenInfo()
        {
            return new TokenInfo
            {
                Symbol = tokenSymbol,
                Name = "TripMeta Token",
                Decimals = tokenDecimals,
                TotalSupply = 1000000000,
                ContractAddress = web3Manager.tokenContractAddress
            };
        }

        /// <summary>
        /// 格式化代币金额
        /// </summary>
        public string FormatAmount(decimal amount)
        {
            return $"{amount:N2} {tokenSymbol}";
        }

        /// <summary>
        /// 从 wei 转换为代币单位
        /// </summary>
        public decimal FromWei(string weiAmount)
        {
            if (decimal.TryParse(weiAmount, out var wei))
            {
                return wei / (decimal)Mathf.Pow(10, tokenDecimals);
            }
            return 0;
        }

        /// <summary>
        /// 从代币单位转换为 wei
        /// </summary>
        public string ToWei(decimal amount)
        {
            var wei = amount * (decimal)Mathf.Pow(10, tokenDecimals);
            return wei.ToString("0");
        }
    }

    /// <summary>
    /// 代币信息
    /// </summary>
    public class TokenInfo
    {
        public string Symbol;
        public string Name;
        public int Decimals;
        public long TotalSupply;
        public string ContractAddress;
    }
}
