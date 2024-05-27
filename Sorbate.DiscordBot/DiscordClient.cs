using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sorbate.DiscordBot.Data;
using Sorbate.DiscordBot.Data.Events;
using Websocket.Client;

namespace Sorbate.DiscordBot;

public class DiscordClient {
    private PeriodicTimer? HeartbeatTimer;
    private WebsocketClient _websocketClient = new(new Uri(Constants.DiscordGatewayUri));
    private int? _lastSequenceNumber = null;
    private HttpClient _httpClient = new();
    private CancellationTokenSource _cts = new();
    
    public void Start() {
        _websocketClient.MessageReceived
            .Where(x => x.Text != null && x.Text.StartsWith('{'))
            .Subscribe(OnMessage);
        _websocketClient.Start();
    }

    public void Cancel() {
        _cts.Cancel();
    }

    private void OnMessage(ResponseMessage message) {
        try {
            GatewayObject<JsonObject>? unknownGatewayObject =
                JsonSerializer.Deserialize<GatewayObject<JsonObject>>(message.Text!);

            if (unknownGatewayObject is null)
                return;

            if (unknownGatewayObject.SequenceNumber.HasValue)
                _lastSequenceNumber = unknownGatewayObject.SequenceNumber;

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
        throw new NotImplementedException();
    }
    
    private void HandleHello(GatewayObject<JsonObject> gatewayObject) {
        EventHello? data = gatewayObject.Data!.Deserialize<EventHello>();
        if (data is null) throw new ArgumentException("Data was null", nameof(gatewayObject));

        if (HeartbeatTimer is null) {
            HeartbeatTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(data.HeartbeatInterval));
            Task.Factory.StartNew(async () => {
                while (!_cts.Token.IsCancellationRequested) {
                    await HeartbeatTimer.WaitForNextTickAsync(_cts.Token);
                    SendHeartbeat();
                }
            }, TaskCreationOptions.LongRunning);
        }
        
        SendIdentify();
    }

    private void SendHeartbeat() {
        throw new NotImplementedException();
    }

    private void SendIdentify() {
        throw new NotImplementedException();
    }
}