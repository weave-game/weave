using Godot;
using Weave.Utils;

namespace Weave;

public partial class KeyCatcher : Node
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed(WeaveConstants.ToggleFullscreenAction))
            return;

        var isFullScreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        DisplayServer.WindowSetMode(
            isFullScreen ? DisplayServer.WindowMode.Windowed : DisplayServer.WindowMode.Fullscreen
        );
    }
}
