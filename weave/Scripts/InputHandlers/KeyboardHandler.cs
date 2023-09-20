using Godot;

namespace weave.InputHandlers;
public class KeyboardController : IController
{
    public bool IsTurningLeft()
    {
        return Input.IsKeyPressed(Key.Left);
    }

    public bool IsTurningRight()
    {
        return Input.IsKeyPressed(Key.Right);
    }
}