using Godot;

namespace weave.InputDevices;

public sealed class KeyboardInputDevice : IInputDevice
{
    private readonly Key _left;
    private readonly Key _right;

    public KeyboardInputDevice((Key, Key) keybindings)
    {
        _left = keybindings.Item1;
        _right = keybindings.Item2;
    }

    public bool IsTurningLeft()
    {
        return Input.IsKeyPressed(_left);
    }

    public bool IsTurningRight()
    {
        return Input.IsKeyPressed(_right);
    }

    public InputType Type => InputType.Keyboard;

    public bool Equals(IInputDevice other)
    {
        if (other is KeyboardInputDevice keyboardInputDevice)
            return keyboardInputDevice._left == _left && keyboardInputDevice._right == _right;

        return false;
    }
}
