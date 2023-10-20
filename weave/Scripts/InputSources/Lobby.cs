using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSharper;
using Weave.QR;
using Weave.Utils;

namespace Weave.InputSources;

public sealed class Lobby
{
    public string Name { get; set; }

    public delegate void PlayerJoinedEventHandler(IInputSource source);
    public delegate void PlayerLeftEventHandler(IInputSource source);
    public delegate void PlayerInfoUpdatedEventHandler(PlayerInfo playerInfo);
    private const int LobbyCodeLength = 4;
    private const string LobbyCodeCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const string LobbyQrCodePath = WeaveConstants.WeaveFrontendUrl;

    private readonly IList<PlayerInfo> _playerInfos = new List<PlayerInfo>();
    private readonly IQrCodeGenerator _qrCodeGenerator;

    public Lobby()
    {
        Name = UniqueNameGenerator.Instance.New();
        _qrCodeGenerator = new GdQrCodeGenerator();
        LobbyCode = GenerateLobbyCode(LobbyCodeCharacters, LobbyCodeLength);
        LobbyQrCode = GenerateLobbyQrCode($"{LobbyQrCodePath}/{LobbyCode}");
    }

    public PlayerJoinedEventHandler PlayerJoinedListeners { get; set; }
    public PlayerLeftEventHandler PlayerLeftListeners { get; set; }
    public PlayerInfoUpdatedEventHandler PlayerInfoUpdatedListeners { get; set; }
    public IReadOnlyList<PlayerInfo> PlayerInfos => _playerInfos.AsReadOnly();

    public int Count => PlayerInfos.Count;

    public bool Open { get; set; }

    public string LobbyCode { get; }

    public ImageTexture LobbyQrCode { get; }

    public void Join(IInputSource inputSource)
    {
        var alreadyExists = _playerInfos.Any(input => input.InputSource.Equals(inputSource));
        if (alreadyExists)
        {
            return;
        }

        var playerInfo = new PlayerInfo { InputSource = inputSource };

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
        {
            result[i] = allowedCharacters[rnd.Next(allowedCharacters.Length - 1)];
        }

        return new(result);
    }

    private ImageTexture GenerateLobbyQrCode(string str)
    {
        return _qrCodeGenerator.GenerateQrCodeFromString(str);
    }

    private void UpdatePlayerInfos()
    {
        var colorGenerator = new UniqueColorGenerator();
        PlayerInfos.ForEach(
            playerInfo =>
            {
                playerInfo.Color = colorGenerator.NewColor();
                playerInfo.Name = (_playerInfos.IndexOf(playerInfo) + 1).ToString();
                PlayerInfoUpdatedListeners?.Invoke(playerInfo);
            }
        );
    }
}
