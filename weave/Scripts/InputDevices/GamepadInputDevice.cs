using Godot;

namespace weave.InputDevices;

public sealed class GamepadInputDevice : IInputDevice
{
    private const float DeadZone = 0.2f;
    private readonly int _deviceId;

    public GamepadInputDevice(int deviceId)
    {
        _deviceId = deviceId;
    }

    int IInputDevice.DeviceId => _deviceId;
    public InputType Type => InputType.Gamepad;

    bool IInputDevice.IsTurningLeft()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.LeftX) < -DeadZone;
    }

    bool IInputDevice.IsTurningRight()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.LeftX) > DeadZone;
    }

    public bool Equals(IInputDevice other)
    {
        if (other is GamepadInputDevice gamepadInputDevice)
            return _deviceId == gamepadInputDevice._deviceId;

        return false;
    }
}
