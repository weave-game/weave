using System;
using System.Collections.Generic;
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

    /// <summary>
    ///     Dictionary mapping a keybinding to a tuple with the time keybinding was pressed
    ///     and a bool indicating whether the keybinding has left/joined the lobby during this keypress
    /// </summary>
    private readonly Dictionary<(Key, Key), (DateTime?, bool)> _pressingSince = new();

    [GetNode("AnimationPlayer")]
    private AnimationPlayer _animationPlayer;

    [GetNode("BlurLayer")]
    private CanvasLayer _blurLayer;

    [GetNode("UI/Instructions")]
    private Panel _instructions;

    [GetNode("UI/Instructions/Web/LobbyCodeLabel")]
    private RichTextLabel _lobbyCodeLabel;

    private PackedScene _lobbyPlayer = GD.Load<PackedScene>("res://Objects/LobbyPlayer.tscn");

    private IDictionary<PlayerInfo, Control> _lobbyPlayerDict = new Dictionary<PlayerInfo, Control>();

    [GetNode("UI/MemoriesLabel")]
    private RichTextLabel _memoriesLabel;

    [GetUniqueNode("JoinKeybinding")]
    private HBoxContainer _joinKeybindingContainer;

    private RTCClientManager _multiplayerManager;

    [GetNode("UI/MarginContainer/HBoxContainer/PlayerList")]
    private VBoxContainer _playerList;

    [GetUniqueNode("EmptyLobbyLabel")]
    private Label _emptyLobbyLabel;

    [GetNode("UI/Instructions/Web/QRCodeTexture")]
    private TextureRect _qrCodeTexture;

    [GetUniqueNode("ButtonContainer")]
    private BoxContainer _buttonContainer;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Play")]
    private Button _playButton;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Quit")]
    private Button _quitButton;

    [GetNode("UI/StartButton")]
    private Button _startButton;

    private float _turnSpeed = 200;

    [GetNode("UI/MarginContainer/HBoxContainer/VSeparator")]
    private VSeparator _vSeparator;

    public override void _Ready()
    {
        this.GetNodes();

        _playButton.Pressed += ToggleLobby;
        _quitButton.Pressed += OnQuitButtonPressed;
        _startButton.Pressed += OnStartButtonPressed;

        _lobby.PlayerJoinedListeners += _ => CallDeferred(nameof(PrintLobbyPlayers));
        _lobby.PlayerJoinedListeners += _ => CallDeferred(nameof(PrintJoinKeybindingLabel));
        _lobby.PlayerLeftListeners += _ => CallDeferred(nameof(PrintLobbyPlayers));
        _lobby.PlayerLeftListeners += _ => CallDeferred(nameof(PrintJoinKeybindingLabel));
        _lobby.PlayerInfoUpdatedListeners += HandleUpdateWebPlayerColor;

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

    public override void _PhysicsProcess(double delta)
    {
        _lobbyPlayerDict.ForEach(
            player =>
            {
                var character = player.Value.GetNode<TextureRect>("PlayerCharacter");
                if (player.Key.InputSource.IsTurningRight())
                {
                    character.RotationDegrees += _turnSpeed * (float)delta;
                }

                if (player.Key.InputSource.IsTurningLeft())
                {
                    character.RotationDegrees -= _turnSpeed * (float)delta;
                }
            }
        );
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

    private void ToggleLobby()
    {
        if (_lobby.Open)
        {
            CloseLobby();
        }
        else
        {
            OpenLobby();
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
        _instructions.Visible = true;

        _playButton.MouseEntered += ExpandButtons;
        _playButton.MouseExited += CollapseButtons;
        _quitButton.MouseEntered += ExpandButtons;
        _quitButton.MouseExited += CollapseButtons;

        PrintJoinKeybindingLabel();
        PrintLobbyPlayers();

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
        _instructions.Visible = false;

        _playButton.MouseEntered -= ExpandButtons;
        _playButton.MouseExited -= CollapseButtons;
        _quitButton.MouseEntered -= ExpandButtons;
        _quitButton.MouseExited -= CollapseButtons;

        PrintLobbyPlayers();

        ExpandButtons();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnStartButtonPressed()
    {
        if (_lobby.Count == 0)
        {
            _animationPlayer.Stop();
            _animationPlayer.Play("NoPlayers");
            return;
        }

        GameConfig.Lobby = _lobby;
        GameConfig.MultiplayerManager = _multiplayerManager;
        GetTree().ChangeSceneToFile(SceneGetter.GetPath<SplashScreen>());
    }

    private void ExpandButtons()
    {
        _playButton.Text = "PLAY";
        _playButton.CustomMinimumSize = new Vector2(200, 66);

        _quitButton.Text = "QUIT";
        _quitButton.CustomMinimumSize = new Vector2(200, 66);
    }

    private void CollapseButtons()
    {
        if (_buttonContainer.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
        {
            return;
        }

        _playButton.Text = "";
        _playButton.CustomMinimumSize = new Vector2(66, 66);

        _quitButton.Text = "";
        _quitButton.CustomMinimumSize = new Vector2(66, 66);
    }
    private void PrintLobbyPlayers()
    {
        foreach (var child in _playerList.GetChildren())
        {
            _playerList.RemoveChild(child);
            child.QueueFree();
        }

        _lobbyPlayerDict = new Dictionary<PlayerInfo, Control>();

        if (_lobby.Count == 0)
        {
            _emptyLobbyLabel.Visible = true;
            return;
        }

        foreach (var playerInfo in _lobby.PlayerInfos)
        {
            _emptyLobbyLabel.Visible = false;
            var lobbyPlayer = _lobbyPlayer.Instantiate<Control>();
            lobbyPlayer.Modulate = playerInfo.Color;

            if (playerInfo.InputSource.LeftInputIcon() != null && playerInfo.InputSource.RightInputIcon() != null)
            {
                lobbyPlayer.GetNode<HBoxContainer>("HBoxContainer/LeftBinding").AddChild(playerInfo.InputSource.LeftInputIcon());
                lobbyPlayer.GetNode<HBoxContainer>("HBoxContainer/RightBinding").AddChild(playerInfo.InputSource.RightInputIcon());
            }
            else
            {
                lobbyPlayer.GetNode<Label>("HBoxContainer/LeftBinding/Label").Text = $"\u2b05 {playerInfo.InputSource.LeftInputString()}";
                lobbyPlayer.GetNode<Label>("HBoxContainer/RightBinding/Label").Text = $"{playerInfo.InputSource.RightInputString()} \u2b95";
            }
            _playerList.AddChild(lobbyPlayer);
            _lobbyPlayerDict.Add(playerInfo, lobbyPlayer);
        }
    }

    private void PrintJoinKeybindingLabel()
    {
        foreach (var child in _joinKeybindingContainer.GetChildren())
        {
            _joinKeybindingContainer.RemoveChild(child);
            child.QueueFree();
        }

        foreach (var keybindingTuple in KeyboardBindings.Keybindings)
        {
            var kb = new KeyboardInputSource(keybindingTuple);
            if (!_lobby.PlayerInfos.Any(playerInfo =>
                    playerInfo.InputSource.Equals(kb)))
            {
                _joinKeybindingContainer.AddChild(kb.LeftInputIcon());
                _joinKeybindingContainer.AddChild(new Label() { Text = " + " });
                _joinKeybindingContainer.AddChild(kb.RightInputIcon());
                break;
            }
        }

        if (_joinKeybindingContainer.GetChildCount() == 0)
        {
            _joinKeybindingContainer.AddChild(new Label() { Text = "-"} );
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

            if (_pressingSince.TryGetValue(keybindingTuple, out var value) && value.Item2)
            {
                continue;
            }

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

                if (!(DateTime.Now - _pressingSince[keybindingTuple].Item1 >= TimeSpan.FromSeconds(0.5)))
                {
                    continue;
                }

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
        _lobbyCodeLabel.Text = $"[center][b]{newCode}";
    }

    private void SetLobbyQrCodeTexture(Texture2D newTexture)
    {
        _qrCodeTexture.Texture = newTexture;
    }

    private void HandleUpdateWebPlayerColor(PlayerInfo playerInfo)
    {
        if (playerInfo.InputSource is WebInputSource source)
            _multiplayerManager.NotifyChangePlayerColor(source.Id, ColorToHex(playerInfo.Color));
    }

    private static string ColorToHex(Color color)
    {
        var r = (int)(color.R * 255.0f);
        var g = (int)(color.G * 255.0f);
        var b = (int)(color.B * 255.0f);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
