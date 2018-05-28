using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public class SysInfoWifiResponse
    {
        public string Sta_Ip { get; set; }
        public string Ap_Ip { get; set; }
        public string Status { get; set; }
        public string Ssid { get; set; }
    }

    public class SysInfoResponse
    {
        public string App { get; set; }
        public string Fw_Version { get; set; }
        public string Fw_Id { get; set; }
        public string Mac { get; set; }
        public string Arch { get; set; }
        public int Uptime { get; set; }
        public int Ram_Size { get; set; }
        public int Ram_Free { get; set; }
        public int Ram_Min_Free { get; set; }
        public int Fs_Size { get; set; }
        public int Fs_Free { get; set; }
        public SysInfoWifiResponse Wifi { get; set; }
    }

    public interface ISysRpc
    {
        Task<SysInfoResponse> GetInfo(string deviceId);
    }

    public class SysRpc : ISysRpc
    {
        private readonly IMgosRpcClient _rpcClient;

        public SysRpc(IMgosRpcClient client)
        {
            _rpcClient = client;
        }

        public Task<SysInfoResponse> GetInfo(string deviceId)
        {
            return _rpcClient.SendAsync<SysInfoResponse>(deviceId, "Sys.GetInfo");
        }
    }
}
