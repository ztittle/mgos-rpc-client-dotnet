using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public class UpdateFirmwareResponse
    {
        public bool Success { get; set; }
    }

    public interface IFirmwareRpc
    {
        Task<UpdateFirmwareResponse> UpdateFirmwareAsync(string deviceId, string firmwareUrl, CancellationToken cancellationToken = default);
    }

    public class FirmwareRpc : IFirmwareRpc
    {
        private readonly IMgosRpcClient _rpcClient;
        private static TimeSpan _timeout = TimeSpan.FromSeconds(30);

        public FirmwareRpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        /// <summary>
        /// Updates the device firmware.
        /// </summary>
        /// <param name="deviceId">The device's ID.</param>
        /// <param name="firmwareUrl">The URL of the firmware used to update the device.</param>
        /// <param name="cancellationToken">The token used to cancel the request.</param>
        /// <returns>A response indicating success or failure.</returns>
        public async Task<UpdateFirmwareResponse> UpdateFirmwareAsync(string deviceId, string firmwareUrl, CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default) {
                using var cts = new CancellationTokenSource(_timeout);
                return await UpdateFirmwareAsync(deviceId, firmwareUrl, cancellationToken);
            }

            var args = new
            {
                Url = firmwareUrl
            };

            var success = await _rpcClient.SendAsync<bool>(deviceId, "OTA.Update", args, cancellationToken);

            return new UpdateFirmwareResponse
            {
                Success = success
            };
        }
    }
}
