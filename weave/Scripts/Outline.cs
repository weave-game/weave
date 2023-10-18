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
        AddPoint(new(-WeaveConstants.LineWidth / 2, -WeaveConstants.LineWidth / 2));
        AddPoint(new(width + WeaveConstants.LineWidth / 2, -WeaveConstants.LineWidth / 2));
        AddPoint(new(width + WeaveConstants.LineWidth / 2, height + WeaveConstants.LineWidth / 2));
        AddPoint(new(-WeaveConstants.LineWidth / 2, height + WeaveConstants.LineWidth / 2));
        AddPoint(Points[0] + new Vector2(0, -WeaveConstants.LineWidth / 2));

    }

    public override void _Process(double delta)
    {
        
    }
}
