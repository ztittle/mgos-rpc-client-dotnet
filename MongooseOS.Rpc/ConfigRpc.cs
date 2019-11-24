using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public interface IConfigRpc
    {
        Task ConfigSetAsync<TConfig>(string deviceId, string key, TConfig value);
        Task ConfigSetAsync<TConfig>(string deviceId, string key, TConfig value, TimeSpan timeout);
        Task<TConfig> ConfigGetAsync<TConfig>(string deviceId, string key);
        Task<TConfig> ConfigGetAsync<TConfig>(string deviceId, string key, TimeSpan timeout);
        Task ConfigSaveAsync(string deviceId);
        Task ConfigSaveAsync(string deviceId, TimeSpan timeout);
    }

    public class ConfigRpc : IConfigRpc
    {
        private IMgosRpcClient _rpcClient;

        public ConfigRpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        public Task<TConfig> ConfigGetAsync<TConfig>(string deviceId, string key)
        {
            var args = new
            {
                key
            };

            return _rpcClient.SendAsync<TConfig>(deviceId, "Config.Get", args);
        }

        public Task<TConfig> ConfigGetAsync<TConfig>(string deviceId, string key, TimeSpan timeout)
        {
            var args = new
            {
                key
            };

            return _rpcClient.SendAsync<TConfig>(deviceId, "Config.Get", timeout, args);
        }

        public Task ConfigSaveAsync(string deviceId)
        {
            return _rpcClient.SendAsync(deviceId, "Config.Save");
        }

        public Task ConfigSaveAsync(string deviceId, TimeSpan timeout)
        {
            return _rpcClient.SendAsync(deviceId, "Config.Save", timeout);
        }

        public Task ConfigSetAsync<TConfig>(string deviceId, string key, TConfig value)
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

        public Task ConfigSetAsync<TConfig>(string deviceId, string key, TConfig value, TimeSpan timeout)
        {
            var args = new
            {
                config = new Dictionary<string, object>(1)
                {
                    { key, value }
                }
            };

            return _rpcClient.SendAsync(deviceId, "Config.Set", timeout, args);
        }
    }
}
