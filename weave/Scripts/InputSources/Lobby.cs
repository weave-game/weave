using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GodotSharper;
using Weave.Utils;

namespace Weave.InputSources;

public sealed class Lobby
{
    private readonly IList<IInputSource> _inputSources = new List<IInputSource>();

    private readonly IList<PlayerInfo> _playerInfos = new List<PlayerInfo>();
    public IReadOnlyList<PlayerInfo> PlayerInfos => _playerInfos.AsReadOnly();

    public int Count => PlayerInfos.Count;

    public bool Open { get; set; }

    public void Join(IInputSource inputSource)
    {
        var alreadyExists = _inputSources.FirstOrDefault(input => input.Equals(inputSource));
        if (alreadyExists != null)
            return;

        _inputSources.Add(inputSource);

        var playerInfo = new PlayerInfo
        {
            InputSource = inputSource
        };

        _playerInfos.Add(playerInfo);
        UpdatePlayerInfos();
    }

    public void Leave(IInputSource inputSource)
    {
        _inputSources.Remove(inputSource);
        _playerInfos.RemoveWhere(info => info.InputSource.Equals(inputSource));
        UpdatePlayerInfos();
    }

    private void UpdatePlayerInfos()
    {
        var colorGenerator = new UniqueColorGenerator();
        PlayerInfos.ForEach(playerInfo =>
        {
            playerInfo.Color =  colorGenerator.NewColor();
            playerInfo.Name = (_playerInfos.IndexOf(playerInfo) + 1).ToString();
        });
    }
}