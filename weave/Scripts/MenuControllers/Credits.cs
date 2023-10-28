using Godot;

namespace Weave.MenuControllers;

public partial class Credits : CanvasLayer
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
            return;

        Hide();
    }
}
