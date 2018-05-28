namespace MongooseOS.Rpc
{
    public class MgosRpcResponse
    {
        public string Src { get; set; }
        public string Dst { get; set; }
        public string Tag { get; set; }
        public MgosRpcError Error { get; set; }
        public object Result { get; set; }
    }
}
