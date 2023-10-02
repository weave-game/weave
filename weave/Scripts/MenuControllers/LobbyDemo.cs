using System.Collections.Generic;
using System.Linq;
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
    private readonly IList<IInputDevice> _connectedInputDevices = new List<IInputDevice>();

    [GetNode("Button")]
    private Button _button;

    [GetNode("TextEdit")]
    private TextEdit _textEdit;

    public override void _Ready()
    {
        this.GetNodes();
        _button.Pressed += () =>
        {
            GameConfig.InputDevices = _connectedInputDevices;
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

            var kb = new KeyboardInputDevice(keybindingTuple);
            var alreadyExisting = _connectedInputDevices.FirstOrDefault(c => c.Equals(kb));

            if (alreadyExisting != null)
            {
                _connectedInputDevices.Remove(alreadyExisting);
            }
            else
            {
                _connectedInputDevices.Add(kb);
            }

            PrintInputDevices();
        }
    }

    #endregion


    /// <summary>
    ///     IMPORTANT: This is a hack, only used for debugging purposes
    /// </summary>
    private void PrintInputDevices()
    {
        // NO NEED TO REVIEW THIS; WILL BE REMOVED
        var sb = new StringBuilder();

        var i = 1;
        foreach (var inputDevice in _connectedInputDevices)
        {
            if (inputDevice is KeyboardInputDevice k)
            {
                var left = typeof(KeyboardInputDevice)
                    .GetField("_left", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(k);
                var right = typeof(KeyboardInputDevice)
                    .GetField("_right", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(k);

                sb.AppendLine(
                    $"({i++}) Device ID: {inputDevice.DeviceId}. Type: {inputDevice.Type}"
                );
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
        if (_connectedInputDevices.Any(c => c.DeviceId == deviceId))
            return;

        _connectedInputDevices.Add(new GamepadInputDevice(deviceId));
        PrintInputDevices();
    }

    private void RemoveGamepad(int deviceId)
    {
        var toRemove = _connectedInputDevices.FirstOrDefault(c => c.DeviceId == deviceId);
        if (toRemove == null)
            return;

        _connectedInputDevices.Remove(toRemove);
        PrintInputDevices();
    }

    #endregion
}
