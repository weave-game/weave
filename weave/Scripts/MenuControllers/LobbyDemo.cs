using System.Linq;
using System.Reflection;
using System.Text;
using Godot;
using GodotSharper.AutoGetNode;
using weave.InputSources;
using weave.Utils;

namespace weave.MenuControllers;

// NOTE: This will be moved to the original StartScreen once the changes are added
public partial class LobbyDemo : Control
{
    private readonly Lobby _lobby = new();

    [GetNode("Button")]
    private Button _button;

    [GetNode("TextEdit")]
    private TextEdit _textEdit;

    [GetNode("LobbyCodeLabel")]
    private RichTextLabel _lobbyCodeLabel;

    [GetNode("QRCodeTexture")]
    private TextureRect _qrCodeTexture;

    public override void _Ready()
    {
        this.GetNodes();

        SetLobbyCodeLabelText(_lobby.LobbyCode);
        SetLobbyQRCodeTexture(_lobby.LobbyQRCode);

        _button.Pressed += () =>
        {
            GameConfig.Lobby = _lobby;
            GetTree().ChangeSceneToFile(SceneResources.MainScene);
        };
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

    public override void _Process(double delta)
    {
        PrintInputSources();
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

    #endregion

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
            sb.AppendLine($"({i++}) Device ID: {inputSource.DeviceId}. Type: {inputSource.Type}");

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

                sb.AppendLine($"Left: {left}. Right: {right}");
            }

            sb.AppendLine("");
        }

        _textEdit.Text = sb.ToString();
    }

    #region Gamepad

    private void GamepadPressed(InputEvent @event)
    {
        var deviceId = @event.Device;
        if (deviceId < 0)
            return;

        if (@event.IsActionPressed(ActionConstants.GamepadJoinAction))
            _lobby.Join(new GamepadInputSource(deviceId));

        if (@event.IsActionPressed(ActionConstants.GamepadLeaveAction))
            _lobby.Leave(new GamepadInputSource(deviceId));
    }

    #endregion

    private void SetLobbyCodeLabelText(string newCode)
    {
        _lobbyCodeLabel.Text = $"[center]Lobby code: {newCode}[/center]";
    }

    private void SetLobbyQRCodeTexture(ImageTexture newTexture)
    {
        _qrCodeTexture.Texture = newTexture;
    }
}
