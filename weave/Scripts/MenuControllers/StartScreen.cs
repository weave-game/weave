using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.InputSources;
using Weave.Networking;
using Weave.Utils;

namespace Weave.MenuControllers;

[Scene("res://Menus/StartScreen.tscn")]
public partial class StartScreen : Control
{
    private readonly Lobby _lobby = new();

    [GetNode("BlurLayer")]
    private CanvasLayer _blurLayer;

    [GetNode("UI/LobbyCodeLabel")]
    private RichTextLabel _lobbyCodeLabel;

    /// <summary>
    /// Dictionary mapping a keybinding to a tuple with the time keybinding was pressed
    /// and a bool indicating whether the keybinding has left/joined the lobby during this keypress
    /// </summary>
    private Dictionary<(Key, Key), (DateTime?, bool)> _pressingSince = new();

    private PackedScene _lobbyPlayer = GD.Load<PackedScene>("res://Objects/LobbyPlayer.tscn");

    [GetNode("UI/MemoriesLabel")]
    private RichTextLabel _memoriesLabel;

    private RTCClientManager _multiplayerManager;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Options")]
    private Button _optionsButton;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Play")]
    private Button _playButton;

    [GetNode("UI/MarginContainer/HBoxContainer/PlayerList")]
    private VBoxContainer _playerList;

    [GetNode("UI/QRCodeTexture")]
    private TextureRect _qrCodeTexture;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Quit")]
    private Button _quitButton;

    [GetNode("UI/StartButton")]
    private Button _startButton;

    [GetNode("UI/MarginContainer/HBoxContainer/VSeparator")]
    private VSeparator _vSeparator;

    public override void _Ready()
    {
        this.GetNodes();
        _playButton.Pressed += OpenLobby;
        _quitButton.Pressed += OnQuitButtonPressed;
        _startButton.Pressed += OnStartButtonPressed;

        _lobby.PlayerJoinedListeners += _ => CallDeferred(nameof(PrintInputSources));
        _lobby.PlayerLeftListeners += _ => CallDeferred(nameof(PrintInputSources));

        SetLobbyCodeLabelText(_lobby.LobbyCode);
        SetLobbyQrCodeTexture(_lobby.LobbyQrCode);

        _multiplayerManager = new(_lobby.LobbyCode);
        _multiplayerManager.StartClientAsync();

        _multiplayerManager.ClientJoinedListeners += _lobby.Join;
        _multiplayerManager.ClientLeftListeners += _lobby.Leave;

        var colorGen = new UniqueColorGenerator();
        GetTree()
            .GetNodesInGroup(WeaveConstants.FireflyGroup)
            .Cast<Firefly>()
            .ForEach(f => f.SetColor(colorGen.NewColor()));
    }

    public override void _Input(InputEvent @event)
    {
        if (!_lobby.Open)
        {
            return;
        }

        switch (@event)
        {
            case InputEventJoypadButton button:
                GamepadPressed(button);
                break;
            case InputEventKey { Pressed: true }:
                KeyboardPressed();
                break;
        }
    }

    private void OpenLobby()
    {
        _lobby.Open = true;
        _blurLayer.Visible = true;
        _playerList.Visible = true;
        _startButton.Visible = true;
        _vSeparator.Visible = true;
        _memoriesLabel.Visible = true;
        _lobbyCodeLabel.Visible = true;
        _qrCodeTexture.Visible = true;
        CollapseButtons();
    }

