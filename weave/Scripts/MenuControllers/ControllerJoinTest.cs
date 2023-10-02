using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSharper;
using GodotSharper.AutoGetNode;
using weave.Controller;
using weave.Utils;

namespace weave.MenuControllers;

public partial class ControllerJoinTest : Control
{
    private readonly ISet<IController> _connected = new HashSet<IController>();

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventJoypadButton button)
            GamepadPressed(button);

        if (@event is InputEventKey { Pressed: true } key)
            KeyPressed(key);
    }

    private static void KeyPressed(InputEventKey key)
    {
        GD.Print(key);

        var keyboardPlayerIndex = 0;
        foreach (var keybindingTuple in KeyboardBindings.Keybindings)
        {
            if (key.Keycode == keybindingTuple.Item1 || key.Keycode == keybindingTuple.Item2)
            {
                GD.Print("Keyboard player " + keyboardPlayerIndex);
                break;
            }

            keyboardPlayerIndex++;
        }
    }

    private void GamepadPressed(InputEvent @event)
    {
        var deviceId = @event.Device;
        if (deviceId < 0)
            return;

        var joined = @event.IsActionPressed(ActionConstants.GamepadJoinAction);
        var left = @event.IsActionPressed(ActionConstants.GamepadLeaveAction);

        if (joined)
        {
            if (IsConnected(deviceId))
                return;

            _connected.Add(new GamepadController(deviceId));
            PrintControllers();
        }

        if (left)
        {
            var toRemove = _connected.FirstOrDefault(c => c.DeviceId == deviceId);
            if (toRemove == null)
                return;

            _connected.Remove(toRemove);

            PrintControllers();
        }
    }

    private bool IsConnected(int deviceId)
    {
        return _connected.Any(c => c.DeviceId == deviceId);
    }

    private void PrintControllers()
    {
        GD.Print("Controllers:");

        foreach (var controller in _connected)
            GD.Print($"{controller}. Device ID: {controller.DeviceId}");

        GD.Print("");
    }
}
