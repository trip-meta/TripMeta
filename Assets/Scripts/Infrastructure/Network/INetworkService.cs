using System;
using System.Threading.Tasks;

namespace TripMeta.Infrastructure.Network
{
    /// <summary>
    /// 网络服务接口
    /// </summary>
    public interface INetworkService
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task DeleteAsync(string endpoint);
        
        event Action<bool> OnConnectionStatusChanged;
        bool IsConnected { get; }
    }

    public class NetworkException : Exception
    {
        public int StatusCode { get; }
        
        public NetworkException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}