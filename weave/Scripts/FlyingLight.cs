using System;
using Godot;

namespace weave;

public partial class FlyingLight : Area2D
{
    private PathFollow2D pathFollow;
    private float Speed;
    private float GoalSpeed;
    private float lastSpeed;
    private float goalSpeed;
    private const int NrPoints = 10;
    private Vector2[] points;
    private float distBetweenPoints = 1;

    public override void _Ready()
    {
        pathFollow = GetParent<PathFollow2D>();

        points = new Vector2[NrPoints];
        for (var i = 0; i < NrPoints; i++)
        {
            points[i] = new Vector2(Position.X, Position.Y);
        }
    }

    public override void _Process(double delta)
    {
        if (MathF.Abs(Speed - GoalSpeed) < (float)10e-5)
        {
            GoalSpeed = GD.Randf() * 10;
        }

        var lastPoint = GlobalPosition;

        for (var i = 0; i < NrPoints; i++)
        {
            if (points[i].DistanceTo(lastPoint) >= distBetweenPoints)
            {
                points[i].X = GlobalPosition.X;
                points[i].Y = GlobalPosition.Y;
            }
            else if (i > 0)
            {
                points[i].X = points[i-1].X;
                points[i].Y = points[i-1].Y;
            }
            
            GD.Print($"Point nr {i+1} is at position {points[i]}");
        }

        Speed = Mathf.Lerp(Speed, GoalSpeed, 0.3f);
        pathFollow.Progress += Speed;
    }
}
