using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Web3
{
    /// <summary>
    /// WalletConnect 适配器
    /// 支持移动钱包连接
    /// </summary>
    public class WalletConnectAdapter : IWalletAdapter
    {
        public string Address { get; private set; }
        public bool IsConnected => !string.IsNullOrEmpty(Address);
        public WalletType WalletType => WalletType.WalletConnect;

        public async Task<bool> Connect()
        {
            // 这里应该初始化 WalletConnect 会话
            await Task.Delay(800);
            Address = "0x" + System.Guid.NewGuid().ToString("N").Substring(0, 40);
            Debug.Log($"[WalletConnectAdapter] 已连接: {Address}");
            return true;
        }

        public async Task Disconnect()
        {
            await Task.Delay(100);
            Address = null;
            Debug.Log("[WalletConnectAdapter] 已断开连接");
        }

        public async Task<string> SignMessage(string message)
        {
            if (!IsConnected) return null;
            await Task.Delay(800);
            return "0x" + System.Guid.NewGuid().ToString("N");
        }

        public async Task<string> SendTransaction(string to, decimal amount, string data = null)
        {
            if (!IsConnected) return null;
            await Task.Delay(2500);
            return "0x" + System.Guid.NewGuid().ToString("N");
        }

        public async Task<string> CallContract(string contractAddress, string abi, string methodName, object[] parameters)
        {
            if (!IsConnected) return null;
            await Task.Delay(1200);
            return "0x" + System.Guid.NewGuid().ToString("N");
        }
    }
}
