namespace MongooseOS.Rpc
{
    public class MgosRpcRequest
    {
        public MgosRpcRequest(string method) => Method = method;

        /// <summary>
        /// Required. Function name to call. Example: Math.Add
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Optional. Function arguments. Example: { "a": 1, "b": 2 }
        /// </summary>
        public object Args { get; set; }

        /// <summary>
        /// Optional. Used with MQTT: response will be sent
        /// to that topic followed by "/rpc", so in this
        /// case it'll be "joe/32efc823aa/rpc".
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        ///  Frame ID. Required if a response is expected.
        /// </summary>
        public int? Id { get; set; }
    }
}
