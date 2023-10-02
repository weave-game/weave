using System.Collections.Generic;
using System.Linq;
using Godot;
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
            var isPressingBoth = Input.IsKeyPressed(keybindingTuple.Item1) && Input.IsKeyPressed(keybindingTuple.Item2);

            if (isPressingBoth)
            {
                GD.Print("both!");
            }

            if (!isPressingBoth) continue;

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
            GD.Print($"Device ID: {controller.DeviceId}. Type: {controller.Type}");

        GD.Print("");
    }
}