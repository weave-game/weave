using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSharper;
using Weave.Utils;

namespace Weave.InputSources;

public sealed class Lobby
{
    public delegate void PlayerJoinedEventHandler(IInputSource source);
    public PlayerJoinedEventHandler PlayerJoinedListeners { get; set; }
    public delegate void PlayerLeftEventHandler(IInputSource source);
    public PlayerLeftEventHandler PlayerLeftListeners { get; set; }

    private readonly IList<PlayerInfo> _playerInfos = new List<PlayerInfo>();
    public IReadOnlyList<PlayerInfo> PlayerInfos => _playerInfos.AsReadOnly();

    public int Count => PlayerInfos.Count;

    public bool Open { get; set; }

    public string LobbyCode { get; set; }
    private const int _lobbyCodeLength = 5;
    private const string _lobbyCodeCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

    public ImageTexture LobbyQRCode { get; set; }
    private const string _lobbyQRCodePath = WeaveConstants.WeaveFrontendURL;

    public Lobby()
    {
        LobbyCode = GenerateLobbyCode(_lobbyCodeCharacters, _lobbyCodeLength);
        LobbyQRCode = GenerateLobbyQRCode($"{_lobbyQRCodePath}/{LobbyCode}");
    }

    public void Join(IInputSource inputSource)
    {
        var alreadyExists = _playerInfos.Any(input => input.InputSource.Equals(inputSource));
        if (alreadyExists)
            return;

        var playerInfo = new PlayerInfo
        {
            InputSource = inputSource
        };

        _playerInfos.Add(playerInfo);
        UpdatePlayerInfos();
        PlayerJoinedListeners?.Invoke(inputSource);
    }

    public void Leave(IInputSource inputSource)
    {
        _playerInfos.RemoveWhere(info => info.InputSource.Equals(inputSource));
        UpdatePlayerInfos();
        PlayerLeftListeners?.Invoke(inputSource);
    }

    private static string GenerateLobbyCode(string allowedCharacters, int length)
    {
        var rnd = new Random();
        var result = new char[length];
        for (var i = 0; i < length; i++)
            result[i] = allowedCharacters[rnd.Next(allowedCharacters.Length - 1)];
        return new string(result);
    }

    private static ImageTexture GenerateLobbyQRCode(string str)
    {
        return GDScriptHelper.GenerateQRCodeFromString(str);
    }
    private void UpdatePlayerInfos()
    {
        var colorGenerator = new UniqueColorGenerator();
        PlayerInfos.ForEach(playerInfo =>
        {
            playerInfo.Color = colorGenerator.NewColor();
            playerInfo.Name = (_playerInfos.IndexOf(playerInfo) + 1).ToString();
        });
    }
}
