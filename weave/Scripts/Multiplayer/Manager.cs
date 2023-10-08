using System.Net;
using System.Threading.Tasks;
using SIPSorcery.Net;
using WebSocketSharp.Server;
using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace weave.Multiplayer;

public class Manager
{
    private const int WEBSOCKET_PORT = 8081;
    private WebSocketServer webSocketServer;
    private ISet<Player> _players = new HashSet<Player>();

    public void StartServer(string lobbyCode)
    {
        GD.Print("Starting signaling server...");

        webSocketServer = new WebSocketServer(IPAddress.Any, WEBSOCKET_PORT);
        webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>($"/{lobbyCode}", (peer) => peer.CreatePeerConnection = () => CreatePeerConnection(peer.ID));
        webSocketServer.Start();

        GD.Print($"Waiting for web socket connections on {webSocketServer.Address}:{webSocketServer.Port}...");
    }

    private async Task<RTCPeerConnection> CreatePeerConnection(string id)
    {
        var pc = new RTCPeerConnection(null);

        var dataChannel = await pc.createDataChannel("chat");
        dataChannel.onopen += () => HandlePlayerJoin(id);
        dataChannel.onclose += () => HandlePlayerLeave(id);
        dataChannel.onmessage += (dc, protocol, data) => HandlePlayerInput(id, data.GetStringFromUtf8());
        dataChannel.onerror += (error) => HandlePlayerError(id, error);

        pc.onconnectionstatechange += async (state) =>
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
                    break;
            }
        };

        return pc;
    }

    private void HandlePlayerJoin(string playerId)
    {
        _players.Add(new Player(playerId));
    }

    private void HandlePlayerLeave(string playerId)
    {
        var itemToRemove = _players.FirstOrDefault(obj => obj.Id == playerId);

        if (itemToRemove != null)
        {
            _players.Remove(itemToRemove);
        }
    }

    private void HandlePlayerInput(string playerId, string input)
    {
        GD.Print($"{playerId}: {input}");
    }

    private void HandlePlayerError(string playerId, string error)
    {
        GD.Print("WebRTC data channel error: " + error);
    }

    public void NotifyStartGame()
    {
        var message = new Message(MessageType.StartGame);
        webSocketServer.WebSocketServices["/"].Sessions.Broadcast(JsonConvert.SerializeObject(message));
    }

    public void NotifyEndGame()
    {
        var message = new Message(MessageType.EndGame);
        webSocketServer.WebSocketServices["/"].Sessions.Broadcast(JsonConvert.SerializeObject(message));
    }
}
