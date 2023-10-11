using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Weave.InputSources;

public sealed class Lobby
{
    private readonly IList<IInputSource> _inputSources = new List<IInputSource>();
    public IReadOnlyList<IInputSource> InputSources => _inputSources.AsReadOnly();
    public int Count => _inputSources.Count;

    public string LobbyCode { get; set; }
    private const int _lobbyCodeLength = 5;
    private const string _lobbyCodeCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

    public ImageTexture LobbyQRCode { get; set; }
    private const string _lobbyQRCodePath = "localhost:3000";

    public Lobby()
    {
        LobbyCode = GenerateLobbyCode(_lobbyCodeCharacters, _lobbyCodeLength);
        LobbyQRCode = GenerateLobbyQRCode($"{_lobbyQRCodePath}/{LobbyCode}");
    }

    public void Join(IInputSource inputSource)
    {
        var alreadyExists = _inputSources.FirstOrDefault(input => input.Equals(inputSource));
        if (alreadyExists != null)
            return;

        _inputSources.Add(inputSource);
    }

    public void Leave(IInputSource inputSource)
    {
        _inputSources.Remove(inputSource);
    }

    private static string GenerateLobbyCode(string allowedCharacters, int length)
    {
        var rnd = new Random();
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = allowedCharacters[rnd.Next(allowedCharacters.Length - 1)];
        }
        return new string(result);
    }

    private static ImageTexture GenerateLobbyQRCode(string str)
    {
        return GDScriptHelper.GenerateQRCodeFromString(str);
    }
}
