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
    private readonly Dictionary<string, WebInputSource> _playerSources = new();

    public Manager(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    public async Task StartClientAsync()
    {
        await ConnectWebSocket();

        await SendWebSocketMessage("{\"type\": \"register-host\"}");

        var peerConnection = new RTCPeerConnection(null);

        var dataChannel = await peerConnection.createDataChannel("chat");
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

        peerConnection.onicecandidate += (candidate) =>
        {
            var iceMessage = new { type = "ice-candidate", candidate = candidate };
            SendWebSocketMessage(JsonConvert.SerializeObject(iceMessage)).Wait();
        };

        while (_webSocket.State == WebSocketState.Open)
        {
            var message = await ReceiveWebSocketMessage();
            if (!string.IsNullOrWhiteSpace(message))
            {
                dynamic msg = JsonConvert.DeserializeObject(message);
                if (msg.type == "offer")
                {
                    GD.Print("received offer");
                    var offer = new RTCSessionDescriptionInit();
                    if (RTCSessionDescriptionInit.TryParse(msg.offer, out offer))
                    {
                        peerConnection.setRemoteDescription(offer);
                        var answer = peerConnection.createAnswer(null);
                        await peerConnection.setLocalDescription(answer);

                        var answerMessage = new { type = "answer", answer = new { type = answer.type.ToString().ToLower(), sdp = answer.sdp } };
                        await SendWebSocketMessage(JsonConvert.SerializeObject(answerMessage));
                    }
                    else
                    {
                        GD.Print("Unable to parse offer");
                    }
                }
                else if (msg.type == "ice-candidate")
                {
                    if (msg.candidate != null)
                    {
                        GD.Print("Received ice candidate");
                        var iceCandidate = new RTCIceCandidateInit();
                        if (RTCIceCandidateInit.TryParse(msg.candidate, out iceCandidate))
                            peerConnection.addIceCandidate(iceCandidate);
                        else GD.Print("Unable to parse ice candidate");
                    }
                }
            }
        }
    }

    private static async Task ConnectWebSocket()
    {
        await _webSocket.ConnectAsync(new Uri(SERVER_URL), CancellationToken.None);
        GD.Print($"WebSocket connection established to {SERVER_URL}");
    }

    private static async Task SendWebSocketMessage(string message)
    {
        var msgBuffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(msgBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<string> ReceiveWebSocketMessage()
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
        _playerSources.Add(playerId, sourceToAdd);
    }

    private void HandlePlayerLeave(string playerId)
    {
        var sourceToRemove = _playerSources.GetValueOrDefault(playerId);
        EmitSignal(SignalName.PlayerLeft, sourceToRemove);
        _playerSources.Remove(playerId);
    }

    private void HandlePlayerInput(string playerId, string input)
    {
        var source = _playerSources.GetValueOrDefault(playerId);
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
