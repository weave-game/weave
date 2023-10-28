using Godot;
using Weave.Utils;

namespace Weave;

public partial class Outline : Line2D
{
    public override void _Ready()
    {
        Width = WeaveConstants.LineWidth;
        var halfWidth = Width / 2;

        var viewportWidth = GetViewportRect().Size.X;
        var viewportHeight = GetViewportRect().Size.Y;

        ClearPoints();
        AddPoint(new Vector2(-halfWidth, -halfWidth));
        AddPoint(new Vector2(viewportWidth + halfWidth, -halfWidth));
        AddPoint(new Vector2(viewportWidth + halfWidth, viewportHeight + halfWidth));
        AddPoint(new Vector2(-halfWidth, viewportHeight + halfWidth));
        AddPoint(GetPointPosition(0) + new Vector2(0, -halfWidth));
    }
}
