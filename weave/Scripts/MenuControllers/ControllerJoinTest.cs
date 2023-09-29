using System.Collections.Generic;
using Godot;
using GodotSharper.AutoGetNode;

namespace weave.MenuControllers;

public partial class ControllerJoinTest : Control
{
    private readonly ISet<int> _deviceIds = new HashSet<int>();

    public override void _Ready()
    {
        this.GetNodes();
    }

    public override void _Process(double delta)
    {
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventJoypadButton button:
                GamepadPressed(button.Device);
                break;
            case InputEventJoypadMotion motion:
                GamepadPressed(motion.Device);
                break;
        }
    }

    private void GamepadPressed(int deviceId)
    {
        if (deviceId < 0)
            return;

        if (_deviceIds.Contains(deviceId))
            return;

        _deviceIds.Add(deviceId);
        GD.Print("Gamepad joined: " + deviceId);
    }
}