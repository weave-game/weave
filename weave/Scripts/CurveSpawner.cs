using System;
using Godot;
using GodotSharper.Instancing;
using System.Collections.Generic;
using weave.Utils;

namespace weave;

[Instantiable(ObjectResources.CurveSpawnerScene)]
public partial class CurveSpawner : Node2D
{
    [Signal]
    public delegate void CreatedLineEventHandler(Line2D line, SegmentShape2D segment);
    public bool IsDrawing { get; set; } = true;
    private const float CurveSpawnOffset = 4f;
    private const float TimeBetweenGaps = 5;
    private const float TimeForGaps = 0.5f;
    private Timer _drawTimer;
    private Timer _gapTimer;
    private bool _hasStarted;
    private Vector2 _lastPoint;
    private Color _lineColor = new(1, 0, 0);
    public ISet<SegmentShape2D> Segments { get; } = new HashSet<SegmentShape2D>();

    public override void _Ready()
    {
        InitializeTimers();
    }

    public void Step(Vector2 playerPosition, float playerRotation, float playerRadius)
    {
        var angleBehind = playerRotation + (float)(Math.PI / 2);
        var pointBehind = CalculatePointOnCircle(
            playerPosition,
            playerRadius + CurveSpawnOffset,
            angleBehind
        );

        // Don't draw line on first iteration (first line will otherwise originate from (0,0))
        if (_hasStarted && IsDrawing)
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
        IsDrawing = false;
        _drawTimer.Stop();
        _gapTimer.Start();
    }

    private void HandleGapTimerTimeout()
    {
        IsDrawing = true;
        _gapTimer.Stop();
        _drawTimer.Start();
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        // Line that is drawn to screen
        var line = new Line2D { DefaultColor = _lineColor, Width = Constants.LineWidth, };
        line.AddPoint(from);
        line.AddPoint(to);

        // Segment that is used to test for intersections
        var segment = new SegmentShape2D { A = from, B = to };

        // Pass information to Main
        EmitSignal(SignalName.CreatedLine, line, segment);
    }

    private static Vector2 CalculatePointOnCircle(
        Vector2 center,
        float radius,
        float angleInRadians
    )
    {
        float x = center.X + (radius * Mathf.Cos(angleInRadians));
        float y = center.Y + (radius * Mathf.Sin(angleInRadians));
        return new Vector2(x, y);
    }
}
