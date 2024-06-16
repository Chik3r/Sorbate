using System.Net;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Sorbate.DiscordBot.Data;
using Sorbate.DiscordBot.Data.Events;
using Websocket.Client;

namespace Sorbate.DiscordBot;

public class DiscordClient {
    private PeriodicTimer? _heartbeatTimer;
    private readonly HttpClient _httpClient;
    private readonly WebsocketClient _websocketClient = new(new Uri(Constants.DiscordGatewayUri));
    private int? _lastSequenceNumber = null;
    private readonly CancellationTokenSource _cts = new();

    private readonly JsonSerializerOptions _serializerOptions = new();

    public DiscordClient(HttpClient client) {
        _httpClient = client;
    }

    public void Start() {
        _serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        _websocketClient.MessageReceived
            .Where(x => x.Text != null && x.Text.StartsWith('{'))
            .Subscribe(OnMessage);
        _websocketClient.Start();

        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Secrets.AuthToken);
    }

    public void Stop() {
        _cts.Cancel();
        _websocketClient.Dispose();
    }

    public async IAsyncEnumerable<Attachment> SearchForFiles(int initialOffset = 0) {
        int offset = initialOffset;
        int totalNumMessages = int.MaxValue;

        while (offset < totalNumMessages) {
            string url = Constants.SearchUri + $"&offset={offset}"; 
            HttpResponseMessage result = await _httpClient.GetAsync(url, _cts.Token);
            
            if (result.StatusCode == HttpStatusCode.TooManyRequests && result.Headers.RetryAfter?.Delta.HasValue == true) {
                // wait and try again
                await Task.Delay(result.Headers.RetryAfter.Delta.Value, _cts.Token);
                result = await _httpClient.GetAsync(url, _cts.Token);
            }

            if (!result.IsSuccessStatusCode)
                yield break;

            SearchResults? results = null;
            try {
                results = JsonSerializer.Deserialize<SearchResults>(await result.Content.ReadAsStreamAsync());
            }
            catch (Exception e) {
                Console.WriteLine("Error deserializing search results\n" + e);
                yield break;
            }
            
            if (results is null)
                yield break;
                
            totalNumMessages = results!.TotalResults;
            
            foreach (Message message in results.Messages.SelectMany(x => x)) {
                foreach (Attachment attachment in message.Attachments) {
                    yield return attachment;
                }
            }

            offset += results.Messages.Length;
        }
    }

    private void OnMessage(ResponseMessage message) {
        try {
            GatewayObject<JsonObject>? unknownGatewayObject =
                JsonSerializer.Deserialize<GatewayObject<JsonObject>>(message.Text!);

            if (unknownGatewayObject is null)
                return;

            if (unknownGatewayObject.SequenceNumber.HasValue)
                _lastSequenceNumber = unknownGatewayObject.SequenceNumber;

            Console.WriteLine("Got message with opcode " + unknownGatewayObject.OpCode);
            switch (unknownGatewayObject.OpCode) {
                case GatewayOpCode.Dispatch:
                    HandleDispatchedEvent(unknownGatewayObject);
                    break;
                case GatewayOpCode.Heartbeat:
                    SendHeartbeat();
                    break;
                case GatewayOpCode.Reconnect:
                case GatewayOpCode.InvalidSession:
                    throw new NotImplementedException();
                case GatewayOpCode.Hello:
                    HandleHello(unknownGatewayObject);
                    break;
                case GatewayOpCode.HeartbeatAck:
                    break;
                case GatewayOpCode.Identify:
                case GatewayOpCode.PresenceUpdate:
                case GatewayOpCode.VoiceStateUpdate:
                case GatewayOpCode.Resume:
                case GatewayOpCode.RequestGuildMembers:
                // These should never be sent to us by discord according to the API
                // If we do receive them, something is wrong?
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }
        catch (Exception e) {
            Console.WriteLine("An error occurred while processing a gateway message.\n" + e);
        }
    }

    private void HandleDispatchedEvent(GatewayObject<JsonObject> gatewayObject) {
        switch (gatewayObject.EventName) {
            case "MESSAGE_CREATE":
                // TODO: Handle, check attachments for .tmod files
                break;
            default:
                // Unknown event
                break;
        }
    }

    private void HandleHello(GatewayObject<JsonObject> gatewayObject) {
        EventHello? data = gatewayObject.Data!.Deserialize<EventHello>();
        if (data is null) throw new ArgumentException("Data was null", nameof(gatewayObject));

        if (_heartbeatTimer is null) {
            SendHeartbeat();
            
            _heartbeatTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(data.HeartbeatInterval));
            Task.Factory.StartNew(async () => {
                while (!_cts.Token.IsCancellationRequested) {
                    await _heartbeatTimer.WaitForNextTickAsync(_cts.Token);
                    SendHeartbeat();
                }
            }, TaskCreationOptions.LongRunning);
        }

        SendIdentify();
    }

    private void SendHeartbeat() {
        string seqNumber = _lastSequenceNumber.HasValue ? _lastSequenceNumber.Value.ToString() : "null";
        string msg = $"{{\"op\":1,\"d\":{seqNumber}}}";
        _websocketClient.Send(msg);

        Console.WriteLine("Sent heartbeat");
    }

    private void SendIdentify() {
        EventIdentify identify = new(Secrets.AuthToken, ConnectionProperties.Default,
            GatewayIntent.GuildMessages | GatewayIntent.MessageContent);

        GatewayObject<EventIdentify> gatewayObj = new(GatewayOpCode.Identify, identify);
        string serialized = JsonSerializer.Serialize(gatewayObj, _serializerOptions);

        Console.WriteLine("Sending identify message:\n" + serialized);
        _websocketClient.Send(serialized);
    }
}