using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public interface IMgosRpcClient
    {
        string ClientId { get; }
        string RpcTopic { get; }
        bool IsConnected { get; }
        void RegisterHandler(IMgosRpcHandler rpcHandler, string method);
        Task ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task<object> SendAsync(string deviceId, string method, object args = null, CancellationToken cancellationToken = default);
        Task<TResponse> SendAsync<TResponse>(string deviceId, string method, object args = null, CancellationToken cancellationToken = default);
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
