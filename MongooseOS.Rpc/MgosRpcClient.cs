using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MongooseOS.Rpc
{
    public class MgosRpcClient : IMgosRpcClient, IDisposable
    {
        private int _messageId = 0;
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMilliseconds(3000);

        private readonly JsonSerializerOptions _serializerSettings;

        private readonly IMqttClient _mqttClient;
        private MqttClientOptions _mqttClientOpts;

        private readonly IDictionary<string, IMgosRpcHandler> _handlers;

        private readonly Dictionary<int, TaskCompletionSource<object>> _pendingRequests;

        public string ClientId => _mqttClientOpts?.ClientId;

        public bool IsConnected => _mqttClient.IsConnected;

        public string RpcTopic => $"{ClientId}/rpc";

        public MgosRpcClient(IMqttClient mqttClient)
        {
            _serializerSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            _serializerSettings.Converters.Add(new JsonConverters.BooleanConverter());

            _mqttClient = mqttClient;
            _mqttClient.UseApplicationMessageReceivedHandler(MsgReceived);

            _pendingRequests = new Dictionary<int, TaskCompletionSource<object>>();
            _handlers = new Dictionary<string, IMgosRpcHandler>();
        }

        public async Task ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default)
        {
            _mqttClientOpts = options;
            await _mqttClient.ConnectAsync(options, cancellationToken);

            await _mqttClient.SubscribeAsync(RpcTopic, cancellationToken);
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            return _mqttClient.DisconnectAsync(cancellationToken);
        }

        private async Task MsgReceived(MqttApplicationMessageReceivedEventArgs msg, CancellationToken cancellationToken = default)
        {
            if (msg.Topic != RpcTopic)
            {
                return;
            }

            var res = Encoding.UTF8.GetString(msg.Payload);

            var jObject = JsonDocument.Parse(res);

            if (jObject.RootElement.TryGetProperty("method", out _))
            {
                var request = JsonSerializer.Deserialize<MgosRpcRequest>(res, _serializerSettings);

                await ProcessMgosRequest(msg.ClientId, request, cancellationToken);
            }

            var response = JsonSerializer.Deserialize<MgosRpcResponse>(res, _serializerSettings);

            ProcessMgosResponse(response);
        }

        private void ProcessMgosResponse(MgosRpcResponse response)
        {
            if (_pendingRequests.TryGetValue(response.Id.GetValueOrDefault(), out var tcs) == false)
            {
                throw new InvalidOperationException($"Unable to find a pending request for id '{response.Id}'");
            }
            _pendingRequests.Remove(response.Id.GetValueOrDefault());

            if (response.Error != null)
            {
                tcs.TrySetException(new MgosRpcException(response.Error));
            }
            else
            {
                tcs.TrySetResult(response.Result);
            }
        }

        private async Task ProcessMgosRequest(string deviceId, MgosRpcRequest request, CancellationToken cancellationToken)
        {
            if (_handlers.TryGetValue(request.Method, out var handler))
            {
                var response = new MgosRpcResponse
                {
                    Id = request.Id,
                    Src = deviceId,
                    Dst = request.Src,
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

                var payload = JsonSerializer.Serialize(response, _serializerSettings);

                await _mqttClient.PublishAsync($"{request.Src}/rpc", Encoding.UTF8.GetBytes(payload), cancellationToken);
            }
        }

        public async Task<TResponse> SendAsync<TResponse>(string deviceId, string method, object args = null, CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default) {
                using var cts = new CancellationTokenSource(_defaultTimeout);

                return await SendAsync<TResponse>(deviceId, method, args, cts.Token);
            }

            if (_mqttClient.IsConnected == false)
            {
                throw new InvalidOperationException("MqttClient is not connected");
            }

            var tcs = new TaskCompletionSource<object>();

            cancellationToken.Register(() => tcs.TrySetCanceled(), false);

            var id = Interlocked.Increment(ref _messageId);
            _pendingRequests.Add(id, tcs);
            
            var request = new MgosRpcRequest(method)
            {
                Id = id,
                Src = _mqttClientOpts.ClientId,
                Args = args
            };

            var payload = JsonSerializer.Serialize(request, _serializerSettings);

            await _mqttClient.PublishAsync($"{deviceId}/rpc", Encoding.UTF8.GetBytes(payload));

            var result = await tcs.Task;

            if (!(result is JsonElement jElement))
            {
                return default;
            }

            return JsonSerializer.Deserialize<TResponse>(jElement.GetRawText(), _serializerSettings);
        }

        public async Task<object> SendAsync(string deviceId, string method, object args = null, CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default) {
                using var cts = new CancellationTokenSource(_defaultTimeout);

                return await SendAsync(deviceId, method, args, cts.Token);
            }

            var jsonResponse = await SendAsync<string>(deviceId, method, args, cancellationToken);

            if (jsonResponse == null)
            {
                return null;
            }

            var response = JsonSerializer.Deserialize<MgosRpcResponse>(jsonResponse, _serializerSettings);

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
