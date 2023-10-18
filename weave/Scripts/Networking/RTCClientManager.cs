using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using SIPSorcery.Net;
using Weave.InputSources;
using Weave.Utils;

namespace Weave.Networking;

public class RTCClientManager
{
    public delegate void ClientJoinedEventHandler(WebInputSource source);
    public delegate void ClientLeftEventHandler(WebInputSource source);
    private const string ServerUrl = WeaveConstants.SignallingServerUrl;
    private readonly IDictionary<string, RTCPeerConnection> _clientConnections = new Dictionary<string, RTCPeerConnection>();
    private readonly IDictionary<string, WebInputSource> _clientSources = new Dictionary<string, WebInputSource>();

    private readonly RTCConfiguration _connectionConfig = new() { iceServers = new() { new() { urls = WeaveConstants.StunServerUrl } } };

    private readonly string _lobbyCode;
    private readonly ClientWebSocket _webSocket = new();

    public RTCClientManager(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    public ClientJoinedEventHandler ClientJoinedListeners { get; set; }
    public ClientLeftEventHandler ClientLeftListeners { get; set; }

    public async void StartClientAsync()
    {
        await ConnectWebSocketAsync();

        var joinMessage = new { type = "register-host", lobby_code = _lobbyCode };
        await SendWebSocketMessageAsync(JsonConvert.SerializeObject(joinMessage));

        while (_webSocket.State == WebSocketState.Open)
        {
            var message = await ReceiveWebSocketMessageAsync();

            if (string.IsNullOrWhiteSpace(message))
            {
                GD.Print("Message is null");
                continue;
            }

            dynamic msg = JsonConvert.DeserializeObject(message);
            if (msg?.type == "answer")
            {
                var idString = (string)msg.clientId;
                var answerString = JsonConvert.SerializeObject(msg.answer);
                var peerConnection = _clientConnections[idString];

                if (RTCSessionDescriptionInit.TryParse(answerString, out RTCSessionDescriptionInit answer))
                {
                    peerConnection.setRemoteDescription(answer);
                }
                else
                {
                    GD.Print("Unable to parse answer");
                }
            }
            else if (msg?.type == "ice-candidate")
            {
                var idString = (string)msg.clientId;
                var candidateString = JsonConvert.SerializeObject(msg.candidate);
                var peerConnection = _clientConnections[idString];

                if (RTCIceCandidateInit.TryParse(candidateString, out RTCIceCandidateInit iceCandidate))
                {
                    peerConnection.addIceCandidate(iceCandidate);
                }
                else
                {
                    GD.Print("Unable to parse ice candidate");
                }
            }
            else if (msg?.type == "client-connected")
            {
                var idString = (string)msg.clientId;
                var peerConnection = await CreatePeerConnectionAsync(idString);
                await SendOfferAsync(peerConnection, idString);
            }
        }
    }

    public async void StopClientAsync()
    {
        if (_webSocket is not { State: WebSocketState.Open })
        {
            return;
        }

        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
        foreach (var connection in _clientConnections.Values)
        {
            connection.Close("");
        }
    }

    private async Task<RTCPeerConnection> CreatePeerConnectionAsync(string clientId)
    {
        var peerConnection = new RTCPeerConnection(_connectionConfig);

        _clientConnections.Add(clientId, peerConnection);

        var dataChannel = await peerConnection.createDataChannel($"chat-{clientId}");
        dataChannel.onopen += () => HandlePlayerJoin(clientId);
        dataChannel.onclose += () => HandlePlayerLeave(clientId);
        dataChannel.onmessage += (_, _, data) => HandlePlayerInput(clientId, data.GetStringFromUtf8());

        peerConnection.onconnectionstatechange += state =>
        {
            GD.Print($"{clientId} connection state change to {state}.");

            switch (state)
            {
                case RTCPeerConnectionState.connected:
                    break;
                case RTCPeerConnectionState.failed:
                    peerConnection.Close("ice disconnection");
                    break;
                case RTCPeerConnectionState.closed:
                case RTCPeerConnectionState.disconnected:
                    HandlePlayerLeave(clientId);
                    break;
                case RTCPeerConnectionState.@new:
                    break;
                case RTCPeerConnectionState.connecting:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        };

        peerConnection.onicecandidate += async candidate =>
        {
            var iceMessage = new { type = "ice-candidate-host", candidate, clientId };
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(iceMessage));
        };

        return peerConnection;
    }

    private async Task SendOfferAsync(IRTCPeerConnection peerConnection, string clientId)
    {
        var offer = peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);

        var offerMessage = new { type = "offer", offer, clientId };
        await SendWebSocketMessageAsync(JsonConvert.SerializeObject(offerMessage));
    }

    private async Task ConnectWebSocketAsync()
    {
        await _webSocket.ConnectAsync(new(ServerUrl), CancellationToken.None);
        GD.Print($"WebSocket connection established to {ServerUrl}");
    }

    private async Task SendWebSocketMessageAsync(string message)
    {
        var msgBuffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new(msgBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task<string> ReceiveWebSocketMessageAsync()
    {
        var buffer = new byte[4096];
        var segment = new ArraySegment<byte>(buffer);
        var result = await _webSocket.ReceiveAsync(segment, CancellationToken.None);
        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    private void HandlePlayerJoin(string clientId)
    {
        var sourceToAdd = new WebInputSource(clientId);
        ClientJoinedListeners?.Invoke(sourceToAdd);
        _clientSources.Add(clientId, sourceToAdd);
    }

    private void HandlePlayerLeave(string clientId)
    {
        _clientSources.TryGetValue(clientId, out var source);
        ClientLeftListeners?.Invoke(source);
        _clientConnections.Remove(clientId);
        _clientSources.Remove(clientId);
    }

    private void HandlePlayerInput(string playerId, string input)
    {
        _clientSources.TryGetValue(playerId, out var source);
        if (source == null)
        {
            return;
        }

        source.DirectionState = input;
    }

    public async void NotifyStartGameAsync()
    {
        foreach (var startMessage in _clientConnections.Keys.Select(clientId => new { type = "message", message = "start", clientId }))
        {
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(startMessage));
        }
    }

    public async void NotifyEndGameAsync()
    {
        foreach (var endMessage in _clientConnections.Keys.Select(clientId => new { type = "message", message = "end", clientId }))
        {
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(endMessage));
        }
    }
}
