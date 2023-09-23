using System;
using System.Collections.Generic;
using Godot;
using GodotSharper.Instancing;

namespace weave;

[Instantiable("res://Objects/CurveSpawner.tscn")]
public partial class CurveSpawner : Node2D
{
    private const float CurveSpawnOffset = 4f;
    private bool _hasStarted;
    private Vector2 _lastPoint;
    private Color _lineColor = new(1, 0, 0);
    public Player Player { get; set; }
    public ISet<SegmentShape2D> Segments { get; } = new HashSet<SegmentShape2D>();
    public int LineWidth { get; set; }

    public override void _Process(double delta)
    {
        if (Player == null)
            return;

        var playerShape = (CircleShape2D)Player.CollisionShape2D.Shape;
        var angleBehind = Player.Rotation + (float)(Math.PI / 2);
        var pointBehind = CalculatePointOnCircle(
            Player.GlobalPosition,
            playerShape.Radius + CurveSpawnOffset,
            angleBehind
        );

        if (_hasStarted) // Don't draw line on first iteration (will otherwise originate from (0,0))
            DrawLine(_lastPoint, pointBehind);
        else
            _hasStarted = true;
        _lastPoint = pointBehind;
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        // Line that is drawn to screen
        var line = new Line2D { DefaultColor = _lineColor, Width = LineWidth };
        line.AddPoint(from);
        line.AddPoint(to);
        AddChild(line);

        // Create segment that is used to check for intersections
        Segments.Add(new SegmentShape2D { A = from, B = to });
    }

    private static Vector2 CalculatePointOnCircle(
        Vector2 center,
        float radius,
        float angleInRadians
    )
    {
        var x = center.X + radius * Mathf.Cos(angleInRadians);
        var y = center.Y + radius * Mathf.Sin(angleInRadians);
        return new Vector2(x, y);
    }
}
