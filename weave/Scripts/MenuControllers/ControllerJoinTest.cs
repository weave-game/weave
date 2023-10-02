using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Godot;
using GodotSharper.AutoGetNode;
using weave.Controller;
using weave.Utils;

namespace weave.MenuControllers;

// NOTE: This will be moved to the original StartScreen once the changes are added
public partial class ControllerJoinTest : Control
{
    private readonly IList<IController> _connected = new List<IController>();

    [GetNode("Button")]
    private Button _button;

    [GetNode("TextEdit")]
    private TextEdit _textEdit;

    public override void _Ready()
    {
        this.GetNodes();
        _button.Pressed += () =>
        {
            GameState.Controllers = _connected;
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
                KeyPressed();
                break;
        }
    }

    #region Keyboard

    private void KeyPressed()
    {
        foreach (var keybindingTuple in KeyboardBindings.Keybindings)
        {
            var isPressingBoth =
                Input.IsKeyPressed(keybindingTuple.Item1)
                && Input.IsKeyPressed(keybindingTuple.Item2);
            if (!isPressingBoth)
                continue;

            var kb = new KeyboardController(keybindingTuple);
            var alreadyExisting = _connected.FirstOrDefault(c => c.Equals(kb));

            if (alreadyExisting != null)
            {
                _connected.Remove(alreadyExisting);
            }
            else
            {
                _connected.Add(kb);
            }

            PrintControllers();
        }
    }

    #endregion


    /// <summary>
    ///     IMPORTANT: This is a hack, only used for debugging purposes
    /// </summary>
    private void PrintControllers()
    {
        // NO NEED TO REVIEW THIS; WILL BE REMOVED
        var sb = new StringBuilder();

        var i = 1;
        foreach (var controller in _connected)
        {
            if (controller is KeyboardController k)
            {
                var left = typeof(KeyboardController)
                    .GetField("_left", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(k);
                var right = typeof(KeyboardController)
                    .GetField("_right", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(k);

                sb.AppendLine($"({i++}) Device ID: {controller.DeviceId}. Type: {controller.Type}");
                sb.AppendLine($"Left: {left}. Right: {right}\n");
            }
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
            AddGamepad(deviceId);

        if (@event.IsActionPressed(ActionConstants.GamepadLeaveAction))
            RemoveGamepad(deviceId);
    }

    private void AddGamepad(int deviceId)
    {
        if (_connected.Any(c => c.DeviceId == deviceId))
            return;

        _connected.Add(new GamepadController(deviceId));
        PrintControllers();
    }

    private void RemoveGamepad(int deviceId)
    {
        var toRemove = _connected.FirstOrDefault(c => c.DeviceId == deviceId);
        if (toRemove == null)
            return;

        _connected.Remove(toRemove);
        PrintControllers();
    }

    #endregion
}
