using Godot;

namespace weave.InputSources;

public sealed partial class WebInputSource : Node, IInputSource
{
    public string Id;
    public string DirectionState;

    public WebInputSource(string id)
    {
        Id = id;
    }

    public bool IsTurningLeft()
    {
        return string.Equals(DirectionState, "left", System.StringComparison.OrdinalIgnoreCase);
    }

    public bool IsTurningRight()
    {
        return string.Equals(DirectionState, "right", System.StringComparison.OrdinalIgnoreCase);
    }

    public InputType Type => InputType.Web;

    public bool Equals(IInputSource other)
    {
        if (other is WebInputSource web)
            return web.Id == Id;

        return false;
    }
}
