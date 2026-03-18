using System.Threading.Tasks;

namespace TripMeta.Web3
{
    /// <summary>
    /// 钱包适配器接口
    /// 统一不同钱包类型的交互方式
    /// </summary>
    public interface IWalletAdapter
    {
        string Address { get; }
        bool IsConnected { get; }
        WalletType WalletType { get; }

        Task<bool> Connect();
        Task Disconnect();
        Task<string> SignMessage(string message);
        Task<string> SendTransaction(string to, decimal amount, string data = null);
        Task<string> CallContract(string contractAddress, string abi, string methodName, object[] parameters);
    }
}
