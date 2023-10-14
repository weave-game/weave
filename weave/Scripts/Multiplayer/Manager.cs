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

namespace Weave.Multiplayer;

public partial class Manager : Node
{
    [Signal]
    public delegate void PlayerJoinedEventHandler(WebInputSource source);
    [Signal]
    public delegate void PlayerLeftEventHandler(WebInputSource source);

    private string _lobbyCode;
    private const string SERVER_URL = "ws://localhost:8080";
    private static readonly ClientWebSocket _webSocket = new();
    private readonly Dictionary<string, WebInputSource> _clientSources = new();
    private readonly Dictionary<string, RTCPeerConnection> _clientConnections = new();

    public Manager(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    public async Task StartClientAsync()
    {
        await ConnectWebSocketAsync();

        GD.Print($"Lobby code: {_lobbyCode}");

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
                SendOfferAsync(peerConnection, idString);
            }
        }
    }

    private async Task<RTCPeerConnection> CreatePeerConnectionAsync(string clientId)
    {
        var peerConnection = new RTCPeerConnection(null);

        _clientConnections.Add(clientId, peerConnection);

        var dataChannel = await peerConnection.createDataChannel($"chat-{clientId}");
        dataChannel.onopen += () => GD.Print("data channel open");
        dataChannel.onclose += () => GD.Print("data channel closed");
        dataChannel.onmessage += (_, __, data) => GD.Print(data.GetStringFromUtf8());

        peerConnection.onconnectionstatechange += (state) =>
        {
            GD.Print($"Peer connection state change to {state}.");

            switch (state)
            {
                case RTCPeerConnectionState.connected:
                    break;
                case RTCPeerConnectionState.failed:
                    peerConnection.Close("ice disconnection");
                    break;
                case RTCPeerConnectionState.closed:
                case RTCPeerConnectionState.disconnected:
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

    private static async void SendOfferAsync(RTCPeerConnection peerConnection, string clientId)
    {
        var offer = peerConnection.createOffer(null);
        await peerConnection.setLocalDescription(offer);

        var offerMessage = new { type = "offer", offer, clientId };
        await SendWebSocketMessageAsync(JsonConvert.SerializeObject(offerMessage));
    }

    private static async Task ConnectWebSocketAsync()
    {
        await _webSocket.ConnectAsync(new Uri(SERVER_URL), CancellationToken.None);
        GD.Print($"WebSocket connection established to {SERVER_URL}");
    }

    private static async Task SendWebSocketMessageAsync(string message)
    {
        var msgBuffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(msgBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<string> ReceiveWebSocketMessageAsync()
    {
        var buffer = new byte[4096];
        var segment = new ArraySegment<byte>(buffer);
        var result = await _webSocket.ReceiveAsync(segment, CancellationToken.None);
        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    private void HandlePlayerJoin(string playerId)
    {
        var sourceToAdd = new WebInputSource(playerId);
        EmitSignal(SignalName.PlayerJoined, sourceToAdd);
        _clientSources.Add(playerId, sourceToAdd);
    }

    private void HandlePlayerLeave(string playerId)
    {
        var sourceToRemove = _clientSources.GetValueOrDefault(playerId);
        EmitSignal(SignalName.PlayerLeft, sourceToRemove);
        _clientSources.Remove(playerId);
    }

    private void HandlePlayerInput(string playerId, string input)
    {
        var source = _clientSources.GetValueOrDefault(playerId);
        source.DirectionState = input;
    }

    private static void HandlePlayerError(string playerId, string error)
    {
        GD.Print($"Error: {playerId} got error {error}");
    }

    public void NotifyStartGame()
    {
    }

    public void NotifyEndGame()
    {
    }
}
