using System;
using Godot;

namespace weave;

public partial class FlyingLight : Area2D
{
    [Signal]
    public delegate void CreatePathEventHandler(Vector2[] points);
    private PathFollow2D pathFollow;
    private float Speed;
    private float GoalSpeed;
    private float lastSpeed;
    private float goalSpeed;
    private const int NrPoints = 10;
    private Vector2[] points;
    private float distanceBetweenPoints = 1;

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

        if (points[0].DistanceTo(GlobalPosition) >= distanceBetweenPoints)
        {
            var lastPoint = GlobalPosition;
            var tempPoints = (Vector2[])points.Clone();

            for (var i = 0; i < NrPoints; i++)
            {
                tempPoints[i].X = lastPoint.X;
                tempPoints[i].Y = lastPoint.Y;
                if (i > 0)
                    lastPoint = points[i - 1];
            }

            for (var i = 0; i < NrPoints; i++)
            {
                points[i].X = tempPoints[i].X;
                points[i].Y = tempPoints[i].Y;
            }
        }

        EmitSignal(SignalName.CreatePath, points);

        Speed = Mathf.Lerp(Speed, GoalSpeed, 0.3f);
        pathFollow.Progress += Speed;
    }
}
