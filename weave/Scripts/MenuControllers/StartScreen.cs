using System.Linq;
using System.Reflection;
using System.Text;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.Utils;
using Weave.InputSources;
using Weave.Multiplayer;

namespace Weave.MenuControllers;

[Scene("res://Menus/StartScreen.tscn")]
public partial class StartScreen : Control
{
    private readonly Lobby _lobby = new();
    private Manager _multiplayerManager;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Play")]
    private Button _playButton;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Options")]
    private Button _optionsButton;

    [GetNode("UI/MarginContainer/HBoxContainer/ButtonContainer/Quit")]
    private Button _quitButton;

    [GetNode("BlurLayer")]
    private CanvasLayer _blurLayer;

    [GetNode("UI/MarginContainer/HBoxContainer/PlayerList")]
    private VBoxContainer _playerList;

    [GetNode("UI/MarginContainer/HBoxContainer/PlayerList/TextEdit")]
    private TextEdit _textEdit;

    [GetNode("UI/MarginContainer/HBoxContainer/Start")]
    private Button _startButton;

    public override void _Ready()
    {
        this.GetNodes();
        _playButton.Pressed += OnPlayButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;
        _startButton.Pressed += OnStartButtonPressed;

        _multiplayerManager = new(_lobby.LobbyCode);
        Manager.StartClientAsync();

        var colorGen = new UniqueColorGenerator();
        GetTree()
            .GetNodesInGroup(GodotConfig.FireflyGroup)
            .Cast<Firefly>()
            .ForEach(f => f.SetColor(colorGen.NewColor()));
    }

    public override void _Process(double delta)
    {
        PrintInputSources();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
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

    private void OnPlayButtonPressed()
    {
        _blurLayer.Visible = true;
        _playerList.Visible = true;
        _startButton.Visible = true;
        CollapseButtons();
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
        _playButton.Text = "START";
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

    /// <summary>
    ///     IMPORTANT: This is a hack, only used for debugging purposes
    /// </summary>
    private void PrintInputSources()
    {
        // NO NEED TO REVIEW THIS; WILL BE REMOVED
        var sb = new StringBuilder();

        var i = 1;
        foreach (var inputSource in _lobby.InputSources)
        {
            sb.Append("Player ").Append(i++).AppendLine(": ");

            if (inputSource is KeyboardInputSource k)
            {
                // ReSharper disable once PossibleNullReferenceException
                var left = typeof(KeyboardInputSource)
                    .GetField("_left", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(k);

                // ReSharper disable once PossibleNullReferenceException
                var right = typeof(KeyboardInputSource)
                    .GetField("_right", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(k);

                sb.Append("L: ").Append(left).Append(" R: ").Append(right).AppendLine("");
            }

            sb.AppendLine("");
        }

        _textEdit.Text = sb.ToString();
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
            var alreadyExisting = _lobby.InputSources.FirstOrDefault(c => c.Equals(kb));

            if (alreadyExisting != null)
                _lobby.Leave(alreadyExisting);
            else
                _lobby.Join(kb);
        }
    }

    #endregion Keyboard

    #region Gamepad

    private void GamepadPressed(InputEvent @event)
    {
        var deviceId = @event.Device;
        if (deviceId < 0)
            return;

        if (@event.IsActionPressed(GodotConfig.GamepadJoinAction))
            _lobby.Join(new GamepadInputSource(deviceId));

        if (@event.IsActionPressed(GodotConfig.GamepadLeaveAction))
            _lobby.Leave(new GamepadInputSource(deviceId));
    }

    #endregion Gamepad
}