    private void CloseLobby()
    {
        _lobby.Open = false;
        _blurLayer.Visible = false;
        _playerList.Visible = false;
        _startButton.Visible = false;
        _vSeparator.Visible = false;
        _memoriesLabel.Visible = false;
        _lobbyCodeLabel.Visible = false;
        _qrCodeTexture.Visible = false;
        ExpandButtons();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnStartButtonPressed()
    {
        GameConfig.Lobby = _lobby;
        GameConfig.MultiplayerManager = _multiplayerManager;
        GetTree().ChangeSceneToFile(SceneGetter.GetPath<Main>());
    }

    private void ExpandButtons()
    {
        _playButton.Text = "PLAY";
        _optionsButton.Text = "OPTIONS";
        _quitButton.Text = "QUIT";
        _playButton.CustomMinimumSize = new(200, 0);
        _optionsButton.CustomMinimumSize = new(200, 0);
        _quitButton.CustomMinimumSize = new(200, 0);
    }

    private void CollapseButtons()
    {
        _playButton.Text = "";
        _optionsButton.Text = "";
        _quitButton.Text = "";
        _playButton.CustomMinimumSize = new(0, 0);
        _optionsButton.CustomMinimumSize = new(0, 0);
        _quitButton.CustomMinimumSize = new(0, 0);
    }

    private void PrintInputSources()
    {
        foreach (var child in _playerList.GetChildren())
        {
            _playerList.RemoveChild(child);
            child.QueueFree();
        }

        foreach (var playerInfo in _lobby.PlayerInfos)
        {
            var lobbyPlayer = _lobbyPlayer.Instantiate<MarginContainer>();
            lobbyPlayer.Modulate = playerInfo.Color;
            lobbyPlayer.GetNode<Label>("HBoxContainer/LeftBinding").Text = $"← {playerInfo.InputSource.LeftInputString()}";
            lobbyPlayer.GetNode<Label>("HBoxContainer/RightBinding").Text = $"{playerInfo.InputSource.RightInputString()} →";
            _playerList.AddChild(lobbyPlayer);
        }
    }

    #region Keyboard

    private void KeyboardPressed()
    {
        foreach (var keybindingTuple in KeyboardBindings.Keybindings)
        {
            var isPressingBoth =
                Input.IsKeyPressed(keybindingTuple.Item1)
                && Input.IsKeyPressed(keybindingTuple.Item2);

            var kb = new KeyboardInputSource(keybindingTuple);
            var alreadyExisting = _lobby.PlayerInfos.FirstOrDefault(c => c.InputSource.Equals(kb))?.InputSource;

            if (!isPressingBoth)
            {
                _pressingSince[keybindingTuple] = (null, false);
                continue;
            }

            if (_pressingSince.ContainsKey(keybindingTuple) && _pressingSince[keybindingTuple].Item2)
                continue;

            if (alreadyExisting == null)
            {
                _lobby.Join(kb);
                _pressingSince[keybindingTuple] = (null, true);
            }
            else
            {
                if (!_pressingSince.ContainsKey(keybindingTuple) || _pressingSince[keybindingTuple].Item1 == null)
                {
                    _pressingSince[keybindingTuple] = (DateTime.Now, false);
                    continue;
                }

                if (!(DateTime.Now - _pressingSince[keybindingTuple].Item1 >= TimeSpan.FromSeconds(0.5))) continue;

                _lobby.Leave(alreadyExisting);

                _pressingSince[keybindingTuple] = (null, true);
            }
        }
    }

    #endregion Keyboard

    #region Gamepad

    private void GamepadPressed(InputEvent @event)
    {
        var deviceId = @event.Device;
        if (deviceId < 0)
        {
            return;
        }

        if (@event.IsActionPressed(WeaveConstants.GamepadJoinAction))
        {
            _lobby.Join(new GamepadInputSource(deviceId));
        }

        if (@event.IsActionPressed(WeaveConstants.GamepadLeaveAction))
        {
            _lobby.Leave(new GamepadInputSource(deviceId));
        }
    }

    #endregion Gamepad

    private void SetLobbyCodeLabelText(string newCode)
    {
        _lobbyCodeLabel.Text = $"[center]Lobby code: {newCode}[/center]";
    }

    private void SetLobbyQrCodeTexture(Texture2D newTexture)
    {
        _qrCodeTexture.Texture = newTexture;
    }
}
