using Godot;

namespace Weave.InputSources;

public sealed class GamepadInputSource : IInputSource
{
    private const float DeadZone = 0.2f;
    private readonly int _deviceId;

    public GamepadInputSource(int deviceId)
    {
        _deviceId = deviceId;
    }

    public InputType Type => InputType.Gamepad;

    bool IInputSource.IsTurningLeft()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.TriggerLeft) > DeadZone;
    }

    bool IInputSource.IsTurningRight()
    {
        return Input.GetJoyAxis(_deviceId, JoyAxis.TriggerRight) > DeadZone;
    }

    string IInputSource.LeftInputString()
    {
        return "L2/LT";
    }

    string IInputSource.RightInputString()
    {
        return "R2/RT";
    }

    public bool Equals(IInputSource other)
    {
        if (other is GamepadInputSource gamepad)
        {
            return _deviceId == gamepad._deviceId;
        }

        return false;
    }
}
