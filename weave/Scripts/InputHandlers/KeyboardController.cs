using System.Collections.Generic;
using Godot;

namespace weave.InputHandlers;

public sealed class KeyboardController : IController
{
    private readonly Key _left;
    private readonly Key _right;
    public static readonly IReadOnlyList<(Key, Key)> Keybindings = new List<(Key, Key)>
    {
        (Key.Left, Key.Right),
        (Key.Key1, Key.Q),
        (Key.B, Key.N),
        (Key.Z, Key.X)
    };

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
