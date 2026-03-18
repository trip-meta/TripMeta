using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Web3
{
    /// <summary>
    /// Phantom 钱包适配器 (Solana)
    /// </summary>
    public class PhantomAdapter : IWalletAdapter
    {
        public string Address { get; private set; }
        public bool IsConnected => !string.IsNullOrEmpty(Address);
        public WalletType WalletType => WalletType.Phantom;

        public async Task<bool> Connect()
        {
            await Task.Delay(500);
            // Solana 地址格式不同
            Address = System.Guid.NewGuid().ToString("N").Substring(0, 32) + "ABC";
            Debug.Log($"[PhantomAdapter] 已连接: {Address}");
            return true;
        }

        public async Task Disconnect()
        {
            await Task.Delay(100);
            Address = null;
            Debug.Log("[PhantomAdapter] 已断开连接");
        }

        public async Task<string> SignMessage(string message)
        {
            if (!IsConnected) return null;
            await Task.Delay(400);
            return System.Guid.NewGuid().ToString("N");
        }

        public async Task<string> SendTransaction(string to, decimal amount, string data = null)
        {
            if (!IsConnected) return null;
            await Task.Delay(1500);
            return System.Guid.NewGuid().ToString("N");
        }

        public async Task<string> CallContract(string contractAddress, string abi, string methodName, object[] parameters)
        {
            if (!IsConnected) return null;
            await Task.Delay(800);
            return System.Guid.NewGuid().ToString("N");
        }
    }
}
