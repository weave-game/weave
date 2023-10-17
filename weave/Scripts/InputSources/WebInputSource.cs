using System;

namespace Weave.InputSources;

public sealed class WebInputSource : IInputSource
{
    public WebInputSource(string id)
    {
        Id = id;
    }

    public string Id { get; set; }
    public string DirectionState { get; set; }

    public bool IsTurningLeft()
    {
        return string.Equals(DirectionState, "left", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsTurningRight()
    {
        return string.Equals(DirectionState, "right", StringComparison.OrdinalIgnoreCase);
    }

    public InputType Type => InputType.Web;

    public bool Equals(IInputSource other)
    {
        if (other is WebInputSource web)
        {
            return web.Id == Id;
        }

        return false;
    }

    public string LeftInputString()
    {
        return "mobile-left";
    }

    public string RightInputString()
    {
        return "mobile-right";
    }
}
