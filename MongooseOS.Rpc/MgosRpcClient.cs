using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public class MgosRpcClient : IMgosRpcClient, IDisposable
    {
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMilliseconds(3000);

        private readonly JsonSerializerSettings _serializerSettings;

        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttClientOpts;

        private readonly IDictionary<string, IMgosRpcHandler> _handlers;

        private readonly Dictionary<string, TaskCompletionSource<object>> _pendingRequests;

        public event EventHandler<EventArgs> Disconnected;

        public string ClientId => _mqttClientOpts.ClientId;

        public bool IsConnected => _mqttClient.IsConnected;

        public string RpcTopic => $"{_mqttClientOpts.ClientId}/rpc";

        public MgosRpcClient(IMqttClientFactory factory, string mqttEndpoint, string clientId)
            : this(factory, CreateClientOptions(mqttEndpoint, clientId))
        {
        }

        private static IMqttClientOptions CreateClientOptions(string mqttEndpoint, string clientId)
        {
            ParseMqttEndpoint(mqttEndpoint, out var mqttEndpointHost, out var mqttEndpointPort, 1883);

            return new MqttClientOptionsBuilder()
                .WithTcpServer(mqttEndpointHost, port: mqttEndpointPort)
                .WithClientId(clientId)
                .Build();
        }

        private static void ParseMqttEndpoint(string mqttEndpoint, out string mqttEndpointHost, out int mqttEndpointPort, int defaultPort)
        {
            mqttEndpointPort = defaultPort;
            var endpointParts = mqttEndpoint.Split(':');
            mqttEndpointHost = endpointParts[0];
            if (endpointParts.Length > 1)
            {
                int.TryParse(endpointParts[1], out mqttEndpointPort);
            }
        }

        public MgosRpcClient(IMqttClientFactory factory, string mqttEndpoint, string clientId, byte[] clientPfx, byte[] caCert)
            : this(factory, CreateSecureClientOptions(mqttEndpoint, clientId, clientPfx, caCert))
        {
        }

        private static IMqttClientOptions CreateSecureClientOptions(string mqttEndpoint, string clientId, byte[] clientPfx, byte[] caCert)
        {
            ParseMqttEndpoint(mqttEndpoint, out var mqttEndpointHost, out var mqttEndpointPort, 8883);

            return new MqttClientOptionsBuilder()
                .WithTcpServer(mqttEndpointHost, port: mqttEndpointPort)
                .WithClientId(clientId)
                .WithTls(certificates: new byte[][] {
                        clientPfx,
                        caCert
                })
                .Build();
        }

        public MgosRpcClient(IMqttClientFactory factory, IMqttClientOptions mqttClientOptions)
        {
            _mqttClientOpts = mqttClientOptions;
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            _mqttClient = factory.CreateMqttClient();
            _mqttClient.ApplicationMessageReceived += MsgReceived;
            _mqttClient.Disconnected += (sender, e) => Disconnected?.Invoke(sender, e);

            _pendingRequests = new Dictionary<string, TaskCompletionSource<object>>();
            _handlers = new Dictionary<string, IMgosRpcHandler>();
        }

        public async Task ConnectAsync()
        {
            await _mqttClient.ConnectAsync(_mqttClientOpts);

            await _mqttClient.SubscribeAsync(RpcTopic, MqttQualityOfServiceLevel.AtLeastOnce);
        }

        public Task DisconnectAsync()
        {
            return _mqttClient.DisconnectAsync();
        }

        private void MsgReceived(object sender, MqttApplicationMessageReceivedEventArgs evt)
        {
            var res = Encoding.UTF8.GetString(evt.ApplicationMessage.Payload);

            var jObject = JObject.Parse(res);

            if (jObject["method"] != null)
            {
                var request = jObject.ToObject<MgosRpcRequest>();

                ProcessMgosRequest(evt.ClientId, request);

                return;
            }

            var response = jObject.ToObject<MgosRpcResponse>();

            ProcessMgosResponse(response);
        }

        private void ProcessMgosResponse(MgosRpcResponse response)
        {
            if (_pendingRequests.TryGetValue(response.Tag, out var tcs) == false)
            {
                throw new InvalidOperationException($"Unable to find a pending request for tag '{response.Tag}'");
            }
            _pendingRequests.Remove(response.Tag);

            if (response.Error != null)
            {
                tcs.TrySetException(new MgosRpcException(response.Error));
            }
            else
            {
                tcs.TrySetResult(response.Result);
            }
        }

        private void ProcessMgosRequest(string deviceId, MgosRpcRequest request)
        {
            if (_handlers.TryGetValue(request.Method, out var handler))
            {
                Task.Run(async () =>
                {
                    var response = new MgosRpcResponse
                    {
                        Src = deviceId,
                        Dst = request.Src,
                        Tag = request.Tag
                    };

                    try
                    {
                        var responseArgs = await handler.ProcessAsync(deviceId, request.Args);

                        response.Result = responseArgs;
                    }
                    catch (MgosRpcException e)
                    {
                        response.Error = e.Error;
                    }

                    var payload = JsonConvert.SerializeObject(response, _serializerSettings);

                    await _mqttClient.PublishAsync(new MqttApplicationMessage
                    {
                        Topic = $"{request.Src}/rpc",
                        Payload = Encoding.UTF8.GetBytes(payload),
                        QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                        Retain = false
                    });
                });
            }
        }

        public Task<TResponse> SendAsync<TResponse>(string deviceId, string method, object args = null)
            => SendAsync<TResponse>(deviceId, method, _defaultTimeout, args);

        public async Task<TResponse> SendAsync<TResponse>(string deviceId, string method, TimeSpan timeout, object args = null)
        {
            if (_mqttClient.IsConnected == false)
            {
                throw new InvalidOperationException("MqttClient is not connected");
            }

            using (var ct = new CancellationTokenSource())
            {
                var tcs = new TaskCompletionSource<object>();

                ct.Token.Register(() => tcs.TrySetCanceled(), false);

                var tag = Guid.NewGuid().ToString();
                _pendingRequests.Add(tag, tcs);

                var request = new MgosRpcRequest(method)
                {
                    Tag = tag,
                    Src = _mqttClientOpts.ClientId,
                    Args = args
                };

                var payload = JsonConvert.SerializeObject(request, _serializerSettings);

                await _mqttClient.PublishAsync(new MqttApplicationMessage
                {
                    Topic = $"{deviceId}/rpc",
                    Payload = Encoding.UTF8.GetBytes(payload),
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                    Retain = false
                });

                ct.CancelAfter(timeout);

                var result = await tcs.Task;
                var token = result as JToken;

                if (token == null)
                {
                    return default(TResponse);
                }

                return token.ToObject<TResponse>();
            }
        }

        public Task<object> SendAsync(string deviceId, string method, object args = null)
            => SendAsync(deviceId, method, _defaultTimeout, args);

        public async Task<object> SendAsync(string deviceId, string method, TimeSpan timeout, object args = null)
        {
            var jsonResponse = await SendAsync<string>(deviceId, method, timeout, args);

            if (jsonResponse == null)
            {
                return null;
            }

            var response = JsonConvert.DeserializeObject<MgosRpcResponse>(jsonResponse);

            return response.Result;
         }

        public void Dispose()
        {
            _mqttClient.Dispose();
        }

        public void RegisterHandler(IMgosRpcHandler rpcHandler, string method)
        {
            _handlers.Add(method, rpcHandler);
        }
    }
}
