using Godot;

namespace weave.InputSources;

public sealed class GamepadInputSource : IInputSource
{
    private const float DeadZone = 0.2f;
    private readonly int _deviceId;

    public GamepadInputSource(int deviceId)
    {
        _deviceId = deviceId;
    }

    int IInputSource.DeviceId => _deviceId;
    public InputType Type => InputType.Gamepad;

    bool IInputSource.IsTurningLeft()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.LeftX) < -DeadZone;
    }

    bool IInputSource.IsTurningRight()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.LeftX) > DeadZone;
    }

    public bool Equals(IInputSource other)
    {
        if (other is GamepadInputSource gamepad)
            return _deviceId == gamepad._deviceId;

        return false;
    }
}