using Godot;

namespace weave.InputSources;

public sealed class KeyboardInputSource : IInputSource
{
    private readonly Key _left;
    private readonly Key _right;

    public KeyboardInputSource((Key, Key) keybindings)
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

    public bool Equals(IInputSource other)
    {
        if (other is KeyboardInputSource keyboard)
            return keyboard._left == _left && keyboard._right == _right;

        return false;
    }
}