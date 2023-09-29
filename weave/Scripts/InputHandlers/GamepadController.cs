using Godot;

namespace weave.InputHandlers;

public sealed class GamepadController : IController
{
    private const float DeadZone = 0.2f;
    private readonly int _deviceId;

    public GamepadController(int deviceId)
    {
        _deviceId = deviceId;
    }

    public bool IsTurningLeft()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.LeftX) < -DeadZone;
    }

    public bool IsTurningRight()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.LeftX) > DeadZone;
    }
}