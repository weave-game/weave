using System;
using System.Collections.Generic;
using Godot;
using GodotSharper.Instancing;
using weave;

[Instantiable("res://Objects/CurveSpawner.tscn")]
public partial class CurveSpawner : Node2D
{
    [Signal]
    public delegate void CreatedLineEventHandler(Line2D line);
    public bool IsDrawing { get; set; } = true;
    public ISet<SegmentShape2D> Segments { get; set; } = new HashSet<SegmentShape2D>();
    private Vector2 _lastPoint;
    private bool _hasStarted;
    private Timer _drawTimer;
    private Timer _gapTimer;
    private readonly float _timeBetweenGaps = 5;
    private readonly float _timeForGaps = 0.5f;
    private Color _lineColor = new(1, 0, 0);
    private readonly float _curveSpawnOffset = 4f;

    public override void _Ready()
    {
        InitializeTimers();
    }

    public void Step(CircleShape2D playerShape, float playerRotation, Vector2 playerPosition)
    {
            var angleBehind = playerRotation + (float)(Math.PI / 2);
            var pointBehind = CalculatePointOnCircle(
                playerPosition,
                playerShape.Radius + _curveSpawnOffset,
                angleBehind
            );

            // Don't draw line on first iteration (first line will otherwise originate from (0,0))
            if (_hasStarted && IsDrawing)
            {
                DrawLine(_lastPoint, pointBehind);
            }
            else
            {
                _hasStarted = true;
            }
            _lastPoint = pointBehind;
    }

    private void InitializeTimers()
    {
        _drawTimer = new Timer { WaitTime = _timeBetweenGaps, OneShot = true };
        _drawTimer.Timeout += HandleDrawTimerTimeout;
        AddChild(_drawTimer);

        _gapTimer = new Timer { WaitTime = _timeForGaps, OneShot = true };
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
        EmitSignal(SignalName.CreatedLine, line);

        // Create segment that is used to check for intersections
        Segments.Add(new SegmentShape2D { A = from, B = to });
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
