using System;
using System.Collections.Generic;
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

    private readonly IDictionary<string, RTCPeerConnection> _clientConnections =
        new Dictionary<string, RTCPeerConnection>();

    private readonly IDictionary<string, WebInputSource> _clientSources =
        new Dictionary<string, WebInputSource>();

    private readonly RTCConfiguration _connectionConfig =
        new()
        {
            iceServers = new List<RTCIceServer> { new() { urls = WeaveConstants.StunServerUrl } }
        };

    private readonly string _lobbyCode;
    private readonly ClientWebSocket _webSocket = new();

    public RTCClientManager(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    public bool IsAcceptingInput { get; set; }

    public ClientJoinedEventHandler ClientJoinedListeners { get; set; }
    public ClientLeftEventHandler ClientLeftListeners { get; set; }

    public async void StartClientAsync()
    {
        await ConnectWebSocketAsync();

        var joinMessage = new { type = "register-host", lobbyCode = _lobbyCode };
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

                if (
                    RTCSessionDescriptionInit.TryParse(
                        answerString,
                        out RTCSessionDescriptionInit answer
                    )
                )
                    peerConnection.setRemoteDescription(answer);
                else
                    GD.Print("Unable to parse answer");
            }
            else if (msg?.type == "ice-candidate")
            {
                var idString = (string)msg.clientId;
                var candidateString = JsonConvert.SerializeObject(msg.candidate);
                var peerConnection = _clientConnections[idString];

                if (
                    RTCIceCandidateInit.TryParse(
                        candidateString,
                        out RTCIceCandidateInit iceCandidate
                    )
                )
                    peerConnection.addIceCandidate(iceCandidate);
                else
                    GD.Print("Unable to parse ice candidate");
            }
            else if (msg?.type == "client-connected")
            {
                var idString = (string)msg.clientId;
                var peerConnection = await CreatePeerConnectionAsync(idString);
                await SendOfferAsync(peerConnection, idString);
            }
            else if (msg.type == "client-disconnected")
            {
                var idString = (string)msg.clientId;
                HandlePlayerLeave(idString);
            }
            else if (msg.type == "error")
            {
                GD.Print($"Received error: {msg.message}");
            }
            else
            {
                GD.Print("Received unknown message type");
            }
        }
    }

    public async void StopClientAsync()
    {
        if (_webSocket is not { State: WebSocketState.Open })
            return;

        await _webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Normal closure",
            CancellationToken.None
        );

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
        dataChannel.onmessage += (_, _, data) =>
            HandlePlayerInput(clientId, data.GetStringFromUtf8());
        dataChannel.onerror += error => GD.Print($"Data channel closed: {error}");

        peerConnection.onconnectionstatechange += state =>
        {
            GD.Print($"{clientId} connection state change to {state}.");

            switch (state)
            {
                case RTCPeerConnectionState.failed:
                    peerConnection.Close("ice disconnection");
                    break;
                case RTCPeerConnectionState.closed:
                case RTCPeerConnectionState.disconnected:
                    HandlePlayerLeave(clientId);
                    break;
            }
        };

        peerConnection.onicecandidate += async candidate =>
        {
            var iceMessage = new
            {
                type = "ice-candidate-host",
                candidate,
                clientId,
                lobbyCode = _lobbyCode
            };
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(iceMessage));
        };

        return peerConnection;
    }

    private async Task SendOfferAsync(IRTCPeerConnection peerConnection, string clientId)
    {
        var offer = peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);

        var offerMessage = new
        {
            type = "offer",
            offer,
            clientId,
            lobbyCode = _lobbyCode
        };
        await SendWebSocketMessageAsync(JsonConvert.SerializeObject(offerMessage));
    }

    private async Task ConnectWebSocketAsync()
    {
        await _webSocket.ConnectAsync(new Uri(ServerUrl), CancellationToken.None);
        GD.Print($"WebSocket connection established to {ServerUrl}");
    }

    private async Task SendWebSocketMessageAsync(string message)
    {
        var msgBuffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(
            new ArraySegment<byte>(msgBuffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
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
        _clientConnections.TryGetValue(clientId, out var connection);

        if (connection != null && connection.connectionState == RTCPeerConnectionState.connected)
            connection.close();

        if (source != null && _webSocket.State == WebSocketState.Open)
            ClientLeftListeners?.Invoke(source);

        _clientConnections.Remove(clientId);
        _clientSources.Remove(clientId);
    }

    private void HandlePlayerInput(string playerId, string input)
    {
        if (!IsAcceptingInput)
            return;

        _clientSources.TryGetValue(playerId, out var source);
        if (source == null)
            return;

        source.SetDirection(input);
    }

    public async void NotifyStartGameAsync()
    {
        IsAcceptingInput = true;
        foreach (var clientId in _clientConnections.Keys)
        {
            var startMessage = new
            {
                type = "message",
                message = "start",
                clientId,
                lobbyCode = _lobbyCode
            };
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(startMessage));
        }
    }

    public async void NotifyEndGameAsync()
    {
        IsAcceptingInput = false;
        foreach (var clientId in _clientConnections.Keys)
        {
            var endMessage = new
            {
                type = "message",
                message = "end",
                clientId,
                lobbyCode = _lobbyCode
            };
            await SendWebSocketMessageAsync(JsonConvert.SerializeObject(endMessage));
        }
    }

    public async void NotifyChangePlayerColor(string clientId, string newColor)
    {
        var colorMessage = new
        {
            type = "color-change",
            color = newColor,
            clientId,
            lobbyCode = _lobbyCode
        };
        await SendWebSocketMessageAsync(JsonConvert.SerializeObject(colorMessage));
    }
}
