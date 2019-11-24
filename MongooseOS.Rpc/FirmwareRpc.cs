using System;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public class UpdateFirmwareResponse
    {
        public bool Success { get; set; }
    }

    public interface IFirmwareRpc
    {
        Task<UpdateFirmwareResponse> UpdateFirmwareAsync(string deviceId, string firmwareUrl);
        Task<UpdateFirmwareResponse> UpdateFirmwareAsync(string deviceId, string firmwareUrl, TimeSpan timeout);
    }

    public class FirmwareRpc : IFirmwareRpc
    {
        private readonly IMgosRpcClient _rpcClient;
        private static TimeSpan _timeout = TimeSpan.FromSeconds(30);

        public FirmwareRpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        public async Task<UpdateFirmwareResponse> UpdateFirmwareAsync(string deviceId, string firmwareUrl)
        {
            var args = new
            {
                Url = firmwareUrl
            };

            var success = await _rpcClient.SendAsync<bool>(deviceId, "OTA.Update", _timeout, args);

            return new UpdateFirmwareResponse
            {
                Success = success
            };
        }

        public async Task<UpdateFirmwareResponse> UpdateFirmwareAsync(string deviceId, string firmwareUrl, TimeSpan timeout)
        {
            var args = new
            {
                Url = firmwareUrl
            };

            var success = await _rpcClient.SendAsync<bool>(deviceId, "OTA.Update", timeout, args);

            return new UpdateFirmwareResponse
            {
                Success = success
            };
        }
    }
}
