using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public class PingResponse
    {
        public string channel_info { get; set; }
    }

    public interface IRpc
    {
        Task<PingResponse> Ping(string deviceId);
    }

    public class Rpc : IRpc
    {
        private readonly IMgosRpcClient _rpcClient;

        public Rpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        public Task<PingResponse> Ping(string deviceId)
        {
            return _rpcClient.SendAsync<PingResponse>(deviceId, "RPC.Ping");
        }
    }
}
