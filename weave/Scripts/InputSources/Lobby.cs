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
    public delegate void PlayerInfoUpdatedEventHandler(PlayerInfo playerInfo);

    public delegate void PlayerJoinedEventHandler(IInputSource source);

    public delegate void PlayerLeftEventHandler(IInputSource source);

    private readonly IList<PlayerInfo> _playerInfos = new List<PlayerInfo>();

    /// <summary>
    ///     Note: Lobby is reset on start screen, so its one "session"
    /// </summary>
    public Lobby()
    {
        Id = Guid.NewGuid().ToString();
        Name = UniqueNameGenerator.Instance.New();
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
            return;

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

    private void UpdatePlayerInfos()
    {
        var colorGenerator = new UniqueColorGenerator();
        PlayerInfos.ForEach(playerInfo =>
        {
            playerInfo.Color = colorGenerator.NewColor();
            playerInfo.Name = (_playerInfos.IndexOf(playerInfo) + 1).ToString();
            PlayerInfoUpdatedListeners?.Invoke(playerInfo);
        });
    }

    #region GameSessionIdentifiers

    public string Id { get; set; }
    public string Name { get; set; }
    public int HighScore { get; set; }

    #endregion GameSessionIdentifiers
}
