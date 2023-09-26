using Godot;
using System;
using GodotSharper.AutoGetNode;
using weave.Utils;

namespace weave;

public partial class Firefly : Path2D
{
    [GetNode("PathFollow2D")]
    private PathFollow2D pathFollow;

    [GetNode("PathFollow2D/Area2D")]
    private Area2D area;

    [GetNode("Line2D")]
    private Line2D line;

    private float _currentSpeed;
    private float _goalSpeed;
    private const float MaxSpeed = 15;
    private const int NrPoints = 30;
    private float _distanceBetweenPoints = 5;

    public override void _Ready()
    {
        this.GetNodes();

        line.Width = 4;
        line.DefaultColor = Unique.NewColor();

        for (var i = 0; i < NrPoints; i++)
        {
            line.AddPoint(new Vector2(Position.X, Position.Y));
        }
    }

    public override void _Process(double delta)
    {
        if (MathF.Abs(_currentSpeed - _goalSpeed) < (float)10e-5)
        {
            _goalSpeed = GD.Randf() * 15;
        }

        if (line.Points[0].DistanceTo(area.GlobalPosition) >= _distanceBetweenPoints)
        {
            var lastPoint = area.GlobalPosition;
            var tempPoints = (Vector2[])line.Points.Clone();

            for (var i = 0; i < NrPoints; i++)
            {
                tempPoints[i].X = lastPoint.X;
                tempPoints[i].Y = lastPoint.Y;
                if (i > 0)
                    lastPoint = line.Points[i - 1];
            }

            line.Points = tempPoints;
        }

        _currentSpeed = Mathf.Lerp(_currentSpeed, _goalSpeed, 0.3f);
        pathFollow.Progress += _currentSpeed;
    }
}
