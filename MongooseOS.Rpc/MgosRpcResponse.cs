namespace MongooseOS.Rpc
{
    public class MgosRpcResponse
    {
        public int? Id { get; set; }
        public string Src { get; set; }
        public string Dst { get; set; }
        public MgosRpcError Error { get; set; }
        public object Result { get; set; }
    }
}
