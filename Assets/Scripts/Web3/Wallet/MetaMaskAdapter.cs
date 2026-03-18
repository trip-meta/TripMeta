using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Web3
{
    /// <summary>
    /// MetaMask 钱包适配器
    /// </summary>
    public class MetaMaskAdapter : IWalletAdapter
    {
        public string Address { get; private set; }
        public bool IsConnected => !string.IsNullOrEmpty(Address);
        public WalletType WalletType => WalletType.MetaMask;

        public async Task<bool> Connect()
        {
            // 这里应该调用 MetaMask SDK 或浏览器插件
            // 简化实现：模拟连接
            await Task.Delay(500);
            Address = "0x" + System.Guid.NewGuid().ToString("N").Substring(0, 40);
            Debug.Log($"[MetaMaskAdapter] 已连接: {Address}");
            return true;
        }

        public async Task Disconnect()
        {
            await Task.Delay(100);
            Address = null;
            Debug.Log("[MetaMaskAdapter] 已断开连接");
        }

        public async Task<string> SignMessage(string message)
        {
            if (!IsConnected) return null;
            await Task.Delay(500);
            // 返回模拟的签名
            return "0x" + System.Guid.NewGuid().ToString("N");
        }

        public async Task<string> SendTransaction(string to, decimal amount, string data = null)
        {
            if (!IsConnected) return null;
            await Task.Delay(2000);
            // 返回模拟的交易哈希
            return "0x" + System.Guid.NewGuid().ToString("N");
        }

        public async Task<string> CallContract(string contractAddress, string abi, string methodName, object[] parameters)
        {
            if (!IsConnected) return null;
            await Task.Delay(1000);
            // 返回模拟的调用结果
            return "0x" + System.Guid.NewGuid().ToString("N");
        }
    }
}
