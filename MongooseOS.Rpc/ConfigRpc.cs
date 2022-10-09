using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public interface IConfigRpc
    {
        Task ConfigSetAsync<TConfig>(string deviceId, string key, TConfig value, CancellationToken cancellationToken = default);
        Task<TConfig> ConfigGetAsync<TConfig>(string deviceId, string key, CancellationToken cancellationToken = default);
        Task ConfigSaveAsync(string deviceId, CancellationToken cancellationToken = default);
    }

    public class ConfigRpc : IConfigRpc
    {
        private IMgosRpcClient _rpcClient;

        public ConfigRpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        public Task<TConfig> ConfigGetAsync<TConfig>(string deviceId, string key, CancellationToken cancellationToken = default)
        {
            var args = new
            {
                key
            };

            return _rpcClient.SendAsync<TConfig>(deviceId, "Config.Get", args, cancellationToken);
        }

        public Task ConfigSaveAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            return _rpcClient.SendAsync(deviceId, "Config.Save", cancellationToken);
        }

        public Task ConfigSetAsync<TConfig>(string deviceId, string key, TConfig value, CancellationToken cancellationToken = default)
        {
            var args = new
            {
                config = new Dictionary<string, object>(1)
                {
                    { key, value }
                }
            };

            return _rpcClient.SendAsync(deviceId, "Config.Set", args, cancellationToken);
        }
    }
}
