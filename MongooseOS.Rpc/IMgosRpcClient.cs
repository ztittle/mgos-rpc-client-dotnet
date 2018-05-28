using System;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public interface IMgosRpcClient
    {
        string ClientId { get; }
        string RpcTopic { get; }
        bool IsConnected { get; }
        event EventHandler<EventArgs> Disconnected;
        void RegisterHandler(IMgosRpcHandler rpcHandler, string method);
        Task ConnectAsync();
        Task DisconnectAsync();
        Task<object> SendAsync(string deviceId, string method, object args = null);
        Task<object> SendAsync(string deviceId, string method, TimeSpan timeout, object args = null);
        Task<TResponse> SendAsync<TResponse>(string deviceId, string method, object args = null);
        Task<TResponse> SendAsync<TResponse>(string deviceId, string method, TimeSpan timeout, object args = null);
    }

    public interface IMgosRpcHandler
    {
        /// <summary>
        /// Processes an incoming request.
        /// </summary>
        /// <param name="deviceId">The device id of the request.</param>
        /// <param name="args">The args</param>
        /// <returns></returns>
        Task<object> ProcessAsync(string deviceId, object args);
    }
}
