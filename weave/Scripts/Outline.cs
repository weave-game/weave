using Godot;
using Weave.Utils;

namespace Weave;

public partial class Outline : Line2D
{
    public override void _Ready()
    {
        Width = WeaveConstants.LineWidth;

        var width = GetViewportRect().Size.X;
        var height = GetViewportRect().Size.Y;

        ClearPoints();
        AddPoint(new(-Width / 2, -Width / 2));
        AddPoint(new(width + (Width / 2), -Width / 2));
        AddPoint(new(width + (Width / 2), height + (Width / 2)));
        AddPoint(new(-Width / 2, height + (Width / 2)));
        AddPoint(GetPointPosition(0) + new Vector2(0, -Width / 2));
    }
}
