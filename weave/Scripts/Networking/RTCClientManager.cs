using Godot;
using System;
using SIPSorcery.Net;
using Newtonsoft.Json;
using Weave.InputSources;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using Weave.Utils;

namespace Weave.Networking;

public class RTCClientManager
{
    public delegate void ClientJoinedEventHandler(WebInputSource source);
    public ClientJoinedEventHandler ClientJoinedListeners { get; set; }
    public delegate void ClientLeftEventHandler(WebInputSource source);
    public ClientLeftEventHandler ClientLeftListeners { get; set; }

    private readonly string _lobbyCode;
    private const string SERVER_URL = WeaveConstants.SignallingServerURL;
    private readonly ClientWebSocket _webSocket = new();
    private readonly Dictionary<string, WebInputSource> _clientSources = new();
    private readonly Dictionary<string, RTCPeerConnection> _clientConnections = new();
    private readonly RTCConfiguration _connectionConfig = new()
    {
        iceServers = new List<RTCIceServer>
    {
        new() {
            urls = WeaveConstants.STUNServerURL
        }
    }
    };

    public RTCClientManager(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    public async Task StartClientAsync()
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
            if (msg.type == "answer")
            {
                var idString = (string)msg.clientId;
                var answerString = JsonConvert.SerializeObject(msg.answer);
                var peerConnection = _clientConnections[idString];

                var answer = new RTCSessionDescriptionInit();
                if (RTCSessionDescriptionInit.TryParse(answerString, out answer))
                    peerConnection.setRemoteDescription(answer);
                else GD.Print("Unable to parse answer");
            }
            else if (msg.type == "ice-candidate")
            {
                var idString = (string)msg.clientId;
                var candidateString = JsonConvert.SerializeObject(msg.candidate);
                var peerConnection = _clientConnections[idString];

                var iceCandidate = new RTCIceCandidateInit();
                if (RTCIceCandidateInit.TryParse(candidateString, out iceCandidate))
                    peerConnection.addIceCandidate(iceCandidate);
                else GD.Print("Unable to parse ice candidate");
            }
            else if (msg.type == "client-connected")
            {
                var idString = (string)msg.clientId;
                var peerConnection = await CreatePeerConnectionAsync(idString);
                await SendOfferAsync(peerConnection, idString);
            }
        }
    }

    public async Task StopClientAsync()
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            return;

        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
        foreach (var connection in _clientConnections.Values)
            connection.Close("");
    }

    private async Task<RTCPeerConnection> CreatePeerConnectionAsync(string clientId)
    {
        var peerConnection = new RTCPeerConnection(_connectionConfig);

        _clientConnections.Add(clientId, peerConnection);

        var dataChannel = await peerConnection.createDataChannel($"chat-{clientId}");
        dataChannel.onopen += () => HandlePlayerJoin(clientId);
        dataChannel.onclose += () => HandlePlayerLeave(clientId);
        dataChannel.onmessage += (_, __, data) => HandlePlayerInput(clientId, data.GetStringFromUtf8());

        peerConnection.onconnectionstatechange += (state) =>
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
            }
        };

        peerConnection.onicecandidate += async (candidate) =>
        {
            var iceMessage = new { type = "ice-candidate-host", candidate, clientId };
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(iceMessage));
        };

        return peerConnection;
    }

    private async Task SendOfferAsync(RTCPeerConnection peerConnection, string clientId)
    {
        var offer = peerConnection.createOffer(null);
        await peerConnection.setLocalDescription(offer);

        var offerMessage = new { type = "offer", offer, clientId };
        await SendWebSocketMessageAsync(JsonConvert.SerializeObject(offerMessage));
    }

    private async Task ConnectWebSocketAsync()
    {
        await _webSocket.ConnectAsync(new Uri(SERVER_URL), CancellationToken.None);
        GD.Print($"WebSocket connection established to {SERVER_URL}");
    }

    private async Task SendWebSocketMessageAsync(string message)
    {
        var msgBuffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(msgBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
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
        ClientLeftListeners?.Invoke(_clientSources.GetValueOrDefault(clientId));
        _clientConnections.Remove(clientId);
        _clientSources.Remove(clientId);
    }

    private void HandlePlayerInput(string playerId, string input)
    {
        var source = _clientSources.GetValueOrDefault(playerId);
        source.DirectionState = input;
    }

    public async Task NotifyStartGameAsync()
    {
        foreach (var clientId in _clientConnections.Keys)
        {
            var startMessage = new { type = "message", message = "start", clientId };
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(startMessage));
        }
    }

    public async Task NotifyEndGameAsync()
    {
        foreach (var clientId in _clientConnections.Keys)
        {
            var endMessage = new { type = "message", message = "end", clientId };
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(endMessage));
        }
    }
}
