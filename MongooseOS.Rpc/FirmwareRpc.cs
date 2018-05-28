using System;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public interface IFirmwareRpc
    {
        Task UpdateFirmwareAsync(string deviceId, string firmwareUrl);
    }

    public class FirmwareRpc : IFirmwareRpc
    {
        private readonly IMgosRpcClient _rpcClient;
        private static TimeSpan _timeout = TimeSpan.FromSeconds(30);

        public FirmwareRpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        public Task UpdateFirmwareAsync(string deviceId, string firmwareUrl)
        {
            var args = new
            {
                Url = firmwareUrl
            };

            return _rpcClient.SendAsync(deviceId, "OTA.Update", _timeout, args);
        }
    }
}
