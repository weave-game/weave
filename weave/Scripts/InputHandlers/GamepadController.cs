using Godot;

namespace weave.InputHandlers;

public sealed class GamepadController : IController
{
    private const float DeadZone = 0.2f;
    private readonly int _deviceId;
    int IController.DeviceId => _deviceId;

    public GamepadController(int deviceId)
    {
        _deviceId = deviceId;
    }

    bool IController.IsTurningLeft()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.LeftX) < -DeadZone;
    }

    bool IController.IsTurningRight()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.LeftX) > DeadZone;
    }
}