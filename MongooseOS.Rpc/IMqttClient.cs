using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public interface IMqttClient : IDisposable
    {
        bool IsConnected { get; }

        Task PublishAsync(string topic, byte[] payload, CancellationToken cancellationToken = default);
        Task SubscribeAsync(string topic, CancellationToken cancellationToken = default);
        Task ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        void UseApplicationMessageReceivedHandler(Func<MqttApplicationMessageReceivedEventArgs, CancellationToken, Task> msgReceived);
    }

    public class MqttApplicationMessageReceivedEventArgs
    {
        public string ClientId { get; set; }
        public string Topic { get; set; }
        public byte[] Payload { get; set; }
    }
}