using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.Utils;
using Weave.InputSources;

namespace Weave.MenuControllers;

[Scene("res://Menus/StartScreen.tscn")]
public partial class StartScreen : Control
{
    private readonly Lobby _lobby = new();

    private PackedScene _lobbyPlayer = GD.Load<PackedScene>("res://Objects/LobbyPlayer.tscn");

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Play")]
    private Button _playButton;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Options")]
    private Button _optionsButton;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Quit")]
    private Button _quitButton;

    [GetNode("BlurLayer")]
    private CanvasLayer _blurLayer;

    [GetNode("UI/MarginContainer/HBoxContainer/VSeparator")]
    private VSeparator _vSeparator;

    [GetNode("UI/MarginContainer/HBoxContainer/PlayerList")]
    private VBoxContainer _playerList;

    [GetNode("UI/MarginContainer/HBoxContainer/Start")]
    private Button _startButton;

    public override void _Ready()
    {
        this.GetNodes();
        _playButton.Pressed += OpenLobby;
        _quitButton.Pressed += OnQuitButtonPressed;
        _startButton.Pressed += OnStartButtonPressed;

        var colorGen = new UniqueColorGenerator();
        GetTree()
            .GetNodesInGroup(WeaveConstants.FireflyGroup)
            .Cast<Firefly>()
            .ForEach(f => f.SetColor(colorGen.NewColor()));
    }

    public override void _Input(InputEvent @event)
    {
        if (!_lobby.Open)
            return;
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
        _blurLayer.Visible = _lobby.Open;
        _playerList.Visible = _lobby.Open;
        _startButton.Visible = _lobby.Open;
        _vSeparator.Visible = _lobby.Open;
        CollapseButtons();
    }

    private void CloseLobby()
    {
        _lobby.Open = false;
        _blurLayer.Visible = false;
        _playerList.Visible = false;
        _startButton.Visible = false;
        _vSeparator.Visible = _lobby.Open;
        ExpandButtons();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnStartButtonPressed()
    {
        GameConfig.Lobby = _lobby;
        GetTree().ChangeSceneToFile(SceneGetter.GetPath<Main>());
    }

    private void ExpandButtons()
    {
        _playButton.Text = "PLAY";
        _optionsButton.Text = "OPTIONS";
        _quitButton.Text = "QUIT";
        _playButton.CustomMinimumSize = new Vector2(200, 0);
        _optionsButton.CustomMinimumSize = new Vector2(200, 0);
        _quitButton.CustomMinimumSize = new Vector2(200, 0);
    }

    private void CollapseButtons()
    {
        _playButton.Text = "";
        _optionsButton.Text = "";
        _quitButton.Text = "";
        _playButton.CustomMinimumSize = new Vector2(0, 0);
        _optionsButton.CustomMinimumSize = new Vector2(0, 0);
        _quitButton.CustomMinimumSize = new Vector2(0, 0);
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

            if (!isPressingBoth)
                continue;

            var kb = new KeyboardInputSource(keybindingTuple);
            var alreadyExisting = _lobby.PlayerInfos.FirstOrDefault(c => c.InputSource.Equals(kb))?.InputSource;

            if (alreadyExisting != null)
                _lobby.Leave(alreadyExisting);
            else
                _lobby.Join(kb);

            PrintInputSources();
        }
    }

    #endregion Keyboard

    #region Gamepad

    private void GamepadPressed(InputEvent @event)
    {
        var deviceId = @event.Device;
        if (deviceId < 0)
            return;

        if (@event.IsActionPressed(WeaveConstants.GamepadJoinAction))
            _lobby.Join(new GamepadInputSource(deviceId));

        if (@event.IsActionPressed(WeaveConstants.GamepadLeaveAction))
            _lobby.Leave(new GamepadInputSource(deviceId));

        PrintInputSources();
    }

    #endregion Gamepad
}
