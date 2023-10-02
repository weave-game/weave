using Godot;

namespace weave.Controller;

public sealed class KeyboardController : IController
{
    private readonly Key _left;
    private readonly Key _right;

    public KeyboardController((Key, Key) keybindings)
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

    public Controller Type => Controller.Keyboard;

    public bool Equals(IController other)
    {
        if (other is KeyboardController keyboardController)
            return keyboardController._left == _left && keyboardController._right == _right;

        return false;
    }
}
