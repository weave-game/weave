using System.Net;
using System.Threading.Tasks;
using SIPSorcery.Net;
using WebSocketSharp.Server;
using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using weave.InputSources;
using System.Security.Cryptography.X509Certificates;

namespace weave.Multiplayer;

public partial class Manager : Node
{
    [Signal]
    public delegate void PlayerJoinedEventHandler(WebInputSource source);
    [Signal]
    public delegate void PlayerLeftEventHandler(WebInputSource source);

    private const int WEBSOCKET_PORT = 8081;
    private string _lobbyCode;
    private WebSocketServer webSocketServer;
    private readonly Dictionary<string, WebInputSource> _playerSources = new();

    public Manager(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    public void StartServer()
    {
        GD.Print("Starting signaling server...");

        webSocketServer = new WebSocketServer(IPAddress.Any, WEBSOCKET_PORT, true);
        webSocketServer.SslConfiguration.ServerCertificate = new X509Certificate2("Scripts/Multiplayer/Certificates/certificate.pfx", "123");
        webSocketServer.SslConfiguration.CheckCertificateRevocation = false;
        webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>($"/{_lobbyCode}", (peer) => peer.CreatePeerConnection = () => CreatePeerConnection(peer.ID));
        webSocketServer.Start();

        GD.Print($"Waiting for web socket connections on {webSocketServer.Address}:{webSocketServer.Port}...");
    }

    private async Task<RTCPeerConnection> CreatePeerConnection(string id)
    {
        var pc = new RTCPeerConnection(null);

        var dataChannel = await pc.createDataChannel("chat");
        dataChannel.onopen += () => HandlePlayerJoin(id);
        dataChannel.onclose += () => HandlePlayerLeave(id);
        dataChannel.onmessage += (_, __, data) => HandlePlayerInput(id, data.GetStringFromUtf8());
        dataChannel.onerror += (error) => HandlePlayerError(id, error);

        pc.onconnectionstatechange += (state) =>
        {
            GD.Print($"Peer connection state change to {state}.");

            switch (state)
            {
                case RTCPeerConnectionState.connected:
                    break;
                case RTCPeerConnectionState.failed:
                    pc.Close("ice disconnection");
                    break;
                case RTCPeerConnectionState.closed:
                case RTCPeerConnectionState.disconnected:
                    HandlePlayerLeave(id);
                    break;
            }
        };

        return pc;
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

    private  void HandlePlayerInput(string playerId, string input)
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
        var messageObj = new Message(MessageType.StartGame);
        var wrappedMessage = new { message = messageObj };
        webSocketServer.WebSocketServices[$"/{_lobbyCode}"].Sessions.Broadcast(JsonConvert.SerializeObject(wrappedMessage));
    }

    public void NotifyEndGame()
    {
        var messageObj = new Message(MessageType.EndGame);
        var wrappedMessage = new { message = messageObj };
        webSocketServer.WebSocketServices[$"/{_lobbyCode}"].Sessions.Broadcast(JsonConvert.SerializeObject(wrappedMessage));
    }
}
