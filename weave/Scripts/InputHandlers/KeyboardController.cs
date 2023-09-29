using Godot;

namespace weave.InputHandlers;

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
}
