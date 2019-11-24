using System;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public class PingResponse
    {
        public string channel_info { get; set; }
    }

    public interface IRpc
    {
        Task<PingResponse> PingAsync(string deviceId);
        Task<PingResponse> PingAsync(string deviceId, TimeSpan timeout);
    }

    public class Rpc : IRpc
    {
        private readonly IMgosRpcClient _rpcClient;

        public Rpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        public Task<PingResponse> PingAsync(string deviceId)
        {
            return _rpcClient.SendAsync<PingResponse>(deviceId, "RPC.Ping");
        }

        public Task<PingResponse> PingAsync(string deviceId, TimeSpan timeout)
        {
            return _rpcClient.SendAsync<PingResponse>(deviceId, "RPC.Ping", timeout);
        }
    }
}
