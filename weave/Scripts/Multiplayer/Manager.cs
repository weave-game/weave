using Godot;
using SIPSorcery.Net;
using Newtonsoft.Json;
using weave.InputSources;

namespace weave.Multiplayer;

public partial class Manager : Node
{
    [Signal]
    public delegate void PlayerJoinedEventHandler(WebInputSource source);
    [Signal]
    public delegate void PlayerLeftEventHandler(WebInputSource source);

    private string _lobbyCode;
    private const string SERVER_URL = "ws://localhost:8080";
    private readonly Dictionary<string, WebInputSource> _playerSources = new();

    public Manager(string lobbyCode)
    {
        _lobbyCode = lobbyCode;
    }

    public async Task StartServer()
    {
        var cts = new CancellationToken();
        var client = new WebRTCWebSocketClient(SERVER_URL, CreatePeerConnection);
        await client.Start(cts);
    }

    private static Task<RTCPeerConnection> CreatePeerConnection()
    {
        var pc = new RTCPeerConnection(null);

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
                    break;
            }
        };

        return Task.FromResult(pc);
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
