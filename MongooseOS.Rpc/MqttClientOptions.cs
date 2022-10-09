namespace MongooseOS.Rpc
{
    public class MqttClientOptions
    {
        public string ClientId { get; set; }
        public string Host { get;set; }
        public ushort Port { get;set; }
        public byte[] ClientPfx { get;set; }
        public byte[] CaCert { get;set; }
    }
}