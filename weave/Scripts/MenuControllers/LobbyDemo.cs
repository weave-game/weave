using System.Reflection;
using System.Text;
using Godot;
using GodotSharper.AutoGetNode;
using weave.InputDevices;
using weave.Utils;

namespace weave.MenuControllers;

// NOTE: This will be moved to the original StartScreen once the changes are added
public partial class LobbyDemo : Control
{
    [GetNode("Button")]
    private Button _button;

    [GetNode("TextEdit")]
    private TextEdit _textEdit;

    private readonly Lobby _lobby = new();

    public override void _Ready()
    {
        this.GetNodes();
        _button.Pressed += () =>
        {
            GameConfig.Lobby = _lobby;
            GetTree().ChangeSceneToFile(SceneResources.MainScene);
        };
    }

    public override void _Process(double delta) => PrintInputDevices(); // <-- Dev

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventJoypadButton button:
                GamepadPressed(button);
                break;
            case InputEventKey { Pressed: true }:
                KeyPressed();
                break;
        }
    }

    private void KeyPressed()
    {
        foreach (var keybindingTuple in KeyboardBindings.Keybindings)
        {
            var isPressingBoth =
                Input.IsKeyPressed(keybindingTuple.Item1)
                && Input.IsKeyPressed(keybindingTuple.Item2);

            if (!isPressingBoth)
                continue;

            _lobby.ToggleKeyboard(keybindingTuple);
        }
    }

    private void GamepadPressed(InputEvent @event)
    {
        var deviceId = @event.Device;
        if (deviceId < 0)
            return;

        if (@event.IsActionPressed(ActionConstants.GamepadJoinAction))
        {
            _lobby.AddGamepad(deviceId);
        }

        if (@event.IsActionPressed(ActionConstants.GamepadLeaveAction))
        {
            _lobby.RemoveGamepad(deviceId);
        }
    }

    /// <summary>
    ///     IMPORTANT: This is a hack, only used for debugging purposes
    /// </summary>
    private void PrintInputDevices()
    {
        // NO NEED TO REVIEW THIS; WILL BE REMOVED
        var sb = new StringBuilder();

        var i = 1;
        foreach (var inputDevice in _lobby.ConnectedInputDevices)
        {
            sb.AppendLine($"({i++}) Device ID: {inputDevice.DeviceId}. Type: {inputDevice.Type}");

            if (inputDevice is KeyboardInputDevice k)
            {
                var left = typeof(KeyboardInputDevice)
                    .GetField("_left", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(k);

                var right = typeof(KeyboardInputDevice)
                    .GetField("_right", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(k);

                sb.AppendLine($"Left: {left}. Right: {right}");
            }

            sb.AppendLine("");
        }

        _textEdit.Text = sb.ToString();
    }
}
