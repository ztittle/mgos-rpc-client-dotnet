using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public interface IConfigRpc
    {
        Task ConfigSet<TConfig>(string deviceId, string key, TConfig value);
        Task<TConfig> ConfigGet<TConfig>(string deviceId, string key);
        Task ConfigSave(string deviceId);
    }

    public class ConfigRpc : IConfigRpc
    {
        private IMgosRpcClient _rpcClient;

        public ConfigRpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        public Task<TConfig> ConfigGet<TConfig>(string deviceId, string key)
        {
            var args = new
            {
                key
            };

            return _rpcClient.SendAsync<TConfig>(deviceId, "Config.Get", args);
        }

        public Task ConfigSave(string deviceId)
        {
            return _rpcClient.SendAsync(deviceId, "Config.Save");
        }

        public Task ConfigSet<TConfig>(string deviceId, string key, TConfig value)
        {
            var args = new
            {
                config = new Dictionary<string, object>(1)
                {
                    { key, value }
                }
            };

            return _rpcClient.SendAsync(deviceId, "Config.Set", args);
        }
    }
}
