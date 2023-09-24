using System;
using System.Collections.Generic;
using Godot;
using GodotSharper.Instancing;
using weave.Utils;

namespace weave;

[Instantiable(ObjectResources.CurveSpawnerScene)]
public partial class CurveSpawner : Node2D
{
    private const float CurveSpawnOffset = 4f;
    private const float TimeBetweenGaps = 5;
    private const float TimeForGaps = 0.5f;
    private Timer _drawTimer;
    private Timer _gapTimer;
    private bool _hasStarted;
    private bool _isDrawing = true;
    private Vector2 _lastPoint;
    private Color _lineColor = new(1, 0, 0);
    public Player Player { get; set; }
    public ISet<SegmentShape2D> Segments { get; } = new HashSet<SegmentShape2D>();
    public int LineWidth { get; set; }

    public override void _Ready()
    {
        InitializeTimers();
    }

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

        // Don't draw line on first iteration (first line will otherwise originate from (0,0))
        if (_hasStarted && _isDrawing)
            DrawLine(_lastPoint, pointBehind);
        else
            _hasStarted = true;
        _lastPoint = pointBehind;
    }

    private void InitializeTimers()
    {
        _drawTimer = new Timer { WaitTime = TimeBetweenGaps, OneShot = true };
        _drawTimer.Timeout += HandleDrawTimerTimeout;
        AddChild(_drawTimer);

        _gapTimer = new Timer { WaitTime = TimeForGaps, OneShot = true };
        _gapTimer.Timeout += HandleGapTimerTimeout;
        AddChild(_gapTimer);

        _drawTimer.Start();
    }

    private void HandleDrawTimerTimeout()
    {
        _isDrawing = false;
        _drawTimer.Stop();
        _gapTimer.Start();
    }

    private void HandleGapTimerTimeout()
    {
        _isDrawing = true;
        _gapTimer.Stop();
        _drawTimer.Start();
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
