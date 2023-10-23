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

    public TextureRect LeftInputIcon()
    {
        var imageResource = GD.Load<CompressedTexture2D>("res://Assets/Icons/xbox_lt.png");
        var icon = new TextureRect() { Texture = imageResource };
        icon.CustomMinimumSize = new Vector2(38, 38);
        icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        return icon;
    }

    public TextureRect RightInputIcon()
    {
        var imageResource = GD.Load<CompressedTexture2D>("res://Assets/Icons/xbox_rt.png");
        var icon = new TextureRect() { Texture = imageResource };
        icon.CustomMinimumSize = new Vector2(38, 38);
        icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        return icon;
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
