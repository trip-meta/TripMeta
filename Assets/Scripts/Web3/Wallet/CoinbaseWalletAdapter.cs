using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Web3
{
    /// <summary>
    /// Coinbase Wallet 适配器
    /// </summary>
    public class CoinbaseWalletAdapter : IWalletAdapter
    {
        public string Address { get; private set; }
        public bool IsConnected => !string.IsNullOrEmpty(Address);
        public WalletType WalletType => WalletType.CoinbaseWallet;

        public async Task<bool> Connect()
        {
            await Task.Delay(600);
            Address = "0x" + System.Guid.NewGuid().ToString("N").Substring(0, 40);
            Debug.Log($"[CoinbaseWalletAdapter] 已连接: {Address}");
            return true;
        }

        public async Task Disconnect()
        {
            await Task.Delay(100);
            Address = null;
            Debug.Log("[CoinbaseWalletAdapter] 已断开连接");
        }

        public async Task<string> SignMessage(string message)
        {
            if (!IsConnected) return null;
            await Task.Delay(600);
            return "0x" + System.Guid.NewGuid().ToString("N");
        }

        public async Task<string> SendTransaction(string to, decimal amount, string data = null)
        {
            if (!IsConnected) return null;
            await Task.Delay(2200);
            return "0x" + System.Guid.NewGuid().ToString("N");
        }

        public async Task<string> CallContract(string contractAddress, string abi, string methodName, object[] parameters)
        {
            if (!IsConnected) return null;
            await Task.Delay(1100);
            return "0x" + System.Guid.NewGuid().ToString("N");
        }
    }
}
