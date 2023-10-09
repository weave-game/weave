using System;
using System.Linq;
using Godot;
using GodotSharper.AutoGetNode;
using weave.Utils;

namespace weave;

public partial class Firefly : Path2D
{
    private const float MaxSpeed = 12;
    private const float MinSpeed = 5;
    private const int NrPoints = 30;
    private const float DistanceBetweenPoints = 5;
    private Godot.Timer _animationTimer;

    [GetNode("PathFollow2D/Area2D")]
    private Area2D _area;

    private float _currentSpeed;
    private bool _firstIteration = true;
    private float _goalSpeed;
    private bool _isWaiting;
    private float _lastProgress;

    [GetNode("Line2D")]
    private Line2D _line;

    [GetNode("PathFollow2D")]
    private PathFollow2D _pathFollow;

    public override void _Ready()
    {
        this.GetNodes();

        Visible = true;
        _line.Width = Constants.MenuLineWidth;

        var animationDelay = (GD.Randf() * 10) + 3;
        _animationTimer = new Godot.Timer { WaitTime = animationDelay, OneShot = true };
        _animationTimer.Timeout += HandleTimerTimeout;
        AddChild(_animationTimer);

        for (var i = 0; i < NrPoints; i++)
            _line.AddPoint(new Vector2());
    }

    public void SetColor(Color color)
    {
        _line.DefaultColor = color;
    }

    public override void _Process(double delta)
    {
        if (_isWaiting)
            return;

        // Reset line points & pause animation when progress is done or on first iteration
        if (_lastProgress > _pathFollow.Progress || _firstIteration)
        {
            _line.Points = Enumerable.Repeat(_area.GlobalPosition, NrPoints).ToArray();
            _animationTimer.Start();
            _isWaiting = true;
            _firstIteration = false;
        }

        // Reached goal speed, set new speed
        if (MathF.Abs(_currentSpeed - _goalSpeed) < (float)10e-5)
            _goalSpeed = (GD.Randf() * MaxSpeed) + MinSpeed;

        // Make line follow the leading point
        if (_line.Points[0].DistanceTo(_area.GlobalPosition) >= DistanceBetweenPoints)
        {
            var lastPoint = _area.GlobalPosition;
            var tempPoints = (Vector2[])_line.Points.Clone();

            for (var i = 0; i < NrPoints; i++)
            {
                tempPoints[i] = lastPoint;
                lastPoint = _line.Points[Math.Max(i - 1, 0)]; // Avoid index out of bounds
            }

            _line.Points = tempPoints;
        }

        _lastProgress = _pathFollow.Progress;
        _currentSpeed = Mathf.Lerp(_currentSpeed, _goalSpeed, 0.3f);
        _pathFollow.Progress += _currentSpeed;
    }

    private void HandleTimerTimeout()
    {
        _isWaiting = false;
        _animationTimer.Stop();
    }
}
